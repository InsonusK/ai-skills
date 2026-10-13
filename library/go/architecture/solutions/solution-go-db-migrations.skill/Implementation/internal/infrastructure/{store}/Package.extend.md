---
description: The persistence adapter package's schema ownership moves from inline DDL to versioned, embedded migrations
name: "internal/infrastructure/{store}"
element_kind: package
change_kind: extend
tags:
  - solution/go-db-migrations
  - element/internal-infrastructure-store
---

# Core Principles
- Owns its table's schema via versioned migration files, not inline DDL — supersedes
  `solution-persistent-db`'s own "creates it with `CREATE TABLE IF NOT EXISTS` on construction" Core
  Principle. The embedded `migrations/` directory is now the single source of truth for the schema
  and every change made to it since.

# Structure

## Package Structure
```
internal/infrastructure/{store}/
  store.go
  migrations.go
  migrations/
    0001_create_{table}.up.sql
    0001_create_{table}.down.sql
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| migrations.go | `Migrate` — applies every embedded migration; the one function both call sites use | [[./migrations.go.create.md]] |
| migrations/ | Versioned, embedded SQL migration file pairs — the schema's single source of truth | [[./migrations.go.create.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| github.com/pressly/goose/v3 | v3.28 | schema migration runner + Postgres session-level advisory lock |
| github.com/jackc/pgx/v5/stdlib | (already required by `github.com/jackc/pgx/v5`, this solution's own persistent-db dependency) | wraps `pgx` as a `database/sql.DB` for goose, without pulling in `lib/pq` |

# Allowed Dependencies
- `github.com/pressly/goose/v3`, `github.com/pressly/goose/v3/lock` — `migrations.go` only, per
  [[./migrations.go.create.md#MUST|migrations.go's own Rule]].
- `github.com/jackc/pgx/v5/stdlib` — `migrations.go` only, to bridge `pgx.ParseConfig` into the
  `*sql.DB` goose's `NewProvider` requires.

# Rules

## MUST
- [[./migrations.go.create.md#MUST|migrations.go]]
- [[./store.go.extend.md#MUST|store.go]]

# Check list
- [ ] [[./migrations.go.create.md#Check list|migrations.go]]
- [ ] [[./store.go.extend.md#Check list|store.go]]
