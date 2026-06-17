---
uid: 8f2a4c7e-3b1d-4e9c-a5f7-d2b6e8c3f1a4
order: 14
name: entity-concurrency
description: Defines the full optimistic concurrency control stack — Version/xmin on mutable entities, IHasVersions on update commands, ETagEncoder in BuildingBlocks, IEntityVersionResolver in BuildingBlocks with App.Infrastructure implementation, ConcurrencyBehavior in BuildingBlocks inserted between ValidationBehavior and UnitOfWorkBehavior, ETag on GET responses, and 412 on missing If-Match
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - application
  - infrastructure
  - concurrency
  - etag
  - rowversion
  - mediatr
triggers:
  - implement concurrency control
  - optimistic concurrency
  - prevent lost updates
  - version check
  - etag if-match
  - mutable entity
  - IHasVersions
creates:
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/IHasVersions.class.skill|IHasVersions.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/IEntityVersionResolver.class.skill|IEntityVersionResolver.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/ETagEncoder.class.skill|ETagEncoder.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/ConcurrencyBehavior.class.skill|ConcurrencyBehavior.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Infrastructure csproj/classes/EntityVersionResolver.class.skill|EntityVersionResolver.class.skill]]"
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/BuildingBlocks.csproj.skill|BuildingBlocks.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Infrastructure csproj/App.Infrastructure.csproj.skill|App.Infrastructure.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Host csproj/App.Host.csproj.skill|App.Host.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Entity.class.skill|Entity.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/EntityConfiguration.class.skill|EntityConfiguration.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Interfaces csproj/classes/Command.class.skill|Command.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Api csproj/classes/SingleEntityController.class.skill|SingleEntityController.class.skill]]"
depends_on:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill|01-module-boundary.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill|02-solution-layer-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill|03-domain-configuration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/06-domain-behaviour.solution.skill|06-domain-behaviour.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/08-repository.solution.skill|08-repository.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/09-command-handler.solution.skill|09-command-handler.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/10-validation.solution.skill|10-validation.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/11-unit-of-work.solution.skill|11-unit-of-work.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/13-api-structure.solution.skill|13-api-structure.solution.skill]]"
---

# Goal
- Define `Version` / `xmin` as the concurrency token on every mutable entity — the database-level last line of defence
- Define `IHasVersions` in BuildingBlocks as the interface all update commands implement to carry client-supplied version information
- Define `ETagEncoder` in BuildingBlocks to encode entity versions as base64 JSON ETags for HTTP transport
- Define `IEntityVersionResolver` in BuildingBlocks and `EntityVersionResolver` in App.Infrastructure to map stable string entity names to C# types for version checking
- Define `ConcurrencyBehavior` in BuildingBlocks as the pipeline behavior that validates all versions before the handler runs — inserted between `ValidationBehavior` and `UnitOfWorkBehavior`
- Define the full ETag flow: GET encodes version into `ETag` header, PUT/PATCH decodes `If-Match` header, missing or malformed `If-Match` returns 412 before MediatR dispatch

# Core Principles
- `Version` is a `uint` property mapped to PostgreSQL `xmin` — auto-incremented by the database on every row change, never set by application code
- `IHasVersions` and `ETagEncoder` live in BuildingBlocks — referenced by both Application and Api layers
- `IEntityVersionResolver` lives in BuildingBlocks — implementation lives in App.Infrastructure
- Entity name string keys in `IHasVersions` are stable business names — never C# type names or namespaces — decouples HTTP contract from assembly structure
- `ConcurrencyBehavior` constrained on `where TRequest : IHasVersions` — only update commands are checked, not all commands
- `ConcurrencyBehavior` runs after `ValidationBehavior` and before `UnitOfWorkBehavior` — invalid or stale commands never open a unit of work
- Missing `If-Match` returns 412 Precondition Failed — not 400 (bad input) or 409 (conflict) — 412 means precondition not supplied
- EF concurrency token is the final guard — `ConcurrencyBehavior` is the early client-friendly check that gives a meaningful 409 before EF raises `DbUpdateConcurrencyException`
- All version mismatches return `Result.Conflict` from the behavior — handler never runs for stale updates
- ETag encodes ALL entity versions involved in the operation — not only the primary entity

