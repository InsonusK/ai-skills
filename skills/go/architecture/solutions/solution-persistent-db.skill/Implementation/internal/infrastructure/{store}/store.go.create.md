---
description: Store struct — PostgreSQL-backed implementation of the domain's persistence port
project_name: "internal/infrastructure/{store}"
name: Store
element_kind: struct
change_kind: create
tags:
  - solution/persistent-db
  - element/internal-infrastructure-store-store-go
---

# Naming convention
| use case | struct name pattern | struct name | file name pattern | file name |
| -------- | -------------------- | ------------ | ------------------- | --------- |
| the adapter | `Store` | `Store` | `store.go` | `store.go` |

# Implementation changes
```go
// Package {store} durably stores {concept} records in PostgreSQL.
package {store}

import (
	"context"
	"fmt"

	"github.com/jackc/pgx/v5/pgxpool"

	"{module-path}/internal/domain/interfaces"
)

type Store struct {
	pool *pgxpool.Pool
}

// New connects to PostgreSQL and ensures this adapter's table exists. See
// this solution's own Boundaries: real migration tooling is expected to
// replace this CREATE TABLE IF NOT EXISTS in a production consumer.
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

func (s *Store) Close() {
	s.pool.Close()
}

func (s *Store) Record(ctx context.Context, rec interfaces.{Record}) error {
	_, err := s.pool.Exec(ctx,
		`INSERT INTO {table} (input, checked_at) VALUES ($1, $2)`,
		rec.Input, rec.CheckedAt)
	return err
}

func (s *Store) Recent(ctx context.Context, limit int) ([]interfaces.{Record}, error) {
	rows, err := s.pool.Query(ctx,
		`SELECT input, checked_at FROM {table} ORDER BY checked_at DESC LIMIT $1`, limit)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var out []interfaces.{Record}
	for rows.Next() {
		var rec interfaces.{Record}
		if err := rows.Scan(&rec.Input, &rec.CheckedAt); err != nil {
			return nil, err
		}
		out = append(out, rec)
	}
	return out, rows.Err()
}
```

This catalog's own runnable examples concretize this as `linkstore.Store` implementing `interfaces.LinkHistory` against a `link_checks` table — see `plateau-persistent-service`'s `example/`.

# Rule changes

## MUST
- `New` must fail fast (return an error) if the schema cannot be ensured — never return a `*Store` that might fail on its first real query instead.
  - Risk: deferring the schema check to the first `Record`/`Recent` call turns a startup-time configuration problem into a request-time failure, discovered later and in production traffic instead of at deploy time.
  - Fix: run `CREATE TABLE IF NOT EXISTS` inside `New`, before returning the `*Store`.
- Every query must use parameter placeholders (`$1`, `$2`, ...) — never string-concatenate a value into SQL.
  - Violation: `fmt.Sprintf("... WHERE input = '%s'", input)`.
  - Risk: string-concatenated SQL is a SQL-injection vulnerability the moment `input` can contain attacker-influenced content.
  - Fix: always pass values as `pool.Exec`/`pool.Query` arguments, never interpolated into the query string.

# Check list
- [ ] `New` returns a non-nil error if the table cannot be created.
- [ ] No query in this file builds its SQL string via `fmt.Sprintf`/string concatenation with a caller-supplied value.

# Unittest TestCases
- [ ] WHEN `Record` then `Recent(limit=1)` is called THEN the just-recorded entry is returned
- [ ] WHEN `Recent` is called with `limit` smaller than the number of stored rows THEN exactly `limit` rows are returned, most recent first
