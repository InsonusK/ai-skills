---
name: repository
description: rules for using IRepository and IReadRepository abstractions in Application layer handlers
domain: skill
type: pattern
tags:
  - dotnet
  - application
  - repository
  - unit-of-work
triggers:
  - use repository in handler
  - IRepository usage
  - IReadRepository usage
  - data access in handler
---
# Goal
Define how Application layer handlers use repository abstractions to load and stage data. Repositories decouple handlers from DbContext — handlers never touch EF directly. `IUnitOfWork` commits all staged changes atomically after the handler via pipeline behavior. This skill covers usage only. Interface and implementation definitions live in BuildingBlocks and App.Infrastructure.

# Core Principles
- `IRepository<T>` used in command handlers — stages writes, never calls SaveChanges
- `IReadRepository<T>` used in query handlers — read-only, signals no write intent
- Handler never calls `SaveChangesAsync` — `UnitOfWorkBehavior` commits after handler returns
- Only the top-level command commits — sub-commands dispatched from handlers defer to the root
- All queries use Ardalis Specifications — no raw LINQ in handlers

# Interface Contracts
Defined in BuildingBlocks — handlers depend on these abstractions only.

```csharp
// Read-only — used in query handlers and idempotency checks
public interface IReadRepository<T> where T : class
{
    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> spec, CancellationToken ct = default);
    Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec, CancellationToken ct = default);
    Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default);
}

// Write access — used in command handlers to stage changes
public interface IRepository<T> : IReadRepository<T> where T : class
{
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}
```

# Usage in Command Handler
```csharp
public class AssignTaskHandler : IRequestHandler<AssignTaskCommand, Result>
{
    private readonly IRepository<Task> _repository;

    public AssignTaskHandler(IRepository<Task> repository)
        => _repository = repository;

    public async Task<Result> Handle(AssignTaskCommand command, CancellationToken ct)
    {
        var task = await _repository.FirstOrDefaultAsync(
            new TaskByIdSpec(command.TaskId), ct);

        if (task is null)
            return Result.NotFound();

        task.Assign(command.AssigneeId);  // domain call — EF tracks the change

        // no SaveChangesAsync — UnitOfWorkBehavior commits after this returns
        return Result.Success();
    }
}
```

# Usage in Query Handler
```csharp
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

# Rules
MUST:
- Command handlers inject `IRepository<T>`
- Query handlers inject `IReadRepository<T>`
- All queries use Ardalis Specifications — no inline LINQ
- Never call `SaveChangesAsync` in any handler
MUST NOT:
- Reference DbContext directly in any Application class
- Use `IRepository<T>` in query handlers — use `IReadRepository<T>`
- Call `SaveChangesAsync` — this belongs to `UnitOfWorkBehavior`

# Anti-patterns
- Handler calls `_repository.SaveChangesAsync()` — UnitOfWorkBehavior owns this
- `IRepository<T>` injected in query handler — use `IReadRepository<T>` to signal read intent
- Raw LINQ in handler — use specifications

# Checklist
- [ ] Command handlers inject `IRepository<T>`
- [ ] Query handlers inject `IReadRepository<T>`
- [ ] No `SaveChangesAsync` in any handler
- [ ] No DbContext reference in Application
- [ ] All data access via named specifications

# Unittest TestCases
- [ ] When top-level command runs Then SaveChanges called exactly once
- [ ] When command dispatches sub-command Then SaveChanges called once after root completes
- [ ] When handler throws Then SaveChanges not called
- [ ] When query handler runs Then UnitOfWorkBehavior does not activate

# Relations
- feature-command-handler.skill — command handlers use IRepository<T>
- feature-query-handler.skill — query handlers use IReadRepository<T>
- ardalis-specification.skill — all queries passed to repository use specifications
- unit-of-work.skill — UnitOfWorkBehavior and IUnitOfWork implementation detail
- command-handling.solution.skill — full pipeline including UnitOfWorkBehavior
