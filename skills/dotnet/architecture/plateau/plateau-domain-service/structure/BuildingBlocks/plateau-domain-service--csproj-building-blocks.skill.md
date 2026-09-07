---
name: plateau-domain-service--csproj-building-blocks
description: Project BuildingBlocks in the plateau-domain-service plateau — reusable technical-pattern implementations, at this plateau the two MediatR pipeline behaviors (validation, exception handling)
whenToUse: when adding or editing a MediatR pipeline behavior or another reusable technical pattern in BuildingBlocks, or deciding whether a pattern belongs here rather than in Shared or a module
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/domain-service
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/solutions/solution-validation-behavior.skill/solution-validation-behavior.skill|solution-validation-behavior]]"
  - "[[skills/dotnet/architecture/solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]]"
  - "[[skills/dotnet/architecture/solutions/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]]"
  - "[[skills/dotnet/architecture/solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]]"
---

# Goal
- Give the family one project for reusable technical-pattern implementations — MediatR pipeline behaviors at this plateau; persistence/outbox/concurrency helpers arrive with their own features.
- Keep it referencing only `Shared`.

# Core Principles
- `BuildingBlocks` implements patterns; it never *defines* a cross-cutting contract — those live in `Shared`.
- `BuildingBlocks` references only `Shared` (and MediatR / Ardalis.Result / Logging.Abstractions from NuGet).
- Behavior order is not decided here — it lives in `App.Host`'s [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/App.Host/classes/plateau-domain-service--class-pipeline-registration.skill|PipelineRegistration]].

# Structure

## Solution place
```
/src/BuildingBlocks
```

## Project Structure
- /BuildingBlocks
  - /MediatR
    - [ValidationBehavior.cs](skills/dotnet/architecture/plateau/plateau-domain-service/structure/BuildingBlocks/classes/plateau-domain-service--class-validation-behavior.skill.md) — collect-all FluentValidation, short-circuit with `Result.Invalid`
    - [ExceptionHandlingBehavior.cs](skills/dotnet/architecture/plateau/plateau-domain-service/structure/BuildingBlocks/classes/plateau-domain-service--class-exception-handling-behavior.skill.md) — catch `Exception`, log `Critical` with `LogEvents.UnhandledException`, return `Result.Error`
    - [ConcurrencyBehavior.cs](skills/dotnet/architecture/plateau/plateau-domain-service/structure/BuildingBlocks/classes/plateau-domain-service--class-concurrency-behavior.skill.md) — guard `IHasVersions` commands against stale writes (VP5)
    - [UnitOfWorkContext.cs](skills/dotnet/architecture/plateau/plateau-domain-service/structure/BuildingBlocks/classes/plateau-domain-service--class-unit-of-work-context.skill.md) — scoped nesting-depth counter (VP2)
    - [UnitOfWorkBehavior.cs](skills/dotnet/architecture/plateau/plateau-domain-service/structure/BuildingBlocks/classes/plateau-domain-service--class-unit-of-work-behavior.skill.md) — commit once, last, after the outermost command (VP2)
  - BuildingBlocks.csproj

`GuidResolvingBehavior.cs` (VP6) arrives with plateau-offline-sync-service.

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /MediatR/ValidationBehavior.cs | Runs every `IValidator<TRequest>` before the handler; short-circuits invalid requests | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/BuildingBlocks/classes/plateau-domain-service--class-validation-behavior.skill\|class-validation-behavior]] |
| /MediatR/ExceptionHandlingBehavior.cs | Catch-all that turns an unhandled exception into a generic `Result.Error` | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/BuildingBlocks/classes/plateau-domain-service--class-exception-handling-behavior.skill\|class-exception-handling-behavior]] |
| /MediatR/ConcurrencyBehavior.cs | Version guard for `IHasVersions` commands | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/BuildingBlocks/classes/plateau-domain-service--class-concurrency-behavior.skill\|class-concurrency-behavior]] |
| /MediatR/UnitOfWorkContext.cs | Scoped nesting-depth counter | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/BuildingBlocks/classes/plateau-domain-service--class-unit-of-work-context.skill\|class-unit-of-work-context]] |
| /MediatR/UnitOfWorkBehavior.cs | Atomic commit after the outermost command | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/BuildingBlocks/classes/plateau-domain-service--class-unit-of-work-behavior.skill\|class-unit-of-work-behavior]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| MediatR | central | `IPipelineBehavior<TRequest, TResponse>` |
| FluentValidation | central | `IValidator<T>`, `ValidationFailure` (ValidationBehavior) |
| Ardalis.Result | central | `Result.Invalid` / `Result.Error` / `IResult` |
| Microsoft.Extensions.Logging.Abstractions | central | `ILogger<T>` |

## What Does NOT Belong Here
- Business logic, entities — belong to `{Module}.Domain`.
- Module-specific handlers/validators — belong to [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/{Module}.Application/plateau-domain-service--csproj-module-application.skill|{Module}.Application]].
- Cross-cutting contract definitions (`ICommand`, `IUnitOfWork`, …) — belong to [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/Shared/plateau-domain-service--csproj-shared.skill|Shared]].

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
- [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]]
- [[skills/dotnet/architecture/solutions/solution-validation-behavior.skill/solution-validation-behavior.skill|solution-validation-behavior]] - [[skills/dotnet/architecture/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend|BuildingBlocks.csproj]]
- [[skills/dotnet/architecture/solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend|BuildingBlocks.csproj]]
