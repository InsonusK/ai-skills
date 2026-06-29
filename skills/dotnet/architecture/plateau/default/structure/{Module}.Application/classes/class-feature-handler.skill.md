---
name: class-feature-handler
description: Single-module query handler implementation
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration.skill]]"
---

# Goal
- Fetch and project data for a single module's read operation
- Never modify state — return typed Result with DTO
- Orchestrate one write operation: load required data via specs, guard against business failures, delegate to domain, stage changes, return a typed result
- Never contain business rules — always delegate decisions to the domain layer

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]

# Core Principals
- Apply ONE plateau template per class
- Implements `IRequestHandler<TQuery, Result<T>>`
- Injects `IReadRepository<T>` from Shared — signals read-only intent at type level
- Two implementation shapes depending on DTO complexity:
  - **Projection via spec** — when DTO maps directly from entity fields, use `Specification<T, TDto>` and `ListAsync`
  - **Load then map in handler** — when DTO requires computed fields, conditional logic, or nested mapping
- All entity loading uses named specs — no inline LINQ
- Returns `Result.NotFound()` when entity is missing — never returns null or empty DTO
- Implements `IRequestHandler<TCommand, Result<T>>`
- Injects `IRepository<T>` for entity loading and staging — never `DbContext`
- Follows fixed structure: load → guard → domain call → stage → return result
- Returns `Ardalis.Result<T>` for all outcomes — no exceptions for flow control
- Cross-module writes dispatched via `_mediator.Send(new OtherModuleCommand(...))` — never direct calls

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Query handler | `{FeatureName}Handler` | `GetTaskHandler` | `{FeatureName}.Handler.cs` | `GetTask.Handler.cs` |
| Command handler | `{FeatureName}Handler` | `CreateTaskHandler` | `{FeatureName}.Handler.cs` | `CreateTask.Handler.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-feature-handler
//Plateau: default
//Version: 20260628
```

Simple — projection via spec (DTO maps directly from entity fields):

```csharp
// {Module}.Application/Queries/GetTasks/GetTasks.Handler.cs
using Ardalis.Result;
using MediatR;
using Shared.Repositories;

namespace {Module}.Application.Queries.GetTasks;

public class GetTasksHandler
    : IRequestHandler<GetTasksQuery, Result<IReadOnlyList<TaskSummaryDto>>>
{
    private readonly IReadRepository<TodoTask> _repository;

    public GetTasksHandler(IReadRepository<TodoTask> repository)
        => _repository = repository;

    public async Task<Result<IReadOnlyList<TaskSummaryDto>>> Handle(
        GetTasksQuery query, CancellationToken ct)
    {
        // projection spec — DTO built inside the spec, AsNoTracking applied by repository
        var results = await _repository.ListAsync(
            new TaskSummarySpec(query.AssigneeId), ct);

        return Result.Success<IReadOnlyList<TaskSummaryDto>>(results);
    }
}
```

Complex — load entity then map in handler (DTO requires non-trivial mapping):

```csharp
// {Module}.Application/Queries/GetTask/GetTask.Handler.cs
using Ardalis.Result;
using MediatR;
using Shared.Repositories;

namespace {Module}.Application.Queries.GetTask;

public class GetTaskHandler
    : IRequestHandler<GetTaskQuery, Result<TaskDto>>
{
    private readonly IReadRepository<TodoTask> _repository;

    public GetTaskHandler(IReadRepository<TodoTask> repository)
        => _repository = repository;

    public async Task<Result<TaskDto>> Handle(
        GetTaskQuery query, CancellationToken ct)
    {
        // load via named spec — never inline LINQ
        var task = await _repository.FirstOrDefaultAsync(
            new TaskByIdSpec(query.Id), ct);

        if (task is null)
            return Result.NotFound();

        // map in handler — DTO requires Status.ToString() conversion
        var dto = new TaskDto(
            task.Id,
            task.Title,
            task.Status.ToString(),
            task.AssigneeId);

        return Result.Success(dto);
    }
}
```

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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]

# Rules
MUST:
	- Implement `IRequestHandler<TQuery, Result<T>>`
	- Inject `IReadRepository<T>` — never `IRepository<T>` or DbContext
	- Load entities via named specs — never inline LINQ
	- Return `Result.NotFound()` when entity is missing
	- Return `Result<T>` for all outcomes — no exceptions for flow control
	- Implement `IRequestHandler<TCommand, Result<T>>`
	- Inject `IRepository<T>` — never `DbContext`
	- Follow load → guard → domain call → stage → return structure
	- Return `Result<T>` for all outcomes — never throw for flow control
	- Dispatch cross-module writes via `_mediator.Send()` — never direct method calls
MUST NOT:
	- Modify any entity state
	- Call `SaveChangesAsync` or inject `IUnitOfWork`
	- Dispatch commands
	- Use inline LINQ — all filtering goes through named specs
	- Contain business logic or domain rules — delegate to entity or domain service
	- Call `SaveChangesAsync`
	- Reference another module's Domain or Application projects directly
	- Use inline LINQ — all queries go through named specs

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- `IRepository<T>` injected into query handler — use `IReadRepository<T>`
- Inline LINQ in handler: `_repository.FirstOrDefaultAsync(x => x.Id == id)` — define `TaskByIdSpec` instead
- Returning null instead of `Result.NotFound()`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Fetch and project data for a single module's read operation
- [ ] WHEN applied THEN Never modify state — return typed Result with DTO
- [ ] WHEN applied THEN Implements IRequestHandler<TQuery, Result<T>>
- [ ] WHEN applied THEN Injects IReadRepository<T> from Shared — signals read-only intent at type level
- [ ] WHEN applied THEN Two implementation shapes depending on DTO complexity:
- [ ] WHEN applied THEN **Projection via spec** — when DTO maps directly from entity fields, use Specification<T, TDto> and ListAsync
- [ ] WHEN applied THEN **Load then map in handler** — when DTO requires computed fields, conditional logic, or nested mapping
- [ ] WHEN applied THEN All entity loading uses named specs — no inline LINQ
- [ ] WHEN applied THEN Returns Result.NotFound() when entity is missing — never returns null or empty DTO
- [ ] WHEN naming 'Query handler' THEN pattern matches convention
- [ ] WHEN applied THEN Orchestrate one write operation: load required data via specs, guard against business failures, delegate to domain, stage changes, return a typed result
- [ ] WHEN applied THEN Never contain business rules — always delegate decisions to the domain layer
- [ ] WHEN applied THEN Implements IRequestHandler<TCommand, Result<T>>
- [ ] WHEN applied THEN Injects IRepository<T> for entity loading and staging — never DbContext
- [ ] WHEN applied THEN Follows fixed structure: load → guard → domain call → stage → return result
- [ ] WHEN applied THEN Returns Ardalis.Result<T> for all outcomes — no exceptions for flow control
- [ ] WHEN applied THEN Cross-module writes dispatched via _mediator.Send(new OtherModuleCommand(...)) — never direct calls
- [ ] WHEN naming 'Command handler' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]
