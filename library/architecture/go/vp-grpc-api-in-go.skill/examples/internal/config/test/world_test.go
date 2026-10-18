package test

import (
	"context"
	"fmt"
	"os"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/config"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// variables are the environment variables a scenario may set; each scenario starts with none.
var variables = []string{"LOG_LEVEL"}

// World holds per-scenario state, reset before every scenario.
type World struct {
	cfg config.Config
	err error
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = World{}
		for _, name := range variables {
			if err := os.Unsetenv(name); err != nil {
				return ctx, err
			}
		}
		return ctx, nil
	})
}
