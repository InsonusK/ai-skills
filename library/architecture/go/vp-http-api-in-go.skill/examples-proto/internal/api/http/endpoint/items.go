package endpoint

import (
	"net/http"

	"github.com/example/todo/gen/api/todo"
	httpapi "github.com/example/todo/internal/api/http"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

// This file is the items resource (/v1/items/...): its command handlers, its
// routes, and per route the mapping of the generated messages to and from
// the command. A new command on items adds a field, a route line, and its two
// mappings here, and its rpc with its google.api.http annotation in todo.proto.

// ItemHandlers are the command handlers of the items resource.
type ItemHandlers struct {
	PlanItems pipeline.Handler[command.PlanItems, command.PlanItemsResult]
}

// itemRoutes are the items operations of the HTTP form.
func itemRoutes(h ItemHandlers) []httpapi.Route {
	return []httpapi.Route{
		// 200: planning creates nothing that is kept, and its work is done.
		{Pattern: "POST /v1/items/plan", Handler: httpapi.Endpoint(h.PlanItems, decodePlanItems, encodePlanItems, http.StatusOK)},
	}
}

func decodePlanItems(r *http.Request) (command.PlanItems, error) {
	req := &todo.PlanItemsRequest{}
	if err := httpapi.DecodeProto(r, req); err != nil {
		return command.PlanItems{}, err
	}
	return command.PlanItems{Titles: req.GetTitles()}, nil
}

func encodePlanItems(result command.PlanItemsResult) any {
	return httpapi.ProtoJSON(&todo.PlanItemsResponse{Items: itemMessages(result.Items)})
}

// itemMessages maps the command layer's items to the contract's messages.
func itemMessages(views []command.ItemView) []*todo.Item {
	items := make([]*todo.Item, len(views))
	for i, view := range views {
		items[i] = &todo.Item{Title: view.Title, Done: view.Done}
	}
	return items
}
