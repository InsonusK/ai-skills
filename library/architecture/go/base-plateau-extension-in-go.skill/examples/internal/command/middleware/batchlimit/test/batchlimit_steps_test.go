package test

import (
	"errors"
	"fmt"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command/middleware/batchlimit"
	"github.com/example/todo/internal/command/pipeline"
)

func registerBatchLimitSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the batch limit (\d+)$`, func(limit int) error { w.limit = limit; return nil })
	sc.Step(`^a batch of (\d+) items goes through the batch limit$`, func(size int) error {
		handle(w, batchlimit.Middleware(w.limit), batchCommand{size: size})
		return nil
	})
	sc.Step(`^a command without a mark goes through the batch limit$`, func() error {
		handle(w, batchlimit.Middleware(w.limit), plainCommand{})
		return nil
	})
	sc.Step(`^the handler should have run$`, func() error {
		if !w.handled || w.err != nil {
			return fmt.Errorf("handled=%v err=%v, want the handler run without an error", w.handled, w.err)
		}
		return nil
	})
	sc.Step(`^the handler should not have run$`, func() error {
		if w.handled {
			return errors.New("the handler ran")
		}
		return nil
	})
	sc.Step(`^the call should be rejected as resource exhausted with "([^"]*)"$`, func(message string) error {
		if !errors.Is(w.err, pipeline.ErrResourceExhausted) || w.err.Error() != message {
			return fmt.Errorf("error: got %v, want resource exhausted %q", w.err, message)
		}
		return nil
	})
}
