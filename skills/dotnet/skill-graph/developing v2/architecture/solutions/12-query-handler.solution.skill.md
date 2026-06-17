---
uid: 1a6c3e8f-4d2b-4f9a-b7e5-c3f8a1d6e2b9
order: 12
name: query-handler
description: Defines IQuery<TResponse> in Shared, Query record and DTO declarations in Interfaces, single-module QueryHandler in Application using IReadRepository and specs, cross-module QueryHandler in App.Queries using DbContext directly, and App.Queries DI registration in App.Host
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
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
  - cross-module read
creates:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Shared csproj/classes/IQuery.class.skill|IQuery.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Interfaces csproj/classes/Query.class.skill|Query.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Interfaces csproj/classes/Dto.class.skill|Dto.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/classes/QueryHandler.class.skill|QueryHandler.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Queries csproj/classes/CrossModuleQueryHandler.class.skill|CrossModuleQueryHandler.class.skill]]"
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Shared csproj/Shared.csproj.skill|Shared.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Interfaces csproj/{Module}.Interfaces.csproj.skill|{Module}.Interfaces.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/{Module}.Application.csproj.skill|{Module}.Application.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Queries csproj/App.Queries.csproj.skill|App.Queries.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Host csproj/App.Host.csproj.skill|App.Host.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/classes/ModuleApplicationRegistration.class.skill|ModuleApplicationRegistration.class.skill]]"
depends_on:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill|01-module-boundary.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill|02-solution-layer-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/07-ardalis-specification.solution.skill|07-ardalis-specification.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/08-repository.solution.skill|08-repository.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/09-command-handler.solution.skill|09-command-handler.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/11-unit-of-work.solution.skill|11-unit-of-work.solution.skill]]"
---

# Goal
- Define `IQuery<TResponse>` in Shared as the marker that identifies read-only operations and excludes them from write-side pipeline behaviors
- Define where Queries and DTOs are declared — as records in `{Module}.Interfaces/Queries` and `{Module}.Interfaces/DTOs`
- Define two handler locations: single-module handlers in `{Module}.Application/Features` using `IReadRepository<T>` and specs, cross-module handlers in `App.Queries` using DbContext directly
- Define when to use projection spec vs in-handler mapping — projection spec for flat DTOs, in-handler mapping for computed or conditional DTOs
- Register App.Queries handlers via assembly scan in App.Host

# Core Principles
- `IQuery<TResponse>` lives in Shared — same as `ICommand`, consistent placement of all MediatR markers
- Query handlers are strictly read-only — no entity mutation, no `SaveChangesAsync`, no `IRepository<T>`
- `UnitOfWorkBehavior` does not activate for queries — `IQuery` marker excludes them by not implementing `ICommand`
- `ValidationBehavior` does not activate for queries — constrained to `ICommand` only (solution 10)
- Single-module handlers use `IReadRepository<T>` from Shared — never DbContext, never `IRepository<T>`
- Cross-module handlers live in `App.Queries` — the only layer with access to all module entity types simultaneously
- Cross-module handlers use DbContext directly with `AsNoTracking()` — no repository abstraction needed here
- All single-module entity loading goes through named specs from solution 07 — no inline LINQ in handlers
- DTOs are the only data shape that crosses module boundaries — never domain entities
- No validator for query handlers — queries are read-only, input does not mutate state

# Depend on solutions
- [[01-module-boundary.solution.skill]] — defines Shared, `{Module}.Interfaces`, `{Module}.Application`, and App.Queries boundaries
- [[02-solution-layer-structure.solution.skill]] — defines dependency rules: App.Queries references Shared and all module Domains
- [[07-ardalis-specification.solution.skill]] — single-module handlers load data via named specs
- [[08-repository.solution.skill]] — `IReadRepository<T>` in Shared used by single-module handlers
- [[09-command-handler.solution.skill]] — `ICommand` marker in Shared; `IQuery` is its read-side counterpart
- [[11-unit-of-work.solution.skill]] — `UnitOfWorkBehavior` constrained to `ICommand`; `IQuery` excludes queries from commit pipeline