# Depend on solutions
- [[01-module-boundary.solution.skill]] — defines BuildingBlocks, App.Infrastructure, App.Host, and module project boundaries
- [[02-solution-layer-structure.solution.skill]] — BuildingBlocks references Shared; App.Infrastructure references BuildingBlocks; Api references own Interfaces
- [[03-domain-configuration.solution.skill]] — `Version` EF configuration (`xmin`, `IsConcurrencyToken`) follows this solution's configuration pattern
- [[06-domain-behaviour.solution.skill]] — mutable entities that need concurrency control follow entity behavior rules
- [[08-repository.solution.skill]] — `IReadRepository<T>` from Shared used by `ConcurrencyBehavior` to load entities for version checking
- [[09-command-handler.solution.skill]] — update commands extended with `IHasVersions`
- [[10-validation.solution.skill]] — `ValidationBehavior` runs before `ConcurrencyBehavior` — pipeline order dependency
- [[11-unit-of-work.solution.skill]] — `UnitOfWorkBehavior` runs after `ConcurrencyBehavior` — pipeline order dependency
- [[13-api-structure.solution.skill]] — `SingleEntityController` extended with ETag on GET, If-Match on PUT/PATCH, 412 response

# Requirements
- `System.Text.Json` — provides `JsonSerializer` used in `ETagEncoder`
- `Microsoft.EntityFrameworkCore` — provides `IsConcurrencyToken()` for `Version` EF configuration

# Template Skill Mutations

## {Module}.Domain (.csproj) (extended)

### Project extension

#### Goal
- Add `Version` concurrency token to every mutable entity and configure it as the PostgreSQL `xmin` system column

#### Rules
MUST:
- Every mutable entity (Internal Mutable, External Mutable from solution 01) has a `Version` property
- `Version` configured as `IsConcurrencyToken()` mapping to `xmin` in EF configuration

MUST NOT:
- Application code set or read `Version` for any purpose other than concurrency checking — it is a database concern

---

### Class extension

#### Entity.class.skill (extended)

##### Goal
- Add `Version` as a required property on all mutable entities

##### Core Principals
- `Version` is `uint` with `internal set` — never set by application code, only by database
- Present on Internal Mutable and External Mutable entity types — absent on Immutable entities
- Read by `ConcurrencyBehavior` via the entity loaded from the repository — never passed as a domain parameter

##### Implementation changes
Mutable entity must declare `Version`:

```csharp
// Task.Domain/Entities/TodoTask.cs
public class TodoTask
{
    public int Id { get; internal set; }
    public string Title { get; internal set; }
    public TaskStatus Status { get; internal set; }
    public int AssigneeId { get; internal set; }
    public uint Version { get; internal set; }   // ← added by this solution

    internal void Assign(int assigneeId) { ... }
}
```

##### Rule changes
MUST:
- All mutable entities have `public uint Version { get; internal set; }`

MUST NOT:
- Immutable entities (no lifecycle transitions) have `Version` — they are never updated
- Application code assign `Version` — it is controlled exclusively by the database

---

#### EntityConfiguration.class.skill (extended)

##### Goal
- Configure `Version` as the EF concurrency token mapped to the PostgreSQL `xmin` system column

##### Core Principals
- `xmin` is a PostgreSQL system column — automatically incremented on every row update
- `IsConcurrencyToken()` tells EF to include `Version` in `WHERE` clause on `UPDATE` — EF raises `DbUpdateConcurrencyException` if zero rows affected (version mismatch at DB level)
- `ValueGeneratedOnAddOrUpdate()` tells EF the value comes from the database — never from application code

##### Implementation changes
Every mutable entity configuration must include the `Version` mapping:

```csharp
// Task.Domain/Configurations/TodoTaskConfig.cs
public class TodoTaskConfig : IEntityTypeConfiguration<TodoTask>
{
    public static string TableName = nameof(TodoTask);

    public void Configure(EntityTypeBuilder<TodoTask> builder)
    {
        // ... other configuration (indexes, relations)

        builder
            .Property(e => e.Version)
            .HasColumnName("xmin")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();
    }
}
```

##### Rule changes
MUST:
- Every mutable entity configuration maps `Version` to `xmin` with `IsConcurrencyToken()` and `ValueGeneratedOnAddOrUpdate()`

