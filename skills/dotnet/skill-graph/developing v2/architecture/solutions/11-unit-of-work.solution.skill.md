---
uid: 4b7e2c9a-1f5d-4a8b-c3e6-d9f1a4b2e7c5
order: 11
name: unit-of-work
description: Defines IUnitOfWork, UnitOfWorkContext, and UnitOfWorkBehavior — the pipeline mechanism that commits all staged entity changes atomically after the top-level command handler completes, ensuring sub-commands never commit prematurely
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - application
  - infrastructure
  - unit-of-work
  - mediatr
  - pipeline
triggers:
  - define unit of work
  - commit changes after handler
  - atomic command transaction
  - UnitOfWorkBehavior
  - SaveChanges pattern
  - nested command dispatch
creates:
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/IUnitOfWork.class.skill|IUnitOfWork.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/UnitOfWorkContext.class.skill|UnitOfWorkContext.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/UnitOfWorkBehavior.class.skill|UnitOfWorkBehavior.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Infrastructure csproj/classes/UnitOfWork.class.skill|UnitOfWork.class.skill]]"
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/BuildingBlocks.csproj.skill|BuildingBlocks.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Infrastructure csproj/App.Infrastructure.csproj.skill|App.Infrastructure.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Host csproj/App.Host.csproj.skill|App.Host.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/classes/CommandHandler.class.skill|CommandHandler.class.skill]]"
depends_on:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill|01-module-boundary.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill|02-solution-layer-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/08-repository.solution.skill|08-repository.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/09-command-handler.solution.skill|09-command-handler.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/10-validation.solution.skill|10-validation.solution.skill]]"
---
# Goal
- Define `IUnitOfWork` as the single commit point for all staged entity changes — the only place `SaveChangesAsync` is called
- Define `UnitOfWorkBehavior` as the pipeline mechanism that automatically commits after the top-level command handler completes
- Define `UnitOfWorkContext` as the nesting depth tracker that prevents sub-commands from committing prematurely
- Ensure all changes from a command and all its sub-commands are committed atomically in a single `SaveChangesAsync` call
- Enforce that no handler ever calls `SaveChangesAsync` directly

# Core Principles
- `IUnitOfWork` lives in Shared — every layer can reference the commit contract without coupling to infrastructure
- `IUnitOfWork` is the only component that calls `SaveChangesAsync` — handlers, repositories, and domain services never call it
- `UnitOfWorkBehavior` and `UnitOfWorkContext` live in BuildingBlocks — they reference `ICommand` and `IUnitOfWork` from Shared
- `UnitOfWorkBehavior` activates only on `ICommand` — queries never trigger a commit
- `UnitOfWorkContext` tracks nesting depth — only the outermost command (`Depth == 1`) commits
- Sub-commands dispatched via `_mediator.Send()` increment depth — they stage changes but defer commit to the root
- **No explicit rollback needed** — EF Core uses implicit transactions: if `SaveChangesAsync` is never called, the DbContext is disposed at request scope end and all staged changes are silently abandoned. Explicit rollback is only required when using `DbContext.Database.BeginTransactionAsync()`, which this architecture does not use.
- If the handler throws, `SaveChangesAsync` is never called — all staged changes are discarded with the request scope
- `UnitOfWorkContext` is registered as `Scoped` — one instance per HTTP request, shared across all nested command dispatches within that request

# Depend on solutions
- [[01-module-boundary.solution.skill]] — defines BuildingBlocks, App.Infrastructure, and App.Host project boundaries
- [[02-solution-layer-structure.solution.skill]] — defines the layer structure these projects belong to
- [[08-repository.solution.skill]] — `IRepository<T>` stages changes that `IUnitOfWork` commits
- [[09-command-handler.solution.skill]] — `ICommand` marker that `UnitOfWorkBehavior` constrains on
- [[10-validation.solution.skill]] — `ValidationBehavior` runs before `UnitOfWorkBehavior` — pipeline order dependency

# Requirements
- `MediatR` NuGet package — provides `IPipelineBehavior<TRequest, TResponse>`
- `Microsoft.EntityFrameworkCore` NuGet package — provides `DbContext.SaveChangesAsync` called by `UnitOfWork`

# Template Skill Mutations

## Shared (.csproj) (extended)

### Project extension

#### Goal
- Own `IUnitOfWork` — the commit contract accessible by every layer without infrastructure coupling

#### Core Principals
- Interface only — no EF Core dependency in Shared
- Single method: `SaveChangesAsync` — nothing else

#### Structure