# Requirements
- `MediatR` NuGet package — provides `IRequest<T>`, `IRequestHandler<TRequest, TResponse>`, `ISender`
- `Ardalis.Result` NuGet package — provides `Result<T>`, `Result.Success`, `Result.NotFound`
- `Microsoft.EntityFrameworkCore` NuGet package — provides `DbContext`, `AsNoTracking`, LINQ extensions used in App.Queries

# Template Skill Mutations

## Shared (.csproj) (extended)

### Project extension

#### Goal
- Own the `IQuery<TResponse>` marker interface — the read-side counterpart to `ICommand<TResponse>`

#### Core Principals
- Marker interface only — no properties, no methods, no logic
- Lives in Shared alongside `ICommand` — consistent placement, every layer can reference it
- Not implementing `ICommand` is what excludes queries from `ValidationBehavior` and `UnitOfWorkBehavior`

#### Structure

##### Project Structure
```
/Shared
  /Repositories
    IReadRepository.cs    ← solution 08
    IRepository.cs        ← solution 08
  /MediatR
    ICommand.cs           ← solution 09
    IQuery.cs
  /UnitOfWork
    IUnitOfWork.cs        ← solution 11
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /MediatR/IQuery.cs | Read-only operation marker interface | IQuery.class.skill |

#### NuGet Packages
| Package | Purpose |
| --- | --- |
| `MediatR` | Provides `IRequest<T>` that `IQuery<T>` extends |

#### Rules
MUST:
- `IQuery<TResponse>` defined in Shared — not BuildingBlocks, not any module
- Placed alongside `ICommand` in `/Shared/MediatR/`

MUST NOT:
- `IQuery` extend `ICommand` — queries must be excluded from all write-side pipeline behaviors

---

### Class extension

#### IQuery (created)

##### Goal
- Mark a MediatR request as a read-only operation
- Exclude the request from `ValidationBehavior` and `UnitOfWorkBehavior` by not implementing `ICommand`

##### Core Principals
- Interface only — no properties, no methods
- Extends `IRequest<TResponse>` so MediatR routes it to a handler
- Does NOT extend `ICommand` — this is the mechanism that excludes it from write-side behaviors

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Query marker | `IQuery<TResponse>` | `IQuery<Result<TaskDto>>` | `IQuery.cs` | `IQuery.cs` |

##### Implementation changes

```csharp
// Shared/MediatR/IQuery.cs
public interface IQuery<TResponse> : IRequest<TResponse> { }
```

##### Rule changes
MUST:
- All query records implement `IQuery<Result<T>>` — not `IRequest<T>` directly
- Defined in Shared — never in BuildingBlocks or any module project

MUST NOT:
- Extend `ICommand` or `ICommand<TResponse>`

---

## {Module}.Interfaces (.csproj) (extended)

### Project extension

#### Goal
- Own all Query record declarations and DTO response shapes for this module
- Be the contract surface other modules and the API layer use to request data from this module

#### Core Principals
- Queries are declarations only — records with input properties, no methods, no logic
- DTOs are read-only response shapes — records, no domain entity references
- Both declared in Interfaces so other modules can dispatch queries without depending on Application or Domain
- Cross-module queries are also declared here — implemented in App.Queries, declared in the owning module's Interfaces

#### Structure

##### Project Structure
```
/{Module}.Interfaces
  /Commands        ← solution 09
  /Queries
    Get{Entity}Query.cs
    Get{Entities}Query.cs
    Get{Entity}With{Related}Query.cs
  /DTOs
    {Entity}Dto.cs
    {Entity}SummaryDto.cs
    {Entity}With{Related}Dto.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Queries | Read intent contract declarations for this module | Query.class.skill |
| /DTOs | Response shape declarations consumed by query handlers and API | Dto.class.skill |

#### Rules
MUST:
- All queries for this module declared in `/{Module}.Interfaces/Queries`
- All DTOs for this module declared in `/{Module}.Interfaces/DTOs`
- Cross-module query contracts declared in the owning module's Interfaces — implemented in App.Queries

