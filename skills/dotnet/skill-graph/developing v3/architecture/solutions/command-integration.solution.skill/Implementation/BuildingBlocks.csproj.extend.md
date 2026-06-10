---
description: Add FluentValidation, MediatR, and Ardalis.Result packages for the ValidationBehavior
name: BuildingBlocks.csproj
change_kind: extend
---

# Goals
- Own the generic `ValidationBehavior` pipeline behavior that intercepts all `ICommand` requests
- Keep command input validation as a cross-cutting technical concern implemented once and reused across modules

# Core Principles
- BuildingBlocks implements technical patterns using interfaces defined in Shared
- `ValidationBehavior` is generic — one implementation handles all commands across all modules
- BuildingBlocks does not define `ICommand` — it consumes the marker from Shared

# Structure

## Project Structure
```
/BuildingBlocks
  /MediatR
    ValidationBehavior.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /MediatR | MediatR pipeline behaviors |
| ValidationBehavior.cs | Pipeline behavior that validates `ICommand` requests |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `FluentValidation` | latest stable | Provides `IValidator<T>` injected into `ValidationBehavior` |
| `MediatR` | latest stable | Provides `IPipelineBehavior<TRequest, TResponse>` |
| `Ardalis.Result` | latest stable | Provides `Result.Invalid` and `ValidationError` |

# Allowed Dependencies
- Shared

# Rules

MUST:
- `FluentValidation`, `MediatR`, and `Ardalis.Result` packages referenced in `BuildingBlocks.csproj`
- `ValidationBehavior` placed in `/BuildingBlocks/MediatR`
- `ValidationBehavior` references `ICommand` from Shared

MUST NOT:
- Add business logic or command-specific conditions to `ValidationBehavior`
- Throw exceptions for validation failures — always return `Result.Invalid`

# Anti-patterns
- Implementing per-command validation logic inside `ValidationBehavior`
- Returning exceptions instead of `Result.Invalid`

# Check list
- [ ] `FluentValidation` referenced in `BuildingBlocks.csproj`
- [ ] `MediatR` referenced in `BuildingBlocks.csproj`
- [ ] `Ardalis.Result` referenced in `BuildingBlocks.csproj`
- [ ] `/BuildingBlocks/MediatR/ValidationBehavior.cs` exists
