package test

import (
	"context"
	"errors"
	"fmt"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/api/core/rpcstatus"
	"github.com/example/todo/internal/command/pipeline"
	"github.com/example/todo/internal/shared/errorkind"
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
	sc.Step(`^a command fails with the kind "([^"]*)" and the message "([^"]*)", wrapped in "([^"]*)"$`, w.failsWrapped)
	sc.Step(`^a command fails with the kind "([^"]*)" and the message "([^"]*)", caused by "([^"]*)"$`, w.failsWithCause)
	sc.Step(`^a command fails with the kind "([^"]*)" and the message "([^"]*)", caused by an error of the kind "([^"]*)"$`, w.failsWithKindedCause)
	sc.Step(`^a command returns the kind "([^"]*)" bare, wrapped in "([^"]*)"$`, w.returnsBareKind)
	sc.Step(`^a command fails with the plain error "([^"]*)"$`, w.failsPlain)
	sc.Step(`^the rpc code should be (\d+)$`, w.theCodeShouldBe)
	sc.Step(`^the rpc code's HTTP status should be (\d+)$`, w.theHTTPStatusShouldBe)
	sc.Step(`^the rpc message should be "([^"]*)"$`, w.theMessageShouldBe)
	sc.Step(`^the rpc code (\d+) should have the HTTP status (\d+)$`, w.theCodeShouldHaveHTTPStatus)
}

func (w *World) answer(err error) {
	w.code, w.message = rpcstatus.Of(err)
	logf("when: Of(%v) -> %d %q", err, w.code, w.message)
}

func kindNamed(name string) (error, error) {
	kind, ok := kinds[name]
	if !ok {
		return nil, fmt.Errorf("unknown kind %q", name)
	}
	return kind, nil
}

func (w *World) failsWithKind(ctx context.Context, name, message string) error {
	kind, err := kindNamed(name)
	if err != nil {
		return err
	}
	w.answer(errorkind.New(kind, message))
	return nil
}

func (w *World) failsWrapped(ctx context.Context, name, message, outer string) error {
	kind, err := kindNamed(name)
	if err != nil {
		return err
	}
	// wrapped as an error passing up through a service and a command would be
	w.answer(fmt.Errorf("%s: %w", outer, errorkind.New(kind, message)))
	return nil
}

func (w *World) failsWithCause(ctx context.Context, name, message, cause string) error {
	kind, err := kindNamed(name)
	if err != nil {
		return err
	}
	w.answer(errorkind.Wrap(kind, message, errors.New(cause)))
	return nil
}

func (w *World) failsWithKindedCause(ctx context.Context, name, message, causeName string) error {
	kind, err := kindNamed(name)
	if err != nil {
		return err
	}
	causeKind, err := kindNamed(causeName)
	if err != nil {
		return err
	}
	// the other service's error, of a kind of its own, told in this service's terms
	cause := errorkind.New(causeKind, "the titles are invalid")
	w.answer(errorkind.Wrap(kind, message, cause))
	return nil
}

func (w *World) returnsBareKind(ctx context.Context, name, outer string) error {
	kind, err := kindNamed(name)
	if err != nil {
		return err
	}
	w.answer(fmt.Errorf("%s: %w", outer, kind))
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
