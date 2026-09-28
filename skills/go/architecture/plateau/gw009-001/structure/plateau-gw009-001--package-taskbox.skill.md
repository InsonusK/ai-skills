---
name: plateau-gw009-001--package-taskbox
description: internal/taskbox package of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when editing the TaskBox mechanism — task values, handler registry, outcome classification, worker — or adding a TaskBox store
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
The store-independent TaskBox mechanism realizing the VP-C003 contract.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/Package.create.md#MUST|internal/taskbox]]

# Core Principles
- A shared mechanism, not an adapter: `linkstore` imports it to enqueue inside its own transaction.
- Never imports anything of the service.

# Structure
## Package Structure
```
internal/taskbox/
  taskbox.go
  handler.go
  worker.go
  pgstore/
  features/
    taskbox-conformance.feature   ← verbatim from solution-taskbox
  test/
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| taskbox.go | task values, `Store` interface | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-taskbox-taskbox.skill.md]] |
| handler.go | `Handler`, `Registry`, `Retryable`, `Backoff` | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-taskbox-handler.skill.md]] |
| worker.go | lease-bounded worker pool | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-taskbox-worker.skill.md]] |
| pgstore/ | PostgreSQL store | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-taskbox-pgstore.skill.md]] |
| features/, test/ | the conformance feature and its runner | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-taskbox-test.skill.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| github.com/google/uuid | >= 1.6 | `uuid.NewV7` task ids |

# Allowed Dependencies
- Standard library, `github.com/google/uuid`

# Rules
MUST:
- Never import `internal/domain/*` or `internal/infrastructure/*`.
- Never put a task handler or a business rule here.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/Package.create.md#MUST|internal/taskbox]]
