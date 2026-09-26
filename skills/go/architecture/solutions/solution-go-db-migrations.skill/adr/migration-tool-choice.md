---
name: migration-tool-choice
description: Which schema-migration tool manages PersistentDb's table(s), and how it is invoked
problem: solution-persistent-db's Store.New creates its table with a bare CREATE TABLE IF NOT EXISTS — adequate for this catalog's own runnable examples, but explicitly flagged (its own Boundaries) as not a real schema-migration story. A production consumer needs one versioned, single-source-of-truth description of the schema across DB versions, runnable either once at service startup or once as a separate deploy-time job — not on every request, and not re-derived by hand from reading old code.
decision: github.com/pressly/goose/v3, driven over a plain *sql.DB opened from this catalog's own pgx/v5 pool config (via pgx.ParseConfig + pgx/v5/stdlib.OpenDB) and a go:embed'd migrations/ directory, with Postgres session-level advisory locking enabled via goose.WithSessionLocker — exposed as one Migrate(ctx, dsn) function callable from either cmd/{service}/main.go (startup) or the new cmd/migrate binary (deploy-time job).
tags:
  - solution/go-db-migrations
  - concern/documentation
  - concern/documentation/adr
  - stack/go
---

# Problem

`solution-persistent-db`'s adapter ensures its own schema with `CREATE TABLE IF NOT EXISTS` inside
`Store.New`, run on every process start. That solution's own `# Boundaries` already names this as a
deliberate simplification: adequate for this catalog's own runnable examples, but not a real
migration story — no versioning, no `ALTER TABLE` path between schema versions, and no way to run
schema changes independently of starting the service.

This decision needs one concrete, demonstrated realization of "manage `PersistentDb`'s schema via a
migration tool" for Go — a library, an invocation shape, and a single place that describes the
schema and every change made to it since — the same way `postgres-via-pgx.md` picked one concrete
database/driver pair rather than leaving both unstated. Two requirements came directly from the
catalog owner: the tool must support running **once, at service startup** *or* **once, as a
separate job at deploy time** (not gated behind every request, and not required to pick only one of
the two shapes at authoring time); and the migration files themselves must be the **one place** that
describes the schema and its history, not tribal knowledge split across ad hoc `ALTER TABLE`
statements.

