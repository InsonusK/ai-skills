package endpoint

import (
	"net/http"

	"github.com/example/todo/gen/api/todo"
	httpapi "github.com/example/todo/internal/api/http"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

// This file is the lists resource (/v1/lists/{listId}/...). The path
// parameter is named as the generated spec names it - listId, the
// lowerCamelCase form of the proto field list_id - and the route's decode
// reads it with r.PathValue, since it is not in the body.

// ListHandlers are the command handlers of the lists resource.
type ListHandlers struct {
	AddItem   pipeline.Handler[command.AddItem, command.AddItemResult]
	ListItems pipeline.Handler[command.ListItems, command.ListItemsResult]
}

// listRoutes are the lists operations of the HTTP form.
func listRoutes(h ListHandlers) []httpapi.Route {
	return []httpapi.Route{
		// 201: the item is created and kept.
		{Pattern: "POST /v1/lists/{listId}/items", Handler: httpapi.Endpoint(h.AddItem, decodeAddItem, encodeAddItem, http.StatusCreated)},
		{Pattern: "GET /v1/lists/{listId}/items", Handler: httpapi.Endpoint(h.ListItems, decodeListItems, encodeListItems, http.StatusOK)},
		// ClearList is deprecated since 1.1.0: no command, the same answer as its gRPC form.
		{Pattern: "POST /v1/lists/{listId}/clear", Handler: httpapi.Unimplemented("ClearList is deprecated since 1.1.0")},
	}
}

func decodeAddItem(r *http.Request) (command.AddItem, error) {
	req := &todo.AddItemRequest{}
	if err := httpapi.DecodeProto(r, req); err != nil {
		return command.AddItem{}, err
	}
	return command.AddItem{ListID: r.PathValue("listId"), Title: req.GetTitle()}, nil
}

func encodeAddItem(result command.AddItemResult) any {
	return httpapi.ProtoJSON(&todo.AddItemResponse{Item: &todo.Item{Title: result.Item.Title, Done: result.Item.Done}})
}

// decodeListItems reads only the path: a GET has no body.
func decodeListItems(r *http.Request) (command.ListItems, error) {
	return command.ListItems{ListID: r.PathValue("listId")}, nil
}

func encodeListItems(result command.ListItemsResult) any {
	return httpapi.ProtoJSON(&todo.ListItemsResponse{Items: itemMessages(result.Items)})
}
