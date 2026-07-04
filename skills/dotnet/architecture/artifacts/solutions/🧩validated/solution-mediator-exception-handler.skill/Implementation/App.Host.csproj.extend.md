---
description: Register ExceptionHandlingBehavior last in the MediatR pipeline chain
name: App.Host.csproj
element_kind: project
change_kind: extend
---

# Goals
- Wire `ExceptionHandlingBehavior` into the centralized `PipelineRegistration.AddPipeline()` extension
- Ensure the behavior is registered after all other pipeline behaviors

# Core Principles
- Pipeline behavior registration is centralized in App.Host
- `AddPipeline()` is the single place where cross-cutting behaviors are ordered
- `ExceptionHandlingBehavior` is registered last so it wraps the handler and any behaviors registered before it

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
- `ExceptionHandlingBehavior` registered after all other pipeline behaviors

## MUST NOT
- Register `ExceptionHandlingBehavior` inside a module-specific registration method
- Register `ExceptionHandlingBehavior` before other pipeline behaviors

# Anti-patterns
- Registering the exception handler in a module instead of App.Host
- Changing the pipeline order in multiple files

# Check list
- [ ] `PipelineRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] `ExceptionHandlingBehavior` added to `AddPipeline()`
- [ ] `ExceptionHandlingBehavior` is the last behavior registered
