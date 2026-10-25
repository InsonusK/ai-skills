package test

import (
	"context"
	"errors"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command/pipeline"
)

func registerDecoratorSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^a handler that succeeds, decorated by:$`, w.aHandlerDecoratedBy)
	sc.Step(`^the decorated handler handles the command "([^"]*)" as "([^"]*)"$`, w.theDecoratedHandlerHandles)
	sc.Step(`^the calls should be "([^"]*)"$`, w.theCallsShouldBe)
	sc.Step(`^the result should be "([^"]*)"$`, w.theResultShouldBe)
	sc.Step(`^the error should be "([^"]*)"$`, w.theErrorShouldBe)
	sc.Step(`^the decorator "([^"]*)" should have seen the command "([^"]*)" named "([^"]*)"$`, w.theDecoratorShouldHaveSeen)
}

// recording returns a decorator that writes "{name} in" and "{name} out" to
// the calls around next, and what it was called with to seen; does says
// whether it passes the command on, rejects it, or passes a number instead.
func (w *World) recording(decorator, does string) pipeline.Decorator {
	return func(ctx context.Context, name string, cmd any, next pipeline.Next) (any, error) {
		w.calls = append(w.calls, decorator+" in")
		w.seen[decorator] = fmt.Sprintf("%v named %s", cmd, name)
		var (
			result any
			err    error
		)
		switch does {
		case "rejects":
			return nil, errors.New("rejected by " + decorator)
		case "passes a number on":
			result, err = next(ctx, 42)
		default:
			result, err = next(ctx, cmd)
		}
		w.calls = append(w.calls, decorator+" out")
		return result, err
	}
}

func (w *World) aHandlerDecoratedBy(ctx context.Context, table *godog.Table) error {
	var decorators []pipeline.Decorator
	for _, row := range table.Rows[1:] {
		decorators = append(decorators, w.recording(row.Cells[0].Value, row.Cells[1].Value))
	}
	w.decorators = decorators
	return nil
}

func (w *World) theDecoratedHandlerHandles(ctx context.Context, cmd, name string) error {
	handler := pipeline.HandlerFunc[string, string](func(_ context.Context, cmd string) (string, error) {
		w.calls = append(w.calls, "handler "+cmd)
		return "planned " + cmd, nil
	})
	w.result, w.err = pipeline.Decorate[string, string](name, handler, w.decorators).Handle(ctx, cmd)
	logf("when: handle %q as %s -> result=%q err=%v calls=%v", cmd, name, w.result, w.err, w.calls)
	return nil
}

func (w *World) theCallsShouldBe(ctx context.Context, want string) error {
	if got := strings.Join(w.calls, ", "); got != want {
		return fmt.Errorf("calls: got %q, want %q", got, want)
	}
	return nil
}

func (w *World) theResultShouldBe(ctx context.Context, want string) error {
	if w.result != want {
		return fmt.Errorf("result: got %q, want %q", w.result, want)
	}
	return nil
}

func (w *World) theErrorShouldBe(ctx context.Context, want string) error {
	if w.err == nil || w.err.Error() != want {
		return fmt.Errorf("error: got %v, want %q", w.err, want)
	}
	return nil
}

func (w *World) theDecoratorShouldHaveSeen(ctx context.Context, decorator, cmd, name string) error {
	if got, want := w.seen[decorator], cmd+" named "+name; got != want {
		return fmt.Errorf("%s saw %q, want %q", decorator, got, want)
	}
	return nil
}
