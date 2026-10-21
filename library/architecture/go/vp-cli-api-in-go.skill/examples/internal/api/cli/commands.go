// Package cli is the command-line entry point of the application: the place
// its resources are registered (commands.go). The resources live one file each
// in endpoint/, the fixed code in pipeline/. Other packages import it as cliapi.
package cli

import (
	"context"
	"io"
	"log/slog"

	"github.com/example/todo/internal/api/cli/endpoint"
	clipipeline "github.com/example/todo/internal/api/cli/pipeline"
)

// Endpoints are the resources of the command line, one field per resource
// file of endpoint/ - the same resources as the HTTP API's. The composition
// root fills them. A new resource adds a field here and a line to Resources;
// a new action of an existing resource changes only that resource's file.
type Endpoints struct {
	Items endpoint.ItemsHandler // endpoint/items.go
}

// Resources are the resources of the command line: `todo {resource} {action}`.
func Resources(e Endpoints) []clipipeline.Resource {
	return []clipipeline.Resource{
		e.Items.Resource(),
	}
}

// Run is the CLI entry point's one call from the composition root: it runs
// the action args name and returns its error (see clipipeline.Run).
func Run(ctx context.Context, logger *slog.Logger, name, version string, args []string, e Endpoints, stdout, stderr io.Writer) error {
	return clipipeline.Run(ctx, logger, name, version, args, Resources(e), stdout, stderr)
}
