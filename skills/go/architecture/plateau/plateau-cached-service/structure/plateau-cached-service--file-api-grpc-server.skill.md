---
name: plateau-cached-service--file-api-grpc-server
description: internal/api/grpc/server.go of the plateau-cached-service plateau
whenToUse: when creating or editing internal/api/grpc/server.go, or adding a new RPC method
domain: skill
type: template
plateau: plateau-cached-service
version: 20260917030000
tags:
  - skill/template/file
  - plateau/plateau-cached-service
created_by:
  - "[[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
  - "[[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
---

# Goal
Translate gRPC calls into calls on `LinkCheckService` and translate results (including the reputation verdict) back into the generated response type.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/grpc/server.go.extend.md|server.go]]

# Core Principles
- Apply ONE plateau template per file.
- Embeds the generated `UnimplementedLinkCheckServiceServer` for forward compatibility.
- Holds the domain service's concrete type — the exact same instance `internal/api/http`'s `Server` holds.

# Implementation
```go
// Skill: file-api-grpc-server
// Plateau: plateau-cached-service
// Version: 20260917030000

package grpc

import (
	"context"
	"errors"

	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

	apiv1 "{module-path}/gen/api"
	"{module-path}/internal/domain/interfaces"
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
	return &apiv1.CheckResponse{
		Url:        result.URL,
		Normalized: result.Normalized,
		Flagged:    result.Flagged,
		Reason:     result.Reason,
	}, nil
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
```
Verified against this plateau's own `example/internal/api/grpc/server.go` — smoke-tested with `grpcurl` against a real (throwaway) fake reputation server: a flagged URL returns `flagged: true` with the reason; the reputation service stopped returns `codes.Unavailable`; an invalid URL returns `codes.InvalidArgument`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/grpc/server.go.extend.md|server.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `Server` embeds `UnimplementedLinkCheckServiceServer`.
- `Server` holds the same domain-service instance `internal/api/http`'s `Server` holds — never construct a second one.
- Every handler translates every domain sentinel error it knows about to a specific `codes.*` status, defaulting to `codes.Internal`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/server.go.create.md#MUST|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/grpc/server.go.extend.md#MUST|server.go]]

# Check list
- [ ] `Server` embeds `UnimplementedLinkCheckServiceServer`.
- [ ] `Server` and `internal/api/http`'s `Server` are constructed from the same domain-service pointer in `main.go`.
- [ ] The response carries `flagged`/`reason`; the unavailable sentinel maps to `codes.Unavailable`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/grpc/server.go.extend.md|server.go]]

# Unittest TestCases
- [ ] WHEN the domain service returns `ErrInvalidURL` THEN `Check` returns a `codes.InvalidArgument` status
- [ ] WHEN the domain service returns `interfaces.ErrUnavailable` THEN `Check` returns a `codes.Unavailable` status

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/grpc/server.go.extend.md|server.go]]
