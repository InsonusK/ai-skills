---
description: Wire the domain service and the HTTP server into the composition root
project_name: "cmd/{service}"
name: main
element_kind: functions
change_kind: extend
tags:
  - solution/go-http-api
  - element/cmd-service-main-go
---

# Goals
- Start serving HTTP once the domain service is constructed, and stop cleanly on `SIGINT`/`SIGTERM`.

# Implementation changes
```go
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

	ctx, stop := signal.NotifyContext(context.Background(), syscall.SIGINT, syscall.SIGTERM)
	defer stop()

	domainService := services.New{Service}()

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

# Rule changes

## MUST
- `run()` must construct `domainService` exactly once and pass the same pointer to every adapter that needs it.
  - Risk: a second construction elsewhere in `run()` silently gives two adapters two different instances.
  - Fix: keep exactly one `services.New{Service}(...)` call in `run()`; every adapter constructor takes that same variable.
- A single-server `run()` (this solution's shape) blocks on `httpServer.ListenAndServe()` directly; the moment a second concurrent long-running loop is added (e.g. by [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]), `run()` must be converted to an `errgroup.Group` — never two bare blocking calls in sequence.
  - Risk: two sequential blocking calls means the second server never starts.
  - Fix: the solution introducing the second loop makes this conversion; see its own `main.go.extend.md`.

# Check list
- [ ] `run()` blocks on `httpServer.ListenAndServe()` and returns cleanly on shutdown.
- [ ] `SIGINT`/`SIGTERM` triggers `httpServer.Shutdown`, not an abrupt process exit.
