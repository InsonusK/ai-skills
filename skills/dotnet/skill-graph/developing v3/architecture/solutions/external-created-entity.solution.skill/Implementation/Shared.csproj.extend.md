---
description: Add ConflictException, IHasGuid, and IGuidResolver
name: Shared.csproj
change_kind: extend
---

# Goals
- Own `ConflictException<T>` and non-generic `ConflictException` base — the exception thrown by `GuidResolvingBehavior` and caught by `ConflictExceptionMiddleware`
- Own `IHasGuid` — the marker interface for commands carrying a client-generated Guid
- Own `IGuidResolver<TResult>` — the per-entity resolver contract consumed by `GuidResolvingBehavior`

# Core Principles
- All four live in Shared — they are contracts implemented or consumed by multiple layers
- `ConflictException` base class is accessible by BuildingBlocks middleware (caught)
- `ConflictException<T>` is accessible by BuildingBlocks (thrown) and middleware (caught via base class)
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
| /Exceptions/ConflictException.cs | Exception base class and generic `ConflictException<T>` carrying existing entity result for 409 responses |
| /Guid/IHasGuid.cs | Marker interface for commands carrying a client-generated Guid |
| /Guid/IGuidResolver.cs | Per-entity resolver contract — checks if Guid already exists |

# Allowed Dependencies
- None

# Rules

MUST:
- `ConflictException`, `ConflictException<T>`, `IHasGuid`, `IGuidResolver<TResult>` defined in Shared
- All four are contracts with no implementation

MUST NOT:
- `ConflictException`, `ConflictException<T>`, `IHasGuid`, or `IGuidResolver<TResult>` defined in BuildingBlocks — they are contracts consumed by multiple layers
- Shared reference any other project

# Anti-patterns
- Guid contracts defined in BuildingBlocks — forces `{Module}.Interfaces` and `{Module}.Application` to reference BuildingBlocks for contracts

# Check list
- [ ] `ConflictException` base class defined in `Shared/Exceptions/ConflictException.cs`
- [ ] `ConflictException<T>` defined in `Shared/Exceptions/ConflictException.cs`
- [ ] `IHasGuid` defined in `Shared/Guid/IHasGuid.cs`
- [ ] `IGuidResolver<TResult>` defined in `Shared/Guid/IGuidResolver.cs`
