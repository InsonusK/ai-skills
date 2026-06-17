---
name: feature-command-handler
description: rules for implementing MediatR command handlers in the Application layer
domain: skill
type: template
version: 20260607
tags:
  - dotnet
  - application
  - cqrs
  - mediatR
  - command
  - skill/template/class
triggers:
  - implement command handler
  - write command handler
  - handle write operation
---
# Goal
Define how to implement a command handler in `{ModuleName}.Application`. A command handler is the orchestration point for a write operation — it loads data, calls domain logic, and returns a typed result. It never contains business logic itself.

# Core Principles
- Handler orchestrates — domain layer decides
- Handler receives only valid input — `ValidationBehavior` runs first in pipeline
- Handler never calls `SaveChanges` — UnitOfWorkBehavior commits after handler returns
- Handler returns `Ardalis.Result<T>` — all outcomes as typed results, no exceptions for flow control
- One handler per command — no shared handlers
- Cross-module writes dispatched via `_mediator.Send()` — never direct method calls

# Governed by
	- [[skills/dotnet/skill-graph/developing/Module/Application Layer/Solutions/command-handling.solution.skill|command-handling.solution.skill]]

# File Location
```
/{ModuleName}.Application
  /Features
    /CreateTask
      CreateTask.Handler.cs
```

# Command Declaration
Commands live in [[skills/dotnet/skill-graph/developing/Module/Interfaces csproj/classes/module-interface-command.class.skill|{ModuleName}.Interfaces/Commands]] — not in Application.
![[skills/dotnet/skill-graph/developing/Module/Interfaces csproj/classes/module-interface-command.class.skill#Implementation|{ModuleName}.Interfaces/Commands]]

# Handler Structure
Standard flow: **load → guard → domain call → stage → return result**

```csharp
// Task.Application/Features/CreateTask/CreateTask.Handler.cs
public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, Result<CreateTaskResult>>
{
    private readonly IRepository<Task> _repository;

    public CreateTaskHandler(IRepository<Task> repository)
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
        var task = Task.Create(command.Guid, command.Title, command.AssigneeId);

        // stage — repository tracks the change, UnitOfWorkBehavior commits
        await _repository.AddAsync(task, ct);

        return Result.Created(new CreateTaskResult(task.Id));
    }
}
```

# Cross-module Write
Sub-command dispatched via `MediatR`. `UnitOfWorkContext` depth counter ensures sub-command does not commit prematurely.

```csharp
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderResult>>
{
    private readonly IRepository<Order> _repository;
    private readonly IMediator _mediator;

    public async Task<Result<CreateOrderResult>> Handle(
        CreateOrderCommand command, CancellationToken ct)
    {
        // cross-module write — MediatR dispatch, not direct method call
        var bookResult = await _mediator.Send(
            new BookItemCommand(command.ProductId, command.Quantity), ct);

        if (!bookResult.IsSuccess)
            return Result.Error("Failed to book supply.");

        var order = Order.Create(command.Guid, command.ProductId, command.Quantity);
        await _repository.AddAsync(order, ct);

        return Result.Created(new CreateOrderResult(order.Id));
    }
}
```

# Result Status Reference
![[skills/dotnet/skill-graph/developing/App/Host csproj/class/app-host-result-mapper.class.skill#Result status mapping|app-host-result-mapper.class.skill]]

HTTP mapping is the API layer's responsibility — see [[skills/dotnet/skill-graph/developing/App/Host csproj/class/app-host-result-mapper.class.skill|app-host-result-mapper.class.skill]].
# Rules
MUST:
- Command implements `ICommand<Result<T>>` — activates `UnitOfWorkBehavior`
- Handler follows load → guard → domain call → stage → return structure
- Handler injects `IRepository<T>` — never `DbContext`
- Handler returns `Result<T>` for every outcome
- Sub-commands dispatched via `_mediator.Send()`
MUST NOT:
- Call `SaveChangesAsync` — `UnitOfWorkBehavior` owns this
- Contain business rules — delegate to entity or domain service
- Reference other module's Domain or Application directly

# Anti-patterns
- Business logic in handler: `if (task.AssigneeId == command.AssigneeId) return Result.Conflict(...)` — belongs in entity
- Cross-module write via direct Application method call — use `_mediator.Send()`
- Handler catches and swallows exceptions — let pipeline handle failures

# Checklist
- [ ] Command implements `ICommand<Result<T>>`
- [ ] Handler file named `{FeatureName}.Handler.cs`
- [ ] Handler follows load → guard → domain → stage → return
- [ ] No `SaveChangesAsync` in handler
- [ ] No business logic in handler
- [ ] `IRepository<T>` injected — never DbContext

# Unittest TestCases
- [ ] When valid command Then handler returns expected Result
- [ ] When entity not found Then handler returns Result.NotFound
- [ ] When business conflict Then handler returns Result.Conflict
- [ ] When sub-command fails Then root command returns error without committing
- [ ] When handler throws Then UnitOfWorkBehavior does not commit

# Relations
- feature-validator.skill — validator runs before this handler via ValidationBehavior
- ardalis-specification.skill — all entity loading uses specifications
- repository.skill — IRepository used for staging changes
- command-handling.solution.skill — full pipeline flow this handler participates in
- api-controller.skill — maps Result<T> to HTTP responses
