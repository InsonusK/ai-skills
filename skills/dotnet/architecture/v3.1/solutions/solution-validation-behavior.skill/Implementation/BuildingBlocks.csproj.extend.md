---
description: Add FluentValidation, MediatR, and Ardalis.Result packages for the ValidationBehavior
name: BuildingBlocks.csproj
element_kind: project
change_kind: extend
tags:
  - solution/validation-behavior
  - element/buildingblocks-csproj
---

# Goals
- Own the generic `ValidationBehavior` pipeline behavior that intercepts any `IRequest<TResponse>`
- Keep input validation as a cross-cutting technical concern implemented once and reused across modules

# Core Principles
- BuildingBlocks implements technical patterns using interfaces defined in Shared or provided by MediatR
- BuildingBlocks does not define request markers — it consumes `IRequest<T>` from MediatR

# Implementation changes

**AS IS** (from `plateau-stateless-non-interactive-service`, via `solution-sln-structure` + `solution-mediator-exception-handler`):
```
/BuildingBlocks
  /MediatR
    ExceptionHandlingBehavior.cs
  BuildingBlocks.csproj
```
NuGet: `MediatR`, `Ardalis.Result`, `Microsoft.Extensions.Logging.Abstractions`. Allowed Dependencies: `Shared`.

**TO BE** (after this solution):
```
/BuildingBlocks
  /MediatR
    ExceptionHandlingBehavior.cs
    ValidationBehavior.cs
  BuildingBlocks.csproj
```
NuGet: adds `FluentValidation`. Allowed Dependencies: `Shared` (unchanged).

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
- Never add business logic or request-specific conditions to `ValidationBehavior`
- Never throw exceptions for validation failures — always return `Result.Invalid`

## SHOULD
- Avoid implementing per-request validation logic inside `ValidationBehavior`
- Avoid returning exceptions instead of `Result.Invalid`

# Check list
- [ ] `FluentValidation` referenced in `BuildingBlocks.csproj`
- [ ] `MediatR` referenced in `BuildingBlocks.csproj`
- [ ] `Ardalis.Result` referenced in `BuildingBlocks.csproj`
- [ ] `/BuildingBlocks/MediatR/ValidationBehavior.cs` exists
