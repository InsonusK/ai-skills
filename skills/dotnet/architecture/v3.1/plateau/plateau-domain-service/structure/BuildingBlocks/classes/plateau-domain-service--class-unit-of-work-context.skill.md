---
name: plateau-domain-service--class-unit-of-work-context
description: Class UnitOfWorkContext in the plateau-domain-service plateau — the scoped, thread-safe nesting-depth counter that lets UnitOfWorkBehavior tell whether it is the outermost command
whenToUse: when editing the nesting-depth counter, or checking that sub-commands do not commit prematurely
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
- Track the nesting depth of active command pipeline invocations within a request scope so `UnitOfWorkBehavior` commits only at `Depth == 1`.

__Applied solutions:__
- [[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] - [[../../../../../solutions/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create.md|UnitOfWorkContext.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- Plain `sealed class` in `BuildingBlocks/MediatR` — no interface, no base, no infrastructure dependency.
- `Enter()` / `Leave()` mutate `_depth` via `Interlocked`; `Depth` is read-only.
- Registered `Scoped` — one instance shared across all nested `Send()` calls in a request. Never `Singleton` (leaks across requests) or `Transient` (depth never reaches 1 in nesting).
- Read/written only by `UnitOfWorkBehavior` — never injected into a handler.

# Implementation
```csharp
// Skill: plateau-domain-service--class-unit-of-work-context
// Plateau: domain-service
// Version: 20260902000000
namespace BuildingBlocks.MediatR;

public sealed class UnitOfWorkContext
{
    private int _depth;
    public void Enter() => Interlocked.Increment(ref _depth);
    public void Leave() => Interlocked.Decrement(ref _depth);
    public int Depth => _depth;
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] - [[../../../../../solutions/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create.md|UnitOfWorkContext.cs.create]]

# Rules
MUST:
- Plain class with `Enter()` / `Leave()` (via `Interlocked`) and a read-only `Depth`, in `BuildingBlocks/MediatR`.
- Be registered `Scoped`; be used only by `UnitOfWorkBehavior`.
- Never contain business logic; never be injected into a handler; never be `Singleton`/`Transient`.
- Never apply several plateau templates per class.

# Check list
- [ ] `Enter()` / `Leave()` use `Interlocked`; `Depth` is read-only.
- [ ] Registered `Scoped`; not injected into any handler.

# Unittest TestCases
- [ ] WHEN `Enter()` is called twice and `Leave()` once THEN `Depth == 1`.
