// Source: skill base-plateau-in-go

package pipeline

import (
	"context"
	"fmt"
)

// Next calls the rest of the chain: the decorators inside the current one,
// then the handler. A decorator calls it once to let the command through, or
// not at all to reject it.
type Next func(ctx context.Context, cmd any) (any, error)

// Decorator is one cross-cutting step around every command handler: logging,
// an access check, a transaction. It is called with the command's name
// ("PlanItems"), the command itself, and next; it returns what next returned,
// or an error of its own to reject the command.
//
// A Decorator is not generic, so decorators of different steps stand in one
// list ([]Decorator) that the composition root orders and the command
// registry (command.NewHandlers) applies to every handler. It sees the
// command as any; a decorator for some commands only selects them by a mark,
// an interface the command implements:
//
//	if restricted, ok := cmd.(Restricted); ok { ... }
//
// It passes the command on to next unchanged - the same value, or at least
// the same type: Decorate gives the handler what next receives.
type Decorator func(ctx context.Context, name string, cmd any, next Next) (any, error)

// Decorate wraps a typed handler with the decorators, the first the outermost:
// it runs first on the way in and last on the way out. It is the one place
// where the typed world of handlers meets the untyped list of decorators: the
// command goes into the chain as any and comes back to the handler as
// TCommand; the result goes out as any and comes back as TResult.
//
// TCommand is the command type (PlanItemsCommand), TResult the result type
// (PlanItemsResult) of handler; the returned handler has the same types, so
// an entry point cannot tell a decorated handler from a plain one.
func Decorate[TCommand, TResult any](name string, handler Handler[TCommand, TResult], decorators []Decorator) Handler[TCommand, TResult] {
	// The innermost link calls the handler itself.
	chain := Next(func(ctx context.Context, cmd any) (any, error) {
		typed, ok := cmd.(TCommand)
		if !ok {
			return nil, fmt.Errorf("pipeline: a decorator of %s passed on a %T, not the command", name, cmd)
		}
		return handler.Handle(ctx, typed)
	})
	// Each decorator, from the last to the first, wraps the chain built so
	// far, so the first ends up outermost.
	for i := len(decorators) - 1; i >= 0; i-- {
		decorator, next := decorators[i], chain
		chain = func(ctx context.Context, cmd any) (any, error) {
			return decorator(ctx, name, cmd, next)
		}
	}
	return HandlerFunc[TCommand, TResult](func(ctx context.Context, cmd TCommand) (TResult, error) {
		var zero TResult
		result, err := chain(ctx, cmd)
		if result == nil {
			// a decorator rejected the command, or the handler failed
			return zero, err
		}
		typed, ok := result.(TResult)
		if !ok {
			return zero, fmt.Errorf("pipeline: a decorator of %s returned a %T, not the result", name, result)
		}
		return typed, err
	})
}
