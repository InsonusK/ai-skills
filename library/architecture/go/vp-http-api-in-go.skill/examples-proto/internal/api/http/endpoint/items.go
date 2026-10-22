package endpoint

import (
	"net/http"

	"github.com/example/todo/gen/api/todo"
	httppipeline "github.com/example/todo/internal/api/http/pipeline"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

// This file is the items resource (/v1/items/...): its command handlers, its
// routes, and per route the mapping of the generated messages to and from
// the command. A new command on items adds a field, a route line, and its two
// mappings here, and its rpc with its google.api.http annotation in todo.proto.

// ItemsHandler is the items resource: the command handlers its routes call. The
// composition root fills it; Routes lists its operations.
type ItemsHandler struct {
	PlanItems pipeline.Handler[command.PlanItemsCommand, command.PlanItemsResult]
}

// Routes are the items operations of the HTTP form.
func (h ItemsHandler) Routes() []httppipeline.Route {
	return []httppipeline.Route{
		// 200: planning creates nothing that is kept, and its work is done.
		{Pattern: "POST /v1/items/plan", Handler: httppipeline.Endpoint(h.PlanItems, decodePlanItems, encodePlanItems, http.StatusOK)},
	}
}

func decodePlanItems(r *http.Request) (command.PlanItemsCommand, error) {
	req := &todo.PlanItemsRequest{}
	if err := httppipeline.DecodeProto(r, req); err != nil {
		return command.PlanItemsCommand{}, err
	}
	return command.PlanItemsCommand{Titles: req.GetTitles()}, nil
}

func encodePlanItems(result command.PlanItemsResult) any {
	return httppipeline.ProtoJSON(&todo.PlanItemsResponse{Items: itemMessages(result.Items)})
}

// temMessages maps the command layer's items to the contract's messages.
func itemMessages(views []command.ItemView) []*todo.Item {
	items := make([]*todo.Item, len(views))
	for i, view := range views {
		items[i] = &todo.Item{Title: view.Title, Done: view.Done}
	}
	return items
}
