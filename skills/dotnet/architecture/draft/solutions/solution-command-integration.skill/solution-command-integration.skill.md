---
name: solution-command-integration
description: Defines the integrated command pipeline — ICommand marker in Shared, command records in {Module}.Interfaces, handler and validator co-location in {Module}.Application/Features, module DI self-registration, and App.Host composition-root wiring
whenToUse: when implementing a new write operation — declaring a command record, writing its handler and validator, or wiring a module's command-handling chain into DI
domain: skill
type: architecture
version: 20260611
tags:
  - skill/architecture/solution
  - stack/dotnet
  - application
  - cqrs
  - framework/mediatr
  - command
  - handler
  - pipeline
  - concern/architecture
  - solution/command-integration

creates:
  - Shared.ICommand.cs
  - "{Module}.Interfaces.Commands.{Command}.cs"
  - "{Module}.Application.Features.{FeatureName}.{FeatureName}.Handler.cs"
  - "{Module}.Application.Features.{FeatureName}.{FeatureName}.Validator.cs"
  - "{Module}.Application.{Module}ApplicationRegistration.cs"
extends:
  - Shared.csproj
  - "{Module}.Interfaces.csproj"
  - "{Module}.Application.csproj"
  - App.Host.csproj
depends_on:
  - "[[skills/dotnet/architecture/draft/solutions/solution-validation-behavior.skill/solution-validation-behavior.skill|solution-validation-behavior]]"
  - "[[skills/dotnet/architecture/draft/solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill|solution-dto-property-validators]]"
  - "[[skills/dotnet/architecture/draft/solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]]"
built_on_plateau: "[[skills/dotnet/architecture/draft/plateau/plateau-stateless-non-interactive-service/plateau-stateless-non-interactive-service.skill/plateau-stateless-non-interactive-service.skill.md|plateau-stateless-non-interactive-service]]"
---

# Goal
- Define `ICommand` (no payload) and `ICommand<TResponse>` in Shared as the marker interfaces that identify write operations and activate pipeline behaviors
- Define where and how commands are declared — as immutable records in `/{Module}.Interfaces/Commands`
- Define how handlers are structured — guard, domain call, return `Result<T>`; load/stage a persisted entity through `IRepository<T>`/`IReadRepository<T>` only when the command actually needs one
- Define how validators are structured — transport correctness only, co-located with the handler in the feature folder
- Define module DI registration — each module self-registers via one extension method, handlers and validators auto-scanned
- Define App.Host module wiring — centralized module registrations assembled in the composition root
- Establish that handlers never contain business logic, never call `SaveChanges`, and never reference `DbContext`

# Capabilities
- Standardized command/handler/validator structure across all modules
- Clear separation between transport validation and domain logic
- Cross-module write operations via MediatR without direct coupling
- Automatic handler and validator discovery through assembly scanning
- Consistent `Result<T>`-based response contract for all write operations

# Core Principles
- Handler orchestrates — it never contains business rules
- Domain layer decides — a handler that touches a persisted entity delegates all decisions to that entity's own methods and domain services (see [[skills/dotnet/architecture/draft/solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]])
- Handler follows a fixed structure: guard → domain call → return result. A command that reads or mutates a persisted entity adds load/stage steps around the domain call, through `IRepository<T>`/`IReadRepository<T>` — a command with no persisted state (pure computation, an external call, orchestration only) skips load/stage entirely; it is still a command, not a lesser or different kind of write operation
- Handler returns `Ardalis.Result<T>` — all outcomes expressed as typed results, no exceptions for flow control
- `ICommand<TResponse>` lives in Shared — every layer can reference it without coupling to BuildingBlocks
- One command, one handler — no shared handlers, no handler dispatching multiple top-level commands
- Cross-module writes go through `_mediator.Send()` — never via direct method calls on another module's classes
- Handler never calls `SaveChangesAsync` — committing is the Unit of Work's responsibility
- Handlers and validators are registered via assembly scan — never manually one by one
- Validator enforces transport correctness only — presence, length, format, range
- Business invariants belong in domain entities and domain services — never in validators
- One validator per command — co-located with the handler in the same feature folder
- Command validators use `IValidator<Soft{ValueObject}>` and `IValidator<{Dto}>` from `solution-dto-property-validators.skill` instead of duplicating cross-module validation rules

