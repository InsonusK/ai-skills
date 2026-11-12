package test

import (
	"context"
	"errors"
	"fmt"
	"log/slog"
	"os"
	"strings"
	"sync"
	"time"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/infrastructure/client/core/outcome"
	"github.com/example/todo/internal/infrastructure/client/core/pipeline"
	"github.com/example/todo/internal/infrastructure/client/middleware/retry"
)

// variables are the settings a scenario may set; each starts with none.
var variables = []string{"TEST_API_RETRY_ATTEMPTS", "TEST_API_RETRY_BACKOFF"}

// World holds per-scenario state, reset before every scenario.
type World struct {
	cfg      retry.Config
	call     pipeline.Call
	outcomes []string
	attempts int
	err      error
	records  *recorder
	loaded   retry.Config
	loadErr  error
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = World{records: &recorder{}}
		for _, name := range variables {
			if err := os.Unsetenv(name); err != nil {
				return ctx, err
			}
		}
		return ctx, nil
	})
}

// attempt ends each attempt as the scenario lists: ok, unavailable, or status.
func (w *World) attempt(context.Context) error {
	outcomeOf := w.outcomes[w.attempts]
	w.attempts++
	switch outcomeOf {
	case "unavailable":
		return fmt.Errorf("%w: connection refused", outcome.ErrUnavailable)
	case "status":
		return &outcome.StatusError{Code: outcome.CodeInvalidArgument, Message: "rejected"}
	}
	return nil
}

func (w *World) run(ctx context.Context) {
	logger := slog.New(w.records)
	w.err = pipeline.Run(ctx, w.call, []pipeline.Middleware{retry.Middleware(w.cfg, logger)}, w.attempt)
	fmt.Printf("when: %d attempts, error %v\n", w.attempts, w.err)
}

func registerSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the retry allows (\d+) attempts with a backoff of (\w+)$`, func(attempts int, backoff string) error {
		d, err := time.ParseDuration(backoff)
		w.cfg = retry.Config{Attempts: attempts, Backoff: d}
		return err
	})
	given := func(retryable bool) func(string) error {
		return func(outcomes string) error {
			w.call = pipeline.Call{API: "todo-api", Operation: "POST /v1/items/plan", Retryable: retryable}
			w.outcomes = strings.Split(outcomes, "|")
			return nil
		}
	}
	sc.Step(`^a retryable call whose attempts end "([^"]*)"$`, given(true))
	sc.Step(`^a call not marked retryable whose attempts end "([^"]*)"$`, given(false))
	sc.Step(`^the call is run through the retry$`, func() error {
		w.run(context.Background())
		return nil
	})
	sc.Step(`^the call is run through the retry within (\w+)$`, func(within string) error {
		d, err := time.ParseDuration(within)
		if err != nil {
			return err
		}
		ctx, cancel := context.WithTimeout(context.Background(), d)
		defer cancel()
		w.run(ctx)
		return nil
	})
	sc.Step(`^the call should succeed after (\d+) attempts$`, func(n int) error {
		if w.err != nil || w.attempts != n {
			return fmt.Errorf("%d attempts, error %v; want success after %d", w.attempts, w.err, n)
		}
		return nil
	})
	sc.Step(`^the call should fail as unavailable after (\d+) attempts$`, func(n int) error {
		if !errors.Is(w.err, outcome.ErrUnavailable) || w.attempts != n {
			return fmt.Errorf("%d attempts, error %v; want unavailable after %d", w.attempts, w.err, n)
		}
		return nil
	})
	sc.Step(`^the call should fail with a status after (\d+) attempts$`, func(n int) error {
		var status *outcome.StatusError
		if !errors.As(w.err, &status) || w.attempts != n {
			return fmt.Errorf("%d attempts, error %v; want a status after %d", w.attempts, w.err, n)
		}
		return nil
	})
	sc.Step(`^(\d+) "api client call retried" warnings should name the API "([^"]*)" and the operation "([^"]*)"$`, func(n int, api, operation string) error {
		found := 0
		for _, record := range w.records.records {
			attrs := attrs(record)
			if record.Message == "api client call retried" && record.Level == slog.LevelWarn && attrs["api"] == api && attrs["operation"] == operation {
				found++
			}
		}
		if found != n {
			return fmt.Errorf("%d warnings, want %d", found, n)
		}
		return nil
	})
	sc.Step(`^no "api client call retried" warning should be logged$`, func() error {
		if len(w.records.records) != 0 {
			return fmt.Errorf("%d records, want none", len(w.records.records))
		}
		return nil
	})

	// config.feature
	sc.Step(`^the environment variable "([^"]*)" is "([^"]*)"$`, os.Setenv)
	sc.Step(`^the retry settings of "([^"]*)" are loaded$`, func(prefix string) error {
		w.loaded, w.loadErr = retry.LoadConfig(prefix)
		return nil
	})
	sc.Step(`^the retry should allow (\d+) attempts with a backoff of "([^"]*)"$`, func(attempts int, backoff string) error {
		if w.loadErr != nil {
			return w.loadErr
		}
		if got := fmt.Sprintf("%d %s", w.loaded.Attempts, w.loaded.Backoff); got != fmt.Sprintf("%d %s", attempts, backoff) {
			return fmt.Errorf("got %s, want %d %s", got, attempts, backoff)
		}
		return nil
	})
	sc.Step(`^loading should fail mentioning "([^"]*)"$`, func(text string) error {
		if w.loadErr == nil || !strings.Contains(w.loadErr.Error(), text) {
			return fmt.Errorf("error %v, want one mentioning %q", w.loadErr, text)
		}
		return nil
	})
}

// recorder is a slog.Handler keeping every record with all its attributes.
type recorder struct {
	mu      sync.Mutex
	records []slog.Record
}

func (r *recorder) Enabled(context.Context, slog.Level) bool { return true }

func (r *recorder) Handle(_ context.Context, record slog.Record) error {
	r.mu.Lock()
	defer r.mu.Unlock()
	r.records = append(r.records, record.Clone())
	return nil
}

func (r *recorder) WithAttrs([]slog.Attr) slog.Handler { return r }
func (r *recorder) WithGroup(string) slog.Handler      { return r }

func attrs(record slog.Record) map[string]any {
	out := map[string]any{}
	record.Attrs(func(a slog.Attr) bool {
		out[a.Key] = a.Value.Any()
		return true
	})
	return out
}
