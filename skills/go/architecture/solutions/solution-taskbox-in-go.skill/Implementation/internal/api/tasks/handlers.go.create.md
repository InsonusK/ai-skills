---
description: One handler per task type, registered in the TaskBox registry
project_name: internal/api/tasks
name: Handlers
element_kind: struct
change_kind: create
tags:
  - solution/taskbox-in-go
  - element/internal-api-tasks-handlers-go
---

# Goals
- Turn a TaskBox delivery into a domain call and its result into an HTTP status code.

# Core Principles
- Error mapping follows solution-taskbox's "Map every handler result to a status code": invalid payload or input → `400`, missing entity → `404`, conflicting state → `409`, unavailable dependency → `503`; any other error is returned and counts as `500`.

# Naming convention
| use case | struct name pattern | struct name | file name pattern | file name |
| -------- | -------------------- | ------------ | ------------------ | --------- |
| the adapter | `Handlers` | `Handlers` | `handlers.go` | handlers.go |
| one task type | `{followUp}` method | `recheckFlaggedLink` | — | — |

# Implementation changes
```go
// Package tasks is the TaskBox inbound adapter: one handler per task type.
package tasks

type Handlers struct {
	svc *services.{Service}
}

func New(svc *services.{Service}) *Handlers {
	return &Handlers{svc: svc}
}

// Register registers every task type this service handles.
func (h *Handlers) Register(r *taskbox.Registry) {
	r.Register(services.Task{FollowUp}, h.{followUp})
}

func (h *Handlers) {followUp}(ctx context.Context, d taskbox.Delivery) (taskbox.Outcome, error) {
	var p services.{FollowUp}
	if err := json.Unmarshal(d.Payload, &p); err != nil || p.{Key} == "" {
		return taskbox.Outcome{Status: http.StatusBadRequest}, nil
	}
	_, err := h.svc.{FollowUpMethod}(ctx, p.{Key})
	switch {
	case err == nil:
		return taskbox.OK, nil
	case errors.Is(err, services.ErrInvalid{Input}):
		return taskbox.Outcome{Status: http.StatusBadRequest}, nil
	case errors.Is(err, interfaces.ErrUnavailable):
		return taskbox.Outcome{Status: http.StatusServiceUnavailable}, nil
	default:
		return taskbox.Outcome{}, err // counts as 500, retried
	}
}
```

# Rule changes

## MUST

### Pass the handler context on
Pass the handler's `ctx` to the domain call.
- Risk: a call that ignores the lease deadline keeps running after the task was re-claimed (solution-taskbox, "Bound handlers by the lease").
- Fix: never replace `ctx` with `context.Background()` in a handler.

### Never answer 2xx for a failure
Return a non-2xx outcome or an error for every failed domain call.
- Risk: the task is marked `done` and the work silently disappears.
- Fix: map every domain error explicitly; let unknown errors fall through as `500`.

# Check list
- [ ] Each handler maps the domain's sentinel errors to the codes above and returns other errors unchanged.

# Unittest TestCases
- [ ] WHEN the payload is malformed THEN the handler answers `400` without calling the domain.
- [ ] WHEN the domain returns `ErrUnavailable` THEN the handler answers `503`.
