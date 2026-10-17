package command

import (
	"context"

	"github.com/example/todo/internal/domain"
)

// PlanItems asks to plan one open to-do item per title.
type PlanItems struct {
	Titles []string
}

// PlanItemsResult lists the planned items in the order of the titles.
type PlanItemsResult struct {
	Items []ItemView `json:"items"`
}

// ItemView is an item as callers see it; domain types never leave the application.
type ItemView struct {
	Title string `json:"title"`
	Done  bool   `json:"done"`
}

// PlanItemsHandler handles PlanItems.
type PlanItemsHandler struct {
	planner domain.Planner
}

func NewPlanItemsHandler() *PlanItemsHandler {
	return &PlanItemsHandler{planner: domain.NewPlanner()}
}

func (h *PlanItemsHandler) Handle(_ context.Context, cmd PlanItems) (PlanItemsResult, error) {
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
