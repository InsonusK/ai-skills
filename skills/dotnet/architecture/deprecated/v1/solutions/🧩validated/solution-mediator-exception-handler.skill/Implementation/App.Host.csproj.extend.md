---
description: Register ExceptionHandlingBehavior first in the MediatR pipeline chain
name: App.Host.csproj
element_kind: project
change_kind: extend
tags:
  - solution/mediator-exception-handler
  - element/app-host-csproj
---

# Goals
- Wire `ExceptionHandlingBehavior` into the centralized `PipelineRegistration.AddPipeline()` extension
- Ensure the behavior is registered before all other pipeline behaviors

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
- `ExceptionHandlingBehavior` registered in `PipelineRegistration.AddPipeline()`
- `ExceptionHandlingBehavior` registered before all other pipeline behaviors

## MUST NOT
- Register `ExceptionHandlingBehavior` inside a module-specific registration method
- Register `ExceptionHandlingBehavior` after other pipeline behaviors

# Anti-patterns
- Registering the exception handler in a module instead of App.Host
- Changing the pipeline order in multiple files

# Check list
- [ ] `PipelineRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] `ExceptionHandlingBehavior` added to `AddPipeline()`
- [ ] `ExceptionHandlingBehavior` is the first behavior registered
