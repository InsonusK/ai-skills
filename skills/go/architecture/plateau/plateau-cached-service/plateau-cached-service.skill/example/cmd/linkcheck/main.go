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

	apiv1 "github.com/example/linkcheck-service/gen/api"
	apigrpc "github.com/example/linkcheck-service/internal/api/grpc"
	apihttp "github.com/example/linkcheck-service/internal/api/http"
	"github.com/example/linkcheck-service/internal/config"
	"github.com/example/linkcheck-service/internal/domain/services"
	"github.com/example/linkcheck-service/internal/infrastructure/reputationcache"
	"github.com/example/linkcheck-service/internal/infrastructure/reputationclient"
	"github.com/example/linkcheck-service/internal/logging"
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

	domainService := services.NewLinkCheckService(reputation, cache)

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
