// Source: skill base-plateau-in-go

package pipeline

import (
	"context"
	"log/slog"
	"time"

	"github.com/example/todo/internal/logging"
)

// WithLogging is a decorator: it returns a Handler that calls next and writes
// two log records per call, so the log reads top-down as the call ran -
// "command called" with the command as the payload, then "command handled"
// with the duration and the result as the payload, or "command failed" at
// warn level with the error and the command again, so the input of a failure
// is in the log even when only failed records keep their payload. name is the
// command's name in the log. Whether the payload reaches the output is the handler's choice
// (package logging), never this decorator's.
//
// Every cross-cutting step (a transaction, a metric, ...) follows the same
// pattern: take a Handler, return a Handler of the same TCommand and TResult.
// The composition root wraps a handler with as many decorators as it needs.
func WithLogging[TCommand, TResult any](name string, logger *slog.Logger, next Handler[TCommand, TResult]) Handler[TCommand, TResult] {
	return HandlerFunc[TCommand, TResult](func(ctx context.Context, cmd TCommand) (TResult, error) {
		logger.LogAttrs(ctx, slog.LevelInfo, "command called", slog.String("command", name), logging.Payload(cmd))
		start := time.Now()
		res, err := next.Handle(ctx, cmd)
		if err != nil {
			logger.LogAttrs(ctx, slog.LevelWarn, "command failed",
				slog.String("command", name), slog.Duration("duration", time.Since(start)),
				slog.Any("error", err), logging.Failed(true), logging.Payload(cmd))
			return res, err
		}
		logger.LogAttrs(ctx, slog.LevelInfo, "command handled",
			slog.String("command", name), slog.Duration("duration", time.Since(start)), logging.Failed(false),
			logging.Payload(res))
		return res, nil
	})
}
