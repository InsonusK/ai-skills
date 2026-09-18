---
name: plateau-persistent-service--file-infrastructure-linkstore-store
description: internal/infrastructure/linkstore/store.go of the plateau-persistent-service plateau
whenToUse: when creating or editing internal/infrastructure/linkstore/store.go, or reviewing how link-check history is durably stored
domain: skill
type: template
plateau: plateau-persistent-service
version: 20260917040000
tags:
  - skill/template/file
  - plateau/plateau-persistent-service
created_by:
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
---

# Goal
Implement `interfaces.LinkHistory` against a PostgreSQL `link_checks` table.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/store.go.create.md|store.go]]

# Core Principles
- Apply ONE plateau template per file.
- `New` fails fast if the schema cannot be ensured — never returns a `*Store` that might fail on its first real query.
- Every query uses parameter placeholders — never string-concatenated SQL.

# Implementation
```go
// Skill: file-infrastructure-linkstore-store
// Plateau: plateau-persistent-service
// Version: 20260917040000

// Package linkstore durably stores link-check records in PostgreSQL.
package linkstore

import (
	"context"
	"fmt"

	"github.com/jackc/pgx/v5/pgxpool"

	"{module-path}/internal/domain/interfaces"
)

type Store struct {
	pool *pgxpool.Pool
}

// New connects to PostgreSQL and ensures this adapter's table exists.
func New(ctx context.Context, dsn string) (*Store, error) {
	pool, err := pgxpool.New(ctx, dsn)
	if err != nil {
		return nil, fmt.Errorf("connect: %w", err)
	}
	if _, err := pool.Exec(ctx, `
		CREATE TABLE IF NOT EXISTS link_checks (
			id SERIAL PRIMARY KEY,
			normalized TEXT NOT NULL,
			flagged BOOLEAN NOT NULL,
			reason TEXT NOT NULL,
			checked_at TIMESTAMPTZ NOT NULL
		)`); err != nil {
		pool.Close()
		return nil, fmt.Errorf("ensure schema: %w", err)
	}
	return &Store{pool: pool}, nil
}

func (s *Store) Close() {
	s.pool.Close()
}

func (s *Store) Record(ctx context.Context, entry interfaces.LinkHistoryEntry) error {
	_, err := s.pool.Exec(ctx,
		`INSERT INTO link_checks (normalized, flagged, reason, checked_at) VALUES ($1, $2, $3, $4)`,
		entry.Normalized, entry.Flagged, entry.Reason, entry.CheckedAt)
	return err
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
Verified against this plateau's own `example/internal/infrastructure/linkstore/store.go` against a **real PostgreSQL instance** (installed via `apt`, started manually — container init doesn't auto-start services): two `Record` calls followed by `Recent(limit=10)` returned both rows, most-recent-first; confirmed independently via a direct `psql` query against `link_checks`; **the service process was killed and restarted, and `Recent` still returned the same two rows with no new checks made** — the ground-truth proof of durable persistence across a process lifetime, not just within one.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/store.go.create.md|store.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `New` must fail fast (return an error) if the schema cannot be ensured — never return a `*Store` that might fail on its first real query instead.
- Every query must use parameter placeholders (`$1`, `$2`, ...) — never string-concatenate a value into SQL.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/store.go.create.md#MUST|store.go]]

# Check list
- [ ] `New` returns a non-nil error if the table cannot be created.
- [ ] No query in this file builds its SQL string via `fmt.Sprintf`/string concatenation with a caller-supplied value.
- [ ] `Close` is called exactly once, deferred immediately after a successful `New` in `main.go`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/store.go.create.md|store.go]]

# Unittest TestCases
- [ ] WHEN `Record` then `Recent(limit=1)` is called THEN the just-recorded entry is returned
- [ ] WHEN `Recent` is called with `limit` smaller than the number of stored rows THEN exactly `limit` rows are returned, most recent first

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/store.go.create.md|store.go]]
