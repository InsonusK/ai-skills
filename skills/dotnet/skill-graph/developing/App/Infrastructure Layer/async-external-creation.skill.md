---
uid: aea8f324-0302-4c5f-9de2-b50635b2b930
status: draft
name: async-external-creation
description: pattern for handling async entity creation initiated by an external system using client-generated Guid
domain: skill
type: pattern
tags:
  - entity
  - creation
  - architecture
  - guid
  - idempotency
  - mediatR
triggers:
  - develop async creation process
  - client-generated guid
  - idempotent create endpoint
  - external entity creation
---
# Goal
Define how to handle entity creation initiated by an external system that generates its own Guid before the entity exists in the backend. The pattern solves two problems: idempotency (retried requests must not create duplicates) and correlation (external system can reference the entity by Guid before it knows the internal Id). Without this pattern, network retries cause duplicate entities and the client has no safe way to recover from transient failures.

# Core Principles
- External system owns the Guid — backend never generates it for external creation
- Guid is a correlation handle only — never used in domain logic
- Unique index on Guid enforces idempotency at the database level
- 409 Conflict on duplicate Guid includes the existing entity so client can recover without a second request
- MediatR pipeline resolves Guid to internal Id before the handler runs
- Internal Id is the only identity used inside the domain after creation

# Flow
```
Angular generates Guid → optimistic NgRx store update
    ↓
POST /entities { guid, ...payload }
    ↓
GuidResolvingBehavior checks if entity with Guid already exists
    ↓ (not found)
Handler creates entity → returns 201 with internal Id
    ↓
Angular updates NgRx store: replaces optimistic entry with real Id

On retry (transient failure, backend unavailable):
    ↓
Same POST with same Guid
    ↓
GuidResolvingBehavior finds existing entity by Guid
    ↓
Returns 409 Conflict with existing entity Id — no duplicate created
    ↓
Angular recovers: updates NgRx store with returned Id
```

# Structure / Contracts

## [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|Entity]] — [[skills/dotnet/skill-graph/developing/Module/Domain csproj/module-domain-csproj.skill|{ModuleName}.Domain]]

Guid property is immutable after creation. See [[skills/dotnet/skill-graph/Domain Layer/entity/external-created-entity.skill]].

```csharp
public class TodoTask
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }  // set once on creation, never changed
    public string Title { get; internal set; }
}
```

## Command — {Module}.Interfaces/Commands

Command carries the client-generated Guid.

```csharp
public record CreateTaskCommand(
    Guid Guid,
    string Title
) : IRequest<CreateTaskResult>;

public record CreateTaskResult(int Id);
```

## GuidResolvingBehavior — BuildingBlocks

MediatR pipeline behavior that intercepts commands carrying a Guid, checks for an existing entity, and short-circuits with 409 before the handler runs.

```csharp
// BuildingBlocks/MediatR/GuidResolvingBehavior.cs
public interface IHasGuid
{
    Guid Guid { get; }
}

public interface IGuidResolver<TResult>
{
    Task<TResult?> ResolveAsync(Guid guid, CancellationToken ct);
}

public class GuidResolvingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasGuid
{
    private readonly IGuidResolver<TResponse> _resolver;

    public GuidResolvingBehavior(IGuidResolver<TResponse> resolver)
        => _resolver = resolver;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var existing = await _resolver.ResolveAsync(request.Guid, ct);
        if (existing is not null)
            throw new ConflictException<TResponse>(existing);

        return await next();
    }
}
```

## ConflictException — Shared

```csharp
// Shared/Exceptions/ConflictException.cs
public class ConflictException<T> : Exception
{
    public T Existing { get; }
    public ConflictException(T existing) : base("Entity already exists")
        => Existing = existing;
}
```

## GuidResolver implementation — {Module}.Application

Each module implements `IGuidResolver` for its own entity type.

