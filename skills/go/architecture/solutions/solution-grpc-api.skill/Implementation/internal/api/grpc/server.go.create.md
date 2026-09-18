---
description: Server struct — thin gRPC adapter over the domain service
project_name: internal/api/grpc
name: Server
element_kind: struct
change_kind: create
tags:
  - solution/grpc-api
  - element/internal-api-grpc-server-go
---

# Goals
- Translate gRPC calls into calls on the shared domain service and translate its results back into the generated response type.

# Core Principles
- Embeds the generated `Unimplemented{Service}Server` type for forward compatibility with future proto changes.
- Holds the domain service's concrete type — the exact same instance `internal/api/http`'s `Server` holds.

# Naming convention
| use case | struct name pattern | struct name | file name pattern | file name |
| -------- | -------------------- | ------------ | ------------------- | --------- |
| the gRPC adapter | `Server` | `Server` | `server.go` | `server.go` |

# Implementation changes
```go
// Package grpc is the inbound gRPC adapter: it decodes requests, calls the
// domain service, and encodes the result back as the generated response
// type.
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
	apiv1.Unimplemented{Service}Server
	service *services.{Service}
}

func New(service *services.{Service}) *Server {
	return &Server{service: service}
}

func (s *Server) {Method}(ctx context.Context, req *apiv1.{Method}Request) (*apiv1.{Method}Response, error) {
	if err := s.service.{Method}(ctx); err != nil {
		return nil, toStatus(err)
	}
	return &apiv1.{Method}Response{}, nil
}

func toStatus(err error) error {
	// Example: map a domain sentinel error to a specific gRPC status.
	// if errors.Is(err, interfaces.ErrNotFound) {
	// 	return status.Error(codes.NotFound, err.Error())
	// }
	return status.Error(codes.Internal, err.Error())
}
```

This catalog's own runnable examples give `{Method}` a real request/response shape (e.g. `Check(ctx, *CheckRequest) (*CheckResponse, error)`) — see `plateau-dual-api-service`'s `example/`.

# Rule changes

## MUST
- `Server` must embed `apiv1.Unimplemented{Service}Server`.
  - Risk: without it, adding a new RPC to the `.proto` later breaks every existing `Server` at compile time instead of failing only the new, genuinely-unimplemented method at run time.
  - Fix: always embed the generated `Unimplemented*` type.
- `Server` must hold the same domain-service instance `internal/api/http`'s `Server` holds — never construct a second one.
  - Risk: two independently-constructed instances can drift, so behavior differs by which transport handled the call.
  - Fix: `main.go` constructs the domain service once and passes the same pointer to both adapters.
- Every handler must translate every domain sentinel error it knows about to a specific `codes.*` status, defaulting to `codes.Internal`.
  - Risk: returning `codes.Internal` (or a bare Go error) for every failure gives a gRPC client no way to distinguish a client mistake from a server fault.
  - Fix: `errors.Is` against the domain's sentinel errors first.

# Check list
- [ ] `Server` embeds `Unimplemented{Service}Server`.
- [ ] `Server` and `internal/api/http`'s `Server` are constructed from the same domain-service pointer in `main.go`.

# Unittest TestCases
- [ ] WHEN the domain service returns its not-found sentinel error THEN the method returns a `codes.NotFound` status
