---
name: plateau-domain-service--class-i-unit-of-work
description: Class IUnitOfWork in the plateau-domain-service plateau — the single-method commit contract in Shared/UnitOfWork
whenToUse: when creating or editing the IUnitOfWork contract, or checking that nothing but its implementation calls SaveChangesAsync
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
- Define the single commit contract — one method, one responsibility — in `Shared` so every layer can reference it without depending on infrastructure.

__Applied solutions:__
- [[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] - [[../../../../../solutions/solution-unit-of-work.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create.md|IUnitOfWork.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `interface IUnitOfWork` in `Shared/UnitOfWork` with exactly one method: `Task SaveChangesAsync(CancellationToken ct = default)`.
- No `BeginTransaction`, `Rollback`, or `Commit` — nothing else.
- Implemented in `App.Infrastructure`; invoked only by `UnitOfWorkBehavior`.

# Implementation
```csharp
// Skill: plateau-domain-service--class-i-unit-of-work
// Plateau: domain-service
// Version: 20260902000000
namespace Shared.UnitOfWork;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] - [[../../../../../solutions/solution-unit-of-work.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create.md|IUnitOfWork.cs.create]]

# Rules
MUST:
- Single method only — `SaveChangesAsync(CancellationToken)`; no other member.
- Live in `Shared/UnitOfWork`; contain no implementation.
- Never apply several plateau templates per class.

# Check list
- [ ] `IUnitOfWork` in `Shared/UnitOfWork` with exactly one method.
- [ ] No transaction/rollback member.

# Unittest TestCases
- [ ] WHEN `IUnitOfWork` is reflected THEN it declares exactly one method.
