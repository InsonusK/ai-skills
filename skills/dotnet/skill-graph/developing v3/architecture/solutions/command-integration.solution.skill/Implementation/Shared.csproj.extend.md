---
description: Add MediatR package and the ICommand marker interfaces to Shared
name: Shared.csproj
change_kind: extend
---

# Goals
- Make the `ICommand` marker available to every layer without coupling to BuildingBlocks
- Enable MediatR routing and pipeline behavior constraints for write operations

# Core Principles
- Shared defines only interfaces and markers — no implementations
- `ICommand<TResponse>` extends MediatR `IRequest<TResponse>` so MediatR can route commands automatically
- Any layer may depend on Shared safely

# Structure

## Project Structure
```
/Shared
  /MediatR
    ICommand.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /MediatR | MediatR marker interfaces |
| ICommand.cs | Write operation marker interfaces |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Provides `IRequest<T>` that `ICommand<T>` extends |

# Allowed Dependencies
- None — Shared has no project dependencies

# Rules

MUST:
- `MediatR` package referenced in `Shared.csproj`
- `ICommand` and `ICommand<TResponse>` placed in `/Shared/MediatR`
- Both interfaces extend MediatR `IRequest` / `IRequest<TResponse>`

MUST NOT:
- Add FluentValidation, Ardalis.Result, or EF Core packages to Shared
- Add implementation code to Shared

# Anti-patterns
- Defining `ICommand` in BuildingBlocks — forces modules to reference BuildingBlocks for contracts
- Adding behavior logic to a marker interface

# Check list
- [ ] `MediatR` referenced in `Shared.csproj`
- [ ] `/Shared/MediatR/ICommand.cs` exists
- [ ] `ICommand` extends `IRequest`
- [ ] `ICommand<TResponse>` extends `IRequest<TResponse>`
