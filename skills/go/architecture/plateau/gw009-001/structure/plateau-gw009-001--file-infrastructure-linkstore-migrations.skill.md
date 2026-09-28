---
name: plateau-gw009-001--file-infrastructure-linkstore-migrations
description: internal/infrastructure/linkstore/migrations.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when adding a migration to internal/infrastructure/linkstore/migrations/ or changing how Migrate applies them
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]]"
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
source: internal/infrastructure/linkstore/migrations.go
---

# Goal
Apply the service's embedded migrations — `link_checks` and TaskBox schema v1 — with goose under a PostgreSQL session lock.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/infrastructure/{store}/migrations.go.create.md|migrations.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/infrastructure/{store}/Package.extend.md|00002_taskbox_v1.sql]]

# Core Principles
- Apply ONE plateau template per file.
- `Migrate` runs from exactly one place per deployment: `cmd/migrate` (Job mode) or `cmd/linkcheck` when `MIGRATE_ON_START=true`.
- goose is given `fs.Sub(migrationsFS, "migrations")` — it reads the FS root.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/infrastructure/{store}/migrations.go.create.md|migrations.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/infrastructure/{store}/Package.extend.md|00002_taskbox_v1.sql]]

# Implementation
```go
// Skill: file-infrastructure-linkstore-migrations
// Plateau: plateau-gw009-001
// Version: 20260928120000

package linkstore

import (
	"context"
	"embed"
	"fmt"
	"io/fs"

	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/stdlib"
	"github.com/pressly/goose/v3"
	"github.com/pressly/goose/v3/lock"
)

//go:embed migrations/*.sql
var migrationsFS embed.FS

// Migrate applies every migration in migrations/ that dsn's database has not
// yet recorded — link_checks and TaskBox schema v1 alike: the service's one
// schema history. Called by cmd/migrate (Job mode) or by cmd/linkcheck's
// guarded startup (MIGRATE_ON_START=true), never both.
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

	// goose reads migrations from the root of the FS it is given.
	migrations, err := fs.Sub(migrationsFS, "migrations")
	if err != nil {
		return fmt.Errorf("open embedded migrations: %w", err)
	}

	provider, err := goose.NewProvider(goose.DialectPostgres, db, migrations,
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

```sql
-- 00001_create_link_checks.sql
-- +goose Up
CREATE TABLE IF NOT EXISTS link_checks (
	id SERIAL PRIMARY KEY,
	normalized TEXT NOT NULL,
	flagged BOOLEAN NOT NULL,
	reason TEXT NOT NULL,
	checked_at TIMESTAMPTZ NOT NULL
);

-- +goose Down
DROP TABLE IF EXISTS link_checks;
```

```sql
-- 00002_taskbox_v1.sql
-- TaskBox storage contract (VP-C003), schema v1 — copied verbatim from the contract's §6 DDL.
-- +goose Up
CREATE TABLE taskbox_task (
  seq             bigint GENERATED ALWAYS AS IDENTITY (CACHE 1) PRIMARY KEY,
  id              uuid        NOT NULL UNIQUE,
  status_key      uuid        UNIQUE,
  queue           text        NOT NULL DEFAULT 'default',
  queue_group     text,
  type            text        NOT NULL,
  payload         jsonb       NOT NULL,
  idempotency_key text        UNIQUE,
  status          text        NOT NULL DEFAULT 'pending'
                  CHECK (status IN ('pending','running','done','dead','cancelled')),
  attempt         int         NOT NULL DEFAULT 0,
  max_attempts    int         NOT NULL DEFAULT 10,
  run_at          timestamptz NOT NULL DEFAULT now(),
  locked_until    timestamptz,
  last_status     int,
  last_error      text,
  retention       interval,
  created_at      timestamptz NOT NULL DEFAULT now(),
  updated_at      timestamptz NOT NULL DEFAULT now(),
  finished_at     timestamptz
);
CREATE INDEX taskbox_task_due   ON taskbox_task (queue, run_at)          WHERE status IN ('pending','running');
CREATE INDEX taskbox_task_group ON taskbox_task (queue, queue_group, seq) WHERE status IN ('pending','running','dead');
CREATE INDEX taskbox_task_done  ON taskbox_task (finished_at)            WHERE status IN ('done','cancelled');

-- One row per group; exists only to be locked by enqueuing transactions (§3).
CREATE TABLE taskbox_group (
  queue       text NOT NULL,
  queue_group text NOT NULL,
  PRIMARY KEY (queue, queue_group)
);

-- +goose Down
DROP TABLE IF EXISTS taskbox_group;
DROP TABLE IF EXISTS taskbox_task;
```

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/infrastructure/{store}/migrations.go.create.md|migrations.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/infrastructure/{store}/Package.extend.md|00002_taskbox_v1.sql]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `Migrate` must use `goose.WithSessionLocker(lock.NewPostgresSessionLocker())`.
- `Migrate` must pass the `migrations/` subdirectory to goose, never the embed root.
- `00002_taskbox_v1.sql` must stay identical to the contract's schema v1 DDL.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/infrastructure/{store}/migrations.go.create.md|migrations.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/infrastructure/{store}/Package.extend.md|00002_taskbox_v1.sql]]

# Check list
- [ ] Running `Migrate` twice is a no-op the second time.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/infrastructure/{store}/migrations.go.create.md|migrations.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/infrastructure/{store}/Package.extend.md|00002_taskbox_v1.sql]]
