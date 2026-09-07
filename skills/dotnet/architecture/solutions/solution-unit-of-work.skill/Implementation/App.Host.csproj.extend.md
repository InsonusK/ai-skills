---
description: Register IUnitOfWork/UnitOfWorkContext with scoped lifetimes, and UnitOfWorkBehavior last in the pipeline
name: App.Host.csproj
element_kind: project
change_kind: extend
tags:
  - solution/unit-of-work
  - element/app-host-csproj
---

# Goals
- Register `IUnitOfWork` and `UnitOfWorkContext` with correct lifetimes
- Wire `UnitOfWorkBehavior` into the centralized `PipelineRegistration.AddPipeline()` extension, last — after every other applied pipeline behavior

# Core Principles
- `IUnitOfWork` and `UnitOfWorkContext` share `Scoped` lifetime with `DbContext` and `Repository<T>`
- Pipeline behavior registration is centralized in App.Host — `AddPipeline()` is the single place where cross-cutting behaviors are ordered

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    RepositoryRegistration.cs
    PipelineRegistration.cs    ← extended to register UnitOfWorkBehavior
```

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Required by `PipelineRegistration.AddPipeline()` to register `IPipelineBehavior<,>` |

# Allowed Dependencies
- Shared
- BuildingBlocks
- App.Infrastructure

# Rules

## MUST
- `IUnitOfWork` registered as `Scoped`
- `UnitOfWorkContext` registered as `Scoped`
- `UnitOfWorkBehavior` registered in `PipelineRegistration.AddPipeline()`
- `UnitOfWorkBehavior` registered last, after `ValidationBehavior`/`ConcurrencyBehavior`/`GuidResolvingBehavior` (whichever are applied)
- Never register `IUnitOfWork` or `UnitOfWorkContext` inside module registration methods
- Never register `UnitOfWorkBehavior` inside a module-specific registration method
- Never register `UnitOfWorkBehavior` before any other applied pipeline behavior

## SHOULD
- Avoid registering `IUnitOfWork` or `UnitOfWorkContext` in module registration — these are global services, belong in App.Host

# Check list
- [ ] `UnitOfWorkBehavior` added to `AddPipeline()` as the last behavior registered
