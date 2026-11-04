// Package grpc is the gRPC entry point of the application: the place its
// services are registered (services.go). The services live one file each in
// endpoint/, the fixed code in pipeline/. Other packages import it as grpcapi.
package grpc

import (
	"context"
	"log/slog"

	"github.com/example/todo/internal/api/core/entry"
	"github.com/example/todo/internal/api/grpc/endpoint"
	grpcpipeline "github.com/example/todo/internal/api/grpc/pipeline"
	"github.com/example/todo/internal/command"
)

// Interceptor is an API middleware of every call of the contract (see
// grpcpipeline.Interceptor); the composition root lists them for Entry.
type Interceptor = grpcpipeline.Interceptor

// Endpoints are the services of the proto contract this API serves, one field
// per service file of endpoint/. A new service adds a field here and a line
// to NewEndpoints and to Services; a new method of an existing service
// changes only that service's file.
type Endpoints struct {
	Items endpoint.ItemsHandler // endpoint/items.go
}

// NewEndpoints gives every service the handlers of its commands from the
// command registry.
func NewEndpoints(handlers command.Handlers) Endpoints {
	return Endpoints{
		Items: endpoint.NewItemsHandler(handlers),
	}
}

// Services are the services of the published contract
// (docs/integration/api/todo/todo.proto).
func Services(e Endpoints) []grpcpipeline.Service {
	return []grpcpipeline.Service{
		e.Items.Service(),
	}
}

// Entry is the gRPC entry point as the composition root lists it for
// entry.RunAll: every service over the handlers of the registry, every call
// wrapped by the interceptors - the first the outermost, all inside the call
// log - served until the process stops (see grpcpipeline.Run).
func Entry(handlers command.Handlers, logger *slog.Logger, interceptors []Interceptor) entry.Entry {
	services := Services(NewEndpoints(handlers))
	return entry.Entry{Name: "grpc", Run: func(ctx context.Context) error {
		return grpcpipeline.Run(ctx, services, logger, interceptors)
	}}
}
