package test

import (
	"context"
	"fmt"
	"os"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command/access"
	"github.com/example/todo/internal/command/pipeline"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// variables are the environment variables a scenario may set; each scenario starts with none.
var variables = []string{"ACCESS_TOKENS", "ACCESS_MAX_BATCH"}

// World holds per-scenario state, reset before every scenario.
type World struct {
	ctx      context.Context // the call's context: with the caller's identity, or without one
	limit    int
	handled  bool
	err      error
	cfg      access.Config
	cfgError error
}

func (w *World) reset() error {
	*w = World{ctx: context.Background()}
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

// The commands of these tests: one with each mark, one with none, so the
// decorators are tested without any command of the application.
type restrictedCommand struct{ role string }

func (c restrictedCommand) RequiredRole() string { return c.role }

type batchCommand struct{ size int }

func (c batchCommand) BatchSize() int { return c.size }

type plainCommand struct{}

// handle runs cmd, named "Echo", through the decorator to a handler that
// only records that it ran.
func handle[TCommand any](w *World, decorator pipeline.Decorator, cmd TCommand) {
	handler := pipeline.HandlerFunc[TCommand, string](func(context.Context, TCommand) (string, error) {
		w.handled = true
		return "done", nil
	})
	_, w.err = pipeline.Decorate("Echo", handler, []pipeline.Decorator{decorator}).Handle(w.ctx, cmd)
	logf("when: %T %+v -> handled=%v err=%v", cmd, cmd, w.handled, w.err)
}
