// Source: skill base-plateau-in-go

package pipeline

import (
	"context"
	"log/slog"
	"time"
)

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
