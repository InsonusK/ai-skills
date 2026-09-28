---
description: The TaskBox inbound adapter — one handler per task type, translating deliveries into domain calls and domain errors into status codes
name: internal/api/tasks
element_kind: package
change_kind: create
tags:
  - solution/taskbox-in-go
  - element/internal-api-tasks
---

# Goals
- Give TaskBox deliveries the same thin inbound adapter HTTP and gRPC calls have.

# Core Principles
- A task arrives like a request: decode the payload, call the domain service, map the result to a status code — no business logic here (solution-taskbox, "Handlers are inbound adapters").
- The adapter calls the domain service's concrete type, like `internal/api/http` and `internal/api/grpc`.

# Structure

## Repository place
```
internal/
  api/
    http/
    grpc/
    tasks/
```

## Package Structure
```
internal/api/tasks/
  handlers.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| handlers.go | `Handlers`: `Register(registry)` + one method per task type | [[./handlers.go.create.md|handlers.go]] |

# Allowed Dependencies
- `internal/domain/services`, `internal/domain/interfaces` (its sentinel errors), `internal/taskbox`.

# What Does NOT Belong Here
- Deciding that a task is needed — the domain service does that.
- Retry policy — TaskBox classifies the returned status code.

# Check list
- [ ] Every task type the domain declares has exactly one handler registered by `Register`.