A first pass at this ADR selected `golang-migrate`. Before finalizing, its currently-perceived
"is this project still maintained?" question was checked directly rather than assumed either way —
see [Verification performed](#verification-performed) — and the same direct verification was then
applied to `goose`, which changed the outcome below.

# Selected variant

**Selected variant:** [[#goose, via a plain *sql.DB + go:embed + session-level advisory lock (selected)]]

# Verification performed

Both `golang-migrate/migrate/v4` and `pressly/goose/v3` were fetched at their real latest releases
into throwaway scratch modules, compiled and `go vet`-ed against this solution's actual code shape
(a `Migrate(ctx, dsn) error` function, `pgx`-based, `go:embed`'d SQL migrations), and checked against
the Go module proxy (`proxy.golang.org/.../@latest`, `@v/list`) and the GitHub API for release
recency and maintenance status — not assumed from either tool's general reputation:

| | golang-migrate/migrate/v4 | pressly/goose/v3 |
| --- | --- | --- |
| Latest release (checked 2026-09-19) | `v4.20.1`, published 2026-09-09 | `v3.28.0`, published 2026-09-02 |
| Repository archived? | No | No |
| GitHub stars / open issues | 18,930 / 494 | 11,478 / 132 |
| Concurrent-replica (Postgres advisory) locking | **Default-on** — its own `pgx/v5` driver defaults to `x-lock-strategy=advisory` | **Opt-in** — `lock.NewPostgresSessionLocker()` passed to `goose.WithSessionLocker`, same underlying `pg_advisory_lock` mechanism once enabled |
| Database handle | Its own `database/pgx/v5` driver opens its own `*sql.DB` internally via `sql.Open("pgx/v5", ...)`, itself backed by `pgx/v5/stdlib` | `goose.NewProvider(dialect, *sql.DB, fs.FS, ...)` takes a plain `*sql.DB` the caller builds — verified working via `pgx.ParseConfig(dsn)` + `stdlib.OpenDB(*connConfig)` |
| `go.sum` footprint in a minimal scratch module importing only what this solution needs | 87 lines — its own `go.mod` is not fully graph-pruned across its many optional database drivers | **27 lines** |
| Migration file shape | Separate `{version}_{name}.up.sql` / `.down.sql` file pair per migration | One `{version}_{name}.sql` file per migration, `-- +goose Up` / `-- +goose Down` sections |
| SQL-only vs. also-Go-function migrations | SQL only | SQL, or Go functions when a change needs to query/transform data, not just DDL (unused today, available if a future migration needs it) |

Neither project is stalled — both released within roughly two weeks of this decision being made.
The two searched variants below are described on this now-verified basis, not on unverified
reputation.

# Searched variants

## goose, via a plain *sql.DB + go:embed + session-level advisory lock (selected)

### Description

Use `github.com/pressly/goose/v3`. `migrations.go` opens a short-lived `*sql.DB` for the duration of
a migration run via `pgx.ParseConfig(dsn)` + `pgx/v5/stdlib.OpenDB(*connConfig)` (never `lib/pq`,
keeping this decision consistent with `postgres-via-pgx.md`'s own "`pgx` directly" choice), builds a
`goose.NewProvider(goose.DialectPostgres, db, migrationsFS, goose.WithSessionLocker(lock.
NewPostgresSessionLocker()))`, and calls `provider.Up(ctx)`. Migration files are embedded via
`go:embed migrations/*.sql`, one `{version}_{name}.sql` file per migration with `-- +goose Up` /
`-- +goose Down` sections — the single, versioned source of truth for the schema and every change
made to it since.

### Benefits

- Noticeably lighter dependency footprint (27 vs. 87 `go.sum` lines in the verification above) —
  its own `go.mod` is properly graph-pruned.
- Session-level Postgres advisory locking (`lock.NewPostgresSessionLocker`, enabled via
  `WithSessionLocker`) gives the same "concurrent replicas starting at once serialize instead of
  racing" guarantee `golang-migrate` provides by default — one extra, explicit line, not a missing
  capability.
- `NewProvider` taking a plain `*sql.DB` (rather than dispatching by URL scheme) means this
  solution's connection-opening code is explicit and inspectable in `migrations.go` itself, instead
  of hidden behind a driver's own URL parsing.
- Go-function migrations are available (not used today) if a future migration ever needs to
  transform existing row data rather than just change DDL — a capability `golang-migrate` does not
  offer at all.

### Costs

- Locking must be explicitly enabled (`WithSessionLocker`) — an omission here is silent (no error,
  just no lock), unlike `golang-migrate` where it is on unless explicitly turned off. Mitigated by
  this being a single, tested line in `migrations.go` itself (see its own `# Rule changes`).
- A short-lived `*sql.DB` opened via `stdlib.OpenDB` for the duration of a migration run is a second
  connection path alongside `Store`'s own long-lived `pgxpool.Pool` — an accepted cost, since a
  migration run is a short, infrequent, separate operation from the pool's normal request-serving
  lifetime (the same accepted shape `golang-migrate` would have had too).
- Smaller community/ecosystem than `golang-migrate` by star count, though both are actively
  maintained as of this decision (see the verification table above).

## golang-migrate/migrate v4 (rejected)

### Description

Use `github.com/golang-migrate/migrate/v4`, with its own `database/pgx/v5` driver (`pgx5://` URL
scheme) and `source/iofs` driver over the same `go:embed`'d `migrations/` directory, in a
`{version}_{name}.up.sql`/`.down.sql` file-pair shape.

### Benefits

- Larger community (18.9k stars vs. 11.5k) and, in this catalog author's own experience, more
  commonly the first tool named in Go migration tutorials.
- Postgres advisory-lock safety is on by default — nothing to remember to enable.

### Costs

- Noticeably heavier `go.sum` footprint for the same capability (87 vs. 27 lines, measured above).
- Its `pgx/v5` driver always opens its own `*sql.DB` via `sql.Open("pgx/v5", ...)` internally when
  driven through the simpler `NewWithSourceInstance(..., databaseURL)` path; reaching the same
  "caller supplies the `*sql.DB`" shape `goose` offers directly requires the lower-level
  `NewWithDatabaseInstance` + the driver's own `WithInstance`, more ceremony for the same result.
- No Go-function migration support — a real (if currently unused) capability gap next to `goose`.

## amacneil/dbmate (rejected)

### Description

Use `github.com/amacneil/dbmate`, a CLI-first, language-agnostic migration tool (works the same way
across Go, Ruby, Node projects) with a thinner Go library surface than either `goose` or
`golang-migrate`.

### Benefits

- Deploy-time-job mode (the CLI binary) is dbmate's primary, best-supported shape.
- Language-agnostic tool choice is attractive for a team whose other services are not Go.

### Costs

- Weaker story for the "run once at service startup, in-process" mode this catalog's own owner
  explicitly asked to support — its Go library API for embedding a migration run inside another Go
  binary is thinner and less commonly used than `goose`'s or `golang-migrate`'s.
- No native `pgx` driver — would reintroduce `database/sql`/`lib/pq` into the dependency set for no
  benefit specific to this catalog's own adapter.

## Atlas (ariga/atlas) (rejected)

### Description

Use Atlas, a declarative "desired state" schema-management tool: the schema is described once, as
its current shape (HCL or plain SQL), and Atlas computes and applies the diff needed to reach it —
rather than a linear history of hand-written migration files.

### Benefits

- Directly solves the "one place describing the DB structure" requirement in its strongest form: the
  desired-state file *is* the current schema, not a history that must be replayed to reconstruct it.
- Can generate migrations automatically from schema changes, reducing hand-written SQL.

### Costs

- A materially heavier tool and a different paradigm (declarative diffing vs. versioned scripts)
  than anything else in this catalog — steeper to explain in a demonstration catalog whose other
  solutions are all thin, single-purpose adapters.
- Its most complete workflow (schema-as-code plus automatic diff/plan) is built around the Atlas CLI
  and, for some features, Atlas's own cloud service — more infrastructure than this catalog needs to
  demonstrate one narrow capability.

## Manual CREATE TABLE IF NOT EXISTS (status quo, rejected)

### Description

Keep `solution-persistent-db`'s current inline `CREATE TABLE IF NOT EXISTS` in `Store.New`, with no
migration tool at all.

### Benefits

- Zero additional dependency; adequate for the catalog's own single-table runnable examples.

### Costs

- No versioning: `ALTER TABLE` changes between schema versions have nowhere to live and no ordered
  history — this is exactly the gap this ADR exists to close, already named in
  `solution-persistent-db`'s own `# Boundaries`.
- No separate-job mode: schema changes only ever happen as a side effect of a service starting, which
  the catalog owner explicitly wants to be optional, not mandatory.
