package test

import (
	"bytes"
	"context"
	"fmt"
	"log/slog"
	"net"
	"net/http"
	"net/http/httptest"
	"os"
	"strings"

	"github.com/cucumber/godog"

	httpapi "github.com/example/todo/internal/api/http"
	"github.com/example/todo/internal/command/pipeline"
	"github.com/example/todo/internal/logging"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// variables are the environment variables a scenario may set; each scenario starts with none.
var variables = []string{"HTTP_LISTEN_PORT", "HTTP_BASE_PATH", "HTTP_API_DOCS_ENABLED"}

// World holds per-scenario state, reset before every scenario.
type World struct {
	// the fake echo handler - a command of these tests only, so the generic
	// entry point is tested without any command of the application
	echoAnswer string
	echoErr    error
	echoCalled bool
	echoGot    echoCommand
	echoReqID  string // the request id in the command's context

	cfg  httpapi.Config
	logs bytes.Buffer // what the server logged, every attribute kept

	response *httptest.ResponseRecorder

	loadedCfg httpapi.Config
	loadErr   error

	stop     context.CancelFunc
	served   chan error
	occupied net.Listener
}

func (w *World) reset() error {
	if w.stop != nil {
		w.stop()
	}
	if w.occupied != nil {
		_ = w.occupied.Close()
	}
	*w = World{}
	for _, name := range variables {
		if err := os.Unsetenv(name); err != nil {
			return err
		}
	}
	return nil
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		return ctx, w.reset()
	})
	sc.After(func(ctx context.Context, _ *godog.Scenario, err error) (context.Context, error) {
		return ctx, w.reset()
	})
}

// echoCommand and echoResult stand for any command and its result.
type echoCommand struct{ Text string }
type echoResult struct{ Text string }

// echoHandler is the fake handler: it records the command and answers what the scenario set.
func (w *World) echoHandler() pipeline.Handler[echoCommand, echoResult] {
	return pipeline.HandlerFunc[echoCommand, echoResult](
		func(ctx context.Context, cmd echoCommand) (echoResult, error) {
			w.echoCalled = true
			w.echoGot = cmd
			w.echoReqID = logging.RequestID(ctx)
			return echoResult{Text: w.echoAnswer}, w.echoErr
		})
}

// routes is the one test route, POST /v1/echo, answering 201 on success.
func (w *World) routes() []httpapi.Route {
	type body struct {
		Text string `json:"text"`
	}
	decode := func(r *http.Request) (echoCommand, error) {
		b, err := httpapi.DecodeJSON[body](r)
		return echoCommand{Text: b.Text}, err
	}
	encode := func(result echoResult) any { return body{Text: result.Text} }
	routes := []httpapi.Route{{Pattern: "POST /v1/echo", Handler: httpapi.Endpoint(w.echoHandler(), decode, encode, http.StatusCreated)}}
	if moreRoutes != nil {
		routes = append(routes, moreRoutes(w)...)
	}
	return routes
}

// moreRoutes adds the test routes of a file only some services copy: when the
// contract is a proto file, proto_steps_test.go sets it to the routes over
// proto.go. Nil otherwise.
var moreRoutes func(w *World) []httpapi.Route

// server logs through a plain JSON handler, which keeps the payload, so the
// scenarios see every attribute the server writes.
func (w *World) server() *httpapi.Server {
	logger := slog.New(slog.NewJSONHandler(&w.logs, nil))
	return httpapi.NewServer(w.cfg, w.routes(), logger)
}

func (w *World) send(method, path, body string) {
	w.sendWithHeaders(method, path, body, nil)
}

func (w *World) sendWithHeaders(method, path, body string, headers map[string]string) {
	req := httptest.NewRequest(method, path, strings.NewReader(body))
	for name, value := range headers {
		req.Header.Set(name, value)
	}
	w.response = httptest.NewRecorder()
	w.server().Handler().ServeHTTP(w.response, req)
	logf("when: %s %s %s -> %d %s", method, path, body, w.response.Code, strings.TrimSpace(w.response.Body.String()))
}

// errRejected is a business-rule error as the command layer returns it.
func errRejected(message string) error {
	return fmt.Errorf("%w: %s", pipeline.ErrInvalid, message)
}
