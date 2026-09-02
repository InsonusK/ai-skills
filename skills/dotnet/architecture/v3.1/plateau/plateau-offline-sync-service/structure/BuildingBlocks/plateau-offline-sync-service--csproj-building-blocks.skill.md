---
name: plateau-offline-sync-service--csproj-building-blocks
description: Project BuildingBlocks in the plateau-offline-sync-service plateau — reusable technical-pattern implementations, at this plateau the two MediatR pipeline behaviors (validation, exception handling)
whenToUse: when adding or editing a MediatR pipeline behavior or another reusable technical pattern in BuildingBlocks, or deciding whether a pattern belongs here rather than in Shared or a module
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/offline-sync-service
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]"
  - "[[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]]"
  - "[[../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]"
  - "[[../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
---

# Goal
- Give the family one project for reusable technical-pattern implementations — MediatR pipeline behaviors at this plateau; persistence/outbox/concurrency helpers arrive with their own features.
- Keep it referencing only `Shared`.

# Core Principles
- `BuildingBlocks` implements patterns; it never *defines* a cross-cutting contract — those live in `Shared`.
- `BuildingBlocks` references only `Shared` (and MediatR / Ardalis.Result / Logging.Abstractions from NuGet).
- Behavior order is not decided here — it lives in `App.Host`'s [[../App.Host/classes/plateau-offline-sync-service--class-pipeline-registration.skill.md|PipelineRegistration]].

# Structure

## Solution place
```
/src/BuildingBlocks
```

## Project Structure
- /BuildingBlocks
  - /MediatR
    - [ValidationBehavior.cs](./classes/plateau-offline-sync-service--class-validation-behavior.skill.md) — collect-all FluentValidation, short-circuit with `Result.Invalid`
    - [ExceptionHandlingBehavior.cs](./classes/plateau-offline-sync-service--class-exception-handling-behavior.skill.md) — catch `Exception`, log `Critical` with `LogEvents.UnhandledException`, return `Result.Error`
    - [ConcurrencyBehavior.cs](./classes/plateau-offline-sync-service--class-concurrency-behavior.skill.md) — guard `IHasVersions` commands against stale writes (VP5)
    - [UnitOfWorkContext.cs](./classes/plateau-offline-sync-service--class-unit-of-work-context.skill.md) — scoped nesting-depth counter (VP2)
    - [UnitOfWorkBehavior.cs](./classes/plateau-offline-sync-service--class-unit-of-work-behavior.skill.md) — commit once, last, after the outermost command (VP2)
  - BuildingBlocks.csproj

    - [GuidResolvingBehavior.cs](./classes/plateau-offline-sync-service--class-guid-resolving-behavior.skill.md) — idempotent create for an `IHasGuid` command (VP6), registered after `ConcurrencyBehavior`

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /MediatR/ValidationBehavior.cs | Runs every `IValidator<TRequest>` before the handler; short-circuits invalid requests | [[./classes/plateau-offline-sync-service--class-validation-behavior.skill.md\|class-validation-behavior]] |
| /MediatR/ExceptionHandlingBehavior.cs | Catch-all that turns an unhandled exception into a generic `Result.Error` | [[./classes/plateau-offline-sync-service--class-exception-handling-behavior.skill.md\|class-exception-handling-behavior]] |
| /MediatR/ConcurrencyBehavior.cs | Version guard for `IHasVersions` commands | [[./classes/plateau-offline-sync-service--class-concurrency-behavior.skill.md\|class-concurrency-behavior]] |
| /MediatR/UnitOfWorkContext.cs | Scoped nesting-depth counter | [[./classes/plateau-offline-sync-service--class-unit-of-work-context.skill.md\|class-unit-of-work-context]] |
| /MediatR/UnitOfWorkBehavior.cs | Atomic commit after the outermost command | [[./classes/plateau-offline-sync-service--class-unit-of-work-behavior.skill.md\|class-unit-of-work-behavior]] |
| /MediatR/GuidResolvingBehavior.cs | Idempotent create for an `IHasGuid` command | [[./classes/plateau-offline-sync-service--class-guid-resolving-behavior.skill.md\|class-guid-resolving-behavior]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| MediatR | central | `IPipelineBehavior<TRequest, TResponse>` |
| FluentValidation | central | `IValidator<T>`, `ValidationFailure` (ValidationBehavior) |
| Ardalis.Result | central | `Result.Invalid` / `Result.Error` / `IResult` |
| Microsoft.Extensions.Logging.Abstractions | central | `ILogger<T>` |

## What Does NOT Belong Here
- Business logic, entities — belong to `{Module}.Domain`.
- Module-specific handlers/validators — belong to [[../{Module}.Application/plateau-offline-sync-service--csproj-module-application.skill.md|{Module}.Application]].
- Cross-cutting contract definitions (`ICommand`, `IUnitOfWork`, …) — belong to [[../Shared/plateau-offline-sync-service--csproj-shared.skill.md|Shared]].

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
