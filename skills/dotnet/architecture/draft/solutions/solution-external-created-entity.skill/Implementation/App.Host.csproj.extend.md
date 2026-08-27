---
description: Register GuidResolvingBehavior in the MediatR pipeline chain
name: App.Host.csproj
element_kind: project
change_kind: extend
tags:
  - solution/external-created-entity
  - element/app-host-csproj
---

# Goals
- Wire `GuidResolvingBehavior` into the centralized `PipelineRegistration.AddPipeline()` extension
- Ensure the behavior is registered after `ConcurrencyBehavior` (when applied) and before `UnitOfWorkBehavior`

# Core Principles
- Pipeline behavior registration is centralized in App.Host
- `AddPipeline()` is the single place where cross-cutting behaviors are ordered

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs
```

## Directory and class skills
| Directory | file | Description |
| ----------------- | --------------------- | -------------------------------------------------- |
| /DependencyInjection | PipelineRegistration.cs | Centralized registration of all MediatR pipeline behaviors |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Required by `PipelineRegistration.AddPipeline()` to register `IPipelineBehavior<,>` |

# Allowed Dependencies
- BuildingBlocks

# Rules

## MUST
- `GuidResolvingBehavior` registered in `PipelineRegistration.AddPipeline()`
- `GuidResolvingBehavior` registered after `ValidationBehavior`/`ConcurrencyBehavior` (whichever are applied) and before `UnitOfWorkBehavior` (when applied)

## MUST NOT
- Register `GuidResolvingBehavior` inside a module-specific registration method
- Register `GuidResolvingBehavior` after `UnitOfWorkBehavior`

# Check list
- [ ] `PipelineRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] `GuidResolvingBehavior` added to `AddPipeline()`
- [ ] `GuidResolvingBehavior` follows `ConcurrencyBehavior` and precedes `UnitOfWorkBehavior`
