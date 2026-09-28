---
description: The PostgreSQL data adapter writes the data change and enqueues its tasks in one pgx transaction
project_name: internal/infrastructure/{store}
name: Store
element_kind: struct
change_kind: extend
tags:
  - solution/taskbox-in-go
  - element/internal-infrastructure-store-store-go
---

# Goals
- Make a data write and its follow-up tasks atomic: both commit, or neither does.

# Core Principles
- The adapter shares the TaskBox store's database and pool; `main.go` builds one `pgxpool.Pool` and passes it to both.
- Applied on top of `solution-persistent-db`'s store and `solution-go-db-migrations`' change to `New` (schema from migrations, not inline DDL).

# Implementation changes

### AS IS
```go
type Store struct {
	pool *pgxpool.Pool
}

func New(ctx context.Context, dsn string) (*Store, error) {
	pool, err := pgxpool.New(ctx, dsn)
	if err != nil {
		return nil, fmt.Errorf("connect: %w", err)
	}
	return &Store{pool: pool}, nil
}

func (s *Store) {Write}(ctx context.Context, entry interfaces.{Entry}) error {
	_, err := s.pool.Exec(ctx, `INSERT INTO {table} (...) VALUES (...)`, ...)
	return err
}
```

### TO BE
```go
type Store struct {
	pool  *pgxpool.Pool
	tasks *pgstore.Store
}

// New uses a pool whose schema Migrate has already brought up to date; tasks
// is the TaskBox store on the same database, so a write and its tasks share
// one transaction.
func New(pool *pgxpool.Pool, tasks *pgstore.Store) *Store {
	return &Store{pool: pool, tasks: tasks}
}

// {Write} inserts entry and enqueues tasks in one transaction.
func (s *Store) {Write}(ctx context.Context, entry interfaces.{Entry}, tasks ...interfaces.Task) error {
	return pgx.BeginFunc(ctx, s.pool, func(tx pgx.Tx) error {
		if _, err := tx.Exec(ctx, `INSERT INTO {table} (...) VALUES (...)`, ...); err != nil {
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
```
`Close` moves to `main.go` (the pool's owner); read methods are unchanged.

# Rule changes

## MUST

### Write data and tasks in one transaction
Execute the data insert and every `Enqueue` on the same `pgx.Tx`, committed by `pgx.BeginFunc`.
- Violation: `s.pool.Exec(...)` for the data, then `Enqueue` in a second transaction.
- Risk: a crash between the two leaves a change without its follow-up.
- Fix: one `BeginFunc`, every statement on its `tx`.

# Check list
- [ ] The data insert and the enqueues run on one `tx`; an enqueue error rolls back the data insert.