```csharp
// Task.Application/Resolvers/CreateTaskGuidResolver.cs
public class CreateTaskGuidResolver : IGuidResolver<CreateTaskResult>
{
    private readonly IReadRepository<TodoTask> _repository;

    public async Task<CreateTaskResult?> ResolveAsync(Guid guid, CancellationToken ct)
    {
        var task = await _repository.FirstOrDefaultAsync(
            new TaskByGuidSpec(guid), ct);

        return task is null ? null : new CreateTaskResult(task.Id);
    }
}
```

## API — returns 409 with existing entity

```csharp
// Task.Api/Controllers/TaskController.cs
try
{
    var result = await _mediator.Send(command);
    return CreatedAtAction(..., result);
}
catch (ConflictException<CreateTaskResult> ex)
{
    return Conflict(ex.Existing);  // 409 with existing Id — client can recover
}
```

## EF unique index — {Module}.Domain/Configurations

See [[skills/dotnet/skill-graph/Domain Layer/entity/external-created-entity.skill]] and [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-configuration-pattern.skill]].

```csharp
public class TodoTaskConfig : IEntityTypeConfiguration<TodoTask>
{
    public static string TableName = nameof(TodoTask);
    public static string UX_Guid = $"UX_{TableName}_Guid";

    public void Configure(EntityTypeBuilder<TodoTask> entityBuilder)
    {
        entityBuilder
            .HasIndex(e => e.Guid)
            .IsUnique()
            .HasDatabaseName(UX_Guid);
    }
}
```

# Rules

MUST:
- Client always generates Guid — backend never generates it for external creation requests
- Guid stored as unique indexed column — database is the final idempotency guard
- `GuidResolvingBehavior` runs before handler — handler never sees duplicate requests
- 409 response includes existing entity data — client recovers without extra GET
- Guid immutable after creation — never updated, never reused
- Command implements `IHasGuid` to participate in the pipeline 
MUST NOT:
- Use Guid in domain logic — only `Id` used internally after creation
- Return 200 on duplicate — 409 signals the client that retry succeeded
- Generate Guid server-side for external creation flows — removes idempotency guarantee
- Store Guid on entity as nullable — it is required for all external-created entities

# Anti-patterns
- Server generates Guid — client has no correlation handle for retries
- No unique index on Guid — database allows duplicates, idempotency relies only on application logic
- 409 returns no body — client must make a second GET request to recover, adds latency and complexity
- Handler checks for duplicate Guid itself — logic scattered, not reusable across commands
- Guid used as foreign key in relations — leaks external identity into domain relationships

# Checklist
- [ ] Entity has `Guid` property — immutable after creation
- [ ] Unique index configured on `Guid` — see [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-configuration-pattern.skill]]
- [ ] Command implements `IHasGuid`
- [ ] `GuidResolvingBehavior` registered in MediatR pipeline
- [ ] `IGuidResolver<TResult>` implemented in Application for this command
- [ ] API catches `ConflictException<T>` and returns 409 with existing entity
- [ ] `Guid` never used in domain logic or relations
- [ ] Unit test: duplicate Guid returns 409 with correct existing entity
- [ ] Unit test: unique index violation produces correct PostgresException constraint name

# Unittest TestCases
- [ ] When create entity with new Guid Then returns 201 with internal Id
- [ ] When create entity with duplicate Guid Then GuidResolvingBehavior short-circuits
- [ ] When create entity with duplicate Guid Then response is 409 with existing entity Id
- [ ] When insert duplicate Guid bypassing pipeline Then DbUpdateException with UX_Guid constraint name
- [ ] When entity created Then Guid is immutable — update attempt has no effect

# Relations
- [[skills/dotnet/skill-graph/Domain Layer/entity/external-created-entity.skill]] — defines Guid property and unique index on entity
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-configuration-pattern.skill]] — defines EF unique index configuration
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill]] — external created entities follow external entity type rules
- [[skills/dotnet/skill-graph/developing/Architecture/backend-project-structure.skill]] — GuidResolvingBehavior lives in BuildingBlocks, resolver in Application