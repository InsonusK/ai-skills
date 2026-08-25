---
name: class-repository
description: Class Repository<T> in the v1 plateau
whenToUse: when reviewing the generic EF Core repository implementation
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
---

# Goal
- Provide the single generic `Repository<T>` implementation every entity type shares, eliminating boilerplate spec evaluation

# Core Principles
- Inherits Ardalis's `RepositoryBase<T>`, which handles all `SpecificationEvaluator` logic internally
- Never calls `SaveChangesAsync` — staging only, commit is `IUnitOfWork`'s job

# Implementation
```csharp
//Skill: class-repository
//Plateau: v1
//Version: 20260825140000

public sealed class Repository<T>(AppDbContext context)
    : RepositoryBase<T>(context), IRepository<T> where T : class
{
}
```

# Rules
MUST:
- Implement `IRepository<T>` (and transitively `IReadRepository<T>`)
- Constructor accept `AppDbContext` and pass it to the Ardalis base
MUST NOT:
- Call `SaveChangesAsync`
- Have a per-entity subclass — the open generic covers every entity type

# Check list
- [ ] `Repository<T>` inherits `RepositoryBase<T>`, implements `IRepository<T>`
- [ ] No `SaveChangesAsync` call
- [ ] No per-entity repository subclass exists anywhere

__Applied solutions:__
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/Repository.cs.create.md|Repository.cs.create]]