MUST NOT:
- `HasDefaultValue` or `HasComputedColumnSql` used on `Version` — `xmin` is managed entirely by PostgreSQL

---

## BuildingBlocks (.csproj) (extended)

### Project extension

#### Goal
- Own `IHasVersions`, `IEntityVersionResolver`, `ETagEncoder`, and `ConcurrencyBehavior` — the full client-facing concurrency contract and pipeline enforcement
- Reference `IReadRepository<T>` from Shared for version loading in `ConcurrencyBehavior`

#### Structure

##### Project Structure
```
/BuildingBlocks
  /MediatR
    ValidationBehavior.cs     ← solution 10
    UnitOfWorkContext.cs       ← solution 11
    UnitOfWorkBehavior.cs      ← solution 11
    ConcurrencyBehavior.cs
  /Concurrency
    IHasVersions.cs
    IEntityVersionResolver.cs
    ETagEncoder.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Concurrency/IHasVersions.cs | Interface carried by all update commands | IHasVersions.class.skill |
| /Concurrency/IEntityVersionResolver.cs | Maps string entity name to C# Type | IEntityVersionResolver.class.skill |
| /Concurrency/ETagEncoder.cs | Encodes/decodes entity versions as base64 JSON ETag | ETagEncoder.class.skill |
| /MediatR/ConcurrencyBehavior.cs | Pipeline behavior validating versions before handler runs | ConcurrencyBehavior.class.skill |

#### NuGet Packages
| Package | Purpose |
| --- | --- |
| `System.Text.Json` | `JsonSerializer` used in `ETagEncoder` |
| `MediatR` | `IPipelineBehavior<TRequest, TResponse>` |

#### Rules
MUST:
- All four components defined in BuildingBlocks
- `ConcurrencyBehavior` constrained on `where TRequest : IHasVersions`
- `ETagEncoder` and `IHasVersions` available to both Application and Api layers via BuildingBlocks reference

---

### Class extension

#### IHasVersions (created)

##### Goal
- Provide a typed contract for update commands to carry client-supplied version information
- Enable `ConcurrencyBehavior` to activate selectively on commands that carry versions — not all commands

##### Core Principals
- Dictionary structure: entity name string → (entity Id → expected version)
- Supports multi-entity updates — a single command can carry versions for multiple entities
- Entity name keys are stable business strings — `"Task"`, `"TimeLog"` — never C# type names
- Declared in BuildingBlocks — update commands in `{Module}.Interfaces` implement this

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Version carrier interface | `IHasVersions` | `IHasVersions` | `IHasVersions.cs` | `IHasVersions.cs` |

##### Implementation changes

```csharp
// BuildingBlocks/Concurrency/IHasVersions.cs
public interface IHasVersions
{
    // entity name → (entity id → expected row version)
    // e.g. {"Task": {"2": 3}, "TimeLog": {"1": 19}}
    IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions { get; }
}
```

##### Rule changes
MUST:
- Keys are stable business string names — never C# type names or namespace-qualified names
- Used only on update and patch commands — never on create or delete commands

MUST NOT:
- Use C# `Type` as the dictionary key — breaks when entities are renamed

---

#### IEntityVersionResolver (created)

##### Goal
- Decouple `ConcurrencyBehavior` from concrete entity types by mapping string names to C# types at runtime
- Allow `ConcurrencyBehavior` to resolve `IReadRepository<TEntity>` from DI without knowing entity types at compile time

##### Core Principals
- Single method: `Resolve(string entityName) → Type?`
- Returns `null` for unknown entity names — `ConcurrencyBehavior` returns `Result.Error` on null
- Implementation in App.Infrastructure — BuildingBlocks owns only the interface

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Entity name to type resolver | `IEntityVersionResolver` | `IEntityVersionResolver` | `IEntityVersionResolver.cs` | `IEntityVersionResolver.cs` |

##### Implementation changes

```csharp
// BuildingBlocks/Concurrency/IEntityVersionResolver.cs
public interface IEntityVersionResolver
{
    Type? Resolve(string entityName);
}
```

---

#### ETagEncoder (created)

##### Goal
- Encode a dictionary of entity versions as a base64 JSON string suitable for the HTTP `ETag` header
- Decode an `If-Match` header value back to the versions dictionary — return null on malformed input

##### Core Principals
- Static class — no instance, no DI registration needed
- Encode: `Dictionary<string, Dictionary<int, uint>>` → base64 JSON string
- Decode: base64 JSON string → `Dictionary<string, Dictionary<int, uint>>?` — null on any error
- Decode swallows exceptions and returns null — malformed ETag handled by controller as 412

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| ETag encode/decode | `ETagEncoder` | `ETagEncoder` | `ETagEncoder.cs` | `ETagEncoder.cs` |

##### Implementation changes

```csharp
// BuildingBlocks/Concurrency/ETagEncoder.cs
public static class ETagEncoder
{
    public static string Encode(
        Dictionary<string, Dictionary<int, uint>> versions)
    {
        var json = JsonSerializer.Serialize(versions);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static Dictionary<string, Dictionary<int, uint>>? Decode(string etag)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(etag));
            return JsonSerializer
                .Deserialize<Dictionary<string, Dictionary<int, uint>>>(json);
        }
        catch
        {
            // malformed ETag — controller returns 412
            return null;
        }
    }
}
```

##### Rule changes
MUST:
- `Decode` returns null on any exception — never throws
- `Encode` produces a string usable directly as an `ETag` header value (without surrounding quotes — controller adds quotes)

---

#### ConcurrencyBehavior (created)

##### Goal
- Validate all entity versions carried by an update command before the handler runs
- Return `Result.Conflict` immediately on any version mismatch — handler never executes for stale updates
- Run after `ValidationBehavior` and before `UnitOfWorkBehavior` — stale commands never open a unit of work

##### Core Principals
- Constrained on `where TRequest : IHasVersions` — only activates for commands that carry versions
- Resolves `IReadRepository<TEntity>` from DI dynamically using `IServiceProvider` — entity type known only at runtime
- Loads each entity by Id using a `ByIdSpec` — returns `Result.NotFound` if entity missing during version check
- Compares loaded `entity.Version` against `expectedVersion` from command — returns `Result.Conflict` on mismatch
- Checks all entities before deciding — first mismatch short-circuits entire command
- Does not call `SaveChangesAsync` — purely a read and guard operation

##### Pipeline position
```
ValidationBehavior      ← solution 10 — rejects invalid input
    ↓
