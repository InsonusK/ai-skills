---
description: Add ConflictResult, IHasGuid, and IGuidResolver
name: Shared.csproj
element_kind: project
change_kind: extend
tags:
  - solution/external-created-entity
  - element/shared-csproj
---

# Goals
- Own `ConflictResult<T>` — the result type used by resolvers to express a duplicate Guid conflict
- Own `IHasGuid` — the marker interface for commands carrying a client-generated Guid
- Own `IGuidResolver<TResponse>` — the per-entity resolver contract consumed by `GuidResolvingBehavior`

# Core Principles
- All three live in Shared — they are contracts or primitives implemented/consumed by multiple layers
- `ConflictResult<T>` is accessible by module Application resolvers (created) and BuildingBlocks behavior (returned)
- `IHasGuid` is implemented by commands in `{Module}.Interfaces`
- `IGuidResolver<TResponse>` is implemented by resolvers in `{Module}.Application` and consumed by `GuidResolvingBehavior` in BuildingBlocks
- Shared defines common cross-cutting primitives only — no business logic, no pipeline implementations

# Structure

## Project Structure
```
/Shared
  /Results
    ConflictResult.cs
  /Guid
    IHasGuid.cs
    IGuidResolver.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Results/ConflictResult.cs | `Result<T>` subclass carrying the existing entity result for 409 responses |
| /Guid/IHasGuid.cs | Marker interface for commands carrying a client-generated Guid |
| /Guid/IGuidResolver.cs | Per-entity resolver contract — resolves Guid to existing command response |

# Allowed Dependencies
- `Ardalis.Result` — required for `ConflictResult<T>` to inherit from `Result<T>`

# Rules

## MUST
- `ConflictResult<T>`, `IHasGuid`, `IGuidResolver<TResponse>` defined in Shared
- `ConflictResult<T>` inherits from `Ardalis.Result.Result<T>` and sets `Status` to `ResultStatus.Conflict`
- `IGuidResolver<TResponse>` returns `Task<TResponse?>` — null means not found, non-null means conflict
- `TResponse` of `IGuidResolver` matches the command handler response type exactly
- Never `ConflictResult<T>`, `IHasGuid`, or `IGuidResolver<TResponse>` defined in BuildingBlocks — they are consumed by multiple layers
- Never shared reference any other project

## SHOULD
- Avoid guid contracts defined in BuildingBlocks — forces `{Module}.Interfaces` and `{Module}.Application` to reference BuildingBlocks for contracts
- Avoid `IGuidResolver` returning a different response type than the command handler

# Check list
- [ ] `ConflictResult<T>` defined in `Shared/Results/ConflictResult.cs`
- [ ] `IHasGuid` defined in `Shared/Guid/IHasGuid.cs`
- [ ] `IGuidResolver<TResponse>` defined in `Shared/Guid/IGuidResolver.cs`
- [ ] `Shared.csproj` references `Ardalis.Result`
