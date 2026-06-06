---
name: feature-query-handler
description: rules for implementing single-module MediatR query handlers in the Application layer
domain: skill
type: pattern
tags:
  - dotnet
  - application
  - cqrs
  - mediatr
  - query
triggers:
  - implement query handler
  - write query handler
  - get entity
  - list entities
---
# Goal
Define how to implement a query handler inside `{ModuleName}.Application`. A query handler is strictly read-only — it fetches and projects data, never modifies state. This skill covers single-module queries only. Cross-module JOIN queries live in `App.Queries` — see app-queries.skill.

# Core Principles
- Query handlers are strictly read-only — no entity mutation, no SaveChanges
- `IReadRepository<T>` used always — never `IRepository<T>` or DbContext
- `UnitOfWorkBehavior` does not activate — `IQuery<T>` marker excludes queries from it
- Query and DTOs declared in `{ModuleName}.Interfaces` — handler implemented here
- Simple projection via spec; complex mapping done in handler

# File Location
```
/{ModuleName}.Application
  /Features
    /GetTask
      GetTask.Handler.cs     ← no validator file
    /GetTasks
      GetTasks.Handler.cs
```

# Query and DTO Declaration
Queries and DTOs live in `{ModuleName}.Interfaces` — not in Application.

```csharp
// Task.Interfaces/Queries/GetTaskQuery.cs
public record GetTaskQuery(int Id) : IQuery<Result<TaskDto>>;

// Task.Interfaces/DTOs/TaskDto.cs
public record TaskDto(int Id, string Title, string Status, int AssigneeId);
```

# Handler — Simple Projection via Spec
Use when DTO maps directly from entity fields. Spec handles the projection.

```csharp
// Task.Application/Features/GetTasks/GetTasks.Handler.cs
public class GetTasksHandler : IRequestHandler<GetTasksQuery, Result<IReadOnlyList<TaskSummaryDto>>>
{
    private readonly IReadRepository<Task> _repository;

    public GetTasksHandler(IReadRepository<Task> repository)
        => _repository = repository;

    public async Task<Result<IReadOnlyList<TaskSummaryDto>>> Handle(
        GetTasksQuery query, CancellationToken ct)
    {
        var results = await _repository.ListAsync(
            new TaskSummarySpec(query.AssigneeId), ct);

        return Result.Success(results as IReadOnlyList<TaskSummaryDto>);
    }
}
```

# Handler — Complex Mapping in Handler
Use when DTO requires computed fields, nested mapping, or conditional logic that cannot go in a spec.

```csharp
// Task.Application/Features/GetTask/GetTask.Handler.cs
public class GetTaskHandler : IRequestHandler<GetTaskQuery, Result<TaskDto>>
{
    private readonly IReadRepository<Task> _repository;

    public GetTaskHandler(IReadRepository<Task> repository)
        => _repository = repository;

    public async Task<Result<TaskDto>> Handle(
        GetTaskQuery query, CancellationToken ct)
    {
        var task = await _repository.FirstOrDefaultAsync(
            new TaskByIdSpec(query.Id), ct);

        if (task is null)
            return Result.NotFound();

        var dto = new TaskDto(
            task.Id,
            task.Title,
            task.Status.ToString(),
            task.AssigneeId);

        return Result.Success(dto);
    }
}
```

# Rules
MUST:
- Query implements `IQuery<Result<T>>`
- Handler injects `IReadRepository<T>` — never `IRepository<T>` or DbContext
- Handler returns `Result.NotFound()` when entity is missing
- Handler never modifies entity state
MUST NOT:
- Call `SaveChangesAsync` or modify entities
- Dispatch commands
- Use raw LINQ inline — use specifications
- Handle cross-module JOIN data — that belongs in App.Queries

# Anti-patterns
- `IRepository<T>` injected in query handler — signals write intent, use `IReadRepository<T>`
- Raw `Where(...)` LINQ in handler — use a named specification
- Query handler that also writes state — split into command + query

# Checklist
- [ ] Query implements `IQuery<Result<T>>`
- [ ] No validator file alongside query handler
- [ ] Handler injects `IReadRepository<T>`
- [ ] Returns `Result.NotFound()` when entity missing
- [ ] No state mutation

# Unittest TestCases
- [ ] When entity exists Then handler returns Result.Success with correct DTO
- [ ] When entity not found Then handler returns Result.NotFound
- [ ] When query runs Then UnitOfWorkBehavior does not activate

# Relations
- ardalis-specification.skill — specs used for filtering and projection
- repository.skill — IReadRepository contract used here
- feature-command-handler.skill — counterpart for write operations
- app-queries.skill — cross-module JOIN queries belong there, not here
- api-controller.skill — maps Result<T> to HTTP response