ConcurrencyBehavior     ← this solution — rejects stale versions
    ↓
UnitOfWorkBehavior      ← solution 11 — commits on success
    ↓
Handler
```

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Concurrency pipeline behavior | `ConcurrencyBehavior<TRequest, TResponse>` | `ConcurrencyBehavior<UpdateTaskCommand, Result>` | `ConcurrencyBehavior.cs` | `ConcurrencyBehavior.cs` |

##### Implementation changes

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
                return (TResponse)Result.Error($"Unknown entity type: '{entityName}'.");

            // resolve IReadRepository<TEntity> from DI at runtime
            var repoType = typeof(IReadRepository<>).MakeGenericType(entityType);
            var repo = _serviceProvider.GetRequiredService(repoType);

            foreach (var (id, expectedVersion) in idVersions)
            {
                // load entity — uses ByIdSpec resolved dynamically
                var entity = await LoadEntityAsync(repo, entityType, id, ct);

                if (entity is null)
                    return (TResponse)Result.NotFound();

                // compare versions — mismatch means client has stale data
                var actualVersion = (uint)entityType
                    .GetProperty(nameof(IVersioned.Version))!
                    .GetValue(entity)!;

                if (actualVersion != expectedVersion)
                    return (TResponse)Result.Conflict(
                        $"'{entityName}' with Id {id} was modified by another user. " +
                        $"Expected version {expectedVersion}, found {actualVersion}.");
            }
        }

        return await next();
    }

    private static async Task<object?> LoadEntityAsync(
        object repo, Type entityType, int id, CancellationToken ct)
    {
        // invoke FirstOrDefaultAsync via reflection — entity type known only at runtime
        var method = repo.GetType()
            .GetMethod(nameof(IReadRepository<object>.FirstOrDefaultAsync),
                new[] { typeof(ISpecification<>).MakeGenericType(entityType),
                        typeof(CancellationToken) })!;

        var specType = typeof(EntityByIdSpec<>).MakeGenericType(entityType);
        var spec = Activator.CreateInstance(specType, id)!;

        var task = (Task)method.Invoke(repo, new[] { spec, ct })!;
        await task.ConfigureAwait(false);

        return ((dynamic)task).Result;
    }
}
```

