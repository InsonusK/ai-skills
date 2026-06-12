---
description: Add GuidResolvingBehavior and ConflictExceptionMiddleware
name: BuildingBlocks.csproj
change_kind: extend
---

# Goals
- Own `GuidResolvingBehavior` — the MediatR pipeline behavior that consumes `IHasGuid` and `IGuidResolver<TResult>` from Shared
- Own `ConflictExceptionMiddleware` — the HTTP middleware that catches `ConflictException` and writes 409 with the existing entity body

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
  /Middleware
    ConflictExceptionMiddleware.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /MediatR/GuidResolvingBehavior.cs | Pipeline behavior that short-circuits on duplicate Guid |
| /Middleware/ConflictExceptionMiddleware.cs | HTTP middleware that catches ConflictException and writes 409 |

# Allowed Dependencies
- Shared
- ASP.NET Core HTTP abstractions (for middleware)
- System.Text.Json (for response serialization)

# Rules

MUST:
- `GuidResolvingBehavior` defined in `BuildingBlocks/MediatR/GuidResolvingBehavior.cs`
- `ConflictExceptionMiddleware` defined in `BuildingBlocks/Middleware/ConflictExceptionMiddleware.cs`
- `GuidResolvingBehavior` consumes `IHasGuid` and `IGuidResolver<TResult>` from Shared
- `GuidResolvingBehavior` throws `ConflictException<TResponse>` from Shared — never returns a result directly
- `ConflictExceptionMiddleware` catches non-generic `ConflictException` and writes 409 with `ex.GetValue()`

MUST NOT:
- `IHasGuid` or `IGuidResolver<TResult>` defined in BuildingBlocks — they are contracts that belong in Shared
- `GuidResolvingBehavior` registered as open generic — DI resolves `IGuidResolver<TResult>` per concrete command result type
- `ConflictExceptionMiddleware` defined in App.Host or `{Module}.Api` — centralized in BuildingBlocks

# Anti-patterns
- `GuidResolvingBehavior` registered as open generic — breaks DI resolution per command result type
- Defining `IHasGuid` or `IGuidResolver<TResult>` in BuildingBlocks — violates the rule that BuildingBlocks consumes interfaces from Shared
- Per-controller try/catch for `ConflictException` — duplicates middleware logic

# Check list
- [ ] `GuidResolvingBehavior` defined in `BuildingBlocks/MediatR/GuidResolvingBehavior.cs`
- [ ] `GuidResolvingBehavior` consumes `IHasGuid` and `IGuidResolver<TResult>` from Shared
- [ ] `ConflictExceptionMiddleware` defined in `BuildingBlocks/Middleware/ConflictExceptionMiddleware.cs`
- [ ] BuildingBlocks references ASP.NET Core HTTP abstractions for middleware support
