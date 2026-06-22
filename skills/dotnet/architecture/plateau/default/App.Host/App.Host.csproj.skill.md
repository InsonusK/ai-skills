---
uid: 69f2ba24-7e98-4626-a234-dea3d0a2794a
name: app-host-csproj
description: Be the single composition root — wire all modules, infrastructure, pipeline behaviors, and DI registrations together
domain: skill
type: template
version: 20260622
tags:
  - skill/template/csproj
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration.solution.skill]]"
---

# Goal
- Register `IUnitOfWork` and `UnitOfWorkContext` with correct lifetimes
- Be the single composition root — wire all modules, infrastructure, pipeline behaviors, and DI registrations together
- Be the only project that knows about all other projects simultaneously
- Wire repository registrations into the application composition root
- Add `RegisterAppQueries()` call to the composition root alongside module registrations
- Ensure cross-module query handlers are discovered by MediatR
- Register the centralized `AddPipeline()` extension in the composition root
- Ensure `Program.cs` calls `AddPipeline()` exactly once
- Extend the centralized `AddPipeline()` extension so it registers all pipeline behaviors in the canonical execution order
- Ensure `Program.cs` continues to call `AddPipeline()` exactly once
- Register controllers from all module Api assemblies
- Register Minimal API endpoint groups
- Configure ASP.NET Core JSON and ProblemDetails middleware
- Register `IEntityVersionResolverFactory` and all module `IEntityVersionResolver` implementations
- Feed the factory module Domain assemblies (for validation) and module Application assemblies (for resolver discovery)
- Ensure module registrations are called via the centralized `AddModules()` extension in `Program.cs`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

# Core Principals
- `IUnitOfWork` and `UnitOfWorkContext` share `Scoped` lifetime with `DbContext` and `Repository<T>`
- App.Host is the only composition root — it wires everything together
- App.Host references BuildingBlocks directly; Shared is consumed transitively through BuildingBlocks
- App.Host contains no business logic — only wiring
- Pipeline behaviors are registered once here — not inside individual modules
- App.Host is the single composition root — all DI registration happens here or via extension methods called from here
- App.Host is the only composition root — all wiring happens here
- `RegisterAppQueries()` called after all module registrations — App.Queries depends on module entity types being registered
- Centralized in `ModuleRegistration.AddModules` or called directly from `Program.cs` if following the high-level extension pattern
- `Program.cs` only calls the high-level composition extension: `AddPipeline()`
- Pipeline behavior order is enforced inside `PipelineRegistration.AddPipeline()` — not in `Program.cs`
- `PipelineRegistration.AddPipeline()` is the single source of truth for behavior order
- Behaviors are registered in execution order — first registered runs first
- `EntityVersionResolverFactory` registered as `Scoped` — it creates `Scoped` resolvers that depend on `IReadRepository<T>`
- `EntityVersionResolverFactory` receives module Domain assemblies (validation) and module Application assemblies (resolver discovery) from App.Host — the only project that references all modules
- `Program.cs` calls `AddModules()` — the centralized registration point created by solution-structure
- Each module's `Register{ModuleName}Module(configuration)` is called inside `ModuleRegistration.AddModules`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

# Structure

## Solution place
```
/src/App/App.Host
```


## Project Structure
```
/App.Host
  /DependencyInjection
    RepositoryRegistration.cs
```

```
/App.Host
  /DependencyInjection
    ModuleRegistration.cs
    PipelineRegistration.cs
    InfrastructureRegistration.cs
  Program.cs
  App.Host.csproj
```

```
/App.Host
  /DependencyInjection
    RepositoryRegistration.cs
  Program.cs
```

```
/App.Host
  /DependencyInjection
    ModuleRegistration.cs    ← solution-structure
    PipelineRegistration.cs  ← pipeline-registration
  Program.cs
```

```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs
  Program.cs
```

```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs
  Program.cs
```

```
/App.Host
  /DependencyInjection
    RepositoryRegistration.cs    ← repository-integration
    PipelineRegistration.cs      ← pipeline-registration
    ApiRegistration.cs
  Program.cs
```

```
/App.Host
  /DependencyInjection
    EntityVersionResolverRegistration.cs    ← created to register the factory and resolvers
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

## Directory and class skills
| `Directory|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
| /DependencyInjection | DI registrations and pipeline setup |  |
| ModuleRegistration.cs | Centralized module registration extension | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Host/classes/ModuleRegistration.class.skill.md|ModuleRegistration.class.skill]] |
| /DependencyInjection/PipelineRegistration.cs | Centralized pipeline behavior registration | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Host/classes/PipelineRegistration.class.skill.md|PipelineRegistration.class.skill]] |
| /DependencyInjection/PipelineRegistration.cs | Centralized pipeline behavior registration with ordered behaviors | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Host/classes/PipelineRegistration.class.skill.md|PipelineRegistration.class.skill]] |
| /DependencyInjection/ApiRegistration.cs | Controller registration and middleware configuration | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Host/classes/ApiRegistration.class.skill.md|ApiRegistration.class.skill]] |
| /DependencyInjection/EntityVersionResolverRegistration.cs | Register IEntityVersionResolverFactory and module resolvers as Scoped with Domain and Application assemblies | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Host/classes/EntityVersionResolverRegistration.class.skill.md|EntityVersionResolverRegistration.class.skill]] |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Handler implementations — belong to module Application
- Infrastructure implementations — belong to App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

