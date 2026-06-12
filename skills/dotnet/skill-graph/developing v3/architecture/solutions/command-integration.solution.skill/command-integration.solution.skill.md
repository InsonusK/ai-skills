---
uid: 9449536e-6300-484a-b0b3-66a00a1929e1
name: command-integration
description: Defines the integrated command pipeline — ICommand marker in Shared, command records in {Module}.Interfaces, handler and validator co-location in {Module}.Application/Features, module DI self-registration, and App.Host composition-root wiring
domain: skill
type: architecture
version: 20260611
tags:
  - skill/architecture/solution
  - dotnet
  - application
  - cqrs
  - mediatr
  - command
  - handler
  - pipeline
triggers:
  - implement command handler
  - create command
  - write command handler
  - handle write operation
  - add feature to module
creates:
  - Shared.MediatR.ICommand.cs
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
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior.solution.skill]]"
---

# Goal
- Define `ICommand<TResponse>` in Shared as the marker interface that identifies write operations and activates pipeline behaviors
- Define where and how commands are declared — as immutable records in `/{Module}.Interfaces/Commands`
- Define how handlers are structured — load via spec, guard, domain call, stage, return `Result<T>`
- Define how validators are structured — transport correctness only, co-located with the handler in the feature folder
- Define module DI registration — each module self-registers via one extension method, handlers and validators auto-scanned
- Define App.Host module wiring — centralized module registrations assembled in the composition root
- Establish that handlers never contain business logic, never call `SaveChanges`, and never reference `DbContext`

# Core Principles
- Handler orchestrates — it never contains business rules
- Domain layer decides — handlers load data and delegate all decisions to entities and domain services
- Handler follows a fixed structure: load → guard → domain call → stage → return result
- Handler returns `Ardalis.Result<T>` — all outcomes expressed as typed results, no exceptions for flow control
- `ICommand<TResponse>` lives in Shared — every layer can reference it without coupling to BuildingBlocks
- One command, one handler — no shared handlers, no handler dispatching multiple top-level commands
- Cross-module writes go through `_mediator.Send()` — never via direct method calls on another module's classes
- Handler never calls `SaveChangesAsync` — committing is the Unit of Work's responsibility
- Handlers and validators are registered via assembly scan — never manually one by one
- Validator enforces transport correctness only — presence, length, format, range
- Business invariants belong in domain entities and domain services — never in validators
- One validator per command — co-located with the handler in the same feature folder

# Requirements
- `Ardalis.Result` NuGet package — provides `Result<T>`, `Result.Created`, `Result.NotFound`, `Result.Conflict`, `Result.Error`, `Result.Invalid`
- `MediatR` NuGet package — provides `IRequest<T>`, `IRequestHandler<TRequest, TResponse>`, `ISender`, `IMediator`
- `FluentValidation` NuGet package — provides `AbstractValidator<T>`, `RuleFor`, validation rule DSL
- `FluentValidation.DependencyInjectionExtensions` NuGet package — provides `AddValidatorsFromAssembly`
- definition of `module project structure` — [[solution-structure.solution.skill]] defines the projects where commands, handlers, and validators live
- definition of `IRepository<T>` — [[repository-integration.solution.skill]] defines the repository abstraction used by handlers to load and stage entities
- definition of `validation pipeline` — [[validation-behavior.solution.skill]] defines the `ValidationBehavior` that intercepts and validates commands before handlers run

# Template Skill Mutations

PROJECT:
- [[./Implementation/Shared.csproj.extend.md|Shared.csproj]] - extend - Add MediatR package and the `ICommand` marker interfaces
  - [[./Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs]] - create - Write operation marker interfaces
- [[./Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj]] - extend - Add command record conventions in `/Commands`
  - [[./Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs]] - create - Command and result record declaration
- [[./Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - extend - Add feature folder layout, handlers, validators, and module registration
  - [[./Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs]] - create - Command handler implementation
  - [[./Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs]] - create - Transport correctness validator
  - [[./Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs]] - create - Module DI self-registration extension
- [[./Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Wire module registrations in the composition root

# Rules

MUST:
- `ICommand` and `ICommand<TResponse>` defined in Shared — not BuildingBlocks, not any module
- All commands implement `ICommand<Result<T>>` — not `IRequest<T>` directly
- Commands declared as `record` in `/{Module}.Interfaces/Commands`
- Result records declared in the same file as their command
- One handler per command — `IRequestHandler<TCommand, Result<T>>`
- Handler structure: load → guard → domain call → stage → return result
- All entity loading in handlers uses named specs from [[repository-integration.solution.skill]]
- Handlers inject `IRepository<T>` from Shared — never `DbContext`
- Cross-module writes dispatched via `_mediator.Send()` — never direct calls
- Each module has `Register{ModuleName}Module()` extension method
- Handlers and validators registered via assembly scan — never manually
- One `AbstractValidator<TCommand>` per command — co-located with handler in feature folder
- Validator file named `{FeatureName}.Validator.cs`, class named `{FeatureName}Validator`
- Validator extends `AbstractValidator<TCommand>`
- Validators registered via `AddValidatorsFromAssembly` in module registration
- No validator for query handlers

MUST NOT:
- Handler contain business logic — delegate to domain
- Handler call `SaveChangesAsync` — Unit of Work owns commit
- Handler reference another module's Domain or Application directly
- Command properties reference domain entity types
- `ICommand` defined in BuildingBlocks — belongs in Shared
- Validator contain business rules — transport correctness only
- Validator inject repositories or services — purely declarative
- Validator be shared across multiple commands
- Pipeline behaviors registered inside any module's registration method

SHOULD:
- Guard checks return early before domain call — fail fast pattern
- Handler follow the exact load → guard → domain call → stage → return sequence
- Validator rules cover all command properties that carry input constraints
- Use the transport validation boundary table to decide what belongs in validator vs handler vs domain

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

# Check list
- [ ] `ICommand` and `ICommand<TResponse>` defined in `Shared/MediatR/ICommand.cs`
- [ ] All commands declared as `record` in `/{Module}.Interfaces/Commands`
- [ ] All commands implement `ICommand<Result<T>>`
- [ ] Result records co-located with their command in the same file
- [ ] Each feature has its own folder under `/{Module}.Application/Features`
- [ ] Handler file named `{FeatureName}.Handler.cs`
- [ ] Handler class named `{FeatureName}Handler`
- [ ] Handler implements `IRequestHandler<TCommand, Result<T>>`
- [ ] Handler injects `IRepository<T>` — never `DbContext`
- [ ] Handler loads entities via named specs — no inline LINQ
- [ ] Handler follows load → guard → domain call → stage → return structure
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

# Unittest TestCases
- [ ] When valid command is handled Then handler returns expected `Result.Created` or `Result.Success`
- [ ] When required entity not found during load Then handler returns `Result.NotFound`
- [ ] When business guard condition is met Then handler returns `Result.Conflict` before domain call
- [ ] When domain call completes Then entity is staged in repository — not yet persisted
- [ ] When sub-command fails Then root handler returns `Result.Error` without staging own entity
- [ ] When two handlers in same module Then both are discovered by assembly scan
- [ ] When command is dispatched from API Then correct handler is invoked by MediatR

