---
description: Single-module query handler implementation
project_name: "{Module}.Application"
name: "{FeatureName}.Handler.cs"
change_kind: create
---

# Goals
- Fetch and project data for a single module's read operation
- Never modify state — return typed Result with DTO

# Core Principles
- Implements `IRequestHandler<TQuery, Result<T>>`
- Injects `IReadRepository<T>` from Shared — signals read-only intent at type level
- Two implementation shapes depending on DTO complexity:
  - **Projection via spec** — when DTO maps directly from entity fields, use `Specification<T, TDto>` and `ListAsync`
  - **Load then map in handler** — when DTO requires computed fields, conditional logic, or nested mapping
- All entity loading uses named specs — no inline LINQ
- Returns `Result.NotFound()` when entity is missing — never returns null or empty DTO

# When to use projection spec vs in-handler mapping

| Scenario | Shape | Example |
| --- | --- | --- |
| DTO maps 1:1 from entity fields | Projection spec `Specification<T, TDto>` | `TaskSummarySpec` → `ListAsync` |
| DTO needs `.ToString()`, enum label, or simple transform | In-handler mapping | load entity, map manually |
| DTO has nested structure or conditional fields | In-handler mapping | load entity, compose DTO |
| Collection with filter + ordering | Projection spec | `ActiveTasksByAssigneeSpec` → `ListAsync` |

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Query handler | `{FeatureName}Handler` | `GetTaskHandler` | `{FeatureName}.Handler.cs` | `GetTask.Handler.cs` |

# Implementation changes

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

# Result status conventions

| Result | Meaning | Typical use |
| --- | --- | --- |
| `Result.Success(value)` | Data returned successfully | All query outcomes with data |
| `Result.NotFound()` | Entity does not exist | When load returns null |

# Rules

MUST:
- Implement `IRequestHandler<TQuery, Result<T>>`
- Inject `IReadRepository<T>` — never `IRepository<T>` or DbContext
- Load entities via named specs — never inline LINQ
- Return `Result.NotFound()` when entity is missing
- Return `Result<T>` for all outcomes — no exceptions for flow control

MUST NOT:
- Modify any entity state
- Call `SaveChangesAsync` or inject `IUnitOfWork`
- Dispatch commands
- Use inline LINQ — all filtering goes through named specs

# Anti-patterns
- `IRepository<T>` injected into query handler — use `IReadRepository<T>`
- Inline LINQ in handler: `_repository.FirstOrDefaultAsync(x => x.Id == id)` — define `TaskByIdSpec` instead
- Returning null instead of `Result.NotFound()`
