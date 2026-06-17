---
uid: 6d1f3a9c-2e8b-4d7f-c5a1-b4e7f2c9a3d6
order: 15
name: external-created-entity
description: Defines the full external-created entity stack — Guid property and unique index on entity, IHasGuid and IGuidResolver in BuildingBlocks, ConflictException in Shared, GuidResolvingBehavior in BuildingBlocks inserted before ConcurrencyBehavior, EntityByGuidSpec in Domain, GuidResolver implementation in Application, and controller catching ConflictException to return 409 with existing entity body
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - application
  - infrastructure
  - guid
  - idempotency
  - mediatr
  - pipeline
triggers:
  - external created entity
  - client-generated guid
  - idempotent create
  - async creation
  - prevent duplicate creation
  - GuidResolvingBehavior
  - IHasGuid
creates:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Shared csproj/classes/ConflictException.class.skill|ConflictException.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/IHasGuid.class.skill|IHasGuid.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/IGuidResolver.class.skill|IGuidResolver.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/GuidResolvingBehavior.class.skill|GuidResolvingBehavior.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/classes/GuidResolver.class.skill|GuidResolver.class.skill]]"
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Shared csproj/Shared.csproj.skill|Shared.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/BuildingBlocks.csproj.skill|BuildingBlocks.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Host csproj/App.Host.csproj.skill|App.Host.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Entity.class.skill|Entity.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/EntityConfiguration.class.skill|EntityConfiguration.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Specification.class.skill|Specification.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Interfaces csproj/classes/Command.class.skill|Command.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/{Module}.Application.csproj.skill|{Module}.Application.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/classes/ModuleApplicationRegistration.class.skill|ModuleApplicationRegistration.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Api csproj/classes/CollectionController.class.skill|CollectionController.class.skill]]"
depends_on:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill|01-module-boundary.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill|02-solution-layer-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill|03-domain-configuration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/07-ardalis-specification.solution.skill|07-ardalis-specification.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/08-repository.solution.skill|08-repository.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/09-command-handler.solution.skill|09-command-handler.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/10-validation.solution.skill|10-validation.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/11-unit-of-work.solution.skill|11-unit-of-work.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/13-api-structure.solution.skill|13-api-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/14-entity-concurrency.solution.skill|14-entity-concurrency.solution.skill]]"
---

# Goal
- Define `Guid` as an immutable correlation property on externally-created entities — set once on creation, never changed
- Define a unique database index on `Guid` as the final idempotency guard — duplicate requests that bypass the pipeline are rejected at the DB level
- Define `IHasGuid` and `IGuidResolver<TResult>` in BuildingBlocks as the command marker and resolver contract
- Define `GuidResolvingBehavior` in BuildingBlocks as the pipeline behavior that short-circuits with `ConflictException<TResult>` when the Guid already exists
- Define `ConflictException<T>` in Shared — carries the existing entity result so the controller can return 409 with a body the client can use to recover without a second GET
- Define `{Entity}ByGuidSpec` in `{Module}.Domain/Specifications` — the spec used by the resolver
- Define `Create{Entity}GuidResolver` in `{Module}.Application/Resolvers` — one resolver per external-created entity type
- Insert `GuidResolvingBehavior` into the pipeline between `ValidationBehavior` and `ConcurrencyBehavior`

# Core Principles
- External system (frontend, partner API) generates the Guid — the backend never generates it for external creation flows
- Guid is a correlation handle only — never used in domain logic, never exposed as a foreign key, never used in routing after creation
- Internal `int Id` is the only identity used inside the domain after the entity is created
- 409 response body contains the existing entity result — client recovers without a second GET request
- `GuidResolvingBehavior` is generic — one implementation handles all entity types via `IGuidResolver<TResult>` resolved from DI
- `IGuidResolver<TResult>` is NOT registered as open generic — each external-created entity type registers its own concrete resolver
- `ConflictException<T>` is thrown by the behavior — the controller catches it and maps to 409
- The unique database index on `Guid` is the last line of defence — it catches duplicate Guids that bypass the pipeline (e.g. concurrent requests that both pass the pipeline check simultaneously)
- `GuidResolvingBehavior` runs after `ValidationBehavior` — invalid commands are rejected before the DB lookup
- `GuidResolvingBehavior` runs before `ConcurrencyBehavior` — duplicate creation is caught before any version check

