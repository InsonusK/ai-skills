package test

import (
	"context"
	"errors"
	"fmt"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/api/core/rpcstatus"
	"github.com/example/todo/internal/command/pipeline"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// World holds per-scenario state, reset before every scenario.
type World struct {
	code    rpcstatus.Code
	message string
}

// kinds names every error kind as the scenarios write it.
var kinds = map[string]error{
	"invalid":             pipeline.ErrInvalid,
	"not found":           pipeline.ErrNotFound,
	"conflict":            pipeline.ErrConflict,
	"concurrent change":   pipeline.ErrConcurrentChange,
	"failed precondition": pipeline.ErrFailedPrecondition,
	"unauthenticated":     pipeline.ErrUnauthenticated,
	"permission denied":   pipeline.ErrPermissionDenied,
	"resource exhausted":  pipeline.ErrResourceExhausted,
	"unimplemented":       pipeline.ErrUnimplemented,
	"unavailable":         pipeline.ErrUnavailable,
}

func registerSteps(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = World{}
		return ctx, nil
	})
	sc.Step(`^a command fails with the kind "([^"]*)" and the message "([^"]*)"$`, w.failsWithKind)
	sc.Step(`^a command fails with the plain error "([^"]*)"$`, w.failsPlain)
	sc.Step(`^the code should be (\d+)$`, w.theCodeShouldBe)
	sc.Step(`^the HTTP status should be (\d+)$`, w.theHTTPStatusShouldBe)
	sc.Step(`^the message should be "([^"]*)"$`, w.theMessageShouldBe)
	sc.Step(`^the code (\d+) should have the HTTP status (\d+)$`, w.theCodeShouldHaveHTTPStatus)
}

func (w *World) answer(err error) {
	w.code, w.message = rpcstatus.Of(err)
	logf("when: Of(%v) -> %d %q", err, w.code, w.message)
}

func (w *World) failsWithKind(ctx context.Context, name, message string) error {
	kind, ok := kinds[name]
	if !ok {
		return fmt.Errorf("unknown kind %q", name)
	}
	// wrapped twice, as an error passing through a service and a command would be
	w.answer(fmt.Errorf("plan items: %w", fmt.Errorf("%w: %s", kind, message)))
	return nil
}

func (w *World) failsPlain(ctx context.Context, message string) error {
	w.answer(errors.New(message))
	return nil
}

func (w *World) theCodeShouldBe(ctx context.Context, want int) error {
	if int(w.code) != want {
		return fmt.Errorf("code: got %d, want %d", w.code, want)
	}
	return nil
}

func (w *World) theHTTPStatusShouldBe(ctx context.Context, want int) error {
	if got := w.code.HTTPStatus(); got != want {
		return fmt.Errorf("HTTP status of %d: got %d, want %d", w.code, got, want)
	}
	return nil
}

func (w *World) theMessageShouldBe(ctx context.Context, want string) error {
	if w.message != want {
		return fmt.Errorf("message: got %q, want %q", w.message, want)
	}
	return nil
}

func (w *World) theCodeShouldHaveHTTPStatus(ctx context.Context, code, want int) error {
	if got := rpcstatus.Code(code).HTTPStatus(); got != want {
		return fmt.Errorf("HTTP status of %d: got %d, want %d", code, got, want)
	}
	return nil
}
