---
description: The inbound gRPC adapter package
name: internal/api/grpc
element_kind: package
change_kind: create
tags:
  - solution/grpc-api
  - element/internal-api-grpc
---

# Goals
- Expose the domain service's capabilities over gRPC, using the same domain calls the HTTP adapter uses.

# Core Principles
- No business rule or decision lives in this package.

# Structure

## Repository place
```
internal/
  api/
    grpc/
```

## Package Structure
```
internal/api/grpc/
  server.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| server.go | `Server` struct implementing the generated service interface | [[./server.go.create.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| google.golang.org/grpc | >= 1.83 | hosts the generated service |
| google.golang.org/protobuf | >= 1.36 | generated message types |

# What Does NOT Belong Here
- Business logic or validation beyond decoding — belongs to `internal/domain/services`.

# Allowed Dependencies
- `internal/domain/services`
- `internal/domain/interfaces` (for sentinel errors, once it exists)
- `gen/api` (this module's own generated stubs)
- `google.golang.org/grpc`, `google.golang.org/grpc/codes`, `google.golang.org/grpc/status`

# Rules

## MUST
- Never call anything under `internal/infrastructure` directly from this package.
  - Risk: a gRPC handler that reaches past the domain service into infrastructure duplicates whatever the domain service already orchestrates.
  - Fix: route every request through the domain service.

# Check list
- [ ] No import of any `internal/infrastructure/*` package.
