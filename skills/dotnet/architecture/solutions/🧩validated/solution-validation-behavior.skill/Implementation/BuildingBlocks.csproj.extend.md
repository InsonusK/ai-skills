---
description: Add FluentValidation, MediatR, and Ardalis.Result packages for the ValidationBehavior
name: BuildingBlocks.csproj
element_kind: project
change_kind: extend
---

# Goals
- Own the generic `ValidationBehavior` pipeline behavior that intercepts any `IRequest<TResponse>`
- Keep input validation as a cross-cutting technical concern implemented once and reused across modules

# Core Principles
- BuildingBlocks implements technical patterns using interfaces defined in Shared or provided by MediatR
- BuildingBlocks does not define request markers — it consumes `IRequest<T>` from MediatR

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
| ValidationBehavior.cs | Pipeline behavior that validates any `IRequest<TResponse>` |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `FluentValidation` | latest stable | Provides `IValidator<T>` injected into `ValidationBehavior` |
| `MediatR` | latest stable | Provides `IPipelineBehavior<TRequest, TResponse>` and `IRequest<T>` |
| `Ardalis.Result` | latest stable | Provides `Result.Invalid`, `ValidationError`, and `IResult` |

# Allowed Dependencies
- Shared

# Rules

## MUST
- `FluentValidation`, `MediatR`, and `Ardalis.Result` packages referenced in `BuildingBlocks.csproj`
- `ValidationBehavior` placed in `/BuildingBlocks/MediatR`
- `ValidationBehavior` constrained to `IRequest<TResponse>` from MediatR

## MUST NOT
- Add business logic or request-specific conditions to `ValidationBehavior`
- Throw exceptions for validation failures — always return `Result.Invalid`

# Anti-patterns
- Implementing per-request validation logic inside `ValidationBehavior`
- Returning exceptions instead of `Result.Invalid`

# Check list
- [ ] `FluentValidation` referenced in `BuildingBlocks.csproj`
- [ ] `MediatR` referenced in `BuildingBlocks.csproj`
- [ ] `Ardalis.Result` referenced in `BuildingBlocks.csproj`
- [ ] `/BuildingBlocks/MediatR/ValidationBehavior.cs` exists
