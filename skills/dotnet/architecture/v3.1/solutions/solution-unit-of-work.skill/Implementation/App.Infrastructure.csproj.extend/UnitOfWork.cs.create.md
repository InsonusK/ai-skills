---
description: IUnitOfWork implementation delegating to AppDbContext
project_name: App.Infrastructure
name: UnitOfWork.cs
element_kind: class
change_kind: create
tags:
  - solution/unit-of-work
  - element/unitofwork-cs
---

# Goals
- Implement `IUnitOfWork` by delegating to `AppDbContext.SaveChangesAsync`
- Be the single place in the entire solution where `DbContext.SaveChangesAsync` is called

# Core Principles
- Wraps `AppDbContext` — receives it via constructor injection
- Single method implementation — no transaction management, no retry logic
- Registered as `Scoped` — same `DbContext` instance as `Repository<T>`, ensuring all staged changes are committed together

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| UoW EF implementation | `UnitOfWork` | `UnitOfWork` | `UnitOfWork.cs` | `UnitOfWork.cs` |

# Implementation changes

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
# Rule changes

## MUST
- Implement `IUnitOfWork` from Shared
- Delegate to `AppDbContext.SaveChangesAsync` — no additional logic
- Registered as `Scoped`
- `IUnitOfWork` defined in Shared — single `SaveChangesAsync` method only
- `UnitOfWorkContext` defined in BuildingBlocks — registered as `Scoped`
- `UnitOfWorkBehavior` defined in BuildingBlocks — constrained to `ICommand` only
- `UnitOfWork` implementation in App.Infrastructure
- `UnitOfWorkBehavior` uses `try/finally` with `_context.Leave()` — depth always restored on exception
- `UnitOfWorkBehavior` commits only when `_context.Depth == 1`
- Never be called from anywhere except `UnitOfWorkBehavior`
- Never call `SaveChangesAsync` or inject `IUnitOfWork` from any handler
- Never let `UnitOfWorkBehavior` activate on queries — it is constrained to `ICommand`
- Never register `UnitOfWorkContext` as `Singleton` or `Transient`
- Never put logic in `UnitOfWork` beyond delegating to `DbContext.SaveChangesAsync`
- Never put a catch/rollback block in `UnitOfWorkBehavior` — EF implicit transactions do not require it
- Never `IUnitOfWork` defined in BuildingBlocks — belongs in Shared

## SHOULD
- Avoid `UnitOfWork` called directly from a handler — bypasses the pipeline and breaks atomicity guarantees
- Avoid `UnitOfWork` containing business logic — should be pure delegation

# Check list
- [ ] `UnitOfWork` implements `IUnitOfWork`
- [ ] Delegates directly to `AppDbContext.SaveChangesAsync`

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
