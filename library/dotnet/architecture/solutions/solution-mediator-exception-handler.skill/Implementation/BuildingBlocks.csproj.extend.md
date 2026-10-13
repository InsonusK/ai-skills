---
description: Add ExceptionHandlingBehavior pipeline behavior to BuildingBlocks
name: BuildingBlocks.csproj
element_kind: project
change_kind: extend
tags:
  - solution/mediator-exception-handler
  - element/buildingblocks-csproj
---

# Goals
- Own the generic `ExceptionHandlingBehavior` pipeline behavior that intercepts unhandled exceptions
- Keep global exception handling as a cross-cutting technical concern implemented once and reused across modules

# Core Principles
- BuildingBlocks implements technical patterns using interfaces defined in Shared or provided by MediatR
- BuildingBlocks does not define request markers — it consumes `IRequest<T>` from MediatR

# Structure

## Project Structure
```
/BuildingBlocks
  /MediatR
    ExceptionHandlingBehavior.cs
```

## Directory and class skills
| Directory | file | Description |
| ----------------- | --------------------- | -------------------------------------------------- |
| /MediatR | ExceptionHandlingBehavior.cs | Pipeline behavior that catches unhandled exceptions and returns a generic Result error |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Provides `IPipelineBehavior<TRequest, TResponse>` and `IRequest<T>` |
| `Ardalis.Result` | latest stable | Provides `Result.Error` and `IResult` |
| `Microsoft.Extensions.Logging.Abstractions` | latest stable | Provides `ILogger<T>` |

# Allowed Dependencies
- Shared

# Rules

## MUST
- `MediatR`, `Ardalis.Result`, and `Microsoft.Extensions.Logging.Abstractions` packages referenced in `BuildingBlocks.csproj`
- `ExceptionHandlingBehavior` placed in `/BuildingBlocks/MediatR`
- `ExceptionHandlingBehavior` constrained to `notnull` and `IResult` from Ardalis.Result
- Never add business logic or request-specific conditions to `ExceptionHandlingBehavior`
- Never expose original exception messages or stack traces in the returned `Result`

## SHOULD
- Avoid implementing per-request exception handling inside `ExceptionHandlingBehavior`
- Avoid returning exception details instead of a generic `Result.Error`

# Check list
- [ ] `MediatR` referenced in `BuildingBlocks.csproj`
- [ ] `Ardalis.Result` referenced in `BuildingBlocks.csproj`
- [ ] `Microsoft.Extensions.Logging.Abstractions` referenced in `BuildingBlocks.csproj`
- [ ] `/BuildingBlocks/MediatR/ExceptionHandlingBehavior.cs` exists