##### Project Structure
```
/Shared
  /UnitOfWork
    IUnitOfWork.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /UnitOfWork/IUnitOfWork.cs | Commit contract — single SaveChangesAsync method | IUnitOfWork.class.skill |

#### Rules
MUST:
- `IUnitOfWork` defined in Shared — not BuildingBlocks, not App.Infrastructure

MUST NOT:
- Shared reference EF Core

---

### Class extension

#### IUnitOfWork (created)

##### Goal
- Define the single commit contract — exactly one method, exactly one responsibility
- Live in Shared so every layer can reference it without depending on infrastructure

##### Core Principals
- Single method only: `SaveChangesAsync(CancellationToken)` — nothing else
- Implementation in App.Infrastructure — Shared holds only the interface
- Registered as `Scoped` — shares the same DbContext instance as `Repository<T>` within the request

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Unit of work interface | `IUnitOfWork` | `IUnitOfWork` | `IUnitOfWork.cs` | `IUnitOfWork.cs` |

##### Implementation changes

```csharp
// Shared/UnitOfWork/IUnitOfWork.cs
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

##### Rule changes
MUST:
- Single method only — `SaveChangesAsync(CancellationToken ct = default)`
- No additional methods — not `BeginTransaction`, not `Rollback`, not `Commit`

---

## BuildingBlocks (.csproj) (extended)

### Project extension

#### Goal
- Own `UnitOfWorkContext` and `UnitOfWorkBehavior` — the depth tracking and pipeline commit enforcement
- Reference `ICommand` and `IUnitOfWork` from Shared

#### Core Principals
- `UnitOfWorkContext` is a plain class with a counter — no infrastructure dependency
- `UnitOfWorkBehavior` depends on `IUnitOfWork` and `UnitOfWorkContext` — both resolved from DI

#### Structure

##### Project Structure
```
/BuildingBlocks
  /MediatR
    UnitOfWorkContext.cs
    UnitOfWorkBehavior.cs
```

##### Directory and class skills
| `Directory\|file`              | Description                                                       | Pattern skill                  |
| ------------------------------ | ----------------------------------------------------------------- | ------------------------------ |
| /MediatR/UnitOfWorkContext.cs  | Scoped depth counter preventing premature sub-command commit      | UnitOfWorkContext.class.skill  |
| /MediatR/UnitOfWorkBehavior.cs | Pipeline behavior that commits at depth 1 after handler completes | UnitOfWorkBehavior.class.skill |

#### NuGet Packages
| Package | Purpose |
| --- | --- |
| `MediatR` | Provides `IPipelineBehavior<TRequest, TResponse>` implemented by `UnitOfWorkBehavior` |

#### Rules
MUST:
- `UnitOfWorkContext` and `UnitOfWorkBehavior` defined in BuildingBlocks
- Both reference `ICommand` and `IUnitOfWork` from Shared
- `UnitOfWorkBehavior` constrained on `where TRequest : ICommand`
- `UnitOfWorkBehavior` constrains on `where TRequest : ICommand` — never activates on queries



---

### Class extension

#### IUnitOfWork (created)

##### Goal
- Define the single commit contract — exactly one method, exactly one responsibility
- Be the only component in the entire solution that is permitted to call `SaveChangesAsync`

##### Core Principals
- Single method: `SaveChangesAsync(CancellationToken)` — nothing else
- Implementation lives in App.Infrastructure — BuildingBlocks only holds the interface
- Registered as `Scoped` — shares the same DbContext instance as `Repository<T>` within the request

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Unit of work interface | `IUnitOfWork` | `IUnitOfWork` | `IUnitOfWork.cs` | `IUnitOfWork.cs` |

##### Implementation changes

