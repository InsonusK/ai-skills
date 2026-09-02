---
name: plateau-offline-sync-service--class-guid-resolving-behavior
description: Class GuidResolvingBehavior<TRequest, TResponse> in the plateau-offline-sync-service plateau — the pipeline behavior that makes creation idempotent for an IHasGuid command
whenToUse: when editing the Guid-resolving pipeline behavior, or checking the idempotent-create short-circuit semantics
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
---

# Goal
- For an `IHasGuid` create command, ask the entity's `IGuidResolver<TResponse>` whether the Guid already exists; if so, return the resolver's `ConflictResult<T>` and skip the handler (and any commit). First request proceeds normally.

__Applied solutions:__
- [[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[../../../../../solutions/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend/GuidResolvingBehavior.cs.create.md|GuidResolvingBehavior.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `sealed`, in `BuildingBlocks/MediatR`, constrained `where TRequest : IHasGuid` — activates only for Guid-carrying commands (an open-generic registration is fine: the constraint filters non-`IHasGuid` requests out).
- Resolves `IGuidResolver<TResponse>` from DI (concrete per response type). Returns the resolver's non-null result unchanged, or `await next()` on null.
- Never calls `SaveChangesAsync`, never constructs a response DTO, never throws for a duplicate.
- Registered after `ConcurrencyBehavior`, before `UnitOfWorkBehavior` — a duplicate short-circuits before any commit.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-guid-resolving-behavior
// Plateau: offline-sync-service
// Version: 20260902000000
using MediatR;
using Shared.Guid;

namespace BuildingBlocks.MediatR;

public sealed class GuidResolvingBehavior<TRequest, TResponse>(IGuidResolver<TResponse> resolver)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasGuid
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var existing = await resolver.ResolveAsync(request.Guid, ct);
        return existing is not null ? existing : await next();
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[../../../../../solutions/solution-external-created-entity.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Rules
MUST:
- Constrain `where TRequest : IHasGuid`; consume `IHasGuid` / `IGuidResolver<TResponse>` from `Shared`.
- Return the resolver's non-null result unchanged; `await next()` on null.
- Be registered after `ConcurrencyBehavior` and before `UnitOfWorkBehavior`.
- Never call `SaveChangesAsync`, construct a response DTO, or throw for a duplicate.
- Never apply several plateau templates per class.

# Check list
- [ ] `where TRequest : IHasGuid`; resolves `IGuidResolver<TResponse>` from DI.
- [ ] Returns the resolver result on non-null; passes through on null.
- [ ] Registered after concurrency, before unit-of-work; no `SaveChangesAsync`.

# Unittest TestCases
- [ ] WHEN the resolver returns a `ConflictResult` THEN the handler is not invoked and that result is returned.
- [ ] WHEN the resolver returns null THEN the handler runs.
