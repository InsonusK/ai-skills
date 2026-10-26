// Package todoapi is the adapter of the todo API of another service
// (docs/integration/client/todo/todo.proto): one method per method of the
// contract this application calls, each a call of the generated client over
// grpcclient's connection, mapped to types of its own. It reports technical
// outcomes - grpcclient.ErrUnavailable, *grpcclient.StatusError - and leaves
// their meaning to the service over it (internal/service).
package todoapi

import (
	"context"
	"log/slog"

	"github.com/example/todo/gen/client/todo"
	"github.com/example/todo/internal/infrastructure/client/core/grpcclient"
	"github.com/example/todo/internal/infrastructure/client/todoapi/mock"
)

// Prefix names the settings of this API: TODO_API_HOST, TODO_API_GRPC_PORT,
// TODO_API_TLS, TODO_API_TIMEOUT.
const Prefix = "TODO_API"

// Methods are every method of the contract this adapter calls, by full name;
// one scenario holds them against the copied contract. A new method of the
// adapter adds its line.
var Methods = []string{
	todo.ItemsService_PlanItems_FullMethodName,
}

// Item is an item as the API answers it, the adapter's own type.
type Item struct {
	Title string
	Done  bool
}

// Client is the todo API.
type Client struct {
	conn  *grpcclient.Client
	items todo.ItemsServiceClient
}

// New reads the settings of the API and builds its client; with
// TODO_API_HOST=mock it answers from mock.Emulator.
func New(logger *slog.Logger) (*Client, error) {
	cfg, err := grpcclient.LoadConfig(Prefix)
	if err != nil {
		return nil, err
	}
	return NewClient(cfg, logger)
}

// NewClient builds the client from settings given - the scenarios point it at
// an emulator of their own.
func NewClient(cfg grpcclient.Config, logger *slog.Logger) (*Client, error) {
	conn, err := grpcclient.New(cfg, logger, mock.Emulator)
	if err != nil {
		return nil, err
	}
	return &Client{conn: conn, items: todo.NewItemsServiceClient(conn)}, nil
}

// Close closes the connection to the API.
func (c *Client) Close() error { return c.conn.Close() }

// PlanItems is ItemsService.PlanItems: one open item per title, planned by the API.
func (c *Client) PlanItems(ctx context.Context, titles []string) ([]Item, error) {
	answer, err := c.items.PlanItems(ctx, &todo.PlanItemsRequest{Titles: titles})
	if err != nil {
		return nil, err
	}
	items := make([]Item, len(answer.GetItems()))
	for i, item := range answer.GetItems() {
		items[i] = Item{Title: item.GetTitle(), Done: item.GetDone()}
	}
	return items, nil
}
