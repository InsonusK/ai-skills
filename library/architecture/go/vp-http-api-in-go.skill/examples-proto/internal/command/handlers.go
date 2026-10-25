package command

import "github.com/example/todo/internal/command/pipeline"

// Handlers is the command registry: one field per command, each holding the
// command's handler already wrapped by every decorator. The composition root
// builds it once with NewHandlers; an entry point takes from it the handlers
// it serves (startup the one it runs, an API's resource the ones it routes).
type Handlers struct {
	PlanItems pipeline.Handler[PlanItemsCommand, PlanItemsResult]
	AddItem   pipeline.Handler[AddItemCommand, AddItemResult]
	ListItems pipeline.Handler[ListItemsCommand, ListItemsResult]
}

// itemLists is what the handlers need of the item lists; service.ItemLists
// has it.
type itemLists interface {
	itemAdder
	itemReader
}

// NewHandlers builds the handler of every command and wraps each with the
// decorators, the first the outermost (see pipeline.Decorate). The services
// a handler uses are parameters here, after the decorators.
//
// A new command adds its field above and its line below - the composition
// root does not change.
func NewHandlers(decorators []pipeline.Decorator, lists itemLists) Handlers {
	return Handlers{
		PlanItems: pipeline.Decorate("PlanItems", NewPlanItemsHandler(), decorators),
		AddItem:   pipeline.Decorate("AddItem", NewAddItemHandler(lists), decorators),
		ListItems: pipeline.Decorate("ListItems", NewListItemsHandler(lists), decorators),
	}
}
