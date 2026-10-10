package main

import (
	"context"
	"log/slog"
	"os"

	"github.com/example/linkcheck-service/internal/config"
	"github.com/example/linkcheck-service/internal/infrastructure/linkstore"
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
	ctx := context.Background()
	return linkstore.Migrate(ctx, cfg.DatabaseDSN)
}
