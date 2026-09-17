---
name: plateau-persistent-service--file-api-grpc-server
description: internal/api/grpc/server.go of the plateau-persistent-service plateau
whenToUse: when creating or editing internal/api/grpc/server.go, or adding a new RPC method
domain: skill
type: template
plateau: plateau-persistent-service
version: 20260917040000
tags:
  - skill/template/file
  - plateau/plateau-persistent-service
created_by:
  - "[[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
  - "[[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
registry:
  - "[[../registry/internal-api-grpc-server-go.md|internal-api-grpc-server-go]]"
---

# Goal
Translate gRPC calls into calls on `LinkCheckService` and translate results (including the reputation verdict and the recorded history) back into the generated response types.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/grpc/server.go.extend.md|server.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/grpc/server.go.extend.md|server.go]]

# Core Principles
- Apply ONE plateau template per file.
- Embeds the generated `UnimplementedLinkCheckServiceServer` for forward compatibility.
- Holds the domain service's concrete type — the exact same instance `internal/api/http`'s `Server` holds.
- The history-read RPC returns a bounded, caller-limitable page — never an unbounded dump of the store.

# Implementation
```go
// Skill: file-api-grpc-server
// Plateau: plateau-persistent-service
// Version: 20260917040000

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
```
`proto/linkcheck/linkcheck.proto` gains the matching RPC and messages (`RecentChecksRequest{limit}`, `HistoryEntry{normalized, flagged, reason, checked_at_unix}`, `RecentChecksResponse{repeated HistoryEntry entries}`) — see this plateau's own `example/proto/linkcheck/linkcheck.proto`.

Verified against this plateau's own `example/internal/api/grpc/server.go` — smoke-tested with `grpcurl`: `RecentChecks` after two `Check` calls returned both entries, matching the HTTP adapter's own `GET /v1/links/recent` response and the raw `psql` row contents exactly — confirming both transports share the one domain-service instance and its store.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/grpc/server.go.extend.md|server.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/grpc/server.go.extend.md|server.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `Server` embeds `UnimplementedLinkCheckServiceServer`.
- `Server` holds the same domain-service instance `internal/api/http`'s `Server` holds — never construct a second one.
- Every handler translates every domain sentinel error it knows about to a specific `codes.*` status, defaulting to `codes.Internal`.
- `RecentChecks` accepts an optional, bounded `limit` (default 20 when unset or non-positive) — never an unbounded query.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/server.go.create.md#MUST|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/grpc/server.go.extend.md#MUST|server.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/grpc/server.go.extend.md#MUST|server.go]]

# Check list
- [ ] `Server` embeds `UnimplementedLinkCheckServiceServer`.
- [ ] `Server` and `internal/api/http`'s `Server` are constructed from the same domain-service pointer in `main.go`.
- [ ] The response carries `flagged`/`reason`; the unavailable sentinel maps to `codes.Unavailable`.
- [ ] `RecentChecks` returns entries most-recent-first and respects `limit`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/grpc/server.go.extend.md|server.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/grpc/server.go.extend.md|server.go]]

# Unittest TestCases
- [ ] WHEN the domain service returns `ErrInvalidURL` THEN `Check` returns a `codes.InvalidArgument` status
- [ ] WHEN the domain service returns `interfaces.ErrUnavailable` THEN `Check` returns a `codes.Unavailable` status
- [ ] WHEN `RecentChecks` is called after 2 checks THEN it returns exactly those 2 entries

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/grpc/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/grpc/server.go.extend.md|server.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/grpc/server.go.extend.md|server.go]]
