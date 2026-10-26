// Package service holds the services: each wraps one infrastructure and
// speaks the application's language, hiding the technique behind it.
package service

import (
	"context"
	"errors"
	"fmt"

	"google.golang.org/grpc/codes"

	"github.com/example/todo/internal/domain/entity"
	"github.com/example/todo/internal/infrastructure/client/core/grpcclient"
	"github.com/example/todo/internal/infrastructure/client/todoapi"
)

// todoAPI is what RemotePlanner needs of its infrastructure; todoapi.Client has it.
type todoAPI interface {
	PlanItems(ctx context.Context, titles []string) ([]todoapi.Item, error)
}

// RemotePlanner plans items with the todo service. It turns the API's
// technical outcomes into the application's error kinds: an API that does not
// answer is unavailable; titles the API rejects are invalid, with its
// message; any other answer is a failure of this service.
type RemotePlanner struct {
	api todoAPI
}

func NewRemotePlanner(api todoAPI) *RemotePlanner {
	return &RemotePlanner{api: api}
}

// Plan returns one open item per title, planned by the todo service.
func (s *RemotePlanner) Plan(ctx context.Context, titles []string) ([]entity.Item, error) {
	answered, err := s.api.PlanItems(ctx, titles)
	var status *grpcclient.StatusError
	switch {
	case errors.Is(err, grpcclient.ErrUnavailable):
		return nil, fmt.Errorf("%w: the todo service: %v", entity.ErrUnavailable, err)
	case errors.As(err, &status) && status.Code == codes.InvalidArgument:
		return nil, fmt.Errorf("%w: the todo service rejected the titles: %s", entity.ErrInvalid, status.Message)
	case err != nil:
		return nil, fmt.Errorf("the todo service: %w", err)
	}
	items := make([]entity.Item, len(answered))
	for i, item := range answered {
		planned, err := entity.NewItem(item.Title)
		if err != nil {
			// %v, not %w: an item the other service got wrong is a failure of
			// this one, never the caller's invalid input.
			return nil, fmt.Errorf("the todo service answered the item %q: %v", item.Title, err)
		}
		if item.Done {
			if err := planned.Complete(); err != nil {
				return nil, err
			}
		}
		items[i] = planned
	}
	return items, nil
}
