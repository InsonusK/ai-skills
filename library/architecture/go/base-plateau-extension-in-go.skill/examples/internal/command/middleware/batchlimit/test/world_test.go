package test

import (
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
	limit   int
	handled bool
	err     error
}

func (w *World) reset() { *w = World{} }

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		w.reset()
		return ctx, nil
	})
}

// The commands of these tests: one marked as a batch, one with no mark, so the
// middleware is tested without any command of the application.
type batchCommand struct{ size int }

func (c batchCommand) BatchSize() int { return c.size }

type plainCommand struct{}

// handle runs cmd, named "Echo", through the middleware to a handler that
// only records that it ran.
func handle[TCommand any](w *World, middleware pipeline.Middleware, cmd TCommand) {
	handler := pipeline.HandlerFunc[TCommand, string](func(context.Context, TCommand) (string, error) {
		w.handled = true
		return "done", nil
	})
	_, w.err = pipeline.Decorate("Echo", handler, []pipeline.Middleware{middleware}).Handle(context.Background(), cmd)
	logf("when: %T %+v -> handled=%v err=%v", cmd, cmd, w.handled, w.err)
}
