// Package mock is the mock of the todo API's client (todoapi): an emulator
// of the API inside the process, which TODO_API_HOST=mock runs the client
// against, so the application runs with no todo service reachable.
package mock

import (
	"context"

	"google.golang.org/grpc"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

	"github.com/example/todo/gen/client/todo"
)

// Emulator registers the todo API's services, answering every method of
// todoapi.Methods as the todo API does, with stable data and no state. A new
// method of the adapter adds its method to emulator.
func Emulator(server grpc.ServiceRegistrar) {
	todo.RegisterItemsServiceServer(server, emulator{})
}

// emulator is ItemsService inside the process; a method it does not write
// answers UNIMPLEMENTED.
type emulator struct {
	todo.UnimplementedItemsServiceServer
}

func (emulator) PlanItems(_ context.Context, request *todo.PlanItemsRequest) (*todo.PlanItemsResponse, error) {
	if len(request.GetTitles()) == 0 {
		return nil, status.Error(codes.InvalidArgument, "no items to plan")
	}
	items := make([]*todo.Item, len(request.GetTitles()))
	for i, title := range request.GetTitles() {
		items[i] = &todo.Item{Title: title, Done: false}
	}
	return &todo.PlanItemsResponse{Items: items}, nil
}