# Boundaries
- Persisted-entity access (`IRepository<T>`/`IReadRepository<T>`) is not provided by this solution — that is `solution-repository-integration`'s job, applied separately once a command needs it (not yet migrated to draft as of this plateau). This solution does not require persistence to exist: `ICommand`, handler/validator structure, and DI/composition-root wiring are all usable by a command that never touches a stored aggregate.
- Whether a given command needs load/stage at all is a per-command decision, not something this solution mandates — a command whose entire effect is a domain-service computation, an external call, or dispatching further commands has no `IRepository<T>` dependency and is not thereby a lesser or incomplete application of this solution.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/draft/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/draft/solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]] - hosts the `ICommand<T>` marker interface project
  - [[skills/dotnet/architecture/draft/solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create|{Module}.Interfaces.csproj]] - hosts command and result records
  - [[skills/dotnet/architecture/draft/solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create|{Module}.Application.csproj]] - hosts handlers, validators, and module registration
  - [[skills/dotnet/architecture/draft/solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create|App.Host.csproj]] - hosts composition-root wiring
- [[skills/dotnet/architecture/draft/solutions/solution-validation-behavior.skill/solution-validation-behavior.skill|solution-validation-behavior]]
  - [[skills/dotnet/architecture/draft/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend|BuildingBlocks.csproj]] - provides `ValidationBehavior` pipeline behavior
    - [[skills/dotnet/architecture/draft/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create|ValidationBehavior.cs]] - intercepts and validates commands before handlers run
- [[skills/dotnet/architecture/draft/solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill|solution-dto-property-validators]]
  - [[skills/dotnet/architecture/draft/solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend|{Module}.Application.csproj]] - provides `{ValueObject}PropertyValidator` and `{Dto}Validator` that command validators reuse through `IValidator<T>`
- [[skills/dotnet/architecture/draft/solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]]
  - [[skills/dotnet/architecture/draft/solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]] - provides the entity behavior methods and domain services a handler's domain call delegates to

NUGET:
- `Ardalis.Result` {version} - provides `Result<T>`, `Result.Created`, `Result.NotFound`, `Result.Conflict`, `Result.Error`, `Result.Invalid`
- `MediatR` {version} - provides `IRequest<T>`, `IRequestHandler<TRequest, TResponse>`, `ISender`, `IMediator`
- `FluentValidation` {version} - provides `AbstractValidator<T>`, `RuleFor`, validation rule DSL
- `FluentValidation.DependencyInjectionExtensions` {version} - provides `AddValidatorsFromAssembly`

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - extend - Add MediatR package and the `ICommand` marker interfaces
  - [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create|ICommand.cs]] - create - Write operation marker interfaces
- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend|{Module}.Interfaces.csproj]] - extend - Add command record conventions in `/Commands`
  - [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create|{Command}.cs]] - create - Command and result record declaration
- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend|{Module}.Application.csproj]] - extend - Add feature folder layout, handlers, validators, and module registration
  - [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create|{FeatureName}.Handler.cs]] - create - Command handler implementation
  - [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create|{FeatureName}.Validator.cs]] - create - Transport correctness validator
  - [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create|{Module}ApplicationRegistration.cs]] - create - Module DI self-registration extension
- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - extend - Wire module registrations in the composition root

# Rules

## MUST:
- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/App.Host.csproj.extend#MUST|App.Host.csproj]]
- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend#MUST|Shared.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create#MUST|ICommand.cs]]
- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend#MUST|{Module}.Application.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create#MUST|{FeatureName}.Handler.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create#MUST|{FeatureName}.Validator.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create#MUST|{Module}ApplicationRegistration.cs]]
- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend#MUST|{Module}.Interfaces.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create#MUST|{Command}.cs]]

