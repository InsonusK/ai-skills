---
description: Add Ardalis.Specification.EntityFrameworkCore and implement Repository<T>
name: App.Infrastructure.csproj
element_kind: project
change_kind: extend
---

# Goals
- Provide the single generic `Repository<T>` EF Core implementation
- Leverage `RepositoryBase<T>` from Ardalis to eliminate boilerplate spec evaluation code

# Core Principles
- `RepositoryBase<T>` from Ardalis handles all `SpecificationEvaluator` logic internally
- App.Infrastructure remains the only layer that knows about EF Core implementation details
- One generic `Repository<T>` class covers all entity types

# Structure

## Project Structure
```
/App.Infrastructure
  /Repositories
    Repository.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Repositories/Repository.cs | Generic EF Core repository implementation inheriting Ardalis `RepositoryBase<T>` |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Ardalis.Specification.EntityFrameworkCore` | latest stable | Provides `RepositoryBase<T>` and EF Core spec evaluator |

# Allowed Dependencies
- BuildingBlocks
- Shared
- `{ModuleName}.Domain` (all modules)
- `{ModuleName}.Interfaces` (all modules)

# Rules

## MUST
- Reference `Ardalis.Specification.EntityFrameworkCore`
- Single generic `Repository<T>` inheriting `RepositoryBase<T>`
- Constructor accept `AppDbContext` and pass it to base
- Implement `IRepository<T>` from Shared

## MUST NOT
- Call `SaveChangesAsync` inside `Repository<T>`
- Create per-entity repository subclasses

# Anti-patterns
- `TaskRepository : Repository<TodoTask>` — unnecessary subclass, generic handles all types
- Manual `SpecificationEvaluator.Default.GetQuery(...)` calls — Ardalis base handles this

# Check list
- [ ] `Ardalis.Specification.EntityFrameworkCore` referenced
- [ ] `Repository<T>` inherits `RepositoryBase<T>`
- [ ] Constructor forwards `AppDbContext` to base
- [ ] `Repository<T>` implements `IRepository<T>`
- [ ] No `SaveChangesAsync` calls in repository
