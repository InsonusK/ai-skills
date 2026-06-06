---
uid: 712517f4-c36b-40b6-b282-020f581f376b
status: draft
name: command-handler-pattern
description: rules for implementing MediatR command handlers in the application layer
domain: skill
type: pattern
tags:
  - dotnet
  - application
  - cqrs
  - mediatr
  - command
  - handler
  - validation
triggers:
  - implement command handler
  - create command
  - write command handler
  - handle write operation
aliases:
  - CommandHandler
  - Command Handler
  - ICommand
---
# Goal
Define how to implement MediatR command handlers in the Application layer. A command handler is the orchestration point for a write operation — it loads data, calls domain logic, and returns a result. It never contains business logic itself. Validation runs before the handler via a pipeline behavior — the handler only receives valid commands. Without this pattern, business logic leaks into handlers, validation scatters across layers, and write operations lose predictable structure.

# Core Principles
- Handler orchestrates — it never contains business rules
- Domain layer decides — handler loads data and delegates decisions to entities and domain services
- Validation runs in pipeline before handler — handler receives only valid input
- `UnitOfWorkBehavior` commits after handler — handler never calls SaveChanges
- Sub-commands dispatched via `_mediator.Send()` — cross-module writes go through MediatR
- Handler returns `Ardalis.Result<T>` — all outcomes expressed as typed results, no exceptions for flow control
- One handler per command — no shared handlers

# Pipeline Order
```
HTTP Request
    ↓
ValidationBehavior — validates command, returns Result.Invalid if invalid
    ↓
UnitOfWorkBehavior — starts depth tracking, commits on root completion
    ↓
Handler — loads data, calls domain, dispatches sub-commands if needed
    ↓
UnitOfWorkBehavior — SaveChanges if depth == 1
    ↓
HTTP Response
```

# Structure / Contracts

## Command — {Module}.Interfaces/Commands

Commands implement `ICommand<TResponse>` to activate `UnitOfWorkBehavior`.

```csharp
// Task.Interfaces/Commands/CreateTaskCommand.cs
public record CreateTaskCommand(
    Guid Guid,
    string Title,
    int AssigneeId
) : ICommand<Result<CreateTaskResult>>;

public record CreateTaskResult(int Id);
```

## Validator — {Module}.Application/Features/{FeatureName}

One validator per command. Lives next to the handler.

```csharp
// Task.Application/Features/CreateTask/CreateTaskCommandValidator.cs
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Guid).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AssigneeId).GreaterThan(0);
    }
}
```

## ValidationBehavior — BuildingBlocks

Runs before every command handler. Returns `Result.Invalid` if validation fails. Handler never executes for invalid input.

```csharp
// BuildingBlocks/MediatR/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand
    where TResponse : IResult
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any())
            return await next();

        var errors = _validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(e => e != null)
            .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
            .ToList();

        if (errors.Any())
            return (TResponse)Result.Invalid(errors);

        return await next();
    }
}
```

## Handler — {Module}.Application/Features/{FeatureName}

Standard structure: load → guard → domain call → return result.

```csharp
// Task.Application/Features/CreateTask/CreateTaskHandler.cs
public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, Result<CreateTaskResult>>
{
    private readonly IRepository<TodoTask> _repository;

    public CreateTaskHandler(IRepository<TodoTask> repository)
        => _repository = repository;

    public async Task<Result<CreateTaskResult>> Handle(
        CreateTaskCommand command, CancellationToken ct)
    {
        // load — fetch required data via specifications
        var existing = await _repository.AnyAsync(
            new TaskByGuidSpec(command.Guid), ct);

        // guard — return early for business-level failures
        if (existing)
            return Result.Conflict("Task with this Guid already exists.");

        // domain call — entity or domain service makes the decision
        var task = TodoTask.Create(command.Guid, command.Title, command.AssigneeId);

        // stage — repository tracks the change, UnitOfWorkBehavior commits
        await _repository.AddAsync(task, ct);

        return Result.Created(new CreateTaskResult(task.Id));
    }
}
```

## Handler with sub-command — cross-module write

Sub-command dispatched via MediatR. `UnitOfWorkContext` depth counter ensures the sub-command does not commit prematurely — root command commits everything.