## MUST NOT:
- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/App.Host.csproj.extend#MUST NOT|App.Host.csproj]]
- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend#MUST NOT|Shared.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create#MUST NOT|ICommand.cs]]
- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend#MUST NOT|{Module}.Application.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create#MUST NOT|{FeatureName}.Handler.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create#MUST NOT|{FeatureName}.Validator.cs]]
	- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create#MUST NOT|{Module}ApplicationRegistration.cs]]
- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend#MUST NOT|{Module}.Interfaces.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create#MUST NOT|{Command}.cs]]

# Anti-patterns
- Business rule in handler: `if (task.Status == TaskStatus.Closed) return Result.Conflict(...)` — belongs in entity
- Inline LINQ in handler: `_repository.FirstOrDefaultAsync(x => x.Id == id, ct)` — use named spec
- Manual handler registration in module: `services.AddTransient<CreateTaskHandler>()` — use assembly scan
- `SaveChangesAsync` in handler — Unit of Work commits after handler returns
- Direct call to another module: `_taskService.Create(...)` — use `_mediator.Send(new CreateTaskCommand(...))`
- `CreateTaskCommandHandler.cs` as file name — use `CreateTask.Handler.cs`
- Multiple top-level commands dispatched sequentially from one handler — design as a single orchestrating command
- `ICommand` defined in BuildingBlocks — modules would need a BuildingBlocks reference, violating layer rules
- `RuleFor(x => x.AssigneeId).MustAsync(async (id, ct) => await _repo.AnyAsync(...))` — entity existence is a handler guard, not transport validation
- Validator placed outside its feature folder — always co-located with handler
- Shared validator used for multiple commands: `public class EntityIdValidator : AbstractValidator<IHasId>` — one validator per command
- Business invariant in validator: `RuleFor(x => x.Status).Must(s => s != TaskStatus.Closed)` — belongs in domain entity
- Duplicating Soft{ValueObject} validation rules in a command validator instead of using `IValidator<Soft{ValueObject}>`

# Check list
- [ ] `ICommand : IRequest<Result>` and `ICommand<TResponse> : IRequest<TResponse>` defined in `Shared/MediatR/ICommand.cs`
- [ ] All commands declared as `record` in `/{Module}.Interfaces/Commands`
- [ ] All commands implement `ICommand<Result<T>>` for a result payload of `T`, or `ICommand<Result>` when there is no payload beyond success/failure; bare `ICommand` is reserved for commands with no persisted-entity effect at all
- [ ] Result records co-located with their command in the same file
- [ ] Each feature has its own folder under `/{Module}.Application/Features`
- [ ] Handler file named `{FeatureName}.Handler.cs`
- [ ] Handler class named `{FeatureName}Handler`
- [ ] Handler implements `IRequestHandler<TCommand, Result<T>>`
- [ ] Handler never injects `DbContext` directly — a command that touches a persisted entity injects `IRepository<T>`/`IReadRepository<T>` instead
- [ ] Handler loads entities via named specs — no inline LINQ
- [ ] Handler follows guard → domain call → return structure, with load/stage added around the domain call only when the command touches a persisted entity
- [ ] Handler returns `Result<T>` for all outcomes — no exceptions for flow control
- [ ] Handler never calls `SaveChangesAsync`
- [ ] Cross-module writes dispatched via `_mediator.Send()`
- [ ] Module has `Register{ModuleName}Module()` extension method
- [ ] Handlers registered via `AddMediatR` assembly scan
- [ ] Validators registered via `AddValidatorsFromAssembly` assembly scan
- [ ] One validator per command in `/{Module}.Application/Features/{FeatureName}`
- [ ] Validator file named `{FeatureName}.Validator.cs`
- [ ] Validator class named `{FeatureName}Validator`
- [ ] Validator extends `AbstractValidator<TCommand>`
- [ ] Validator rules cover transport correctness only — no business rules, no DB access
- [ ] No validator exists for any query handler
- [ ] Command validator uses `IValidator<Soft{ValueObject}>` for cross-module Soft VO properties via `SetValidator`
- [ ] Command validator uses `IValidator<{Dto}>` for cross-module DTO properties via `SetValidator`
- [ ] Command validator does not duplicate rules already defined in `{ValueObject}PropertyValidator` or `{Dto}Validator`
