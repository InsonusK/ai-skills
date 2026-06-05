---
uid: 5b346aa9-2fd0-4716-b2b7-a34c787961af
status: draft
name: guid-resolving-pipeline
description: rules for resolving client-generated Guid to internal Id before handler runs via MediatR pipeline
domain: skill
type: pattern
tags:
  - dotnet
  - application
  - mediatr
  - guid
  - idempotency
  - pipeline
triggers:
  - guid resolving
  - resolve guid to id
  - idempotent create pipeline
  - GuidResolvingBehavior
aliases:
  - GuidResolvingBehavior
  - IGuidResolver
  - IHasGuid
---
# Goal
Define how client-generated Guids are resolved to internal entity Ids before the command handler runs. The pipeline behavior intercepts any command carrying a Guid, checks if the entity already exists, and short-circuits with 409 Conflict if it does — returning the existing entity so the client can recover without a second GET request. See [[async-external-creation.skill]] for the full client-side flow and architecture decisions behind this pattern.

# Core Principles
- Guid resolution happens in the pipeline — handler never sees duplicate requests
- `GuidResolvingBehavior` is generic and reusable — one implementation for all entity types
- Each module implements `IGuidResolver<TResult>` for its own entity type
- 409 response includes existing entity data — client recovers in one round trip
- Guid is a correlation handle only — never used in domain logic after creation
- `IHasGuid` marker on command opts into the pipeline — non-Guid commands unaffected

# Pipeline Position
```
HTTP Request
    ↓
ValidationBehavior
    ↓
GuidResolvingBehavior ← checks if Guid already exists
    ↓ (not found — continue)
ConcurrencyBehavior
    ↓
UnitOfWorkBehavior
    ↓
Handler — creates entity, stores Guid
    ↓
UnitOfWorkBehavior — commits
    ↓
201 Created with internal Id

    ↓ (found — short-circuit)
409 Conflict with existing entity data — handler never runs
```

# Structure / Contracts

## IHasGuid — BuildingBlocks

Commands that carry a client-generated Guid implement this interface.

```csharp
// BuildingBlocks/MediatR/IHasGuid.cs
public interface IHasGuid
{
    Guid Guid { get; }
}
```

## IGuidResolver — BuildingBlocks

Each module implements this for its own entity type. Returns the existing result if entity with Guid exists, null otherwise.

```csharp
// BuildingBlocks/MediatR/IGuidResolver.cs
public interface IGuidResolver<TResult>
{
    Task<TResult?> ResolveAsync(Guid guid, CancellationToken ct);
}
```

## ConflictException — Shared

Carries the existing entity so the API layer can return it in the 409 body.

```csharp
// Shared/Exceptions/ConflictException.cs
public class ConflictException<T> : Exception
{
    public T Existing { get; }

    public ConflictException(T existing)
        : base("Entity with this Guid already exists.")
        => Existing = existing;
}
```

## GuidResolvingBehavior — BuildingBlocks

Generic pipeline behavior. Activates only for commands implementing `IHasGuid`.

```csharp
// BuildingBlocks/MediatR/GuidResolvingBehavior.cs
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

## IGuidResolver implementation — {Module}.Application

One resolver per external-created entity type. Uses a spec to find the entity by Guid and maps to the command result type.

```csharp
// Task.Application/Resolvers/CreateTaskGuidResolver.cs
public class CreateTaskGuidResolver : IGuidResolver<Result<CreateTaskResult>>
{
    private readonly IReadRepository<TodoTask> _repository;

    public CreateTaskGuidResolver(IReadRepository<TodoTask> repository)
        => _repository = repository;

