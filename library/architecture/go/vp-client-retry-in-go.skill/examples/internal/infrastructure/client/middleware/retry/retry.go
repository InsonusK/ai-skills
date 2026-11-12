// Source: skill vp-client-retry-in-go

package retry

import (
	"context"
	"errors"
	"log/slog"
	"math/rand/v2"
	"time"

	"github.com/example/todo/internal/infrastructure/client/core/outcome"
	"github.com/example/todo/internal/infrastructure/client/core/pipeline"
)

// Middleware sends a Retryable call again while its attempt ends in
// outcome.ErrUnavailable, up to cfg.Attempts attempts in all. Each attempt has
// the whole {API}_TIMEOUT of the client; all of them together end with the
// caller's deadline: no wait starts that would end past it. Before each
// repeat a WARN record "api client call retried" names the API, the
// operation, the attempt that failed, the wait, and its error. A call that
// is not Retryable, or that ends any other way, passes on unchanged - a
// repeat of a call the API answered could do its work twice.
func Middleware(cfg Config, logger *slog.Logger) pipeline.Middleware {
	return func(ctx context.Context, call pipeline.Call, next pipeline.Attempt) error {
		if !call.Retryable {
			return next(ctx)
		}
		backoff := cfg.Backoff
		for attempt := 1; ; attempt++ {
			err := next(ctx)
			if err == nil || !errors.Is(err, outcome.ErrUnavailable) || attempt >= cfg.Attempts {
				return err
			}
			wait := jitter(backoff)
			if deadline, ok := ctx.Deadline(); ok && time.Until(deadline) <= wait {
				return err
			}
			logger.LogAttrs(ctx, slog.LevelWarn, "api client call retried",
				slog.String("api", call.API), slog.String("operation", call.Operation),
				slog.Int("attempt", attempt), slog.Duration("wait", wait), slog.String("error", err.Error()))
			timer := time.NewTimer(wait)
			select {
			case <-timer.C:
			case <-ctx.Done():
				timer.Stop()
				return err
			}
			backoff *= 2
		}
	}
}

// jitter is a wait between half and one and a half of backoff, so the
// clients an outage stopped together do not all call again at once.
func jitter(backoff time.Duration) time.Duration {
	return backoff/2 + rand.N(backoff+1)
}
