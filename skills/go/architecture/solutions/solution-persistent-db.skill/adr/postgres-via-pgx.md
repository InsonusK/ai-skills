---
name: postgres-via-pgx
description: Which relational database and Go driver this solution uses
problem: A persistent-store solution needs one concrete database and Go access pattern to demonstrate concretely, rather than leaving both unstated.
decision: PostgreSQL, accessed via github.com/jackc/pgx/v5/pgxpool directly (not database/sql).
tags:
  - solution/persistent-db
  - concern/documentation
  - concern/documentation/adr
  - stack/go
---

# Problem

This catalog needs one concrete, demonstrated realization of "durable system-of-record storage" for Go — both a database engine and a Go access pattern — the same way [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] demonstrates caching concretely via Redis. Leaving either choice unstated would leave the adapter's own Implementation file with nothing real to show.

# Selected variant

**Selected variant:** [[#PostgreSQL via pgxpool (selected)]]

# Searched variants

## PostgreSQL via pgxpool (selected)

### Description

Use PostgreSQL as the database, accessed through `github.com/jackc/pgx/v5/pgxpool` — pgx's own connection-pool type — called directly, not wrapped behind the standard library's `database/sql`.

### Benefits

- PostgreSQL is the most common relational default for a new Go web-service in this ecosystem, with mature tooling (`pgxpool`, migration tools, local Docker images) a consumer of this catalog is likely to already have.
- `pgx`'s native interface (as opposed to `database/sql`) supports PostgreSQL-specific types and batching directly, and is the interface the Go community's own `pgx` maintainers recommend for new code over the `database/sql` compatibility shim.
- A single, concrete choice here means `solution-persistent-db`'s adapter Implementation file shows real, copy-adaptable code instead of an abstracted "your driver here" placeholder.

### Costs

- Ties this solution's adapter code specifically to `pgx`'s API; a consumer preferring `database/sql` (e.g. to swap engines later via a generic driver interface) adapts the one adapter file, not the port or the domain service.
- PostgreSQL specifically (vs. MySQL or SQLite) is itself a choice a real consumer may want to revisit — this ADR records the catalog's own demonstrated default, not a mandate.

## database/sql with a swappable driver

### Description

Use the standard library's `database/sql` package with a driver registered by import side-effect (`_ "github.com/lib/pq"` or similar), so the SQL dialect/driver is nominally swappable later.

### Benefits

- Standard-library interface, familiar to any Go developer regardless of which database they've used before.
- Driver swap requires only an import change, in principle.

### Costs

- `database/sql`'s lowest-common-denominator interface loses PostgreSQL-specific capabilities (native array/JSONB types, `COPY`, pgx's efficient batch protocol) that a real system-of-record adapter is likely to want eventually.
- "Swappable driver" is rarely exercised in practice once a schema starts using engine-specific SQL — the abstraction's main benefit is usually theoretical.

## SQLite (embedded, no separate server)

### Description

Use `modernc.org/sqlite` (a pure-Go, CGO-free SQLite driver) so the runnable example needs no separate database process.

### Benefits

- Zero external infrastructure to run the catalog's own example — simpler ground-truth verification.

### Costs

- Diverges from what a real multi-instance, horizontally-scaled web-service would actually run in production (SQLite's single-writer model does not fit a service meant to run more than one replica), so the catalog's own example would model a shape most consumers would have to redesign immediately.
