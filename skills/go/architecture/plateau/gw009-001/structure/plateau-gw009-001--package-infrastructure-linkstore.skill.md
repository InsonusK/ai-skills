---
name: plateau-gw009-001--package-infrastructure-linkstore
description: internal/infrastructure/linkstore package of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when editing the link-history persistence adapter or the service's migration history, or deciding whether new durable-storage code belongs in internal/infrastructure/linkstore
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/package
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
  - "[[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]]"
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
---

# Goal
Implement the domain's `LinkHistory` port on PostgreSQL and own the service's one schema history — `link_checks` and TaskBox schema v1 alike.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/Package.create.md#MUST|internal/infrastructure/{store}]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/infrastructure/{store}/Package.extend.md#MUST|internal/infrastructure/{store}]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Core Principles
- No `pgx` type escapes this package — `Record`/`Recent` take and return only domain types.
- The schema has one ordered history in `migrations/`, applied by `Migrate` (goose); TaskBox's tables are one more migration in it, copied verbatim from the contract.
- `linkstore` enqueues through `internal/taskbox/pgstore`, a shared mechanism, not a sibling adapter.

# Structure
## Package Structure
```
internal/infrastructure/linkstore/
  store.go          ← Store
  migrations.go     ← Migrate (goose)
  migrations/
    00001_create_link_checks.sql
    00002_taskbox_v1.sql      ← TaskBox contract schema v1, verbatim
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| store.go | `Store` implementing `LinkHistory`; `Record` writes data and tasks in one transaction | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-infrastructure-linkstore-store.skill.md]] |
| migrations.go, migrations/ | `Migrate` and the embedded goose migrations | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-infrastructure-linkstore-migrations.skill.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| github.com/jackc/pgx/v5 | >= 5.11 | pool, `pgx.Tx` |
| github.com/pressly/goose/v3 | >= 3.28 | migration runner |

# Allowed Dependencies
- `internal/domain/interfaces`
- `internal/taskbox`, `internal/taskbox/pgstore`
- `github.com/jackc/pgx/v5`, `github.com/pressly/goose/v3`

# Rules
MUST:
- Never let a `pgx` type cross this package's boundary.
- Never import a sibling `internal/infrastructure/*` package; `internal/taskbox/*` is a mechanism, not a sibling.
- Never edit a shipped migration; add the next-numbered file.
- `00002_taskbox_v1.sql`'s Up section is the TaskBox contract's schema v1 DDL, unchanged.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/Package.create.md#MUST|internal/infrastructure/{store}]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/infrastructure/{store}/Package.extend.md#MUST|internal/infrastructure/{store}]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Check list
- [ ] `Migrate` on an empty database creates `link_checks`, `taskbox_task`, `taskbox_group`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/Package.create.md#MUST|internal/infrastructure/{store}]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/infrastructure/{store}/Package.extend.md#MUST|internal/infrastructure/{store}]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]
