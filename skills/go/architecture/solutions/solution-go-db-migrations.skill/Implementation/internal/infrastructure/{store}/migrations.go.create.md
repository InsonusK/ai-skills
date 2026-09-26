---
description: Migrate — the one function that applies this catalog's versioned, embedded SQL migrations via goose, called from exactly one of cmd/migrate's deploy-time job or cmd/{service}/main.go's guarded startup path, never both
project_name: "internal/infrastructure/{store}"
name: migrations
element_kind: functions
change_kind: create
tags:
  - solution/go-db-migrations
  - element/internal-infrastructure-store-migrations-go
---

# Naming convention
| use case | identifier name pattern | identifier name | file name pattern | file name |
| -------- | ------------------------ | ---------------- | ------------------- | --------- |
| the migration runner | `Migrate` | `Migrate` | `migrations.go` | `migrations.go` |
| the embedded migration files | `{version}_{name}.sql` (one file, `-- +goose Up`/`-- +goose Down` sections) | `00001_create_{table}.sql` | `migrations/{version}_{name}.sql` | `migrations/00001_create_link_checks.sql` |

# Implementation changes
```go
// Package {store} durably stores {concept} records in PostgreSQL.
package {store}

import (
	"context"
	"embed"
	"fmt"

	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/stdlib"
	"github.com/pressly/goose/v3"
	"github.com/pressly/goose/v3/lock"
)

//go:embed migrations/*.sql
var migrationsFS embed.FS

// Migrate applies every migration in migrations/ that dsn's database has not
// yet recorded, and is a no-op if it is already current. It is this
// solution's single source of truth for {table}'s schema and every change
// made to it since — see this solution's own glossary/goose.md.
//
// Called from exactly one of two places, chosen by the deployment platform
// via the MigrateOnStart config flag — cmd/migrate's own main.go (Job mode,
// MIGRATE_ON_START=false, the default) or cmd/{service}/main.go's own
// guarded startup path (MigrateOnStart mode, MIGRATE_ON_START=true) — never
// both. See adr/migration-mode-per-platform.md for which platforms use
// which and why.
func Migrate(ctx context.Context, dsn string) error {
	connConfig, err := pgx.ParseConfig(dsn)
	if err != nil {
		return fmt.Errorf("parse dsn: %w", err)
	}
	db := stdlib.OpenDB(*connConfig)
	defer db.Close()

	locker, err := lock.NewPostgresSessionLocker()
	if err != nil {
		return fmt.Errorf("build session locker: %w", err)
	}

	provider, err := goose.NewProvider(goose.DialectPostgres, db, migrationsFS,
		goose.WithSessionLocker(locker))
	if err != nil {
		return fmt.Errorf("init migrator: %w", err)
	}

	if _, err := provider.Up(ctx); err != nil {
		return fmt.Errorf("run migrations: %w", err)
	}
	return nil
}
```

```go
migrations/00001_create_{table}.sql
```
```sql
-- +goose Up
CREATE TABLE IF NOT EXISTS {table} (
	id SERIAL PRIMARY KEY,
	input TEXT NOT NULL,
	checked_at TIMESTAMPTZ NOT NULL
);

-- +goose Down
DROP TABLE IF EXISTS {table};
```

This catalog's own runnable examples concretize this as `linkstore.Migrate` and
`migrations/00001_create_link_checks.sql` — see `plateau-persistent-service`'s `example/` for how
[[../store.go.extend.md|store.go]] stops creating the table inline, and how
[[../../cmd/migrate/main.go.create.md|cmd/migrate/main.go]] and
[[../../cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]'s guarded call are this
function's two (mutually exclusive) callers. Verified directly against the real
`github.com/pressly/goose/v3 v3.28.0` release (compiled and vetted in a throwaway scratch module
during this solution's authoring, including the
`lock.NewPostgresSessionLocker`/`WithSessionLocker` wiring) — see
[[../../../../adr/migration-tool-choice.md|adr/migration-tool-choice.md]].

# Rule changes

## MUST
- Every migration file must ship as a single `NNNNN_{name}.sql` with a `-- +goose Up` section and,
  when the change is reversible, a `-- +goose Down` section — zero-padded, strictly increasing.
  Never edit an already-shipped migration file once it may have run against any environment.
  - Risk: editing a migration a running environment already applied leaves that environment's
    tracking table recording a version whose file content has since changed, silently diverging from
    every environment that migrated after the edit.
  - Fix: change the schema going forward with a new, higher-numbered migration file; never rewrite
    history.
- `Migrate` must build its `goose.Provider` with `goose.WithSessionLocker(lock.
  NewPostgresSessionLocker())` — never call `provider.Up` unlocked.
  - Violation: omitting `WithSessionLocker`, which goose accepts silently (no error, just no lock).
  - Risk: two replicas starting at the same moment and both calling `Migrate` unlocked can race each
    other applying the same migration, corrupting the tracking table or double-applying a
    non-idempotent statement.
  - Fix: always pass `goose.WithSessionLocker(locker)` when constructing the provider, exactly as
    shown above.
- `migrations.go` must be the only file in this package that imports
  `github.com/pressly/goose/v3` (or its subpackages) — `store.go` calls `Migrate`, it never drives
  goose directly.
  - Risk: a second, slightly different migration call site drifts from this file's own lock/error
    handling and defeats the "one source of truth" goal this solution exists for.
  - Fix: keep every goose import inside this file; every other caller goes through `Migrate`.

# Check list
- [ ] Every migration file is a single, zero-padded, strictly increasing `NNNNN_{name}.sql` with a
      `-- +goose Up` section; no already-shipped file was edited in place.
- [ ] `Migrate` always constructs its provider with `goose.WithSessionLocker(...)`.
- [ ] No file other than `migrations.go` imports `github.com/pressly/goose/v3`.

# Unittest TestCases
- [ ] WHEN `Migrate` is called twice in a row against the same database THEN the second call also
      returns `nil` (idempotent, not an error the second time)
- [ ] WHEN `Migrate` is called against a database already at the latest version THEN it returns `nil`
      without attempting to reapply any migration
