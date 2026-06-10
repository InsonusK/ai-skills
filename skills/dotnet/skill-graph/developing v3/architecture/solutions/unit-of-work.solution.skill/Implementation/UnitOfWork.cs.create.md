---
description: IUnitOfWork implementation delegating to AppDbContext
project_name: App.Infrastructure
name: UnitOfWork.cs
change_kind: create
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

# Rules

MUST:
- Implement `IUnitOfWork` from Shared
- Delegate to `AppDbContext.SaveChangesAsync` — no additional logic
- Registered as `Scoped`

MUST NOT:
- Contain transaction management logic — EF Core manages transactions implicitly via `SaveChangesAsync`
- Be called from anywhere except `UnitOfWorkBehavior`

# Anti-patterns
- `UnitOfWork` called directly from a handler — bypasses the pipeline and breaks atomicity guarantees
- `UnitOfWork` containing business logic — should be pure delegation

# Check list
- [ ] `UnitOfWork` implements `IUnitOfWork`
- [ ] Delegates directly to `AppDbContext.SaveChangesAsync`
- [ ] Registered as `Scoped`
