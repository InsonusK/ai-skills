package test

import (
	"context"
	"fmt"
	"time"

	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgxpool"

	"github.com/example/linkcheck-service/internal/infrastructure/linkstore"
	"github.com/example/linkcheck-service/internal/taskbox"
	"github.com/example/linkcheck-service/internal/taskbox/pgstore"
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
