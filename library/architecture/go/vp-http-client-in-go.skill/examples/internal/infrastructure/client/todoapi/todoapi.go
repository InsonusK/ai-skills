// Package todoapi is the adapter of the todo API of another service
// (docs/integration/client/todo/openapi.json): one method per operation this
// application calls, each a call of httpclient with request and response
// types of its own. It reports technical outcomes - httpclient.ErrUnavailable,
// *httpclient.StatusError - and leaves their meaning to the service over it
// (internal/service).
package todoapi

import (
	"context"
	"log/slog"
	"net/http"

	"github.com/example/todo/internal/infrastructure/client/core/httpclient"
)

// Prefix names the settings of this API: TODO_API_HOST, TODO_API_HTTP_PORT,
// TODO_API_TLS, TODO_API_TIMEOUT.
const Prefix = "TODO_API"

// Operation is one operation of the contract this adapter calls.
type Operation struct{ Method, Path string }

// Operations are every operation this adapter calls; one scenario holds them
// against the copied contract. A new method of the adapter adds its line.
var Operations = []Operation{
	{http.MethodPost, "/v1/items/plan"},
}

// Item is an item as the API answers it, the adapter's own type.
type Item struct {
	Title string
	Done  bool
}

// Client is the todo API.
type Client struct {
	http *httpclient.Client
}

// New reads the settings of the API and builds its client; with
// TODO_API_HOST=mock it answers from Emulator.
func New(logger *slog.Logger) (*Client, error) {
	cfg, err := httpclient.LoadConfig(Prefix)
	if err != nil {
		return nil, err
	}
	return NewClient(cfg, logger), nil
}

// NewClient builds the client from settings given - the scenarios point it at
// an emulator of their own.
func NewClient(cfg httpclient.Config, logger *slog.Logger) *Client {
	return &Client{http: httpclient.New(cfg, logger, Emulator())}
}

type planItemsRequest struct {
	Titles []string `json:"titles"`
}

type planItemsResponse struct {
	Items []struct {
		Title string `json:"title"`
		Done  bool   `json:"done"`
	} `json:"items"`
}

// PlanItems is POST /v1/items/plan: one open item per title, planned by the API.
func (c *Client) PlanItems(ctx context.Context, titles []string) ([]Item, error) {
	var answer planItemsResponse
	if err := c.http.Call(ctx, http.MethodPost, "/v1/items/plan", planItemsRequest{Titles: titles}, &answer); err != nil {
		return nil, err
	}
	items := make([]Item, len(answer.Items))
	for i, item := range answer.Items {
		items[i] = Item{Title: item.Title, Done: item.Done}
	}
	return items, nil
}
