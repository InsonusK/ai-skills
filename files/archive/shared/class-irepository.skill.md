---
name: class-irepository
description: defines IRepository and IReadRepository — the repository abstractions used by Application handlers
domain: skill
type: class
tags:
  - skill/pattern/class
  - dotnet
  - repository
  - application
triggers:
  - IRepository interface
  - IReadRepository interface
  - repository abstraction
---
# Goal
Define `IRepository<T>` and `IReadRepository<T>`. These are the only data access abstractions Application handlers may use. They decouple handlers from EF Core and DbContext. `IReadRepository<T>` signals read intent — query handlers inject this. `IRepository<T>` signals write intent — command handlers inject this.

# Governed by
- solution-command-handling.skill.md — handlers use these, never DbContext directly

# Structure
## Place in csproj
Defined in `csproj-shared.skill.md`
```
/Shared
  /Repositories
    IReadRepository.cs
    IRepository.cs
```

## Naming convention
```
interface names: IReadRepository<T>, IRepository<T>
file names: IReadRepository.cs, IRepository.cs
```

# Contracts

```csharp
// IReadRepository.cs
public interface IReadRepository<T> where T : class
{
    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> spec, CancellationToken ct = default);
    Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec, CancellationToken ct = default);
    Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default);
}

// IRepository.cs
public interface IRepository<T> : IReadRepository<T> where T : class
{
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}
```

# Rules
MUST:
- Command handlers inject `IRepository<T>`
- Query handlers inject `IReadRepository<T>`
- `IRepository<T>` never exposes `SaveChangesAsync` — UnitOfWork owns commits
MUST NOT:
- Handler inject DbContext directly
- Query handler inject `IRepository<T>` — signals wrong intent

# Anti-patterns
- `IRepository<T>` has `SaveChangesAsync` — breaks UnitOfWork atomicity
- Query handler injects `IRepository<T>` — use `IReadRepository<T>` to signal read intent

# Relations
- csproj-shared.skill.md — lives here
- class-iunit-of-work.skill.md — SaveChanges belongs here, not on repository
- solution-command-handling.skill.md — handlers use these abstractions
- class-repository.skill.md — Application usage patterns
