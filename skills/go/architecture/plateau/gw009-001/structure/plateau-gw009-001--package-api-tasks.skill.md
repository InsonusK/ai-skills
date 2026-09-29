---
name: plateau-gw009-001--package-api-tasks
description: internal/api/tasks package of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when adding a task type's handler or changing how task outcomes map to status codes
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/package
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
---

# Goal
The TaskBox inbound adapter: one handler per task type, as thin as the HTTP and gRPC adapters.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Core Principles
- Decode payload → call `LinkCheckService` → map its error to a status code; no business logic.

# Structure
## Package Structure
```
internal/api/tasks/
  handlers.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| handlers.go | `Handlers`: `Register` + `recheckFlaggedLink` | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-api-tasks-handlers.skill.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| — | — | standard library only |

# Allowed Dependencies
- `internal/domain/services`, `internal/domain/interfaces`, `internal/taskbox`

# Rules
MUST:
- Never decide here whether a task is needed.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]
