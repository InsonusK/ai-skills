---
description: Register ValidationBehavior in the MediatR pipeline chain
name: App.Host.csproj
element_kind: project
change_kind: extend
tags:
  - solution/validation-behavior
  - element/app-host-csproj
---

# Goals
- Wire `ValidationBehavior` into the centralized `PipelineRegistration.AddPipeline()` extension
- Ensure the behavior is registered right after `ExceptionHandlingBehavior` (when applied) and before any behavior that assumes a validated request

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
- `ValidationBehavior` registered in `PipelineRegistration.AddPipeline()`
- `ValidationBehavior` registered after `ExceptionHandlingBehavior` (when applied) and before `ConcurrencyBehavior`/`GuidResolvingBehavior`/`UnitOfWorkBehavior` (whichever are applied)

## MUST NOT
- Register `ValidationBehavior` inside a module-specific registration method
- Register `ValidationBehavior` after a behavior that assumes a validated request

# Check list
- [ ] `PipelineRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] `ValidationBehavior` added to `AddPipeline()`
- [ ] `ValidationBehavior` follows `ExceptionHandlingBehavior` and precedes any downstream write-guard behavior
