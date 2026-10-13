package test

import (
	"context"
	"fmt"
	"net/http"
	"time"

	"github.com/cucumber/godog"

	apihttp "github.com/example/linkcheck-service/internal/api/http"
	"github.com/example/linkcheck-service/internal/domain/interfaces"
	"github.com/example/linkcheck-service/internal/domain/services"
)

// logf logs a step's action/observation via stdout, matching godog's pretty
// output so the line stays attached to the step that produced it (see
// cucumber-testing-in-go's "Log through stdout, not testing.T" rule).
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// stubReputationChecker is a scenario-configurable fake of
// interfaces.ReputationChecker - no network call, no real gRPC service.
type stubReputationChecker struct {
	reputation interfaces.Reputation
	err        error
}

var _ interfaces.ReputationChecker = (*stubReputationChecker)(nil)

func (s *stubReputationChecker) CheckReputation(ctx context.Context, url string) (interfaces.Reputation, error) {
	return s.reputation, s.err
}

// stubReputationCache is a fake of interfaces.ReputationCache that never
// holds anything, so every check reaches the reputation stub.
type stubReputationCache struct{}

var _ interfaces.ReputationCache = (*stubReputationCache)(nil)

func (c *stubReputationCache) Get(ctx context.Context, url string) (interfaces.Reputation, bool, error) {
	return interfaces.Reputation{}, false, nil
}

func (c *stubReputationCache) Set(ctx context.Context, url string, rep interfaces.Reputation) error {
	return nil
}

// stubLinkHistory is an in-memory fake of interfaces.LinkHistory - no
// network call, no real Postgres.
type stubLinkHistory struct {
	entries []interfaces.LinkHistoryEntry
}

var _ interfaces.LinkHistory = (*stubLinkHistory)(nil)

func (h *stubLinkHistory) Record(ctx context.Context, entry interfaces.LinkHistoryEntry, tasks ...interfaces.Task) error {
	h.entries = append(h.entries, entry)
	return nil
}

// Recent returns the newest entries first, as the real store does.
func (h *stubLinkHistory) Recent(ctx context.Context, limit int) ([]interfaces.LinkHistoryEntry, error) {
	out := []interfaces.LinkHistoryEntry{}
	for i := len(h.entries) - 1; i >= 0 && len(out) < limit; i-- {
		out = append(out, h.entries[i])
	}
	return out, nil
}

// World holds per-scenario state, reset before every scenario runs.
type World struct {
	service    *services.LinkCheckService
	reputation *stubReputationChecker
	server     *apihttp.Server

	status   int
	location string
	body     string
}

func newWorld() *World {
	w := &World{}
	w.reset()
	return w
}

// reset starts the adapter as a deployment with no variable set does: docs off,
// no base path. Its outbound ports are stubs; the domain service is the real one.
func (w *World) reset() {
	reputation := &stubReputationChecker{}
	cache := &stubReputationCache{}
	history := &stubLinkHistory{}
	service := services.NewLinkCheckService(reputation, cache, history, time.Hour)
	*w = World{service: service, reputation: reputation}
	w.server = w.newServer(apihttp.Options{})
}

func (w *World) newServer(options apihttp.Options) *apihttp.Server {
	return apihttp.New(w.service, options)
}

func (w *World) handler() http.Handler {
	return w.server.Handler()
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, s *godog.Scenario) (context.Context, error) {
		w.reset()
		return ctx, nil
	})
}
