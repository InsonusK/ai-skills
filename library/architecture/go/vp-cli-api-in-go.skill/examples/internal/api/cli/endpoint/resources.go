// Package endpoint is the application's part of the command-line entry point:
// one file per resource, each the counterpart of an ASP.NET controller - the
// same resources as the HTTP API's. The package above it (internal/api/cli) is
// copied from templates and never edited; everything that grows with the
// command line lives here.
package endpoint

import (
	cliapi "github.com/example/todo/internal/api/cli"
)

// Handlers are the command handlers the command line serves, one group per
// resource. The composition root fills them. A new resource adds a field here
// and a line to Resources; a new action of an existing resource changes only
// that resource's file.
type Handlers struct {
	Items ItemHandlers // items.go
}

// Resources are the resources of the command line: `todo {resource} {action}`.
func Resources(h Handlers) []cliapi.Resource {
	return []cliapi.Resource{
		itemsResource(h.Items),
	}
}
