---
name: solution-grpc-integration
description: Defines the gRPC publication layer — one {Entity}GrpcService per entity implementing a generated .proto server base class as a thin MediatR adapter, Result-to-RpcException status mapping, and App.Host wiring. Optional and independent of solution-http-api-publication — a module can publish over gRPC without REST, over REST without gRPC, or both.
whenToUse: when publishing a module's commands/queries as a gRPC service — designing a .proto contract, a gRPC endpoint, or mapping a Result to an RPC status
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - concern/architecture
  - framework/grpc
  - api
  - protobuf
  - cqrs
  - framework/mediatr
  - solution/grpc-integration
  - stack/dotnet
creates:
  - "{Module}.Api.Protos.{Entity}.proto"
  - "{Module}.Api.Grpc.{Entity}GrpcService.cs"
  - "{Module}.Api.Extensions.RpcExceptionExtensions.cs"
  - App.Host.DependencyInjection.GrpcApiRegistration.cs
extends:
  - "{Module}.Api.csproj"
  - App.Host.DependencyInjection.ApiRegistration.cs
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
built_on_plateau:
---

# Goal
- Define the gRPC layer as a thin adapter over MediatR — no business logic, no domain rules, no persistence, parallel to (and independent of) the HTTP Controllers `solution-http-api-publication` defines
- Define one `.proto` service contract and one `{Entity}GrpcService` implementation per entity — the wire contract is generated code, never hand-written
- Define the `Result<T>` to `RpcException`/`StatusCode` mapping — gRPC's equivalent of `ProblemDetails`
- Wire the gRPC layer in App.Host — service registration, `MapGrpcService<T>()`

# Capabilities
- Thin gRPC adapter layer with no business logic leakage, dispatching the exact same commands/queries the HTTP Controllers dispatch when both are applied
- Standardized `Result<T>` to `RpcException`/`StatusCode` mapping
- One `.proto` contract per entity, generated server base classes, never hand-maintained wire types
- Composable independently of `solution-http-api-publication` — see Boundaries

# Core Principles
- gRPC service methods are thin adapters — map the proto request to a command/query, dispatch via `ISender`, map the result to a proto response
- gRPC is method-oriented, not resource-route-oriented — do not force `solution-http-api-publication`'s five-controller-type model onto it. One `{Entity}GrpcService` class per entity, one RPC method per operation the entity actually needs (`Get`, `List`, `Create`, `Update`, `Delete` — only the ones that exist)
- The `.proto` file is the single source of truth for the wire contract; the generated `{Entity}GrpcServiceBase` is never hand-edited — only `{Entity}GrpcService` (the class that inherits it) is hand-written
- Every RPC method dispatches exactly one `ISender.Send()` — no business logic, no orchestration
- A failed `Result` becomes a thrown `RpcException` via the shared `ToRpcException()` extension — never a raw exception, never a proto response with an ad hoc error field
- Write RPCs are always available (command dispatch is common); read RPCs (`Get`/`List`) are only meaningful once `solution-query-integration` (VP2) provides a query handler. A module without persistence gets a write-only gRPC service.

# Boundaries
- Optional, and independent of [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] (VP8). Both extend the same `partial ApiRegistration` from [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]] (this one adds `AddGrpcApi()`), and both dispatch through the same `ISender` to commands/queries defined once in `{Module}.Interfaces`.
- `solution-repository-integration` is not a dependency — the gRPC adapter never references `IRepository<T>`, only the handler behind `ISender.Send()` does. Read RPCs are pointless without a query handler to back them.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj]] - the project this solution adds `/Protos` + `/Grpc` to
  - [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs]] - the `partial` class this solution adds `AddGrpcApi()` to
- [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs]] - the markers an RPC method dispatches
- [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] (VP2, for read RPCs — not required; write-only service without it)
- [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]] (for read RPCs — not yet composed at this solution's own `built_on_plateau`, see Boundaries)
  - `Shared.csproj` - provides `IQuery<T>` marker for read operations

NUGET:
- `Grpc.AspNetCore` {version} - provides `Grpc.Core.Server`-generated service base classes, `MapGrpcService<T>()`, `AddGrpc()`
- `Google.Protobuf`/`Grpc.Tools` {version} - `.proto` → C# codegen at build time
- `MediatR` {version} - provides `ISender` injected into gRPC services
- `Ardalis.Result` {version} - provides `Result<T>`, `ResultStatus` mapped to `StatusCode`

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend|{Module}.Api.csproj]] - extend - Add `/Protos`, `/Grpc`, and the `RpcException` extension
  - [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/{Entity}.proto.create|{Entity}.proto]] - create - The wire contract for one entity's gRPC service
  - [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/{Entity}GrpcService.cs.create|{Entity}GrpcService.cs]] - create - Thin adapter implementing the generated service base class
  - [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/RpcExceptionExtensions.cs.create|RpcExceptionExtensions.cs]] - create - `ToRpcException()` helper for `Result` error mapping
- [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - extend - Wire gRPC registration into the composition root
  - [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend/GrpcRegistration.cs.create|GrpcRegistration.cs]] - create - gRPC service and Kestrel HTTP/2 registration

# Rules

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend#MUST|{Module}.Api.csproj]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/{Entity}.proto.create#MUST|{Entity}.proto]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/{Entity}GrpcService.cs.create#MUST|{Entity}GrpcService.cs]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/RpcExceptionExtensions.cs.create#MUST|RpcExceptionExtensions.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend#MUST|App.Host.csproj]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend/GrpcRegistration.cs.create#MUST|GrpcRegistration.cs]]
- gRPC layer is a thin adapter — map input, dispatch once, map output
- Every failed `Result` becomes a thrown `RpcException` via `ToRpcException()`
- [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend#MUST|{Module}.Api.csproj]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/{Entity}GrpcService.cs.create#MUST|{Entity}GrpcService.cs]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/RpcExceptionExtensions.cs.create#MUST|RpcExceptionExtensions.cs]]
- Never redeclare a command/query already defined for `solution-http-api-publication` — both adapters dispatch the same `{Module}.Interfaces` contracts
- Never hand-edit generated `{Entity}GrpcServiceBase` code

## SHOULD
- Avoid business logic inside an RPC method — belongs in Domain
- Avoid forcing REST's Collection/SingleEntity/Property/SubCollection/Relationship taxonomy onto a `.proto` service — gRPC is method-oriented
- Avoid returning an error as a field on the proto response message instead of throwing `RpcException`
- Avoid composing this solution and `solution-http-api-publication` under the assumption one requires the other

# Check list
- [ ] Each module publishing gRPC has `/Protos` and `/Grpc` folders in `{Module}.Api`
- [ ] One `.proto` service, one `{Entity}GrpcService` class, per published entity
- [ ] Every RPC method dispatches exactly one `ISender.Send()`
- [ ] Every failure path throws `RpcException` via `ToRpcException()`, never a raw exception or an ad hoc error field
- [ ] `{Entity}GrpcServiceBase` is generated, never hand-edited
- [ ] `GrpcRegistration.AddGrpcApi()`/`UseGrpcApi()` are the only call sites `Program.cs` uses for gRPC
- [ ] Applying this solution alone (without `solution-http-api-publication`) still yields a fully working module
