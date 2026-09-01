---
name: solution-mediator-integration
description: Defines the MediatR integration pattern for the family — ICommand/IQuery/INotification markers in Shared, request records in {Module}.Interfaces, handler + validator co-location in {Module}.Application, module DI self-registration, and App.Host composition-root wiring. The dispatch mechanism only; repository-backed query handlers belong to solution-query-integration, entity-loading handlers to solution-domain-behaviour.
whenToUse: when declaring a command/query/notification record, writing its handler or validator, wiring a module's MediatR chain into DI, or deciding whether a cross-module interaction should be a Command, a Query, or a Notification
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - concern/architecture
  - application
  - cqrs
  - framework/mediatr
  - command
  - query
  - notification
  - handler
  - pipeline
  - solution/mediator-integration
  - stack/dotnet

creates:
  - Shared.ICommand.cs
  - Shared.IQuery.cs
  - Shared.INotificationEvent.cs
  - "{Module}.Interfaces.Commands.{Command}.cs"
  - "{Module}.Interfaces.Queries.{Query}.cs"
  - "{Module}.Interfaces.Events.{Event}.cs"
  - "{Module}.Application.Features.{FeatureName}.{FeatureName}.Handler.cs"
  - "{Module}.Application.Features.{FeatureName}.{FeatureName}.Validator.cs"
  - "{Module}.Application.{Module}ApplicationRegistration.cs"
extends:
  - Shared.csproj
  - "{Module}.Interfaces.csproj"
  - "{Module}.Application.csproj"
  - App.Host.csproj
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
built_on_plateau:
adr:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/adr/mediator-pattern-is-one-common-solution.md|The MediatR pattern is one common solution, not per-request-kind, and does not depend on the domain layer]]"
---

# Goal
- Define the MediatR integration pattern for the whole family: `ICommand`/`ICommand<TResponse>`, `IQuery<TResponse>`, and `INotificationEvent` markers in `Shared`, and the request/handler/validator conventions every module follows.
- Define where requests are declared — immutable records in `/{Module}.Interfaces/{Commands|Queries|Events}`.
- Define how a handler is structured — guard → (domain call | read) → return `Result<T>` — with the persisted-entity load/stage steps added only when the request actually touches stored state.
- Define module DI self-registration (one extension method, handlers/validators assembly-scanned) and App.Host module wiring.
- Establish that a handler never contains business rules, never calls `SaveChanges`, and never references `DbContext`.

# Capabilities
- One standardized request/handler/validator structure across every module, for writes (Command), reads (Query), and pub/sub (Notification).
- Cross-module interaction via MediatR with no direct type coupling.
- Automatic handler and validator discovery through assembly scanning.
- A consistent `Result<T>`-based contract for Commands and Queries.

# Core Principle
- **Three request kinds, one mechanism** - `ICommand`/`ICommand<T>` = a write (request/response, may mutate state); `IQuery<T>` = a read (request/response, no mutation, no side effect); `INotificationEvent` = a fact already true, published for zero-or-more handlers (pub/sub, no response). The dispatch, handler location, validation, and DI are identical for all three — only the semantics differ.
- **Handler orchestrates, never decides** - A handler that touches a persisted entity delegates every business decision to that entity's own methods and domain services (see [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] when a domain layer exists); a handler with no domain layer only shapes data and dispatches further requests.
- **Fixed handler shape** - guard → (domain call for a Command / repository read for a Query) → return `Result<T>`. Load/stage steps are added around the middle only when stored state is involved; a pure-computation or orchestration-only handler skips them and is still a complete Command/Query.
- **Markers live in `Shared`** - every layer references `Shared` freely; putting a marker in `BuildingBlocks` would force modules to reference a technical-pattern layer.
- One request, one handler. Cross-module calls go through `ISender.Send()` / `IPublisher.Publish()`, never a direct method call.
- A handler never calls `SaveChangesAsync` — committing is the unit-of-work behavior's job (once persistence exists).
- Handlers and validators are registered by assembly scan, never one by one.
- A validator enforces transport correctness only (presence, length, format, range); business invariants belong in the entity. Command/Query validators reuse `IValidator<Soft{ValueObject}>` / `IValidator<{Dto}>` from [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] instead of re-declaring cross-module rules.
- A Query with no persistence yet answers from in-memory/pass-through data or delegates to another module; its repository-backed form arrives with [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] (VP2).

# Boundaries
- The domain layer a Command handler delegates to (`{Module}.Domain` entity methods, domain services) is **not** created by this solution — it is [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] (VP1). A module with no domain layer still uses this solution fully: its handlers orchestrate and shape data. This solution does **not** `depends_on solution-domain-behaviour`.
- Persisted-entity access (`IRepository<T>`/`IReadRepository<T>`) and repository-backed query handlers are `solution-repository-integration` / `solution-query-integration` (VP2), applied separately. `ICommand`/`IQuery`/`INotificationEvent`, the handler/validator structure, and DI wiring are all usable with no persistence.
- The pipeline behaviors that the markers activate (`ValidationBehavior`, `ExceptionHandlingBehavior`) are owned by their own solutions; this solution only ensures a request implements a marker `IRequest<T>`/`INotification`.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj]] - hosts the `ICommand`/`IQuery`/`INotificationEvent` markers
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj]] - hosts request and result records
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj]] - hosts handlers, validators, module registration
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj]] - hosts composition-root wiring
- [[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs]] - validates a request before its handler runs
- [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs]] - validators a request validator reuses through `IValidator<T>`

