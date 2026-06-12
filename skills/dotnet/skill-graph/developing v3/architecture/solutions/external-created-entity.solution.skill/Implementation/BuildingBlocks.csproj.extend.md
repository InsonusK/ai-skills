---
description: Add IHasGuid, IGuidResolver, and GuidResolvingBehavior
name: BuildingBlocks.csproj
change_kind: extend
---

# Goals
- Own `IHasGuid`, `IGuidResolver<TResult>`, and `GuidResolvingBehavior` — the Guid pipeline contract and enforcement

# Structure

## Project Structure
```
/BuildingBlocks
  /Guid
    IHasGuid.cs
    IGuidResolver.cs
  /MediatR
    ValidationBehavior.cs      ← validation-behavior.solution.skill
    GuidResolvingBehavior.cs
    ConcurrencyBehavior.cs     ← entity-concurrency-change.solution.skill
    UnitOfWorkContext.cs       ← unit-of-work.solution.skill
    UnitOfWorkBehavior.cs      ← unit-of-work.solution.skill
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Guid/IHasGuid.cs | Marker interface for commands carrying a client-generated Guid |
| /Guid/IGuidResolver.cs | Per-entity resolver contract — checks if Guid already exists |
| /MediatR/GuidResolvingBehavior.cs | Pipeline behavior that short-circuits on duplicate Guid |

# Allowed Dependencies
- Shared

# Rules

MUST:
- `IHasGuid`, `IGuidResolver<TResult>`, `GuidResolvingBehavior` defined in BuildingBlocks
- `GuidResolvingBehavior` throws `ConflictException<TResponse>` from Shared — never returns a result directly

MUST NOT:
- `GuidResolvingBehavior` registered as open generic — DI resolves `IGuidResolver<TResult>` per concrete command result type

# Anti-patterns
- `GuidResolvingBehavior` registered as open generic — breaks DI resolution per command result type

# Check list
- [ ] `IHasGuid` defined in `BuildingBlocks/Guid/IHasGuid.cs`
- [ ] `IGuidResolver<TResult>` defined in `BuildingBlocks/Guid/IGuidResolver.cs`
- [ ] `GuidResolvingBehavior` defined in `BuildingBlocks/MediatR/GuidResolvingBehavior.cs`
