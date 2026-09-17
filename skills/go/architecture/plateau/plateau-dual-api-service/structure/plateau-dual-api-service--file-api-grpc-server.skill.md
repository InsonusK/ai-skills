---
name: plateau-dual-api-service--file-api-grpc-server
description: internal/api/grpc/server.go of the plateau-dual-api-service plateau
whenToUse: when creating or editing internal/api/grpc/server.go, or adding a new RPC method
domain: skill
type: template
plateau: plateau-dual-api-service
version: 20260917010000
tags:
  - skill/template/file
  - plateau/plateau-dual-api-service
created_by:
  - "[[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
---

# Goal
Translate gRPC calls into calls on `LinkCheckService` and translate results back into the generated response type.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/server.go.create.md|server.go]]

# Core Principles
- Apply ONE plateau template per file.
- Embeds the generated `UnimplementedLinkCheckServiceServer` for forward compatibility.
- Holds the domain service's concrete type — the exact same instance `internal/api/http`'s `Server` holds.

# Implementation
```go
// Skill: file-api-grpc-server
// Plateau: plateau-dual-api-service
// Version: 20260917010000

package grpc

import (
	"context"
	"errors"

	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

	apiv1 "{module-path}/gen/api"
	"{module-path}/internal/domain/services"
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
```
Verified against this plateau's own `example/internal/api/grpc/server.go` — `go build`/`go vet` clean; smoke-tested with `grpcurl`: a well-formed URL returns `200`-equivalent success with the normalized form; an invalid URL returns `codes.InvalidArgument`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/server.go.create.md|server.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `Server` embeds `UnimplementedLinkCheckServiceServer`.
- `Server` holds the same domain-service instance `internal/api/http`'s `Server` holds — never construct a second one.
- Every handler translates every domain sentinel error it knows about to a specific `codes.*` status, defaulting to `codes.Internal`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/server.go.create.md#MUST|server.go]]

# Check list
- [ ] `Server` embeds `UnimplementedLinkCheckServiceServer`.
- [ ] `Server` and `internal/api/http`'s `Server` are constructed from the same domain-service pointer in `main.go`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/server.go.create.md|server.go]]

# Unittest TestCases
- [ ] WHEN the domain service returns `ErrInvalidURL` THEN `Check` returns a `codes.InvalidArgument` status

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/server.go.create.md|server.go]]
