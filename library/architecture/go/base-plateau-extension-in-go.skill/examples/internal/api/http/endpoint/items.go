package endpoint

import (
	"net/http"

	httppipeline "github.com/example/todo/internal/api/http/pipeline"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

// This file is the items resource (/v1/items/...): its command handlers, its
// routes, and the request and response types of each route with their
// mappings. A new command on items adds a field, a line of NewItemsHandler, a
// route line, and its types here, and its path in openapi.json.

// ItemsHandler is the items resource: the command handlers its routes call.
// NewItemsHandler takes them from the command registry; Routes lists its operations.
type ItemsHandler struct {
	PlanItems pipeline.Handler[command.PlanItemsCommand, command.PlanItemsResult]
}

// NewItemsHandler takes the handlers of the items commands from the command registry.
func NewItemsHandler(handlers command.Handlers) ItemsHandler {
	return ItemsHandler{
		PlanItems: handlers.PlanItems,
	}
}

// Routes are the items operations of the published contract.
func (h ItemsHandler) Routes() []httppipeline.Route {
	return []httppipeline.Route{
		// 200: planning creates nothing that is kept, and its work is done.
		{Pattern: "POST /v1/items/plan", Handler: httppipeline.Endpoint(h.PlanItems, decodePlanItems, encodePlanItems, http.StatusOK)},
	}
}

// planItemsRequest is the body of POST /v1/items/plan.
type planItemsRequest struct {
	Titles []string `json:"titles"`
}

// planItemsResponse is the answer of POST /v1/items/plan.
type planItemsResponse struct {
	Items []itemResponse `json:"items"`
}

type itemResponse struct {
	Title string `json:"title"`
	Done  bool   `json:"done"`
}

func decodePlanItems(r *http.Request) (command.PlanItemsCommand, error) {
	body, err := httppipeline.DecodeJSON[planItemsRequest](r)
	if err != nil {
		return command.PlanItemsCommand{}, err
	}
	return command.PlanItemsCommand{Titles: body.Titles}, nil
}

func encodePlanItems(result command.PlanItemsResult) any {
	items := make([]itemResponse, len(result.Items))
	for i, item := range result.Items {
		items[i] = itemResponse{Title: item.Title, Done: item.Done}
	}
	return planItemsResponse{Items: items}
}
