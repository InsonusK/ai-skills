---
name: plateau-core--csproj-building-blocks
description: Project BuildingBlocks in the plateau-core plateau — reusable technical-pattern implementations, at this plateau the two MediatR pipeline behaviors (validation, exception handling)
whenToUse: when adding or editing a MediatR pipeline behavior or another reusable technical pattern in BuildingBlocks, or deciding whether a pattern belongs here rather than in Shared or a module
domain: skill
type: template
plateau: core
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/core
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]"
  - "[[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]]"
---

# Goal
- Give the family one project for reusable technical-pattern implementations — MediatR pipeline behaviors at this plateau; persistence/outbox/concurrency helpers arrive with their own features.
- Keep it referencing only `Shared`.

# Core Principles
- `BuildingBlocks` implements patterns; it never *defines* a cross-cutting contract — those live in `Shared`.
- `BuildingBlocks` references only `Shared` (and MediatR / Ardalis.Result / Logging.Abstractions from NuGet).
- Behavior order is not decided here — it lives in `App.Host`'s [[../App.Host/classes/plateau-core--class-pipeline-registration.skill.md|PipelineRegistration]].

# Structure

## Solution place
```
/src/BuildingBlocks
```

## Project Structure
- /BuildingBlocks
  - /MediatR
    - [ValidationBehavior.cs](./classes/plateau-core--class-validation-behavior.skill.md) — collect-all FluentValidation, short-circuit with `Result.Invalid`
    - [ExceptionHandlingBehavior.cs](./classes/plateau-core--class-exception-handling-behavior.skill.md) — catch `Exception`, log `Critical` with `LogEvents.UnhandledException`, return `Result.Error`
  - BuildingBlocks.csproj

Later features add `UnitOfWorkBehavior.cs` + `UnitOfWorkContext.cs` (VP2), `ConcurrencyBehavior.cs` (VP5), `GuidResolvingBehavior.cs` (VP6) — none at plateau-core.

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /MediatR/ValidationBehavior.cs | Runs every `IValidator<TRequest>` before the handler; short-circuits invalid requests | [[./classes/plateau-core--class-validation-behavior.skill.md\|class-validation-behavior]] |
| /MediatR/ExceptionHandlingBehavior.cs | Catch-all that turns an unhandled exception into a generic `Result.Error` | [[./classes/plateau-core--class-exception-handling-behavior.skill.md\|class-exception-handling-behavior]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| MediatR | central | `IPipelineBehavior<TRequest, TResponse>` |
| FluentValidation | central | `IValidator<T>`, `ValidationFailure` (ValidationBehavior) |
| Ardalis.Result | central | `Result.Invalid` / `Result.Error` / `IResult` |
| Microsoft.Extensions.Logging.Abstractions | central | `ILogger<T>` |

## What Does NOT Belong Here
- Business logic, entities — belong to `{Module}.Domain`.
- Module-specific handlers/validators — belong to [[../{Module}.Application/plateau-core--csproj-module-application.skill.md|{Module}.Application]].
- Cross-cutting contract definitions (`ICommand`, `IUnitOfWork`, …) — belong to [[../Shared/plateau-core--csproj-shared.skill.md|Shared]].

## Allowed Dependencies
- `Shared`
- NuGet: `MediatR`, `FluentValidation`, `Ardalis.Result`, `Microsoft.Extensions.Logging.Abstractions`

# Rules
MUST:
- Reference only `Shared` — never a module or infrastructure project.
- Implement patterns here; never define a cross-cutting interface here — contracts live in `Shared`.
- Keep every behavior generic (`where TRequest : IRequest<TResponse>`, `where TResponse : IResult` where the behavior returns a `Result`); no request-specific branching.
- Never put business logic or a module-specific handler/validator in `BuildingBlocks`.

# Check list
- [ ] `BuildingBlocks.csproj` references only `Shared` (+ the four NuGet packages, versionless).
- [ ] `/MediatR/ValidationBehavior.cs` and `/MediatR/ExceptionHandlingBehavior.cs` exist; no other behavior.
- [ ] No contract definitions, no business logic, no module-specific type.

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]] - [[../../../../solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
