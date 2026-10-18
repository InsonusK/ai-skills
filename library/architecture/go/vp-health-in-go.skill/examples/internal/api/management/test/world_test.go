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

	"github.com/example/todo/internal/api/management"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// variables are the environment variables a scenario may set; each scenario starts with none.
var variables = []string{"MANAGEMENT_LISTEN_PORT"}

// World holds per-scenario state, reset before every scenario.
type World struct {
	version string
	checks  []management.Check

	response *httptest.ResponseRecorder

	loadedCfg management.Config
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

func discard() *slog.Logger { return slog.New(slog.NewTextHandler(io.Discard, nil)) }

func (w *World) send(method, path string) {
	server := management.NewServer(management.Config{}, w.version, discard(), w.checks...)
	req := httptest.NewRequest(method, path, strings.NewReader(""))
	w.response = httptest.NewRecorder()
	server.Handler().ServeHTTP(w.response, req)
	logf("when: %s %s -> %d %s", method, path, w.response.Code, strings.TrimSpace(w.response.Body.String()))
}
