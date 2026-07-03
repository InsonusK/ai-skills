---
description: Command handler implementation
project_name: "{Module}.Application"
name: "{FeatureName}.Handler.cs"
element_kind: class
change_kind: create
---

# Goals
- Orchestrate one write operation: load required data via specs, guard against business failures, delegate to domain, stage changes, return a typed result
- Never contain business rules — always delegate decisions to the domain layer

# Core Principles
- Implements `IRequestHandler<TCommand, Result<T>>`
- Injects `IRepository<T>` for entity loading and staging — never `DbContext`
- Follows fixed structure: load → guard → domain call → stage → return result
- All entity loading uses named specs — no inline LINQ
- Returns `Ardalis.Result<T>` for all outcomes — no exceptions for flow control
- Cross-module writes dispatched via `_mediator.Send(new OtherModuleCommand(...))` — never direct calls

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Command handler | `{FeatureName}Handler` | `CreateTaskHandler` | `{FeatureName}.Handler.cs` | `CreateTask.Handler.cs` |

# Implementation changes

Handler follows the load → guard → domain call → stage → return structure:

```csharp
// {Module}.Application/Features/CreateTask/CreateTask.Handler.cs
using Ardalis.Result;
using MediatR;
using Shared.Repositories;

namespace {Module}.Application.Features.CreateTask;

public class CreateTaskHandler
    : IRequestHandler<CreateTaskCommand, Result<CreateTaskResult>>
{
    private readonly IRepository<TodoTask> _repository;

    public CreateTaskHandler(IRepository<TodoTask> repository)
        => _repository = repository;

    public async Task<Result<CreateTaskResult>> Handle(
        CreateTaskCommand command, CancellationToken ct)
    {
        var assignee = await _repository.FirstOrDefaultAsync(
            new UserByIdSpec(command.AssigneeId), ct);

        if (assignee is null)
            return Result.NotFound();

        var task = TodoTask.Create(command.Title, command.AssigneeId);

        await _repository.AddAsync(task, ct);

        return Result.Created(new CreateTaskResult(task.Id));
    }
}
```

Handler that dispatches a cross-module sub-command:

```csharp
// {Module}.Application/Features/CreateOrder/CreateOrder.Handler.cs
using Ardalis.Result;
using MediatR;
using Shared.Repositories;

namespace {Module}.Application.Features.CreateOrder;

public class CreateOrderHandler
    : IRequestHandler<CreateOrderCommand, Result<CreateOrderResult>>
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
        var bookResult = await _mediator.Send(
            new BookItemCommand(command.ProductId, command.Quantity), ct);

        if (!bookResult.IsSuccess)
            return Result.Error("Failed to book supply.");

        var order = Order.Create(command.ProductId, command.Quantity);
        await _repository.AddAsync(order, ct);

        return Result.Created(new CreateOrderResult(order.Id));
    }
}
```

## Result status conventions

| Result | Meaning | Typical use |
| --- | --- | --- |
| `Result.Created(value)` | Entity created successfully | After `AddAsync` on new entity |
| `Result.Success()` / `Result.Success(value)` | Operation succeeded | After updating existing entity |
| `Result.NoContent()` | Success with no return body | After delete |
| `Result.NotFound()` | Required entity does not exist | Guard after load returns null |
| `Result.Conflict(msg)` | Business state prevents the operation | Guard for business rule violation |
| `Result.Error(msg)` | Unexpected failure | Sub-command failure, unrecoverable state |

# Rule changes

## MUST
- Implement `IRequestHandler<TCommand, Result<T>>`
- Inject `IRepository<T>` — never `DbContext`
- Load entities via named specs — never inline LINQ
- Follow load → guard → domain call → stage → return structure
- Return `Result<T>` for all outcomes — never throw for flow control
- Dispatch cross-module writes via `_mediator.Send()` — never direct method calls
- Handler structure: load → guard → domain call → stage → return result
- All entity loading in handlers uses named specs from [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration.skill]]
- Cross-module writes dispatched via `_mediator.Send()` — never direct calls
- Handlers and validators registered via assembly scan — never manually
- No validator for query handlers

## MUST NOT
- Contain business logic or domain rules — delegate to entity or domain service
- Reference another module's Domain or Application projects directly
- Use inline LINQ — all queries go through named specs
- Handler contain business logic — delegate to domain
- Handler call `SaveChangesAsync` — Unit of Work owns commit
- Handler reference another module's Domain or Application directly
## SHOULD
- Handler follow the exact load → guard → domain call → stage → return sequence
- Use the transport validation boundary table to decide what belongs in validator vs handler vs domain
- Guard checks return early before domain call — fail fast pattern

# Unittest TestCases
- [ ] WHEN applied THEN Orchestrate one write operation: load required data via specs, guard against business failures, delegate to domain, stage changes, return a typed result
- [ ] WHEN applied THEN Never contain business rules — always delegate decisions to the domain layer
- [ ] WHEN applied THEN Implements IRequestHandler<TCommand, Result<T>>
- [ ] WHEN applied THEN Injects IRepository<T> for entity loading and staging — never DbContext
- [ ] WHEN applied THEN Follows fixed structure: load → guard → domain call → stage → return result
- [ ] WHEN applied THEN All entity loading uses named specs — no inline LINQ
- [ ] WHEN applied THEN Returns Ardalis.Result<T> for all outcomes — no exceptions for flow control
- [ ] WHEN applied THEN Cross-module writes dispatched via _mediator.Send(new OtherModuleCommand(...)) — never direct calls
- [ ] WHEN naming 'Command handler' THEN pattern matches convention
