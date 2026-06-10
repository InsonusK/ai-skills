---
uid: 9c4e2d1f-3a7b-4f8e-b5d2-c6e9f3a2b8d5
order: 8
name: repository
description: Defines IReadRepository<T> and IRepository<T> abstractions in BuildingBlocks and the generic Repository<T> EF Core implementation in App.Infrastructure — decoupling handlers from DbContext and powering all entity queries via Ardalis Specifications
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - application
  - infrastructure
  - repository
  - ardalis
triggers:
  - define repository
  - add data access abstraction
  - IRepository usage
  - IReadRepository usage
  - load entity in handler
creates:
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/IReadRepository.class.skill|IReadRepository.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/IRepository.class.skill|IRepository.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Infrastructure csproj/classes/Repository.class.skill|Repository.class.skill]]"
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/BuildingBlocks.csproj.skill|BuildingBlocks.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Infrastructure csproj/App.Infrastructure.csproj.skill|App.Infrastructure.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Host csproj/App.Host.csproj.skill|App.Host.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/{Module}.Application.csproj.skill|{Module}.Application.csproj.skill]]"
depends_on:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill|01-module-boundary.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill|02-solution-layer-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/07-ardalis-specification.solution.skill|07-ardalis-specification.solution.skill]]"
---
>[!todo] Исправить solution
>Переделать, чтобы использовалась реализация Ardalis а не самописка
# Goal
- Decouple Application layer handlers from EF Core and DbContext by providing repository abstractions
- Define `IReadRepository<T>` as the read-only access contract used by query handlers and idempotency checks
- Define `IRepository<T>` as the write-staging contract used by command handlers — deliberately without `SaveChangesAsync`
- Provide a single generic `Repository<T>` implementation that covers all entity types — no per-entity repository class needed
- Ensure all entity queries go through Ardalis Specifications — no raw LINQ in handlers

# Core Principles
- Application layer never references DbContext — only `IReadRepository<T>` and `IRepository<T>`
- `IRepository<T>` stages changes in the EF tracker — it never commits
- `IReadRepository<T>` is strictly read-only — no write methods, no SaveChanges
- One generic `Repository<T>` implementation serves all entity types — registered once in DI
- All read queries accept `ISpecification<T>` — no raw LINQ parameters on repository methods
- Read queries use `AsNoTracking()` — EF change tracking is wasted overhead on reads
- `SaveChangesAsync` is intentionally absent from both interfaces — committing is the responsibility of the Unit of Work (solution 11)

# Depend on solutions
- [[01-module-boundary.solution.skill]] — defines BuildingBlocks, App.Infrastructure, and App.Host project boundaries
- [[02-solution-layer-structure.solution.skill]] — defines the layer structure these projects belong to
- [[07-ardalis-specification.solution.skill]] — `ISpecification<T>` is the parameter type for all repository read methods

# Requirements
- Ardalis.Specification NuGet package — provides `ISpecification<T>`, `Specification<T>`, `Specification<T, TResult>`
- Ardalis.Specification.EntityFrameworkCore NuGet package — provides `SpecificationEvaluator` used in `Repository<T>`

# Template Skill Mutations

## Shared (.csproj) (extended)

### Project extension

#### Goal
- Own the `IReadRepository<T>` and `IRepository<T>` interface definitions
- Make repository abstractions available to every layer — Domain, Application, Infrastructure — without any layer coupling to another

#### Core Principals
- Interfaces only — no EF Core, no DbContext reference in BuildingBlocks
- Both interfaces accept `ISpecification<T>` — raw LINQ never crosses the repository boundary
- Shared has no project dependencies — everything can reference it safely
#### Structure

##### Project Structure
```
/Shared
  /Repositories
    IReadRepository.cs
    IRepository.cs
```

##### Directory and class skills
| `Directory\|file`                | Description                       | Pattern skill               |
| -------------------------------- | --------------------------------- | --------------------------- |
| /Repositories/IReadRepository.cs | Read-only repository contract     | IReadRepository.class.skill |
| /Repositories/IRepository.cs     | Write-staging repository contract | IRepository.class.skill     |

#### NuGet Packages
| Package | Purpose |
| --- | --- |
| `Ardalis.Specification` | Provides `ISpecification<T>` used as parameter type in repository methods |

#### Rules
MUST:
- `IReadRepository<T>` and `IRepository<T>` defined in BuildingBlocks
- Both interfaces accept only `ISpecification<T>` parameters for queries — never raw predicates

