// Package grpc is the gRPC entry point of the application: the place its
// services are registered (services.go). The services live one file each in
// endpoint/, the fixed code in pipeline/. Other packages import it as grpcapi.
package grpc

import (
	"context"
	"log/slog"

	"github.com/example/todo/internal/api/grpc/endpoint"
	grpcpipeline "github.com/example/todo/internal/api/grpc/pipeline"
)

// Endpoints are the services of the proto contract this API serves, one field
// per service file of endpoint/. The composition root fills them. A new
// service adds a field here and a line to Services; a new method of an
// existing service changes only that service's file.
type Endpoints struct {
	Items endpoint.ItemsHandler // endpoint/items.go
}

// Services are the services of the published contract
// (docs/integration/api/todo/todo.proto).
func Services(e Endpoints) []grpcpipeline.Service {
	return []grpcpipeline.Service{
		e.Items.Service(),
	}
}

// Run is the gRPC entry point's one call from the composition root: it serves
// every service until ctx is cancelled (see grpcpipeline.Run).
func Run(ctx context.Context, e Endpoints, logger *slog.Logger) error {
	return grpcpipeline.Run(ctx, Services(e), logger)
}
