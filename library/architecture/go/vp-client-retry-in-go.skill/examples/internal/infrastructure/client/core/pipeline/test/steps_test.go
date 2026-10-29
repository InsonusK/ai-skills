package test

import (
	"context"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/infrastructure/client/core/pipeline"
)

// World holds per-scenario state, reset before every scenario.
type World struct {
	middlewares []pipeline.Middleware
	steps       []string
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = World{}
		return ctx, nil
	})
}

// recording is a middleware that records its steps; "twice" calls next twice.
func (w *World) recording(name string) pipeline.Middleware {
	return func(ctx context.Context, call pipeline.Call, next pipeline.Attempt) error {
		w.steps = append(w.steps, name+" before")
		err := next(ctx)
		if name == "twice" {
			err = next(ctx)
		}
		w.steps = append(w.steps, name+" after")
		return err
	}
}

func registerSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the client middleware "([^"]*)"$`, func(names string) error {
		for _, name := range strings.Split(names, "|") {
			w.middlewares = append(w.middlewares, w.recording(name))
		}
		return nil
	})
	sc.Step(`^a call of "([^"]*)" is run$`, func(operation string) error {
		call := pipeline.Call{API: "test-api", Operation: operation}
		return pipeline.Run(context.Background(), call, w.middlewares, func(context.Context) error {
			w.steps = append(w.steps, "attempt")
			return nil
		})
	})
	sc.Step(`^the steps should be, in order, "([^"]*)"$`, func(want string) error {
		if got := strings.Join(w.steps, "|"); got != want {
			return fmt.Errorf("steps %q, want %q", got, want)
		}
		return nil
	})
}
