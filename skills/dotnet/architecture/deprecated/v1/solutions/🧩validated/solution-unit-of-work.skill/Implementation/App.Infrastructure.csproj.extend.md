---
description: Add UnitOfWork EF Core implementation
name: App.Infrastructure.csproj
element_kind: project
change_kind: extend
tags:
  - solution/unit-of-work
  - element/app-infrastructure-csproj
---

# Goals
- Provide the `UnitOfWork` EF Core implementation that wraps `AppDbContext.SaveChangesAsync`

# Core Principles
- `UnitOfWork` delegates directly to `AppDbContext.SaveChangesAsync` — no additional logic
- Registered as `Scoped` — shares the same `DbContext` instance as `Repository<T>`

# Structure

## Project Structure
```
/App.Infrastructure
  /UnitOfWork
    UnitOfWork.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /UnitOfWork | Unit of work implementations |
| UnitOfWork.cs | EF Core SaveChangesAsync implementation |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides `DbContext.SaveChangesAsync` |

# Allowed Dependencies
- Shared
- App.Domain (implicit via AppDbContext)

# Rules

## MUST
- `UnitOfWork` registered as `Scoped` — must share the same `DbContext` instance as `Repository<T>`

## MUST NOT
- `UnitOfWork` expose any method beyond `SaveChangesAsync`
- `UnitOfWork` contain transaction management logic — EF manages transactions implicitly

# Anti-patterns
- `UnitOfWork` containing retry logic — belongs in a decorator or policy, not the UoW
- `UnitOfWork` exposing `BeginTransaction` — violates single-responsibility contract

# Check list
- [ ] `UnitOfWork` implemented in `App.Infrastructure/UnitOfWork/UnitOfWork.cs`
- [ ] Registered as `Scoped`
