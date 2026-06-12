---
description: Add ConflictException, IHasGuid, and IGuidResolver
name: Shared.csproj
change_kind: extend
---

# Goals
- Own `ConflictException<T>` — the exception thrown by `GuidResolvingBehavior` that carries the existing entity result to the controller
- Own `IHasGuid` — the marker interface for commands carrying a client-generated Guid
- Own `IGuidResolver<TResult>` — the per-entity resolver contract consumed by `GuidResolvingBehavior`

# Core Principles
- All three live in Shared — they are contracts implemented or consumed by multiple layers
- `ConflictException<T>` is accessible by both BuildingBlocks (thrown) and Api layer (caught)
- `IHasGuid` is implemented by commands in `{Module}.Interfaces`
- `IGuidResolver<TResult>` is implemented by resolvers in `{Module}.Application` and consumed by `GuidResolvingBehavior` in BuildingBlocks
- Shared defines common cross-cutting interfaces only — no implementation, no business logic

# Structure

## Project Structure
```
/Shared
  /Exceptions
    ConflictException.cs
  /Guid
    IHasGuid.cs
    IGuidResolver.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Exceptions/ConflictException.cs | Exception carrying existing entity result for 409 responses |
| /Guid/IHasGuid.cs | Marker interface for commands carrying a client-generated Guid |
| /Guid/IGuidResolver.cs | Per-entity resolver contract — checks if Guid already exists |

# Allowed Dependencies
- None

# Rules

MUST:
- `ConflictException<T>`, `IHasGuid`, `IGuidResolver<TResult>` defined in Shared
- All three are contracts with no implementation

MUST NOT:
- `ConflictException<T>`, `IHasGuid`, or `IGuidResolver<TResult>` defined in BuildingBlocks — they are contracts consumed by multiple layers
- Shared reference any other project

# Anti-patterns
- Guid contracts defined in BuildingBlocks — forces `{Module}.Interfaces` and `{Module}.Application` to reference BuildingBlocks for contracts

# Check list
- [ ] `ConflictException<T>` defined in `Shared/Exceptions/ConflictException.cs`
- [ ] `IHasGuid` defined in `Shared/Guid/IHasGuid.cs`
- [ ] `IGuidResolver<TResult>` defined in `Shared/Guid/IGuidResolver.cs`
