package command

import (
	"context"

	"github.com/example/todo/internal/domain/entity"
)

// PlanRemoteItemsCommand asks the todo service to plan one open item per title.
type PlanRemoteItemsCommand struct {
	Titles []string
}

// PlanRemoteItemsResult is the items the todo service planned, in the order
// of the titles.
type PlanRemoteItemsResult struct {
	Items []ItemView `json:"items"`
}

// remotePlanner is what PlanRemoteItemsHandler needs; service.RemotePlanner has it.
type remotePlanner interface {
	Plan(ctx context.Context, titles []string) ([]entity.Item, error)
}

// PlanRemoteItemsHandler handles PlanRemoteItemsCommand: titles the todo
// service rejects are invalid; a todo service that does not answer is
// unavailable.
type PlanRemoteItemsHandler struct {
	planner remotePlanner
}

func NewPlanRemoteItemsHandler(planner remotePlanner) *PlanRemoteItemsHandler {
	return &PlanRemoteItemsHandler{planner: planner}
}

func (h *PlanRemoteItemsHandler) Handle(ctx context.Context, cmd PlanRemoteItemsCommand) (PlanRemoteItemsResult, error) {
	items, err := h.planner.Plan(ctx, cmd.Titles)
	if err != nil {
		return PlanRemoteItemsResult{}, err
	}
	views := make([]ItemView, len(items))
	for i, item := range items {
		views[i] = ItemView{Title: item.Title(), Done: item.Done()}
	}
	return PlanRemoteItemsResult{Items: views}, nil
}