MUST NOT:
- Queries contain any logic or methods
- DTOs expose domain entity types — projection shapes only
- DTOs have public setters — declared as `record` for immutability

---

### Class extension

#### Query (created)

##### Goal
- Express a named read intent as an immutable record that carries all filter/selection input needed for the operation
- Implement `IQuery<Result<T>>` so MediatR routes it to the correct handler and excludes it from write-side behaviors

##### Core Principals
- Declared as `record` — immutable, structural equality
- Implements `IQuery<Result<T>>` — return type is always `Result<T>` or `Result<IReadOnlyList<T>>`
- Input properties are primitives — no domain entity references
- One query per read intent — `GetTaskQuery`, `GetTasksQuery`, `GetTaskWithUserDetailsQuery`

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Get single entity | `Get{Entity}Query` | `GetTaskQuery` | `Get{Entity}Query.cs` | `GetTaskQuery.cs` |
| Get collection | `Get{Entities}Query` | `GetTasksQuery` | `Get{Entities}Query.cs` | `GetTasksQuery.cs` |
| Cross-module JOIN | `Get{Entity}With{Related}Query` | `GetTaskWithUserDetailsQuery` | `Get{Entity}With{Related}Query.cs` | `GetTaskWithUserDetailsQuery.cs` |

##### Implementation changes

```csharp
// Task.Interfaces/Queries/GetTaskQuery.cs
public record GetTaskQuery(int Id) : IQuery<Result<TaskDto>>;
```

```csharp
// Task.Interfaces/Queries/GetTasksQuery.cs
public record GetTasksQuery(int AssigneeId) : IQuery<Result<IReadOnlyList<TaskSummaryDto>>>;
```

```csharp
// Task.Interfaces/Queries/GetTaskWithUserDetailsQuery.cs
public record GetTaskWithUserDetailsQuery(int TaskId)
    : IQuery<Result<TaskWithUserDetailsDto>>;
```

##### Rule changes
MUST:
- Declared as `record`
- Implement `IQuery<Result<T>>` — never `IRequest<T>` directly
- Properties are primitives or simple types — no domain entity references

MUST NOT:
- Contain methods or logic
- Reference domain entity types as properties

---

#### Dto (created)

##### Goal
- Define the response shape returned by a query handler — a flat, read-only projection of domain data
- Be the only data shape that crosses module and layer boundaries for read operations

##### Core Principals
- Declared as `record` — immutable, structural equality
- Properties are primitives or other DTOs — never domain entity types
- Declared in Interfaces alongside the query that returns it
- One DTO per distinct response shape — `TaskDto` for full detail, `TaskSummaryDto` for list items

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Full entity detail | `{Entity}Dto` | `TaskDto` | `{Entity}Dto.cs` | `TaskDto.cs` |
| List item / summary | `{Entity}SummaryDto` | `TaskSummaryDto` | `{Entity}SummaryDto.cs` | `TaskSummaryDto.cs` |
| Cross-module projection | `{Entity}With{Related}Dto` | `TaskWithUserDetailsDto` | `{Entity}With{Related}Dto.cs` | `TaskWithUserDetailsDto.cs` |

##### Implementation changes

```csharp
// Task.Interfaces/DTOs/TaskDto.cs
public record TaskDto(
    int Id,
    string Title,
    string Status,
    int AssigneeId);
```

```csharp
// Task.Interfaces/DTOs/TaskSummaryDto.cs
public record TaskSummaryDto(
    int Id,
    string Title,
    string Status);
```

```csharp
// Task.Interfaces/DTOs/TaskWithUserDetailsDto.cs
public record TaskWithUserDetailsDto(
    int Id,
    string Title,
    string AssigneeName,
    string AssigneeEmail);
```

##### Rule changes
MUST:
- Declared as `record`
- Properties are primitives or other DTOs
- Declared in `/{Module}.Interfaces/DTOs`

MUST NOT:
- Expose domain entity types as properties
- Have public setters — `record` provides immutability

---

## {Module}.Application (.csproj) (extended)

### Project extension

#### Goal
- Own single-module query handler implementations in `/Features` alongside command handlers
- No validator alongside query handlers — queries are read-only

