---
name: generated-stub-behind-shared-result-interface
description: How a module calls an external gRPC service — the shape of the client, where it lives, and what a handler sees
problem: VP11 (SyncOutboundApi over gRPC) has no v3 prior art. A handler needs to call another internal service over gRPC; it must not deal with generated stubs, RpcException, deadlines, or channels, and a failed call must be a Result, not an exception.
decision: A Grpc.Tools-generated client stub per dependency, wrapped by {Dependency}GrpcClient : I{Dependency}Client in App.Infrastructure; the handler-facing contract is I{Dependency}Client in Shared, returning Result<T>. Status mapping is one shared extension mirroring solution-grpc-integration's table. Independent of solution-http-api-client (VP10).
tags:
  - solution/grpc-client
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

Internal service-to-service calls in this family go over gRPC (HTTP is reserved for front↔back). A handler in module A needs to call module B (or an external service) over gRPC. The generated `Grpc.Tools` client is a low-level stub: it throws `RpcException`, needs a channel and a deadline, and its message types are wire records, not the module's DTOs. Exposing that to a handler scatters transport concerns and makes the handler untestable without a live gRPC server.

`solution-grpc-integration` (VP9) already defines the *inbound* side (a `.proto` service base wrapped by a thin MediatR adapter, `Result` → `RpcException`). The outbound side should mirror it.

# Selected variant

**Selected variant:** [[#Generated stub wrapped per-dependency, contract in Shared returning Result<T>]]

# Searched variants

## Handler uses the generated stub directly

### Description
`AddGrpcClient<Pricing.PricingClient>()`; the handler injects `Pricing.PricingClient` and calls it.

### Benefits
- No adapter layer to write.
- Full access to every RPC and every gRPC feature.

### Costs
- The handler owns deadline, `RpcException` handling, and mapping wire types to DTOs — transport logic in application code.
- `{Module}.Application.Tests` needs a gRPC test server or a hand-mocked stub (the generated client is a concrete class, awkward to fake).
- A `RpcException` thrown mid-handler bypasses the `Result` contract every other handler path uses.

## One generic `IGrpcCaller` abstraction

### Description
A single `IGrpcCaller.Call<TReq, TReply>(...)` service that any handler uses for any dependency.

### Benefits
- One class, no per-dependency adapter.

### Costs
- The handler still deals in wire message types (`GetPriceRequest`/`PriceReply`), just through a generic seam.
- No place for per-dependency concerns (deadline, retry policy, DTO mapping).
- Loses the "one narrow contract per dependency, only the operations we call" property.

## Generated stub wrapped per-dependency, contract in Shared returning Result<T> (selected)

### Description
Per dependency: vendor its `.proto` (`GrpcServices=Client`) into `App.Infrastructure/Protos`; declare `I{Dependency}Client` in `Shared/Clients` with one method per called operation, each returning `Result<T>` over module DTOs; implement `{Dependency}GrpcClient : I{Dependency}Client` in `App.Infrastructure/Clients` wrapping the generated stub, applying the configured deadline, and catching `RpcException` → `Result` via a shared `GrpcStatusExtensions.ToResult` (the mirror of `solution-grpc-integration`'s `ToRpcException`). Register with `AddGrpcClient<T>()` + `.AddStandardResilienceHandler()` in `App.Host`.

### Benefits
- A handler injects `I{Dependency}Client` and branches on `Result` — identical to how it consumes any other operation; no transport concept in application code.
- `{Module}.Application.Tests` substitutes a trivial fake of `I{Dependency}Client`.
- `Result` round-trips faithfully across a gRPC hop because both directions use the same status table.
- Per-dependency channel/deadline/retry configuration has an obvious home.
- Symmetric with the inbound solution — one mental model for gRPC in the family.

### Costs
- One adapter class + one interface per dependency to write (mechanical).
- The `I{Dependency}Client` name shape overlaps `solution-http-api-client` (VP10); a module using both does so for different dependencies (different files), so no collision — but if one dependency ever needs both transports, a shared `solution-outbound-client` prerequisite would be needed. Noted in `# Boundaries`; not built today.
- The vendored `.proto` is a manual copy — it can drift from the dependency's real contract until re-vendored.
