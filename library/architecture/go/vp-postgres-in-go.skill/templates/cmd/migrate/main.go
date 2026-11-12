// The one-shot migration job, /app/migrate of the image: it applies every
// migration not yet applied and exits 0, also on a database already up to
// date; a failure exits 1. The service starts once it has finished, unless
// the deployment is pinned to one instance and sets MIGRATE_ON_START.
package main

import (
	"context"
	"io"
	"log/slog"
	"os"
	"os/signal"
	"syscall"

	"{module-path}/internal/infrastructure/postgres"
	"{module-path}/internal/infrastructure/postgres/migrations"
)

func main() {
	ctx, stop := signal.NotifyContext(context.Background(), syscall.SIGINT, syscall.SIGTERM)
	code := run(ctx, os.Stderr)
	stop()
	os.Exit(code)
}

func run(ctx context.Context, stderr io.Writer) int {
	logger := slog.New(slog.NewJSONHandler(stderr, nil))
	cfg, err := postgres.LoadConfig()
	if err != nil {
		logger.Error("migration failed", "error", err)
		return 1
	}
	db, err := postgres.Open(cfg)
	if err != nil {
		logger.Error("migration failed", "error", err)
		return 1
	}
	defer db.Close()
	if err := postgres.Migrate(ctx, db, migrations.Files); err != nil {
		logger.Error("migration failed", "error", err)
		return 1
	}
	logger.Info("migrations applied")
	return 0
}
