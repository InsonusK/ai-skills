---
name: plateau-offline-sync-service--class-guid-contracts
description: Classes IHasGuid / IGuidResolver<TResponse> / ConflictResult<T> in the plateau-offline-sync-service plateau — the Shared contracts for idempotent creation by a client-generated Guid
whenToUse: when creating or editing the Guid/idempotency contracts in Shared, or wiring a new external-created entity into the idempotent-create stack
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
- Declare the idempotent-create contracts in `Shared` so `{Module}.Interfaces` commands, `{Module}.Application` resolvers, and the `BuildingBlocks` behavior all reference them without coupling.

__Applied solutions:__
- [[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[../../../../../solutions/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IHasGuid.cs.create.md|IHasGuid.cs.create]]

# Core Principles
- Apply ONE plateau template per class (a contract family — one file per type).
- `IHasGuid` (`Shared/Guid`) — `Guid Guid { get; }`; implemented only by create commands for external-created entities.
- `IGuidResolver<TResponse>` (`Shared/Guid`) — `Task<TResponse?> ResolveAsync(Guid, CancellationToken)`; `null` = first request, non-null = the existing response marked as a conflict. `TResponse` matches the command's `ICommand<TResponse>` exactly. Registered per concrete entity, never open generic.
- `ConflictResult<T>` (`Shared/Results`) — `sealed class ConflictResult<T> : Result<T>` with `ResultStatus.Conflict` and `Value` = the existing entity's response. No exception, no extra metadata.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-guid-contracts
// Plateau: offline-sync-service
// Version: 20260902000000
namespace Shared.Guid;

public interface IHasGuid { System.Guid Guid { get; } }

public interface IGuidResolver<TResponse>
{
    Task<TResponse?> ResolveAsync(System.Guid guid, CancellationToken ct);
}
```
```csharp
using Ardalis.Result;
namespace Shared.Results;

public sealed class ConflictResult<T> : Result<T>
{
    public ConflictResult(T value) : base(ResultStatus.Conflict) => Value = value;
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[../../../../../solutions/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/ConflictResult.cs.create.md|ConflictResult.cs.create]]

# Rules
MUST:
- `IHasGuid` + `IGuidResolver<TResponse>` in `Shared/Guid`; `ConflictResult<T>` in `Shared/Results`, inheriting `Ardalis.Result.Result<T>` with `ResultStatus.Conflict`.
- `IGuidResolver` is generic on the exact command response type; return `null` for not-found (never throw).
- `ConflictResult<T>` carries the value in `Value` only — never in `Errors`/`Location`; never throws in the ctor.
- Never define any of these in `BuildingBlocks`; only create commands for external-created entities implement `IHasGuid`.
- Never apply several plateau templates per class.

# Check list
- [ ] `IHasGuid` / `IGuidResolver<TResponse>` in `Shared/Guid`; `ConflictResult<T>` in `Shared/Results`.
- [ ] `IGuidResolver.ResolveAsync` returns `Task<TResponse?>`, `null` for not-found.
- [ ] `ConflictResult<T>.Status == ResultStatus.Conflict`, value in `Value`.

# Unittest TestCases
- [ ] WHEN a `ConflictResult<T>` is created THEN it is assignable to `Result<T>` and its `Status` is `Conflict`.
- [ ] WHEN a resolver returns `null` THEN `GuidResolvingBehavior` proceeds to the handler.
