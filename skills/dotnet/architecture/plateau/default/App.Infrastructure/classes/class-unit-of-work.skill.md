---
name: class-unit-of-work
description: IUnitOfWork implementation delegating to AppDbContext
domain: skill
type: template
version: 20260616
plateau: default
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work.skill]]"
---

# Goal
- Implement `IUnitOfWork` by delegating to `AppDbContext.SaveChangesAsync`
- Be the single place in the entire solution where `DbContext.SaveChangesAsync` is called

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create.md|UnitOfWork.cs.create]]

# Core Principals
- Wraps `AppDbContext` — receives it via constructor injection
- Single method implementation — no transaction management, no retry logic
- Registered as `Scoped` — same `DbContext` instance as `Repository<T>`, ensuring all staged changes are committed together

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create.md|UnitOfWork.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| UoW EF implementation | `UnitOfWork` | `UnitOfWork` | `UnitOfWork.cs` | `UnitOfWork.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create.md|UnitOfWork.cs.create]]

# Implementation
```csharp
// App.Infrastructure/UnitOfWork/UnitOfWork.cs
using Shared.UnitOfWork;

namespace App.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
        => _dbContext = dbContext;

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _dbContext.SaveChangesAsync(ct);
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create.md|UnitOfWork.cs.create]]

# Rules
MUST:
	- Implement `IUnitOfWork` from Shared
	- Delegate to `AppDbContext.SaveChangesAsync` — no additional logic
	- Registered as `Scoped`
MUST NOT:
	- Contain transaction management logic — EF Core manages transactions implicitly via `SaveChangesAsync`
	- Be called from anywhere except `UnitOfWorkBehavior`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create.md|UnitOfWork.cs.create]]

# Anti-patterns
- `UnitOfWork` called directly from a handler — bypasses the pipeline and breaks atomicity guarantees
- `UnitOfWork` containing business logic — should be pure delegation

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create.md|UnitOfWork.cs.create]]

# Check list
- [ ] `UnitOfWork` implements `IUnitOfWork`
- [ ] Delegates directly to `AppDbContext.SaveChangesAsync`
- [ ] Registered as `Scoped`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create.md|UnitOfWork.cs.create]]

# Unittest TestCases
- [ ] WHEN inspected THEN it implement IUnitOfWork by delegating to AppDbContext.SaveChangesAsync
- [ ] WHEN applied THEN Be the single place in the entire solution where DbContext.SaveChangesAsync is called
- [ ] WHEN applied THEN Wraps AppDbContext — receives it via constructor injection
- [ ] WHEN applied THEN Single method implementation — no transaction management, no retry logic
- [ ] WHEN applied THEN Registered as Scoped — same DbContext instance as Repository<T>, ensuring all staged changes are committed together
- [ ] WHEN verified THEN UnitOfWork implements IUnitOfWork
- [ ] WHEN verified THEN Delegates directly to AppDbContext.SaveChangesAsync
- [ ] WHEN verified THEN Registered as Scoped
- [ ] WHEN naming 'UoW EF implementation' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create.md|UnitOfWork.cs.create]]
