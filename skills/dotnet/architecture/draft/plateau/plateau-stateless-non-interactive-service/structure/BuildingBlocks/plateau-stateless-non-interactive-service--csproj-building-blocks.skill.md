---
name: csproj-building-blocks
description: Project BuildingBlocks in the stateless-non-interactive-service plateau
whenToUse: when adding or editing a reusable framework-level pattern (pipeline behavior, cross-cutting utility) in BuildingBlocks, or deciding whether new code belongs here
domain: skill
type: template
plateau: stateless-non-interactive-service
version: 20260821120000
tags:
  - skill/template/csproj
  - plateau/stateless-non-interactive-service
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]]"
---

# Goal
- Implement application technical patterns used by App.Host and infrastructure across all modules
- Provide pipeline behaviors, repository implementations, and cross-cutting utilities
- Own the generic `ExceptionHandlingBehavior` pipeline behavior that intercepts unhandled exceptions

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

# Core Principles
- BuildingBlocks depends only on Shared
- BuildingBlocks does NOT define common interfaces — it consumes interfaces from Shared
- All pipeline behavior implementations live here — registered once in App.Host, used by all modules
- Keeps global exception handling as a cross-cutting technical concern implemented once and reused across modules

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

# Structure

## Solution place
```
/src/BuildingBlocks
```

## Project Structure
- /BuildingBlocks
  - /MediatR
    - [ExceptionHandlingBehavior.cs](./classes/plateau-stateless-non-interactive-service--class-exception-handling-behavior.skill.md)
  - BuildingBlocks.csproj

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /MediatR | Pipeline behavior implementations and context | |
| ExceptionHandlingBehavior.cs | Catches unhandled exceptions, returns a generic `Result.Error` | [[./classes/plateau-stateless-non-interactive-service--class-exception-handling-behavior.skill.md\|class-exception-handling-behavior]] |

__Applied solutions:__
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Provides `IPipelineBehavior<TRequest, TResponse>` and `IRequest<T>` |
| `Ardalis.Result` | latest stable | Provides `Result.Error` and `IResult` |
| `Microsoft.Extensions.Logging.Abstractions` | latest stable | Provides `ILogger<T>` |

__Applied solutions:__
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Module-specific handlers or validators — belong to module Application
- Common interface definitions — belong to Shared

## Allowed Dependencies
- Shared

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]

# Rules
MUST:
- All pipeline behavior implementations defined here
- BuildingBlocks depends only on Shared
- BuildingBlocks does not define common interfaces — only implements patterns using interfaces from Shared
- `ExceptionHandlingBehavior` placed in `/BuildingBlocks/MediatR`, constrained to `IRequest<TResponse>` from MediatR and `IResult` from Ardalis.Result
MUST NOT:
- BuildingBlocks reference any module project
- BuildingBlocks reference App.Infrastructure or App.Queries
- BuildingBlocks contain business logic
- BuildingBlocks define common interfaces — only Shared defines interfaces
- Add business logic or request-specific conditions to `ExceptionHandlingBehavior`
- Expose original exception messages or stack traces in the returned `Result`

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

# Check list
- [ ] BuildingBlocks.csproj references only Shared
- [ ] BuildingBlocks.csproj contains only pattern implementations
- [ ] No common interface definitions in BuildingBlocks
- [ ] `/BuildingBlocks/MediatR/ExceptionHandlingBehavior.cs` exists

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
