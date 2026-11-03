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
	grpcapi "github.com/example/todo/internal/api/grpc"
	httpapi "github.com/example/todo/internal/api/http"
	"github.com/example/todo/internal/command"
	commandlogging "github.com/example/todo/internal/command/middleware/logging"
	commandunitofwork "github.com/example/todo/internal/command/middleware/unitofwork"
	"github.com/example/todo/internal/command/pipeline"
	"github.com/example/todo/internal/config"
	"github.com/example/todo/internal/infrastructure/postgres"
	"github.com/example/todo/internal/infrastructure/postgres/migrations"
	"github.com/example/todo/internal/infrastructure/postgres/tables"
	"github.com/example/todo/internal/infrastructure/postgres/unitofwork"
	"github.com/example/todo/internal/logging"
	"github.com/example/todo/internal/service"
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

	// [services] - infrastructure and the services over it. The lists live in
	// PostgreSQL: DATABASE_DSN names it; the migrations run here only with
	// MIGRATE_ON_START, else the job cmd/migrate has run them. The unit of
	// work joins the database before its first statement.
	dbConfig, err := postgres.LoadConfig()
	if err != nil {
		logger.Error("startup failed", "error", err)
		return 1
	}
	db, err := postgres.Open(dbConfig)
	if err != nil {
		logger.Error("startup failed", "error", err)
		return 1
	}
	defer db.Close()
	units, err := unitofwork.Attach(db)
	if err != nil {
		logger.Error("startup failed", "error", err)
		return 1
	}
	if dbConfig.MigrateOnStart {
		if err := postgres.Migrate(ctx, db, migrations.Files); err != nil {
			logger.Error("startup failed", "error", err)
			return 1
		}
	}
	itemLists := service.NewItemLists(postgres.NewRepository[tables.ItemRow](db))

	// [handlers] - the handler of every command, built by the command registry
	// over the services and wrapped by this command middleware, the first the
	// outermost. A variation point adds its middleware to this list at the
	// place its skill names; the unit of work is the last.
	handlers := command.NewHandlers([]pipeline.Middleware{
		commandlogging.Middleware(logger),
		commandunitofwork.Middleware(units),
	}, itemLists)

	// [entries] - every entry point, one Entry each, run together by
	// entry.RunAll until the first one ends. The gRPC and HTTP forms of one
	// proto contract serve the same handlers.
	err = entry.RunAll(ctx, logger,
		grpcapi.Entry(handlers, logger, []grpcapi.Interceptor{}),
		httpapi.Entry(handlers, logger, []httpapi.Middleware{}),
	)
	return entry.ExitCode(err)
}
