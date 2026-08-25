---
name: class-guid-resolving-behavior
description: Class GuidResolvingBehavior in the shared-rules plateau
whenToUse: when reviewing how a duplicate client-generated Guid is detected before a create handler runs
domain: skill
type: template
plateau: shared-rules
version: 20260824163000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
---

# Goal
- Short-circuit with the resolver's `ConflictResult<TResponse>` when a command's `Guid` already exists, before the handler runs

# Core Principles
- Generic — one implementation handles every `IHasGuid` command via `IGuidResolver<TResponse>` resolved from DI
- Returns the resolver's result, never throws — duplicate detection is not exceptional flow control

# Implementation
```csharp
//Skill: class-guid-resolving-behavior
//Plateau: shared-rules
//Version: 20260824163000

public class GuidResolvingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasGuid, IRequest<TResponse>
{
    private readonly IGuidResolver<TResponse> _resolver;

    public GuidResolvingBehavior(IGuidResolver<TResponse> resolver) => _resolver = resolver;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var existing = await _resolver.ResolveAsync(request.Guid, ct);
        return existing is not null ? existing : await next();
    }
}
```

# Rules
MUST:
- Registered after `ConcurrencyBehavior` (or immediately after `ValidationBehavior` if concurrency does not apply), before `UnitOfWorkBehavior`
- Return the resolver's result unchanged on conflict — never construct a response itself
MUST NOT:
- Throw an exception for a duplicate Guid

# Check list
- [ ] `GuidResolvingBehavior` registered in `AddPipeline()`, before `UnitOfWorkBehavior`
- [ ] Never throws for a duplicate `Guid`

__Applied solutions:__
- [[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[../../../../../solutions/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend/GuidResolvingBehavior.cs.create.md|GuidResolvingBehavior.cs.create]]