```csharp
// BuildingBlocks/UnitOfWork/IUnitOfWork.cs
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

##### Rule changes
MUST:
- Single method only — `SaveChangesAsync(CancellationToken ct = default)`
- No additional methods — not `BeginTransaction`, not `Rollback`, not `Commit`

---

#### UnitOfWorkContext (created)

##### Goal
- Track the nesting depth of active command pipeline invocations within a single request scope
- Allow `UnitOfWorkBehavior` to determine whether it is the outermost command (`Depth == 1`) and therefore responsible for committing

##### Core Principals
- Plain class — no interfaces, no base classes, no infrastructure dependencies
- Single mutable integer property — `Depth`
- Registered as `Scoped` — one instance shared across all nested `_mediator.Send()` calls within the same HTTP request
- Never used directly in handlers — only `UnitOfWorkBehavior` reads and writes this

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Nesting depth tracker | `UnitOfWorkContext` | `UnitOfWorkContext` | `UnitOfWorkContext.cs` | `UnitOfWorkContext.cs` |

##### Implementation changes

```csharp
// BuildingBlocks/MediatR/UnitOfWorkContext.cs
public class UnitOfWorkContext
{
    public int Depth { get; set; }
}
```

##### Rule changes
MUST:
- Registered as `Scoped` — never `Singleton` or `Transient`
- Never injected into handlers — only `UnitOfWorkBehavior` uses it

MUST NOT:
- Contain any business logic
- Be used to share state between handlers beyond depth tracking

---

#### UnitOfWorkBehavior (created)

##### Goal
- Automatically commit all staged changes after the top-level command handler completes
- Prevent sub-commands from committing prematurely by checking `UnitOfWorkContext.Depth`
- Guarantee that if the handler throws, `SaveChangesAsync` is never called — changes are discarded

##### Core Principals
- Increments `UnitOfWorkContext.Depth` on entry, decrements in `finally` — depth always restored even on exception
- Calls `SaveChangesAsync` only when `Depth == 1` — the outermost command in the current request
- Sub-commands reach this behavior with `Depth > 1` — they stage changes but do not commit
- **No catch/rollback block** — EF Core uses implicit transactions. When `SaveChangesAsync` is not called (because handler threw), the DbContext is disposed at end of request scope and all pending changes are silently abandoned. No explicit rollback is necessary. If explicit transactions are introduced in the future, a catch/rollback block must be added at that point.
- `try/finally` ensures depth counter is always restored — no leaked depth on exception
- Constrained to `where TRequest : ICommand` — never activates for query requests

##### Nesting depth flow
```
HTTP Request arrives
    ↓
UnitOfWorkBehavior: Depth++ → Depth = 1   (root command)
    ↓
Handler dispatches sub-command via _mediator.Send()
    ↓
UnitOfWorkBehavior: Depth++ → Depth = 2   (sub-command)
    ↓
Sub-command handler completes — stages changes
    ↓
UnitOfWorkBehavior: Depth == 2 → skip SaveChanges
UnitOfWorkBehavior: Depth-- → Depth = 1   (finally)
    ↓
Root handler continues — stages its own changes
    ↓
UnitOfWorkBehavior: Depth == 1 → call SaveChangesAsync  ← single atomic commit
UnitOfWorkBehavior: Depth-- → Depth = 0   (finally)

On handler exception:
    ↓
UnitOfWorkBehavior: Depth == 1 → SaveChangesAsync NOT called (response = await next() threw)
UnitOfWorkBehavior: Depth-- → Depth = 0   (finally)
DbContext disposed at request scope end → all pending changes abandoned automatically
```

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| UoW pipeline behavior | `UnitOfWorkBehavior<TRequest, TResponse>` | `UnitOfWorkBehavior<AssignTaskCommand, Result>` | `UnitOfWorkBehavior.cs` | `UnitOfWorkBehavior.cs` |

##### Implementation changes

```csharp
// BuildingBlocks/MediatR/UnitOfWorkBehavior.cs
public class UnitOfWorkBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand
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

            // only the outermost command commits
            if (_context.Depth == 1)
                await _unitOfWork.SaveChangesAsync(ct);

            return response;
        }
        finally
        {
            // always restore depth — even on exception
            _context.Depth--;
        }
    }
}
```

##### Rule changes
MUST:
- Constrained to `where TRequest : ICommand` — never activates on queries
- Use `try/finally` to guarantee depth counter is always restored
- Call `SaveChangesAsync` only when `Depth == 1`
- Increment depth before `next()` — decrement in `finally`

MUST NOT:
- Call `SaveChangesAsync` when `Depth > 1` — sub-commands must not commit
- Add a catch/rollback block — EF implicit transactions do not require it; adding one without explicit transaction management would be incorrect
- Catch exceptions to swallow them — let them propagate, `SaveChangesAsync` is skipped naturally

---

## App.Infrastructure (.csproj) (extended)

### Project extension

#### Goal
- Provide the `UnitOfWork` EF Core implementation that wraps `AppDbContext.SaveChangesAsync`

#### Structure

##### Project Structure
```
/App.Infrastructure
  /UnitOfWork
    UnitOfWork.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /UnitOfWork/UnitOfWork.cs | EF Core SaveChangesAsync implementation | UnitOfWork.class.skill |

#### NuGet Packages
| Package | Purpose |
| --- | --- |
| `Microsoft.EntityFrameworkCore` | Provides `DbContext.SaveChangesAsync` |

