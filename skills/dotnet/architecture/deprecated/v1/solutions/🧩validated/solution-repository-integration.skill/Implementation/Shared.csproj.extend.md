---
description: Add Ardalis.Specification package and repository abstractions to Shared
element_kind: project
change_kind: extend
create: Shared.csproj
tags:
  - solution/repository-integration
  - element/shared-csproj
---

# Goals
- Make `Ardalis.Specification` interfaces available to every layer without coupling to EF Core
- Define `IReadRepository<T>` and `IRepository<T>` as thin wrappers around Ardalis base interfaces

# Core Principles
- Shared has no EF Core dependencies — only the lightweight `Ardalis.Specification` package
- Interfaces are thin — they inherit all Ardalis methods, adding no new signatures unless required
- Any layer may depend on Shared safely

# Structure

## Project Structure
```
/Shared
  /Repositories
    IReadRepository.cs
    IRepository.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Repositories | Repository abstractions |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Ardalis.Specification` | latest stable | Provides `IReadRepositoryBase<T>` and `IRepositoryBase<T>` |

# Allowed Dependencies
- None — Shared has no project dependencies

# Rules

## MUST
- `Ardalis.Specification` package referenced in `Shared.csproj`
- `IReadRepository<T>` inherit `IReadRepositoryBase<T>`
- `IRepository<T>` inherit `IRepositoryBase<T>` and `IReadRepository<T>`
- Both interfaces placed in `/Shared/Repositories`

## MUST NOT
- Reference `Ardalis.Specification.EntityFrameworkCore` in Shared
- Add custom method signatures to the interfaces unless they are cross-cutting concerns

# Anti-patterns
- Duplicating Ardalis method signatures manually instead of inheriting the base interfaces
- Adding infrastructure concerns to the repository interfaces

# Check list
- [ ] `Ardalis.Specification` referenced in `Shared.csproj`
- [ ] `/Repositories` folder exists
- [ ] `IReadRepository<T>` inherits `IReadRepositoryBase<T>`
- [ ] `IRepository<T>` inherits `IRepositoryBase<T>` and `IReadRepository<T>`
