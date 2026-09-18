---
name: plateau-dual-api-service--file-cmd-service-main
description: cmd/{service}/main.go of the plateau-dual-api-service plateau
whenToUse: when creating or editing cmd/{service}/main.go, or creating another composition root that plays the same role
domain: skill
type: template
plateau: plateau-dual-api-service
version: 20260917010000
tags:
  - skill/template/file
  - plateau/plateau-dual-api-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]]"
  - "[[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]]"
  - "[[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
  - "[[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
registry:
  - "[[skills/go/architecture/registry/cmd-service-main-go.md|cmd-service-main-go]]"
---

# Goal
The composition root: load config, initialize logging, construct the domain service, and start both the gRPC and HTTP servers concurrently via an `errgroup.Group`, stopping both cleanly on `SIGINT`/`SIGTERM`. See [[skills/go/architecture/registry/cmd-service-main-go.md|the registry entry]] for how the four contributing solutions' deltas combine.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/cmd/{service}/main.go.create.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]

# Core Principles
- Apply ONE plateau template per file.
- `run()` returns `error`; `main()` is the only place that logs a fatal error and sets the exit code.
- Every construction happens once, here; the same domain-service instance is handed to every adapter.
- Two or more concurrent long-running servers are run via `errgroup.Group`, never sequential blocking calls.

# Implementation
```go
// Skill: file-cmd-service-main
// Plateau: plateau-dual-api-service
// Version: 20260917010000

package main

import (
	"context"
	"errors"
	"log/slog"
	"net"
	"net/http"
	"os"
	"os/signal"
	"syscall"

	grpclib "google.golang.org/grpc"

	"golang.org/x/sync/errgroup"

	apiv1 "{module-path}/gen/api"
	apigrpc "{module-path}/internal/api/grpc"
	apihttp "{module-path}/internal/api/http"
	"{module-path}/internal/config"
	"{module-path}/internal/domain/services"
	"{module-path}/internal/logging"
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

	domainService := services.NewLinkCheckService()

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
		<-ctx.Done()
		grpcServer.GracefulStop()
		_ = httpServer.Shutdown(context.Background())
		return nil
	})

	return g.Wait()
}
```
Verified against this plateau's own `example/cmd/linkcheck/main.go` — `go build`/`go vet` clean; HTTP and gRPC both smoke-tested (`curl`, `grpcurl` against a running process).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/cmd/{service}/main.go.create.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `logging.Init(cfg.LogLevel)` runs immediately after the config-load error check, before any adapter constructor.
- `run()` constructs `domainService` exactly once; every adapter is handed the same pointer.
- The shutdown goroutine calls `grpcServer.GracefulStop()` and `httpServer.Shutdown(...)` together, in the same `g.Go` closure.
- Every serve-loop closure returns `nil` on its own expected-shutdown error (`http.ErrServerClosed`), never treats it as a real failure.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/cmd/{service}/main.go.create.md#MUST|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/cmd/{service}/main.go.extend.md#MUST|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/cmd/{service}/main.go.extend.md#MUST|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/cmd/{service}/main.go.extend.md#MUST|cmd/{service}/main.go]]

# Check list
- [ ] `SIGINT`/`SIGTERM` triggers both `grpcServer.GracefulStop()` and `httpServer.Shutdown`, not an abrupt process exit.
- [ ] `logging.Init` runs before the first adapter constructor.
- [ ] `run()` uses exactly one `errgroup.Group`; no serve loop runs outside it.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
