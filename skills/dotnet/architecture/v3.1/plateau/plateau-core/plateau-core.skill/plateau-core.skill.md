---
name: plateau-core
description: The v3.1 common baseline — Central Package Management, the two-project module layout (Interfaces + Application), the MediatR command/query/notification mechanism, the validation and exception pipeline behaviors, boundary Soft Value Objects with cross-module validators, structured console logging, and the conformance test harness. No domain layer, no persistence, no API surface.
whenToUse: when scaffolding a brand-new service repository or a new module before any domain logic, persistence, or API exists; or when reviewing whether a change to the composition root, the MediatR conventions, the validation/exception pipeline, Soft Value Objects, logging, or the test-project layout follows this baseline
domain: skill
type: template
version: 20260902000000
tags:
  - skill/template/plateau
  - plateau/core
  - stack/dotnet
parent_plateaus:
created_by:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-central-package-management.skill/solution-central-package-management.skill.md|solution-central-package-management]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/solution-soft-value-objects.skill.md|solution-soft-value-objects]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
standalone: false
---

# Goal
Establish the common baseline every v3.1 service shares before any variability is chosen: a repository with Central Package Management, modules made of exactly `Interfaces` + `Application`, and a composition root that wires the MediatR pipeline, module registration, and logging. A module at this plateau has public contracts, validated request dispatch, notification pub/sub, and a conformance test suite — but **no domain layer, no persistence, and no external API**.

# Core Principles
- **Central Package Management** — every NuGet version is pinned once in `Directory.Packages.props`; project files carry versionless `<PackageReference>`.
- **Two-project module** — a module is `{Module}.Interfaces` (public contracts) + `{Module}.Application` (handlers, validators). `{Module}.Domain` and `{Module}.Api` do not exist here; they arrive with their features.
- **One MediatR mechanism** — `ICommand`/`ICommand<T>`, `IQuery<T>`, `INotificationEvent` in `Shared/MediatR`. Cross-module interaction is `ISender.Send` / `IPublisher.Publish` against `{Module}.Interfaces` contracts, never a direct call. Handlers follow `guard → work/dispatch → return Result<T>`; at this plateau there is no `load/stage` step.
- **Pipeline, ordered in one place** — `ExceptionHandlingBehavior` first (wraps everything, logs `Critical`, returns a generic `Result.Error`), then `ValidationBehavior` (collect-all, short-circuits with `Result.Invalid` before the handler). Order lives only in `PipelineRegistration.AddPipeline()`.
- **Soft Value Objects at the boundary** — a value carrying business meaning on a DTO/command/query is a `Soft{ValueObject}` record in `{Module}.Interfaces` (permissive, no validation); a `{ValueObject}PropertyValidator : AbstractValidator<Soft{ValueObject}>` owns its condition and is resolvable cross-module via `IValidator<T>`.
- **Structured logging** — every class logs through `ILogger<T>`; the provider and levels are configured once in `App.Host`; searched-for lines carry an `EventId` from `Shared.Logging.LogEvents`.
- **One test project per production project** — `Shared.Tests`, `BuildingBlocks.Tests`, `{Module}.Interfaces.Tests`, `{Module}.Application.Tests`, each mirroring its counterpart's allowed dependencies. `{Module}.Domain.Tests` appears only with a domain layer. The `make unit-test` target is the gate.

# Capabilities
- request dispatch
  - Commands, queries, and notifications dispatched through MediatR; every request validated by `ValidationBehavior` before its handler; every unhandled exception mapped to `Result.Error` by `ExceptionHandlingBehavior`.
- cross-module contracts
  - A module exposes `Soft{ValueObject}`, DTOs, and command/query/event records from `{Module}.Interfaces`; another module consumes them and resolves `IValidator<Soft{ValueObject}>` / `IValidator<{Dto}>` from DI without referencing internals.
- composition
  - `Program.cs` calls only `AddAppLogging()`, `AddModules()`, `AddPipeline()`. Each module self-registers via `Add{Module}Module()` (MediatR + validator assembly scan).
- conformance
  - `make unit-test` runs every test project; `make test-report` adds coverage; `make mutation-test` runs Stryker (heavy, off the fast gate).

# Usecases

## Dispatch a command through the validated pipeline
```mermaid
sequenceDiagram
    autonumber
    participant Caller
    participant Ex as ExceptionHandlingBehavior
    participant Val as ValidationBehavior
    participant H as {Feature}Handler
    participant P as IPublisher
    Caller->>Ex: Send({Feature}Command)
    Ex->>Val: next()
    alt invalid
        Val-->>Caller: Result.Invalid(errors)
    else valid
        Val->>H: next()
        H->>P: Publish({Fact}Event)
        H-->>Caller: Result.Success(payload)
    end
```

## Validate another module's Soft Value Object
```mermaid
sequenceDiagram
    autonumber
    participant B as Module B validator
    participant DI
    participant A as Module A's {ValueObject}PropertyValidator
    B->>DI: resolve IValidator<Soft{ValueObject}>
    DI-->>B: A's validator instance
    B->>A: SetValidator(...) inside a {Dto}Validator rule
```

# Structure
See [[skills/dotnet/architecture/v3.1/plateau/plateau-core/structure/plateau-core--sln-core.skill.md|plateau-core--sln-core]] for the repository layout and the per-project / per-class skills.

# Example
A complete, runnable minimal service is in [`example/`](./example/) — a `Sample` module with one command, one query, one notification, and one Soft Value Object, wired through the full pipeline. `dotnet build Sample.slnx` and `make unit-test` are green (the plateau's ground-truth check).
