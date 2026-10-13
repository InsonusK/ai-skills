// Package linkstore durably stores link-check records in PostgreSQL, and
// owns the service's schema history (migrations/).
package linkstore

import (
	"context"
	"encoding/json"
	"fmt"

	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgxpool"

	"github.com/example/linkcheck-service/internal/domain/interfaces"
	"github.com/example/linkcheck-service/internal/taskbox"
	"github.com/example/linkcheck-service/internal/taskbox/pgstore"
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