```csharp
// Order.Application/Features/CreateOrder/CreateOrderHandler.cs
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderResult>>
{
    private readonly IRepository<Order> _repository;
    private readonly IMediator _mediator;

    public CreateOrderHandler(IRepository<Order> repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task<Result<CreateOrderResult>> Handle(
        CreateOrderCommand command, CancellationToken ct)
    {
        // cross-module write — dispatched via MediatR, not called directly
        var bookResult = await _mediator.Send(
            new BookItemCommand(command.ProductId, command.Quantity), ct);

        if (!bookResult.IsSuccess)
            return Result.Error("Failed to book supply.");

        // own domain call after sub-command succeeds
        var order = Order.Create(command.Guid, command.ProductId, command.Quantity);
        await _repository.AddAsync(order, ct);

        // UnitOfWorkBehavior commits both order and supply changes atomically
        return Result.Created(new CreateOrderResult(order.Id));
    }
}
```

## File structure — {Module}.Application

```
/{Module}.Application
    /Features
        /CreateTask
            CreateTaskHandler.cs
            CreateTaskCommandValidator.cs
        /AssignTask
            AssignTaskHandler.cs
            AssignTaskCommandValidator.cs
```

## Result status mapping to HTTP

See [[skills/dotnet/skill-graph/developing/API Layer/api-structure.skill]] for full HTTP mapping. Standard conventions:

|Result status|Meaning|
|---|---|
|`Result.Created(value)`|Entity created — 201|
|`Result.Success(value)`|Operation succeeded — 200|
|`Result.NoContent()`|Operation succeeded, no body — 204|
|`Result.NotFound()`|Entity not found — 404|
|`Result.Conflict(msg)`|Business conflict — 409|
|`Result.Invalid(errors)`|Validation failed — 400|
|`Result.Error(msg)`|Unexpected failure — 500|

# Rules
MUST:
- Command implements `ICommand<Result<T>>` — activates `UnitOfWorkBehavior`
- One validator per command — lives in same feature folder as handler
- Handler never calls `SaveChangesAsync` — `UnitOfWorkBehavior` commits
- Handler never contains business rules — domain layer decides
- Handler returns `Result<T>` — no exceptions for flow control
- Sub-commands dispatched via `_mediator.Send()` — never called directly
- Sub-commands must be safe inside an existing unit of work — see [[skills/dotnet/skill-graph/developing/Module/Application Layer/repository-pattern.skill]] 
SHOULD:
- Handler follow load → guard → domain call → stage → return structure
- Guard checks return early with typed result before domain call 
MUST NOT:
- Handler reference DbContext or call SaveChanges
- Handler contain domain logic — delegate to entity or domain service
- Handler reference other module's Domain or Application directly
- Validator contain business rules — transport correctness only

# Anti-patterns
- Business logic in handler: `if (task.AssigneeId == command.AssigneeId) return Result.Conflict(...)` — domain rule belongs in entity
- Validation inside handler — use `ValidationBehavior` pipeline instead
- Handler catches and swallows exceptions — let pipeline handle failures
- One handler dispatches multiple top-level commands sequentially — design as orchestrating command instead
- Cross-module write via direct method call — always use `_mediator.Send()`

# Checklist
- [ ] Command implements `ICommand<Result<T>>`
- [ ] Validator defined in same feature folder
- [ ] `ValidationBehavior` registered in MediatR pipeline before `UnitOfWorkBehavior`
- [ ] Handler follows load → guard → domain → stage → return structure
- [ ] No `SaveChangesAsync` in handler
- [ ] No business logic in handler
- [ ] Sub-commands dispatched via `_mediator.Send()`
- [ ] All result statuses mapped to HTTP responses in API layer

# Unittest TestCases
- [ ] When valid command Then handler executes and returns expected Result
- [ ] When invalid command Then ValidationBehavior returns Result.Invalid before handler runs
- [ ] When entity not found Then handler returns Result.NotFound
- [ ] When business conflict Then handler returns Result.Conflict
- [ ] When sub-command fails Then root command returns error without committing
- [ ] When handler throws Then UnitOfWorkBehavior does not commit

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Application Layer/repository-pattern.skill]] — IRepository, IUnitOfWork, UnitOfWorkBehavior
- [[skills/dotnet/skill-graph/developing/Module/Application Layer/ardalis-specification-pattern.skill]] — all entity loading uses specifications
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-event-pattern.skill]] — domain calls inside handler raise events collected by entity
- [[skills/dotnet/skill-graph/developing/API Layer/api-structure.skill]] — Result status mapped to HTTP responses in controller
- [[skills/dotnet/skill-graph/developing/Architecture/cross-module-interaction.skill]] — sub-commands are the cross-module write mechanism
- [[skills/dotnet/skill-graph/developing/Architecture/backend-project-structure.skill]] — handler and validator placement in Application/Features