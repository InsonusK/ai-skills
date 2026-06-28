---
description: Add MediatR package and the IQuery marker interface to Shared
name: Shared.csproj
element_kind: project
change_kind: extend
---

# Goals
- Own the `IQuery<TResponse>` marker interface for read-only operations
- Make the read-only operation marker available to every layer without coupling to BuildingBlocks

# Core Principles
- Shared defines only interfaces and markers — no implementations
- `IQuery<TResponse>` extends MediatR `IRequest<TResponse>` so MediatR can route queries automatically
- Any layer may depend on Shared safely

# Structure

## Project Structure
```
/Shared
  /MediatR
    IQuery.cs
  /Repositories
    IReadRepository.cs    ← repository-integration
    IRepository.cs        ← repository-integration
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /MediatR | MediatR marker interfaces |
| IQuery.cs | Read-only operation marker interface |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Provides `IRequest<T>` that `IQuery<T>` extends |

# Allowed Dependencies
- None — Shared has no project dependencies

# Rules

MUST:
- `MediatR` package referenced in `Shared.csproj`
- `IQuery<TResponse>` placed in `/Shared/MediatR`
- `IQuery<TResponse>` extends MediatR `IRequest<TResponse>`
- `IQuery<TResponse>` does NOT extend `ICommand` or `ICommand<TResponse>`

MUST NOT:
- Add FluentValidation, Ardalis.Result, or EF Core packages to Shared
- Add implementation code to Shared
- `IQuery` extend `ICommand` — queries must remain distinct from write-side markers

# Anti-patterns
- Defining `IQuery` in BuildingBlocks — forces modules to reference BuildingBlocks for contracts
- `IQuery` extending `ICommand` — would blur the boundary between read and write operations

# Check list
- [ ] `MediatR` referenced in `Shared.csproj`
- [ ] `/Shared/MediatR/IQuery.cs` exists
- [ ] `IQuery<TResponse>` extends `IRequest<TResponse>`
- [ ] `IQuery<TResponse>` does not extend `ICommand`
