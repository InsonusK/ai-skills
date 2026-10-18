package http

import (
	"net/http"

	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

// Handlers are the command handlers this API serves; the composition root
// fills them. A new command served over HTTP adds a field here.
type Handlers struct {
	PlanItems pipeline.Handler[command.PlanItems, command.PlanItemsResult]
}

// contractRoutes are the operations of the published contract
// (docs/integration/api/todo/openapi.json). The contract is written by hand
// and so are these routes, so they are kept in one list a scenario holds
// against the contract. A new operation adds one line here, its request and
// response types below, and its path in openapi.json.
func contractRoutes(h Handlers) []Route {
	return []Route{
		// 200: planning creates nothing that is kept, and its work is done.
		{"POST /v1/items/plan", Endpoint(h.PlanItems, decodePlanItems, encodePlanItems, http.StatusOK)},
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
	body, err := DecodeJSON[planItemsRequest](r)
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
