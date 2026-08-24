---
name: csproj-app-host
description: Project App.Host in the statefull-service plateau
whenToUse: when adding or editing DI/module/pipeline wiring in App.Host, or deciding whether new code belongs here
domain: skill
type: template
plateau: statefull-service
version: 20260824100000
tags:
  - skill/template/csproj
  - plateau/statefull-service
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]]"
  - "[[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]]"
  - "[[../../../../solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]"
  - "[[../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]"
  - "[[../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]]"
---

# Goal
- Be the single composition root — wire all modules, infrastructure, pipeline behaviors, and DI registrations together
- Be the only project that knows about all other projects simultaneously
- Register the centralized `AddPipeline()` extension in the composition root, with `ExceptionHandlingBehavior` first in that pipeline

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

# Core Principles
- App.Host references BuildingBlocks directly; Shared is consumed transitively through BuildingBlocks
- App.Host contains no business logic — only wiring
- Pipeline behaviors are registered once here — not inside individual modules
- `Program.cs` only calls the high-level composition extensions: `AddModules()`, `AddPipeline()`
- Pipeline behavior order is enforced inside `PipelineRegistration.AddPipeline()` — not in `Program.cs`

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

# Structure

## Solution place
```
/src/App/App.Host
```

## Project Structure
- /App.Host
  - /DependencyInjection
    - [ModuleRegistration.cs](./classes/plateau-statefull-service--class-module-registration.skill.md)
    - [PipelineRegistration.cs](./classes/plateau-statefull-service--class-pipeline-registration.skill.md)
    - [InfrastructureRegistration.cs, RepositoryRegistration.cs, EntityVersionResolverRegistration.cs](./classes/plateau-statefull-service--class-infrastructure-registration.skill.md)
  - Program.cs
  - App.Host.csproj

`Program.cs` now calls `AddModules(builder.Configuration)`, `AddPipeline()`, and `AddInfrastructure(builder.Configuration)` — the third call is new at this plateau, per `solution-infrastructure-project`.

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /DependencyInjection | DI registrations and pipeline setup | |
| ModuleRegistration.cs | Centralized module registration extension (`AddModules()`) | [[./classes/plateau-statefull-service--class-module-registration.skill.md\|class-module-registration]] |
| PipelineRegistration.cs | Centralized pipeline behavior registration (`AddPipeline()`), `ExceptionHandlingBehavior` first | [[./classes/plateau-statefull-service--class-pipeline-registration.skill.md\|class-pipeline-registration]] |
| InfrastructureRegistration.cs, RepositoryRegistration.cs, EntityVersionResolverRegistration.cs | `AppDbContext`, repositories, unit of work, version resolvers — all wired via `AddInfrastructure()` | [[./classes/plateau-statefull-service--class-infrastructure-registration.skill.md\|class-infrastructure-registration]] |

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Required by `PipelineRegistration.AddPipeline()` to register `IPipelineBehavior<,>` |

__Applied solutions:__
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Handler implementations — belong to module Application
- Infrastructure implementations — belong to App.Infrastructure

## Allowed Dependencies
- {ModuleName}.Api (all modules)
- {ModuleName}.Application (all modules — for registration methods)
- BuildingBlocks
- App.Infrastructure — new at this plateau, for `AddInfrastructure()`'s `AppDbContext`/repository/unit-of-work/version-resolver registrations
- App.Queries — new at this plateau, for `RegisterAppQueries()`

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
MUST NOT:
- App.Host contain business logic
- App.Host contain handler implementations
- Register `IPipelineBehavior<,>` directly in `Program.cs`
- Call `AddPipeline()` more than once
- Register `ExceptionHandlingBehavior` inside a module-specific registration method, or after other pipeline behaviors

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

# Check list
- [ ] All module registration methods called via `AddModules()`
- [ ] `AddPipeline()` called from `Program.cs`, exactly once
- [ ] `ExceptionHandlingBehavior` is the first behavior registered in `AddPipeline()`
- [ ] No business logic in App.Host

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
