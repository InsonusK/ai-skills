---
description: Register IEntityVersionResolver and ConcurrencyBehavior in pipeline
name: App.Host.csproj
element_kind: project
change_kind: extend
---

# Goals
- Register `IEntityVersionResolver` as `Singleton` with module Domain assemblies
- Register `ConcurrencyBehavior` in pipeline between `ValidationBehavior` and `UnitOfWorkBehavior`

# Core Principles
- `EntityVersionResolver` registered as `Singleton` — map is built once at startup, safe for singleton lifetime
- `EntityVersionResolver` receives module Domain assemblies from App.Host — the only project that references all modules
- `ConcurrencyBehavior` registered after `ValidationBehavior` and before `UnitOfWorkBehavior` — invalid or stale commands never open a unit of work

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs                 ← extended with ConcurrencyBehavior
    EntityVersionResolverRegistration.cs    ← created to register EntityVersionResolver
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /DependencyInjection/PipelineRegistration.cs | Insert ConcurrencyBehavior between ValidationBehavior and UnitOfWorkBehavior |
| /DependencyInjection/EntityVersionResolverRegistration.cs | Register IEntityVersionResolver as Singleton with module Domain assemblies |

# Allowed Dependencies
- Shared
- BuildingBlocks
- App.Infrastructure
- All module Application and Api projects

# Rules

MUST:
- `EntityVersionResolver` registered as `Singleton`
- `EntityVersionResolver` receives all module Domain assemblies that contain versioned entities
- `ConcurrencyBehavior` registered after `ValidationBehavior` and before `UnitOfWorkBehavior`

MUST NOT:
- `ConcurrencyBehavior` registered after `UnitOfWorkBehavior` — stale commands would open a unit of work
- Change the signature of `RepositoryRegistration.AddRepositories`

# Anti-patterns
- `ConcurrencyBehavior` registered after `UnitOfWorkBehavior` — stale commands open a unit of work unnecessarily
- Passing non-Domain assemblies to `EntityVersionResolver` — scans unrelated types

# Check list
- [ ] `EntityVersionResolver` registered as `Singleton`
- [ ] `EntityVersionResolver` receives module Domain assemblies
- [ ] `ConcurrencyBehavior` registered between `ValidationBehavior` and `UnitOfWorkBehavior`

# Unittest TestCases
- [ ] WHEN applied THEN Register IEntityVersionResolver as Singleton with module Domain assemblies
- [ ] WHEN applied THEN Register ConcurrencyBehavior in pipeline between ValidationBehavior and UnitOfWorkBehavior
