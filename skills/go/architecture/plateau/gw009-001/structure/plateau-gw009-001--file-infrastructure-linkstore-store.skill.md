---
name: plateau-gw009-001--file-infrastructure-linkstore-store
description: internal/infrastructure/linkstore/store.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when creating or editing internal/infrastructure/linkstore/store.go, or reviewing how a link-check record and its follow-up tasks are stored in one transaction
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
  - "[[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]]"
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
source: internal/infrastructure/linkstore/store.go
---

# Goal
Implement `interfaces.LinkHistory` against PostgreSQL: `Record` writes the `link_checks` row and enqueues its follow-up TaskBox tasks in one `pgx.Tx`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/store.go.create.md|store.go]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/infrastructure/{store}/store.go.extend.md|store.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Core Principles
- Apply ONE plateau template per file.
- The schema comes from the service's migrations (`linkstore.Migrate`); `New` only takes the pool and the TaskBox store `main.go` built.
- `Record` writes data and tasks on one `pgx.Tx` via `pgx.BeginFunc` — both commit or neither does.
- Every query uses parameter placeholders — never string-concatenated SQL.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/store.go.create.md|store.go]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/infrastructure/{store}/store.go.extend.md|store.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Implementation
```go
// Skill: file-infrastructure-linkstore-store
// Plateau: plateau-gw009-001
// Version: 20260928120000

// Package linkstore durably stores link-check records in PostgreSQL, and
// owns the service's schema history (migrations/).
package linkstore

import (
	"context"
	"encoding/json"
	"fmt"

	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgxpool"

	"{module-path}/internal/domain/interfaces"
	"{module-path}/internal/taskbox"
	"{module-path}/internal/taskbox/pgstore"
)

type Store struct {
	pool  *pgxpool.Pool
	tasks *pgstore.Store
}

// New uses a pool whose schema Migrate has already brought up to date; tasks
// is the TaskBox store on the same database, so a record and its tasks share
// one transaction.
func New(pool *pgxpool.Pool, tasks *pgstore.Store) *Store {
	return &Store{pool: pool, tasks: tasks}
}

// Record inserts entry and enqueues tasks in one transaction.
func (s *Store) Record(ctx context.Context, entry interfaces.LinkHistoryEntry, tasks ...interfaces.Task) error {
	return pgx.BeginFunc(ctx, s.pool, func(tx pgx.Tx) error {
		if _, err := tx.Exec(ctx,
			`INSERT INTO link_checks (normalized, flagged, reason, checked_at) VALUES ($1, $2, $3, $4)`,
			entry.Normalized, entry.Flagged, entry.Reason, entry.CheckedAt); err != nil {
			return err
		}
		for _, t := range tasks {
			payload, err := json.Marshal(t.Payload)
			if err != nil {
				return fmt.Errorf("encode %s payload: %w", t.Type, err)
			}
			if _, err := s.tasks.Enqueue(ctx, tx, taskbox.NewTask{
				Type: t.Type, Payload: payload, Group: t.Group, RunAt: t.RunAt,
			}); err != nil {
				return fmt.Errorf("enqueue %s: %w", t.Type, err)
			}
		}
		return nil
	})
}

func (s *Store) Recent(ctx context.Context, limit int) ([]interfaces.LinkHistoryEntry, error) {
	rows, err := s.pool.Query(ctx,
		`SELECT normalized, flagged, reason, checked_at FROM link_checks ORDER BY checked_at DESC LIMIT $1`, limit)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var out []interfaces.LinkHistoryEntry
	for rows.Next() {
		var e interfaces.LinkHistoryEntry
		if err := rows.Scan(&e.Normalized, &e.Flagged, &e.Reason, &e.CheckedAt); err != nil {
			return nil, err
		}
		out = append(out, e)
	}
	return out, rows.Err()
}
```

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/store.go.create.md|store.go]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/infrastructure/{store}/store.go.extend.md|store.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Every query must use parameter placeholders (`$1`, `$2`, ...) — never string-concatenate a value into SQL.
- `Record` must execute the insert and every `Enqueue` on the same `tx` — never `s.pool.Exec` for one of them.
- Never create or alter a table here; schema changes are new migration files.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/store.go.create.md|store.go]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/infrastructure/{store}/store.go.extend.md|store.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Check list
- [ ] An `Enqueue` error rolls back the `link_checks` insert (one `BeginFunc`).
- [ ] No query builds its SQL string from a caller-supplied value.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/store.go.create.md|store.go]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/infrastructure/{store}/store.go.extend.md|store.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]
