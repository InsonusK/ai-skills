package test

import (
	"bytes"
	"context"
	"fmt"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// World holds per-scenario state, reset before every scenario.
type World struct {
	result command.PlanItemsResult
	err    error

	logs    bytes.Buffer
	wrapped command.Handler[string, string]
}

func (w *World) reset() { *w = World{} }

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		w.reset()
		return ctx, nil
	})
}
