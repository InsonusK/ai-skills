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
	"github.com/example/todo/internal/api/core/entry"
	"github.com/example/todo/internal/command"
)

// Endpoints are the resources of the command line, one field per resource
// file of endpoint/ - the same resources as the HTTP API's. A new resource
// adds a field here and a line to NewEndpoints and to Resources; a new action
// of an existing resource changes only that resource's file.
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

// Resources are the resources of the command line: `todo {resource} {action}`.
func Resources(e Endpoints) []clipipeline.Resource {
	return []clipipeline.Resource{
		e.Items.Resource(),
	}
}

// Entry is the CLI entry point as the composition root lists it for
// entry.RunAll: it runs the action args name over the handlers of the
// registry and ends, and the process ends with it. It shows its error itself
// (ReportsErrors), so RunAll does not log it again. The command line has no
// decorators of its own: a step on every action is a command decorator.
func Entry(handlers command.Handlers, logger *slog.Logger, name, version string, args []string, stdout, stderr io.Writer) entry.Entry {
	endpoints := NewEndpoints(handlers)
	return entry.Entry{Name: "cli", ReportsErrors: true, Run: func(ctx context.Context) error {
		return Run(ctx, logger, name, version, args, endpoints, stdout, stderr)
	}}
}

// Run runs the action args name over the resources and returns its error
// (see clipipeline.Run); tests call it, the composition root lists Entry.
func Run(ctx context.Context, logger *slog.Logger, name, version string, args []string, e Endpoints, stdout, stderr io.Writer) error {
	return clipipeline.Run(ctx, logger, name, version, args, Resources(e), stdout, stderr)
}
