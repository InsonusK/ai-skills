package test

import (
	"context"
	"fmt"
	"time"

	"github.com/cucumber/godog"

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
	called     bool
}

var _ interfaces.ReputationChecker = (*stubReputationChecker)(nil)

func (s *stubReputationChecker) CheckReputation(ctx context.Context, url string) (interfaces.Reputation, error) {
	s.called = true
	return s.reputation, s.err
}

// stubReputationCache is an in-memory fake of interfaces.ReputationCache -
// no network call, no real Redis.
type stubReputationCache struct {
	store map[string]interfaces.Reputation
	set   bool
}

var _ interfaces.ReputationCache = (*stubReputationCache)(nil)

func newStubReputationCache() *stubReputationCache {
	return &stubReputationCache{store: map[string]interfaces.Reputation{}}
}

func (c *stubReputationCache) Get(ctx context.Context, url string) (interfaces.Reputation, bool, error) {
	rep, ok := c.store[url]
	return rep, ok, nil
}

func (c *stubReputationCache) Set(ctx context.Context, url string, rep interfaces.Reputation) error {
	c.set = true
	c.store[url] = rep
	return nil
}

// stubLinkHistory is an in-memory fake of interfaces.LinkHistory - no
// network call, no real Postgres.
type stubLinkHistory struct {
	entries   []interfaces.LinkHistoryEntry
	tasks     [][]interfaces.Task // tasks[i] were passed with entries[i]
	recordErr error
}

var _ interfaces.LinkHistory = (*stubLinkHistory)(nil)

func (h *stubLinkHistory) Record(ctx context.Context, entry interfaces.LinkHistoryEntry, tasks ...interfaces.Task) error {
	if h.recordErr != nil {
		return h.recordErr
	}
	h.entries = append(h.entries, entry)
	h.tasks = append(h.tasks, tasks)
	return nil
}

func (h *stubLinkHistory) Recent(ctx context.Context, limit int) ([]interfaces.LinkHistoryEntry, error) {
	if limit > len(h.entries) {
		limit = len(h.entries)
	}
	return h.entries[:limit], nil
}

// recheckAfter is the delay the scenarios' service schedules re-checks with.
const recheckAfter = time.Hour

// World holds per-scenario state, reset before every scenario runs.
type World struct {
	svc        *services.LinkCheckService
	reputation *stubReputationChecker
	cache      *stubReputationCache
	history    *stubLinkHistory

	input  string
	result services.Result
	err    error
}

func newWorld() *World {
	rep := &stubReputationChecker{}
	cache := newStubReputationCache()
	history := &stubLinkHistory{}
	return &World{svc: services.NewLinkCheckService(rep, cache, history, recheckAfter), reputation: rep, cache: cache, history: history}
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