# Full idempotent creation flow
```
Client generates Guid (e.g. frontend NgRx optimistic store)
    ↓
POST /task { guid, title, assigneeId }
    ↓
ValidationBehavior — rejects invalid input (solution 10)
    ↓
GuidResolvingBehavior — checks if entity with Guid already exists
    ↓ (not found — first request)
ConcurrencyBehavior — no IHasVersions on create commands, skips (solution 14)
    ↓
UnitOfWorkBehavior — depth++ (solution 11)
    ↓
Handler — creates entity, stores Guid
    ↓
UnitOfWorkBehavior — SaveChangesAsync (depth == 1)
← 201 Created { id: 42 }
Client stores internal Id

    ↓ (Guid already exists — retry after transient failure)
GuidResolvingBehavior — resolver finds existing entity by Guid
GuidResolvingBehavior — throws ConflictException<Result<CreateTaskResult>>
    ↓
Controller catch (ConflictException<Result<CreateTaskResult>> ex)
← 409 Conflict { id: 42 }  ← same Id, no duplicate created
Client recovers from optimistic state using returned Id

    ↓ (two concurrent requests with same Guid both pass pipeline simultaneously)
UnitOfWorkBehavior — SaveChangesAsync
DB unique index on Guid raises PostgresException (SqlState 23505)
← 500 (or mapped 409 via exception middleware with constraint name check)
```

# Depend on solutions
- [[01-module-boundary.solution.skill]] — defines Shared, BuildingBlocks, App.Host, and module project boundaries
- [[02-solution-layer-structure.solution.skill]] — Shared has no dependencies; BuildingBlocks references Shared; Application references Shared + BuildingBlocks
- [[03-domain-configuration.solution.skill]] — unique index on `Guid` follows the EF configuration pattern with named constant
- [[07-ardalis-specification.solution.skill]] — `{Entity}ByGuidSpec` is a Domain spec used by the resolver
- [[08-repository.solution.skill]] — `IReadRepository<T>` from Shared used by `GuidResolver` to look up entity by Guid
- [[09-command-handler.solution.skill]] — create commands extended with `IHasGuid`
- [[10-validation.solution.skill]] — `ValidationBehavior` runs before `GuidResolvingBehavior` — pipeline order dependency
- [[11-unit-of-work.solution.skill]] — `UnitOfWorkBehavior` runs after `GuidResolvingBehavior`
- [[13-api-structure.solution.skill]] — `CollectionController` extended with try/catch for `ConflictException<T>` on POST
- [[14-entity-concurrency.solution.skill]] — `ConcurrencyBehavior` runs after `GuidResolvingBehavior` — pipeline order dependency

# Requirements
- No additional NuGet packages — all dependencies already present from earlier solutions

# Template Skill Mutations

## Shared (.csproj) (extended)

### Project extension

#### Goal
- Own `ConflictException<T>` — the exception thrown by `GuidResolvingBehavior` that carries the existing entity result to the controller

#### Core Principals
- Lives in Shared — accessible by both BuildingBlocks (thrown) and Api layer (caught)
- Generic on the result type — carries `TResult` so the controller can return the existing entity body typed correctly
- Not a domain exception — it is a pipeline coordination exception

#### Structure

