---
description: Store.New stops ensuring its own schema inline — schema is now this solution's own versioned migrations, run by a caller before/at construction
project_name: "internal/infrastructure/{store}"
name: Store
element_kind: struct
change_kind: extend
tags:
  - solution/go-db-migrations
  - element/internal-infrastructure-store-store-go
---

# Implementation changes

**AS IS** (from [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/store.go.create.md|solution-persistent-db's own store.go]]):
```go
func New(ctx context.Context, dsn string) (*Store, error) {
	pool, err := pgxpool.New(ctx, dsn)
	if err != nil {
		return nil, fmt.Errorf("connect: %w", err)
	}
	if _, err := pool.Exec(ctx, `
		CREATE TABLE IF NOT EXISTS {table} (
			id SERIAL PRIMARY KEY,
			input TEXT NOT NULL,
			checked_at TIMESTAMPTZ NOT NULL
		)`); err != nil {
		pool.Close()
		return nil, fmt.Errorf("ensure schema: %w", err)
	}
	return &Store{pool: pool}, nil
}
```

**TO BE** (this solution):
```go
// New connects to PostgreSQL. It no longer ensures {table}'s schema itself —
// see this solution's own migrations.go: Migrate(ctx, dsn) is called once,
// before this New is ever called, by whichever of cmd/migrate's job or
// cmd/{service}/main.go's guarded startup call this deployment's
// MigrateOnStart flag selects (see adr/migration-mode-per-platform.md).
func New(ctx context.Context, dsn string) (*Store, error) {
	pool, err := pgxpool.New(ctx, dsn)
	if err != nil {
		return nil, fmt.Errorf("connect: %w", err)
	}
	return &Store{pool: pool}, nil
}
```

`Close`, `Record`, and `Recent` are unchanged — this solution touches only `New`'s schema-setup
half.

# Rule changes

## MUST
- `New` must never call `Migrate` itself, in either mode, and must never re-add an inline
  `CREATE TABLE`.
  - Risk: a store constructor that migrates on every connect collapses this solution's two distinct,
    explicitly-chosen modes into an unconditional third behavior nothing selected, reintroducing the
    exact "every process instance attempts it" shape `adr/migration-mode-per-platform.md` documents
    the problems of.
  - Fix: keep `New` limited to opening the pool; the only two call sites for `Migrate` are
    `cmd/migrate`'s `main` and `cmd/{service}/main.go`'s `cfg.MigrateOnStart`-guarded call (see
    [[./migrations.go.create.md#MUST|migrations.go]]).
- A deploy where neither call site actually ran (Job skipped in Job mode; `MIGRATE_ON_START` left
  `false` with no Job configured either) gets ordinary query failures (relation does not exist) from
  `Record`/`Recent`, not a clearer error — document this in
  [[../../../../solution-go-db-migrations.skill.md#Boundaries|this solution's own Boundaries]] rather
  than adding defensive schema-existence checks to `New` or `Record`/`Recent`.
  - Risk: adding a schema-existence check to every query call defeats the point of moving schema
    management out of the request/connect path, and duplicates what `Migrate` already guarantees
    when actually run.
  - Fix: rely on the deploying platform having correctly configured exactly one of its two modes
    (per `adr/migration-mode-per-platform.md`); treat "neither ran" as an operational
    misconfiguration, not a case `Store` itself must detect.

# Check list
- [ ] `New` contains no `CREATE TABLE`/DDL statement and does not call `Migrate`.
- [ ] `Close`, `Record`, `Recent` are byte-for-byte unchanged from `solution-persistent-db`'s own
      `store.go.create.md`.

# Unittest TestCases
- [ ] WHEN `New` is called against a reachable PostgreSQL instance (schema already migrated by a
      test fixture) THEN it returns a non-nil `*Store` and no error
- [ ] WHEN `Record` is called against a `Store` whose schema was never migrated THEN it returns the
      underlying "relation does not exist" error unchanged, not a `Migrate`-specific one