    public async Task<Result<CreateTaskResult>?> ResolveAsync(
        Guid guid, CancellationToken ct)
    {
        var task = await _repository.FirstOrDefaultAsync(
            new TaskByGuidSpec(guid), ct);

        return task is null
            ? null
            : Result.Success(new CreateTaskResult(task.Id));
    }
}
```

## Command — {Module}.Interfaces/Commands

```csharp
// Task.Interfaces/Commands/CreateTaskCommand.cs
public record CreateTaskCommand(
    Guid Guid,
    string Title,
    int AssigneeId
) : ICommand<Result<CreateTaskResult>>, IHasGuid;
```

## API controller — catch ConflictException, return 409 with body

```csharp
// Task.Api/Controllers/TaskController.cs
[HttpPost]
public async Task<ActionResult<CreateTaskResult>> Create(
    [FromBody] CreateTaskCommand command,
    CancellationToken ct)
{
    try
    {
        var result = await _sender.Send(command, ct);
        return result.Status switch
        {
            ResultStatus.Created => CreatedAtAction(
                nameof(SingleTaskController.Get),
                "SingleTask",
                new { id = result.Value.Id },
                result.Value),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}'.")
        };
    }
    catch (ConflictException<Result<CreateTaskResult>> ex)
    {
        // 409 with existing entity — client can recover without second GET
        return Conflict(ex.Existing.Value);
    }
}
```

## DI Registration — {Module}.Application registration method

```csharp
// Task.Application/TaskApplicationRegistration.cs
services.AddScoped<IGuidResolver<Result<CreateTaskResult>>, CreateTaskGuidResolver>();
```

## File structure

```
/BuildingBlocks
    /MediatR
        IHasGuid.cs
        IGuidResolver.cs
        GuidResolvingBehavior.cs

/Shared
    /Exceptions
        ConflictException.cs

/{Module}.Application
    /Resolvers
        CreateTaskGuidResolver.cs
    /Specifications       ← TaskByGuidSpec lives in Domain
```

# Rules

MUST:

- `IHasGuid`, `IGuidResolver<T>`, `GuidResolvingBehavior` defined in BuildingBlocks
- `ConflictException<T>` defined in Shared
- Command implements `IHasGuid` to participate in Guid resolution
- One `IGuidResolver<TResult>` per external-created entity type — in `{Module}.Application`
- `GuidResolvingBehavior` registered before `ConcurrencyBehavior` and `UnitOfWorkBehavior`
- API catches `ConflictException<T>` and returns 409 with existing entity body
- Guid spec (`TaskByGuidSpec`) lives in `{Module}.Domain/Specifications` MUST NOT:
- Handler check for duplicate Guid itself — pipeline owns this
- Return 200 on duplicate — 409 signals successful idempotent retry to client
- Use Guid in domain logic after creation — internal Id only
- Register `GuidResolvingBehavior` as open generic — DI resolves per concrete command type

# Anti-patterns

- Handler queries for existing Guid manually — duplicates pipeline logic, not reusable
- 409 response returns empty body — client must make second GET to recover
- `IGuidResolver` implemented in Domain — resolver uses repository, belongs in Application
- `GuidResolvingBehavior` registered after `UnitOfWorkBehavior` — unit of work starts before duplicate checked

# Checklist

- [ ] Command implements `IHasGuid`
- [ ] `IGuidResolver<TResult>` implemented in `{Module}.Application/Resolvers`
- [ ] `TaskByGuidSpec` defined in `{Module}.Domain/Specifications`
- [ ] `IGuidResolver` registered in module DI registration
- [ ] `GuidResolvingBehavior` registered before ConcurrencyBehavior in pipeline
- [ ] API catches `ConflictException<T>` and returns 409 with body
- [ ] Guid never used in domain logic after entity creation

# Unittest TestCases

- [ ] When Guid not found Then resolver returns null and handler runs
- [ ] When Guid already exists Then GuidResolvingBehavior throws ConflictException
- [ ] When ConflictException thrown Then API returns 409 with existing entity Id
- [ ] When 409 returned Then existing entity data included in response body
- [ ] When duplicate Guid bypasses pipeline Then unique index throws DbUpdateException with correct constraint name

# Relations

- [[async-external-creation.skill]] — architecture and client flow this pipeline implements
- [[external-created-entity.skill]] — entity Guid property and unique index
- [[ardalis-specification-pattern.skill]] — TaskByGuidSpec used by resolver
- [[repository-pattern.skill]] — IReadRepository used in IGuidResolver implementation
- [[command-handler-pattern.skill]] — commands that use this pipeline implement IHasGuid
- [[domain-configuration-pattern.skill]] — unique index on Guid enforces DB-level idempotency