package test

import (
	"context"
	"errors"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command/middleware/unitofwork"
	"github.com/example/todo/internal/command/pipeline"
)

func logf(format string, args ...any) { fmt.Printf(format+"\n", args...) }

// recordingUnits is a unit of work that records what the middleware asked of
// it, and the state of the context it was asked with.
type recordingUnits struct {
	begun      bool  // the context already carries a unit: the command is nested
	commitErr  error // what Commit answers
	asked      []string
	commitDone bool // the context of Commit was done
}

func (u *recordingUnits) Begin(ctx context.Context) (context.Context, bool) {
	u.asked = append(u.asked, "begin")
	return ctx, !u.begun
}

func (u *recordingUnits) Commit(ctx context.Context) error {
	u.asked = append(u.asked, "commit")
	u.commitDone = ctx.Err() != nil
	return u.commitErr
}

func (u *recordingUnits) Rollback(context.Context) {
	u.asked = append(u.asked, "rollback")
}

type middlewareWorld struct {
	units    recordingUnits
	handler  func(ctx context.Context, cancel context.CancelFunc) (any, error)
	result   any
	err      error
	panicked any
}

func registerMiddlewareSteps(sc *godog.ScenarioContext) {
	w := &middlewareWorld{}
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		// The handler returns its result after the caller has left, so a
		// commit with the caller's context would see it cancelled.
		*w = middlewareWorld{handler: func(_ context.Context, cancel context.CancelFunc) (any, error) {
			cancel()
			return "the result", nil
		}}
		return ctx, nil
	})

	sc.Step(`^the handler fails with "([^"]*)"$`, func(message string) error {
		w.handler = func(context.Context, context.CancelFunc) (any, error) { return nil, errors.New(message) }
		return nil
	})
	sc.Step(`^the handler panics$`, func() error {
		w.handler = func(context.Context, context.CancelFunc) (any, error) { panic("the handler broke") }
		return nil
	})
	sc.Step(`^a unit of work already begun by the outer command$`, func() error {
		w.units.begun = true
		return nil
	})
	sc.Step(`^the commit fails with "([^"]*)"$`, func(message string) error {
		w.units.commitErr = errors.New(message)
		return nil
	})
	sc.Step(`^a command runs through the unit of work middleware$`, func() error {
		caller, cancel := context.WithCancel(context.Background())
		defer cancel()
		middleware := unitofwork.Middleware(&w.units)
		func() {
			defer func() { w.panicked = recover() }()
			w.result, w.err = middleware(caller, "Import", "the command", func(ctx context.Context, _ any) (any, error) {
				return w.handler(ctx, cancel)
			})
		}()
		logf("when: the middleware -> result=%v err=%v panic=%v asked=%v", w.result, w.err, w.panicked, w.units.asked)
		return nil
	})

	sc.Step(`^the unit of work should have been asked for "([^"]*)"$`, func(want string) error {
		if got := strings.Join(w.units.asked, "|"); got != want {
			return fmt.Errorf("the unit of work was asked for %q, want %q", got, want)
		}
		return nil
	})
	sc.Step(`^the command should answer its result$`, func() error {
		if w.err != nil || w.result != "the result" {
			return fmt.Errorf("the command answered %v, %v; want the handler's result", w.result, w.err)
		}
		return nil
	})
	sc.Step(`^the caller's cancellation should not reach the commit$`, func() error {
		if w.units.commitDone {
			return errors.New("the commit was asked with the caller's cancelled context")
		}
		return nil
	})
	sc.Step(`^the command should fail with "([^"]*)"$`, func(message string) error {
		if w.err == nil || w.err.Error() != message {
			return fmt.Errorf("the command failed with %v, want %q", w.err, message)
		}
		return nil
	})
	sc.Step(`^the panic should have reached the caller$`, func() error {
		if w.panicked != "the handler broke" {
			return fmt.Errorf("the caller got the panic %v", w.panicked)
		}
		return nil
	})
	sc.Step(`^the command should fail with the kind "unavailable"$`, func() error {
		if !errors.Is(w.err, pipeline.ErrUnavailable) {
			return fmt.Errorf("the command failed with %v, want the kind unavailable", w.err)
		}
		return nil
	})
	sc.Step(`^the command's error should not mention "([^"]*)"$`, func(text string) error {
		if strings.Contains(fmt.Sprint(w.err), text) {
			return fmt.Errorf("the command's error %q mentions %q", w.err, text)
		}
		return nil
	})
}