##### Project Structure
```
/Shared
  /Repositories
    IReadRepository.cs    ← solution 08
    IRepository.cs        ← solution 08
  /MediatR
    ICommand.cs           ← solution 09
    IQuery.cs             ← solution 12
  /UnitOfWork
    IUnitOfWork.cs        ← solution 11
  /Concurrency
    IVersioned.cs         ← solution 14 (recommended)
  /Exceptions
    ConflictException.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Exceptions/ConflictException.cs | Exception carrying existing entity result for 409 responses | ConflictException.class.skill |

#### Rules
MUST:
- `ConflictException<T>` defined in Shared — accessible by BuildingBlocks (throw) and Api (catch)

MUST NOT:
- `ConflictException<T>` defined in BuildingBlocks — Api layer must not reference BuildingBlocks directly

---

### Class extension

#### ConflictException\<T\> (created)

##### Goal
- Carry the existing entity result from `GuidResolvingBehavior` to the API controller
- Enable the controller to return the existing entity in the 409 response body — client recovers without a second GET

##### Core Principals
- Generic on result type `T` — typed to the command's result type (e.g. `Result<CreateTaskResult>`)
- Single property: `Existing` — the resolved result from `IGuidResolver<T>`
- Controller catches this specific type and extracts `Existing.Value` for the 409 body

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid conflict exception | `ConflictException<T>` | `ConflictException<Result<CreateTaskResult>>` | `ConflictException.cs` | `ConflictException.cs` |

##### Implementation changes

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

##### Rule changes
MUST:
- `Existing` property carries the full resolved result — never just an Id
- Message always the same — controller never reads the message, only `Existing`

---

## BuildingBlocks (.csproj) (extended)

### Project extension

#### Goal
- Own `IHasGuid`, `IGuidResolver<TResult>`, and `GuidResolvingBehavior` — the Guid pipeline contract and enforcement

#### Structure

##### Project Structure
```
/BuildingBlocks
  /MediatR
    ValidationBehavior.cs      ← solution 10
    GuidResolvingBehavior.cs
    ConcurrencyBehavior.cs     ← solution 14
    UnitOfWorkContext.cs       ← solution 11
    UnitOfWorkBehavior.cs      ← solution 11
  /Guid
    IHasGuid.cs
    IGuidResolver.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Guid/IHasGuid.cs | Marker interface for commands carrying a client-generated Guid | IHasGuid.class.skill |
| /Guid/IGuidResolver.cs | Per-entity resolver contract — checks if Guid already exists | IGuidResolver.class.skill |
| /MediatR/GuidResolvingBehavior.cs | Pipeline behavior that short-circuits on duplicate Guid | GuidResolvingBehavior.class.skill |

#### Rules
MUST:
- `IHasGuid`, `IGuidResolver<TResult>`, `GuidResolvingBehavior` defined in BuildingBlocks
- `GuidResolvingBehavior` throws `ConflictException<TResponse>` from Shared — never returns a result directly

MUST NOT:
- `GuidResolvingBehavior` registered as open generic — DI resolves `IGuidResolver<TResult>` per concrete command result type

---

### Class extension

#### IHasGuid (created)

##### Goal
- Mark a create command as carrying a client-generated Guid
- Opt the command into `GuidResolvingBehavior` — non-Guid commands are unaffected

##### Core Principals
- Single property: `Guid Guid { get; }`
- Implemented by create commands for externally-created entity types only
- Not implemented by update, delete, or internal-create commands

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid carrier marker | `IHasGuid` | `IHasGuid` | `IHasGuid.cs` | `IHasGuid.cs` |

##### Implementation changes

```csharp
// BuildingBlocks/Guid/IHasGuid.cs
public interface IHasGuid
{
    Guid Guid { get; }
}
```

##### Rule changes
MUST:
- Only create commands for externally-created entities implement `IHasGuid`

MUST NOT:
- Update, delete, or internally-created entity commands implement `IHasGuid`

---

#### IGuidResolver\<TResult\> (created)

##### Goal
- Define the per-entity contract for checking whether a Guid already exists and returning the existing result
- Keep `GuidResolvingBehavior` generic — each entity type provides its own resolver implementation

##### Core Principals
- Generic on `TResult` — matches the command's result type exactly
- Returns `TResult?` — null means Guid not found (first request), non-null means already exists (retry)
- One implementation per external-created entity type — registered in module DI registration
- Uses `IReadRepository<T>` and a `{Entity}ByGuidSpec` — never hits the DB directly

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid resolver interface | `IGuidResolver<TResult>` | `IGuidResolver<Result<CreateTaskResult>>` | `IGuidResolver.cs` | `IGuidResolver.cs` |

