package endpoint

import (
	"net/http"

	"github.com/example/todo/gen/api/todo"
	httppipeline "github.com/example/todo/internal/api/http/pipeline"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

// This file is the lists resource (/v1/lists/{listId}/...). The path
// parameter is named as the generated spec names it - listId, the
// lowerCamelCase form of the proto field list_id - and the route's decode
// reads it with r.PathValue, since it is not in the body.

// ListsHandler is the lists resource: the command handlers its routes call.
// NewListsHandler takes them from the command registry; Routes lists its operations.
type ListsHandler struct {
	AddItem   pipeline.Handler[command.AddItemCommand, command.AddItemResult]
	ListItems pipeline.Handler[command.ListItemsCommand, command.ListItemsResult]
}

// NewListsHandler takes the handlers of the lists commands from the command registry.
func NewListsHandler(handlers command.Handlers) ListsHandler {
	return ListsHandler{
		AddItem:   handlers.AddItem,
		ListItems: handlers.ListItems,
	}
}

// Routes are the lists operations of the HTTP form.
func (h ListsHandler) Routes() []httppipeline.Route {
	return []httppipeline.Route{
		// 201: the item is created and kept.
		{Pattern: "POST /v1/lists/{listId}/items", Handler: httppipeline.Endpoint(h.AddItem, decodeAddItem, encodeAddItem, http.StatusCreated)},
		{Pattern: "GET /v1/lists/{listId}/items", Handler: httppipeline.Endpoint(h.ListItems, decodeListItems, encodeListItems, http.StatusOK)},
		// ClearList is deprecated since 1.1.0: no command, the same answer as its gRPC form.
		{Pattern: "POST /v1/lists/{listId}/clear", Handler: httppipeline.Unimplemented("ClearList is deprecated since 1.1.0")},
	}
}

func decodeAddItem(r *http.Request) (command.AddItemCommand, error) {
	req := &todo.AddItemRequest{}
	if err := httppipeline.DecodeProto(r, req); err != nil {
		return command.AddItemCommand{}, err
	}
	return command.AddItemCommand{ListID: r.PathValue("listId"), Title: req.GetTitle()}, nil
}

func encodeAddItem(result command.AddItemResult) any {
	return httppipeline.ProtoJSON(&todo.AddItemResponse{Item: &todo.Item{Title: result.Item.Title, Done: result.Item.Done}})
}

// decodeListItems reads only the path: a GET has no body.
func decodeListItems(r *http.Request) (command.ListItemsCommand, error) {
	return command.ListItemsCommand{ListID: r.PathValue("listId")}, nil
}

func encodeListItems(result command.ListItemsResult) any {
	return httppipeline.ProtoJSON(&todo.ListItemsResponse{Items: itemMessages(result.Items)})
}
