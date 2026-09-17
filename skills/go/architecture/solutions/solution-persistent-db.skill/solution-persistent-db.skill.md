---
name: solution-persistent-db
description: A narrow, business-named outbound port backed by a durable system-of-record store (PostgreSQL via pgx), independent of solution-cached-db
whenToUse: when the module needs to durably store and reload data across restarts, or reviewing whether domain code depends on a generic repository interface instead of a business-named one
domain: skill
type: architecture
version: 20260917000000
tags:
  - skill/architecture/solution
  - solution/persistent-db
  - stack/go
  - concern/architecture
creates:
  - "internal/domain/interfaces/{store-port}.go"
  - "internal/infrastructure/{store}/"
extends:
  - "internal/domain/services/{service}.go"
  - "cmd/{service}/main.go"
  - "internal/config/config.go"
depends_on:
  - "[[skills/go/architecture/solutions/solution-go-domain-ports.skill/solution-go-domain-ports.skill.md|solution-go-domain-ports]]"
built_on_plateau:
adr:
  - "[[adr/postgres-via-pgx.md|PostgreSQL via pgx]]"
---

# Goal
- Let the domain durably store and reload data across restarts through a narrow, business-named outbound port — demonstrated backed by PostgreSQL.

# Capabilities
- Data the module owns survives a process restart or redeploy.
- The persistence technology stays fully swappable — nothing outside `internal/infrastructure/{store}` depends on PostgreSQL or `pgx` directly.

# Core Principles
- The port is named for the business record it stores (e.g. `LinkHistory`), never generically (`Repository<T>`) — same reasoning as [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]]'s own ADR, applied here.
- Independent of `solution-cached-db` — this catalog's `CachedDb` and `PersistentDb` VPs carry no constraint between them; a module may have either, neither, or both.

# Boundaries
- Schema migration tooling is out of scope — this solution's adapter creates its table with `CREATE TABLE IF NOT EXISTS` on construction, adequate for this catalog's own runnable examples; a production consumer of this catalog is expected to replace that with real migration tooling of its own choosing.

# Adr
- [[adr/postgres-via-pgx.md|PostgreSQL via pgx]]
  - Selected variant: PostgreSQL, accessed via `github.com/jackc/pgx/v5/pgxpool` directly (not `database/sql`)

# Requirements
SOLUTION:
- [[skills/go/architecture/solutions/solution-go-domain-ports.skill/solution-go-domain-ports.skill.md|solution-go-domain-ports]]
  - [[skills/go/architecture/solutions/solution-go-domain-ports.skill/Implementation/internal/domain/interfaces/Package.create.md|internal/domain/interfaces]] - the package this solution adds its port file to
GO MODULES:
- github.com/jackc/pgx/v5
  - `pgxpool.New`, `Pool.Exec`/`Pool.Query` — the PostgreSQL adapter

# Template Skill Mutations
FILES:
- [[./Implementation/internal/domain/interfaces/{store-port}.go.create.md|internal/domain/interfaces/{store-port}.go]] - create - the outbound persistence port
- [[./Implementation/internal/domain/services/{service}.go.extend.md|internal/domain/services/{service}.go]] - extend - the domain service depends on the new port
- [[./Implementation/internal/infrastructure/{store}/Package.create.md|internal/infrastructure/{store}]] - create - the PostgreSQL adapter package
- [[./Implementation/internal/infrastructure/{store}/store.go.create.md|store.go]] - create - `Store` struct implementing the port
- [[./Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]] - extend - construct the connection pool and pass it to the domain service
- [[./Implementation/internal/config/config.go.extend.md|internal/config/config.go]] - extend - add the database DSN

# Workflow

## Record and reload (happy path)
1. The domain service calls the port's write method after producing a result.
2. The adapter executes an `INSERT` against PostgreSQL.
3. Later, the domain service (or a different process entirely, after a restart) calls the port's read method and gets the durably-stored data back.

# Rules

## MUST
- [[./Implementation/internal/domain/interfaces/{store-port}.go.create.md#MUST|internal/domain/interfaces/{store-port}.go]]
- [[./Implementation/internal/infrastructure/{store}/store.go.create.md#MUST|store.go]]

# Check list
- [ ] `internal/domain/services` never imports `github.com/jackc/pgx/v5` directly.
- [ ] The adapter's table survives and is reloadable across a process restart (verified against a real PostgreSQL instance, not just compiled).
