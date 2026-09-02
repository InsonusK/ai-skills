---
name: plateau-offline-sync-service--class-concurrency-behavior
description: Class ConcurrencyBehavior<TRequest, TResponse> in the plateau-offline-sync-service plateau — the pipeline behavior that validates every version an IHasVersions command carries before the handler runs
whenToUse: when editing the concurrency pipeline behavior, or checking the Conflict / NotFound / Error short-circuit semantics
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
---

# Goal
- Validate every entity version an update command carries before the handler runs; return `Result.Conflict` on any mismatch (or `NotFound` / `Error`) so the handler never executes for a stale update. Transport-agnostic — activates on any `IHasVersions` command regardless of dispatch.

__Applied solutions:__
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/BuildingBlocks.csproj.extend/ConcurrencyBehavior.cs.create.md|ConcurrencyBehavior.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `sealed`, in `BuildingBlocks/MediatR`, constrained `where TRequest : IHasVersions`, `where TResponse : IResult`.
- Per `(entityName, id, expectedVersion)`: `factory.GetFor(name)` (null → `Error`), `resolver.GetCurrentVersionForAsync(id)` (`0` → `NotFound`), compare (mismatch → `Conflict`). First mismatch short-circuits.
- Never loads entities, never calls `SaveChangesAsync`, never modifies state — a pure guard delegating to `IEntityVersionResolver`.
- Builds the short-circuit result via the closed `TResponse`'s own static `Error`/`NotFound`/`Conflict` (reflection) — not a `(TResponse)Result.X(...)` cast.
- Registered after `ValidationBehavior`, before `UnitOfWorkBehavior`.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-concurrency-behavior
// Plateau: domain-service
// Version: 20260902000000
using Ardalis.Result;
using MediatR;
using Shared.Concurrency;

namespace BuildingBlocks.MediatR;

public sealed class ConcurrencyBehavior<TRequest, TResponse>(IEntityVersionResolverFactory factory)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasVersions
    where TResponse : IResult
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        foreach (var (name, idVersions) in request.Versions)
        {
            var resolver = factory.GetFor(name) ?? throw null!; // → Result.Error via helper
            foreach (var (id, expected) in idVersions)
            {
                var actual = await resolver.GetCurrentVersionForAsync(id, ct);
                if (actual == 0) return NoArg(nameof(Result.NotFound));
                if ((uint)actual != expected) return Params(nameof(Result.Conflict), "…");
            }
        }
        return await next();
    }
    // Error/NoArg/Params invoke TResponse's own static factory (see solution implementation).
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Rules
MUST:
- Constrain `where TRequest : IHasVersions` and `where TResponse : IResult`; use `IEntityVersionResolverFactory` from `Shared`.
- Return `NotFound` when the resolver reports `0`, `Conflict` on a version mismatch, `Error` for an unknown entity name — via the closed `TResponse`'s own static factory, never `(TResponse)Result.X(...)`.
- Never load an entity, call `SaveChangesAsync`, or modify state; never reference EF Core / repositories / specs directly.
- Be registered after `ValidationBehavior` and before `UnitOfWorkBehavior`.
- Never apply several plateau templates per class.

# Check list
- [ ] `where TRequest : IHasVersions`, `where TResponse : IResult`; uses the factory.
- [ ] `NotFound` / `Conflict` / `Error` produced via the closed `TResponse`'s own factory (no `InvalidCastException` for `Result<T>`).
- [ ] No entity load, no `SaveChangesAsync`, no state change.

# Unittest TestCases
- [ ] WHEN the current version matches THEN the handler runs.
- [ ] WHEN a version mismatches THEN `Result.Conflict` is returned and the handler is not invoked.
- [ ] WHEN the resolver reports `0` THEN `Result.NotFound` is returned.
