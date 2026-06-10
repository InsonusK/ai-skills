---
description: Single-method commit contract accessible by every layer
project_name: Shared
name: IUnitOfWork.cs
change_kind: create
---

# Goals
- Define the single commit contract — exactly one method, exactly one responsibility
- Live in Shared so every layer can reference it without depending on infrastructure

# Core Principles
- Single method only: `SaveChangesAsync(CancellationToken)` — nothing else
- Implementation in App.Infrastructure — Shared holds only the interface
- Registered as `Scoped` — shares the same DbContext instance as `Repository<T>` within the request

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Unit of work interface | `IUnitOfWork` | `IUnitOfWork` | `IUnitOfWork.cs` | `IUnitOfWork.cs` |

# Implementation changes

```csharp
// Shared/UnitOfWork/IUnitOfWork.cs
namespace Shared.UnitOfWork;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

# Rules

MUST:
- Single method only — `SaveChangesAsync(CancellationToken ct = default)`
- No additional methods — not `BeginTransaction`, not `Rollback`, not `Commit`

MUST NOT:
- Contain any implementation — interface only
