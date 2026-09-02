---
name: plateau-offline-sync-service--class-repository
description: Class Repository<T> in the plateau-offline-sync-service plateau — the one generic EF Core repository, inheriting Ardalis RepositoryBase<T>, implementing Shared IRepository<T>
whenToUse: when editing the generic repository, or checking that data access stays behind IRepository / IReadRepository and named specs
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
---

# Goal
- Implement `Shared.Repositories.IRepository<T>` for every entity with one generic class, by inheriting Ardalis `RepositoryBase<T>` — no per-entity subclass, no manual spec evaluation or `AsNoTracking` boilerplate.

__Applied solutions:__
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/Repository.cs.create.md|Repository.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `Repository<T> : RepositoryBase<T>, IRepository<T> where T : class`, in `/App.Infrastructure/Repositories`.
- Constructor takes `AppDbContext` and forwards it to the Ardalis base.
- Never calls `SaveChangesAsync` — committing is `UnitOfWork`'s job.
- All reads take `ISpecification<T>` — no raw lambda / LINQ parameters, no inline predicates.
- Registered once as an open generic (`IRepository<>` and `IReadRepository<>` → `Repository<>`), `Scoped`.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-repository
// Plateau: domain-service
// Version: 20260902000000
using App.Infrastructure.Persistence;
using Ardalis.Specification.EntityFrameworkCore;
using Shared.Repositories;

namespace App.Infrastructure.Repositories;

public sealed class Repository<T>(AppDbContext dbContext) : RepositoryBase<T>(dbContext), IRepository<T>
    where T : class;
```

__Applied solutions:__
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/Repository.cs.create.md|Repository.cs.create]]

# Rules
MUST:
- Inherit `RepositoryBase<T>`, implement `IRepository<T>`, forward `AppDbContext` to the base.
- Never call `SaveChangesAsync`; never create a per-entity subclass; never accept an inline predicate.
- Never apply several plateau templates per class.

# Check list
- [ ] `Repository<T> : RepositoryBase<T>, IRepository<T>` in `/Repositories`.
- [ ] Constructor forwards `AppDbContext`; no `SaveChangesAsync` call; no per-entity subclass.

# Unittest TestCases
- [ ] WHEN a named spec is passed THEN the repository returns the matching entities.
- [ ] WHEN the repository is used THEN no `SaveChangesAsync` is invoked.
