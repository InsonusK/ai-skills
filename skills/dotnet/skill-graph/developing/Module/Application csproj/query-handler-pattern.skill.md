---
uid: b3a0a366-b45a-4694-b1e1-f700fc61c7ca
status: draft
name: query-handler-pattern
description: rules for implementing MediatR query handlers in the application layer
domain: skill
type: pattern
tags:
  - dotnet
  - application
  - cqrs
  - mediatr
  - query
  - handler
triggers:
  - implement query handler
  - create query
  - write query handler
  - handle read operation
  - get entity
  - list entities
aliases:
  - QueryHandler
  - Query Handler
  - IQuery
---
# Goal
Define how to implement MediatR query handlers in the Application layer and App.Queries. A query handler is a read-only operation — it fetches and projects data, never modifies state. Single-module queries live in Application, cross-module JOIN queries live in App.Queries. Without this pattern, read logic mixes with write logic, cross-module reads bypass boundaries, and projection concerns scatter across layers.

# Core Principles
- Query handlers are strictly read-only — no entity mutation, no SaveChanges
- Single-module queries live in `{Module}.Application` — served by one module's data
- Cross-module JOIN queries live in `App.Queries` — require data from multiple modules
- Query declared in `{Module}.Interfaces` — consumed by API, implemented elsewhere
- `IReadRepository<T>` used in all query handlers — never `IRepository<T>`
- `UnitOfWorkBehavior` does not activate for queries — `IQuery` marker excludes them
- Simple queries project via `Specification<T, TResult>` — complex queries map in handler

# Query Types

| Type                | Declared in                   | Implemented in         | When                         |
| ------------------- | ----------------------------- | ---------------------- | ---------------------------- |
| Single-module query | `{Module}.Interfaces/Queries` | `{Module}.Application` | Data from one module only    |
| Cross-module query  | `{Module}.Interfaces/Queries` | `App.Queries`          | Requires JOIN across modules |

# Structure / Contracts

## IQuery marker interface — BuildingBlocks

Marks queries so `UnitOfWorkBehavior` does not activate.

```csharp
// BuildingBlocks/MediatR/IQuery.cs
public interface IQuery<TResponse> : IRequest<TResponse> { }
```

## Query — {Module}.Interfaces/Queries

Declared in Interfaces — consumed by API layer and any module that needs this data.

```csharp
// Task.Interfaces/Queries/GetTaskQuery.cs
public record GetTaskQuery(int Id) : IQuery<Result<TaskDto>>;

// Task.Interfaces/Queries/GetTasksQuery.cs
public record GetTasksQuery(int AssigneeId) : IQuery<Result<IReadOnlyList<TaskSummaryDto>>>;
```

## DTOs — {Module}.Interfaces/DTOs

Response shapes declared in Interfaces alongside the query.

```csharp
// Task.Interfaces/DTOs/TaskDto.cs
public record TaskDto(int Id, string Title, string Status, int AssigneeId);

// Task.Interfaces/DTOs/TaskSummaryDto.cs
public record TaskSummaryDto(int Id, string Title, string Status);
```

## Single-module handler — {Module}.Application/Features/{FeatureName}

### Simple — projection via spec

Use when DTO maps directly from entity fields.

```csharp
// Task.Application/Features/GetTasks/GetTasksHandler.cs
public class GetTasksHandler : IRequestHandler<GetTasksQuery, Result<IReadOnlyList<TaskSummaryDto>>>
{
    private readonly IReadRepository<TodoTask> _repository;

    public GetTasksHandler(IReadRepository<TodoTask> repository)
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

### Complex — load entity then map in handler

Use when DTO requires computed fields, nested mapping, or conditional logic.

```csharp
// Task.Application/Features/GetTask/GetTaskHandler.cs
public class GetTaskHandler : IRequestHandler<GetTaskQuery, Result<TaskDto>>
{
    private readonly IReadRepository<TodoTask> _repository;

    public GetTaskHandler(IReadRepository<TodoTask> repository)
        => _repository = repository;