##### Implementation changes

```csharp
// BuildingBlocks/Guid/IGuidResolver.cs
public interface IGuidResolver<TResult>
{
    Task<TResult?> ResolveAsync(Guid guid, CancellationToken ct);
}
```

##### Rule changes
MUST:
- `TResult` matches the command's return type exactly — same type as the command's `ICommand<TResult>`
- Returns null when Guid not found — never throws
- Returns existing result when Guid found — `GuidResolvingBehavior` throws on non-null

---

#### GuidResolvingBehavior (created)

##### Goal
- Intercept any create command implementing `IHasGuid` and check whether the Guid already exists
- Short-circuit with `ConflictException<TResponse>` if the entity already exists — handler never runs for duplicate requests
- Pass through to the next behavior if Guid not found — first request proceeds normally

##### Core Principals
- Constrained on `where TRequest : IHasGuid` — only activates for commands carrying a Guid
- Resolves `IGuidResolver<TResponse>` from DI — the resolver is specific to the command's result type
- Throws `ConflictException<TResponse>` on duplicate — never returns a result directly from the behavior
- The exception carries the full existing result — controller extracts `.Existing.Value` for the 409 body
- Does not call `SaveChangesAsync` — purely a read and guard operation

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid resolving behavior | `GuidResolvingBehavior<TRequest, TResponse>` | `GuidResolvingBehavior<CreateTaskCommand, Result<CreateTaskResult>>` | `GuidResolvingBehavior.cs` | `GuidResolvingBehavior.cs` |

##### Implementation changes

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

##### Rule changes
MUST:
- Constrained to `where TRequest : IHasGuid`
- Throw `ConflictException<TResponse>` when resolver returns non-null — never return a result directly
- Pass through (`return await next()`) when resolver returns null

MUST NOT:
- Be registered as open generic — DI resolves per concrete `TRequest`/`TResponse` pair
- Call `SaveChangesAsync`
- Swallow the `ConflictException` — it must propagate to the controller

---

## {Module}.Domain (.csproj) (extended)

### Project extension

#### Goal
- Add `Guid` as an immutable property on externally-created entities
- Add unique database index on `Guid` as the final idempotency guard

---

### Class extension

#### Entity.class.skill (extended)

##### Goal
- Add `Guid` as a required immutable property on External Immutable and External Mutable entity types
- Keep `Guid` strictly as a correlation handle — never used in domain logic, domain events, or relationships

##### Core Principals
- `Guid` declared with `internal set` — set once during entity creation factory method, never changed
- Entity creation factory method receives `Guid` as a parameter — it is the caller's responsibility to supply the client-generated value
- No domain method ever reads `Guid` after creation — only the resolver and the entity factory use it

##### Implementation changes
External-created entity must declare `Guid` with `internal set`:

```csharp
// Task.Domain/Entities/TodoTask.cs
public class TodoTask
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }    // ← added by this solution
    public string Title { get; internal set; }
    public TaskStatus Status { get; internal set; }
    public int AssigneeId { get; internal set; }
    public uint Version { get; internal set; }  // ← solution 14 (if mutable)

    // factory method receives client-generated Guid
    public static TodoTask Create(Guid guid, string title, int assigneeId)
        => new()
        {
            Guid = guid,
            Title = title,
            AssigneeId = assigneeId,
            Status = TaskStatus.Open
        };
}
```

##### Rule changes
MUST:
- `Guid` declared as `public Guid Guid { get; internal set; }`
- Set exactly once in the entity factory method — never reassigned
- Present on External Immutable and External Mutable entity types only

MUST NOT:
- `Guid` used in domain logic, domain events, or as a foreign key in relationships
- `Guid` reassigned after entity creation
- Internal entity types (no external creation) have `Guid`

---

#### EntityConfiguration.class.skill (extended)

##### Goal
- Configure a unique database index on `Guid` as the DB-level idempotency guard
- Define the index name as a `public static string` constant for use in test assertions