MUST NOT:
- BuildingBlocks reference EF Core beyond the Ardalis.Specification package
- `IRepository<T>` expose `SaveChangesAsync` — committing belongs to Unit of Work (solution 11)

---

### Class extension

#### IReadRepository\<T\> (created)

##### Goal
- Provide a read-only data access contract for query handlers and idempotency checks
- Signal read intent at the type level — injecting `IReadRepository<T>` means no writes will occur

##### Core Principals
- All methods are async and accept `CancellationToken`
- All query methods accept `ISpecification<T>` or `ISpecification<T, TResult>` — no raw LINQ
- No write methods — not even `SaveChangesAsync`
- `AsNoTracking()` applied in the implementation — callers never need to specify it

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Read-only repository interface | `IReadRepository<T>` | `IReadRepository<TodoTask>` | `IReadRepository.cs` | `IReadRepository.cs` |

##### Implementation changes
`IReadRepository<T>` must define all read operations accepting specifications:

```csharp
// BuildingBlocks/Repositories/IReadRepository.cs
public interface IReadRepository<T> where T : class
{
    Task<T?> FirstOrDefaultAsync(
        ISpecification<T> spec,
        CancellationToken ct = default);

    Task<TResult?> FirstOrDefaultAsync<TResult>(
        ISpecification<T, TResult> spec,
        CancellationToken ct = default);

    Task<List<T>> ListAsync(
        ISpecification<T> spec,
        CancellationToken ct = default);

    Task<List<TResult>> ListAsync<TResult>(
        ISpecification<T, TResult> spec,
        CancellationToken ct = default);

    Task<bool> AnyAsync(
        ISpecification<T> spec,
        CancellationToken ct = default);

    Task<int> CountAsync(
        ISpecification<T> spec,
        CancellationToken ct = default);
}
```

##### Rule changes
MUST:
- All methods accept `ISpecification<T>` or `ISpecification<T, TResult>` — no raw lambda parameters
- All methods are `async Task<T>`
- Interface has no write or commit methods

---

#### IRepository\<T\> (created)

##### Goal
- Provide a write-staging contract for command handlers
- Extend `IReadRepository<T>` so command handlers have both read and write access through one injection

##### Core Principals
- Inherits `IReadRepository<T>` — command handlers can load and stage in one injected dependency
- Write methods stage changes in the EF tracker — they do not commit
- `SaveChangesAsync` is deliberately absent — committing is the responsibility of Unit of Work (solution 11)

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Read-write repository interface | `IRepository<T>` | `IRepository<TodoTask>` | `IRepository.cs` | `IRepository.cs` |

##### Implementation changes
`IRepository<T>` must extend `IReadRepository<T>` and add write-staging methods:

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

##### Rule changes
MUST:
- Extend `IReadRepository<T>`
- Contain only write-staging methods — no commit, no SaveChanges

MUST NOT:
- Expose `SaveChangesAsync` — this is intentional and must not be added

---

## App.Infrastructure (.csproj) (extended)

### Project extension

#### Goal
- Provide the single generic `Repository<T>` EF Core implementation that satisfies both `IReadRepository<T>` and `IRepository<T>`
- Apply `AsNoTracking()` on all read queries automatically
- Use `SpecificationEvaluator` to translate Ardalis specs into EF `IQueryable` chains

#### Core Principals
- One `Repository<T>` class covers all entity types — registered once as open generic in DI
- No per-entity repository subclass — the generic implementation is sufficient for all cases
- `SpecificationEvaluator.Default.GetQuery(...)` applies the spec to the EF queryable
- References `Shared` for `IRepository<T>` and `IReadRepository<T>` interfaces
#### Structure

##### Project Structure
```
/App.Infrastructure
  /Repositories
    Repository.cs
```

##### Directory and class skills
| `Directory\|file`           | Description                               | Pattern skill          |
| --------------------------- | ----------------------------------------- | ---------------------- |
| /Repositories/Repository.cs | Generic EF Core repository implementation | Repository.class.skill |

#### NuGet Packages
| Package                                     | Purpose                                                                    |
| ------------------------------------------- | -------------------------------------------------------------------------- |
| `Ardalis.Specification.EntityFrameworkCore` | Provides `SpecificationEvaluator` for translating specs to EF `IQueryable` |
| `Microsoft.EntityFrameworkCore`             | DbContext and `Set<T>` access                                              |

#### Rules
MUST:
- Single generic `Repository<T>` — no per-entity subclasses
- All read methods apply `AsNoTracking()` via the spec evaluator
- `SpecificationEvaluator.Default.GetQuery(...)` used for all spec application

