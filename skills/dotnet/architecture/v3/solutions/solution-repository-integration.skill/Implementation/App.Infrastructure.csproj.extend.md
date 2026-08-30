---
description: Add AppDbContext, Ardalis.Specification.EntityFrameworkCore, and implement Repository<T>
name: App.Infrastructure.csproj
element_kind: project
change_kind: extend
tags:
  - solution/repository-integration
  - element/app-infrastructure-csproj
---

# Goals
- Define `AppDbContext`, the service's single `DbContext`
- Provide the single generic `Repository<T>` EF Core implementation
- Leverage `RepositoryBase<T>` from Ardalis to eliminate boilerplate spec evaluation code

# Core Principles
- `AppDbContext` is the only `DbContext` in the service — it applies every module's entity configurations via `ApplyConfigurationsFromAssembly`, scanning each module's Domain assembly
- `RepositoryBase<T>` from Ardalis handles all `SpecificationEvaluator` logic internally
- App.Infrastructure remains the only layer that knows about EF Core implementation details

# Implementation changes

**AS IS** (from `solution-infrastructure-project`) — the project exists, empty:
```
/App.Infrastructure
  App.Infrastructure.csproj
```

**TO BE** (after this solution):
```
/App.Infrastructure
  /Persistence
    AppDbContext.cs
  /Repositories
    Repository.cs
  App.Infrastructure.csproj
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Persistence/AppDbContext.cs | The service's single `DbContext`, applies every module's `IEntityTypeConfiguration<T>` |
| /Repositories/Repository.cs | Generic EF Core repository implementation inheriting Ardalis `RepositoryBase<T>` |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides `DbContext`, `ApplyConfigurationsFromAssembly` |
| `Ardalis.Specification.EntityFrameworkCore` | latest stable | Provides `RepositoryBase<T>` and EF Core spec evaluator |

# Allowed Dependencies
- BuildingBlocks
- Shared
- `{ModuleName}.Domain` (all modules)
- `{ModuleName}.Interfaces` (all modules)

# Rules

## MUST
- Define `AppDbContext : DbContext`, applying every module's configurations via `ApplyConfigurationsFromAssembly` scanning module Domain assemblies
- Reference `Ardalis.Specification.EntityFrameworkCore`
- Single generic `Repository<T>` inheriting `RepositoryBase<T>`
- Constructor accept `AppDbContext` and pass it to base
- Implement `IRepository<T>` from Shared

## MUST NOT
- Call `SaveChangesAsync` inside `Repository<T>`
- Create per-entity repository subclasses
- Configure entities inline in `AppDbContext.OnModelCreating` beyond the single `ApplyConfigurationsFromAssembly` call — per-entity mapping belongs in that entity's own `{Entity}Config` (see `solution-domain-configuration`)

# Anti-patterns
- `TaskRepository : Repository<TodoTask>` — unnecessary subclass, generic handles all types
- Manual `SpecificationEvaluator.Default.GetQuery(...)` calls — Ardalis base handles this

# Check list
- [ ] `AppDbContext` defined in `/Persistence`, applies configurations via `ApplyConfigurationsFromAssembly`
- [ ] `Ardalis.Specification.EntityFrameworkCore` referenced
- [ ] `Repository<T>` inherits `RepositoryBase<T>`
- [ ] Constructor forwards `AppDbContext` to base
- [ ] `Repository<T>` implements `IRepository<T>`
- [ ] No `SaveChangesAsync` calls in repository
