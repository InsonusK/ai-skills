---
name: plateau-domain-service--class-unit-of-work
description: Class UnitOfWork in the plateau-domain-service plateau — the IUnitOfWork implementation, the single place DbContext.SaveChangesAsync is called
whenToUse: when editing the unit-of-work implementation, or checking that no other code path commits the DbContext
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]"
---

# Goal
- Implement `Shared.UnitOfWork.IUnitOfWork` by delegating to `AppDbContext.SaveChangesAsync` — the only place in the whole solution that call is made.

__Applied solutions:__
- [[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] - [[../../../../../solutions/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create.md|UnitOfWork.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `sealed class UnitOfWork : IUnitOfWork` in `/App.Infrastructure/UnitOfWork`.
- Takes `AppDbContext` by constructor injection; pure delegation — no transaction management, no retry.
- Registered `Scoped` — same `DbContext` instance as `Repository<T>` within the request.
- Invoked only by `UnitOfWorkBehavior` (registered last in the pipeline) — never by a handler.

# Implementation
```csharp
// Skill: plateau-domain-service--class-unit-of-work
// Plateau: domain-service
// Version: 20260902000000
using App.Infrastructure.Persistence;
using Shared.UnitOfWork;

namespace App.Infrastructure.UnitOfWork;

public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] - [[../../../../../solutions/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create.md|UnitOfWork.cs.create]]

# Rules
MUST:
- Implement `IUnitOfWork`, delegate straight to `AppDbContext.SaveChangesAsync`, add no other logic.
- Be registered `Scoped`; be invoked only by `UnitOfWorkBehavior`.
- Never be called from a handler; never contain a transaction/rollback block (EF implicit transactions cover it).
- Never apply several plateau templates per class.

# Check list
- [ ] `UnitOfWork : IUnitOfWork`, pure delegation, `Scoped`.
- [ ] No handler references `IUnitOfWork`; no `SaveChangesAsync` anywhere else.

# Unittest TestCases
- [ ] WHEN `SaveChangesAsync` is called THEN the DbContext's `SaveChangesAsync` is invoked exactly once.
