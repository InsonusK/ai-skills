// Package http is the HTTP entry point of the application: the place its
// resources are registered (routes.go). The resources live one file each in
// endpoint/, the fixed code in pipeline/. Other packages import it as httpapi.
package http

import (
	"context"
	"log/slog"
	"slices"

	"github.com/example/todo/internal/api/core/entry"
	"github.com/example/todo/internal/api/http/endpoint"
	httppipeline "github.com/example/todo/internal/api/http/pipeline"
	"github.com/example/todo/internal/command"
)

// Middleware is an API middleware of the requests of every contract route (see
// httppipeline.Middleware); the composition root lists them for Entry.
type Middleware = httppipeline.Middleware

// Endpoints are the resources this API serves, one field per resource file of
// endpoint/. A new resource adds a field here and a line to NewEndpoints and
// to Routes; a new command of an existing resource changes only that
// resource's file.
type Endpoints struct {
	Items endpoint.ItemsHandler // endpoint/items.go
}

// NewEndpoints gives every resource the handlers of its commands from the
// command registry.
func NewEndpoints(handlers command.Handlers) Endpoints {
	return Endpoints{
		Items: endpoint.NewItemsHandler(handlers),
	}
}

// Routes are the operations of the published contract
// (docs/integration/api/todo/openapi.json): the routes of every resource. The contract
// and the routes are kept in step by one scenario.
func Routes(e Endpoints) []httppipeline.Route {
	return slices.Concat(
		e.Items.Routes(),
	)
}

// Entry is the HTTP entry point as the composition root lists it for
// entry.RunAll: the routes of every resource over the handlers of the
// registry, each wrapped by the middlewares - the first the outermost, all
// inside the request log - served until the process stops (see
// httppipeline.Run).
func Entry(handlers command.Handlers, logger *slog.Logger, middlewares []Middleware) entry.Entry {
	routes := Routes(NewEndpoints(handlers))
	return entry.Entry{Name: "http", Run: func(ctx context.Context) error {
		return httppipeline.Run(ctx, routes, logger, middlewares)
	}}
}
