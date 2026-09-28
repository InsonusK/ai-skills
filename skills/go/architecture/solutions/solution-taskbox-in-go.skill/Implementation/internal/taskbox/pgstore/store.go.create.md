---
description: PostgreSQL store: enqueue in the caller's pgx.Tx with the group lock, the contract claim, fenced outcomes, requeue/cancel, cleanup
project_name: internal/taskbox/pgstore
name: store
element_kind: struct
change_kind: create
verbatim_of: internal/taskbox/pgstore/store.go
tags:
  - solution/taskbox-in-go
  - element/internal-taskbox-pgstore-store-go
---

# Goals
- Realize contract §6 PostgreSQL schema v1 with `pgx/v5`.

# Core Principles
- Every SQL statement that the contract spells out (group lock, claim) is used as written; outcome writes add the `attempt` fence.
- The store never creates tables: the schema comes from the service's own migrations.

# Implementation changes
Create `internal/taskbox/pgstore/store.go` exactly as below (`{module-path}` = the service's Go module path, `{store}` = the package owning the service's migrations). The code is proven by the conformance feature on PostgreSQL in this catalog's plateau built with this solution.

```go
// Package pgstore is the PostgreSQL realization of the TaskBox contract
// (schema v1): enqueue inside the caller's pgx.Tx, claim with
// FOR UPDATE SKIP LOCKED, group order by locking taskbox_group before insert.
// The tables come from the service's own migrations, never from this package.
package pgstore

import (
	"context"
	"errors"
	"fmt"
	"time"

	"github.com/google/uuid"
	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgxpool"

	"{module-path}/internal/taskbox"
)

// Store implements taskbox.Store over a pgxpool.Pool.
type Store struct {
	pool *pgxpool.Pool
}

var _ taskbox.Store = (*Store)(nil)

func New(pool *pgxpool.Pool) *Store {
	return &Store{pool: pool}
}

// Enqueue adds t inside tx and never commits: the task becomes visible with
// the caller's own commit, or never. It reports false when t's idempotency
// key is already present (nothing added).
func (s *Store) Enqueue(ctx context.Context, tx pgx.Tx, t taskbox.NewTask) (bool, error) {
	t, err := t.Normalize()
	if err != nil {
		return false, err
	}
	if t.Group != "" {
		// The group lock first, held until the caller commits (contract §3).
		if _, err := tx.Exec(ctx, `
			INSERT INTO taskbox_group (queue, queue_group) VALUES ($1, $2)
			ON CONFLICT (queue, queue_group) DO UPDATE SET queue_group = EXCLUDED.queue_group`,
			t.Queue, t.Group); err != nil {
			return false, fmt.Errorf("lock group: %w", err)
		}
	}
	tag, err := tx.Exec(ctx, `
		INSERT INTO taskbox_task (id, status_key, queue, queue_group, type, payload,
		                          idempotency_key, max_attempts, run_at, retention)
		VALUES ($1, $2, $3, $4, $5, $6, $7, $8, COALESCE($9, now()), $10::interval)
		ON CONFLICT (idempotency_key) DO NOTHING`,
		t.ID, nullUUID(t.StatusKey), t.Queue, nullString(t.Group), t.Type, []byte(t.Payload),
		nullString(t.IdempotencyKey), t.MaxAttempts, nullTime(t.RunAt), nullInterval(t.Retention))
	if err != nil {
		return false, fmt.Errorf("insert task: %w", err)
	}
	return tag.RowsAffected() == 1, nil
}

// Claim is the contract's claim statement.
func (s *Store) Claim(ctx context.Context, queue string, lease time.Duration, batch int) ([]taskbox.Claimed, error) {
	rows, err := s.pool.Query(ctx, `
		UPDATE taskbox_task SET status = 'running', attempt = attempt + 1,
		       locked_until = now() + $2::interval, updated_at = now()
		WHERE seq IN (
		  SELECT t.seq FROM taskbox_task t
		  WHERE t.queue = $1 AND t.run_at <= now()
		    AND (t.status = 'pending' OR (t.status = 'running' AND t.locked_until < now()))
		    AND (t.queue_group IS NULL OR NOT EXISTS (
		      SELECT 1 FROM taskbox_task e
		      WHERE e.queue = t.queue AND e.queue_group = t.queue_group AND e.seq < t.seq
		        AND e.status IN ('pending','running','dead')))
		  ORDER BY t.run_at LIMIT $3
		  FOR UPDATE SKIP LOCKED)
		RETURNING seq, id, type, payload, attempt`,
		queue, interval(lease), batch)
	if err != nil {
		return nil, err
	}
	return pgx.CollectRows(rows, func(r pgx.CollectableRow) (taskbox.Claimed, error) {
		var c taskbox.Claimed
		var payload []byte
		err := r.Scan(&c.Seq, &c.ID, &c.Type, &payload, &c.Attempt)
		c.Payload = payload
		return c, err
	})
}

// Finish records one run's outcome, fenced by (status running, claimed attempt).
func (s *Store) Finish(ctx context.Context, c taskbox.Claimed, r taskbox.Result) error {
	var err error
	switch {
	case taskbox.Success(r.Status):
		_, err = s.pool.Exec(ctx, `
			UPDATE taskbox_task SET status = 'done', last_status = $3, locked_until = NULL,
			       finished_at = now(), updated_at = now()
			WHERE seq = $1 AND status = 'running' AND attempt = $2`,
			c.Seq, c.Attempt, r.Status)
	case r.Retryable:
		// Retry while attempts remain; the last retryable failure is dead.
		_, err = s.pool.Exec(ctx, `
			UPDATE taskbox_task SET
			       status      = CASE WHEN attempt >= max_attempts THEN 'dead' ELSE 'pending' END,
			       run_at      = CASE WHEN attempt >= max_attempts THEN run_at ELSE now() + $5::interval END,
			       finished_at = CASE WHEN attempt >= max_attempts THEN now() END,
			       last_status = $3, last_error = $4, locked_until = NULL, updated_at = now()
			WHERE seq = $1 AND status = 'running' AND attempt = $2`,
			c.Seq, c.Attempt, r.Status, r.Error, interval(r.RetryDelay))
	default:
		_, err = s.pool.Exec(ctx, `
			UPDATE taskbox_task SET status = 'dead', last_status = $3, last_error = $4,
			       locked_until = NULL, finished_at = now(), updated_at = now()
			WHERE seq = $1 AND status = 'running' AND attempt = $2`,
			c.Seq, c.Attempt, r.Status, r.Error)
	}
	return err
}

// Requeue sends a dead task back to pending with attempt 0.
func (s *Store) Requeue(ctx context.Context, id uuid.UUID) error {
	return s.fromDead(ctx, id, `
		UPDATE taskbox_task SET status = 'pending', attempt = 0, run_at = now(),
		       finished_at = NULL, updated_at = now()
		WHERE id = $1 AND status = 'dead'`)
}

// Cancel ends a dead task as cancelled; its group resumes.
func (s *Store) Cancel(ctx context.Context, id uuid.UUID) error {
	return s.fromDead(ctx, id, `
		UPDATE taskbox_task SET status = 'cancelled', finished_at = now(), updated_at = now()
		WHERE id = $1 AND status = 'dead'`)
}

func (s *Store) fromDead(ctx context.Context, id uuid.UUID, sql string) error {
	tag, err := s.pool.Exec(ctx, sql, id)
	if err != nil {
		return err
	}
	if tag.RowsAffected() == 0 {
		if _, err := s.Get(ctx, id); err != nil {
			return err
		}
		return taskbox.ErrNotDead
	}
	return nil
}

// Cleanup deletes done/cancelled tasks past max(default, retention) and the
// group rows left without tasks (contract §5); dead tasks stay.
func (s *Store) Cleanup(ctx context.Context, defaultRetention time.Duration) error {
	if _, err := s.pool.Exec(ctx, `
		DELETE FROM taskbox_task
		WHERE status IN ('done','cancelled')
		  AND finished_at < now() - GREATEST($1::interval, retention)`,
		interval(defaultRetention)); err != nil {
		return fmt.Errorf("delete finished tasks: %w", err)
	}
	if _, err := s.pool.Exec(ctx, `
		DELETE FROM taskbox_group g
		WHERE NOT EXISTS (SELECT 1 FROM taskbox_task t
		                  WHERE t.queue = g.queue AND t.queue_group = g.queue_group)`); err != nil {
		return fmt.Errorf("delete empty groups: %w", err)
	}
	return nil
}

// Get reads one task by id.
func (s *Store) Get(ctx context.Context, id uuid.UUID) (taskbox.Task, error) {
	var t taskbox.Task
	var statusKey *uuid.UUID
	var group, idem, lastErr *string
	var lastStatus *int
	var lockedUntil, finishedAt *time.Time
	var retention *time.Duration
	var payload []byte
	err := s.pool.QueryRow(ctx, `
		SELECT seq, id, status_key, queue, queue_group, type, payload, idempotency_key, status,
		       attempt, max_attempts, run_at, locked_until, last_status, last_error,
		       (extract(epoch FROM retention) * 1e9)::bigint, created_at, updated_at, finished_at
		FROM taskbox_task WHERE id = $1`, id).Scan(
		&t.Seq, &t.ID, &statusKey, &t.Queue, &group, &t.Type, &payload, &idem, &t.Status,
		&t.Attempt, &t.MaxAttempts, &t.RunAt, &lockedUntil, &lastStatus, &lastErr,
		&retention, &t.CreatedAt, &t.UpdatedAt, &finishedAt)
	if errors.Is(err, pgx.ErrNoRows) {
		return taskbox.Task{}, taskbox.ErrNotFound
	}
	if err != nil {
		return taskbox.Task{}, err
	}
	t.Payload = payload
	t.StatusKey = deref(statusKey)
	t.Group, t.IdempotencyKey, t.LastError = deref(group), deref(idem), deref(lastErr)
	t.LastStatus = deref(lastStatus)
	t.LockedUntil, t.FinishedAt = deref(lockedUntil), deref(finishedAt)
	t.Retention = deref(retention)
	return t, nil
}

func deref[T any](p *T) T {
	var zero T
	if p == nil {
		return zero
	}
	return *p
}

// interval renders d as a PostgreSQL interval literal.
func interval(d time.Duration) string {
	return fmt.Sprintf("%d microseconds", d.Microseconds())
}

func nullInterval(d time.Duration) *string {
	if d <= 0 {
		return nil
	}
	s := interval(d)
	return &s
}

func nullString(s string) *string {
	if s == "" {
		return nil
	}
	return &s
}

func nullTime(t time.Time) *time.Time {
	if t.IsZero() {
		return nil
	}
	return &t
}

func nullUUID(id uuid.UUID) *uuid.UUID {
	if id == uuid.Nil {
		return nil
	}
	return &id
}
```

