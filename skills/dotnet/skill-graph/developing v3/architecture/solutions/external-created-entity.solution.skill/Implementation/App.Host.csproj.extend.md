---
description: Register GuidResolvingBehavior in pipeline and extend MiddlewareRegistration with ConflictExceptionMiddleware
name: App.Host.csproj
element_kind: project
change_kind: extend
---

# Goals
- Register `GuidResolvingBehavior` in the MediatR pipeline between `ValidationBehavior` and `ConcurrencyBehavior`
- Register `ConflictExceptionMiddleware` in the centralized HTTP middleware pipeline so it wraps all controller invocations

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs      ← extended with GuidResolvingBehavior
    MiddlewareRegistration.cs    ← extended with ConflictExceptionMiddleware
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /DependencyInjection/PipelineRegistration.cs | Insert GuidResolvingBehavior between ValidationBehavior and ConcurrencyBehavior |
| /DependencyInjection/MiddlewareRegistration.cs | Register ConflictExceptionMiddleware in the centralized HTTP middleware pipeline |

# Allowed Dependencies
- Shared
- BuildingBlocks
- App.Infrastructure
- All module Application and Api projects

# Rules

MUST:
- `GuidResolvingBehavior` registered after `ValidationBehavior` and before `ConcurrencyBehavior`
- `GuidResolvingBehavior` registered as `Transient` open generic
- `ConflictExceptionMiddleware` registered inside `UseMiddlewarePipeline()`
- `UseMiddlewarePipeline()` called before `MapControllers()` or endpoint routing so it wraps endpoint execution

MUST NOT:
- `GuidResolvingBehavior` registered after `UnitOfWorkBehavior` — duplicate commands would open a unit of work
- Register `ConflictExceptionMiddleware` after `MapControllers()` without wrapping endpoints — it must catch pipeline exceptions

# Anti-patterns
- `GuidResolvingBehavior` registered after `UnitOfWorkBehavior` — duplicate commands open a unit of work unnecessarily
- Registering `ConflictExceptionMiddleware` only after `MapControllers()` — won't catch exceptions thrown inside endpoint handlers

# Check list
- [ ] `GuidResolvingBehavior` registered between `ValidationBehavior` and `ConcurrencyBehavior`
- [ ] `ConflictExceptionMiddleware` registered inside `UseMiddlewarePipeline()`
