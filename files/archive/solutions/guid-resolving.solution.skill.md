---
uid:
name: guid-resolving
description: defines the full flow for externally created entities — Guid field, unique index, pipeline resolution, and 409 with existing data
domain: skill
type: architecture
version: 20260607
tags:
  - skill/architecture/solution
  - dotnet
  - guid
  - idempotency
  - external-entity
triggers:
  - externally created entity
  - client generated guid
  - idempotent creation
  - guid resolving pipeline
---
# Goal
Define how to handle entity creation initiated by an external system that generates its own Guid. Solves two problems: idempotency (retried requests must not create duplicates) and correlation (external system references entity by Guid before knowing internal Id). Without this solution, network retries create duplicate entities with no safe recovery path.

# Core Principles
- External system owns the Guid — backend never generates it for external creation
- Guid is a correlation handle only — never used in domain logic after creation
- Unique index on Guid is the database-level idempotency guard
- Pipeline checks Guid before handler runs — handler never sees duplicate requests
- 409 response includes existing entity data — client recovers without a second GET
- Internal `Id` is the only identity used inside domain after creation

# Depend on
- command-handling.solution.skill.md — GuidResolvingBehavior runs inside command pipeline

# Flow
```
Client generates Guid → sends POST with Guid in body
    ↓
GuidResolvingBehavior intercepts — calls IGuidResolver<TRequest, TResponse>
    ↓ (Guid not found)
Handler runs → creates entity with Guid → stores Guid
UnitOfWorkBehavior commits → unique index enforces at DB level
← 201 Created with internal Id

    ↓ (Guid already exists)
IGuidResolver returns existing Result<T>
GuidResolvingBehavior short-circuits — handler never runs
← 409 Conflict with existing entity data — client recovers in one round trip
```

# Implementation

## {Entity}.cs — `{Module}.Domain/Entities`
Add immutable `Guid` property. Set once on creation, never changed.
```csharp
public class Task
{
    public int Id { get; private set; }
    public Guid Guid { get; private set; }  // set once, never changed
}
```

## {Entity}Config.cs — `{Module}.Domain/Configurations`
Unique index with named constant — referenced in error handling and tests.
```csharp
public class TaskConfig : IEntityTypeConfiguration<Task>
{
    public static string TableName = nameof(Task);
    public static string UX_Guid = $"UX_{TableName}_Guid";

    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder
            .HasIndex(e => e.Guid)
            .IsUnique()
            .HasDatabaseName(UX_Guid);
    }
}
```

## {CommandName}Command.cs — `{Module}.Interfaces/Commands`
Command implements `IHasGuid` to opt into the pipeline.
```csharp
public record CreateTaskCommand(
    Guid Guid,
    string Title
) : ICommand<Result<CreateTaskResult>>, IHasGuid;
```

## IGuidResolver.cs — `BuildingBlocks`
```csharp
public interface IGuidResolver<TRequest, TResponse>
    where TRequest : ICommand<TResponse>, IHasGuid
{
    Task<TResponse?> ResolveAsync(TRequest request, CancellationToken ct);
}
```

## {CommandName}GuidResolver.cs — `{Module}.Application/Resolvers`
Returns `null` if not found, existing result wrapped in `Result.Conflict` if found.
```csharp
public class CreateTaskGuidResolver
    : IGuidResolver<CreateTaskCommand, Result<CreateTaskResult>>
{
    private readonly IReadRepository<Task> _repository;

    public CreateTaskGuidResolver(IReadRepository<Task> repository)
        => _repository = repository;

    public async Task<Result<CreateTaskResult>?> ResolveAsync(
        CreateTaskCommand request, CancellationToken ct)
    {
        var task = await _repository.FirstOrDefaultAsync(
            new TaskByGuidSpec(request.Guid), ct);

        return task is null
            ? null
            : Result.Conflict(new CreateTaskResult(task.Id));
    }
}
```

