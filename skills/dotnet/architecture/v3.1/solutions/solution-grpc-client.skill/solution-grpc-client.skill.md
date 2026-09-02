---
name: solution-grpc-client
description: Skeleton — realizes SyncOutboundApi over gRPC (VP11). A generated gRPC client per external dependency from its .proto, wrapped by a narrow Shared interface that returns Result<T> and maps RpcException/StatusCode to a Result, registered with resilience. Independent of solution-http-api-client.
whenToUse: when a module must make synchronous request-response calls to another service over gRPC — wiring the generated client, its RpcException-to-Result mapping, and the Shared contract the handler consumes
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - concern/architecture
  - framework/grpc
  - protobuf
  - api
  - resilience
  - solution/grpc-client
  - stack/dotnet
creates:
  - "App.Infrastructure.Protos.{Dependency}.proto"
  - "App.Infrastructure.Clients.{Dependency}GrpcClient.cs"
  - "Shared.Clients.I{Dependency}Client.cs"
extends:
  - "Shared.csproj"
  - "App.Infrastructure.csproj"
  - "App.Host.csproj"
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]"
---

> **Draft contract — no client yet.** VP11 has no v3 prior art. This skeleton fixes the shape (generated client from the dependency's `.proto`, narrow `Shared` contract returning `Result<T>`, `RpcException` mapping). The channel/retry policy and the deadline convention are finalized with the first real dependency.

# Goal
- Give a module one way to call an external gRPC service: an injected `I{Dependency}Client` (a `Shared` contract) whose methods return `Result<T>` — no generated client type, no `RpcException`, no `StatusCode` handling in the handler.
- Generate the client stub from the dependency's `.proto` (never hand-written); map `RpcException`/`StatusCode` to a `Result` in one adapter.

# Core Principle
- `I{Dependency}Client` is a narrow `Shared` interface returning `Result<T>`.
- `{Dependency}GrpcClient` (in `App.Infrastructure`) wraps the generated stub, applies a deadline, and maps the RPC status: `NotFound → Result.NotFound()`, `Unavailable/DeadlineExceeded → Result.Error`, etc.
- The channel address and credentials come from configuration.
- Independent of `solution-http-api-client` (VP10) — a module may use HTTP outbound, gRPC outbound, both, or neither.

# Boundaries
- Inbound gRPC (this module's own service) is `solution-grpc-integration` (VP9).
- The `.proto` contract is owned by the dependency's team; this solution consumes a copy.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj]] - hosts the generated client + adapter

NUGET:
- `Grpc.Net.ClientFactory`, `Google.Protobuf`, `Grpc.Tools` {version} - generated client + codegen (versions in `Directory.Packages.props`)

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-client.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - extend - add `Protos/{Dependency}.proto`, the adapter, and `App.Host` registration

# Rule

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-client.skill/Implementation/App.Infrastructure.csproj.extend.md#MUST|App.Infrastructure.csproj]]
- Expose the dependency as `I{Dependency}Client` (a `Shared` contract) returning `Result<T>` — never the generated stub or `RpcException`.
  - Risk: a handler holding the generated client owns status mapping and deadlines, scattering transport concerns.
  - Fix: the adapter in `App.Infrastructure` maps status to `Result`.
- Generate the client from `.proto`; never hand-write the stub; apply a deadline to every call.
  - Risk: a hand-written stub drifts from the contract; a call with no deadline can hang indefinitely.
  - Fix: `Grpc.Tools` codegen; `CallOptions` with a deadline from configuration.

# Check list
- [ ] `I{Dependency}Client` in `Shared/Clients`, methods return `Result<T>`.
- [ ] Generated stub from `.proto`; adapter maps `RpcException`/`StatusCode` to `Result`.
- [ ] Every call has a deadline; channel address from configuration.
- [ ] No generated type or `RpcException` reaches a handler.
