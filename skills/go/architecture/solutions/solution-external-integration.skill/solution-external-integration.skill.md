---
name: solution-external-integration
description: Calls out to another service through a narrow, business-named outbound port declared in the domain and implemented by an infrastructure adapter, demonstrated over gRPC
whenToUse: when the module needs to call another service to do its job, or reviewing whether a domain service reaches for a concrete client library instead of a port it declares itself
domain: skill
type: architecture
version: 20260917000000
tags:
  - skill/architecture/solution
  - solution/external-integration
  - stack/go
  - concern/architecture
creates:
  - "internal/domain/interfaces/{port}.go"
  - "internal/infrastructure/{adapter}/"
  - "proto/{external}/v1/{external}.proto"
extends:
  - "internal/domain/services/{service}.go"
  - "cmd/{service}/main.go"
  - "internal/config/config.go"
  - "Makefile"
depends_on:
  - "[[skills/go/architecture/solutions/solution-go-domain-ports.skill/solution-go-domain-ports.skill.md|solution-go-domain-ports]]"
built_on_plateau:
adr:
---

# Goal
- Give the domain a business-named outbound port for "call this other service," implemented by an infrastructure adapter — demonstrated over gRPC, but the port's own shape does not name the transport.
- Keep the domain service testable without the external service reachable, by depending only on the port's interface.

# Capabilities
- The domain service's business logic can now depend on a capability only another service provides, without importing that service's client library, generated stubs, or transport concerns.
- Swapping the external service's transport (or replacing it with a mock, in tests) never touches `internal/domain/services`.

# Core Principles
- The port is named for what the domain needs (e.g. `ReputationChecker`), never for the technology or the external service's own name.
- The adapter translates every technology-specific error (a gRPC "unavailable" status, a timeout) into the port's own sentinel errors — nothing outside `internal/infrastructure/{adapter}` ever sees a `google.golang.org/grpc` type.
- This module's own exposed contract (if `solution-grpc-api` is also applied) and the external service's contract this solution calls are never generated into the same Go package — see that solution's own rule on this.

# Requirements
SOLUTION:
- [[skills/go/architecture/solutions/solution-go-domain-ports.skill/solution-go-domain-ports.skill.md|solution-go-domain-ports]]
  - [[skills/go/architecture/solutions/solution-go-domain-ports.skill/Implementation/internal/domain/interfaces/Package.create.md|internal/domain/interfaces]] - the package this solution adds its port file to
GO MODULES:
- google.golang.org/grpc
  - `grpc.NewClient` — dials the external service
- google.golang.org/protobuf
  - generated client stubs for the external service's contract

# Template Skill Mutations
FILES:
- [[./Implementation/Repository.extend.md|Repository]] - extend - `proto/{external}/v1/{external}.proto`, buf codegen config, `Makefile`'s `proto-gen` target
- [[./Implementation/internal/domain/interfaces/{port}.go.create.md|internal/domain/interfaces/{port}.go]] - create - the outbound port
- [[./Implementation/internal/domain/services/{service}.go.extend.md|internal/domain/services/{service}.go]] - extend - the domain service depends on the new port
- [[./Implementation/internal/infrastructure/{adapter}/Package.create.md|internal/infrastructure/{adapter}]] - create - the gRPC-client adapter package
- [[./Implementation/internal/infrastructure/{adapter}/client.go.create.md|client.go]] - create - `Client` struct implementing the port
- [[./Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]] - extend - construct the client and pass it to the domain service
- [[./Implementation/internal/config/config.go.extend.md|internal/config/config.go]] - extend - add the external service's host/port/TLS settings

# Workflow

## Call the external service (happy path)
1. The domain service calls its port's method.
2. The infrastructure adapter's method calls the generated gRPC client.
3. The adapter maps the response into a plain domain type and returns it.

## External service unavailable
1. The gRPC call fails with `codes.Unavailable`.
2. The adapter maps it to the port's own sentinel error (e.g. `ErrUnavailable`).
3. The domain service returns that error, wrapped, to its caller — no gRPC-specific type crosses into the domain or any inbound adapter.

# Rules

## MUST
- [[./Implementation/Repository.extend.md#MUST|Repository]]
- [[./Implementation/internal/domain/interfaces/{port}.go.create.md#MUST|internal/domain/interfaces/{port}.go]]
- [[./Implementation/internal/infrastructure/{adapter}/Package.create.md#MUST|internal/infrastructure/{adapter}]]
- [[./Implementation/internal/infrastructure/{adapter}/client.go.create.md#MUST|client.go]]

# Check list
- [ ] `internal/domain/services` imports the new port's interface, never the adapter package or a `google.golang.org/grpc` type.
- [ ] The adapter translates every distinguishable failure mode it knows about into a port-declared sentinel error.
- [ ] `main.go` constructs the client once and passes it to the domain service as the port's interface type.
