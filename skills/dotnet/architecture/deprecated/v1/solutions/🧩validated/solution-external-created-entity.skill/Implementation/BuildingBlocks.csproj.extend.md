---
description: Add GuidResolvingBehavior
name: BuildingBlocks.csproj
element_kind: project
change_kind: extend
tags:
  - solution/external-created-entity
  - element/buildingblocks-csproj
---

# Goals
- Own `GuidResolvingBehavior` — the MediatR pipeline behavior that consumes `IHasGuid` and `IGuidResolver<TResponse>` from Shared

# Structure

## Project Structure
```
/BuildingBlocks
  /MediatR
    ValidationBehavior.cs      ← solution-validation-behavior.skill
    GuidResolvingBehavior.cs
    ConcurrencyBehavior.cs     ← solution-entity-concurrency-change.skill
    UnitOfWorkContext.cs       ← solution-unit-of-work.skill
    UnitOfWorkBehavior.cs      ← solution-unit-of-work.skill
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /MediatR/GuidResolvingBehavior.cs | Pipeline behavior that short-circuits on duplicate Guid |

# Allowed Dependencies
- Shared
- Ardalis.Result
- MediatR

# Rules

## MUST
- `GuidResolvingBehavior` defined in `BuildingBlocks/MediatR/GuidResolvingBehavior.cs`
- `GuidResolvingBehavior` consumes `IHasGuid` and `IGuidResolver<TResponse>` from Shared
- `GuidResolvingBehavior` returns the resolver's conflict result — never throws an exception

## MUST NOT
- `IHasGuid` or `IGuidResolver<TResponse>` defined in BuildingBlocks — they are contracts that belong in Shared
- `GuidResolvingBehavior` registered as open generic — DI resolves `IGuidResolver<TResponse>` per concrete command result type
- Define HTTP middleware for conflict handling — conflicts are expressed as `Result<T>` and mapped by the API layer

# Anti-patterns
- `GuidResolvingBehavior` registered as open generic — breaks DI resolution per command result type
- Defining `IHasGuid` or `IGuidResolver<TResponse>` in BuildingBlocks — violates the rule that BuildingBlocks consumes interfaces from Shared
- Throwing exceptions from `GuidResolvingBehavior` — breaks the command-integration principle of no exceptions for flow control

# Check list
- [ ] `GuidResolvingBehavior` defined in `BuildingBlocks/MediatR/GuidResolvingBehavior.cs`
- [ ] `GuidResolvingBehavior` consumes `IHasGuid` and `IGuidResolver<TResponse>` from Shared
- [ ] `GuidResolvingBehavior` returns the resolver's conflict result on duplicate Guid
- [ ] No `ConflictExceptionMiddleware` defined in BuildingBlocks
