package endpoint

import (
	"fmt"
	"net/http"
	"strconv"

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
	// CompleteItem takes the version the caller read.
	CompleteItem pipeline.Handler[command.CompleteItemCommand, command.CompleteItemResult]
}

// NewListsHandler takes the handlers of the lists commands from the command registry.
func NewListsHandler(handlers command.Handlers) ListsHandler {
	return ListsHandler{
		AddItem:      handlers.AddItem,
		ListItems:    handlers.ListItems,
		CompleteItem: handlers.CompleteItem,
	}
}

// Routes are the lists operations of the HTTP form.
func (h ListsHandler) Routes() []httppipeline.Route {
	return []httppipeline.Route{
		// 201: the item is created and kept.
		{Pattern: "POST /v1/lists/{listId}/items", Handler: httppipeline.Endpoint(h.AddItem, decodeAddItem, encodeAddItem, http.StatusCreated)},
		{Pattern: "GET /v1/lists/{listId}/items", Handler: httppipeline.Endpoint(h.ListItems, decodeListItems, encodeListItems, http.StatusOK)},
		{Pattern: "POST /v1/lists/{listId}/items/{itemId}/complete", Handler: httppipeline.Endpoint(h.CompleteItem, decodeCompleteItem, encodeCompleteItem, http.StatusOK)},
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
	return httppipeline.ProtoJSON(&todo.AddItemResponse{Item: itemMessage(result.Item)})
}

// decodeListItems reads only the path: a GET has no body.
func decodeListItems(r *http.Request) (command.ListItemsCommand, error) {
	return command.ListItemsCommand{ListID: r.PathValue("listId")}, nil
}

func encodeListItems(result command.ListItemsResult) any {
	return httppipeline.ProtoJSON(&todo.ListItemsResponse{Items: itemMessages(result.Items)})
}

// decodeCompleteItem reads the item from the path and the version from the
// body. An item id that is no number is a request the contract cannot
// decode: the pipeline answers it 400 INVALID_ARGUMENT.
func decodeCompleteItem(r *http.Request) (command.CompleteItemCommand, error) {
	req := &todo.CompleteItemRequest{}
	if err := httppipeline.DecodeProto(r, req); err != nil {
		return command.CompleteItemCommand{}, err
	}
	itemID, err := strconv.ParseInt(r.PathValue("itemId"), 10, 64)
	if err != nil {
		return command.CompleteItemCommand{}, fmt.Errorf("item id %q is not a number", r.PathValue("itemId"))
	}
	return command.CompleteItemCommand{ListID: r.PathValue("listId"), ItemID: itemID, Version: req.GetVersion()}, nil
}

func encodeCompleteItem(result command.CompleteItemResult) any {
	return httppipeline.ProtoJSON(&todo.CompleteItemResponse{Item: itemMessage(result.Item)})
}
