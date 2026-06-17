---
uid: 6a06b898-f9cc-4977-8ec1-1d0466f8bee9
name: module-application-resolver
description: defines how to implement IGuidResolver for a command that carries a client-generated Guid
domain: skill
type: template
version: 20260606
tags:
  - skill/template/class
  - dotnet
  - application
  - guid
  - idempotency
triggers:
  - implement guid resolver
  - create IGuidResolver
  - resolver for external created entity
aliases:
  - IGuidResolver Implementation
---
# Goal
Define how to implement `IGuidResolver<TRequest, TResponse>` for a specific command. The resolver checks whether an entity with the command's `Guid` already exists and returns the existing result — allowing `GuidResolvingBehavior` from [[skills/dotnet/skill-graph/developing/Module/Application csproj/guid-resolving-pipeline.skill|guid-resolving-pipeline.skill]] to short-circuit with the existing entity data before the handler runs.

# Core Principles
- Resolver checks existence only — it never creates or modifies anything
- Returns `TResponse?` — `null` means not found, non-null means already exists
- Returns the full existing result so the pipeline can return it directly to the caller
- One resolver class per command that implements `IHasGuid`
- Uses `IReadRepository<T>` — read-only, no write intent

# Structure
## Place in csproj
Defined in [[skills/dotnet/skill-graph/developing/Module/Application csproj/module-application.csproj.skill|module-application-csproj.skill]]
```
/{ModuleName}.Application
  /Resolvers
    CreateTaskGuidResolver.cs
```
## Naming convention
class name: `{CommandName}GuidResolver`
file name: `{CommandName}GuidResolver.cs`
rule: class name and file name must match exactly
# Implementation
Realize interface `IGuidResolver<CreateTaskCommand, Result<CreateTaskResult>>` from [[skills/dotnet/skill-graph/developing/Module/Application csproj/guid-resolving-pipeline.skill|guid-resolving-pipeline.skill]]
```csharp
// Task.Application/Resolvers/CreateTaskGuidResolver.cs
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

# Rules
- Implement `IGuidResolver<TRequest, TResponse>`
- Return `null` when entity does not exist
- Return existing result wrapped in `Result.Conflict(...)` when entity exists
- Use `IReadRepository<T>` — never `IRepository<T>` or DbContext
- Use a named specification — never inline LINQ 
MUST NOT:
- Create, update, or delete anything
- Call `SaveChangesAsync`
- Contain business logic

# Anti-patterns
- Returning `Result.Success(...)` for existing entity — must be `Result.Conflict` so API layer maps to 409
- Inline LINQ instead of spec — use `TaskByGuidSpec`
- Injecting `IRepository<T>` — read intent, use `IReadRepository<T>`

# Check list
- [ ]  Class implements `IGuidResolver<TRequest, TResponse>`
- [ ]  Returns `null` when not found
- [ ]  Returns `Result.Conflict(existingResult)` when found
- [ ]  Uses `IReadRepository<T>`
- [ ]  Uses named specification
- [ ]  Registered in DI — see module-application-di.class.skill

# Unittest TestCases
- [ ]  When entity with Guid does not exist Then returns null
- [ ]  When entity with Guid exists Then returns Result.Conflict with existing entity data
- [ ]  When Result.Conflict returned Then existing Id matches persisted entity

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/classes/module-application-di.class.skill]] — resolver must be registered in DI
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/ardalis-specification-pattern.skill|ardalis-specification-pattern.skill]] — `TaskByGuidSpec` used for the lookup
- [[repository.skill]] — `IReadRepository` used here
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/guid-resolving-pipeline.skill|guid-resolving-pipeline.skill]] — full pipeline flow this resolver participates in
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/external-created-entity.skill|external-created-entity.skill]] — entity must have `Guid` field and unique index