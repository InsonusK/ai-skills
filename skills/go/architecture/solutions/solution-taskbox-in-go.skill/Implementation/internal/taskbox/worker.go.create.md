---
description: The worker pool: claim, run under the lease, record the fenced outcome, clean up
project_name: internal/taskbox
name: worker
element_kind: struct
change_kind: create
verbatim_of: internal/taskbox/worker.go
tags:
  - solution/taskbox-in-go
  - element/internal-taskbox-worker-go
---

# Goals
- Drain one queue with a pool of goroutines, dispatching each claimed task to its handler.

# Core Principles
- The handler context ends at the lease end and never earlier on shutdown; a run in flight finishes and its outcome is fenced by the store.
- No handler for a type → `503`, retried (contract §4); a panic → `500`.

# Implementation changes
Create `internal/taskbox/worker.go` exactly as below (`{module-path}` = the service's Go module path, `{store}` = the package owning the service's migrations). The code is proven by the conformance feature on PostgreSQL in this catalog's plateau built with this solution.

```go
package taskbox

import (
	"context"
	"fmt"
	"log/slog"
	"net/http"
	"sync"
	"time"
)

// Config holds a worker pool's settings; zero values take the contract
// defaults (lease 5 min, backoff 1s × 2^attempt capped at 1h, retention 7 days).
type Config struct {
	Queue            string
	Workers          int
	Lease            time.Duration
	BackoffBase      time.Duration
	BackoffCap       time.Duration
	PollInterval     time.Duration
	DefaultRetention time.Duration
	CleanupInterval  time.Duration
}

func (c Config) withDefaults() Config {
	if c.Queue == "" {
		c.Queue = DefaultQueue
	}
	if c.Workers <= 0 {
		c.Workers = 1
	}
	if c.Lease <= 0 {
		c.Lease = 5 * time.Minute
	}
	if c.BackoffBase <= 0 {
		c.BackoffBase = time.Second
	}
	if c.BackoffCap <= 0 {
		c.BackoffCap = time.Hour
	}
	if c.PollInterval <= 0 {
		c.PollInterval = time.Second
	}
	if c.DefaultRetention <= 0 {
		c.DefaultRetention = 7 * 24 * time.Hour
	}
	if c.CleanupInterval <= 0 {
		c.CleanupInterval = time.Minute
	}
	return c
}

// Worker drains one queue of a Store with a pool of goroutines, dispatching
// each claimed task to the handler registered for its type.
type Worker struct {
	store    Store
	registry *Registry
	cfg      Config
}

func NewWorker(store Store, registry *Registry, cfg Config) *Worker {
	return &Worker{store: store, registry: registry, cfg: cfg.withDefaults()}
}

// Run claims and runs tasks until ctx ends, then stops claiming and waits
// for the runs in flight to finish (each is bounded by its lease).
func (w *Worker) Run(ctx context.Context) error {
	var wg sync.WaitGroup
	for i := 0; i < w.cfg.Workers; i++ {
		wg.Go(func() { w.loop(ctx) })
	}
	wg.Go(func() { w.cleanupLoop(ctx) })
	wg.Wait()
	return nil
}

func (w *Worker) loop(ctx context.Context) {
	for ctx.Err() == nil {
		leaseStart := time.Now()
		claimed, err := w.store.Claim(ctx, w.cfg.Queue, w.cfg.Lease, 1)
		if err != nil && ctx.Err() == nil {
			slog.Error("taskbox: claim failed", "queue", w.cfg.Queue, "error", err)
		}
		if len(claimed) == 0 {
			select {
			case <-ctx.Done():
			case <-time.After(w.cfg.PollInterval):
			}
			continue
		}
		for _, c := range claimed {
			w.run(ctx, c, leaseStart.Add(w.cfg.Lease))
		}
	}
}

// run executes one claimed task and records its outcome. The handler's
// context ends at the lease end, never earlier on shutdown: a run in flight
// is finished, and a late outcome is discarded by the store's fencing.
func (w *Worker) run(ctx context.Context, c Claimed, leaseEnd time.Time) {
	hctx, cancel := context.WithDeadline(context.WithoutCancel(ctx), leaseEnd)
	defer cancel()

	var r Result
	if h, ok := w.registry.lookup(c.Type); ok {
		out, err := safeCall(hctx, h, Delivery{ID: c.ID, Type: c.Type, Payload: c.Payload, Attempt: c.Attempt})
		r = resultOf(out, err, c.Attempt, w.cfg)
	} else {
		r = resultOf(Outcome{Status: http.StatusServiceUnavailable}, nil, c.Attempt, w.cfg)
		r.Error = fmt.Sprintf("no handler registered for type %q", c.Type)
	}
	if ferr := w.store.Finish(context.WithoutCancel(ctx), c, r); ferr != nil {
		slog.Error("taskbox: record outcome failed", "id", c.ID, "type", c.Type, "error", ferr)
		return
	}
	level := slog.LevelDebug
	if !Success(r.Status) {
		level = slog.LevelWarn
	}
	slog.Log(ctx, level, "taskbox: task ran", "id", c.ID, "type", c.Type, "attempt", c.Attempt, "status", r.Status, "error", r.Error)
}

// safeCall runs h, turning a panic into an error (an exception counts as 500).
func safeCall(ctx context.Context, h Handler, d Delivery) (out Outcome, err error) {
	defer func() {
		if p := recover(); p != nil {
			err = fmt.Errorf("handler panic: %v", p)
		}
	}()
	return h(ctx, d)
}

func (w *Worker) cleanupLoop(ctx context.Context) {
	for {
		select {
		case <-ctx.Done():
			return
		case <-time.After(w.cfg.CleanupInterval):
		}
		if err := w.store.Cleanup(ctx, w.cfg.DefaultRetention); err != nil && ctx.Err() == nil {
			slog.Error("taskbox: cleanup failed", "queue", w.cfg.Queue, "error", err)
		}
	}
}
```

# Rule changes

## MUST

### Bound the handler by the lease
Give the handler a context whose deadline is the lease end measured from before the claim, detached from the worker's own shutdown.
- Risk: a later deadline lets the run overlap its re-claim; tying it to shutdown cancels runs in flight and burns an attempt on every deploy.
- Fix: `context.WithDeadline(context.WithoutCancel(ctx), leaseStart.Add(lease))`.