##### Core Principals
- Index name follows the convention: `UX_{TableName}_Guid`
- Unique index ensures concurrent requests that both pass the pipeline check are rejected at the DB level
- Constant name `UX_Guid` used in integration tests to assert the correct constraint name in `PostgresException`

##### Implementation changes

```csharp
// Task.Domain/Configurations/TodoTaskConfig.cs
public class TodoTaskConfig : IEntityTypeConfiguration<TodoTask>
{
    public static string TableName = nameof(TodoTask);
    public static string UX_Guid = $"UX_{TableName}_Guid";

    public void Configure(EntityTypeBuilder<TodoTask> builder)
    {
        // unique index — DB-level idempotency guard
        builder
            .HasIndex(e => e.Guid)
            .IsUnique()
            .HasDatabaseName(UX_Guid);

        // Version concurrency token — solution 14 (if mutable)
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
- `UX_Guid` defined as `public static string` on the config class
- `HasDatabaseName(UX_Guid)` used — never inline string
- `IsUnique()` on the `Guid` index

---

#### Specification.class.skill (extended)

##### Goal
- Add `{Entity}ByGuidSpec` as a required Domain spec for every external-created entity type
- Used by `GuidResolver` to look up the entity by its client-generated Guid

##### Core Principals
- Single-condition spec — filters by `Guid` property only
- Lives in `{Module}.Domain/Specifications` — reusable across resolver and any feature that needs Guid lookup
- Follows naming convention from solution 07: `{Entity}ByGuidSpec`

##### Implementation changes

```csharp
// Task.Domain/Specifications/TaskByGuidSpec.cs
public class TaskByGuidSpec : Specification<TodoTask>
{
    public TaskByGuidSpec(Guid guid)
    {
        Query.Where(t => t.Guid == guid);
    }
}
```

##### Rule changes
MUST:
- Every external-created entity type has a `{Entity}ByGuidSpec` in `/{Module}.Domain/Specifications`
- Used only by `GuidResolver` and any feature that explicitly needs Guid-based lookup

---

## {Module}.Interfaces (.csproj) (extended)

### Project extension

#### Goal
- Extend create commands for externally-created entity types with `IHasGuid`

---

### Class extension

#### Command.class.skill (extended)

##### Goal
- Add `Guid` as a required property on create commands for externally-created entity types
- Implement `IHasGuid` alongside `ICommand<Result<T>>`

##### Core Principals
- `Guid` is the first property — signals to the reader that this is an external-created entity
- Command carries the client-generated Guid — never a server-generated value
- Result record unchanged from solution 09 — still just `{Entity}Id`

##### Implementation changes
Create command extended with `Guid` and `IHasGuid`:

```csharp
// Task.Interfaces/Commands/CreateTaskCommand.cs
public record CreateTaskCommand(
    Guid Guid,         // ← client-generated, first property
    string Title,
    int AssigneeId
) : ICommand<Result<CreateTaskResult>>, IHasGuid;

public record CreateTaskResult(int Id);
```

##### Rule changes
MUST:
- `Guid` is the first property on the command record
- Command implements both `ICommand<Result<T>>` and `IHasGuid`
- `Guid` typed as `System.Guid` — never `string` or `int`

MUST NOT:
- Update, delete, or internal-create commands implement `IHasGuid`

---

## {Module}.Application (.csproj) (extended)

### Project extension

#### Goal
- Own `Create{Entity}GuidResolver` implementations in `/Resolvers` — one per external-created entity type
- Register each resolver in the module DI registration

#### Structure

##### Project Structure
```
/{Module}.Application
  /Features
    /CreateTask
      CreateTask.Handler.cs
      CreateTask.Validator.cs
  /Resolvers
    Create{Entity}GuidResolver.cs
  {Module}ApplicationRegistration.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Resolvers/Create{Entity}GuidResolver.cs | Per-entity IGuidResolver implementation | GuidResolver.class.skill |

#### Rules
MUST:
- One `GuidResolver` per external-created entity type in `/{Module}.Application/Resolvers`
- Each resolver registered in module DI registration

MUST NOT:
- Resolver implemented in Domain — resolver uses `IReadRepository<T>`, which belongs in Application