> **Note on `IVersioned`:** Entities accessed by `ConcurrencyBehavior` must expose `Version` via a shared interface or the behavior uses reflection. A clean alternative is to define `IVersioned` in Shared:
> ```csharp
> // Shared/Concurrency/IVersioned.cs
> public interface IVersioned
> {
>     uint Version { get; }
> }
> ```
> All mutable entities implement `IVersioned`. `ConcurrencyBehavior` casts loaded entities to `IVersioned` instead of using reflection. This is the recommended approach.

##### Rule changes
MUST:
- Constrained to `where TRequest : IHasVersions` and `where TResponse : IResult`
- Returns `Result.Conflict` on version mismatch — handler never runs
- Returns `Result.NotFound` if entity missing during version check
- Returns `Result.Error` for unknown entity name
- Never calls `SaveChangesAsync`

MUST NOT:
- Activate on commands without `IHasVersions` — only update/patch commands carry versions
- Modify any entity state during version check

---

## App.Infrastructure (.csproj) (extended)

### Project extension

#### Goal
- Own `EntityVersionResolver` — the mapping from stable string entity names to C# entity types

#### Structure

##### Project Structure
```
/App.Infrastructure
  /Concurrency
    EntityVersionResolver.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Concurrency/EntityVersionResolver.cs | Maps string entity names to C# types for ConcurrencyBehavior | EntityVersionResolver.class.skill |

#### Rules
MUST:
- Every mutable entity type registered in `EntityVersionResolver`
- Keys are stable business string names — same strings used in `IHasVersions` commands and ETag encoding

MUST NOT:
- Keys be C# type names, namespaces, or assembly-qualified names — breaks when entities are renamed

---

### Class extension

#### EntityVersionResolver (created)

##### Goal
- Provide the concrete mapping from stable entity name strings to C# entity types
- Be the single place to update when new mutable entities are added to the solution

##### Core Principals
- Static readonly dictionary — populated at startup, no runtime modification
- Keys are stable business names agreed with the frontend — changing a key is a breaking API change
- Returns `null` for unknown names — `ConcurrencyBehavior` returns `Result.Error` on null

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Entity name resolver | `EntityVersionResolver` | `EntityVersionResolver` | `EntityVersionResolver.cs` | `EntityVersionResolver.cs` |

##### Implementation changes

```csharp
// App.Infrastructure/Concurrency/EntityVersionResolver.cs
public class EntityVersionResolver : IEntityVersionResolver
{
    private static readonly Dictionary<string, Type> _map = new()
    {
        ["Task"]    = typeof(TodoTask),
        ["TimeLog"] = typeof(TimeEntry),
        ["User"]    = typeof(User),
        // add new mutable entity types here when introduced
    };

