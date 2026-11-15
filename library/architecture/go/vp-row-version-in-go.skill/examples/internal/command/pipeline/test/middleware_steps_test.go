package test

import (
	"context"
	"errors"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command/pipeline"
)

func registerMiddlewareSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^a handler that succeeds, wrapped by the middleware:$`, w.aHandlerWrappedBy)
	sc.Step(`^the wrapped handler handles the command "([^"]*)" as "([^"]*)"$`, w.theWrappedHandlerHandles)
	sc.Step(`^the calls should be "([^"]*)"$`, w.theCallsShouldBe)
	sc.Step(`^the result should be "([^"]*)"$`, w.theResultShouldBe)
	sc.Step(`^the error should be "([^"]*)"$`, w.theErrorShouldBe)
	sc.Step(`^the middleware "([^"]*)" should have seen the command "([^"]*)" named "([^"]*)"$`, w.theMiddlewareShouldHaveSeen)
}

// recording returns a middleware that writes "{name} in" and "{name} out" to
// the calls around next, and what it was called with to seen; does says
// whether it passes the command on, rejects it, or passes a number instead.
func (w *World) recording(middleware, does string) pipeline.Middleware {
	return func(ctx context.Context, name string, cmd any, next pipeline.Next) (any, error) {
		w.calls = append(w.calls, middleware+" in")
		w.seen[middleware] = fmt.Sprintf("%v named %s", cmd, name)
		var (
			result any
			err    error
		)
		switch does {
		case "rejects":
			return nil, errors.New("rejected by " + middleware)
		case "passes a number on":
			result, err = next(ctx, 42)
		default:
			result, err = next(ctx, cmd)
		}
		w.calls = append(w.calls, middleware+" out")
		return result, err
	}
}

func (w *World) aHandlerWrappedBy(ctx context.Context, table *godog.Table) error {
	var middlewares []pipeline.Middleware
	for _, row := range table.Rows[1:] {
		middlewares = append(middlewares, w.recording(row.Cells[0].Value, row.Cells[1].Value))
	}
	w.middlewares = middlewares
	return nil
}

func (w *World) theWrappedHandlerHandles(ctx context.Context, cmd, name string) error {
	handler := pipeline.HandlerFunc[string, string](func(_ context.Context, cmd string) (string, error) {
		w.calls = append(w.calls, "handler "+cmd)
		return "planned " + cmd, nil
	})
	w.result, w.err = pipeline.Decorate[string, string](name, handler, w.middlewares).Handle(ctx, cmd)
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

func (w *World) theMiddlewareShouldHaveSeen(ctx context.Context, middleware, cmd, name string) error {
	if got, want := w.seen[middleware], cmd+" named "+name; got != want {
		return fmt.Errorf("%s saw %q, want %q", middleware, got, want)
	}
	return nil
}