## Allowed Dependencies
- Shared
- BuildingBlocks
- App.Infrastructure
- {ModuleName}.Api (all modules)
- {ModuleName}.Application (all modules — for registration methods)
- App.Queries
- App.Queries — for `RegisterAppQueries()` extension
- All module Application projects — for `Register{ModuleName}Module()` extensions
- App.Host.DependencyInjection
- All module Application and Api projects
- `{Module}.Application` for module registration extensions

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

# Rules
MUST:
	- `IUnitOfWork` registered as `Scoped`
	- `UnitOfWorkContext` registered as `Scoped`
	- Pipeline behaviors registered once here in correct order
	- Each module registration method called here
	- App.Host is the only project referencing all modules simultaneously
	- `AddRepositories()` called in `Program.cs`
	- `RegisterAppQueries()` called from App.Host
	- Called after all module registrations — App.Queries depends on module entity types
	- `AddPipeline()` called in `Program.cs`
	- `AddPipeline()` called exactly once
	- `UseExceptionHandler()` registered before `MapControllers()` — unhandled exceptions produce `ProblemDetails`
	- All module Api assemblies added as application parts
	- All Minimal API endpoint groups mapped explicitly
	- `AddProblemDetails()` registered in DI
	- `IEntityVersionResolverFactory` registered as `Scoped`
	- `EntityVersionResolverFactory` receives all module Domain assemblies that contain versioned entities
		- `EntityVersionResolverFactory` receives all module Application assemblies that contain `IEntityVersionResolver` implementations
		- All module `IEntityVersionResolver` implementations registered as `Scoped`
	- `AddModules()` called in `Program.cs`
	- All module `Register{ModuleName}Module()` calls made inside `ModuleRegistration.AddModules`
MUST NOT:
	- Register `IUnitOfWork` or `UnitOfWorkContext` inside module registration methods
	- App.Host contain business logic
	- App.Host contain handler implementations
	- Call `RegisterAppQueries()` from inside any module registration method
	- Register `IPipelineBehavior<,>` directly in `Program.cs`
	- Call `AddPipeline()` more than once
	- Register controllers manually one by one — use `AddApplicationPart` with assembly references
	- Change the signature of `RepositoryRegistration.AddRepositories`
	- Call individual `Register{ModuleName}Module()` methods directly from `Program.cs`
	- Module registration methods called from within another module
	- Add module registration calls outside `ModuleRegistration.AddModules`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

# Anti-patterns
- Registering `IUnitOfWork` or `UnitOfWorkContext` in module registration — these are global services, belong in App.Host
- Putting business logic in App.Host — wiring only
- Putting handler implementations in App.Host — belong in module Application
- Registering pipeline behaviors inside module registration — register once in App.Host
- Calling `RegisterAppQueries()` before module registrations — module handlers and entity types may not be available
- Scattering App.Queries registration across multiple extension methods
- Registering behaviors directly in `Program.cs`
- Calling `AddPipeline()` multiple times
- Missing `UseExceptionHandler()` before `MapControllers()`
- Forgetting to map Minimal API endpoint groups
- Passing non-Domain or non-Application assemblies to `EntityVersionResolverFactory` — scans unrelated types
- Registering the factory as `Singleton` — creates captive dependencies on `Scoped` repositories
- `Program.cs` listing every module explicitly — centralize module calls in `ModuleRegistration`
- Scattering module registration across multiple extension methods called from `Program.cs`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

# Check list
- [ ] `IUnitOfWork` registered as `Scoped`
- [ ] `UnitOfWorkContext` registered as `Scoped`
- [ ] All module registration methods called
- [ ] Pipeline behaviors registered in correct order
- [ ] No business logic in App.Host
- [ ] `builder.Services.AddRepositories()` present in `Program.cs`
- [ ] `RegisterAppQueries()` called from App.Host
- [ ] Called after all module registrations
- [ ] Not called from within any module registration method
- [ ] `AddPipeline()` called from `Program.cs`
- [ ] No direct `IPipelineBehavior<,>` registration in `Program.cs`
- [ ] `ApiRegistration.cs` exists in `/DependencyInjection`
- [ ] All module Api assemblies added as application parts
- [ ] `UseExceptionHandler()` before `MapControllers()`
- [ ] `AddProblemDetails()` registered
- [ ] `IEntityVersionResolverFactory` registered as `Scoped`
- [ ] `EntityVersionResolverFactory` receives module Domain assemblies
- [ ] `EntityVersionResolverFactory` receives module Application assemblies
- [ ] All module `IEntityVersionResolver` implementations registered as `Scoped`
- [ ] `AddModules(builder.Configuration)` called in `Program.cs`
- [ ] Every registered module is added inside `ModuleRegistration.AddModules`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
