package test

import (
	"context"
	"fmt"
	"io"
	"log/slog"
	"net"
	"net/http/httptest"
	"os"
	"strings"

	"github.com/cucumber/godog"

	httpapi "github.com/example/todo/internal/api/http"
	"github.com/example/todo/internal/command"
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
	// the fake PlanItems handler - the command layer below the HTTP entry point
	planResult command.PlanItemsResult
	planErr    error
	planCalled bool
	planGot    command.PlanItems

	cfg     httpapi.Config
	version string
	checks  []httpapi.ReadinessCheck

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
	*w = World{version: "0.0.0-test"}
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

// planItems is the fake handler: it records the command and answers what the scenario set.
func (w *World) planItems() command.Handler[command.PlanItems, command.PlanItemsResult] {
	return command.HandlerFunc[command.PlanItems, command.PlanItemsResult](
		func(_ context.Context, cmd command.PlanItems) (command.PlanItemsResult, error) {
			w.planCalled = true
			w.planGot = cmd
			return w.planResult, w.planErr
		})
}

func (w *World) server() *httpapi.Server {
	logger := slog.New(slog.NewTextHandler(io.Discard, nil))
	return httpapi.NewServer(w.cfg, w.version, httpapi.Handlers{PlanItems: w.planItems()}, logger, w.checks...)
}

func (w *World) send(method, path, body string) {
	req := httptest.NewRequest(method, path, strings.NewReader(body))
	w.response = httptest.NewRecorder()
	w.server().Handler().ServeHTTP(w.response, req)
	logf("when: %s %s %s -> %d %s", method, path, body, w.response.Code, strings.TrimSpace(w.response.Body.String()))
}

// errRejected is a business-rule error as the command layer returns it.
func errRejected(message string) error {
	return fmt.Errorf("%w: %s", command.ErrInvalid, message)
}
