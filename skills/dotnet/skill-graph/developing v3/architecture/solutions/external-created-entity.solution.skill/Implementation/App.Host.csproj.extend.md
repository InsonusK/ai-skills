---
description: Register GuidResolvingBehavior in pipeline
name: App.Host.csproj
change_kind: extend
---

# Goals
- Register `GuidResolvingBehavior` in the pipeline between `ValidationBehavior` and `ConcurrencyBehavior`

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs      ← extended with GuidResolvingBehavior
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /DependencyInjection/PipelineRegistration.cs | Insert GuidResolvingBehavior between ValidationBehavior and ConcurrencyBehavior |

# Allowed Dependencies
- Shared
- BuildingBlocks
- App.Infrastructure
- All module Application and Api projects

# Rules

MUST:
- `GuidResolvingBehavior` registered after `ValidationBehavior` and before `ConcurrencyBehavior`
- Registered as `Transient` open generic — DI resolves `IGuidResolver<TResponse>` per command type

MUST NOT:
- `GuidResolvingBehavior` registered after `UnitOfWorkBehavior` — duplicate commands would open a unit of work

# Anti-patterns
- `GuidResolvingBehavior` registered after `UnitOfWorkBehavior` — duplicate commands open a unit of work unnecessarily

# Check list
- [ ] `GuidResolvingBehavior` registered between `ValidationBehavior` and `ConcurrencyBehavior`