---

### Class extension

#### GuidResolver (created)

##### Goal
- Implement `IGuidResolver<TResult>` for one specific external-created entity type
- Look up the entity by Guid using `IReadRepository<T>` and `{Entity}ByGuidSpec`
- Return the existing result if found, null if not found

##### Core Principals
- Implements `IGuidResolver<Result<Create{Entity}Result>>`
- Injects `IReadRepository<T>` from Shared — read-only lookup
- Uses `{Entity}ByGuidSpec` from Domain — no inline LINQ
- Maps found entity to the command result type — same shape the handler would return on success
- Returns null when not found — `GuidResolvingBehavior` proceeds to handler on null

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid resolver implementation | `Create{Entity}GuidResolver` | `CreateTaskGuidResolver` | `Create{Entity}GuidResolver.cs` | `CreateTaskGuidResolver.cs` |

##### Implementation changes

```csharp
// Task.Application/Resolvers/CreateTaskGuidResolver.cs
public class CreateTaskGuidResolver
    : IGuidResolver<Result<CreateTaskResult>>
{
    private readonly IReadRepository<TodoTask> _repository;

    public CreateTaskGuidResolver(IReadRepository<TodoTask> repository)
        => _repository = repository;

    public async Task<Result<CreateTaskResult>?> ResolveAsync(
        Guid guid, CancellationToken ct)
    {
        var task = await _repository.FirstOrDefaultAsync(
            new TaskByGuidSpec(guid), ct);

        // null — Guid not found, first request, handler should run
        if (task is null)
            return null;

        // non-null — Guid already exists, return existing result
        // GuidResolvingBehavior will throw ConflictException with this value
        return Result.Success(new CreateTaskResult(task.Id));
    }
}
```

##### Rule changes
MUST:
- Return null when entity not found — never throw
- Return `Result.Success(new Create{Entity}Result(...))` when entity found — same shape as handler success
- Inject `IReadRepository<T>` — never `IRepository<T>` or DbContext
- Use `{Entity}ByGuidSpec` — never inline LINQ

MUST NOT:
- Throw exceptions — null signals not found, non-null signals exists
- Return `Result.NotFound()` — null is the "not found" signal in this contract

---

#### ModuleApplicationRegistration.class.skill (extended)

##### Goal
- Register each `GuidResolver` in the module's DI registration

##### Implementation changes
Module registration extended with `IGuidResolver` registrations:

```csharp
// Task.Application/TaskApplicationRegistration.cs
public static class TaskApplicationRegistration
{
    public static IServiceCollection RegisterTaskModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(TaskApplicationRegistration).Assembly));

        services.AddValidatorsFromAssembly(
            typeof(TaskApplicationRegistration).Assembly);

        // one registration per external-created entity type in this module
        services.AddScoped<
            IGuidResolver<Result<CreateTaskResult>>,
            CreateTaskGuidResolver>();

        return services;
    }
}
```

##### Rule changes
MUST:
- Each `IGuidResolver<TResult>` registered explicitly as `Scoped` — not auto-scanned
- One registration per external-created entity type

MUST NOT:
- `IGuidResolver` registrations omitted — `GuidResolvingBehavior` will throw at runtime if resolver not found

---

## {Module}.Api (.csproj) (extended)

### Project extension

#### Goal
- Extend collection controller POST actions to catch `ConflictException<Result<T>>` and return 409 with existing entity body

---

### Class extension

#### CollectionController.class.skill (extended)

##### Goal
- Wrap `ISender.Send()` call in a try/catch for `ConflictException<Result<T>>` on POST create actions
- Return 409 with the existing entity body from `ex.Existing.Value` — client recovers without a second GET

##### Core Principals
- `try/catch` wraps only the `_sender.Send()` call and its result switch — not the entire action
- Catches `ConflictException<Result<Create{Entity}Result>>` — typed to the specific command result
- Returns `Conflict(ex.Existing.Value)` — the existing entity body, not a `ProblemDetails`
- `ConflictException` is only possible from `GuidResolvingBehavior` — no other pipeline component throws it
- `[ProducesResponseType]` for 409 must use the entity result type — not `ProblemDetails` — because the body is the existing entity

