---
uid: 62e22baa-a2ee-492e-9a02-1134c103ae62
status: draft
name: repository-pattern
description: rules for defining and using repository abstractions in the application layer
domain: skill
type: pattern
tags:
  - dotnet
  - application
  - repository
  - ardalis
  - unit-of-work
triggers:
  - repository design
  - data access abstraction
  - IRepository usage
  - IUnitOfWork usage
  - SaveChanges pattern
aliases:
  - IRepository
  - IReadRepository
  - IUnitOfWork
  - Repository
---
# Goal

Define the repository and unit of work abstractions used by Application layer handlers. Repositories stage changes against EF tracked entities without exposing DbContext. IUnitOfWork commits all staged changes atomically at the end of the handler via a MediatR pipeline behavior. Without this separation, handlers call SaveChanges at arbitrary points, making multi-repository atomicity impossible and coupling Application to infrastructure timing decisions.

# Core Principles

- Application layer never references DbContext — only repository and UnitOfWork abstractions
- `IRepository<T>` stages changes — it never calls SaveChanges
- `IReadRepository<T>` is read-only — no writes, no SaveChanges
- `IUnitOfWork` is the only place that calls SaveChanges — invoked by pipeline, not handler
- Only the top-level command commits — sub-commands dispatched from handlers defer to the root
- `UnitOfWorkContext` tracks nesting depth — prevents premature commit in nested command dispatch
- Ardalis.Specification powers all queries — no raw LINQ in handlers

# Interfaces

## IReadRepository — Shared or BuildingBlocks

Read-only access. Used in query handlers and idempotency checks.

```csharp
// BuildingBlocks/Repositories/IReadRepository.cs
public interface IReadRepository<T> where T : class
{
    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> spec, CancellationToken ct = default);
    Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec, CancellationToken ct = default);
    Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default);
}
```

## IRepository — BuildingBlocks

Write access. Used in command handlers to stage changes. SaveChanges is deliberately absent — only UnitOfWork commits.

```csharp
// BuildingBlocks/Repositories/IRepository.cs
public interface IRepository<T> : IReadRepository<T> where T : class
{
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
```

## IUnitOfWork — BuildingBlocks

Single responsibility: commit all staged changes to the database.

```csharp
// BuildingBlocks/UnitOfWork/IUnitOfWork.cs
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

## UnitOfWorkContext — BuildingBlocks

Scoped service tracking command nesting depth. Ensures only the top-level command commits — sub-commands defer to the root.

```csharp
// BuildingBlocks/MediatR/UnitOfWorkContext.cs
public class UnitOfWorkContext  // registered as Scoped — one per HTTP request
{
    public int Depth { get; set; }
}
```

## UnitOfWorkBehavior — BuildingBlocks

MediatR pipeline behavior. Only the outermost command calls SaveChanges. When a command dispatches a sub-command, the sub-command's behavior increments depth but defers the commit — the root command commits everything atomically.

```csharp
// BuildingBlocks/MediatR/UnitOfWorkBehavior.cs
public class UnitOfWorkBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand  // only activates for commands, not queries
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UnitOfWorkContext _context;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork, UnitOfWorkContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        _context.Depth++;
        try
        {
            var response = await next();

            if (_context.Depth == 1)
                await _unitOfWork.SaveChangesAsync(ct);

            return response;
        }
        finally
        {
            _context.Depth--;
        }
    }
}
```

## ICommand marker interface — BuildingBlocks

Marks commands so UnitOfWorkBehavior activates only for writes, not queries.

```csharp
// BuildingBlocks/MediatR/ICommand.cs
public interface ICommand : IRequest { }
public interface ICommand<TResponse> : IRequest<TResponse> { }
```

## Implementation — App.Infrastructure

Generic implementation wraps Ardalis EF repository base. One registration covers all entity types — no per-entity repository class needed.

```csharp
// App.Infrastructure/Repositories/Repository.cs
public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _dbContext;

    public Repository(AppDbContext dbContext)
        => _dbContext = dbContext;

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _dbContext.Set<T>().AddAsync(entity, ct);

    public void Update(T entity)
        => _dbContext.Set<T>().Update(entity);

    public void Remove(T entity)
        => _dbContext.Set<T>().Remove(entity);

    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default)
        => await ApplySpec(spec).FirstOrDefaultAsync(ct);

    public async Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct = default)
        => await ApplySpec(spec).AnyAsync(ct);

    // ... other methods

    private IQueryable<T> ApplySpec(ISpecification<T> spec)
        => SpecificationEvaluator.Default.GetQuery(_dbContext.Set<T>().AsQueryable(), spec);
}

