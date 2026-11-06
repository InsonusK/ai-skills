package test

import (
	"bytes"
	"context"
	"errors"
	"fmt"
	"log/slog"
	"net"
	"net/http"
	"net/http/httptest"
	"os"
	"strings"

	"github.com/cucumber/godog"

	httppipeline "github.com/example/todo/internal/api/http/pipeline"
	commandlogging "github.com/example/todo/internal/command/middleware/logging"
	"github.com/example/todo/internal/command/pipeline"
	"github.com/example/todo/internal/logging"
	"github.com/example/todo/internal/shared/errorkind"
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
	echoPanics bool   // the echo handler panics, as a bug in a handler would

	cfg         httppipeline.Config
	logs        bytes.Buffer // what the server logged, every attribute kept
	decorated   bool         // the echo handler is wrapped with the command logging middleware
	middlewares []httppipeline.Middleware

	response *httptest.ResponseRecorder
	lastPath string // the path of the last request, as the client asked for it

	loadedCfg httppipeline.Config
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
	if moreSteps != nil {
		moreSteps(sc, w)
	}
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

// echoHandler is the fake handler: it records the command and answers what
// the scenario set; wrapped with logging when the scenario asks, so its
// records land between the server's.
func (w *World) echoHandler() pipeline.Handler[echoCommand, echoResult] {
	if w.decorated {
		return pipeline.Decorate("Echo", w.fakeEcho(), []pipeline.Middleware{commandlogging.Middleware(w.logger())})
	}
	return w.fakeEcho()
}

func (w *World) fakeEcho() pipeline.Handler[echoCommand, echoResult] {
	return pipeline.HandlerFunc[echoCommand, echoResult](
		func(ctx context.Context, cmd echoCommand) (echoResult, error) {
			w.echoCalled = true
			w.echoGot = cmd
			if w.echoPanics {
				panic("echo handler broke")
			}
			w.echoReqID = logging.RequestID(ctx)
			return echoResult{Text: w.echoAnswer}, w.echoErr
		})
}

// routes is the one test route, POST /v1/echo, answering 201 on success.
func (w *World) routes() []httppipeline.Route {
	type body struct {
		Text string `json:"text"`
	}
	decode := func(r *http.Request) (echoCommand, error) {
		b, err := httppipeline.DecodeJSON[body](r)
		if err == nil && b.Text == "" {
			// The route needs the field, as a route of the contract does.
			err = errors.New("body.text is required")
		}
		return echoCommand{Text: b.Text}, err
	}
	encode := func(result echoResult) any { return body{Text: result.Text} }
	routes := []httppipeline.Route{{Pattern: "POST /v1/echo", Handler: httppipeline.Endpoint(w.echoHandler(), decode, encode, http.StatusCreated)}}
	if moreRoutes != nil {
		routes = append(routes, moreRoutes(w)...)
	}
	return routes
}

// moreRoutes adds the test routes of a file only some services copy: when the
// contract is a proto file, proto_steps_test.go sets it to the routes over
// proto.go. Nil otherwise.
var moreRoutes func(w *World) []httppipeline.Route

// moreSteps registers the steps of a feature that serves moreRoutes (the
// proto-JSON form), when one is in the package.
var moreSteps func(sc *godog.ScenarioContext, w *World)

// server logs through a plain JSON handler, which keeps the payload, so the
// scenarios see every attribute the server writes.
func (w *World) server() *httppipeline.Server {
	return httppipeline.NewServer(w.cfg, w.routes(), w.logger(), w.middlewares)
}

func (w *World) logger() *slog.Logger {
	return slog.New(slog.NewJSONHandler(&w.logs, nil))
}

func (w *World) send(method, path, body string) {
	w.sendWithHeaders(method, path, body, nil)
}

func (w *World) sendWithHeaders(method, path, body string, headers map[string]string) {
	w.lastPath = path
	req := httptest.NewRequest(method, path, strings.NewReader(body))
	if body != "" {
		req.Header.Set("Content-Type", "application/json") // a scenario may name another below
	}
	for name, value := range headers {
		req.Header.Set(name, value)
	}
	w.response = httptest.NewRecorder()
	w.server().Handler().ServeHTTP(w.response, req)
	logf("when: %s %s %s -> %d %s", method, path, body, w.response.Code, strings.TrimSpace(w.response.Body.String()))
}

// errRejected is a business-rule error as the command layer returns it.
func errRejected(message string) error {
	return errorkind.New(errorkind.ErrInvalid, message)
}
