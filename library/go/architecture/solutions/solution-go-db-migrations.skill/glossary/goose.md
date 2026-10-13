# goose

**goose** (`github.com/pressly/goose/v3`) is a Go library and CLI for applying versioned database
schema changes in order — instead of one process re-running the same `CREATE TABLE IF NOT EXISTS`
on every start, the schema's history is a numbered sequence of migration files, and goose tracks
which of them a given database has already applied.

## Why it exists

Without a migration tool, a database's schema either drifts (each environment gets whatever
`ALTER TABLE` someone happened to run by hand) or a service's own startup code grows an
ever-longer, hand-maintained sequence of idempotent DDL statements it must re-check on every boot.
goose replaces both with an ordered list of migration files, each recording exactly one schema
change, plus a tracking table in the target database recording which changes have already run.
Applying the same migration set to the same database twice is a no-op the second time.

## How it works

1. Migration files are named `{version}_{name}.sql` (e.g. `00001_create_link_checks.sql`), each
   containing a `-- +goose Up` section and, when the change is reversible, a `-- +goose Down`
   section — read from a `go:embed`'d `migrations/` directory in this solution, so the files ship
   inside the compiled binary.
2. A `*sql.DB` — in this solution, opened from `pgx.ParseConfig(dsn)` +
   `pgx/v5/stdlib.OpenDB(*connConfig)`, never `database/sql`'s `lib/pq` driver — is handed to
   `goose.NewProvider(goose.DialectPostgres, db, migrationsFS, ...)`.
3. Before running, this solution enables a **session-level Postgres advisory lock** via
   `goose.WithSessionLocker(lock.NewPostgresSessionLocker())` — if two processes call `Up()` at the
   same moment (e.g. several replicas starting concurrently), the second one waits for the first to
   finish instead of racing it. Unlike some other Go migration tools, this locking is **opt-in** in
   goose — omitting `WithSessionLocker` runs unlocked with no error, which is why this solution's own
   `migrations.go` treats it as a MUST, not an optional flourish.
4. `provider.Up(ctx)` applies every migration newer than the database's current recorded version; if
   none are newer it is simply a no-op.

## How it is structured

- **`goose.Provider`** — the object `goose.NewProvider` returns, combining a dialect, a `*sql.DB`,
  and a migration source (`fs.FS`); its `Up`/`Down`/`Status` methods drive the actual migration run.
- **`lock.SessionLocker`** — the pluggable locking strategy; `lock.NewPostgresSessionLocker()` is
  Postgres's own advisory-lock-based implementation, passed in via `goose.WithSessionLocker`.
- Migration files themselves are plain SQL with `-- +goose Up`/`-- +goose Down` marker comments —
  goose also supports migrations written as Go functions (`AddMigration`) for changes that need to
  query or transform existing data, not used by this solution today.

## Example

This solution's own `Migrate(ctx, dsn) error` function (see
[migrations.go](../Implementation/internal/infrastructure/{store}/migrations.go.create.md)) is
the concrete example: it opens a `*sql.DB` via `pgx`'s own `stdlib` package, wires a
session-locked `goose.Provider` over an embedded `migrations/` directory, and calls `Up(ctx)`.

## Related concepts

- [[../adr/migration-tool-choice.md|This solution's own ADR]] — why goose was chosen over
  golang-migrate, dbmate, and Atlas for this catalog, including a real, measured comparison
  (release recency, `go.sum` footprint, locking defaults) rather than reputation alone.

## Sources

- https://github.com/pressly/goose — official repository and documentation.
- Verified directly during this solution's authoring: fetched
  `github.com/pressly/goose/v3 v3.28.0` into a scratch module, read `lock/postgres.go` and
  `provider_options.go` to confirm `WithSessionLocker`'s opt-in behavior, and compiled a throwaway
  program exercising the exact `Migrate` shape above.
