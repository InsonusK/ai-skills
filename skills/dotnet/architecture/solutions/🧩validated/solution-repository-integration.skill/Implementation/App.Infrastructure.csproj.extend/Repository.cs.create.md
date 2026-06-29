---
description: Generic EF Core repository inheriting Ardalis RepositoryBase<T>
project_name: App.Infrastructure
name: Repository.cs
element_kind: class
change_kind: create
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

# Rules

MUST:
- Inherit `RepositoryBase<T>` from `Ardalis.Specification.EntityFrameworkCore`
- Implement `IRepository<T>` from Shared
- Constructor accept `AppDbContext` and pass to base

MUST NOT:
- Call `SaveChangesAsync`
- Contain inline LINQ predicates
- Create per-entity subclasses

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
