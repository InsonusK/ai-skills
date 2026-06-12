---
description: Add GuidResolvingBehavior
name: BuildingBlocks.csproj
change_kind: extend
---

# Goals
- Own `GuidResolvingBehavior` — the pipeline behavior that consumes `IHasGuid` and `IGuidResolver<TResult>` from Shared

# Structure

## Project Structure
```
/BuildingBlocks
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
| /MediatR/GuidResolvingBehavior.cs | Pipeline behavior that short-circuits on duplicate Guid |

# Allowed Dependencies
- Shared

# Rules

MUST:
- `GuidResolvingBehavior` defined in BuildingBlocks
- `GuidResolvingBehavior` consumes `IHasGuid` and `IGuidResolver<TResult>` from Shared
- `GuidResolvingBehavior` throws `ConflictException<TResponse>` from Shared — never returns a result directly

MUST NOT:
- `IHasGuid` or `IGuidResolver<TResult>` defined in BuildingBlocks — they are contracts that belong in Shared
- `GuidResolvingBehavior` registered as open generic — DI resolves `IGuidResolver<TResult>` per concrete command result type

# Anti-patterns
- `GuidResolvingBehavior` registered as open generic — breaks DI resolution per command result type
- Defining `IHasGuid` or `IGuidResolver<TResult>` in BuildingBlocks — violates the rule that BuildingBlocks consumes interfaces from Shared

# Check list
- [ ] `GuidResolvingBehavior` defined in `BuildingBlocks/MediatR/GuidResolvingBehavior.cs`
- [ ] `GuidResolvingBehavior` consumes `IHasGuid` and `IGuidResolver<TResult>` from Shared