#### Core Principals
- Query handlers co-located with command handlers under `/Features` — one folder per feature
- Handler file named `{FeatureName}.Handler.cs`, class named `{FeatureName}Handler` — same convention as commands
- No `.Validator.cs` file alongside query handlers — queries are never validated
- Single-module handlers only — cross-module JOIN handlers live in App.Queries

#### Structure

##### Project Structure
```
/{Module}.Application
  /Features
    /CreateTask
      CreateTask.Handler.cs     ← command handler
      CreateTask.Validator.cs   ← command validator
    /GetTask
      GetTask.Handler.cs        ← query handler — no validator
    /GetTasks
      GetTasks.Handler.cs       ← query handler — no validator
```

#### Rules
MUST:
- Single-module query handlers live in `/{Module}.Application/Features/{FeatureName}`
- Handler file named `{FeatureName}.Handler.cs`, class named `{FeatureName}Handler`
- Query handlers registered via `AddMediatR` assembly scan in module registration — same scan as command handlers
- Query handlers inject `IReadRepository<T>` — never `IRepository<T>` or DbContext

MUST NOT:
- Query handlers have a paired `.Validator.cs` file
- Cross-module JOIN handlers live here — belongs in App.Queries

---

### Class extension

#### QueryHandler (created)

##### Goal
- Fetch and project data for a single module's read operation
- Never modify state — return typed Result with DTO

##### Core Principals
- Implements `IRequestHandler<TQuery, Result<T>>`
- Injects `IReadRepository<T>` from Shared — signals read-only intent at type level
- Two implementation shapes depending on DTO complexity:
  - **Projection via spec** — when DTO maps directly from entity fields, use `Specification<T, TDto>` and `ListAsync`
  - **Load then map in handler** — when DTO requires computed fields, conditional logic, or nested mapping
- All entity loading uses named specs from solution 07 — no inline LINQ
- Returns `Result.NotFound()` when entity is missing — never returns null or empty DTO

##### When to use projection spec vs in-handler mapping

| Scenario | Shape | Example |
| --- | --- | --- |
| DTO maps 1:1 from entity fields | Projection spec `Specification<T, TDto>` | `TaskSummarySpec` → `ListAsync` |
| DTO needs `.ToString()`, enum label, or simple transform | In-handler mapping | load entity, map manually |
| DTO has nested structure or conditional fields | In-handler mapping | load entity, compose DTO |
| Collection with filter + ordering | Projection spec | `ActiveTasksByAssigneeSpec` → `ListAsync` |

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Query handler | `{FeatureName}Handler` | `GetTaskHandler` | `{FeatureName}.Handler.cs` | `GetTask.Handler.cs` |

##### Implementation changes

Simple — projection via spec (DTO maps directly from entity fields):

```csharp
// Task.Application/Features/GetTasks/GetTasks.Handler.cs
public class GetTasksHandler
    : IRequestHandler<GetTasksQuery, Result<IReadOnlyList<TaskSummaryDto>>>
{
    private readonly IReadRepository<TodoTask> _repository;

    public GetTasksHandler(IReadRepository<TodoTask> repository)
        => _repository = repository;

    public async Task<Result<IReadOnlyList<TaskSummaryDto>>> Handle(
        GetTasksQuery query, CancellationToken ct)
    {
        // projection spec — DTO built inside the spec, AsNoTracking applied by repository
        var results = await _repository.ListAsync(
            new TaskSummarySpec(query.AssigneeId), ct);

        return Result.Success<IReadOnlyList<TaskSummaryDto>>(results);
    }
}
```

Complex — load entity then map in handler (DTO requires non-trivial mapping):

