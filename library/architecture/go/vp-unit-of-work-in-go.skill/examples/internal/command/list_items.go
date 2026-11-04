package command

import (
	"context"

	"github.com/example/todo/internal/domain/entity"
)

// ListItemsCommand asks for the items of the list ListID.
type ListItemsCommand struct {
	ListID string
}

// ListItemsResult lists the items in the order they were added.
type ListItemsResult struct {
	Items []ItemView `json:"items"`
}

// itemReader is what ListItemsHandler needs of the item lists; service.ItemLists has it.
type itemReader interface {
	Items(ctx context.Context, listID string) ([]entity.Item, error)
}

// ListItemsHandler handles ListItemsCommand: an unknown list is the service's not found.
type ListItemsHandler struct {
	lists itemReader
}

func NewListItemsHandler(lists itemReader) *ListItemsHandler {
	return &ListItemsHandler{lists: lists}
}

func (h *ListItemsHandler) Handle(ctx context.Context, cmd ListItemsCommand) (ListItemsResult, error) {
	items, err := h.lists.Items(ctx, cmd.ListID)
	if err != nil {
		return ListItemsResult{}, err
	}
	views := make([]ItemView, len(items))
	for i, item := range items {
		views[i] = ItemView{Title: item.Title(), Done: item.Done()}
	}
	return ListItemsResult{Items: views}, nil
}