## GuidResolvingBehavior.cs — `BuildingBlocks`
```csharp
public class GuidResolvingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>, IHasGuid
{
    private readonly IGuidResolver<TRequest, TResponse> _resolver;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var existing = await _resolver.ResolveAsync(request, ct);

        if (existing is not null)
            return existing;

        return await next();
    }
}
```

## {Entity}Controller.cs — `{Module}.Api/Controllers`
Handles `Result.Conflict` returned by pipeline — maps to 409 with body.
```csharp
[HttpPost]
public async Task<ActionResult<CreateTaskResult>> Create(
    [FromBody] CreateTaskCommand command, CancellationToken ct)
{
    var result = await _sender.Send(command, ct);

    return result.Status switch
    {
        ResultStatus.Created => CreatedAtAction(..., result.Value),
        ResultStatus.Invalid => BadRequest(ToProblemDetails(result.ValidationErrors)),
        ResultStatus.Conflict => Conflict(result.Value),  // existing entity data in body
        _ => throw new InvalidOperationException(
            $"Unexpected result status '{result.Status}'.")
    };
}
```

## {Module}ApplicationRegistration.cs — `{Module}.Application`
```csharp
services.AddScoped<
    IGuidResolver<CreateTaskCommand, Result<CreateTaskResult>>,
    CreateTaskGuidResolver>();
```

# Example
```
POST /task { guid: "abc-123", title: "My Task" }  ← first request
← 201 Created { id: 42 }

POST /task { guid: "abc-123", title: "My Task" }  ← retry
← 409 Conflict { id: 42 }                         ← client recovers, no second GET needed
```

# Rules
MUST:
- Client generates Guid — backend never generates it for external creation
- Entity has `Guid` property — immutable after creation
- Unique index on `Guid` with named constant in config class
- Command implements `IHasGuid`
- One `IGuidResolver<TRequest, TResponse>` per `IHasGuid` command
- Resolver returns `Result.Conflict(existingResult)` — never `Result.Success`
- `GuidResolvingBehavior` registered before `UnitOfWorkBehavior` in pipeline
- Controller handles `ResultStatus.Conflict` — returns 409 with existing entity body
MUST NOT:
- Use Guid in domain logic or relations after creation
- Return 200 on duplicate — 409 signals successful idempotent retry
- Generate Guid server-side for external creation flows
- Handler check for duplicate Guid — pipeline owns this

# Anti-patterns
- Handler queries for existing Guid manually — pipeline handles this, not handler
- 409 returns empty body — client must make second GET, adds latency
- Resolver returns `Result.Success` for existing entity — controller maps to 200, client thinks it created a new entity

# Checklist
- [ ] Entity has `Guid` property with no public setter
- [ ] Unique index configured with named constant
- [ ] Command implements `IHasGuid`
- [ ] `IGuidResolver` implemented in `/Resolvers`
- [ ] Resolver returns `null` or `Result.Conflict(existingResult)`
- [ ] `IGuidResolver` registered in module DI registration
- [ ] `GuidResolvingBehavior` registered before `UnitOfWorkBehavior`
- [ ] Controller maps `ResultStatus.Conflict` to 409 with body
- [ ] Guid never used in domain logic

# Unittest TestCases
- [ ] When new Guid Then handler runs and returns 201
- [ ] When duplicate Guid Then GuidResolvingBehavior short-circuits, handler never runs
- [ ] When duplicate Guid Then 409 returned with existing entity Id in body
- [ ] When duplicate Guid bypasses pipeline Then unique index throws DbUpdateException with UX_Guid constraint name

# Relations
- command-handling.solution.skill.md — pipeline this behavior participates in
- entity.class.skill.md — entity Guid field
- ef-configuration.class.skill.md — unique index configuration
- module-application-resolver.class.skill.md — resolver implementation detail
- module-application-di.class.skill.md — resolver DI registration
