---
name: class-i-unit-of-work
description: Single-method commit contract accessible by every layer
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]]"
---

# Goal
- Define the single commit contract — exactly one method, exactly one responsibility
- Live in Shared so every layer can reference it without depending on infrastructure

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create|IUnitOfWork.cs]]

# Core Principles
- Apply ONE plateau template per class
- Single method only: `SaveChangesAsync(CancellationToken)` — nothing else
- Implementation in App.Infrastructure — Shared holds only the interface
- Registered as `Scoped` — shares the same DbContext instance as `Repository<T>` within the request

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create|IUnitOfWork.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Unit of work interface | `IUnitOfWork` | `IUnitOfWork` | `IUnitOfWork.cs` | `IUnitOfWork.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create|IUnitOfWork.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-i-unit-of-work
//Plateau: default
//Version: 20260628
```

```csharp
// Shared/UnitOfWork/IUnitOfWork.cs
namespace Shared.UnitOfWork;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create|IUnitOfWork.cs]]

# Rules
MUST:
	- Single method only — `SaveChangesAsync(CancellationToken ct = default)`
	- No additional methods — not `BeginTransaction`, not `Rollback`, not `Commit`
MUST NOT:
	- Contain any implementation — interface only

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create|IUnitOfWork.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Define the single commit contract — exactly one method, exactly one responsibility
- [ ] WHEN applied THEN Live in Shared so every layer can reference it without depending on infrastructure
- [ ] WHEN applied THEN Single method only: SaveChangesAsync(CancellationToken) — nothing else
- [ ] WHEN applied THEN Implementation in App.Infrastructure — Shared holds only the interface
- [ ] WHEN applied THEN Registered as Scoped — shares the same DbContext instance as Repository<T> within the request
- [ ] WHEN naming 'Unit of work interface' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create|IUnitOfWork.cs]]
