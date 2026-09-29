---
name: plateau-gw009-001--file-api-tasks-handlers
description: internal/api/tasks/handlers.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when editing the recheck-flagged-link handler or registering a new task type
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
source: internal/api/tasks/handlers.go
---

# Goal
Translate a `recheck-flagged-link` delivery into `LinkCheckService.Recheck` and its result into a status code.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Core Principles
- Apply ONE plateau template per file.
- Malformed payload or `ErrInvalidURL` → `400` (dead at once); `ErrUnavailable` → `503` (retried); other errors → returned, counted as `500`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Implementation
```go
// Skill: file-api-tasks-handlers
// Plateau: plateau-gw009-001
// Version: 20260928120000

// Package tasks is the TaskBox inbound adapter: one handler per task type,
// each decoding its payload, calling the domain service, and mapping the
// result to an HTTP status code — no business logic, exactly like the HTTP
// and gRPC adapters.
package tasks

import (
	"context"
	"encoding/json"
	"errors"
	"net/http"

	"{module-path}/internal/domain/interfaces"
	"{module-path}/internal/domain/services"
	"{module-path}/internal/taskbox"
)

// Handlers adapts TaskBox deliveries to the domain service.
type Handlers struct {
	svc *services.LinkCheckService
}

func New(svc *services.LinkCheckService) *Handlers {
	return &Handlers{svc: svc}
}

// Register registers every task type this service handles.
func (h *Handlers) Register(r *taskbox.Registry) {
	r.Register(services.TaskRecheckFlaggedLink, h.recheckFlaggedLink)
}

func (h *Handlers) recheckFlaggedLink(ctx context.Context, d taskbox.Delivery) (taskbox.Outcome, error) {
	var p services.RecheckFlaggedLink
	if err := json.Unmarshal(d.Payload, &p); err != nil || p.URL == "" {
		return taskbox.Outcome{Status: http.StatusBadRequest}, nil
	}
	_, err := h.svc.Recheck(ctx, p.URL)
	switch {
	case err == nil:
		return taskbox.OK, nil
	case errors.Is(err, services.ErrInvalidURL):
		return taskbox.Outcome{Status: http.StatusBadRequest}, nil
	case errors.Is(err, interfaces.ErrUnavailable):
		return taskbox.Outcome{Status: http.StatusServiceUnavailable}, nil
	default:
		return taskbox.Outcome{}, err
	}
}
```

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Pass the handler `ctx` to the domain call.
- Never answer `2xx` for a failed call.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Check list
- [ ] Smoke test: a `503` from the reputation service retries the task; the next attempt records the fresh verdict.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]