MUST NOT:
- `Repository<T>` call `SaveChangesAsync` — committing belongs to Unit of Work (solution 11)
- Per-entity repository classes be created — generic registration handles all types

#### Anti-patterns
- `TaskRepository : Repository<TodoTask>` — unnecessary subclass, generic handles all types
- `_dbContext.Tasks.Where(t => t.Id == id)` inside repository — use spec evaluator, not inline LINQ

---

### Class extension

#### Repository\<T\> (created)

##### Goal
- Implement all `IRepository<T>` and `IReadRepository<T>` methods against EF Core DbContext
- Translate Ardalis specifications into EF queryables via `SpecificationEvaluator`
- Apply `AsNoTracking()` on all read queries automatically

##### Core Principals
- Wraps `DbContext.Set<T>()` for all operations
- Read path: `SpecificationEvaluator.Default.GetQuery(dbContext.Set<T>().AsNoTracking(), spec)`
- Write path: `dbContext.Set<T>().Add/Update/Remove(entity)` — stages in EF tracker only
- Constructor receives `AppDbContext` via DI

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Generic EF repository | `Repository<T>` | `Repository<TodoTask>` (resolved by DI) | `Repository.cs` | `Repository.cs` |

##### Implementation changes
`Repository<T>` must implement all interface methods using DbContext and SpecificationEvaluator:

```csharp
// App.Infrastructure/Repositories/Repository.cs
public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _dbContext;

    public Repository(AppDbContext dbContext)
        => _dbContext = dbContext;

    // --- write staging ---

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _dbContext.Set<T>().AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        => await _dbContext.Set<T>().AddRangeAsync(entities, ct);

    public void Update(T entity)
        => _dbContext.Set<T>().Update(entity);

    public void UpdateRange(IEnumerable<T> entities)
        => _dbContext.Set<T>().UpdateRange(entities);

    public void Remove(T entity)
        => _dbContext.Set<T>().Remove(entity);

    public void RemoveRange(IEnumerable<T> entities)
        => _dbContext.Set<T>().RemoveRange(entities);

    // --- reads (AsNoTracking applied via evaluator) ---

    public async Task<T?> FirstOrDefaultAsync(
        ISpecification<T> spec, CancellationToken ct = default)
        => await ApplySpec(spec).FirstOrDefaultAsync(ct);

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(
        ISpecification<T, TResult> spec, CancellationToken ct = default)
        => await ApplySpec(spec).FirstOrDefaultAsync(ct);

    public async Task<List<T>> ListAsync(
        ISpecification<T> spec, CancellationToken ct = default)
        => await ApplySpec(spec).ToListAsync(ct);

    public async Task<List<TResult>> ListAsync<TResult>(
        ISpecification<T, TResult> spec, CancellationToken ct = default)
        => await ApplySpec(spec).ToListAsync(ct);

    public async Task<bool> AnyAsync(
        ISpecification<T> spec, CancellationToken ct = default)
        => await ApplySpec(spec).AnyAsync(ct);

    public async Task<int> CountAsync(
        ISpecification<T> spec, CancellationToken ct = default)
        => await ApplySpec(spec).CountAsync(ct);

    // --- private helpers ---

    private IQueryable<T> ApplySpec(ISpecification<T> spec)
        => SpecificationEvaluator.Default.GetQuery(
            _dbContext.Set<T>().AsNoTracking(), spec);

    private IQueryable<TResult> ApplySpec<TResult>(ISpecification<T, TResult> spec)
        => SpecificationEvaluator.Default.GetQuery(
            _dbContext.Set<T>().AsNoTracking(), spec);
}
```

##### Rule changes
MUST:
- Implement both `IRepository<T>` and `IReadRepository<T>`
- Apply `AsNoTracking()` on all read queries
- Use `SpecificationEvaluator.Default.GetQuery(...)` for all spec application
- Never call `SaveChangesAsync`

MUST NOT:
- Contain inline LINQ predicates — all filtering goes through specs
- Call `SaveChangesAsync` — this is the Unit of Work's responsibility (solution 11)

---

## App.Host (.csproj) (extended)

### Project extension

#### Goal
- Register `Repository<T>` as the implementation for both `IRepository<T>` and `IReadRepository<T>` using open generic DI registration
- Register with `Scoped` lifetime — one repository instance per HTTP request, sharing the same DbContext

#### Structure

