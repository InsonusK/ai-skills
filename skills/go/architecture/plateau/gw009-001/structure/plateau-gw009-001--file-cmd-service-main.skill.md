---
name: plateau-gw009-001--file-cmd-service-main
description: cmd/{service}/main.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when editing the composition root: wiring adapters, the TaskBox worker, the migration call, or the servers
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]]"
  - "[[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]]"
  - "[[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
  - "[[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
  - "[[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
  - "[[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]]"
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
  - "[[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]]"
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
source: cmd/{service}/main.go
---

# Goal
Compose the service: config, logging, reputation client, cache, one PostgreSQL pool shared by the history store and TaskBox, the task handlers, and the HTTP/gRPC servers and TaskBox worker in one errgroup.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/cmd/{service}/main.go.create.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Core Principles
- Apply ONE plateau template per file.
- `run()` returns `error`; `main()` is the only place that logs a fatal error and sets the exit code.
- Every construction happens once, here; the same domain-service instance is handed to every adapter.
- Two or more concurrent long-running servers are run via `errgroup.Group`, never sequential blocking calls.
- One `pgxpool.Pool` is built here and shared by `linkstore` and `pgstore` — a history write and its task share one transaction.
- The TaskBox worker runs in the same errgroup as the servers; on shutdown it stops claiming and finishes the runs in flight.
- `linkstore.Migrate` runs here only when `MIGRATE_ON_START=true`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/cmd/{service}/main.go.create.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Implementation
```go
// Skill: file-cmd-service-main
// Plateau: plateau-gw009-001
// Version: 20260928120000

package main

import (
	"context"
	"errors"
	"fmt"
	"log/slog"
	"net"
	"net/http"
	"os"
	"os/signal"
	"syscall"

	"github.com/jackc/pgx/v5/pgxpool"
	grpclib "google.golang.org/grpc"

	"golang.org/x/sync/errgroup"

	apiv1 "{module-path}/gen/api"
	apigrpc "{module-path}/internal/api/grpc"
	apihttp "{module-path}/internal/api/http"
	apitasks "{module-path}/internal/api/tasks"
	"{module-path}/internal/config"
	"{module-path}/internal/domain/services"
	"{module-path}/internal/infrastructure/linkstore"
	"{module-path}/internal/infrastructure/reputationcache"
	"{module-path}/internal/infrastructure/reputationclient"
	"{module-path}/internal/logging"
	"{module-path}/internal/taskbox"
	"{module-path}/internal/taskbox/pgstore"
)

func main() {
	if err := run(); err != nil {
		slog.Error("fatal", "error", err)
		os.Exit(1)
	}
}

func run() error {
	cfg, err := config.Load()
	if err != nil {
		return err
	}

	logging.Init(cfg.LogLevel)

	ctx, stop := signal.NotifyContext(context.Background(), syscall.SIGINT, syscall.SIGTERM)
	defer stop()

	reputation, err := reputationclient.Dial(cfg.ReputationAddr)
	if err != nil {
		return err
	}
	defer func() { _ = reputation.Close() }()

	cacheAddr := net.JoinHostPort(cfg.RedisHost, cfg.RedisPort)
	cache := reputationcache.New(cacheAddr, cfg.RedisPassword, cfg.RedisDB)
	defer func() { _ = cache.Close() }()

	if cfg.MigrateOnStart {
		if err := linkstore.Migrate(ctx, cfg.DatabaseDSN); err != nil {
			return fmt.Errorf("migrate: %w", err)
		}
	}

	pool, err := pgxpool.New(ctx, cfg.DatabaseDSN)
	if err != nil {
		return fmt.Errorf("connect: %w", err)
	}
	defer pool.Close()

	tasks := pgstore.New(pool)
	store := linkstore.New(pool, tasks)

	domainService := services.NewLinkCheckService(reputation, cache, store, cfg.RecheckAfter)

	registry := taskbox.NewRegistry()
	apitasks.New(domainService).Register(registry)
	worker := taskbox.NewWorker(tasks, registry, taskbox.Config{
		Workers:      cfg.TaskWorkers,
		Lease:        cfg.TaskLease,
		PollInterval: cfg.TaskPollInterval,
	})

	grpcServer := grpclib.NewServer()
	apiv1.RegisterLinkCheckServiceServer(grpcServer, apigrpc.New(domainService))

	httpServer := &http.Server{
		Addr:    ":" + cfg.HTTPListenPort,
		Handler: apihttp.New(domainService).Handler(),
	}

	g, ctx := errgroup.WithContext(ctx)

	g.Go(func() error {
		lis, err := net.Listen("tcp", ":"+cfg.GRPCListenPort)
		if err != nil {
			return err
		}
		slog.Info("grpc server listening", "addr", lis.Addr().String())
		return grpcServer.Serve(lis)
	})

	g.Go(func() error {
		slog.Info("http server listening", "addr", httpServer.Addr)
		if err := httpServer.ListenAndServe(); err != nil && !errors.Is(err, http.ErrServerClosed) {
			return err
		}
		return nil
	})

	g.Go(func() error {
		slog.Info("taskbox worker running", "workers", cfg.TaskWorkers)
		return worker.Run(ctx)
	})

	g.Go(func() error {
		<-ctx.Done()
		grpcServer.GracefulStop()
		_ = httpServer.Shutdown(context.Background())
		return nil
	})

	return g.Wait()
}
```

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/cmd/{service}/main.go.create.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `logging.Init(cfg.LogLevel)` runs immediately after the config-load error check, before any adapter constructor.
- `run()` constructs `domainService` exactly once; every adapter is handed the same pointer.
- The shutdown goroutine calls `grpcServer.GracefulStop()` and `httpServer.Shutdown(...)` together, in the same `g.Go` closure.
- Every serve-loop closure returns `nil` on its own expected-shutdown error (`http.ErrServerClosed`), never treats it as a real failure.
- `reputationclient.Dial`, `reputationcache.New`, and `linkstore.New` are all called, and all three results passed into `NewLinkCheckService`, before either server is constructed; `Close()` is deferred immediately after each construction.
- Register every task type's handler before `worker.Run`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/cmd/{service}/main.go.create.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Check list
- [ ] `SIGINT`/`SIGTERM` triggers both `grpcServer.GracefulStop()` and `httpServer.Shutdown`, not an abrupt process exit.
- [ ] `logging.Init` runs before the first adapter constructor.
- [ ] `run()` uses exactly one `errgroup.Group`; no serve loop runs outside it.
- [ ] `reputationclient.Dial`, `reputationcache.New`, and `linkstore.New` are all called before `services.NewLinkCheckService`, and their results passed directly into that call.
- [ ] A flagged check followed by `RECHECK_AFTER` produces a re-check recorded in history (smoke-tested).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/cmd/{service}/main.go.create.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]
