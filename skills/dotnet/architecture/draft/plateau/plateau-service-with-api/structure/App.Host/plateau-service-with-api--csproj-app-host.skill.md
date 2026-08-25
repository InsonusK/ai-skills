---
name: csproj-app-host
description: Project App.Host in the service-with-api plateau
whenToUse: when adding or editing DI/module/pipeline wiring in App.Host, or deciding whether new code belongs here
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/csproj
  - plateau/service-with-api
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]]"
  - "[[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]]"
  - "[[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]]"
  - "[[../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]]"
---

# Goal
- Be the single composition root — wire all modules, infrastructure, pipeline behaviors, and DI registrations together
- Be the only project that knows about all other projects simultaneously
- Register the centralized `AddPipeline()` extension in the composition root, with `ExceptionHandlingBehavior` first in that pipeline
- Run as an ASP.NET Core web host — `WebApplication.CreateBuilder`, not the plain console host the foundation plateau used — now that there's an external surface (HTTP and/or gRPC) to serve

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

# Core Principles
- App.Host references BuildingBlocks directly; Shared is consumed transitively through BuildingBlocks
- App.Host contains no business logic — only wiring
- Pipeline behaviors are registered once here — not inside individual modules
- `Program.cs` calls the high-level composition extensions: `AddModules()`, `AddPipeline()`, and — whichever are actually applied — `AddApi()`/`UseApi()` (HTTP) and/or `AddGrpcApi()`/`UseGrpcApi()` (gRPC)
- Pipeline behavior order is enforced inside `PipelineRegistration.AddPipeline()` — not in `Program.cs`
- `ApiRegistration` and `GrpcRegistration` don't know about each other — each works whether the other is applied or not, per `solution-http-api-publication`/`solution-grpc-integration`'s own Boundaries

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

# Structure

## Solution place
```
/src/App/App.Host
```

## Project Structure
- /App.Host
  - /DependencyInjection
    - [ModuleRegistration.cs](./classes/plateau-service-with-api--class-module-registration.skill.md)
    - [PipelineRegistration.cs](./classes/plateau-service-with-api--class-pipeline-registration.skill.md)
    - [ApiRegistration.cs](./classes/plateau-service-with-api--class-api-registration.skill.md) (solution-http-api-publication)
    - [GrpcRegistration.cs](./classes/plateau-service-with-api--class-grpc-registration.skill.md) (solution-grpc-integration)
  - Program.cs
  - App.Host.csproj

`Program.cs` now uses `WebApplication.CreateBuilder(args)` and calls `AddModules()`, `AddPipeline()`, and whichever of `AddApi()`/`AddGrpcApi()` are applied, then `app.UseApi()`/`app.UseGrpcApi()` before `app.Run()`.

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /DependencyInjection | DI registrations and pipeline setup | |
| ModuleRegistration.cs | Centralized module registration extension (`AddModules()`) | [[./classes/plateau-service-with-api--class-module-registration.skill.md\|class-module-registration]] |
| PipelineRegistration.cs | Centralized pipeline behavior registration (`AddPipeline()`), `ExceptionHandlingBehavior` first | [[./classes/plateau-service-with-api--class-pipeline-registration.skill.md\|class-pipeline-registration]] |
| ApiRegistration.cs | Controller discovery, `ProblemDetails`, per-module Swagger documents | [[./classes/plateau-service-with-api--class-api-registration.skill.md\|class-api-registration]] |
| GrpcRegistration.cs | gRPC service registration | [[./classes/plateau-service-with-api--class-grpc-registration.skill.md\|class-grpc-registration]] |

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Required by `PipelineRegistration.AddPipeline()` to register `IPipelineBehavior<,>` |
| `Microsoft.AspNetCore.Mvc` (implicit, web SDK) | latest stable | `AddControllers()`, `AddProblemDetails()` — solution-http-api-publication |
| `Swashbuckle.AspNetCore` | latest stable | Per-module `AddSwaggerGen()`/`UseSwaggerUI()` — solution-http-api-publication |
| `Grpc.AspNetCore` | latest stable | `AddGrpc()`, `MapGrpcService<T>()` — solution-grpc-integration |

__Applied solutions:__
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Handler implementations — belong to module Application
- Infrastructure implementations — belong to App.Infrastructure

## Allowed Dependencies
- {ModuleName}.Api (all modules)
- {ModuleName}.Application (all modules — for registration methods)
- BuildingBlocks
- App.Infrastructure and App.Queries are not created by this plateau — a persistence-introducing plateau adds them here once composed on top

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]

# Rules
MUST:
- Pipeline behaviors registered once here in correct order
- Each module registration method called here, via `AddModules()`
- App.Host is the only project referencing all modules simultaneously
- App.Host is the only composition root
- App.Host references BuildingBlocks; modules and other layers reference Shared directly to implement or consume interfaces
- `AddPipeline()` called in `Program.cs`, exactly once
- `ExceptionHandlingBehavior` registered before all other pipeline behaviors
- `Program.cs` use `WebApplication.CreateBuilder`, call `AddApi()`/`AddGrpcApi()` (whichever applied) before `builder.Build()`, and `app.UseApi()`/`app.UseGrpcApi()` (whichever applied) before `app.Run()`
- `UseExceptionHandler()` precede `MapControllers()` when `solution-http-api-publication` is applied
MUST NOT:
- App.Host contain business logic
- App.Host contain handler implementations
- Register `IPipelineBehavior<,>` directly in `Program.cs`
- Call `AddPipeline()` more than once
- Register `ExceptionHandlingBehavior` inside a module-specific registration method, or after other pipeline behaviors
- `ApiRegistration`/`GrpcRegistration` reference each other — each must work whether the other solution is applied or not

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

# Check list
- [ ] All module registration methods called via `AddModules()`
- [ ] `AddPipeline()` called from `Program.cs`, exactly once
- [ ] `ExceptionHandlingBehavior` is the first behavior registered in `AddPipeline()`
- [ ] No business logic in App.Host
- [ ] `Program.cs` uses `WebApplication.CreateBuilder`
- [ ] At least one of `AddApi()`/`AddGrpcApi()` is called — matching what `{Module}.Api` actually implements
- [ ] Removing either `ApiRegistration` or `GrpcRegistration` (simulating the other solution not being applied) doesn't break the one that remains

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
