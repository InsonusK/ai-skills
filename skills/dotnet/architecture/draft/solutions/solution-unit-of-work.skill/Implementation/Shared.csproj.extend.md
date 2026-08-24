---
description: Add IUnitOfWork commit contract without infrastructure coupling
name: Shared.csproj
element_kind: project
change_kind: extend
tags:
  - solution/unit-of-work
  - element/shared-csproj
---

# Goals
- Own `IUnitOfWork` — the commit contract accessible by every layer without infrastructure coupling

# Core Principles
- Interface only — no EF Core dependency in Shared
- Single method: `SaveChangesAsync` — nothing else

# Implementation changes

**AS IS** (from `plateau-service-with-validated-module-interaction`, via `solution-command-integration`):
```
/Shared
  /MediatR
    ICommand.cs
  ...(other foundation folders unchanged)
  Shared.csproj
```

**TO BE** (after this solution):
```
/Shared
  /MediatR
    ICommand.cs
  /UnitOfWork
    IUnitOfWork.cs
  Shared.csproj
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /UnitOfWork | Unit of work contracts |
| IUnitOfWork.cs | Commit contract — single SaveChangesAsync method |

# Rules

## MUST
- `IUnitOfWork` defined in Shared — not BuildingBlocks, not App.Infrastructure

## MUST NOT
- Shared reference EF Core

# Anti-patterns
- `IUnitOfWork` defined in BuildingBlocks — creates unnecessary coupling
- Adding transaction methods to `IUnitOfWork` — only `SaveChangesAsync` belongs here

# Check list
- [ ] `IUnitOfWork` defined in `Shared/UnitOfWork/IUnitOfWork.cs`
- [ ] No EF Core reference in Shared
