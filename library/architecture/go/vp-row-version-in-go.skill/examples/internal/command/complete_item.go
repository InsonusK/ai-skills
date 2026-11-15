package command

import (
	"context"

	"github.com/example/todo/internal/domain/entity"
	"github.com/example/todo/internal/shared/errorkind"
)

// CompleteItemCommand asks to mark the item ItemID of the list ListID done.
// Version is the item's version the caller read: an item changed since is
// not completed (skill vp-row-version).
type CompleteItemCommand struct {
	ListID  string
	ItemID  int64
	Version string
}

// CompleteItemResult is the completed item, with its new version.
type CompleteItemResult struct {
	Item ItemView `json:"item"`
}

// itemCompleter is what CompleteItemHandler needs of the item lists;
// service.ItemLists has it.
type itemCompleter interface {
	Item(ctx context.Context, listID string, id int64) (entity.KeptItem, error)
	Save(ctx context.Context, listID string, item entity.KeptItem) (entity.KeptItem, error)
}

// errNoVersion: a change with no version would skip the check, so it is
// refused (skill vp-row-version, "Require the version on a write").
var errNoVersion = errorkind.New(errorkind.ErrInvalid, "the version of the item is missing: give back the version you read")

// CompleteItemHandler handles CompleteItemCommand.
type CompleteItemHandler struct {
	lists itemCompleter
}

func NewCompleteItemHandler(lists itemCompleter) *CompleteItemHandler {
	return &CompleteItemHandler{lists: lists}
}

// Handle reads the item, completes it by the entity's rule, and saves it
// under the caller's version - not the one just read - so a change made
// since the caller's read is a concurrent change, never overwritten.
func (h *CompleteItemHandler) Handle(ctx context.Context, cmd CompleteItemCommand) (CompleteItemResult, error) {
	if cmd.Version == "" {
		return CompleteItemResult{}, errNoVersion
	}
	item, err := h.lists.Item(ctx, cmd.ListID, cmd.ItemID)
	if err != nil {
		return CompleteItemResult{}, err
	}
	if err := item.Complete(); err != nil {
		return CompleteItemResult{}, err
	}
	item.Version = cmd.Version
	saved, err := h.lists.Save(ctx, cmd.ListID, item)
	if err != nil {
		return CompleteItemResult{}, err
	}
	return CompleteItemResult{Item: keptView(saved)}, nil
}

// keptView is the view of an item kept in a list: its fields, its id, its version.
func keptView(item entity.KeptItem) ItemView {
	return ItemView{Title: item.Title(), Done: item.Done(), ID: item.ID, Version: item.Version}
}