##### Implementation changes
POST action extended with ConflictException catch:

```csharp
// Task.Api/Controllers/Task/TaskController.cs
[HttpPost]
[ProducesResponseType(typeof(CreateTaskResult), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(CreateTaskResult), StatusCodes.Status409Conflict)]  // ← entity body, not ProblemDetails
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
            ResultStatus.Invalid => BadRequest(
                ResultExtensions.ToProblemDetails(result.ValidationErrors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for CreateTaskCommand.")
        };
    }
    catch (ConflictException<Result<CreateTaskResult>> ex)
    {
        // duplicate Guid — entity already exists
        // return existing entity body so client can recover without a second GET
        return Conflict(ex.Existing.Value);
    }
}
```

##### Rule changes
MUST:
- `try/catch (ConflictException<Result<Create{Entity}Result>> ex)` wraps the `_sender.Send()` call
- 409 `[ProducesResponseType]` uses `typeof(Create{Entity}Result)` — not `typeof(ProblemDetails)`
- `Conflict(ex.Existing.Value)` returns the entity body — not `Conflict(new ProblemDetails(...))`

MUST NOT:
- Catch `ConflictException` with a generic `Exception` catch — must be typed to the specific result type
- Return empty 409 body — client must receive the existing entity to recover

---

## App.Host (.csproj) (extended)

### Project extension

#### Goal
- Register `GuidResolvingBehavior` in the pipeline between `ValidationBehavior` and `ConcurrencyBehavior`

#### Rules
MUST:
- `GuidResolvingBehavior` registered after `ValidationBehavior` and before `ConcurrencyBehavior`
- Registered as `Transient` open generic — DI resolves `IGuidResolver<TResponse>` per command type

---

### Class extension

#### PipelineRegistration (extended)

##### Goal
- Insert `GuidResolvingBehavior` as the second behavior — after `ValidationBehavior`, before `ConcurrencyBehavior`

##### Final pipeline order after all solutions applied

