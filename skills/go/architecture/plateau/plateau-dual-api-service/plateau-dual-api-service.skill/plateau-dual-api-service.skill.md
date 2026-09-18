---
name: plateau-dual-api-service
description: plateau-http-service plus a gRPC inbound API alongside HTTP, sharing one domain-service instance
whenToUse: when a Go web-service needs to expose its capabilities over gRPC in addition to HTTP, or reviewing whether a change follows this family's errgroup-based concurrent-server convention
domain: skill
type: template
version: 20260917010000
tags:
  - skill/template/plateau
  - plateau/plateau-dual-api-service
  - stack/go
parent_plateaus:
  - "[[skills/go/architecture/plateau/plateau-http-service/plateau-http-service.skill/plateau-http-service.skill.md|plateau-http-service]]"
created_by:
  - "[[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
standalone: true
registry:
  - "[[skills/go/architecture/registry/cmd-service-main-go.md|cmd-service-main-go]]"
  - "[[skills/go/architecture/registry/internal-config-config-go.md|internal-config-config-go]]"
  - "[[skills/go/architecture/registry/repo-root.md|repo-root]]"
---

# Goal
Everything [[skills/go/architecture/plateau/plateau-http-service/plateau-http-service.skill/plateau-http-service.skill.md|plateau-http-service]] has, plus a second inbound entry point over gRPC — the same `LinkCheckService.Check` reachable over both transports from one composition root.

# Core Principles
Union of the parent's principles, plus:
- `internal/api/grpc` is exactly as thin as `internal/api/http` — no business logic, only decode/call/encode.
- Two or more concurrent long-running servers in `run()` are run via `errgroup.Group` — the parent's single blocking `httpServer.ListenAndServe()` call is restructured the moment gRPC is added, not left as a second sequential blocker.
- This module's own exposed contract (`proto/linkcheck/linkcheck.proto` → `gen/api`) stays in its own flat, unversioned path — a `v1/` path segment under a flat `go_package` produces a Go import-path mismatch (verified, not hypothetical — see [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]'s own Rule).

# Capabilities
Union of the parent's capabilities, plus:
- api
  - The same `Check` operation, now callable via `linkcheck.LinkCheckService/Check` (gRPC) in addition to `POST /v1/links/check` (HTTP) — identical validation/normalization behavior, verified identical by construction (one domain-service instance, two thin adapters).

# Usecases

## Check a URL over gRPC
```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant srv as Server (grpc)
    participant svc as LinkCheckService

    Client->>srv: Check({url: "HTTPS://Example.com/Foo"})
    activate srv
    srv->>svc: Check(ctx, "HTTPS://Example.com/Foo")
    activate svc
    svc-->>srv: Result{Normalized: "https://example.com/Foo"}
    deactivate svc
    srv-->>Client: {url: "...", normalized: "https://example.com/Foo"}
    deactivate srv
```
An invalid URL maps to `codes.InvalidArgument` (gRPC) the same way it maps to `400` (HTTP) — see `internal/api/grpc/server.go`'s `toStatus`.

# Structure
See `structure/` — everything from [[skills/go/architecture/plateau/plateau-http-service/plateau-http-service.skill/plateau-http-service.skill.md|plateau-http-service]]'s structure, union'd with:
- [[structure/plateau-dual-api-service--repo-dual-api-service.skill.md|repo-dual-api-service]] (extended: `proto/`, `buf/`, `gen/api/`, `proto-gen` target)
- [[structure/plateau-dual-api-service--package-api-grpc.skill.md|package-api-grpc]] (new)
- [[structure/plateau-dual-api-service--file-api-grpc-server.skill.md|file-api-grpc-server]] (new)
- [[structure/plateau-dual-api-service--file-cmd-service-main.skill.md|file-cmd-service-main]] (extended: `errgroup.Group`)
- [[structure/plateau-dual-api-service--file-config-config.skill.md|file-config-config]] (extended: `GRPCListenPort`)
- Unchanged from the parent: `package-domain-services`, `package-api-http`, `file-domain-services-linkcheck`, `file-version-version`, `file-logging-logger`, `file-api-http-server`.

# Registry
Three intersections, all still canonical (no resolver) but two grew from the parent — see `registry/`:
- [[skills/go/architecture/registry/cmd-service-main-go.md|cmd-service-main-go]] (N=4; `grpc-api` genuinely *restructures* `http-api`'s contribution via its own `depends_on` edge — `TMN`, not `FMN`, for that specific pairing; still canonical)
- [[skills/go/architecture/registry/internal-config-config-go.md|internal-config-config-go]] (N=4, stayed purely additive)
- [[skills/go/architecture/registry/repo-root.md|repo-root]] (N=3, crosses the architectural-signal threshold for the first time; flagged to re-check once `solution-external-integration` also extends the same `proto-gen` target)

# Ground truth
`example/` evolved from `plateau-http-service`'s (copied forward, then extended), verified:
- `buf generate proto/linkcheck --template buf/buf.gen.yaml` (local `protoc-gen-go`/`protoc-gen-go-grpc` plugins, installed via `go install`) — produces `gen/api/{linkcheck.pb.go,linkcheck_grpc.pb.go}`, flat, matching `go_package`.
- `go build ./...` and `go vet ./...` — clean.
- `make unit-test` — 5/5 godog scenarios still green (domain logic unchanged).
- `make mutation-test` — `gremlins` clean run.
- `make test-report` — `public/` assembled.
- Runtime smoke test: built binary started both servers; `GET /health` → `200`; `grpcurl -plaintext -proto proto/linkcheck/linkcheck.proto` against `linkcheck.LinkCheckService/Check` with a valid URL → success with normalized form; with an invalid URL → `InvalidArgument` status. HTTP endpoint re-verified still working alongside gRPC.

To run it yourself: `cd example && go mod tidy && make proto-gen && make build && make unit-test && make run`.
