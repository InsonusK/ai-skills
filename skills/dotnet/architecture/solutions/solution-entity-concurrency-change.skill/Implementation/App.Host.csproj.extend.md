---
description: Register IEntityVersionResolverFactory/module IEntityVersionResolver implementations, and ConcurrencyBehavior in the pipeline, in App.Host
name: App.Host.csproj
element_kind: project
change_kind: extend
tags:
  - solution/entity-concurrency-change
  - element/app-host-csproj
---

# Goals
- Register `IEntityVersionResolverFactory` and all module `IEntityVersionResolver` implementations
- Wire `ConcurrencyBehavior` into the centralized `PipelineRegistration.AddPipeline()` extension, after `ValidationBehavior` and before `GuidResolvingBehavior`/`UnitOfWorkBehavior`

# Core Principles
- `EntityVersionResolverFactory` registered as `Scoped` — it creates `Scoped` resolvers that depend on `IReadRepository<T>`
- `EntityVersionResolverFactory` receives module Domain assemblies (validation) and module Application assemblies (resolver discovery) from App.Host
- Pipeline behavior registration is centralized in App.Host — `AddPipeline()` is the single place where cross-cutting behaviors are ordered

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    EntityVersionResolverRegistration.cs    ← created to register the factory and resolvers
    PipelineRegistration.cs                 ← extended to register ConcurrencyBehavior
```

## Directory and class skills
| Directory \ file | Description |
| ----------------- | ----------- |
| /DependencyInjection/EntityVersionResolverRegistration.cs | Register IEntityVersionResolverFactory and module resolvers |
| /DependencyInjection/PipelineRegistration.cs | Centralized registration of all MediatR pipeline behaviors — extended to add `ConcurrencyBehavior` |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Required by `PipelineRegistration.AddPipeline()` to register `IPipelineBehavior<,>` |

# Allowed Dependencies
- Shared
- BuildingBlocks
- App.Infrastructure
- All module Application and Api projects

# Rules

## MUST
- `IEntityVersionResolverFactory` registered as `Scoped`
- `EntityVersionResolverFactory` receives all module Domain assemblies that contain versioned entities
- `EntityVersionResolverFactory` receives all module Application assemblies that contain `IEntityVersionResolver` implementations
- Register every module `IEntityVersionResolver` implementation as `Scoped`
- `ConcurrencyBehavior` registered in `PipelineRegistration.AddPipeline()`
- `ConcurrencyBehavior` registered after `ValidationBehavior` (when applied) and before `GuidResolvingBehavior`/`UnitOfWorkBehavior` (whichever are applied)
- Never register `IEntityVersionResolverFactory` as `Singleton`
- Never change the signature of `RepositoryRegistration.AddRepositories`
- Never register `ConcurrencyBehavior` inside a module-specific registration method
- Never register `ConcurrencyBehavior` after `UnitOfWorkBehavior`

## SHOULD
- Avoid passing non-Domain or non-Application assemblies to `EntityVersionResolverFactory` — scans unrelated types
- Avoid registering the factory as `Singleton` — creates captive dependencies on `Scoped` repositories

# Check list
- [ ] `EntityVersionResolverFactory` receives module Domain assemblies
- [ ] `EntityVersionResolverFactory` receives module Application assemblies
- [ ] All module `IEntityVersionResolver` implementations registered as `Scoped`
- [ ] `ConcurrencyBehavior` added to `AddPipeline()`, after `ValidationBehavior` and before `UnitOfWorkBehavior`

# Unittest TestCases
- [ ] WHEN applied THEN Register IEntityVersionResolverFactory and module IEntityVersionResolver implementations
- [ ] WHEN applied THEN ConcurrencyBehavior is registered in AddPipeline() after ValidationBehavior and before UnitOfWorkBehavior
