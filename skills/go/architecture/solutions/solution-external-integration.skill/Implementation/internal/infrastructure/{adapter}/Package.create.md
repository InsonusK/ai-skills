---
description: The gRPC-client adapter package for the external service (illustrative name; a real consumer names it for the actual external service)
name: internal/infrastructure/{adapter}
element_kind: package
change_kind: create
tags:
  - solution/external-integration
  - element/internal-infrastructure-adapter
---

# Goals
- Implement the domain's outbound port by calling the external service over gRPC.

# Core Principles
- Implements exactly one port's method set — no exported method beyond what `internal/domain/interfaces` declares.
- Every technology-specific error (a gRPC status, a dial failure) is translated to the port's own sentinel error before returning.

# Structure

## Repository place
```
internal/
  infrastructure/
    {adapter}/
```

## Package Structure
```
internal/infrastructure/{adapter}/
  client.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| client.go | `Client` struct implementing the domain's port | [[./client.go.create.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| google.golang.org/grpc | >= 1.83 | client connection to the external service |

# What Does NOT Belong Here
- Business logic — belongs to `internal/domain/services`.

# Allowed Dependencies
- `internal/domain/interfaces` (the port this package implements)
- `gen/{external}` (this external service's generated client stubs)
- `google.golang.org/grpc`, `google.golang.org/grpc/codes`, `google.golang.org/grpc/status`

# Rules

## MUST
- Implement the port's method set exactly — no exported method beyond it.
  - Risk: an adapter-specific method leaking out invites a caller to depend on the concrete adapter instead of the port, defeating the substitution this pattern exists for.
  - Fix: keep every exported method limited to the port's interface; put adapter-only helpers behind unexported functions.
- Never import a sibling `internal/infrastructure/*` package from here.
  - Risk: two adapters depending on each other couples technologies that should be independently replaceable.
  - Fix: if two adapters must coordinate, do it through the domain layer that already depends on both ports.

# Check list
- [ ] `Client` satisfies its domain port (used as that interface type at every call site).
- [ ] No import of another `internal/infrastructure/*` package.
