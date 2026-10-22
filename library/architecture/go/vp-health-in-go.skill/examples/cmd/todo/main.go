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

	"github.com/example/todo/internal/api/core/entry"
	"github.com/example/todo/internal/api/management"
	"github.com/example/todo/internal/api/startup"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
	"github.com/example/todo/internal/config"
	"github.com/example/todo/internal/logging"
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
	// The handler decides what reaches the log: the base plateau's leaves out
	// every payload; the logging variation point replaces this line.
	logger := slog.New(logging.NewHandler(stderr, cfg.LogLevel))
	logger.Info("starting", "version", version.Version)

	// [services] - infrastructure and the services over it; none in the base plateau.

	// [handlers] - one per command, wrapped by its decorators.
	var planItems pipeline.Handler[command.PlanItems, command.PlanItemsResult] = command.NewPlanItemsHandler()
	planItems = pipeline.WithLogging("PlanItems", logger, planItems)

	// [entries] - every entry point, one Entry each, run together by
	// entry.RunAll until the first one ends. The base plateau has only the
	// startup command; an API variation point replaces its Entry, the health
	// variation point adds one.
	err = entry.RunAll(ctx, logger,
		entry.Entry{Name: "startup", Run: func(ctx context.Context) error {
			return startup.Run(ctx, planItems, command.PlanItems{Titles: startup.EnvList("STARTUP_TITLES")}, stdout)
		}},
		entry.Entry{Name: "management", Run: func(ctx context.Context) error {
			// Readiness checks go here, one argument per dependency the service
			// needs before it takes traffic. The base plateau has none; a service
			// variation point adds its own, built in [services], e.g.
			//
			//	return management.Run(ctx, logger,
			//		management.Check{Name: "database", Check: database.Ping},
			//		management.Check{Name: "user-api", Check: userAPI.Ping},
			//	)
			//
			// /health/ready answers 503 and names a check while it returns an error.
			return management.Run(ctx, logger)
		}},
	)
	return entry.ExitCode(err)
}
