// Package grpc is the inbound gRPC adapter: it decodes requests, calls the
// domain service, and encodes the result back as the generated response
// type.
package grpc

import (
	"context"
	"errors"

	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

	apiv1 "github.com/example/linkcheck-service/gen/api"
	"github.com/example/linkcheck-service/internal/domain/interfaces"
	"github.com/example/linkcheck-service/internal/domain/services"
)

type Server struct {
	apiv1.UnimplementedLinkCheckServiceServer
	service *services.LinkCheckService
}

func New(service *services.LinkCheckService) *Server {
	return &Server{service: service}
}

func (s *Server) Check(ctx context.Context, req *apiv1.CheckRequest) (*apiv1.CheckResponse, error) {
	result, err := s.service.Check(ctx, req.GetUrl())
	if err != nil {
		return nil, toStatus(err)
	}
	return &apiv1.CheckResponse{
		Url:        result.URL,
		Normalized: result.Normalized,
		Flagged:    result.Flagged,
		Reason:     result.Reason,
	}, nil
}

func (s *Server) RecentChecks(ctx context.Context, req *apiv1.RecentChecksRequest) (*apiv1.RecentChecksResponse, error) {
	limit := int(req.GetLimit())
	if limit <= 0 {
		limit = 20
	}

	entries, err := s.service.RecentChecks(ctx, limit)
	if err != nil {
		return nil, toStatus(err)
	}

	out := make([]*apiv1.HistoryEntry, len(entries))
	for i, e := range entries {
		out[i] = &apiv1.HistoryEntry{
			Normalized:    e.Normalized,
			Flagged:       e.Flagged,
			Reason:        e.Reason,
			CheckedAtUnix: e.CheckedAt.Unix(),
		}
	}
	return &apiv1.RecentChecksResponse{Entries: out}, nil
}

func toStatus(err error) error {
	switch {
	case errors.Is(err, services.ErrInvalidURL):
		return status.Error(codes.InvalidArgument, err.Error())
	case errors.Is(err, interfaces.ErrUnavailable):
		return status.Error(codes.Unavailable, err.Error())
	default:
		return status.Error(codes.Internal, err.Error())
	}
}
