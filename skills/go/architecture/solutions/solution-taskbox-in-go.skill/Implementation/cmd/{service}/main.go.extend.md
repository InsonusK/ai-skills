---
description: One pgx pool for data and tasks, the task handlers registered, and the TaskBox worker run beside the servers
project_name: cmd/{service}
name: main
element_kind: functions
change_kind: extend
tags:
  - solution/taskbox-in-go
  - element/cmd-service-main-go
---

# Goals
- Wire TaskBox in the composition root: one pool shared by the data store and the TaskBox store, handlers registered, worker running in the errgroup.

# Implementation changes

### AS IS
After `solution-persistent-db` and `solution-go-db-migrations`:
```go
	if cfg.MigrateOnStart {
		if err := {store}.Migrate(ctx, cfg.DatabaseDSN); err != nil {
			return fmt.Errorf("migrate: %w", err)
		}
	}

	store, err := {store}.New(ctx, cfg.DatabaseDSN)
	if err != nil {
		return err
	}
	defer store.Close()

	domainService := services.New{Service}(..., store)
```

### TO BE
```go
	if cfg.MigrateOnStart {
		if err := {store}.Migrate(ctx, cfg.DatabaseDSN); err != nil {
			return fmt.Errorf("migrate: %w", err)
		}
	}

	pool, err := pgxpool.New(ctx, cfg.DatabaseDSN)
	if err != nil {
		return fmt.Errorf("connect: %w", err)
	}
	defer pool.Close()

	tasks := pgstore.New(pool)
	store := {store}.New(pool, tasks)

	domainService := services.New{Service}(..., store, cfg.{FollowUpDelay})

	registry := taskbox.NewRegistry()
	apitasks.New(domainService).Register(registry)
	worker := taskbox.NewWorker(tasks, registry, taskbox.Config{
		Workers:      cfg.TaskWorkers,
		Lease:        cfg.TaskLease,
		PollInterval: cfg.TaskPollInterval,
	})

	// ... servers constructed as before ...

	g.Go(func() error {
		slog.Info("taskbox worker running", "workers", cfg.TaskWorkers)
		return worker.Run(ctx)
	})
```
`worker.Run` returns after `ctx` ends and the runs in flight finish, so the errgroup's shutdown waits for them.

# Rule changes

## MUST

### Share one pool between data and tasks
Build one `pgxpool.Pool` in `main.go` and give it to both the data store and `pgstore`.
- Risk: two pools on one database work, but the data store can no longer be handed a transaction that TaskBox also writes into without an extra seam.
- Fix: the pool is owned and closed by `main.go`.

# Check list
- [ ] The worker runs in the same errgroup as the servers and stops with them.
- [ ] Every task type the domain declares is registered before `worker.Run`.
