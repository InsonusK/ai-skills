// Package endpoint is the application's part of the HTTP entry point: one file
// per resource, each the counterpart of an ASP.NET controller. The package
// above it (internal/api/http) is copied from templates and never edited;
// everything that grows with the API lives here.
package endpoint

import (
	"slices"

	httpapi "github.com/example/todo/internal/api/http"
)

// Handlers are the command handlers this API serves, one group per resource.
// The composition root fills them. A new resource adds a field here and a line
// to Routes; a new command of an existing resource changes only that
// resource's file.
type Handlers struct {
	Items ItemHandlers // items.go
	Lists ListHandlers // lists.go
}

// Routes are the HTTP form of the proto contract
// (docs/integration/api/todo/todo.proto): one route per google.api.http
// annotation, from every resource file. The spec is generated from the same
// annotations and these routes are written by hand, so they are kept in one
// list a scenario holds against the spec.
func Routes(h Handlers) []httpapi.Route {
	return slices.Concat(
		itemRoutes(h.Items),
		listRoutes(h.Lists),
	)
}
