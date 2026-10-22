package endpoint

import (
	"context"

	"github.com/example/todo/gen/api/todo"
	grpcpipeline "github.com/example/todo/internal/api/grpc/pipeline"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

// This file is the ItemsService of the contract: its command handlers, the
// implementation of its generated server interface, and per method the
// mapping of its messages to and from the command. A new method adds a field,
// a method, and its two mappings here.

// ItemsHandler is the ItemsService of the contract: the command handlers its
// methods call. The composition root fills it; Service registers it.
type ItemsHandler struct {
	PlanItems pipeline.Handler[command.PlanItemsCommand, command.PlanItemsResult]
	AddItem   pipeline.Handler[command.AddItemCommand, command.AddItemResult]
	ListItems pipeline.Handler[command.ListItemsCommand, command.ListItemsResult]
}

// Service is ItemsService as the server registers it.
func (h ItemsHandler) Service() grpcpipeline.Service {
	return grpcpipeline.Service{Desc: &todo.ItemsService_ServiceDesc, Impl: itemsServer{h: h}}
}

// itemsServer implements todo.ItemsServiceServer. It embeds the generated
// Unsafe interface, not UnimplementedItemsServiceServer, so a method added to
// the proto file and not written here fails the build (the assertion below)
// instead of answering UNIMPLEMENTED at run time.
type itemsServer struct {
	todo.UnsafeItemsServiceServer
	h ItemsHandler
}

var _ todo.ItemsServiceServer = itemsServer{}

func (s itemsServer) PlanItems(ctx context.Context, req *todo.PlanItemsRequest) (*todo.PlanItemsResponse, error) {
	return grpcpipeline.Call(ctx, s.h.PlanItems, req, decodePlanItems, encodePlanItems)
}

func (s itemsServer) AddItem(ctx context.Context, req *todo.AddItemRequest) (*todo.AddItemResponse, error) {
	return grpcpipeline.Call(ctx, s.h.AddItem, req, decodeAddItem, encodeAddItem)
}

func (s itemsServer) ListItems(ctx context.Context, req *todo.ListItemsRequest) (*todo.ListItemsResponse, error) {
	return grpcpipeline.Call(ctx, s.h.ListItems, req, decodeListItems, encodeListItems)
}

// ClearList is deprecated since 1.1.0: no command, the same answer as its HTTP form.
func (s itemsServer) ClearList(context.Context, *todo.ClearListRequest) (*todo.ClearListResponse, error) {
	return nil, grpcpipeline.Unimplemented("ClearList is deprecated since 1.1.0")
}

func decodePlanItems(req *todo.PlanItemsRequest) (command.PlanItemsCommand, error) {
	return command.PlanItemsCommand{Titles: req.GetTitles()}, nil
}

func encodePlanItems(result command.PlanItemsResult) *todo.PlanItemsResponse {
	return &todo.PlanItemsResponse{Items: itemMessages(result.Items)}
}

func decodeAddItem(req *todo.AddItemRequest) (command.AddItemCommand, error) {
	return command.AddItemCommand{ListID: req.GetListId(), Title: req.GetTitle()}, nil
}

func encodeAddItem(result command.AddItemResult) *todo.AddItemResponse {
	return &todo.AddItemResponse{Item: &todo.Item{Title: result.Item.Title, Done: result.Item.Done}}
}

func decodeListItems(req *todo.ListItemsRequest) (command.ListItemsCommand, error) {
	return command.ListItemsCommand{ListID: req.GetListId()}, nil
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
