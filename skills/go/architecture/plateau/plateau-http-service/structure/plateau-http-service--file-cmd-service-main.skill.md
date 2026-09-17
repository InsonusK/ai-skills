---
name: plateau-http-service--file-cmd-service-main
description: cmd/{service}/main.go of the plateau-http-service plateau
whenToUse: when creating or editing cmd/{service}/main.go, or creating another composition root that plays the same role
domain: skill
type: template
plateau: plateau-http-service
version: 20260917000000
tags:
  - skill/template/file
  - plateau/plateau-http-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]]"
  - "[[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]]"
  - "[[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
registry:
  - "[[../registry/cmd-service-main-go.md|cmd-service-main-go]]"
---

# Goal
The composition root: load config, initialize logging, construct the domain service and the HTTP server, and stop cleanly on `SIGINT`/`SIGTERM`. See [[../registry/cmd-service-main-go.md|the registry entry]] for how the three contributing solutions' deltas combine.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/cmd/{service}/main.go.create.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]

# Core Principles
- Apply ONE plateau template per file.
- `run()` returns `error`; `main()` is the only place that logs a fatal error and sets the exit code.
- Every construction happens once, here; the same instance is handed to every adapter that needs it.

# Implementation
```go
// Skill: file-cmd-service-main
// Plateau: plateau-http-service
// Version: 20260917000000

package main

import (
	"context"
	"errors"
	"log/slog"
	"net/http"
	"os"
	"os/signal"
	"syscall"

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

	httpServer := &http.Server{
		Addr:    ":" + cfg.HTTPListenPort,
		Handler: apihttp.New(domainService).Handler(),
	}

	go func() {
		<-ctx.Done()
		_ = httpServer.Shutdown(context.Background())
	}()

	slog.Info("http server listening", "addr", httpServer.Addr)
	if err := httpServer.ListenAndServe(); err != nil && !errors.Is(err, http.ErrServerClosed) {
		return err
	}
	return nil
}
```
Verified against this plateau's own `example/cmd/linkcheck/main.go` (`go build`/`go vet` clean, HTTP smoke-tested).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/cmd/{service}/main.go.create.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `logging.Init(cfg.LogLevel)` runs immediately after the config-load error check, before any adapter constructor.
- `run()` constructs `domainService` exactly once; every adapter is handed the same pointer.
- A single-server `run()` blocks on `httpServer.ListenAndServe()` directly; convert to an `errgroup.Group` the moment a second concurrent server is added (see [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]], applied starting at `plateau-dual-api-service`).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/cmd/{service}/main.go.create.md#MUST|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/cmd/{service}/main.go.extend.md#MUST|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/cmd/{service}/main.go.extend.md#MUST|cmd/{service}/main.go]]

# Check list
- [ ] `SIGINT`/`SIGTERM` triggers `httpServer.Shutdown`, not an abrupt process exit.
- [ ] `logging.Init` runs before the first adapter constructor.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]]
