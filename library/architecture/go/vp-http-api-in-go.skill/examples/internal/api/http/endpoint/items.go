package endpoint

import (
	"net/http"

	httpapi "github.com/example/todo/internal/api/http"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

// This file is the items resource (/v1/items/...): its command handlers, its
// routes, and the request and response types of each route with their
// mappings. A new command on items adds a field, a route line, and its types
// here, and its path in openapi.json.

// ItemHandlers are the command handlers of the items resource.
type ItemHandlers struct {
	PlanItems pipeline.Handler[command.PlanItems, command.PlanItemsResult]
}

// itemRoutes are the items operations of the published contract.
func itemRoutes(h ItemHandlers) []httpapi.Route {
	return []httpapi.Route{
		// 200: planning creates nothing that is kept, and its work is done.
		{Pattern: "POST /v1/items/plan", Handler: httpapi.Endpoint(h.PlanItems, decodePlanItems, encodePlanItems, http.StatusOK)},
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

func decodePlanItems(r *http.Request) (command.PlanItems, error) {
	body, err := httpapi.DecodeJSON[planItemsRequest](r)
	if err != nil {
		return command.PlanItems{}, err
	}
	return command.PlanItems{Titles: body.Titles}, nil
}

func encodePlanItems(result command.PlanItemsResult) any {
	items := make([]itemResponse, len(result.Items))
	for i, item := range result.Items {
		items[i] = itemResponse{Title: item.Title, Done: item.Done}
	}
	return planItemsResponse{Items: items}
}
