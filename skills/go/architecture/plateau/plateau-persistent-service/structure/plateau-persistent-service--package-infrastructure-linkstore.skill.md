---
name: plateau-persistent-service--package-infrastructure-linkstore
description: internal/infrastructure/linkstore package of the plateau-persistent-service plateau
whenToUse: when editing the link-history persistence adapter, or deciding whether new durable-storage code belongs in internal/infrastructure/linkstore
domain: skill
type: template
plateau: plateau-persistent-service
version: 20260917040000
tags:
  - skill/template/package
  - plateau/plateau-persistent-service
created_by:
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
---

# Goal
Implement the domain's `LinkHistory` port using PostgreSQL, accessed via `pgx`/`pgxpool`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/Package.create.md|internal/infrastructure/linkstore]]

# Core Principles
- No `pgx` type ever escapes this package — `Record`/`Recent` take and return only `interfaces.LinkHistoryEntry`.
- Schema is ensured (`CREATE TABLE IF NOT EXISTS`) once, in `New`, at startup — never lazily on first use.

# Structure
## Repository place
```
internal/
  infrastructure/
    linkstore/
```
## Package Structure
```
internal/infrastructure/linkstore/
  store.go      ← Store, see plateau-persistent-service--file-infrastructure-linkstore-store.skill.md
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| store.go | `Store` implementing `LinkHistory` via `github.com/jackc/pgx/v5/pgxpool` | [[skills/go/architecture/plateau/plateau-persistent-service/structure/plateau-persistent-service--file-infrastructure-linkstore-store.skill.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| github.com/jackc/pgx/v5 | >= 5.7 | PostgreSQL driver/connection pool |

# Allowed Dependencies
- `internal/domain/interfaces`
- `github.com/jackc/pgx/v5/pgxpool`

# Rules
MUST:
- Never let a `pgx` type (`pgxpool.Pool`, `pgx.Row`, ...) cross this package's boundary.
- Never import a sibling `internal/infrastructure/*` package from here.
- Every query uses parameter placeholders (`$1`, `$2`, ...) — never string-concatenated SQL.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/Package.create.md#MUST|internal/infrastructure/linkstore]]

# Check list
- [ ] No `pgx`/`pgxpool` type appears outside this package.
- [ ] No query builds its SQL string via `fmt.Sprintf`/string concatenation with a caller-supplied value.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/Package.create.md|internal/infrastructure/linkstore]]
