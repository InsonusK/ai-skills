---
name: plateau-gw009-001--file-taskbox-test-postgres-test
description: internal/taskbox/test/postgres_test.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when changing or debugging the TaskBox conformance run on this plateau
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
source: internal/taskbox/test/postgres_test.go
---

# Goal
The conformance runner's the PostgreSQL store under test, migrated by the service's own Migrate.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/postgres_test.go.create.md|postgres_test.go]]

# Core Principles
- Apply ONE plateau template per file.
- Verbatim from the solution's Implementation file (`{store}` = `linkstore`).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/postgres_test.go.create.md|postgres_test.go]]

# Implementation
```go
// Skill: file-taskbox-test-postgres-test
// Plateau: plateau-gw009-001
// Version: 20260928120000

package test

import (
	"context"
	"fmt"
	"time"

	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgxpool"

	"{module-path}/internal/infrastructure/linkstore"
	"{module-path}/internal/taskbox"
	"{module-path}/internal/taskbox/pgstore"
)

// postgresUnderTest binds the steps to pgstore on a real PostgreSQL whose
// schema comes from the service's own migrations.
type postgresUnderTest struct {
	pool  *pgxpool.Pool
	store *pgstore.Store
}

func newPostgresUnderTest(ctx context.Context, dsn string) (*postgresUnderTest, error) {
	if err := linkstore.Migrate(ctx, dsn); err != nil {
		return nil, fmt.Errorf("migrate %s: %w", dsn, err)
	}
	cfg, err := pgxpool.ParseConfig(dsn)
	if err != nil {
		return nil, err
	}
	// Enough connections for the concurrent-enqueue scenario's transactions
	// and the workers at once; the default (4) would serialize them.
	cfg.MaxConns = 40
	pool, err := pgxpool.NewWithConfig(ctx, cfg)
	if err != nil {
		return nil, err
	}
	return &postgresUnderTest{pool: pool, store: pgstore.New(pool)}, nil
}

func (p *postgresUnderTest) name() string               { return "postgres" }
func (p *postgresUnderTest) excludedKind() string       { return "transient" }
func (p *postgresUnderTest) taskStore() taskbox.Store   { return p.store }
func (p *postgresUnderTest) configure(s settings) error { return nil } // partitions: Redis only
func (p *postgresUnderTest) close()                     { p.pool.Close() }

func (p *postgresUnderTest) reset(ctx context.Context) error {
	_, err := p.pool.Exec(ctx, `TRUNCATE taskbox_task, taskbox_group`)
	return err
}

func (p *postgresUnderTest) enqueue(ctx context.Context, commit bool, tasks []taskbox.NewTask) ([]bool, error) {
	tx, err := p.pool.Begin(ctx)
	if err != nil {
		return nil, err
	}
	defer func() { _ = tx.Rollback(ctx) }()
	added := make([]bool, len(tasks))
	for i, t := range tasks {
		if added[i], err = p.store.Enqueue(ctx, tx, t); err != nil {
			return nil, err
		}
	}
	if !commit {
		return added, tx.Rollback(ctx)
	}
	return added, tx.Commit(ctx)
}

func (p *postgresUnderTest) enqueueHeld(ctx context.Context, t taskbox.NewTask, hold time.Duration) error {
	return pgx.BeginFunc(ctx, p.pool, func(tx pgx.Tx) error {
		if _, err := p.store.Enqueue(ctx, tx, t); err != nil {
			return err
		}
		time.Sleep(hold)
		return nil
	})
}
```

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/postgres_test.go.create.md|postgres_test.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Keep this file identical to the solution's Implementation file.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/postgres_test.go.create.md|postgres_test.go]]

# Check list
- [ ] `TEST_DATABASE_DSN=… make unit-test` runs the feature on PostgreSQL.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/postgres_test.go.create.md|postgres_test.go]]
