---
description: Add UnitOfWorkContext and UnitOfWorkBehavior pipeline components
name: BuildingBlocks.csproj
element_kind: project
change_kind: extend
tags:
  - solution/unit-of-work
  - element/buildingblocks-csproj
---

# Goals
- Own `UnitOfWorkContext` and `UnitOfWorkBehavior` — the depth tracking and pipeline commit enforcement
- Reference `ICommand` and `IUnitOfWork` from Shared

# Core Principles
- `UnitOfWorkContext` is a plain class with a counter — no infrastructure dependency
- `UnitOfWorkBehavior` depends on `IUnitOfWork` and `UnitOfWorkContext` — both resolved from DI

# Structure

## Project Structure
```
/BuildingBlocks
  /MediatR
    UnitOfWorkContext.cs
    UnitOfWorkBehavior.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /MediatR | MediatR pipeline behaviors and context |
| UnitOfWorkContext.cs | Scoped depth counter preventing premature sub-command commit |
| UnitOfWorkBehavior.cs | Pipeline behavior that commits at depth 1 after handler completes |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Provides `IPipelineBehavior<TRequest, TResponse>` implemented by `UnitOfWorkBehavior` |

# Allowed Dependencies
- Shared

# Rules

## MUST
- `UnitOfWorkContext` and `UnitOfWorkBehavior` defined in BuildingBlocks
- Both reference `ICommand` and `IUnitOfWork` from Shared
- `UnitOfWorkBehavior` constrained on `where TRequest : ICommand<TResponse>`

## MUST NOT
- Add EF Core dependency to BuildingBlocks

# Anti-patterns
- `UnitOfWorkBehavior` constrained on `IRequest<T>` instead of `ICommand<TResponse>` — would commit on queries
- `UnitOfWorkBehavior` constrained on bare `ICommand` — the non-generic marker no payload-bearing command implements, so the behavior would never activate on any real command

# Check list
- [ ] `UnitOfWorkContext` defined in `BuildingBlocks/MediatR/UnitOfWorkContext.cs`
- [ ] `UnitOfWorkBehavior` defined in `BuildingBlocks/MediatR/UnitOfWorkBehavior.cs`
- [ ] `UnitOfWorkBehavior` constrained to `where TRequest : ICommand<TResponse>`
- [ ] No EF Core reference in BuildingBlocks