    public async Task<Result<TaskDto>> Handle(
        GetTaskQuery query, CancellationToken ct)
    {
        var task = await _repository.FirstOrDefaultAsync(
            new TaskByIdSpec(query.Id), ct);

        if (task is null)
            return Result.NotFound();

        // map in handler when projection logic is non-trivial
        var dto = new TaskDto(
            task.Id,
            task.Title,
            task.Status.ToString(),
            task.AssigneeId);

        return Result.Success(dto);
    }
}
```

## Cross-module handler — App.Queries/Queries/{ModuleName}

Use when query requires data from multiple modules — JOINs across module boundaries. Has direct DbContext access via App.Infrastructure.

```csharp
// App.Queries/Queries/Task/GetTaskWithUserDetailsHandler.cs
public class GetTaskWithUserDetailsHandler
    : IRequestHandler<GetTaskWithUserDetailsQuery, Result<TaskWithUserDetailsDto>>
{
    private readonly AppDbContext _dbContext;

    public GetTaskWithUserDetailsHandler(AppDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<TaskWithUserDetailsDto>> Handle(
        GetTaskWithUserDetailsQuery query, CancellationToken ct)
    {
        var result = await _dbContext.Set<TodoTask>()
            .Where(t => t.Id == query.TaskId)
            .Select(t => new TaskWithUserDetailsDto(
                t.Id,
                t.Title,
                t.Assignee.FullName,   // JOIN to User module data
                t.Assignee.Email))
            .FirstOrDefaultAsync(ct);

        return result is null
            ? Result.NotFound()
            : Result.Success(result);
    }
}
```

## File structure

```
/{Module}.Application
    /Features
        /GetTask
            GetTaskHandler.cs
        /GetTasks
            GetTasksHandler.cs

/App.Queries
    /Queries
        /Task
            GetTaskWithUserDetailsHandler.cs
```

# Rules

MUST:
- Query implements `IQuery<Result<T>>` — excludes from `UnitOfWorkBehavior`
- Query declared in `{Module}.Interfaces/Queries`
- DTOs declared in `{Module}.Interfaces/DTOs`
- Single-module handler uses `IReadRepository<T>` — never `IRepository<T>` or DbContext
- Cross-module handler lives in `App.Queries` — has DbContext access
- Handler returns `Result<T>` — `Result.NotFound()` when entity missing
- Query handler never modifies state 
MUST NOT:
- Query handler call `SaveChangesAsync` or modify entities
- Single-module handler use DbContext directly — use `IReadRepository<T>`
- Cross-module handler live in `{Module}.Application` — belongs in `App.Queries`
- DTO expose domain entity internals — projection only
- Query handler dispatch commands

# Anti-patterns
- Query handler modifies entity state — use command instead
- Single-module query implemented in `App.Queries` — unnecessary cross-module machinery
- Cross-module JOIN in `{Module}.Application` — Application has no cross-module DB access
- `IRepository<T>` used in query handler — signals write intent, use `IReadRepository<T>`
- Raw LINQ in single-module handler — use specification instead
- DTO returns domain entity directly — always project to DTO

# Checklist
- [ ] Query implements `IQuery<Result<T>>`
- [ ] Query declared in `{Module}.Interfaces/Queries`
- [ ] DTO declared in `{Module}.Interfaces/DTOs`
- [ ] Single-module handler in `{Module}.Application/Features`
- [ ] Single-module handler uses `IReadRepository<T>`
- [ ] Cross-module handler in `App.Queries/Queries/{ModuleName}`
- [ ] Cross-module handler uses DbContext directly
- [ ] Handler returns `Result.NotFound()` when entity not found
- [ ] No state mutation in any query handler

# Unittest TestCases
- [ ] When entity exists Then handler returns Result.Success with correct DTO
- [ ] When entity not found Then handler returns Result.NotFound
- [ ] When query runs Then UnitOfWorkBehavior does not activate
- [ ] When cross-module query runs Then data from both modules is correctly joined
- [ ] When projection spec used Then DTO fields are correctly mapped

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/repository-pattern.skill]] — IReadRepository used in single-module handlers
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/ardalis-specification-pattern.skill]] — specs used for filtering and projection
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/Solutions/command-handling.solution.skill]] — counterpart for write operations
- [[skills/dotnet/skill-graph/developing/API Layer/api-structure.skill]] — Result status mapped to HTTP responses in controller
- [[skills/dotnet/skill-graph/developing/Architecture/cross-module-interaction.skill]] — cross-module queries declared in Interfaces, implemented in App.Queries
- [[skills/dotnet/skill-graph/developing/Architecture/backend-project-structure.skill]] — placement rules for Application vs App.Queries