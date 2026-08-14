---
description: Generic EF Core repository inheriting Ardalis RepositoryBase<T>
project_name: App.Infrastructure
name: Repository.cs
element_kind: class
change_kind: create
tags:
  - solution/repository-integration
  - element/repository-cs
---

# Goals
- Implement `IRepository<T>` by leveraging Ardalis `RepositoryBase<T>`
- Eliminate manual spec evaluation and `AsNoTracking` boilerplate

# Core Principles
- `RepositoryBase<T>` internally uses `SpecificationEvaluator` and applies `AsNoTracking` where appropriate
- Constructor receives `AppDbContext` via DI and forwards it to the Ardalis base

# Structure

## Project Structure
```
/App.Infrastructure
  /Repositories
    Repository.cs
```

# Implementation changes

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
# Rule changes

## MUST
- Inherit `RepositoryBase<T>` from `Ardalis.Specification.EntityFrameworkCore`
- Implement `IRepository<T>` from Shared
- Constructor accept `AppDbContext` and pass to base
- `IReadRepository<T>` and `IRepository<T>` defined in Shared, inheriting Ardalis base interfaces
- `IRepository<T>` extends `IReadRepository<T>` and `IRepositoryBase<T>`
- `IRepository<T>` has no `SaveChangesAsync` — committing belongs to Unit of Work
- All repository read methods accept `ISpecification<T>` — no raw lambda or LINQ parameters
- Command handlers inject `IRepository<T>`
- Query handlers inject `IReadRepository<T>`

## MUST NOT
- Contain inline LINQ predicates
- Create per-entity subclasses
- `Repository<T>` call `SaveChangesAsync`
# Check list
- [ ] Inherits `RepositoryBase<T>`
- [ ] Implements `IRepository<T>`
- [ ] Constructor forwards `AppDbContext` to base
- [ ] No `SaveChangesAsync` calls

# Unittest TestCases
- [ ] WHEN inspected THEN it implement IRepository<T> by leveraging Ardalis RepositoryBase<T>
- [ ] WHEN applied THEN Eliminate manual spec evaluation and AsNoTracking boilerplate
- [ ] WHEN applied THEN RepositoryBase<T> internally uses SpecificationEvaluator and applies AsNoTracking where appropriate
- [ ] WHEN applied THEN Constructor receives AppDbContext via DI and forwards it to the Ardalis base
- [ ] WHEN verified THEN Inherits RepositoryBase<T>
- [ ] WHEN verified THEN Implements IRepository<T>
- [ ] WHEN verified THEN Constructor forwards AppDbContext to base
- [ ] WHEN verified THEN No SaveChangesAsync calls