NUGET:
- `Ardalis.Result` {version} - `Result<T>` and its statuses
- `MediatR` {version} - `IRequest<T>`, `INotification`, `IRequestHandler<,>`, `INotificationHandler<>`, `ISender`, `IPublisher`
- `FluentValidation` + `FluentValidation.DependencyInjectionExtensions` {version} - `AbstractValidator<T>`, `AddValidatorsFromAssembly`
- (versions in `Directory.Packages.props` per [[skills/dotnet/architecture/v3.1/solutions/solution-central-package-management.skill/solution-central-package-management.skill.md|solution-central-package-management]])

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - extend - MediatR package + the marker interfaces
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs]] - create - `ICommand` / `ICommand<TResponse>` write markers
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/IQuery.cs.create.md|IQuery.cs]] - create - `IQuery<TResponse>` read marker
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/INotificationEvent.cs.create.md|INotificationEvent.cs]] - create - `INotificationEvent` pub/sub marker
- [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj]] - extend - `/Commands`, `/Queries`, `/Events` record conventions
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs]] - create - command + result record
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Query}.cs.create.md|{Query}.cs]] - create - query + response record
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Event}.cs.create.md|{Event}.cs]] - create - notification record
- [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - extend - feature-folder layout, handlers, validators, registration
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs]] - create - command **or** query handler
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs]] - create - transport validator
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{EventName}.EventHandler.cs.create.md|{EventName}.EventHandler.cs]] - create - notification handler
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs]] - create - module DI self-registration
- [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - wire module registrations

# Rule

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/App.Host.csproj.extend.md#MUST|App.Host.csproj]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend.md#MUST|Shared.csproj]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md#MUST|ICommand.cs]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/IQuery.cs.create.md#MUST|IQuery.cs]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/INotificationEvent.cs.create.md#MUST|INotificationEvent.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend.md#MUST|{Module}.Application.csproj]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md#MUST|{FeatureName}.Handler.cs]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md#MUST|{FeatureName}.Validator.cs]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{EventName}.EventHandler.cs.create.md#MUST|{EventName}.EventHandler.cs]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md#MUST|{Module}ApplicationRegistration.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md#MUST|{Module}.Interfaces.csproj]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md#MUST|{Command}.cs]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Query}.cs.create.md#MUST|{Query}.cs]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Event}.cs.create.md#MUST|{Event}.cs]]

- Never define `ICommand`/`IQuery`/`INotificationEvent` in `BuildingBlocks` — they live in `Shared`.
  - Risk: a module would need a `BuildingBlocks` reference to declare a request, inverting the layer rule (`BuildingBlocks` is a technical-pattern layer, not a contract layer).
  - Fix: the markers live in `Shared/MediatR`, which every layer may reference.
- Never register a handler or validator by hand (`services.AddTransient<CreateTaskHandler>()`).
  - Risk: a new feature silently has no handler until someone remembers the registration line.
  - Fix: `AddMediatR` and `AddValidatorsFromAssembly` assembly scan on the module assembly.
- Never call another module directly (`_taskService.Create(...)`); dispatch a request defined in the target module's `Interfaces`.
  - Risk: a direct call couples the modules and bypasses the pipeline.
  - Fix: `_sender.Send(new CreateTaskCommand(...))` / publish a notification.
- Never put a business rule, inline LINQ, or `SaveChangesAsync` in a handler.
  - Risk: domain logic leaks out of the entity, queries bypass named specs, and premature commits break atomicity — all invisible to a reader of the entity.
  - Fix: guard → domain call → return; load via named specs (once persistence exists); commit is the unit-of-work behavior's job.
- Never dispatch several top-level commands sequentially from one handler.
  - Risk: partial-failure states with no transaction boundary spanning them.
  - Fix: model the operation as one orchestrating command.
- Name a handler file `{FeatureName}.Handler.cs` / class `{FeatureName}Handler`, co-located with its validator under `Features/{FeatureName}` — never `{FeatureName}CommandHandler.cs`, never a shared validator across commands.
  - Risk: inconsistent names defeat convention-based navigation and assembly scanning assumptions.
  - Fix: one folder per feature, the fixed file/class names, one validator per request.

# Check list
- [ ] `ICommand`/`ICommand<TResponse>`, `IQuery<TResponse>`, `INotificationEvent` all defined in `Shared/MediatR`, members-free.
- [ ] Commands are `record`s in `/{Module}.Interfaces/Commands` implementing `ICommand<Result<T>>` (or `ICommand`); queries in `/Queries` implementing `IQuery<...>`; events past-tense in `/Events` implementing `INotificationEvent`.
- [ ] Response/result records co-located with their request.
- [ ] Each feature has its own folder under `/{Module}.Application/Features`; event handlers under `/Events/{EventName}`.
- [ ] Handler file `{FeatureName}.Handler.cs` / class `{FeatureName}Handler`; event handler `{EventName}.EventHandler.cs` / `{EventName}EventHandler`.
- [ ] Handler follows guard → (domain call | read) → return `Result<T>`; load/stage only when stored state is involved.
- [ ] Handler never injects `DbContext`, never inline LINQ, never `SaveChangesAsync`, never exceptions for flow control.
- [ ] Cross-module: `ISender.Send` for Command/Query, `IPublisher.Publish` for events — never a direct call.
- [ ] Module has one `Register{ModuleName}Module()` extension; handlers via `AddMediatR` scan, validators via `AddValidatorsFromAssembly` scan.
- [ ] One validator per Command/Query, co-located; `AbstractValidator<TRequest>`; transport correctness only.
- [ ] No validator for a notification; no query handler mutates state.
- [ ] No validator exists for any query handler
- [ ] Command validator uses `IValidator<Soft{ValueObject}>` for cross-module Soft VO properties via `SetValidator`
- [ ] Command validator uses `IValidator<{Dto}>` for cross-module DTO properties via `SetValidator`
- [ ] Command validator does not duplicate rules already defined in `{ValueObject}PropertyValidator` or `{Dto}Validator`
