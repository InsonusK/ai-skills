// Package http is the HTTP entry point of the application: the place its
// resources are registered (routes.go). The resources live one file each in
// endpoint/, the fixed code in pipeline/. Other packages import it as httpapi.
package http

import (
	"context"
	"log/slog"
	"slices"

	"github.com/example/todo/internal/api/http/endpoint"
	httppipeline "github.com/example/todo/internal/api/http/pipeline"
)

// Endpoints are the resources this API serves, one field per resource file of
// endpoint/. The composition root fills them. A new resource adds a field here
// and a line to Routes; a new command of an existing resource changes only
// that resource's file.
type Endpoints struct {
	Items endpoint.ItemsHandler // endpoint/items.go
}

// Routes are the operations of the published contract
// (docs/integration/api/todo/openapi.json): the routes of every resource. The contract
// and the routes are kept in step by one scenario.
func Routes(e Endpoints) []httppipeline.Route {
	return slices.Concat(
		e.Items.Routes(),
	)
}

// Run is the HTTP entry point's one call from the composition root: it serves
// the routes of every resource until ctx is cancelled (see httppipeline.Run).
func Run(ctx context.Context, e Endpoints, logger *slog.Logger) error {
	return httppipeline.Run(ctx, Routes(e), logger)
}
