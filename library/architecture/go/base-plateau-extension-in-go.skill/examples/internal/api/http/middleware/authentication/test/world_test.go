package test

import (
	"bytes"
	"context"
	"fmt"
	"log/slog"
	"net/http"
	"os"

	"github.com/cucumber/godog"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// World holds per-scenario state, reset before every scenario.
type World struct {
	logs   bytes.Buffer // what the server and the middleware logged, every attribute kept
	server http.Handler // the application's HTTP API with the access variation point attached
	status int
	body   []byte
}

func (w *World) reset() error {
	*w = World{}
	return os.Unsetenv("ACCESS_TOKENS")
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		return ctx, w.reset()
	})
}

func (w *World) logger() *slog.Logger {
	return slog.New(slog.NewJSONHandler(&w.logs, nil))
}