# Rule changes

## MUST

### Lock the group before inserting
Run the `taskbox_group` upsert before the task insert, in the caller's transaction, for every grouped task.
- Violation: inserting the task first, or locking in a separate transaction.
- Risk: two concurrent transactions commit out of `seq` order and a later task of the group runs first — the conformance scenario "Concurrent enqueues into one group run in commit order" fails without the lock.
- Fix: keep the upsert as the first statement of `Enqueue` for a grouped task.

### Never commit inside Enqueue
Take the caller's `pgx.Tx` and never commit or roll it back in `Enqueue`.
- Risk: a task committed on its own is no longer atomic with the data change it follows.
- Fix: the caller owns `Begin`/`Commit`; `Enqueue` only executes statements on the given `tx`.

### Fence every outcome write
Condition every `Finish` update on `status = 'running' AND attempt = <claimed attempt>`.
- Violation: fencing on `status` alone.
- Risk: a run whose lease expired writes its late outcome over the re-claimed run's — the lease scenario turns a `done` task into `dead`.
- Fix: keep both conditions in all three `Finish` statements.

### Keep the contract's claim statement
Keep the claim `UPDATE … WHERE seq IN (SELECT … FOR UPDATE SKIP LOCKED)` as the contract states it.
- Risk: a reworded claim silently loses head-of-group or no-double-claim guarantees that every stack relies on.
- Fix: change the claim only by changing the contract first.