```csharp
// Task.Application/Features/GetTask/GetTask.Handler.cs
public class GetTaskHandler
    : IRequestHandler<GetTaskQuery, Result<TaskDto>>
{
    private readonly IReadRepository<TodoTask> _repository;

    public GetTaskHandler(IReadRepository<TodoTask> repository)
        => _repository = repository;

    public async Task<Result<TaskDto>> Handle(
        GetTaskQuery query, CancellationToken ct)
    {
        // load via named spec — never inline LINQ
        var task = await _repository.FirstOrDefaultAsync(
            new TaskByIdSpec(query.Id), ct);

        if (task is null)
            return Result.NotFound();

        // map in handler — DTO requires Status.ToString() conversion
        var dto = new TaskDto(
            task.Id,
            task.Title,
            task.Status.ToString(),
            task.AssigneeId);

        return Result.Success(dto);
    }
}
```

##### Rule changes
MUST:
- Implement `IRequestHandler<TQuery, Result<T>>`
- Inject `IReadRepository<T>` — never `IRepository<T>` or DbContext
- Load entities via named specs — never inline LINQ
- Return `Result.NotFound()` when entity is missing
- Return `Result<T>` for all outcomes — no exceptions for flow control

MUST NOT:
- Modify any entity state
- Call `SaveChangesAsync` or inject `IUnitOfWork`
- Dispatch commands
- Use inline LINQ — all filtering goes through named specs

---

## App.Queries (.csproj) (extended)

### Project extension

#### Goal
- Own cross-module JOIN query handler implementations — the only layer permitted to JOIN across module entity types
- Use DbContext directly with `AsNoTracking()` — no repository abstraction needed for cross-module reads

#### Core Principals
- App.Queries references all module Domain projects — it is the only layer that may do this simultaneously
- Cross-module handlers use DbContext directly — `IReadRepository<T>` is per-entity-type and cannot span a JOIN
- `AsNoTracking()` applied on every query — read-only, no tracking overhead
- Query contract declared in `{Module}.Interfaces/Queries` — App.Queries only implements it, never declares it
- Handlers registered in App.Host via a dedicated `App.Queries` assembly scan — not inside any module registration

#### Structure

##### Project Structure
```
/App.Queries
  /Queries
    /{ModuleName}
      Get{Entity}With{Related}Handler.cs
  /Specifications
    {Entity}With{Related}Spec.cs        ← cross-module projection specs (solution 07)
  AppQueriesRegistration.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Queries/{ModuleName} | Cross-module query handler implementations | CrossModuleQueryHandler.class.skill |
| /Specifications | Cross-module projection specs | solution 07 |
| AppQueriesRegistration.cs | DI registration for App.Queries assembly | |

#### NuGet Packages
| Package | Purpose |
| --- | --- |
| `MediatR` | Provides `IRequestHandler<TRequest, TResponse>` |
| `Microsoft.EntityFrameworkCore` | Provides `DbContext`, `AsNoTracking()`, LINQ async extensions |
| `Ardalis.Result` | Provides `Result<T>`, `Result.NotFound` |

#### Rules
MUST:
- All cross-module JOIN handlers live in `/App.Queries/Queries/{ModuleName}/`
- Handlers use DbContext directly with `AsNoTracking()`
- Handlers registered via assembly scan in App.Host
- Query contract declared in owning module's Interfaces — App.Queries only implements

MUST NOT:
- Single-module queries live here — belongs in `{Module}.Application`
- App.Queries handlers modify entity state
- App.Queries handlers call `SaveChangesAsync`

#### Anti-patterns
- Single-module query handler placed in App.Queries — adds unnecessary cross-module machinery
- Cross-module handler placed in `{Module}.Application` — Application has no access to other module's entity types

---

### Class extension

#### CrossModuleQueryHandler (created)

##### Goal
- Implement a query that requires data from multiple module entity types in a single database query
- Use direct DbContext LINQ projection — the most efficient path for cross-module reads

##### Core Principals
- Implements `IRequestHandler<TQuery, Result<T>>`
- Injects `AppDbContext` directly — cross-module JOIN cannot be expressed through single-entity `IReadRepository<T>`
- Always applies `AsNoTracking()` — read-only operation, no tracking overhead
- Uses LINQ projection (`.Select(...)`) directly in handler — or delegates to a cross-module projection spec from solution 07
- Returns `Result.NotFound()` when entity is missing

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Cross-module query handler | `Get{Entity}With{Related}Handler` | `GetTaskWithUserDetailsHandler` | `Get{Entity}With{Related}Handler.cs` | `GetTaskWithUserDetailsHandler.cs` |

##### Implementation changes

Inline LINQ projection (acceptable in App.Queries — no per-module repo abstraction applies here):

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
            .AsNoTracking()
            .Where(t => t.Id == query.TaskId)
            .Select(t => new TaskWithUserDetailsDto(
                t.Id,
                t.Title,
                t.Assignee.FullName,
                t.Assignee.Email))
            .FirstOrDefaultAsync(ct);

        return result is null
            ? Result.NotFound()
            : Result.Success(result);
    }
}
```

