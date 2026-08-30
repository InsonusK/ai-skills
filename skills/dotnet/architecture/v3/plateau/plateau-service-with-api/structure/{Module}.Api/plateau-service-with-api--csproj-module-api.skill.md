---
name: plateau-service-with-api--csproj-module-api
description: Project {Module}.Api in the service-with-api plateau
whenToUse: when adding or editing an HTTP endpoint or gRPC service in {Module}.Api, or deciding whether new code belongs here
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/csproj
  - plateau/service-with-api
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]]"
  - "[[../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]]"
---

# Goal
- Expose the module externally as thin MediatR adapters — over HTTP (REST Controllers + Minimal API), gRPC, or both

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
- [[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]

# Core Principles
- Api is a thin adapter — no business logic, no domain rules, regardless of protocol
- Api references only its own Interfaces project for contracts, plus `Microsoft.AspNetCore.Mvc`/`Grpc.AspNetCore` for whichever protocol(s) are applied
- `solution-http-api-publication` and `solution-grpc-integration` are independent — a module applies one, the other, or both. Neither is required by this plateau on its own; composing the plateau requires at least one (see Rules)
- Both protocols dispatch the exact same commands/queries from `{Module}.Interfaces` — a command/query is defined once, reused by whichever adapter(s) call it

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
- [[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Api
```

## Project Structure
- /{Module}.Api
  - /Controllers                    (solution-http-api-publication)
    - [{Entity}Controller.cs](./classes/plateau-service-with-api--class-entity-controller.skill.md)
    - [Single{Entity}Controller.cs](./classes/plateau-service-with-api--class-single-entity-controller.skill.md)
    - [Single{Entity}{Property}Controller.cs](./classes/plateau-service-with-api--class-single-entity-property-controller.skill.md)
    - [{Entity}{Related}Controller.cs](./classes/plateau-service-with-api--class-entity-related-controller.skill.md)
    - [Single{Entity}{Related}Controller.cs](./classes/plateau-service-with-api--class-single-entity-related-controller.skill.md)
  - /MinimalApi                     (solution-http-api-publication)
    - [{System}Endpoints.cs](./classes/plateau-service-with-api--class-system-endpoints.skill.md)
  - /Protos                         (solution-grpc-integration)
    - [{Entity}.proto](./classes/plateau-service-with-api--class-entity-proto.skill.md)
  - /Grpc                           (solution-grpc-integration)
    - [{Entity}GrpcService.cs](./classes/plateau-service-with-api--class-entity-grpc-service.skill.md)
  - /Extensions
    - [ResultExtensions.cs](./classes/plateau-service-with-api--class-result-extensions.skill.md) (solution-http-api-publication)
    - [RpcExceptionExtensions.cs](./classes/plateau-service-with-api--class-rpc-exception-extensions.skill.md) (solution-grpc-integration)
  - [{Module}ApiSwaggerRegistration.cs](./classes/plateau-service-with-api--class-module-api-swagger-registration.skill.md) (solution-http-api-publication)
  - {Module}.Api.csproj

Only the folders for the protocol(s) actually applied exist — a gRPC-only module has no `/Controllers`/`/MinimalApi`, an HTTP-only module has no `/Protos`/`/Grpc`.

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
- [[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Controllers | REST entity-lifecycle endpoints, five archetypes | [[./classes/plateau-service-with-api--class-entity-controller.skill.md\|class-entity-controller]], [[./classes/plateau-service-with-api--class-single-entity-controller.skill.md\|class-single-entity-controller]], [[./classes/plateau-service-with-api--class-single-entity-property-controller.skill.md\|class-single-entity-property-controller]], [[./classes/plateau-service-with-api--class-entity-related-controller.skill.md\|class-entity-related-controller]], [[./classes/plateau-service-with-api--class-single-entity-related-controller.skill.md\|class-single-entity-related-controller]] |
| /MinimalApi | System/webhook/batch/cross-aggregate REST endpoints | [[./classes/plateau-service-with-api--class-system-endpoints.skill.md\|class-system-endpoints]] |
| /Protos | gRPC wire contracts, one `.proto` per entity | [[./classes/plateau-service-with-api--class-entity-proto.skill.md\|class-entity-proto]] |
| /Grpc | gRPC service implementations, one per entity | [[./classes/plateau-service-with-api--class-entity-grpc-service.skill.md\|class-entity-grpc-service]] |
| /Extensions | `Result` → `ProblemDetails`/`RpcException` mapping | [[./classes/plateau-service-with-api--class-result-extensions.skill.md\|class-result-extensions]], [[./classes/plateau-service-with-api--class-rpc-exception-extensions.skill.md\|class-rpc-exception-extensions]] |
| {Module}ApiSwaggerRegistration.cs | Per-module Swagger document metadata | [[./classes/plateau-service-with-api--class-module-api-swagger-registration.skill.md\|class-module-api-swagger-registration]] |

## NuGet Packages
| Package | Version constraint | Purpose | Applied by |
| --- | --- | --- | --- |
| `Microsoft.AspNetCore.Mvc` | latest stable | `ControllerBase`, `[ApiController]`, `ProblemDetails` | solution-http-api-publication |
| `Ardalis.Result` | latest stable | `Result<T>`, `ResultStatus` | both |
| `MediatR` | latest stable | `ISender` | both |
| `Grpc.AspNetCore` | latest stable | gRPC server, `MapGrpcService<T>()` | solution-grpc-integration |
| `Grpc.Tools` | latest stable | `.proto` → C# codegen | solution-grpc-integration |

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Handler implementations — belong to Application
- Cross-aggregate reads — belong to App.Queries (once composed)

## Allowed Dependencies
- `{Module}.Interfaces` (own module only)
- Shared

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]

# Rules
MUST:
- Every controller action/Minimal API endpoint/RPC method dispatch exactly one `ISender.Send()`
- Api reference only own Interfaces and Shared
- At least one of `/Controllers`+`/MinimalApi` (HTTP) or `/Protos`+`/Grpc` (gRPC) exist — a module composing this plateau needs some external surface
MUST NOT:
- Api reference Domain or Application directly
- Api contain business logic, validation logic, or domain rules
- Assume both protocols are applied — each adapter's own registration (`AddApi()`/`AddGrpcApi()`) works independently

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
- [[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]
- [[../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj.extend]]

# Check list
- [ ] Api.csproj does not reference Domain or Application
- [ ] Every action/endpoint/RPC method dispatches exactly one MediatR request
- [ ] No business logic in any controller/endpoint/gRPC service
- [ ] At least one protocol (HTTP, gRPC) is actually applied
- [ ] Applying only one protocol still yields a fully working module — verify by checking neither adapter's registration references the other

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
