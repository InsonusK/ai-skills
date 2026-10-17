// The composition root: the one place that builds concrete types and wires
// them together. A variation point joins the application by adding lines to
// the marked sections below - never by editing another package.
package main

import (
	"context"
	"io"
	"log/slog"
	"os"
	"os/signal"
	"syscall"

	httpapi "github.com/example/todo/internal/api/http"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/config"
	"github.com/example/todo/internal/version"
)

func main() {
	// ctx is cancelled on SIGINT or SIGTERM, so a long-running entry point
	// (an HTTP server, a listener) stops gracefully.
	ctx, stop := signal.NotifyContext(context.Background(), syscall.SIGINT, syscall.SIGTERM)
	code := run(ctx, os.Stdout, os.Stderr)
	stop()
	os.Exit(code)
}

func run(ctx context.Context, stdout, stderr io.Writer) int {
	cfg, err := config.Load()
	if err != nil {
		slog.New(slog.NewJSONHandler(stderr, nil)).Error("startup failed", "error", err)
		return 1
	}
	logger := slog.New(slog.NewJSONHandler(stderr, &slog.HandlerOptions{Level: cfg.LogLevel}))
	logger.Info("starting", "version", version.Version)

	// [services] - infrastructure and the services over it; none in the base plateau.

	// [handlers] - one per command, wrapped by its decorators.
	var planItems command.Handler[command.PlanItems, command.PlanItemsResult] = command.NewPlanItemsHandler()
	planItems = command.WithLogging("PlanItems", logger, planItems)

	// [entries] - every entry point.
	httpConfig, err := httpapi.LoadConfig()
	if err != nil {
		logger.Error("startup failed", "error", err)
		return 1
	}
	server := httpapi.NewServer(httpConfig, version.Version, httpapi.Handlers{PlanItems: planItems}, logger)
	if err := server.Serve(ctx); err != nil {
		logger.Error("http server failed", "error", err)
		return 1
	}
	return 0
}
