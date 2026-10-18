// Source: skill base-plateau-in-go

// Package pipeline is the path every command takes from an entry point to its
// handler: the one Handler contract all handlers satisfy, the decorators that
// wrap a handler with a cross-cutting step - one file per decorator: logging
// now, a unit of work or a metric later - and the error kinds an entry point
// maps to its response (errors.go).
//
// It holds no command. The commands live one level up, in package command;
// entry points import both - command for the command types, pipeline for the
// contract and the error kinds. (In dotnet the same role is played by MediatR's
// IRequestHandler and its pipeline behaviors.)
package pipeline

import "context"

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
