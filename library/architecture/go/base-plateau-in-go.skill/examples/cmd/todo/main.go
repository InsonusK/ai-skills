// The composition root: the one place that builds concrete types and wires
// them together. A variation point joins the application by adding lines to
// the marked sections below - never by editing another package.
package main

import (
	"context"
	"io"
	"log/slog"
	"os"

	"github.com/example/todo/internal/api/startup"
	"github.com/example/todo/internal/application"
	"github.com/example/todo/internal/config"
)

func main() {
	os.Exit(run(context.Background(), os.Stdout, os.Stderr))
}

func run(ctx context.Context, stdout, stderr io.Writer) int {
	cfg, err := config.Load()
	if err != nil {
		slog.New(slog.NewJSONHandler(stderr, nil)).Error("startup failed", "error", err)
		return startup.ExitFailure
	}
	logger := slog.New(slog.NewJSONHandler(stderr, &slog.HandlerOptions{Level: cfg.LogLevel}))

	// [services] - infrastructure the handlers use; none in the base plateau.

	// [handlers] - one per command, wrapped by its decorators.
	var planItems application.Handler[application.PlanItems, application.PlanItemsResult] = application.NewPlanItemsHandler()
	planItems = application.WithLogging("PlanItems", logger, planItems)

	// [entries] - every entry point; the base plateau has only the startup command.
	err = startup.Run(ctx, planItems, application.PlanItems{Titles: cfg.StartupTitles}, stdout)
	return startup.ExitCode(err)
}
