---
uid: c86adb4a-156a-4fff-bd9d-600c7d0643e7
name: iunitofwork-class
description: Single-method commit contract accessible by every layer
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work.solution.skill]]"
---

# Goal
- Define the single commit contract — exactly one method, exactly one responsibility
- Live in Shared so every layer can reference it without depending on infrastructure

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create.md|IUnitOfWork.cs.create]]

# Core Principals
- Single method only: `SaveChangesAsync(CancellationToken)` — nothing else
- Implementation in App.Infrastructure — Shared holds only the interface
- Registered as `Scoped` — shares the same DbContext instance as `Repository<T>` within the request

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create.md|IUnitOfWork.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Unit of work interface | `IUnitOfWork` | `IUnitOfWork` | `IUnitOfWork.cs` | `IUnitOfWork.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create.md|IUnitOfWork.cs.create]]

# Implementation
```csharp
// Shared/UnitOfWork/IUnitOfWork.cs
namespace Shared.UnitOfWork;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create.md|IUnitOfWork.cs.create]]

# Rules
MUST:
	- Single method only — `SaveChangesAsync(CancellationToken ct = default)`
	- No additional methods — not `BeginTransaction`, not `Rollback`, not `Commit`
MUST NOT:
	- Contain any implementation — interface only

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create.md|IUnitOfWork.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Define the single commit contract — exactly one method, exactly one responsibility
- [ ] WHEN applied THEN Live in Shared so every layer can reference it without depending on infrastructure
- [ ] WHEN applied THEN Single method only: SaveChangesAsync(CancellationToken) — nothing else
- [ ] WHEN applied THEN Implementation in App.Infrastructure — Shared holds only the interface
- [ ] WHEN applied THEN Registered as Scoped — shares the same DbContext instance as Repository<T> within the request
- [ ] WHEN naming 'Unit of work interface' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create.md|IUnitOfWork.cs.create]]
