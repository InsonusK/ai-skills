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
	grpcendpoint "github.com/example/todo/internal/api/grpc/endpoint"
	httpapi "github.com/example/todo/internal/api/http"
	httpendpoint "github.com/example/todo/internal/api/http/endpoint"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
	"github.com/example/todo/internal/config"
	"github.com/example/todo/internal/infrastructure/memory"
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
	// memory in this example; the DB variation point replaces these two lines.
	itemLists := service.NewItemLists(memory.NewLists())

	// [handlers] - one per command, wrapped by its decorators.
	var planItems pipeline.Handler[command.PlanItemsCommand, command.PlanItemsResult] = command.NewPlanItemsHandler()
	planItems = pipeline.WithLogging("PlanItems", logger, planItems)
	var addItem pipeline.Handler[command.AddItemCommand, command.AddItemResult] = command.NewAddItemHandler(itemLists)
	addItem = pipeline.WithLogging("AddItem", logger, addItem)
	var listItems pipeline.Handler[command.ListItemsCommand, command.ListItemsResult] = command.NewListItemsHandler(itemLists)
	listItems = pipeline.WithLogging("ListItems", logger, listItems)

	// [entries] - every entry point, one Entry each, run together by
	// entry.RunAll until the first one ends. The gRPC and HTTP forms of one
	// proto contract serve the same handlers.
	grpcEndpoints := grpcapi.Endpoints{Items: grpcendpoint.ItemsHandler{
		PlanItems: planItems, AddItem: addItem, ListItems: listItems,
	}}
	httpEndpoints := httpapi.Endpoints{
		Items: httpendpoint.ItemsHandler{PlanItems: planItems},
		Lists: httpendpoint.ListsHandler{AddItem: addItem, ListItems: listItems},
	}
	err = entry.RunAll(ctx, logger,
		entry.Entry{Name: "grpc", Run: func(ctx context.Context) error {
			return grpcapi.Run(ctx, grpcEndpoints, logger)
		}},
		entry.Entry{Name: "http", Run: func(ctx context.Context) error {
			return httpapi.Run(ctx, httpEndpoints, logger)
		}},
	)
	return entry.ExitCode(err)
}
