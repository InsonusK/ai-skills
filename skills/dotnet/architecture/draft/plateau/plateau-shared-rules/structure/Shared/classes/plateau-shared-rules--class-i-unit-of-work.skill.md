---
name: class-i-unit-of-work
description: Class IUnitOfWork in the shared-rules plateau
whenToUse: when reviewing where staged entity changes get committed
domain: skill
type: template
plateau: shared-rules
version: 20260824163000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]"
---

# Goal
- Define the single commit contract every layer can reference without coupling to EF Core

# Core Principles
- `IUnitOfWork` is the only component that calls `SaveChangesAsync` — handlers, repositories, and domain services never call it

# Implementation
```csharp
//Skill: class-i-unit-of-work
//Plateau: shared-rules
//Version: 20260824163000

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
```

# Rules
MUST:
- Live in `Shared/UnitOfWork`, expose only `SaveChangesAsync`
MUST NOT:
- Be injected into a handler — only `UnitOfWorkBehavior` calls it

# Check list
- [ ] `IUnitOfWork` defined in `Shared/UnitOfWork/IUnitOfWork.cs` with a single method

__Applied solutions:__
- [[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] - [[../../../../../solutions/solution-unit-of-work.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