// App.Infrastructure/UnitOfWork/UnitOfWork.cs
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
        => _dbContext = dbContext;

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _dbContext.SaveChangesAsync(ct);
}
```

## DI Registration — App.Host

```csharp
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
services.AddScoped(typeof(IReadRepository<>), typeof(Repository<>));
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<UnitOfWorkContext>();
```

## Usage in a command handler

```csharp
// Task.Application/Features/AssignTask/AssignTaskHandler.cs
public class AssignTaskHandler : IRequestHandler<AssignTaskCommand, Result>
{
    private readonly IRepository<TodoTask> _repository;

    public AssignTaskHandler(IRepository<TodoTask> repository)
        => _repository = repository;

    public async Task<Result> Handle(AssignTaskCommand command, CancellationToken ct)
    {
        var task = await _repository.FirstOrDefaultAsync(
            new TaskByIdSpec(command.TaskId), ct);

        if (task is null)
            return Result.NotFound();

        task.Assign(command.AssigneeId);   // domain call — stages change in EF tracker
        await _repository.AddAsync(task, ct); // only if new entity

        // no SaveChanges here — UnitOfWorkBehavior commits after this returns
        return Result.Success();
    }
}
```

# Rules

MUST:
- `IRepository<T>` and `IReadRepository<T>` defined in BuildingBlocks
- `IUnitOfWork` defined in BuildingBlocks
- `UnitOfWorkContext` registered as `Scoped` in DI
- `IRepository<T>` must NOT expose `SaveChangesAsync`
- `UnitOfWorkBehavior` activates only on `ICommand` — not on queries
- `UnitOfWorkBehavior` uses `UnitOfWorkContext` depth counter — only depth 1 commits
- Handler never calls `SaveChangesAsync` directly
- All queries use Ardalis Specifications — no raw LINQ in handlers
- Generic `Repository<T>` implementation registered for all entities
- Sub-commands must be safe to run inside an existing unit of work — they must not assume they are the top-level transaction owner 
MUST NOT:
- Handler reference DbContext directly
- Handler call `SaveChangesAsync` — this belongs to `UnitOfWorkBehavior`
- Query handler use `IRepository<T>` — use `IReadRepository<T>` instead
- Application layer reference `App.Infrastructure` implementations

# Anti-patterns
- Handler calls `_repository.SaveChangesAsync()` — UnitOfWorkBehavior owns this
- `UnitOfWorkBehavior` without depth counter — sub-commands commit prematurely, breaking atomicity
- Separate repository class per entity — generic implementation handles all types
- Raw LINQ in handler — use specifications for all queries
- `IRepository<T>` used in query handlers — use `IReadRepository<T>` to signal read intent
- `UnitOfWorkBehavior` applied to queries — adds unnecessary overhead on reads

# Checklist
- [ ] `IRepository<T>` defined in BuildingBlocks with no `SaveChangesAsync`
- [ ] `IReadRepository<T>` defined in BuildingBlocks
- [ ] `IUnitOfWork` defined in BuildingBlocks
- [ ] `UnitOfWorkContext` defined in BuildingBlocks and registered as `Scoped`
- [ ] `UnitOfWorkBehavior` uses depth counter — only commits when `Depth == 1`
- [ ] `UnitOfWorkBehavior` registered in MediatR pipeline for `ICommand` only
- [ ] Generic `Repository<T>` registered in DI for both interfaces
- [ ] `UnitOfWork` registered in DI
- [ ] Command handlers inject `IRepository<T>` — never DbContext
- [ ] Query handlers inject `IReadRepository<T>` — never `IRepository<T>`
- [ ] No `SaveChangesAsync` calls in any handler

# Unittest TestCases
- [ ] When top-level command runs Then SaveChanges called exactly once
- [ ] When command dispatches sub-command Then SaveChanges called once after root completes
- [ ] When sub-command completes Then SaveChanges not called at sub-command level
- [ ] When handler throws Then SaveChanges not called — changes discarded
- [ ] When query handler runs Then UnitOfWorkBehavior does not activate
- [ ] When multiple repositories used in one handler Then all changes committed in single SaveChanges

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/Solutions/command-handling.solution.skill]] — handlers use IRepository and IUnitOfWork via this pattern
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/query-handler-pattern.skill]] — query handlers use IReadRepository
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/ardalis-specification-pattern.skill]] — all repository queries use specifications
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-event-handler-pattern.skill]] — event handlers also use IRepository + IUnitOfWork
- [[skills/dotnet/skill-graph/developing/Architecture/backend-project-structure.skill]] — interfaces in BuildingBlocks, implementation in App.Infrastructure