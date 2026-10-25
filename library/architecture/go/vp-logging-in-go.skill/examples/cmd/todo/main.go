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
	"github.com/example/todo/internal/api/startup"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
	"github.com/example/todo/internal/config"
	"github.com/example/todo/internal/logging/masked"
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
	// The handler decides what reaches the log: the logging variation point's
	// writes every payload masked, with the request id of the call.
	maskedCfg, err := masked.LoadConfig(cfg.LogLevel)
	if err != nil {
		slog.New(slog.NewJSONHandler(stderr, nil)).Error("startup failed", "error", err)
		return 1
	}
	logger := slog.New(masked.NewHandler(stderr, cfg.LogLevel, maskedCfg))
	logger.Info("starting", "version", version.Version)

	// [services] - infrastructure and the services over it; none in the base plateau.

	// [handlers] - the handler of every command, built by the command registry
	// and wrapped by these decorators, the first the outermost. A variation
	// point adds its decorator to this list at the place its skill names.
	handlers := command.NewHandlers([]pipeline.Decorator{
		pipeline.WithLogging(logger),
	})

	// [entries] - every entry point, one Entry each, run together by
	// entry.RunAll until the first one ends. The base plateau has only the
	// startup command; an API variation point replaces its Entry, the health
	// variation point adds one.
	err = entry.RunAll(ctx, logger,
		entry.Entry{Name: "startup", Run: func(ctx context.Context) error {
			return startup.Run(ctx, handlers.PlanItems, command.PlanItemsCommand{Titles: startup.EnvList("STARTUP_TITLES")}, stdout)
		}},
	)
	return entry.ExitCode(err)
}
