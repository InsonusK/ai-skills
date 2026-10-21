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
	PlanItems pipeline.Handler[command.PlanItems, command.PlanItemsResult]
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

func decodePlanItems(req *todo.PlanItemsRequest) (command.PlanItems, error) {
	return command.PlanItems{Titles: req.GetTitles()}, nil
}

func encodePlanItems(result command.PlanItemsResult) *todo.PlanItemsResponse {
	items := make([]*todo.Item, len(result.Items))
	for i, item := range result.Items {
		items[i] = &todo.Item{Title: item.Title, Done: item.Done}
	}
	return &todo.PlanItemsResponse{Items: items}
}
