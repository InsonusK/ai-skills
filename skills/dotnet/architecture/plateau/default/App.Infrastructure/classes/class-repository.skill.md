---
uid: 6e894086-873c-47e5-bd65-9d994962a408
name: class-repository
description: Generic EF Core repository inheriting Ardalis RepositoryBase<T>
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration.skill]]"
---

# Goal
- Implement `IRepository<T>` by leveraging Ardalis `RepositoryBase<T>`
- Eliminate manual spec evaluation and `AsNoTracking` boilerplate

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/Repository.cs.create.md|Repository.cs.create]]

# Core Principals
- `RepositoryBase<T>` internally uses `SpecificationEvaluator` and applies `AsNoTracking` where appropriate
- Constructor receives `AppDbContext` via DI and forwards it to the Ardalis base

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/Repository.cs.create.md|Repository.cs.create]]

# Implementation
```csharp
// App.Infrastructure/Repositories/Repository.cs
using Ardalis.Specification.EntityFrameworkCore;
using Shared.Repositories;

namespace App.Infrastructure.Repositories;

public class Repository<T> : RepositoryBase<T>, IRepository<T>
    where T : class
{
    public Repository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/Repository.cs.create.md|Repository.cs.create]]

# Rules
MUST:
	- Inherit `RepositoryBase<T>` from `Ardalis.Specification.EntityFrameworkCore`
	- Implement `IRepository<T>` from Shared
	- Constructor accept `AppDbContext` and pass to base
MUST NOT:
	- Call `SaveChangesAsync`
	- Contain inline LINQ predicates
	- Create per-entity subclasses

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/Repository.cs.create.md|Repository.cs.create]]

# Check list
- [ ] Inherits `RepositoryBase<T>`
- [ ] Implements `IRepository<T>`
- [ ] Constructor forwards `AppDbContext` to base
- [ ] No `SaveChangesAsync` calls

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/Repository.cs.create.md|Repository.cs.create]]

# Unittest TestCases
- [ ] WHEN inspected THEN it implement IRepository<T> by leveraging Ardalis RepositoryBase<T>
- [ ] WHEN applied THEN Eliminate manual spec evaluation and AsNoTracking boilerplate
- [ ] WHEN applied THEN RepositoryBase<T> internally uses SpecificationEvaluator and applies AsNoTracking where appropriate
- [ ] WHEN applied THEN Constructor receives AppDbContext via DI and forwards it to the Ardalis base
- [ ] WHEN verified THEN Inherits RepositoryBase<T>
- [ ] WHEN verified THEN Implements IRepository<T>
- [ ] WHEN verified THEN Constructor forwards AppDbContext to base
- [ ] WHEN verified THEN No SaveChangesAsync calls

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/Repository.cs.create.md|Repository.cs.create]]