```
1. ValidationBehavior      ← solution 10 — rejects invalid input
2. GuidResolvingBehavior   ← solution 15 — rejects duplicate Guid (create only)
3. ConcurrencyBehavior     ← solution 14 — rejects stale versions (update only)
4. UnitOfWorkBehavior      ← solution 11 — commits after handler
```

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

        // 2. guid resolving — rejects duplicate Guid on create commands
        //    only activates for commands implementing IHasGuid
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(GuidResolvingBehavior<,>));

        // 3. concurrency — rejects stale versions on update commands
        //    only activates for commands implementing IHasVersions
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ConcurrencyBehavior<,>));

        // 4. unit of work — commits staged changes after handler completes
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(UnitOfWorkBehavior<,>));

        return services;
    }
}
```

##### Rule changes
MUST:
- `GuidResolvingBehavior` registered after `ValidationBehavior` — invalid commands rejected before DB lookup
- `GuidResolvingBehavior` registered before `ConcurrencyBehavior` — duplicate creation caught before version check
- `GuidResolvingBehavior` registered before `UnitOfWorkBehavior` — duplicate commands never open a unit of work

---

# Rules

MUST:
- External-created entities have `public Guid Guid { get; internal set; }`
- `Guid` set exactly once in the entity factory method — never reassigned
- Unique index on `Guid` configured with named constant `UX_Guid` in entity configuration
- `{Entity}ByGuidSpec` defined in `/{Module}.Domain/Specifications`
- `IHasGuid`, `IGuidResolver<TResult>`, `GuidResolvingBehavior` defined in BuildingBlocks
- `ConflictException<T>` defined in Shared
- Create commands for external-created entities implement both `ICommand<Result<T>>` and `IHasGuid`
- One `Create{Entity}GuidResolver` per external-created entity type in `/{Module}.Application/Resolvers`
- Each `IGuidResolver<TResult>` registered as `Scoped` in module DI registration
- `GuidResolvingBehavior` registered after `ValidationBehavior` and before `ConcurrencyBehavior`
- Controller POST action wraps `_sender.Send()` in `try/catch (ConflictException<Result<T>> ex)`
- 409 response body is `ex.Existing.Value` — the existing entity, not `ProblemDetails`
- `Guid` is first property in create command record

MUST NOT:
- Guid used in domain logic, domain events, relationships, or routes after creation
- Guid regenerated or changed after entity creation
- Update, delete, or internal-create commands implement `IHasGuid`
- `IGuidResolver` registered as open generic — each entity type registers its own concrete resolver
- `GuidResolvingBehavior` registered after `UnitOfWorkBehavior`
- Resolver throw exceptions — null means not found, non-null means exists
- 409 response return empty body — client must receive existing entity to recover

SHOULD:
- `Guid` be the first property in the command record — signals external-created entity at a glance

# Anti-patterns
- Handler checks for duplicate Guid manually — duplicates pipeline logic, not reusable
- 409 returns `ProblemDetails` instead of existing entity — client forced to make a second GET to recover
- `IGuidResolver` implemented in Domain — resolver uses `IReadRepository<T>`, belongs in Application
- `GuidResolvingBehavior` registered after `UnitOfWorkBehavior` — duplicate commands open a unit of work
- `IGuidResolver` registered as open generic — breaks DI resolution per command result type
- `Guid` used as foreign key in a relation — leaks external identity into domain relationships
- `Guid` route parameter after creation — internal `Id` is the only identity in routes

# Check list
- [ ] `Guid Guid { get; internal set; }` on every external-created entity
- [ ] `Guid` set in entity factory method — never reassigned
- [ ] `UX_Guid` constant defined on entity configuration class
- [ ] Unique index on `Guid` configured with `HasDatabaseName(UX_Guid)` and `IsUnique()`
- [ ] `{Entity}ByGuidSpec` in `/{Module}.Domain/Specifications`
- [ ] `IHasGuid` defined in `BuildingBlocks/Guid/IHasGuid.cs`
- [ ] `IGuidResolver<TResult>` defined in `BuildingBlocks/Guid/IGuidResolver.cs`
- [ ] `GuidResolvingBehavior` defined in `BuildingBlocks/MediatR/GuidResolvingBehavior.cs`
- [ ] `ConflictException<T>` defined in `Shared/Exceptions/ConflictException.cs`
- [ ] `Create{Entity}GuidResolver` in `/{Module}.Application/Resolvers`
- [ ] Resolver uses `IReadRepository<T>` and `{Entity}ByGuidSpec` — no inline LINQ
- [ ] Resolver returns null when not found, `Result.Success(...)` when found
- [ ] `IGuidResolver<Result<Create{Entity}Result>>` registered as `Scoped` in module registration
- [ ] Create command implements `ICommand<Result<T>>` and `IHasGuid`
- [ ] `Guid` is first property in create command record
- [ ] `GuidResolvingBehavior` registered 2nd in pipeline (after Validation, before Concurrency)
- [ ] Controller POST catches `ConflictException<Result<Create{Entity}Result>>`
- [ ] 409 `[ProducesResponseType]` uses `typeof(Create{Entity}Result)` — not `ProblemDetails`
- [ ] 409 response returns `Conflict(ex.Existing.Value)` — existing entity body

# Unittest TestCases
- [ ] When create command with new Guid Then resolver returns null — handler runs — 201 Created returned
- [ ] When create command with duplicate Guid Then resolver returns existing — `ConflictException` thrown — 409 returned
- [ ] When 409 returned Then response body contains existing entity Id — not empty, not ProblemDetails
- [ ] When two concurrent requests with same Guid both pass pipeline Then unique index raises `DbUpdateException` with `PostgresException` where `SqlState == "23505"` and `ConstraintName == TodoTaskConfig.UX_Guid`
- [ ] When entity created Then `Guid` is immutable — update attempt has no effect on Guid property
- [ ] When `GuidResolvingBehavior` registered before `UnitOfWorkBehavior` Then duplicate command never calls `SaveChangesAsync`
