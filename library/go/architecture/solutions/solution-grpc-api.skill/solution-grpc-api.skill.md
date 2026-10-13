---
name: solution-grpc-api
description: A second inbound entry point over gRPC, alongside the base HTTP API — the proto file becomes the service's one contract, the OpenAPI spec and the Go code are generated from it, the HTTP adapter moves onto the generated messages, and both adapters share one domain-service instance
whenToUse: when a Go web-service needs to expose its capabilities over gRPC in addition to HTTP, when adding or changing a method of a service that has a proto contract, or reviewing whether a gRPC handler duplicates logic the HTTP handler already has instead of sharing the domain service
domain: skill
type: architecture
version: 20261012000000
tags:
  - skill/architecture/solution
  - solution/grpc-api
  - stack/go
  - concern/architecture
creates:
  - "internal/api/grpc/"
  - "docs/integration/api/{contract-name}/{file}.proto"
  - "buf.yaml"
  - "buf.gen.yaml"
  - "gen/api/{contract-name}/"
  - "internal/api/http/protojson.go"
  - "tools/testing/kinds/generated.sh"
extends:
  - "cmd/{service}/main.go"
  - "internal/config/config.go"
  - "internal/api/http/server.go"
  - "docs/integration/api/{contract-name}/"
  - "Makefile"
  - "go.mod"
depends_on:
  - "[[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
built_on_plateau:
adr:
  - adr/http-form-over-generated-messages.md
---

# Goal
- Give the module a second inbound entry point, over gRPC, exposing the same domain calls `solution-go-http-api` already exposes over HTTP — never a second copy of the logic.
- `docs/integration/api/{contract-name}/{file}.proto` as the service's one hand-written contract, in place of the baseline's `openapi.json`; `{file}.swagger.json` and `gen/api/{contract-name}/*.pb.go` generated from it and committed.
- `internal/api/http/` reading and writing the generated messages through `protojson`, in place of its own structs and `errors.go`.
- `tools/testing/kinds/generated.sh`, an unchanged copy of the file of `api-develop-in-go`.
- The gRPC server and the HTTP server running concurrently, both shutting down cleanly on the same signal.

# Capabilities
- Callers that prefer gRPC's binary contract, typed stubs, or streaming get the same domain behavior HTTP callers get.
- One domain-service instance backs both transports — a bug fix or new field in the domain service is visible to both without touching either adapter's own logic.

# Core Principles
- The gRPC adapter is exactly as thin as the HTTP adapter: decode, call the domain service, encode — no business logic.
- The proto file is this module's own exposed contract and lives under `docs/integration/api/`; the contract of *another* module's gRPC API is [[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]'s and lives under `docs/integration/client/`, generated into `gen/client/` — the two never share a Go package.
- One contract, two transports: the HTTP form is the proto file's `google.api.http` annotations, served by hand-written handlers over the generated messages — decision recorded in [[./adr/http-form-over-generated-messages.md|http-form-over-generated-messages]]. Layout, generation, and the `generated` kind are those of [[library/common-workflow/develop/api-develop-in-go.skill/api-develop-in-go.skill.md|api-develop-in-go]].
- What a client of the HTTP API reads does not change when this solution is composed: the baseline already writes the JSON `protojson` writes.
- The moment a second concurrent long-running loop exists (gRPC alongside HTTP), `run()` is structured as an `errgroup.Group` — never two sequential blocking calls.

# Requirements
SOLUTION:
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]
  - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/cmd/{service}/main.go.extend.md|main.go]] - the single-server `run()` this solution converts to an `errgroup.Group`
GO MODULES:
- google.golang.org/grpc
  - `grpc.NewServer`, `grpc.NewClient` — hosts the generated service implementation
- google.golang.org/protobuf
  - generated message/service stubs this solution's `.proto` compiles to
- golang.org/x/sync/errgroup
  - runs the gRPC and HTTP serve loops (and their shared shutdown goroutine) together
- github.com/grpc-ecosystem/grpc-gateway/v2
  - only its `protoc-gen-openapiv2` generator and the option messages the generated code imports — no gateway runs
GO TOOLS (`tool` lines of `go.mod`, run as `go tool …`):
- github.com/bufbuild/buf/cmd/buf
- google.golang.org/protobuf/cmd/protoc-gen-go
- google.golang.org/grpc/cmd/protoc-gen-go-grpc
- github.com/grpc-ecosystem/grpc-gateway/v2/protoc-gen-openapiv2

# Template Skill Mutations
FILES:
- [[./Implementation/Repository.extend.md|Repository]] - extend - `buf.yaml`, `buf.gen.yaml`, the `tool` lines of `go.mod`, `Makefile`'s `proto-gen` target, `gen/api/`, the `generated` test kind
- [[./Implementation/docs/integration/api/{contract-name}/Package.extend.md|docs/integration/api/{contract-name}]] - extend - the proto file and its generated spec replace `openapi.json`
- [[./Implementation/internal/api/http/server.go.extend.md|internal/api/http/server.go]] - extend - handlers read and write the generated messages
- [[./Implementation/internal/api/http/protojson.go.create.md|internal/api/http/protojson.go]] - create - proto-JSON in and out, the canonical status table; replaces `errors.go`
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

## Handle an HTTP call of the same contract
1. A client sends a request matching a route of `contractRoutes()`.
2. The handler fills the method's generated request message — from the body through `readMessage`, or from the query of a `GET`.
3. It calls the same domain-service instance.
4. It writes the generated response message through `writeMessage`, or a `*status.Status` through `writeError`.

# Rules

## MUST
- [[./Implementation/Repository.extend.md#MUST|Repository]]
- [[./Implementation/docs/integration/api/{contract-name}/Package.extend.md#MUST|docs/integration/api/{contract-name}]]
- [[./Implementation/internal/api/http/server.go.extend.md#MUST|internal/api/http/server.go]]
- [[./Implementation/internal/api/http/protojson.go.create.md#MUST|internal/api/http/protojson.go]]
- [[./Implementation/internal/api/grpc/Package.create.md#MUST|internal/api/grpc]]
- [[./Implementation/internal/api/grpc/server.go.create.md#MUST|server.go]]
- [[./Implementation/cmd/{service}/main.go.extend.md#MUST|cmd/{service}/main.go]]

# Check list
- [ ] `internal/api/grpc` and `internal/api/http` both call the same domain-service instance constructed once in `main.go`.
- [ ] `make proto-gen` regenerates `gen/api/{contract-name}/` and `docs/integration/api/{contract-name}/{file}.swagger.json` with no manual edits needed afterward; `make test-kind-generated` passes.
- [ ] `docs/integration/api/{contract-name}/` holds no `openapi.json`, and `internal/api/http/` no `errors.go` and no request or response struct of its own.
- [ ] The HTTP API feature the baseline wrote passes without a changed expectation.
- [ ] `run()` starts both servers concurrently via `errgroup.Group` and both stop on the same shutdown signal.
