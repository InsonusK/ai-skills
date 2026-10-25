package command

import (
	"context"

	domainservice "github.com/example/todo/internal/domain/service"
)

// PlanItemsCommand asks to plan one open to-do item per title.
type PlanItemsCommand struct {
	Titles []string
}

// RequiredRole marks PlanItemsCommand access.Restricted: only a planner may plan.
func (PlanItemsCommand) RequiredRole() string { return "planner" }

// BatchSize marks PlanItemsCommand batchlimit.Batch: its size is the number of titles.
func (c PlanItemsCommand) BatchSize() int { return len(c.Titles) }

// PlanItemsResult lists the planned items in the order of the titles.
type PlanItemsResult struct {
	Items []ItemView `json:"items"`
}

// ItemView is an item as callers see it; domain types never leave the application.
type ItemView struct {
	Title string `json:"title"`
	Done  bool   `json:"done"`
}

// PlanItemsHandler handles PlanItemsCommand.
type PlanItemsHandler struct {
	planner domainservice.Planner
}

func NewPlanItemsHandler() *PlanItemsHandler {
	return &PlanItemsHandler{planner: domainservice.NewPlanner()}
}

func (h *PlanItemsHandler) Handle(_ context.Context, cmd PlanItemsCommand) (PlanItemsResult, error) {
	items, err := h.planner.Plan(cmd.Titles)
	if err != nil {
		return PlanItemsResult{}, err
	}
	views := make([]ItemView, len(items))
	for i, item := range items {
		views[i] = ItemView{Title: item.Title(), Done: item.Done()}
	}
	return PlanItemsResult{Items: views}, nil
}
