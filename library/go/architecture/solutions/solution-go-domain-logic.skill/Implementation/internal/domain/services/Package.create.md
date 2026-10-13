---
description: The domain layer — business logic, transport- and infrastructure-agnostic
name: internal/domain/services
element_kind: package
change_kind: create
tags:
  - solution/go-domain-logic
  - element/internal-domain-services
---

# Goals
- Hold the module's business logic independent of the transport that triggers it or the infrastructure it may later rely on.

# Core Principles
- No type from `internal/api/*` or any specific `internal/infrastructure/*` adapter is ever imported here — only `internal/domain/interfaces` (once it exists) and the standard library.

# Structure

## Repository place
```
internal/
  domain/
    services/
```

## Package Structure
```
internal/domain/services/
  {service}.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| {service}.go | One domain service: a struct holding only the ports it needs, plus its methods | [[./{service}.go.create.md]] |

# What Does NOT Belong Here
- Request/response DTOs for any specific transport — belong to the adapter package that defines them (`internal/api/http`, `internal/api/grpc`).
- A concrete infrastructure type (a Redis client, a `*sql.DB`, a generated gRPC client) — belongs to the `internal/infrastructure/*` package that implements the port this service depends on.

# Allowed Dependencies
- `internal/domain/interfaces` (once a port-needing solution creates it)
- Standard library

# Rules

## MUST
- A domain service's constructor takes only ports (interfaces from `internal/domain/interfaces`) as arguments, never a concrete type from `internal/infrastructure`.
  - Risk: a concrete infrastructure argument couples the domain to one specific technology and makes the service untestable without that technology running.
  - Fix: depend on the port; let `main.go` pass a concrete adapter that satisfies it.
- Never import a type from `internal/api/*` here.
  - Risk: a transport type in the domain layer makes the business logic answer to a specific transport's shape instead of the transport translating to and from the domain's own shape.
  - Fix: keep the domain service's method signatures in plain Go types; let each adapter do its own translation.

# Check list
- [ ] No import from `internal/api/*` or a named `internal/infrastructure/*` package anywhere in this package.
