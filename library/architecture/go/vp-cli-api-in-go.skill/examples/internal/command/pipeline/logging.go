// Source: skill base-plateau-in-go

package pipeline

import (
	"context"
	"log/slog"
	"time"

	"github.com/example/todo/internal/logging"
)

// WithLogging is the decorator that logs every command: two records per call,
// so the log reads top-down as the call ran - "command called" with the
// command as the payload, then "command handled" with the duration and the
// result as the payload, or "command failed" at warn level with the error and
// the command again, so the input of a failure is in the log even when only
// failed records keep their payload. The command's name is the "command"
// attribute. Whether the payload reaches the output is the handler's choice
// (package logging), never this decorator's.
//
// Every cross-cutting step (a transaction, a metric, an access check, ...)
// follows the same pattern: a function that takes what the step needs and
// returns a Decorator. The composition root lists them; the command registry
// wraps every handler with the list (see Decorate).
func WithLogging(logger *slog.Logger) Decorator {
	return func(ctx context.Context, name string, cmd any, next Next) (any, error) {
		logger.LogAttrs(ctx, slog.LevelInfo, "command called", slog.String("command", name), logging.Payload(cmd))
		start := time.Now()
		res, err := next(ctx, cmd)
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
	}
}
