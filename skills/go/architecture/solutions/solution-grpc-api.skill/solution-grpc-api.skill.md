---
name: solution-grpc-api
description: A second inbound entry point over gRPC, alongside the base HTTP API, sharing the same domain-service instance and translating the same domain sentinel errors into gRPC status codes
whenToUse: when a Go web-service needs to expose its capabilities over gRPC in addition to HTTP, or reviewing whether a gRPC handler duplicates logic the HTTP handler already has instead of sharing the domain service
domain: skill
type: architecture
version: 20260917000000
tags:
  - skill/architecture/solution
  - solution/grpc-api
  - stack/go
  - concern/architecture
creates:
  - "internal/api/grpc/"
  - "proto/{service}/v1/{service}.proto"
  - "buf/buf.gen.yaml"
extends:
  - "cmd/{service}/main.go"
  - "internal/config/config.go"
  - "Makefile"
depends_on:
  - "[[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
built_on_plateau:
adr:
---

# Goal
- Give the module a second inbound entry point, over gRPC, exposing the same domain calls `solution-go-http-api` already exposes over HTTP — never a second copy of the logic.
- Run the gRPC server and the HTTP server concurrently, both shutting down cleanly on the same signal.

# Capabilities
- Callers that prefer gRPC's binary contract, typed stubs, or streaming get the same domain behavior HTTP callers get.
- One domain-service instance backs both transports — a bug fix or new field in the domain service is visible to both without touching either adapter's own logic.

# Core Principles
- The gRPC adapter is exactly as thin as the HTTP adapter: decode, call the domain service, encode — no business logic.
- `proto/{service}/v1/{service}.proto` is this module's own exposed contract — a client-side contract for calling *another* module's gRPC API is a different concern, [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]'s, and lives in its own `proto/` subtree so the two never collide on one generated Go package.
- The moment a second concurrent long-running loop exists (gRPC alongside HTTP), `run()` is structured as an `errgroup.Group` — never two sequential blocking calls.

# Requirements
SOLUTION:
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]
  - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/cmd/{service}/main.go.extend.md|main.go]] - the single-server `run()` this solution converts to an `errgroup.Group`
GO MODULES:
- google.golang.org/grpc
  - `grpc.NewServer`, `grpc.NewClient` — hosts the generated service implementation
- google.golang.org/protobuf
  - generated message/service stubs this solution's `.proto` compiles to
- golang.org/x/sync/errgroup
  - runs the gRPC and HTTP serve loops (and their shared shutdown goroutine) together

# Template Skill Mutations
FILES:
- [[./Implementation/Repository.extend.md|Repository]] - extend - `proto/{service}/v1/{service}.proto`, `buf/buf.gen.yaml`, `Makefile`'s `proto-gen` target
- [[./Implementation/internal/api/grpc/Package.create.md|internal/api/grpc]] - create - the gRPC adapter package
- [[./Implementation/internal/api/grpc/server.go.create.md|server.go]] - create - `Server` struct implementing the generated service interface
- [[./Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]] - extend - convert `run()` to an `errgroup.Group` running both servers
- [[./Implementation/internal/config/config.go.extend.md|internal/config/config.go]] - extend - add `GRPCListenPort`

# Workflow

## Handle a gRPC call (happy path)
1. A client calls the generated gRPC method.
2. `Server.{Method}` decodes the request into domain arguments.
3. `Server.{Method}` calls the same domain-service instance the HTTP adapter calls.
4. `Server.{Method}` maps the result into the generated response message.

## Domain error translation
1. The domain service returns an error wrapping one of its sentinel errors.
2. `Server.{Method}` maps it to the matching `codes.*` gRPC status via `errors.Is`; anything else maps to `codes.Internal`.

# Rules

## MUST
- [[./Implementation/Repository.extend.md#MUST|Repository]]
- [[./Implementation/internal/api/grpc/Package.create.md#MUST|internal/api/grpc]]
- [[./Implementation/internal/api/grpc/server.go.create.md#MUST|server.go]]
- [[./Implementation/cmd/{service}/main.go.extend.md#MUST|cmd/{service}/main.go]]

# Check list
- [ ] `internal/api/grpc` and `internal/api/http` both call the same domain-service instance constructed once in `main.go`.
- [ ] `make proto-gen` regenerates `gen/api` from `proto/{service}/v1/{service}.proto` with no manual edits needed afterward.
- [ ] `run()` starts both servers concurrently via `errgroup.Group` and both stop on the same shutdown signal.