    public Type? Resolve(string entityName)
        => _map.GetValueOrDefault(entityName);
}
```

##### Rule changes
MUST:
- Every mutable entity registered in `_map`
- Keys match the entity name strings used in `IHasVersions` command properties and `ETagEncoder.Encode` calls
- Registered as `Singleton` in DI — static map, no request-scope state

---

## {Module}.Interfaces (.csproj) (extended)

### Project extension

#### Goal
- Extend all update and patch commands with `IHasVersions` to carry client-supplied version information

---

### Class extension

#### Command.class.skill (extended)

##### Goal
- Require all update and patch commands to implement `IHasVersions`
- Make `Versions` a standard property on every command that modifies an existing entity

##### Core Principals
- `Versions` property typed as `IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>>`
- Populated by the API controller from the decoded `If-Match` header — never hardcoded
- Create and delete commands do NOT implement `IHasVersions` — only update and patch

##### Implementation changes
Update command extended with `IHasVersions`:

```csharp
// Task.Interfaces/Commands/UpdateTaskCommand.cs
public record UpdateTaskCommand(
    int TaskId,
    string Title,
    IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions
) : ICommand<Result>, IHasVersions;
```

Patch command similarly:

```csharp
// Task.Interfaces/Commands/PatchTaskCommand.cs
public record PatchTaskCommand(
    int TaskId,
    string? Title,
    IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions
) : ICommand<Result>, IHasVersions;
```

##### Rule changes
MUST:
- All update and patch commands implement both `ICommand<Result>` and `IHasVersions`
- `Versions` populated from decoded `If-Match` header in controller — never constructed in application code

MUST NOT:
- Create commands implement `IHasVersions` — new entities have no version
- Delete commands implement `IHasVersions` — deletion does not require version check in this architecture

---

## {Module}.Api (.csproj) (extended)

### Project extension

#### Goal
- Add ETag header to all GET responses for mutable entities
- Add `If-Match` header extraction, 412 guard, and `Versions` population to all PUT and PATCH endpoints

---

### Class extension

#### SingleEntityController.class.skill (extended)

##### Goal
- Encode entity `Version` into `ETag` response header on every GET for a mutable entity
- Decode `If-Match` request header and return 412 before dispatch if missing or malformed
- Populate `Versions` on the update command from the decoded `If-Match` value

##### Core Principals
- ETag format: `"<base64>"` — surrounding double quotes are part of the HTTP ETag format
- `ETagEncoder.Encode` builds the versions dictionary — entity name string must match `EntityVersionResolver` keys exactly
- If `If-Match` missing or `ETagEncoder.Decode` returns null → return `StatusCode(412)` immediately, before `_sender.Send()`
- `Versions` passed directly as command constructor argument — no manual construction in controller

##### Full ETag flow
```
GET /task/2
    ↓
Handler returns TaskDto with Version = 3
    ↓
Controller calls ETagEncoder.Encode({"Task": {"2": 3}})
    ↓
Response.Headers.ETag = "\"eyJUYXNrIjp7IjIiOjN9fQ==\""
← 200 OK + ETag header

PUT /task/2  { title: "New title" }
If-Match: "eyJUYXNrIjp7IjIiOjN9fQ=="
    ↓
Controller: ifMatch missing? → 412
Controller: ETagEncoder.Decode(ifMatch) null? → 412
Controller: versions = {"Task": {"2": 3}}
Controller: _sender.Send(new UpdateTaskCommand(2, "New title", versions))
    ↓
ConcurrencyBehavior: loads Task#2, checks Version == 3
    ↓ match