#### Rules
MUST:
- `UnitOfWork` registered as `Scoped` — must share the same `DbContext` instance as `Repository<T>`

MUST NOT:
- `UnitOfWork` expose any method beyond `SaveChangesAsync`
- `UnitOfWork` contain transaction management logic — EF manages transactions implicitly

---

### Class extension

#### UnitOfWork (created)

##### Goal
- Implement `IUnitOfWork` by delegating to `AppDbContext.SaveChangesAsync`
- Be the single place in the entire solution where `DbContext.SaveChangesAsync` is called

##### Core Principals
- Wraps `AppDbContext` — receives it via constructor injection
- Single method implementation — no transaction management, no retry logic
- Registered as `Scoped` — same `DbContext` instance as `Repository<T>`, ensuring all staged changes are committed together

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| UoW EF implementation | `UnitOfWork` | `UnitOfWork` | `UnitOfWork.cs` | `UnitOfWork.cs` |

##### Implementation changes

```csharp
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

##### Rule changes
MUST:
- Implement `IUnitOfWork` from Shared
- Delegate to `AppDbContext.SaveChangesAsync` — no additional logic
- Registered as `Scoped`

MUST NOT:
- Contain transaction management logic — EF Core manages transactions implicitly via `SaveChangesAsync`
- Be called from anywhere except `UnitOfWorkBehavior`

---

## App.Host (.csproj) (extended)

### Project extension

#### Goal
- Register `IUnitOfWork` and `UnitOfWorkContext` with correct lifetimes
- Register `UnitOfWorkBehavior` in the pipeline after `ValidationBehavior`

#### Rules
MUST:
- `IUnitOfWork` registered as `Scoped`
- `UnitOfWorkContext` registered as `Scoped`
- `UnitOfWorkBehavior` registered after `ValidationBehavior` in pipeline

---

### Class extension

#### RepositoryRegistration (extended)

##### Goal
- Add `IUnitOfWork` and `UnitOfWorkContext` Scoped registrations alongside the repository registrations from solution 08

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

        return services;
    }
}
```

##### Rule changes
MUST:
- `IUnitOfWork` and `UnitOfWorkContext` registered as `Scoped` — same lifetime as DbContext and Repository

---

#### PipelineRegistration (extended)

##### Goal
- Add `UnitOfWorkBehavior` as the last pipeline behavior — after `ValidationBehavior`

##### Implementation changes
`PipelineRegistration` from solutions 09–10 extended with `UnitOfWorkBehavior`:

```csharp
// App.Host/DependencyInjection/PipelineRegistration.cs
public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(
        this IServiceCollection services)
    {
        // 1. validation — rejects invalid commands before anything else runs
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        // 2. unit of work — commits staged changes after handler completes
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(UnitOfWorkBehavior<,>));

        // solution 14 (concurrency) adds: ConcurrencyBehavior between Validation and UnitOfWork
        // solution 15 (external-created) adds: GuidResolvingBehavior between Validation and Concurrency

        return services;
    }
}
```

##### Rule changes
MUST:
- `UnitOfWorkBehavior` registered after `ValidationBehavior` — invalid commands never open a unit of work
- `IUnitOfWork` and `UnitOfWorkContext` registered as `Scoped`

MUST NOT:
- `UnitOfWorkBehavior` registered before `ValidationBehavior` — would waste a commit attempt on invalid input

---

## CommandHandler.class.skill (extended)

### Class extension

#### CommandHandler (extended)

##### Goal
- Clarify that handlers never call `SaveChangesAsync` — `UnitOfWorkBehavior` commits automatically after the handler returns

##### Core Principals
- Handler stages changes via `IRepository<T>` — `AddAsync`, `Update`, `Remove`
- Handler returns its result — `UnitOfWorkBehavior` then calls `SaveChangesAsync`
- Sub-commands are safe to dispatch inside a handler — `UnitOfWorkContext` ensures they do not commit prematurely

##### Implementation changes
Complete handler showing staging without SaveChanges, and safe sub-command dispatch:

```csharp
// Task.Application/Features/AssignTask/AssignTask.Handler.cs
public class AssignTaskHandler : IRequestHandler<AssignTaskCommand, Result>
{
    private readonly IRepository<TodoTask> _repository;

    public AssignTaskHandler(IRepository<TodoTask> repository)
        => _repository = repository;

    public async Task<Result> Handle(AssignTaskCommand command, CancellationToken ct)
    {
        // load via named spec
        var task = await _repository.FirstOrDefaultAsync(
            new TaskByIdSpec(command.TaskId), ct);

        if (task is null)
            return Result.NotFound();

        // domain call — stages change in EF tracker
        task.Assign(command.AssigneeId);

        // no SaveChangesAsync here — UnitOfWorkBehavior commits after this returns
        return Result.Success();
    }
}
```

