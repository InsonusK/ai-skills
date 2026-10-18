package http

import "slices"

// Handlers are the command handlers this API serves, one group per resource -
// the way controllers group actions in ASP.NET. The composition root fills
// them. A new resource adds a field here and a line to contractRoutes; a new
// command of an existing resource changes only that resource's file.
type Handlers struct {
	Items ItemHandlers // items.go
}

// contractRoutes are the operations of the published contract
// (docs/integration/api/todo/openapi.json): the routes of every resource file.
// The contract is written by hand and so are these routes, so they are kept in
// one list a scenario holds against the contract.
func contractRoutes(h Handlers) []Route {
	return slices.Concat(
		itemRoutes(h.Items),
	)
}
