package test

import (
	"bytes"
	"context"
	"fmt"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command/pipeline"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// World holds per-scenario state, reset before every scenario.
type World struct {
	err     error
	logs    bytes.Buffer
	wrapped pipeline.Handler[string, string]

	decorators []pipeline.Decorator
	calls      []string          // "{decorator} in", "handler {command}", "{decorator} out", in order
	seen       map[string]string // what each decorator was called with
	result     string
}

func (w *World) reset() { *w = World{seen: map[string]string{}} }

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		w.reset()
		return ctx, nil
	})
}
