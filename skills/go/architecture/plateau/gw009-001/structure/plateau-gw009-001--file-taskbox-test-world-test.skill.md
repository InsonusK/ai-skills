---
name: plateau-gw009-001--file-taskbox-test-world-test
description: internal/taskbox/test/world_test.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when changing or debugging the TaskBox conformance run on this plateau
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
source: internal/taskbox/test/world_test.go
---

# Goal
The conformance runner's per-scenario state and the storeUnderTest seam.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/world_test.go.create.md|world_test.go]]

# Core Principles
- Apply ONE plateau template per file.
- Verbatim from the solution's Implementation file (`{store}` = `linkstore`).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/world_test.go.create.md|world_test.go]]

# Implementation
```go
// Skill: file-taskbox-test-world-test
// Plateau: plateau-gw009-001
// Version: 20260928120000

package test

import (
	"context"
	"fmt"
	"sync"
	"time"

	"github.com/cucumber/godog"
	"github.com/google/uuid"

	"{module-path}/internal/taskbox"
)

// logf logs a step's action/observation via stdout so the line stays next
// to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// storeUnderTest binds the store-agnostic steps to one store.
type storeUnderTest interface {
	name() string
	// excludedKind is the @store-* tag this store does not run: "transient"
	// for a VP-C001 store, "persistent" for a VP-C002 store.
	excludedKind() string
	taskStore() taskbox.Store
	configure(s settings) error
	reset(ctx context.Context) error
	// enqueue adds tasks in one transaction, committed or rolled back.
	enqueue(ctx context.Context, commit bool, tasks []taskbox.NewTask) ([]bool, error)
	// enqueueHeld adds t in its own transaction, kept open hold after the insert.
	enqueueHeld(ctx context.Context, t taskbox.NewTask, hold time.Duration) error
}

// settings are the run's TaskBox settings from the feature's Background.
type settings struct {
	lease, backoffBase, backoffCap, retention, poll time.Duration
	partitions                                      int
}

// handlerRule is one row of a scripted handler: attempt 0 = any.
type handlerRule struct {
	attempt    int
	status     int
	retryAfter time.Duration
	delay      time.Duration
	err        string
}

// run is one recorded handler invocation.
type run struct {
	id        uuid.UUID
	attempt   int
	start     time.Time
	end       time.Time
	sawCancel bool
}

// World holds per-scenario state, reset before every scenario.
type World struct {
	sut      storeUnderTest
	settings settings
	registry *taskbox.Registry

	aliases []string             // in enqueue order
	ids     map[string]uuid.UUID // alias → id
	groups  map[string]string    // alias → group
	runAt   map[string]time.Time // alias → requested run_at

	stopWorkers func() error // set by "workers are started"

	mu   sync.Mutex
	runs []run
}

func newWorld(sut storeUnderTest) *World {
	return &World{sut: sut}
}

func (w *World) reset(ctx context.Context) error {
	w.settings = settings{}
	w.registry = taskbox.NewRegistry()
	w.aliases = nil
	w.ids = map[string]uuid.UUID{}
	w.groups = map[string]string{}
	w.runAt = map[string]time.Time{}
	w.runs = nil
	if w.stopWorkers != nil {
		_ = w.stopWorkers()
		w.stopWorkers = nil
	}
	return w.sut.reset(ctx)
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, s *godog.Scenario) (context.Context, error) {
		return ctx, w.reset(ctx)
	})
}

func (w *World) config(workers int) taskbox.Config {
	return taskbox.Config{
		Workers:          workers,
		Lease:            w.settings.lease,
		BackoffBase:      w.settings.backoffBase,
		BackoffCap:       w.settings.backoffCap,
		PollInterval:     w.settings.poll,
		DefaultRetention: w.settings.retention,
		CleanupInterval:  w.settings.poll,
	}
}

// scripted builds a handler that answers by rules and records every run.
func (w *World) scripted(rules []handlerRule) taskbox.Handler {
	return func(ctx context.Context, d taskbox.Delivery) (taskbox.Outcome, error) {
		r := run{id: d.ID, attempt: d.Attempt, start: time.Now()}
		rule := pick(rules, d.Attempt)
		if rule.delay > 0 {
			time.Sleep(rule.delay) // deliberately ignores ctx: models a handler that overruns its lease
			r.sawCancel = ctx.Err() != nil
		}
		r.end = time.Now()
		w.mu.Lock()
		w.runs = append(w.runs, r)
		w.mu.Unlock()
		if rule.err != "" {
			return taskbox.Outcome{}, fmt.Errorf("%s", rule.err)
		}
		return taskbox.Outcome{Status: rule.status, RetryAfter: rule.retryAfter}, nil
	}
}

func pick(rules []handlerRule, attempt int) handlerRule {
	for _, r := range rules {
		if r.attempt == attempt {
			return r
		}
	}
	for _, r := range rules {
		if r.attempt == 0 {
			return r
		}
	}
	return handlerRule{status: 500, err: fmt.Sprintf("no scripted answer for attempt %d", attempt)}
}

// runsOf returns the recorded runs of one task, by start time.
func (w *World) runsOf(id uuid.UUID) []run {
	w.mu.Lock()
	defer w.mu.Unlock()
	var out []run
	for _, r := range w.runs {
		if r.id == id {
			out = append(out, r)
		}
	}
	return out
}

func (w *World) alias(name string) (uuid.UUID, error) {
	id, ok := w.ids[name]
	if !ok {
		return uuid.Nil, fmt.Errorf("unknown task %q", name)
	}
	return id, nil
}

// remember assigns t an id and records it under alias.
func (w *World) remember(alias string, t taskbox.NewTask) taskbox.NewTask {
	id, _ := uuid.NewV7()
	t.ID = id
	w.aliases = append(w.aliases, alias)
	w.ids[alias] = id
	w.groups[alias] = t.Group
	if !t.RunAt.IsZero() {
		w.runAt[alias] = t.RunAt
	}
	return t
}
```

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/world_test.go.create.md|world_test.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Keep this file identical to the solution's Implementation file.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/world_test.go.create.md|world_test.go]]

# Check list
- [ ] `TEST_DATABASE_DSN=… make unit-test` runs the feature on PostgreSQL.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/world_test.go.create.md|world_test.go]]