##### Project Structure
```
/App.Host
  /DependencyInjection
    RepositoryRegistration.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /DependencyInjection/RepositoryRegistration.cs | Registers IRepository and IReadRepository open generics | |

#### Rules
MUST:
- Both `IRepository<>` and `IReadRepository<>` registered as open generics pointing to `Repository<>`
- Registered with `Scoped` lifetime — shares DbContext within the same request

#### Anti-patterns
- Registering `IRepository<TodoTask>` per entity type — use open generic registration instead
- Registering as `Singleton` — DbContext is Scoped, repository must match

---

### Class extension

#### RepositoryRegistration (created)

##### Goal
- Centralise repository DI registration in one extension method called from the composition root

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Repository DI registration | `RepositoryRegistration` | `RepositoryRegistration` | `RepositoryRegistration.cs` | `RepositoryRegistration.cs` |

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

        return services;
    }
}
```

Called from the composition root:

```csharp
// App.Host/Program.cs
builder.Services.AddRepositories();
```

##### Rule changes
MUST:
- Both `IRepository<>` and `IReadRepository<>` registered pointing to `Repository<>`
- Registration uses `Scoped` lifetime

---

## {Module}.Application (.csproj) (extended)

### Project extension

#### Goal
- Enforce that all handlers use repository abstractions — never DbContext directly

#### Rules
MUST:
- Command handlers inject `IRepository<T>` from Shared — never DbContext
- Query handlers inject `IReadRepository<T>` from Shared — never `IRepository<T>` or DbContext

MUST NOT:
- Any Application class reference `App.Infrastructure` or `DbContext`

#### Anti-patterns
- `private readonly AppDbContext _dbContext` in a handler — use `IRepository<T>` instead
- `IRepository<T>` injected into a query handler — use `IReadRepository<T>` to signal read-only intent

---

# Rules

MUST:
- `IReadRepository<T>` and `IRepository<T>` defined in BuildingBlocks
- All repository read methods accept `ISpecification<T>` — no raw lambda or LINQ parameters
- `IRepository<T>` extends `IReadRepository<T>`
- `IRepository<T>` has no `SaveChangesAsync` — committing belongs to Unit of Work (solution 11)
- Single generic `Repository<T>` registered in DI for both interfaces
- `Repository<T>` applies `AsNoTracking()` on all reads
- Command handlers inject `IRepository<T>`
- Query handlers inject `IReadRepository<T>`

MUST NOT:
- Application layer reference DbContext directly
- Per-entity repository subclass be created
- `Repository<T>` call `SaveChangesAsync`
- Raw LINQ predicates appear in repository method signatures
- `IRepository<T>` or `IReadRepository<T>` defined in BuildingBlocks — they belong in Shared

# Anti-patterns
- `TaskRepository : Repository<TodoTask>` — unnecessary subclass, open generic covers all types
- Handler injects DbContext directly — use `IRepository<T>` or `IReadRepository<T>`
- `IRepository<T>` used in query handler — signals wrong intent, use `IReadRepository<T>`
- Repository method accepts `Expression<Func<T, bool>>` — all filtering goes through specs

# Check list
- [ ] `IReadRepository<T>` defined in `BuildingBlocks/Repositories`
- [ ] `IRepository<T>` defined in `BuildingBlocks/Repositories`, extends `IReadRepository<T>`
- [ ] `IRepository<T>` has no `SaveChangesAsync`
- [ ] `Repository<T>` implemented in `App.Infrastructure/Repositories/Repository.cs`
- [ ] `Repository<T>` applies `AsNoTracking()` on all read queries
- [ ] `Repository<T>` uses `SpecificationEvaluator` for all spec application
- [ ] `Repository<T>` never calls `SaveChangesAsync`
- [ ] Open generic DI registration in `App.Host` for both interfaces
- [ ] Registered with `Scoped` lifetime
- [ ] No per-entity repository subclass exists
- [ ] Command handlers inject `IRepository<T>`
- [ ] Query handlers inject `IReadRepository<T>`
- [ ] No DbContext reference in any Application class

# Unittest TestCases
- [ ] When `FirstOrDefaultAsync` called with matching spec Then correct entity returned
- [ ] When `FirstOrDefaultAsync` called with non-matching spec Then null returned
- [ ] When `ListAsync` called Then all matching entities returned
- [ ] When `AnyAsync` called with matching spec Then returns true
- [ ] When `AnyAsync` called with non-matching spec Then returns false
- [ ] When `AddAsync` called Then entity tracked by EF — not yet in database
- [ ] When `Remove` called Then entity marked for deletion — not yet removed from database
- [ ] When read query runs Then EF change tracker does not track returned entities (AsNoTracking)
