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
	httpapi "github.com/example/todo/internal/api/http"
	"github.com/example/todo/internal/command"
	commandlogging "github.com/example/todo/internal/command/middleware/logging"
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

	// [handlers] - the handler of every command, built by the command registry
	// and wrapped by this command middleware, the first the outermost. A
	// variation point adds its middleware to this list at the place its skill
	// names.
	handlers := command.NewHandlers([]pipeline.Middleware{
		commandlogging.Middleware(logger),
	})

	// [entries] - every entry point, one Entry each, run together by
	// entry.RunAll until the first one ends. The HTTP variation point replaced
	// the startup Entry with its own; the health variation point adds one.
	err = entry.RunAll(ctx, logger,
		// the HTTP API: its contract routes wrapped by these middlewares, the
		// first the outermost, inside the request log
		httpapi.Entry(handlers, logger, []httpapi.Middleware{}),
	)
	return entry.ExitCode(err)
}
