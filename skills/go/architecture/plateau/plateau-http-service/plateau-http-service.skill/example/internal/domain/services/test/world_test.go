package test

import (
	"context"
	"fmt"

	"github.com/cucumber/godog"

	"github.com/example/linkcheck-service/internal/domain/services"
)

// logf logs a step's action/observation via stdout, matching godog's pretty
// output so the line stays attached to the step that produced it (see
// cucmber-testing-in-go's "Log through stdout, not testing.T" rule).
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// World holds per-scenario state, reset before every scenario runs.
type World struct {
	svc *services.LinkCheckService

	input  string
	result services.Result
	err    error
}

func newWorld() *World {
	return &World{svc: services.NewLinkCheckService()}
}

func (w *World) reset() {
	*w = World{svc: services.NewLinkCheckService()}
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, s *godog.Scenario) (context.Context, error) {
		w.reset()
		return ctx, nil
	})
}
