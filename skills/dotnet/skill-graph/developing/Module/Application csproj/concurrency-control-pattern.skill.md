---
uid: 42ca4446-8e2c-41b5-b9ac-5a016afb30b2
status: draft
name: concurrency-control-pattern
description: rules for implementing optimistic concurrency control via ETag, If-Match, and MediatR pipeline version checking
domain: skill
type: template
tags:
  - dotnet
  - application
  - concurrency
  - etag
  - rowversion
  - mediatr
triggers:
  - implement concurrency control
  - optimistic concurrency
  - version check
  - etag if-match
  - prevent stale update
aliases:
  - ConcurrencyControl
  - IHasVersions
  - ETag
---
# Goal
Define how to prevent lost updates when multiple clients modify the same entity concurrently. The client receives an ETag on GET containing encoded entity versions. On update, the client sends the ETag as If-Match. The API decodes it, populates the command, and a MediatR pipeline behavior validates all versions before the handler runs. Without this pattern, the last writer silently wins — earlier changes are overwritten with no indication to the user.

# Core Principles
- ETag encodes all entity versions involved in the request as base64 JSON
- Client sends ETag back as If-Match on every update request
- API controller decodes If-Match and populates `IHasVersions` on the command
- `ConcurrencyBehavior` validates all versions before handler runs — returns 409 on mismatch
- Missing If-Match header returns 412 Precondition Failed — not 400 or 409
- EF concurrency token is the final guard — pipeline is the early client-friendly check
- All update commands use `IHasVersions` — consistent interface regardless of entity count
- Entity name string used as key — never C# Type — decouples HTTP contract from assembly names

# ETag Flow
```
GET /tasks/2
← 200 OK
← ETag: "eyJUYXNrIjp7IjIiOjN9fQ=="
   decoded: {"Task":{"2":3}}

Client modifies data locally

PUT /tasks/2  { title: "New title" }
→ If-Match: "eyJUYXNrIjp7IjIiOjN9fQ=="
    ↓
API controller decodes If-Match → {"Task":{"2":3}}
    ↓
Populates UpdateTaskCommand.Versions = {"Task": {"2": 3}}
    ↓
ConcurrencyBehavior loads Task#2 → checks Version == 3
    ↓ (match)
Handler runs → domain call → UnitOfWorkBehavior commits
← 200 OK with new ETag

    ↓ (mismatch — another client updated Task#2 in the meantime)
ConcurrencyBehavior returns Result.Conflict
← 409 Conflict

    ↓ (If-Match header missing)
API controller returns 412 Precondition Failed before MediatR dispatch
```

# Structure / Contracts

## IHasVersions — BuildingBlocks

All update commands implement this interface.

```csharp
// BuildingBlocks/Concurrency/IHasVersions.cs
public interface IHasVersions
{
    // entity name → (entity id → row version)
    // e.g. {"Task": {"2": 3, "3": 8}, "TimeLog": {"1": 19}}
    IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions { get; }
}
```

## IEntityVersionResolver — BuildingBlocks

Maps entity name string to a C# type for repository lookup.

```csharp
// BuildingBlocks/Concurrency/IEntityVersionResolver.cs
public interface IEntityVersionResolver
{
    Type? Resolve(string entityName);
}
```

## EntityVersionResolver — App.Infrastructure

Registered in DI. Maps string names to entity types.

```csharp
// App.Infrastructure/Concurrency/EntityVersionResolver.cs
public class EntityVersionResolver : IEntityVersionResolver
{
    private static readonly Dictionary<string, Type> _map = new()
    {
        ["Task"] = typeof(TodoTask),
        ["TimeLog"] = typeof(TimeEntry),
        ["Order"] = typeof(Order),
    };

    public Type? Resolve(string entityName)
        => _map.GetValueOrDefault(entityName);
}
```

## ConcurrencyBehavior — BuildingBlocks

Runs after ValidationBehavior, before UnitOfWorkBehavior. Loads each entity by id and compares version. Returns conflict on any mismatch.

```csharp
// BuildingBlocks/MediatR/ConcurrencyBehavior.cs
public class ConcurrencyBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasVersions
    where TResponse : IResult
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityVersionResolver _resolver;

    public ConcurrencyBehavior(
        IServiceProvider serviceProvider,
        IEntityVersionResolver resolver)
    {
        _serviceProvider = serviceProvider;
        _resolver = resolver;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        foreach (var (entityName, idVersions) in request.Versions)
        {
            var entityType = _resolver.Resolve(entityName);
            if (entityType is null)
                return (TResponse)Result.Error($"Unknown entity: {entityName}");

            // resolve IReadRepository<TEntity> from DI
            var repoType = typeof(IReadRepository<>).MakeGenericType(entityType);
            var repo = _serviceProvider.GetRequiredService(repoType);

            foreach (var (id, expectedVersion) in idVersions)
            {
                var entity = await GetEntityByIdAsync(repo, entityType, id, ct);

                if (entity is null)
                    return (TResponse)Result.NotFound();

                if (entity.Version != expectedVersion)
                    return (TResponse)Result.Conflict(
                        $"{entityName}#{id} was modified by another user.");
            }
        }

        return await next();
    }
}
```

## ETag encoding/decoding — Shared

