package main

import (
	"context"
	"errors"
	"log/slog"
	"net/http"
	"os"
	"os/signal"
	"syscall"

	apihttp "github.com/example/linkcheck-service/internal/api/http"
	"github.com/example/linkcheck-service/internal/config"
	"github.com/example/linkcheck-service/internal/domain/services"
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
