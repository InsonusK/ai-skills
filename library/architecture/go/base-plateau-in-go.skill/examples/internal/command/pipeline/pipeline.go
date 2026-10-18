// Source: skill base-plateau-in-go

// Package pipeline is the path every command takes from an entry point to its
// handler: the one Handler contract all handlers satisfy, the decorators that
// wrap a handler with a cross-cutting step (logging now; a unit of work or a
// metric later), and the error kind an entry point maps to its response.
//
// It holds no command. The commands live one level up, in package command;
// entry points import both - command for the command types, pipeline for the
// contract and the error kind. (In dotnet the same role is played by MediatR's
// IRequestHandler and its pipeline behaviors.)
package pipeline

import (
	"context"
	"log/slog"
	"time"

	"github.com/example/todo/internal/domain/entity"
)

// ErrInvalid is the error kind of a command rejected for breaking a business
// rule. Every business-rule error wraps it, so an entry point checks
// errors.Is(err, pipeline.ErrInvalid) and answers with its own "bad request"
// (exit code 2, HTTP 400, ...) without knowing the individual rules.
// It is the entity package's sentinel re-exported, so entry points never import domain.
var ErrInvalid = entity.ErrInvalid

// Handler is the one contract every command handler satisfies.
//
// TCommand is the command type - the input of one operation (for example
// PlanItems). TResult is the result type the handler returns on success (for
// example PlanItemsResult). Each handler fixes both: PlanItemsHandler is a
// Handler[PlanItems, PlanItemsResult].
//
// Because every handler has this one shape, code that is not about a specific
// command - an entry point that serves commands, a decorator that wraps them -
// is written once for Handler[TCommand, TResult] and works with any handler.
type Handler[TCommand, TResult any] interface {
	// Handle executes the command. It returns the result, or an error; an error
	// wrapping ErrInvalid means the command was rejected by a business rule.
	Handle(ctx context.Context, cmd TCommand) (TResult, error)
}

// HandlerFunc turns an ordinary function into a Handler: the function is the
// Handle method. A handler with its own dependencies is a struct
// (PlanItemsHandler); HandlerFunc is for handlers built on the fly - a
// decorator returns one wrapping the next handler, and a test writes a stub
// handler in one line.
type HandlerFunc[TCommand, TResult any] func(ctx context.Context, cmd TCommand) (TResult, error)

// Handle calls the function itself, which makes HandlerFunc satisfy Handler.
func (f HandlerFunc[TCommand, TResult]) Handle(ctx context.Context, cmd TCommand) (TResult, error) {
	return f(ctx, cmd)
}

// WithLogging is a decorator: it returns a Handler that calls next and writes
// one log record per call - "command handled" at info level, or "command
// failed" at warn level with the error. name is the command's name in the log.
//
// Every cross-cutting step (a transaction, a metric, ...) follows the same
// pattern: take a Handler, return a Handler of the same TCommand and TResult.
// The composition root wraps a handler with as many decorators as it needs.
func WithLogging[TCommand, TResult any](name string, logger *slog.Logger, next Handler[TCommand, TResult]) Handler[TCommand, TResult] {
	return HandlerFunc[TCommand, TResult](func(ctx context.Context, cmd TCommand) (TResult, error) {
		start := time.Now()
		res, err := next.Handle(ctx, cmd)
		if err != nil {
			logger.WarnContext(ctx, "command failed", "command", name, "duration", time.Since(start), "error", err)
			return res, err
		}
		logger.InfoContext(ctx, "command handled", "command", name, "duration", time.Since(start))
		return res, nil
	})
}
