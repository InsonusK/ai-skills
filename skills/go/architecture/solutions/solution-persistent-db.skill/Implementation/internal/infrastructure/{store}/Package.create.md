---
description: The PostgreSQL-backed persistence adapter package
name: "internal/infrastructure/{store}"
element_kind: package
change_kind: create
tags:
  - solution/persistent-db
  - element/internal-infrastructure-store
---

# Goals
- Implement the domain's persistence port using PostgreSQL, via `pgx`.

# Core Principles
- Owns its own table's existence — creates it with `CREATE TABLE IF NOT EXISTS` on construction (see this solution's own Boundaries on migration tooling).

# Structure

## Repository place
```
internal/
  infrastructure/
    {store}/
```

## Package Structure
```
internal/infrastructure/{store}/
  store.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| store.go | `Store` struct implementing the domain's persistence port | [[./store.go.create.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| github.com/jackc/pgx/v5 | >= 5.6 | PostgreSQL driver/pool |

# What Does NOT Belong Here
- Business logic — belongs to `internal/domain/services`.
- An ephemeral/best-effort lookup with no durability requirement — belongs to [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]]'s adapter instead.

# Allowed Dependencies
- `internal/domain/interfaces` (the port this package implements)
- `github.com/jackc/pgx/v5`, `github.com/jackc/pgx/v5/pgxpool`

# Rules

## MUST
- Never let a `pgx` type (a `pgx.Row`, a `pgconn.PgError`) cross this package's boundary.
  - Risk: a caller catching a `pgx`-specific type directly couples domain-adjacent code to this one adapter's driver choice.
  - Fix: map every query result to the domain's own `{Record}` type, and every driver error to a plain `error` (wrapped with context), before returning.

# Check list
- [ ] No `pgx`/`pgxpool` type appears outside this package.
