---
description: Convert run() to an errgroup.Group running the gRPC server alongside the HTTP server
project_name: "cmd/{service}"
name: main
element_kind: functions
change_kind: extend
tags:
  - solution/grpc-api
  - element/cmd-service-main-go
---

# Implementation changes
```go
import (
	// ...
	"net"

	grpclib "google.golang.org/grpc"
	"golang.org/x/sync/errgroup"

	apigrpc "{module-path}/internal/api/grpc"
)

func run() error {
	cfg, err := config.Load()
	if err != nil {
		return err
	}

	logging.Init(cfg.LogLevel)

	ctx, stop := signal.NotifyContext(context.Background(), syscall.SIGINT, syscall.SIGTERM)
	defer stop()

	domainService := services.New{Service}()

	grpcServer := grpclib.NewServer()
	apiv1.Register{Service}Server(grpcServer, apigrpc.New(domainService))

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

# Rule changes

## MUST
- The shutdown goroutine must call `grpcServer.GracefulStop()` and `httpServer.Shutdown(...)` together, in the same `g.Go` closure — never let one server's shutdown path be independent of the other's.
  - Risk: two separate shutdown mechanisms (e.g. one via `errgroup`, one via a bare `go func` outside it) can race, or leave one server still accepting connections after the process has decided to exit.
  - Fix: one shutdown goroutine, registered via `g.Go`, waits on `ctx.Done()` and stops both servers before returning.
- Every `g.Go` closure for a serve loop must return `nil` on its own expected-shutdown error (`http.ErrServerClosed`) — an unexpected error is what should actually cancel the group.
  - Risk: treating `http.ErrServerClosed` as a real failure makes an ordinary, requested shutdown look like a crash and needlessly cancels the gRPC server's own context.
  - Fix: `errors.Is(err, http.ErrServerClosed)` before returning it.

# Check list
- [ ] `run()` uses exactly one `errgroup.Group`; no serve loop is started outside it.
- [ ] `g.Wait()` is `run()`'s only blocking return point.
