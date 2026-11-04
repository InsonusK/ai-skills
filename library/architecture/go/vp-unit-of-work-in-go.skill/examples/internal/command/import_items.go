package command

import (
	"context"

	"github.com/example/todo/internal/command/pipeline"
)

// ImportItemsCommand asks to add every one of Titles to the list ListID, or
// none.
type ImportItemsCommand struct {
	ListID string
	Titles []string
}

// ImportItemsResult is the added items, in the order of the titles.
type ImportItemsResult struct {
	Items []ItemView `json:"items"`
}

// ImportItemsHandler handles ImportItemsCommand by one nested AddItem command
// per title, one after another. The first that fails ends the import with its
// error; the unit of work, begun by this command, then drops the items added
// before it.
type ImportItemsHandler struct {
	addItem pipeline.Handler[AddItemCommand, AddItemResult]
}

// NewImportItemsHandler takes the AddItem handler the command registry
// decorated, so the nested command runs through every middleware.
func NewImportItemsHandler(addItem pipeline.Handler[AddItemCommand, AddItemResult]) *ImportItemsHandler {
	return &ImportItemsHandler{addItem: addItem}
}

func (h *ImportItemsHandler) Handle(ctx context.Context, cmd ImportItemsCommand) (ImportItemsResult, error) {
	items := make([]ItemView, 0, len(cmd.Titles))
	for _, title := range cmd.Titles {
		added, err := h.addItem.Handle(ctx, AddItemCommand{ListID: cmd.ListID, Title: title})
		if err != nil {
			return ImportItemsResult{}, err
		}
		items = append(items, added.Item)
	}
	return ImportItemsResult{Items: items}, nil
}