Handler: updates entity
UnitOfWorkBehavior: commits
← 204 No Content

    ↓ mismatch (Task#2 updated by another client in the meantime)
ConcurrencyBehavior: returns Result.Conflict
← 409 Conflict + ProblemDetails
```

##### Implementation changes
GET extended with ETag encoding:

```csharp
// Task.Api/Controllers/Task/SingleTaskController.cs
[HttpGet]
[ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<TaskDto>> Get(int id, CancellationToken ct)
{
    var result = await _sender.Send(new GetTaskQuery(id), ct);

    return result.Status switch
    {
        ResultStatus.Ok => BuildOkWithETag(result.Value, id),
        ResultStatus.NotFound => NotFound(
            ResultExtensions.ToProblemDetails(result.Errors)),
        ResultStatus.Error => StatusCode(
            StatusCodes.Status500InternalServerError,
            ResultExtensions.ToProblemDetails(result.Errors)),
        _ => throw new InvalidOperationException(
            $"Unexpected result status '{result.Status}' for GetTaskQuery.")
    };
}

private OkObjectResult BuildOkWithETag(TaskDto dto, int id)
{
    var etag = ETagEncoder.Encode(new()
    {
        ["Task"] = new() { [id] = dto.Version }
    });
    Response.Headers.ETag = $"\"{etag}\"";
    return Ok(dto);
}
```

PUT extended with If-Match guard and Versions population:

```csharp
[HttpPut]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
[ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> Update(
    int id,
    [FromBody] UpdateTaskRequest request,
    [FromHeader(Name = "If-Match")] string? ifMatch,
    CancellationToken ct)
{
    // 412 — If-Match not supplied or malformed
    if (string.IsNullOrEmpty(ifMatch))
        return StatusCode(StatusCodes.Status412PreconditionFailed);

    var versions = ETagEncoder.Decode(ifMatch);
    if (versions is null)
        return StatusCode(StatusCodes.Status412PreconditionFailed);

    var command = new UpdateTaskCommand(id, request.Title, versions);
    var result = await _sender.Send(command, ct);

    return result.Status switch
    {
        ResultStatus.NoContent => NoContent(),
        ResultStatus.Invalid => BadRequest(
            ResultExtensions.ToProblemDetails(result.ValidationErrors)),
        ResultStatus.NotFound => NotFound(
            ResultExtensions.ToProblemDetails(result.Errors)),
        ResultStatus.Conflict => Conflict(
            ResultExtensions.ToProblemDetails(result.Errors)),
        ResultStatus.Error => StatusCode(
            StatusCodes.Status500InternalServerError,
            ResultExtensions.ToProblemDetails(result.Errors)),
        _ => throw new InvalidOperationException(
            $"Unexpected result status '{result.Status}' for UpdateTaskCommand.")
    };
}
```

> **Note:** `TaskDto` must include `Version` as a property so the controller can encode it into the ETag:
> ```csharp
> public record TaskDto(int Id, string Title, string Status, int AssigneeId, uint Version);
> ```

##### Rule changes
MUST:
- GET for mutable entity sets `Response.Headers.ETag` with encoded versions
- PUT/PATCH checks `If-Match` presence — returns 412 immediately if missing
- PUT/PATCH calls `ETagEncoder.Decode` — returns 412 if result is null
- `Versions` passed to command from decoded `If-Match` — never constructed in controller
- 412 added to `[ProducesResponseType]` on all PUT/PATCH endpoints for mutable entities
- DTO returned by GET for mutable entity includes `Version` field

MUST NOT:
- GET for immutable entity set ETag header — immutable entities have no version
- `Versions` hardcoded or constructed in controller — always from decoded `If-Match`

---

## App.Host (.csproj) (extended)

### Project extension

#### Goal
- Register `IEntityVersionResolver` as `Singleton`
- Register `ConcurrencyBehavior` in pipeline between `ValidationBehavior` and `UnitOfWorkBehavior`

---

### Class extension

#### PipelineRegistration (extended)

##### Goal
- Insert `ConcurrencyBehavior` between `ValidationBehavior` and `UnitOfWorkBehavior`

##### Implementation changes

```csharp
// App.Host/DependencyInjection/PipelineRegistration.cs
public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(
        this IServiceCollection services)
    {
        // 1. validation — rejects invalid input before anything else
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        // 2. concurrency — rejects stale versions before unit of work opens
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ConcurrencyBehavior<,>));

        // solution 15 (external-created) inserts: GuidResolvingBehavior — between 1 and 2

        // 3. unit of work — commits staged changes after handler completes
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(UnitOfWorkBehavior<,>));

        return services;
    }
}
```

#### RepositoryRegistration (extended)

##### Goal
- Register `EntityVersionResolver` as `Singleton` alongside repository registrations

##### Implementation changes

```csharp
// App.Host/DependencyInjection/RepositoryRegistration.cs
public static class RepositoryRegistration
{
    public static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<UnitOfWorkContext>();
        services.AddSingleton<IEntityVersionResolver, EntityVersionResolver>();

