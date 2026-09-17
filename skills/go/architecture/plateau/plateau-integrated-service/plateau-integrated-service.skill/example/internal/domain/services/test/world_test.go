package test

import (
	"context"
	"fmt"

	"github.com/cucumber/godog"

	"github.com/example/linkcheck-service/internal/domain/interfaces"
	"github.com/example/linkcheck-service/internal/domain/services"
)

// logf logs a step's action/observation via stdout, matching godog's pretty
// output so the line stays attached to the step that produced it (see
// cucmber-testing-in-go's "Log through stdout, not testing.T" rule).
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// stubReputationChecker is a scenario-configurable fake of
// interfaces.ReputationChecker - no network call, no real gRPC service.
type stubReputationChecker struct {
	reputation interfaces.Reputation
	err        error
	called     bool
}

var _ interfaces.ReputationChecker = (*stubReputationChecker)(nil)

func (s *stubReputationChecker) CheckReputation(ctx context.Context, url string) (interfaces.Reputation, error) {
	s.called = true
	return s.reputation, s.err
}

// World holds per-scenario state, reset before every scenario runs.
type World struct {
	svc        *services.LinkCheckService
	reputation *stubReputationChecker

	input  string
	result services.Result
	err    error
}

func newWorld() *World {
	rep := &stubReputationChecker{}
	return &World{svc: services.NewLinkCheckService(rep), reputation: rep}
}

func (w *World) reset() {
	*w = *newWorld()
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, s *godog.Scenario) (context.Context, error) {
		w.reset()
		return ctx, nil
	})
}
