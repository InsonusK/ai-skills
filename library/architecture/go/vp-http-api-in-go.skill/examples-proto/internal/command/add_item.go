package command

import (
	"context"

	"github.com/example/todo/internal/domain/entity"
)

// AddItemCommand asks to add an open item with Title to the list ListID.
type AddItemCommand struct {
	ListID string
	Title  string
}

// AddItemResult is the added item.
type AddItemResult struct {
	Item ItemView `json:"item"`
}

// itemAdder is what AddItemHandler needs of the item lists; service.ItemLists has it.
type itemAdder interface {
	Add(ctx context.Context, listID string, item entity.Item) error
}

// AddItemHandler handles AddItemCommand: an invalid title is the entity's invalid
// error, a title already in the list the service's conflict.
type AddItemHandler struct {
	lists itemAdder
}

func NewAddItemHandler(lists itemAdder) *AddItemHandler {
	return &AddItemHandler{lists: lists}
}

func (h *AddItemHandler) Handle(ctx context.Context, cmd AddItemCommand) (AddItemResult, error) {
	item, err := entity.NewItem(cmd.Title)
	if err != nil {
		return AddItemResult{}, err
	}
	if err := h.lists.Add(ctx, cmd.ListID, item); err != nil {
		return AddItemResult{}, err
	}
	return AddItemResult{Item: ItemView{Title: item.Title(), Done: item.Done()}}, nil
}
