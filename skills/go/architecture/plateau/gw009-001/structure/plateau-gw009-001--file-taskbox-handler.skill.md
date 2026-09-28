---
name: plateau-gw009-001--file-taskbox-handler
description: internal/taskbox/handler.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when changing how outcomes are classified or handlers registered
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
source: internal/taskbox/handler.go
---

# Goal
The TaskBox handler registry, retry classification, backoff, exactly as proven by the conformance feature.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/handler.go.create.md|handler.go]]

# Core Principles
- Apply ONE plateau template per file.
- Verbatim from the solution's Implementation file; change it there first and rerun the conformance feature.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/handler.go.create.md|handler.go]]

# Implementation
```go
// Skill: file-taskbox-handler
// Plateau: plateau-gw009-001
// Version: 20260928120000

package taskbox

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"sync"
	"time"

	"github.com/google/uuid"
)

// Delivery is what a handler receives for one run (contract §2).
type Delivery struct {
	ID      uuid.UUID
	Type    string
	Payload json.RawMessage
	Attempt int
}

// Outcome is a handler's answer: an HTTP status code (2xx = success) and an
// optional Retry-After.
type Outcome struct {
	Status     int
	RetryAfter time.Duration
}

// OK is the success outcome.
var OK = Outcome{Status: http.StatusOK}

// Handler runs one task. A returned error counts as status 500 (contract §2);
// ctx ends when the task's lease ends, and the handler must honour it.
type Handler func(ctx context.Context, d Delivery) (Outcome, error)

// Registry maps task types to handlers. Safe for concurrent use.
type Registry struct {
	mu       sync.RWMutex
	handlers map[string]Handler
}

func NewRegistry() *Registry {
	return &Registry{handlers: map[string]Handler{}}
}

// Register sets the handler for taskType, replacing any earlier one.
func (r *Registry) Register(taskType string, h Handler) {
	r.mu.Lock()
	defer r.mu.Unlock()
	r.handlers[taskType] = h
}

// Unregister removes the handler for taskType.
func (r *Registry) Unregister(taskType string) {
	r.mu.Lock()
	defer r.mu.Unlock()
	delete(r.handlers, taskType)
}

func (r *Registry) lookup(taskType string) (Handler, bool) {
	r.mu.RLock()
	defer r.mu.RUnlock()
	h, ok := r.handlers[taskType]
	return h, ok
}

// Retryable reports whether an outcome status may succeed on a later
// attempt, by VP-C004's retry classification; every TaskBox handler is
// idempotent, so 500 is retryable.
func Retryable(status int) bool {
	switch status {
	case http.StatusRequestTimeout, http.StatusTooManyRequests, http.StatusInternalServerError,
		http.StatusBadGateway, http.StatusServiceUnavailable, http.StatusGatewayTimeout:
		return true
	}
	return false
}

// Success reports whether status is 2xx.
func Success(status int) bool { return status >= 200 && status < 300 }

// Backoff is the contract's min(base × 2^attempt, cap).
func Backoff(base, cap time.Duration, attempt int) time.Duration {
	d := base
	for i := 0; i < attempt; i++ {
		d *= 2
		if d >= cap {
			return cap
		}
	}
	return min(d, cap)
}

// resultOf classifies one run's outcome into what the store records.
func resultOf(out Outcome, err error, attempt int, cfg Config) Result {
	if err != nil {
		out = Outcome{Status: http.StatusInternalServerError}
	}
	r := Result{Status: out.Status}
	if err != nil {
		r.Error = err.Error()
	} else if !Success(out.Status) {
		r.Error = fmt.Sprintf("handler answered %d", out.Status)
	}
	if Retryable(out.Status) {
		r.Retryable = true
		r.RetryDelay = max(Backoff(cfg.BackoffBase, cfg.BackoffCap, attempt), out.RetryAfter)
	}
	return r
}
```

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/handler.go.create.md|handler.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Keep this file identical to the solution's Implementation file.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/handler.go.create.md|handler.go]]

# Check list
- [ ] `TEST_DATABASE_DSN=… make unit-test` passes the conformance feature.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/handler.go.create.md|handler.go]]
