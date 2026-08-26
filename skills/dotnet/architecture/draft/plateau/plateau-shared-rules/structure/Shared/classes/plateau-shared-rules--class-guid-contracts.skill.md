---
name: plateau-shared-rules--class-guid-contracts
description: Classes IHasGuid/IGuidResolver/ConflictResult in the shared-rules plateau
whenToUse: when a create command accepts a client-generated Guid for idempotent creation
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
- Give a create command a way to carry a client-generated `Guid` (`IHasGuid`), a per-entity resolver contract (`IGuidResolver<TResponse>`) that detects a duplicate before the handler runs, and a `Result<T>` variant carrying the existing entity on conflict (`ConflictResult<T>`)

# Core Principles
- The resolver returns the same response type as the command handler, so 201 and 409 responses share one contract
- `ConflictResult<T>` is returned, never thrown — no exceptions for flow control

# Implementation
```csharp
//Skill: class-guid-contracts
//Plateau: shared-rules
//Version: 20260824163000

public interface IHasGuid
{
    Guid Guid { get; }
}

public interface IGuidResolver<TResponse>
{
    Task<TResponse?> ResolveAsync(Guid guid, CancellationToken ct);
}

public sealed class ConflictResult<T> : Result<T>
{
    private ConflictResult(T value) : base(value) => Status = ResultStatus.Conflict;

    public static ConflictResult<T> For(T existing) => new(existing);
}
```

# Rules
MUST:
- All three live in `Shared` (`Guid/IHasGuid.cs`, `Guid/IGuidResolver.cs`, `Results/ConflictResult.cs`)
- `IGuidResolver<TResponse>` registered per concrete entity type — never as an open generic
MUST NOT:
- Throw an exception for a duplicate `Guid` — return `ConflictResult<T>`

# Check list
- [ ] `IHasGuid`, `IGuidResolver<TResponse>`, `ConflictResult<T>` all defined in `Shared`

__Applied solutions:__
- [[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[../../../../../solutions/solution-external-created-entity.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
