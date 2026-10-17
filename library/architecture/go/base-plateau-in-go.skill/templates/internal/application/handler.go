// Source: skill base-plateau-in-go

// Package application holds the commands and their handlers: the only surface
// an entry point (internal/api/...) calls. An entry point never imports domain.
package application

import (
	"context"
	"log/slog"
	"time"

	"{module-path}/internal/domain"
)

// ErrInvalid is wrapped by every error that rejects a command for breaking a
// business rule; an entry point maps it to its own "bad request".
var ErrInvalid = domain.ErrInvalid

// Handler executes one command. Every handler has this shape, so an entry
// point and a decorator work with any of them.
type Handler[C, R any] interface {
	Handle(ctx context.Context, cmd C) (R, error)
}

// HandlerFunc adapts a function to Handler.
type HandlerFunc[C, R any] func(ctx context.Context, cmd C) (R, error)

func (f HandlerFunc[C, R]) Handle(ctx context.Context, cmd C) (R, error) { return f(ctx, cmd) }

// WithLogging wraps a handler with one log record per call. It is the
// decorator pattern every cross-cutting step (validation, transaction, ...)
// follows: wrap a Handler, return a Handler.
func WithLogging[C, R any](name string, logger *slog.Logger, next Handler[C, R]) Handler[C, R] {
	return HandlerFunc[C, R](func(ctx context.Context, cmd C) (R, error) {
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
