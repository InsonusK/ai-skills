---
description: Register IUnitOfWork, UnitOfWorkContext, and wire UnitOfWorkBehavior after ValidationBehavior
name: App.Host.csproj
change_kind: extend
---

# Goals
- Register `IUnitOfWork` and `UnitOfWorkContext` with correct lifetimes
- Register `UnitOfWorkBehavior` in the pipeline after `ValidationBehavior`

# Core Principles
- `IUnitOfWork` and `UnitOfWorkContext` share `Scoped` lifetime with `DbContext` and `Repository<T>`
- Pipeline behaviors registered in execution order — `ValidationBehavior` first, then `UnitOfWorkBehavior`

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    RepositoryRegistration.cs
    PipelineRegistration.cs
```

# Allowed Dependencies
- Shared
- BuildingBlocks
- App.Infrastructure

# Rules

MUST:
- `IUnitOfWork` registered as `Scoped`
- `UnitOfWorkContext` registered as `Scoped`
- `UnitOfWorkBehavior` registered after `ValidationBehavior` in pipeline

MUST NOT:
- Register pipeline behaviors inside module registration methods
- Change pipeline order in multiple files

# Anti-patterns
- `UnitOfWorkBehavior` registered before `ValidationBehavior` — would waste a commit attempt on invalid input
- Registering behaviors in module registration — pipeline is global, belongs in App.Host

# Check list
- [ ] `IUnitOfWork` registered as `Scoped`
- [ ] `UnitOfWorkContext` registered as `Scoped`
- [ ] `UnitOfWorkBehavior` registered after `ValidationBehavior`
- [ ] All pipeline registrations centralized in App.Host
