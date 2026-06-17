---
description: Register IUnitOfWork and UnitOfWorkContext with scoped lifetimes
name: App.Host.csproj
element_kind: project
change_kind: extend
---

# Goals
- Register `IUnitOfWork` and `UnitOfWorkContext` with correct lifetimes

# Core Principles
- `IUnitOfWork` and `UnitOfWorkContext` share `Scoped` lifetime with `DbContext` and `Repository<T>`

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    RepositoryRegistration.cs
```

# Allowed Dependencies
- Shared
- BuildingBlocks
- App.Infrastructure

# Rules

MUST:
- `IUnitOfWork` registered as `Scoped`
- `UnitOfWorkContext` registered as `Scoped`

MUST NOT:
- Register `IUnitOfWork` or `UnitOfWorkContext` inside module registration methods

# Anti-patterns
- Registering `IUnitOfWork` or `UnitOfWorkContext` in module registration — these are global services, belong in App.Host

# Check list
- [ ] `IUnitOfWork` registered as `Scoped`
- [ ] `UnitOfWorkContext` registered as `Scoped`
