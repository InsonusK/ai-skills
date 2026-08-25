---
name: class-unit-of-work
description: Class UnitOfWork in the shared-rules plateau
whenToUse: when reviewing the EF Core implementation of IUnitOfWork
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
- Implement `IUnitOfWork` by delegating directly to `AppDbContext.SaveChangesAsync`

# Core Principles
- No transaction management logic — EF Core's implicit transactions are sufficient
- Registered `Scoped`, sharing the same `AppDbContext` instance as `Repository<T>`

# Implementation
```csharp
//Skill: class-unit-of-work
//Plateau: shared-rules
//Version: 20260824163000

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct) => context.SaveChangesAsync(ct);
}
```

# Rules
MUST:
- Delegate directly to `AppDbContext.SaveChangesAsync`, nothing else
- Registered `Scoped`
MUST NOT:
- Expose any method beyond `SaveChangesAsync`
- Contain retry or transaction-management logic

# Check list
- [ ] `UnitOfWork` delegates to `AppDbContext.SaveChangesAsync` only
- [ ] Registered `Scoped`

__Applied solutions:__
- [[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] - [[../../../../../solutions/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create.md|UnitOfWork.cs.create]]
