// Package linkstore durably stores link-check records in PostgreSQL.
package linkstore

import (
	"context"
	"fmt"

	"github.com/jackc/pgx/v5/pgxpool"

	"github.com/example/linkcheck-service/internal/domain/interfaces"
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
