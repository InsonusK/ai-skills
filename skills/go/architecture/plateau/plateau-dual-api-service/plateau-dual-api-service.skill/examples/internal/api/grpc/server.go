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
	return &apiv1.CheckResponse{Url: result.URL, Normalized: result.Normalized}, nil
}

func toStatus(err error) error {
	if errors.Is(err, services.ErrInvalidURL) {
		return status.Error(codes.InvalidArgument, err.Error())
	}
	return status.Error(codes.Internal, err.Error())
}