##### Rule changes
MUST NOT:
- Call `SaveChangesAsync` or inject `IUnitOfWork` — `UnitOfWorkBehavior` owns the commit

---

# Rules

MUST:
- `IUnitOfWork` defined in BuildingBlocks — single `SaveChangesAsync` method only
- `UnitOfWorkContext` defined in BuildingBlocks — registered as `Scoped`
- `UnitOfWorkBehavior` defined in BuildingBlocks — constrained to `ICommand` only
- `UnitOfWork` implementation in App.Infrastructure
- `UnitOfWorkBehavior` uses `try/finally` — depth always restored on exception
- `UnitOfWorkBehavior` commits only when `Depth == 1`
- `UnitOfWorkBehavior` registered after `ValidationBehavior` and before no other behavior — it is always last
- `IUnitOfWork` and `UnitOfWorkContext` registered as `Scoped`
- Sub-commands safe to dispatch from handlers — depth counter prevents premature commit

MUST NOT:
- Any handler call `SaveChangesAsync` or inject `IUnitOfWork`
- `UnitOfWorkBehavior` activate on queries — constrained to `ICommand`
- `UnitOfWorkBehavior` registered before `ValidationBehavior`
- `UnitOfWorkContext` registered as `Singleton` or `Transient`
- `UnitOfWork` contain logic beyond `DbContext.SaveChangesAsync` delegation
- `UnitOfWorkBehavior` contain a catch/rollback block — EF implicit transactions do not require it
- `IUnitOfWork` defined in BuildingBlocks — belongs in Shared
# Anti-patterns
- `await _unitOfWork.SaveChangesAsync(ct)` in a handler — `UnitOfWorkBehavior` owns the commit
- `UnitOfWorkBehavior` without depth counter — sub-commands commit prematurely, breaking atomicity
- `UnitOfWorkContext` registered as `Singleton` — depth shared across requests, causes incorrect commit decisions
- `UnitOfWorkContext` registered as `Transient` — sub-command gets a fresh instance, depth counter never reaches 1 in nested dispatch
- `IUnitOfWork` injected into a handler — handler must trust the pipeline
- Explicit `catch { rollback }` in `UnitOfWorkBehavior` without explicit transaction — creates false safety, `DbContext.SaveChangesAsync` is already atomic

# Check list
- [ ] `IUnitOfWork` defined in `BuildingBlocks/UnitOfWork/IUnitOfWork.cs` with single `SaveChangesAsync` method
- [ ] `UnitOfWorkContext` defined in `BuildingBlocks/MediatR/UnitOfWorkContext.cs`
- [ ] `UnitOfWorkBehavior` defined in `BuildingBlocks/MediatR/UnitOfWorkBehavior.cs`
- [ ] `UnitOfWorkBehavior` constrained to `where TRequest : ICommand`
- [ ] `UnitOfWorkBehavior` uses `try/finally` for depth decrement
- [ ] `UnitOfWorkBehavior` calls `SaveChangesAsync` only when `Depth == 1`
- [ ] `UnitOfWork` implemented in `App.Infrastructure/UnitOfWork/UnitOfWork.cs`
- [ ] `UnitOfWork` registered as `Scoped` in App.Host
- [ ] `UnitOfWorkContext` registered as `Scoped` in App.Host
- [ ] `UnitOfWorkBehavior` registered after `ValidationBehavior` in pipeline
- [ ] No `SaveChangesAsync` call in any handler
- [ ] No `IUnitOfWork` injection in any handler

# Unittest TestCases
- [ ] When top-level command completes successfully Then `SaveChangesAsync` called exactly once
- [ ] When command dispatches sub-command Then `SaveChangesAsync` called once after root completes — not at sub-command level
- [ ] When sub-command completes Then `UnitOfWorkContext.Depth` returns to 1 — root still owns commit
- [ ] When handler throws Then `SaveChangesAsync` never called — staged changes discarded
- [ ] When query handler runs Then `UnitOfWorkBehavior` does not activate — no `SaveChangesAsync`
- [ ] When multiple repositories used in one handler Then all changes committed in single `SaveChangesAsync`
- [ ] When `UnitOfWorkContext` registered as Scoped Then nested dispatch shares same depth counter
