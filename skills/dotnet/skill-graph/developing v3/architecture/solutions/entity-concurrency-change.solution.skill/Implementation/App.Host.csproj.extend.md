---
description: Register IEntityVersionResolver and ConcurrencyBehavior in pipeline
name: App.Host.csproj
element_kind: project
change_kind: extend
---

# Goals
- Register `IEntityVersionResolver` as `Singleton`
- Register `ConcurrencyBehavior` in pipeline between `ValidationBehavior` and `UnitOfWorkBehavior`

# Core Principles
- `EntityVersionResolver` registered as `Singleton` — static map, safe for singleton lifetime
- `ConcurrencyBehavior` registered after `ValidationBehavior` and before `UnitOfWorkBehavior` — invalid or stale commands never open a unit of work

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs      ← extended with ConcurrencyBehavior
    RepositoryRegistration.cs    ← extended with EntityVersionResolver
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /DependencyInjection/PipelineRegistration.cs | Insert ConcurrencyBehavior between ValidationBehavior and UnitOfWorkBehavior |
| /DependencyInjection/RepositoryRegistration.cs | Register IEntityVersionResolver as Singleton |

# Allowed Dependencies
- Shared
- BuildingBlocks
- App.Infrastructure
- All module Application and Api projects

# Rules

MUST:
- `EntityVersionResolver` registered as `Singleton` — static map, safe for singleton lifetime
- `ConcurrencyBehavior` registered after `ValidationBehavior` and before `UnitOfWorkBehavior`

MUST NOT:
- `ConcurrencyBehavior` registered after `UnitOfWorkBehavior` — stale commands would open a unit of work

# Anti-patterns
- `ConcurrencyBehavior` registered after `UnitOfWorkBehavior` — stale commands open a unit of work unnecessarily

# Check list
- [ ] `EntityVersionResolver` registered as `Singleton`
- [ ] `ConcurrencyBehavior` registered between `ValidationBehavior` and `UnitOfWorkBehavior`
