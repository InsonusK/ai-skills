---
description: Add IUnitOfWork commit contract without infrastructure coupling
name: Shared.csproj
element_kind: project
change_kind: extend
---

# Goals
- Own `IUnitOfWork` — the commit contract accessible by every layer without infrastructure coupling

# Core Principles
- Interface only — no EF Core dependency in Shared
- Single method: `SaveChangesAsync` — nothing else

# Structure

## Project Structure
```
/Shared
  /UnitOfWork
    IUnitOfWork.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /UnitOfWork | Unit of work contracts |
| IUnitOfWork.cs | Commit contract — single SaveChangesAsync method |

# Rules

MUST:
- `IUnitOfWork` defined in Shared — not BuildingBlocks, not App.Infrastructure

MUST NOT:
- Shared reference EF Core

# Anti-patterns
- `IUnitOfWork` defined in BuildingBlocks — creates unnecessary coupling
- Adding transaction methods to `IUnitOfWork` — only `SaveChangesAsync` belongs here

# Check list
- [ ] `IUnitOfWork` defined in `Shared/UnitOfWork/IUnitOfWork.cs`
- [ ] No EF Core reference in Shared