Alternative — delegate to cross-module projection spec from solution 07:

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
        var result = await SpecificationEvaluator.Default
            .GetQuery(
                _dbContext.Set<TodoTask>().AsNoTracking(),
                new TaskWithUserDetailsSpec(query.TaskId))
            .FirstOrDefaultAsync(ct);

        return result is null
            ? Result.NotFound()
            : Result.Success(result);
    }
}
```

##### Rule changes
MUST:
- Implement `IRequestHandler<TQuery, Result<T>>`
- Inject `AppDbContext` directly
- Apply `AsNoTracking()` on all queries
- Return `Result.NotFound()` when entity is missing
- Live in `/App.Queries/Queries/{ModuleName}/`

MUST NOT:
- Modify entity state
- Call `SaveChangesAsync`
- Dispatch commands

---

### Class extension

#### AppQueriesRegistration (created)

##### Goal
- Register all cross-module query handlers via assembly scan
- Called from App.Host — App.Queries does not self-register

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| App.Queries DI registration | `AppQueriesRegistration` | `AppQueriesRegistration` | `AppQueriesRegistration.cs` | `AppQueriesRegistration.cs` |

##### Implementation changes

```csharp
// App.Queries/AppQueriesRegistration.cs
public static class AppQueriesRegistration
{
    public static IServiceCollection RegisterAppQueries(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(AppQueriesRegistration).Assembly));

        return services;
    }
}
```

##### Rule changes
MUST:
- Register handlers via `AddMediatR` assembly scan
- Called from App.Host — not from any module registration

---

## {Module}.Application — ModuleApplicationRegistration (extended)

### Class extension

#### ModuleApplicationRegistration (extended)

##### Goal
- Confirm that query handlers in `{Module}.Application` are discovered by the same `AddMediatR` assembly scan already registered in solution 09 — no additional registration needed

##### Core Principals
- `AddMediatR` scans the entire Application assembly — it discovers both `IRequestHandler<TCommand, ...>` and `IRequestHandler<TQuery, ...>` implementations automatically
- No separate registration step for query handlers — the module registration from solution 09 already covers them

##### Implementation changes
No change to `{ModuleName}ApplicationRegistration.cs` — existing `AddMediatR` scan covers query handlers:

```csharp
// Task.Application/TaskApplicationRegistration.cs  (unchanged from solution 09)
public static class TaskApplicationRegistration
{
    public static IServiceCollection RegisterTaskModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // discovers all IRequestHandler implementations — commands AND queries
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(TaskApplicationRegistration).Assembly));

        services.AddValidatorsFromAssembly(
            typeof(TaskApplicationRegistration).Assembly);

        return services;
    }
}
```

##### Rule changes
MUST:
- Confirm no separate query handler registration is needed — `AddMediatR` scan covers both

---

## App.Host (.csproj) (extended)

### Project extension

#### Goal
- Add `RegisterAppQueries()` call to the composition root alongside module registrations

#### Rules
MUST:
- `RegisterAppQueries()` called from App.Host Program.cs
- Called after all module registrations — App.Queries depends on module entity types being registered

---

### Class extension

#### Program.cs (extended)

##### Goal
- Wire App.Queries registration into the composition root

##### Implementation changes

```csharp
// App.Host/Program.cs
builder.Services
    .AddPipeline()
    .AddRepositories()
    .RegisterTaskModule(builder.Configuration)
    .RegisterTimeLogModule(builder.Configuration)
    .RegisterUserModule(builder.Configuration)
    .RegisterAppQueries();              // ← cross-module query handlers