```csharp
// BuildingBlocks/Concurrency/ETagEncoder.cs
public static class ETagEncoder
{
    // encode versions dict to base64 JSON ETag
    public static string Encode(
        Dictionary<string, Dictionary<int, uint>> versions)
    {
        var json = JsonSerializer.Serialize(versions);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    // decode If-Match header back to versions dict
    public static Dictionary<string, Dictionary<int, uint>>? Decode(string etag)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(etag));
            return JsonSerializer
                .Deserialize<Dictionary<string, Dictionary<int, uint>>>(json);
        }
        catch { return null; }
    }
}
```

## Update command — {Module}.Interfaces/Commands

```csharp
// Task.Interfaces/Commands/UpdateTaskCommand.cs
public record UpdateTaskCommand(
    int TaskId,
    string Title,
    IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions
) : ICommand<Result>, IHasVersions;
```

## API controller — extract If-Match, decode, populate command

```csharp
// Task.Api/Controllers/SingleTaskController.cs
[HttpPut("{id}")]
public async Task<IActionResult> Update(
    int id,
    [FromBody] UpdateTaskRequest request,
    [FromHeader(Name = "If-Match")] string? ifMatch,
    CancellationToken ct)
{
    if (string.IsNullOrEmpty(ifMatch))
        return StatusCode(412); // 412 Precondition Failed

    var versions = ETagEncoder.Decode(ifMatch);
    if (versions is null)
        return StatusCode(412); // malformed ETag

    var command = new UpdateTaskCommand(id, request.Title, versions);
    var result = await _sender.Send(command, ct);

    return result.Status switch
    {
        ResultStatus.Ok => Ok(),
        ResultStatus.NotFound => NotFound(),
        ResultStatus.Conflict => Conflict(result.Errors),
        _ => throw new InvalidOperationException(
            $"Unexpected result status '{result.Status}'.")
    };
}
```

## GET response — include ETag
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<TaskDto>> Get(int id, CancellationToken ct)
{
    var result = await _sender.Send(new GetTaskQuery(id), ct);

    if (!result.IsSuccess)
        return NotFound();

    // encode entity versions into ETag
    var etag = ETagEncoder.Encode(new()
    {
        ["Task"] = new() { [id] = result.Value.Version }
    });

    Response.Headers.ETag = $"\"{etag}\"";
    return Ok(result.Value);
}
```

## Pipeline registration order — App.Host

```csharp
// ValidationBehavior → ConcurrencyBehavior → UnitOfWorkBehavior
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConcurrencyBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
```

# Rules
MUST:
- All update commands implement `IHasVersions`
- ETag encoded as base64 JSON — never plain version number
- API controller checks If-Match presence — returns 412 if missing or malformed
- `ConcurrencyBehavior` runs after `ValidationBehavior`, before `UnitOfWorkBehavior`
- Entity name keys are stable strings — never C# type names or namespaces
- `EntityVersionResolver` maps string names to types — registered in App.Infrastructure
- EF concurrency token configured on all mutable entities — see [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/entity-concurrency-pattern.skill]] 
MUST NOT:
- Handler check versions manually — `ConcurrencyBehavior` owns this
- Use C# `Type` as dictionary key in `IHasVersions` — breaks on rename
- Return 400 for missing If-Match — 412 is the correct HTTP status
- Skip ETag on GET responses for mutable entities

# Anti-patterns
- Version as plain `uint` property on command — does not scale to multi-entity updates
- Handler catches `DbUpdateConcurrencyException` and returns conflict — pipeline should catch this earlier
- ETag containing only primary entity version — misses secondary entity conflicts
- `ConcurrencyBehavior` registered after `UnitOfWorkBehavior` — unit of work starts before version validated

# Checklist
- [ ] `IHasVersions` defined in BuildingBlocks
- [ ] `IEntityVersionResolver` defined in BuildingBlocks
- [ ] `EntityVersionResolver` implemented and registered in App.Infrastructure
- [ ] `ETagEncoder` encode/decode implemented
- [ ] `ConcurrencyBehavior` registered between ValidationBehavior and UnitOfWorkBehavior
- [ ] All update commands implement `IHasVersions`
- [ ] GET endpoints include ETag header with encoded versions
- [ ] PUT/PATCH endpoints check If-Match — return 412 if missing
- [ ] EF concurrency token configured on all mutable entities

# Unittest TestCases
- [ ] When If-Match missing Then API returns 412 before MediatR dispatch
- [ ] When If-Match malformed Then API returns 412
- [ ] When version matches Then ConcurrencyBehavior calls next handler
- [ ] When version mismatch Then ConcurrencyBehavior returns Result.Conflict
- [ ] When entity not found during version check Then returns Result.NotFound
- [ ] When multiple entities — one mismatch Then returns Conflict without updating any entity
- [ ] When ETag encoded and decoded Then versions round-trip losslessly

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/entity-concurrency-pattern.skill]] — Version field on entity, IsConcurrencyToken EF config
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/Solutions/command-handling.solution.skill]] — update commands implement IHasVersions
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/repository-pattern.skill]] — ConcurrencyBehavior uses IReadRepository to load entities
- [[skills/dotnet/skill-graph/developing/API Layer/api-structure.skill]] — ETag on GET, If-Match on PUT/PATCH, 412 response
- [[skills/dotnet/skill-graph/developing/Architecture/backend-project-structure.skill]] — IHasVersions in BuildingBlocks, resolver in App.Infrastructure