        return services;
    }
}
```

##### Rule changes
MUST:
- `EntityVersionResolver` registered as `Singleton` — static map, safe for singleton lifetime
- `ConcurrencyBehavior` registered after `ValidationBehavior` and before `UnitOfWorkBehavior`

MUST NOT:
- `ConcurrencyBehavior` registered after `UnitOfWorkBehavior` — stale commands would open a unit of work

---

# Rules

MUST:
- Every mutable entity has `public uint Version { get; internal set; }`
- Every mutable entity configuration maps `Version` to `xmin` with `IsConcurrencyToken()` and `ValueGeneratedOnAddOrUpdate()`
- All update and patch commands implement `IHasVersions`
- `IHasVersions`, `IEntityVersionResolver`, `ETagEncoder`, and `ConcurrencyBehavior` live in BuildingBlocks
- `EntityVersionResolver` lives in App.Infrastructure and registers every mutable entity type
- `EntityVersionResolver` registered as `Singleton` in App.Host
- Entity name keys in `IHasVersions` and `EntityVersionResolver` are stable business strings — never C# type names
- `ConcurrencyBehavior` registered after `ValidationBehavior` and before `UnitOfWorkBehavior`
- GET responses for mutable entities include `ETag` header with encoded versions
- PUT/PATCH endpoints check `If-Match` presence — return 412 if missing or malformed
- DTOs returned by GET for mutable entities include `Version` field
- `ConcurrencyBehavior` returns `Result.Conflict` on version mismatch — never throws
- `ConcurrencyBehavior` returns `Result.NotFound` if entity missing during version check

MUST NOT:
- Immutable entities have `Version` property
- Create or delete commands implement `IHasVersions`
- Handler check versions manually — `ConcurrencyBehavior` owns this
- Controller return 400 for missing `If-Match` — 412 Precondition Failed is correct
- Entity name keys use C# type names — breaks on entity rename
- `ConcurrencyBehavior` call `SaveChangesAsync`

# Anti-patterns
- `Version` as plain `uint` on command property instead of `IHasVersions` — does not scale to multi-entity updates
- Handler catches `DbUpdateConcurrencyException` and returns conflict — `ConcurrencyBehavior` should catch this earlier at the application level
- ETag encoding only primary entity version — misses secondary entity conflicts when command touches multiple entities
- `ConcurrencyBehavior` registered after `UnitOfWorkBehavior` — stale commands open a unit of work unnecessarily
- `EntityVersionResolver` key using `nameof(TodoTask)` — fragile, breaks on class rename; use a stable business string constant

# Check list
- [ ] `uint Version { get; internal set; }` on every mutable entity
- [ ] `Version` mapped to `xmin` with `IsConcurrencyToken()` and `ValueGeneratedOnAddOrUpdate()` in entity configuration
- [ ] `IHasVersions` defined in `BuildingBlocks/Concurrency/IHasVersions.cs`
- [ ] `IEntityVersionResolver` defined in `BuildingBlocks/Concurrency/IEntityVersionResolver.cs`
- [ ] `ETagEncoder` defined in `BuildingBlocks/Concurrency/ETagEncoder.cs`
- [ ] `ConcurrencyBehavior` defined in `BuildingBlocks/MediatR/ConcurrencyBehavior.cs`
- [ ] `EntityVersionResolver` defined in `App.Infrastructure/Concurrency/EntityVersionResolver.cs`
- [ ] Every mutable entity registered in `EntityVersionResolver._map`
- [ ] `EntityVersionResolver` registered as `Singleton` in App.Host
- [ ] `ConcurrencyBehavior` registered between `ValidationBehavior` and `UnitOfWorkBehavior`
- [ ] All update and patch commands implement `IHasVersions`
- [ ] GET for mutable entity sets `Response.Headers.ETag`
- [ ] DTO for mutable entity includes `Version` field
- [ ] PUT/PATCH checks `If-Match` — returns 412 if missing or malformed
- [ ] 412 added to `[ProducesResponseType]` on all PUT/PATCH actions
- [ ] `switch` default arm throws `InvalidOperationException` in PUT/PATCH actions

# Unittest TestCases
- [ ] When entity saved Then `Version` (xmin) is non-zero
- [ ] When entity updated Then `Version` changes
- [ ] When two DbContexts load same entity, first saves, second saves Then `DbUpdateConcurrencyException` thrown
- [ ] When `ETagEncoder.Encode` called Then produces valid base64 string
- [ ] When `ETagEncoder.Decode` called with valid ETag Then returns correct versions dictionary
- [ ] When `ETagEncoder.Decode` called with malformed string Then returns null
- [ ] When `If-Match` header missing Then controller returns 412 before MediatR dispatch
- [ ] When `If-Match` header malformed Then controller returns 412 before MediatR dispatch
- [ ] When version matches Then `ConcurrencyBehavior` calls next — handler runs
- [ ] When version mismatches Then `ConcurrencyBehavior` returns `Result.Conflict` — handler does not run
- [ ] When entity not found during version check Then `ConcurrencyBehavior` returns `Result.NotFound`
- [ ] When command has multiple entities and one mismatches Then `Result.Conflict` without updating any
- [ ] When unknown entity name in `IHasVersions` Then `ConcurrencyBehavior` returns `Result.Error`
