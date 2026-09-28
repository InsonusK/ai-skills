---
name: plateau-gw009-001--package-taskbox-pgstore
description: internal/taskbox/pgstore package of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when editing the PostgreSQL TaskBox store
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
Realize the contract's PostgreSQL schema v1: enqueue in the caller's `pgx.Tx`, the claim with `FOR UPDATE SKIP LOCKED`, attempt-fenced outcomes.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/pgstore/Package.create.md#MUST|internal/taskbox/pgstore]]

# Core Principles
- The contract's SQL (group lock, claim) is used as written.

# Structure
## Package Structure
```
internal/taskbox/pgstore/
  store.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| store.go | `Store`: Enqueue, Claim, Finish, Requeue, Cancel, Cleanup, Get | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-taskbox-pgstore-store.skill.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| github.com/jackc/pgx/v5 | >= 5.11 | `pgx.Tx`, `pgxpool.Pool` |

# Allowed Dependencies
- `internal/taskbox`, `github.com/jackc/pgx/v5`

# Rules
MUST:
- Never create tables here — the schema is a migration of the service.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/pgstore/Package.create.md#MUST|internal/taskbox/pgstore]]
