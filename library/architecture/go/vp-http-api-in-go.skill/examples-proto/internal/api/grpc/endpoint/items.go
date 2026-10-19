package endpoint

import (
	"context"

	"github.com/example/todo/gen/api/todo"
	grpcapi "github.com/example/todo/internal/api/grpc"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

// This file is the ItemsService of the contract: its command handlers, the
// implementation of its generated server interface, and per method the
// mapping of its messages to and from the command. A new method adds a field,
// a method, and its two mappings here.

// ItemHandlers are the command handlers of ItemsService.
type ItemHandlers struct {
	PlanItems pipeline.Handler[command.PlanItems, command.PlanItemsResult]
	AddItem   pipeline.Handler[command.AddItem, command.AddItemResult]
	ListItems pipeline.Handler[command.ListItems, command.ListItemsResult]
}

func itemsService(h ItemHandlers) grpcapi.Service {
	return grpcapi.Service{Desc: &todo.ItemsService_ServiceDesc, Impl: itemsServer{h: h}}
}

// itemsServer implements todo.ItemsServiceServer. It embeds the generated
// Unsafe interface, not UnimplementedItemsServiceServer, so a method added to
// the proto file and not written here fails the build (the assertion below)
// instead of answering UNIMPLEMENTED at run time.
type itemsServer struct {
	todo.UnsafeItemsServiceServer
	h ItemHandlers
}

var _ todo.ItemsServiceServer = itemsServer{}

func (s itemsServer) PlanItems(ctx context.Context, req *todo.PlanItemsRequest) (*todo.PlanItemsResponse, error) {
	return grpcapi.Call(ctx, s.h.PlanItems, req, decodePlanItems, encodePlanItems)
}

func (s itemsServer) AddItem(ctx context.Context, req *todo.AddItemRequest) (*todo.AddItemResponse, error) {
	return grpcapi.Call(ctx, s.h.AddItem, req, decodeAddItem, encodeAddItem)
}

func (s itemsServer) ListItems(ctx context.Context, req *todo.ListItemsRequest) (*todo.ListItemsResponse, error) {
	return grpcapi.Call(ctx, s.h.ListItems, req, decodeListItems, encodeListItems)
}

// ClearList is deprecated since 1.1.0: no command, the same answer as its HTTP form.
func (s itemsServer) ClearList(context.Context, *todo.ClearListRequest) (*todo.ClearListResponse, error) {
	return nil, grpcapi.Unimplemented("ClearList is deprecated since 1.1.0")
}

func decodePlanItems(req *todo.PlanItemsRequest) (command.PlanItems, error) {
	return command.PlanItems{Titles: req.GetTitles()}, nil
}

func encodePlanItems(result command.PlanItemsResult) *todo.PlanItemsResponse {
	return &todo.PlanItemsResponse{Items: itemMessages(result.Items)}
}

func decodeAddItem(req *todo.AddItemRequest) (command.AddItem, error) {
	return command.AddItem{ListID: req.GetListId(), Title: req.GetTitle()}, nil
}

func encodeAddItem(result command.AddItemResult) *todo.AddItemResponse {
	return &todo.AddItemResponse{Item: &todo.Item{Title: result.Item.Title, Done: result.Item.Done}}
}

func decodeListItems(req *todo.ListItemsRequest) (command.ListItems, error) {
	return command.ListItems{ListID: req.GetListId()}, nil
}

func encodeListItems(result command.ListItemsResult) *todo.ListItemsResponse {
	return &todo.ListItemsResponse{Items: itemMessages(result.Items)}
}

// itemMessages maps the command layer's items to the contract's messages.
func itemMessages(views []command.ItemView) []*todo.Item {
	items := make([]*todo.Item, len(views))
	for i, view := range views {
		items[i] = &todo.Item{Title: view.Title, Done: view.Done}
	}
	return items
}