```

---

# Rules

MUST:
- `IQuery<TResponse>` defined in Shared — not BuildingBlocks, not any module
- All queries implement `IQuery<Result<T>>` — not `IRequest<T>` directly
- Queries declared as `record` in `/{Module}.Interfaces/Queries`
- DTOs declared as `record` in `/{Module}.Interfaces/DTOs`
- Single-module handlers in `/{Module}.Application/Features` — inject `IReadRepository<T>`
- Single-module handlers load via named specs — no inline LINQ
- Cross-module handlers in `/App.Queries/Queries/{ModuleName}` — inject DbContext directly
- Cross-module handlers apply `AsNoTracking()` on all queries
- App.Queries handlers registered via `RegisterAppQueries()` assembly scan in App.Host
- Query handlers return `Result.NotFound()` when entity is missing

MUST NOT:
- Query handler inject `IRepository<T>` — signals write intent, use `IReadRepository<T>`
- Query handler inject `IUnitOfWork` or call `SaveChangesAsync`
- Query handler modify entity state or dispatch commands
- Single-module handler use DbContext directly — use `IReadRepository<T>`
- Cross-module handler live in `{Module}.Application` — Application has no multi-module DB access
- DTOs expose domain entity types
- Query handlers have paired validators — queries are read-only
- `IQuery` extend `ICommand` — must be excluded from write-side pipeline behaviors

SHOULD:
- Use projection spec when DTO maps directly from entity fields — avoids loading full entity
- Use in-handler mapping when DTO requires computed fields, conditional logic, or nested structure

# Anti-patterns
- `IRepository<T>` injected into query handler — use `IReadRepository<T>`
- Cross-module JOIN in `{Module}.Application` — Application has no cross-module DB access
- Single-module query implemented in App.Queries — unnecessary cross-module machinery
- Inline LINQ in single-module handler — use named spec
- DTO returning domain entity directly — always project to DTO record
- Query handler dispatching a command — queries are read-only

# Check list
- [ ] `IQuery<TResponse>` defined in `Shared/MediatR/IQuery.cs`
- [ ] `IQuery` does not extend `ICommand`
- [ ] All queries declared as `record` implementing `IQuery<Result<T>>`
- [ ] All queries in `/{Module}.Interfaces/Queries`
- [ ] All DTOs declared as `record` in `/{Module}.Interfaces/DTOs`
- [ ] DTOs have no domain entity type properties
- [ ] Single-module handlers in `/{Module}.Application/Features/{FeatureName}`
- [ ] Single-module handlers inject `IReadRepository<T>` — never `IRepository<T>` or DbContext
- [ ] Single-module handlers load via named specs — no inline LINQ
- [ ] Cross-module handlers in `/App.Queries/Queries/{ModuleName}`
- [ ] Cross-module handlers inject `AppDbContext` directly
- [ ] Cross-module handlers apply `AsNoTracking()`
- [ ] `AppQueriesRegistration` defined in App.Queries
- [ ] `RegisterAppQueries()` called from App.Host Program.cs
- [ ] No validator paired with any query handler
- [ ] Query handlers return `Result.NotFound()` when entity is missing
- [ ] No `SaveChangesAsync` call in any query handler

# Unittest TestCases
- [ ] When entity exists Then single-module handler returns `Result.Success` with correct DTO fields
- [ ] When entity not found Then single-module handler returns `Result.NotFound`
- [ ] When collection query runs Then all matching entities returned as DTOs
- [ ] When projection spec used Then DTO fields correctly mapped without loading full entity
- [ ] When in-handler mapping used Then computed fields correctly populated in DTO
- [ ] When cross-module query runs Then data from both modules correctly joined in single DTO
- [ ] When cross-module entity not found Then handler returns `Result.NotFound`
- [ ] When query is dispatched Then `UnitOfWorkBehavior` does not activate
- [ ] When query is dispatched Then `ValidationBehavior` does not activate
- [ ] When App.Queries assembly scanned Then all cross-module handlers discovered by MediatR
