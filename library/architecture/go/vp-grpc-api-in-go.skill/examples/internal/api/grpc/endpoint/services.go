// Package endpoint is the application's part of the gRPC entry point: one file
// per service of the proto contract, each the counterpart of an ASP.NET
// controller. The package above it (internal/api/grpc) is copied from
// templates and never edited; everything that grows with the API lives here.
package endpoint

import (
	grpcapi "github.com/example/todo/internal/api/grpc"
)

// Handlers are the command handlers this API serves, one group per service of
// the contract. The composition root fills them. A new service adds a field
// here and a line to Services; a new method of an existing service changes
// only that service's file.
type Handlers struct {
	Items ItemHandlers // items.go
}

// Services are the services of the published contract
// (docs/integration/api/todo/todo.proto), one per service file.
func Services(h Handlers) []grpcapi.Service {
	return []grpcapi.Service{
		itemsService(h.Items),
	}
}
