---
uid: 7d3f1a8e-2c5b-4e9d-b6f4-a8c2e5d1f7b3
order: 9
name: command-handler
description: Defines the command handler pattern — ICommand marker, Command record declaration in Interfaces, CommandHandler orchestration structure in Application, feature folder layout, module DI registration, and App.Host pipeline wiring
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - application
  - cqrs
  - mediatr
  - command
  - handler
triggers:
  - implement command handler
  - create command
  - write command handler
  - handle write operation
  - add feature to module
creates:
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/ICommand.class.skill|ICommand.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Interfaces csproj/classes/Command.class.skill|Command.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/classes/CommandHandler.class.skill|CommandHandler.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/classes/ModuleApplicationRegistration.class.skill|ModuleApplicationRegistration.class.skill]]"
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/BuildingBlocks.csproj.skill|BuildingBlocks.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Interfaces csproj/{Module}.Interfaces.csproj.skill|{Module}.Interfaces.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/{Module}.Application.csproj.skill|{Module}.Application.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Host csproj/App.Host.csproj.skill|App.Host.csproj.skill]]"
depends_on:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill|01-module-boundary.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill|02-solution-layer-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/07-ardalis-specification.solution.skill|07-ardalis-specification.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/08-repository.solution.skill|08-repository.solution.skill]]"
---
>[!todo] ICommand to shared
>Договаривались что icommand пойдет в shared
# Goal
- Define `ICommand<TResponse>` in Shared as the marker interface that identifies write operations and activates pipeline behaviors
- Define where and how Commands are declared — as records in `{Module}.Interfaces/Commands`
- Define how CommandHandlers are structured — load via spec, guard, domain call, stage, return Result
- Define the feature folder layout in `{Module}.Application/Features` — one folder per feature, handler and validator co-located
- Define module DI registration — each module self-registers via one extension method, handlers auto-scanned
- Define App.Host pipeline wiring — MediatR and module registrations assembled in the composition root
- Establish that handlers never contain business logic, never call SaveChanges, and never reference DbContext

# Core Principles
- Handler orchestrates — it never contains business rules
- Domain layer decides — handler loads data and delegates all decisions to entities and domain services
- Handler follows a fixed structure: load → guard → domain call → stage → return result
- Handler returns `Ardalis.Result<T>` — all outcomes expressed as typed results, no exceptions for flow control
- `ICommand<TResponse>` lives in Shared — every layer can reference it without coupling
- `ICommand<TResponse>` marks a request as a write operation — pipeline behaviors activate on this marker
- One Command, one Handler — no shared handlers, no handler dispatching multiple commands at the top level
- Cross-module writes go through `_mediator.Send()` — never via direct method calls on another module's classes
- Handler never calls `SaveChangesAsync` — this is the Unit of Work's responsibility (solution 11)
- Handlers and validators are registered via assembly scan — never manually one by one

# Depend on solutions
- [[01-module-boundary.solution.skill]] — defines Shared, `{Module}.Interfaces`, and `{Module}.Application` project boundaries
- [[02-solution-layer-structure.solution.skill]] — defines dependency rules: Shared has no dependencies, Application references Shared
- [[07-ardalis-specification.solution.skill]] — handlers load entities via named specs, never inline LINQ
- [[08-repository.solution.skill]] — handlers inject `IRepository<T>` for entity loading and staging

# Requirements
- `Ardalis.Result` NuGet package — provides `Result<T>`, `Result.Created`, `Result.NotFound`, `Result.Conflict`, `Result.Error`, `Result.Invalid`
- `MediatR` NuGet package — provides `IRequest<T>`, `IRequestHandler<TRequest, TResponse>`, `ISender`, `IMediator`

# Template Skill Mutations

## Shared (.csproj) (extended)

### Project extension

#### Goal
- Own the `ICommand` and `ICommand<TResponse>` marker interfaces used by all write operations across all modules

#### Core Principals
- Marker interfaces only — no business logic, no handler code
- `ICommand<TResponse>` extends MediatR's `IRequest<TResponse>` — this is how MediatR routes the request to the handler
- Pipeline behaviors constrain on `ICommand` — only write operations pass through write-side behaviors
- Lives in Shared so every layer (Domain, Application, Infrastructure, Api) can reference the marker without coupling to BuildingBlocks

#### Structure

##### Project Structure
```
/Shared
  /MediatR
    ICommand.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /MediatR/ICommand.cs | Write operation marker interfaces | ICommand.class.skill |

#### NuGet Packages
| Package | Purpose |
| --- | --- |
| `MediatR` | Provides `IRequest<T>` that `ICommand<T>` extends |
| `Ardalis.Result` | Provides `Result<T>` return type convention |

#### Rules
MUST:
- `ICommand` and `ICommand<TResponse>` defined in BuildingBlocks
- Both extend MediatR `IRequest` / `IRequest<TResponse>`
MUST NOT:
- `ICommand` defined in BuildingBlocks — it belongs in Shared so modules don't need a BuildingBlocks reference

---

### Class extension

#### ICommand (created)

##### Goal
- Mark a MediatR request as a write operation so pipeline behaviors can activate selectively on commands only
- Provide two variants: `ICommand` for commands with no response value, `ICommand<TResponse>` for commands that return a result

##### Core Principals
- Interface only — no properties, no methods
- `ICommand<TResponse>` is the standard form — almost all commands return `Result<T>`
- Pipeline behaviors in BuildingBlocks use `where TRequest : ICommand` to activate only for write operations

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Command marker (no return) | `ICommand` | `ICommand` | `ICommand.cs` | `ICommand.cs` |
| Command marker (with return) | `ICommand<TResponse>` | `ICommand<Result<CreateTaskResult>>` | `ICommand.cs` | `ICommand.cs` |

##### Implementation changes
Both variants defined in one file:

```csharp
// BuildingBlocks/MediatR/ICommand.cs
public interface ICommand : IRequest { }
public interface ICommand<TResponse> : IRequest<TResponse> { }
```

##### Rule changes
MUST:
- All command records implement `ICommand<Result<T>>` — not `IRequest<T>` directly
- `ICommand` used only when the command truly produces no return value
-  Defined in Shared — never in BuildingBlocks or any module project

---

## {Module}.Interfaces (.csproj) (extended)

### Project extension

#### Goal
- Own all Command record declarations and their associated result records for this module
- Be the only project other modules depend on when dispatching commands to this module

#### Core Principals
- Commands are declarations only — records with properties, no methods, no logic
- Result records are declared alongside their command in the same file
- Both Command and Result are `record` types — immutable by design

#### Structure

##### Project Structure
```
/{Module}.Interfaces
  /Commands
    Create{Entity}Command.cs
    Update{Entity}Command.cs
    Delete{Entity}Command.cs
    Assign{Entity}Command.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Commands | Write intent contract declarations for this module | Command.class.skill |

#### Rules
MUST:
- All commands for this module declared in `/{Module}.Interfaces/Commands`
- Each command file contains the command record and its result record

MUST NOT:
- Commands contain any logic or methods
- Commands reference Domain entity types — input properties are primitives or shared value types only

---

### Class extension

#### Command (created)

##### Goal
- Express a named write intent as an immutable record that carries all input needed for the operation
- Implement `ICommand<Result<T>>` so the MediatR pipeline routes it to the correct handler and activates write-side behaviors

##### Core Principals
- Declared as `record` — immutable, structural equality by default
- Implements `ICommand<Result<{CommandName}Result>>` — return type is always `Result<T>`
- Properties are primitives or simple value types — no domain entity references
- Result record declared in the same file — named `{CommandName}Result`
- One command per write intent — `CreateTaskCommand`, `AssignTaskCommand`, `DeleteTaskCommand`

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Create entity | `Create{Entity}Command` | `CreateTaskCommand` | `Create{Entity}Command.cs` | `CreateTaskCommand.cs` |
| Update entity | `Update{Entity}Command` | `UpdateTaskCommand` | `Update{Entity}Command.cs` | `UpdateTaskCommand.cs` |
| Delete entity | `Delete{Entity}Command` | `DeleteTaskCommand` | `Delete{Entity}Command.cs` | `DeleteTaskCommand.cs` |
| Domain action | `{Verb}{Entity}Command` | `AssignTaskCommand` | `{Verb}{Entity}Command.cs` | `AssignTaskCommand.cs` |
| Command result | `{CommandName}Result` | `CreateTaskResult` | same file as command | `CreateTaskCommand.cs` |

##### Implementation changes
Command and result declared together in one file:

```csharp
// Task.Interfaces/Commands/CreateTaskCommand.cs
public record CreateTaskCommand(
    string Title,
    int AssigneeId
) : ICommand<Result<CreateTaskResult>>;

public record CreateTaskResult(int Id);
```

```csharp
// Task.Interfaces/Commands/AssignTaskCommand.cs
public record AssignTaskCommand(
    int TaskId,
    int AssigneeId
) : ICommand<Result>;
```

```csharp
// Task.Interfaces/Commands/DeleteTaskCommand.cs
public record DeleteTaskCommand(
    int TaskId
) : ICommand<Result>;
```

##### Rule changes
MUST:
- Declared as `record`
- Implement `ICommand<Result<T>>` or `ICommand<Result>` — never `IRequest<T>` directly
- Result type declared in the same file as the command
- Properties are primitives or simple types — no domain entity references

MUST NOT:
- Command contain methods or logic
- Command reference domain entity types as properties

---

## {Module}.Application (.csproj) (extended)

### Project extension

#### Goal
- Own all CommandHandler implementations and the module's DI registration
- Structure each feature as a vertical slice — handler and validator co-located in one folder
- Self-register all handlers via assembly scan — no manual per-handler registration

#### Core Principals
- One feature folder per write operation under `/Features`
- Handler file named `{FeatureName}.Handler.cs`, class named `{FeatureName}Handler`
- Each module exposes one `Register{ModuleName}Module()` extension method
- Pipeline behaviors are NOT registered here — that is App.Host's responsibility

#### Structure

##### Project Structure
```
/{Module}.Application
  /Features
    /Create{Entity}
      Create{Entity}.Handler.cs
      Create{Entity}.Validator.cs
    /Update{Entity}
      Update{Entity}.Handler.cs
      Update{Entity}.Validator.cs
    /Delete{Entity}
      Delete{Entity}.Handler.cs
      Delete{Entity}.Validator.cs
    /{Verb}{Entity}
      {Verb}{Entity}.Handler.cs
      {Verb}{Entity}.Validator.cs
  {Module}ApplicationRegistration.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Features | One subfolder per feature — handler and validator co-located | CommandHandler.class.skill |
| {Module}ApplicationRegistration.cs | Module DI self-registration extension method | ModuleApplicationRegistration.class.skill |

#### NuGet Packages
| Package                                          | Purpose                                                                 |
| ------------------------------------------------ | ----------------------------------------------------------------------- |
| `MediatR`                                        | Provides `IRequestHandler<TRequest, TResponse>` implemented by handlers |
| `Ardalis.Result`                                 | Provides `Result<T>` return type used in all handlers                   |
| `FluentValidation`                               | Provides `AbstractValidator<T>` — validators defined alongside handlers |
| `FluentValidation.DependencyInjectionExtensions` | Provides `AddValidatorsFromAssembly` used in module registration        |

#### Rules
MUST:
- Each feature in its own subfolder under `/Features`
- Handler file named `{FeatureName}.Handler.cs`
- Handler class named `{FeatureName}Handler`
- Module exposes `Register{ModuleName}Module(IServiceCollection, IConfiguration)` extension method
- Handlers registered via `AddMediatR` assembly scan
- Validators registered via `AddValidatorsFromAssembly`

MUST NOT:
- Pipeline behaviors registered inside module registration — belongs in App.Host
- Handler contain business logic — delegate to domain entities and services
- Handler call `SaveChangesAsync` — Unit of Work owns commit (solution 11)
- Handler reference DbContext directly — use `IRepository<T>` from Shared

#### Anti-patterns
- `CreateTaskCommandHandler.cs` as file name — use `CreateTask.Handler.cs`
- Manual handler registration: `services.AddTransient<CreateTaskHandler>()` — use assembly scan
- Business rule in handler: `if (task.Status == TaskStatus.Closed) return Result.Conflict(...)` — belongs in entity

---

### Class extension

#### CommandHandler (created)

##### Goal
- Orchestrate one write operation: load required data via specs, guard against business failures, delegate to domain, stage changes, return a typed result
- Never contain business rules — always delegate decisions to the domain layer

##### Core Principals
- Implements `IRequestHandler<TCommand, Result<T>>`
- Injects `IRepository<T>` for entity loading and staging — never DbContext
- Follows fixed structure: **load → guard → domain call → stage → return result**
- All entity loading uses named specs — no inline LINQ
- Returns `Ardalis.Result<T>` for all outcomes — no exceptions for flow control
- Cross-module writes dispatched via `_mediator.Send(new OtherModuleCommand(...))` — never direct calls

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Command handler | `{FeatureName}Handler` | `CreateTaskHandler` | `{FeatureName}.Handler.cs` | `CreateTask.Handler.cs` |

##### Implementation changes
Handler follows the load → guard → domain call → stage → return structure:

```csharp
// Task.Application/Features/CreateTask/CreateTask.Handler.cs
public class CreateTaskHandler
    : IRequestHandler<CreateTaskCommand, Result<CreateTaskResult>>
{
    private readonly IRepository<TodoTask> _repository;

    public CreateTaskHandler(IRepository<TodoTask> repository)
        => _repository = repository;

    public async Task<Result<CreateTaskResult>> Handle(
        CreateTaskCommand command, CancellationToken ct)
    {
        // load — use named spec, never inline LINQ
        var assignee = await _repository.FirstOrDefaultAsync(
            new UserByIdSpec(command.AssigneeId), ct);

        // guard — return typed result for business-level failures
        if (assignee is null)
            return Result.NotFound();

        // domain call — entity or domain service makes the decision
        var task = TodoTask.Create(command.Title, command.AssigneeId);

        // stage — repository tracks the change, UnitOfWork commits later
        await _repository.AddAsync(task, ct);

        // return typed result — never throw for flow control
        return Result.Created(new CreateTaskResult(task.Id));
    }
}
```

Handler that dispatches a cross-module sub-command:

```csharp
// Order.Application/Features/CreateOrder/CreateOrder.Handler.cs
public class CreateOrderHandler
    : IRequestHandler<CreateOrderCommand, Result<CreateOrderResult>>
{
    private readonly IRepository<Order> _repository;
    private readonly IMediator _mediator;

    public CreateOrderHandler(IRepository<Order> repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task<Result<CreateOrderResult>> Handle(
        CreateOrderCommand command, CancellationToken ct)
    {
        // cross-module write — dispatched via MediatR, never direct call
        var bookResult = await _mediator.Send(
            new BookItemCommand(command.ProductId, command.Quantity), ct);

        if (!bookResult.IsSuccess)
            return Result.Error("Failed to book supply.");

        var order = Order.Create(command.ProductId, command.Quantity);
        await _repository.AddAsync(order, ct);

        return Result.Created(new CreateOrderResult(order.Id));
    }
}
```

##### Result status conventions

| Result | Meaning | Typical use |
| --- | --- | --- |
| `Result.Created(value)` | Entity created successfully | After `AddAsync` on new entity |
| `Result.Success()` / `Result.Success(value)` | Operation succeeded | After updating existing entity |
| `Result.NoContent()` | Success with no return body | After delete |
| `Result.NotFound()` | Required entity does not exist | Guard after load returns null |
| `Result.Conflict(msg)` | Business state prevents the operation | Guard for business rule violation |
| `Result.Error(msg)` | Unexpected failure | Sub-command failure, unrecoverable state |

##### Rule changes
MUST:
- Implement `IRequestHandler<TCommand, Result<T>>`
- Inject `IRepository<T>` — never DbContext
- Load entities via named specs — never inline LINQ
- Follow load → guard → domain call → stage → return structure
- Return `Result<T>` for all outcomes — never throw for flow control
- Dispatch cross-module writes via `_mediator.Send()` — never direct method calls

MUST NOT:
- Contain business logic or domain rules — delegate to entity or domain service
- Call `SaveChangesAsync` — Unit of Work commits after handler returns (solution 11)
- Reference other module's Domain or Application projects directly
- Use inline LINQ — all queries go through named specs

---

#### ModuleApplicationRegistration (created)

##### Goal
- Self-register all handlers and validators in this module's assembly via scan
- Give App.Host a single call surface for wiring up the module — no module internals exposed

##### Core Principals
- One static extension method per module — `Register{ModuleName}Module`
- `AddMediatR` scans the Application assembly — all `IRequestHandler` implementations registered automatically
- `AddValidatorsFromAssembly` scans the Application assembly — all `AbstractValidator<T>` registered automatically
- Pipeline behaviors NOT registered here — that is App.Host's responsibility

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Module registration | `{ModuleName}ApplicationRegistration` | `TaskApplicationRegistration` | `{ModuleName}ApplicationRegistration.cs` | `TaskApplicationRegistration.cs` |

##### Implementation changes

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

        return services;
    }
}
```

##### Rule changes
MUST:
- Method named `Register{ModuleName}Module`
- Accept `IServiceCollection` and `IConfiguration`
- Register handlers via `AddMediatR` assembly scan
- Register validators via `AddValidatorsFromAssembly`

MUST NOT:
- Register pipeline behaviors — belongs in App.Host
- Register DbContext or infrastructure services — belongs in App.Infrastructure registration
- Reference another module's Application assembly

---

## App.Host (.csproj) (extended)

### Project extension

#### Goal
- Wire all module registrations together in the composition root
- Register MediatR pipeline behaviors in the correct order — this is the only place pipeline order is defined

#### Core Principals
- App.Host calls each module's `Register{ModuleName}Module()` — no module internals referenced
- Pipeline behaviors registered as open generics with `Transient` lifetime — one instance per pipeline invocation
- Pipeline registration order is enforced here — behaviors execute in registration order

#### Structure

##### Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /DependencyInjection/PipelineRegistration.cs | Pipeline behavior registration in correct order | |

#### NuGet Packages
| Package | Purpose |
| --- | --- |
| `MediatR` | Pipeline behavior registration via `IPipelineBehavior<,>` |

#### Rules
MUST:
- All module `Register{ModuleName}Module()` calls made in App.Host composition root
- Pipeline behaviors registered as open generics with `AddTransient`
- Pipeline registration order defined here — not in any module

---

### Class extension

#### PipelineRegistration (created)

##### Goal
- Register all MediatR pipeline behaviors in the single correct order
- Be the authoritative record of what behaviors exist and in what sequence they run

##### Core Principals
- Behaviors registered in execution order — first registered runs first
- Only behaviors relevant at this solution stage registered here — later solutions (10, 11, 14, 15) extend this registration
- `Transient` lifetime — new behavior instance per pipeline invocation

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Pipeline registration | `PipelineRegistration` | `PipelineRegistration` | `PipelineRegistration.cs` | `PipelineRegistration.cs` |

##### Implementation changes
Pipeline registration and module wiring in the composition root:

```csharp
// App.Host/DependencyInjection/PipelineRegistration.cs
public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(
        this IServiceCollection services)
    {
        // behaviors registered in execution order
        // solution 10 (validation) adds: ValidationBehavior
        // solution 11 (unit-of-work) adds: UnitOfWorkBehavior
        // solution 14 (concurrency) adds: ConcurrencyBehavior
        // solution 15 (external-created) adds: GuidResolvingBehavior

        return services;
    }
}
```

Module wiring in Program.cs:

```csharp
// App.Host/Program.cs
builder.Services
    .AddPipeline()
    .AddRepositories()                          // solution 08
    .RegisterTaskModule(builder.Configuration)
    .RegisterTimeLogModule(builder.Configuration)
    .RegisterUserModule(builder.Configuration);
```

##### Rule changes
MUST:
- All module registrations called from App.Host Program.cs
- Pipeline behaviors registered as `AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`
- Behaviors registered in intended execution order — order determines execution sequence

MUST NOT:
- Module registration methods called from within another module
- Pipeline behaviors registered inside any module's registration method

---

# Rules

MUST:
- `ICommand` and `ICommand<TResponse>` defined in Shared — not BuildingBlocks, not any module
- All commands implement `ICommand<Result<T>>` — not `IRequest<T>` directly
- Commands declared as `record` in `{Module}.Interfaces/Commands`
- Result records declared in the same file as their command
- One handler per command — `IRequestHandler<TCommand, Result<T>>`
- Handler structure: load → guard → domain call → stage → return result
- All entity loading in handlers uses named specs from solution 07
- Handlers inject `IRepository<T>`  from Shared — never DbContext
- Cross-module writes dispatched via `_mediator.Send()` — never direct calls
- Each module has `Register{ModuleName}Module()` extension method
- Handlers and validators registered via assembly scan — never manually
- Pipeline behaviors registered in App.Host — never in module registration

MUST NOT:
- Handler contain business logic — delegate to domain
- Handler call `SaveChangesAsync` — Unit of Work owns commit (solution 11)
- Handler reference other module's Domain or Application directly
- Command properties reference domain entity types
- `ICommand` defined in BuildingBlocks — belongs in Shared

SHOULD:
- Guard checks return early before domain call — fail fast pattern
- Handler follow the exact load → guard → domain call → stage → return sequence

# Anti-patterns
- Business rule in handler: `if (task.Status == TaskStatus.Closed) return Result.Conflict(...)` — belongs in entity
- Inline LINQ in handler: `_repository.FirstOrDefaultAsync(x => x.Id == id, ct)` — use `TaskByIdSpec`
- Manual handler registration in module: `services.AddTransient<CreateTaskHandler>()` — use assembly scan
- `SaveChangesAsync` in handler — Unit of Work commits after handler returns
- Direct call to another module: `_taskService.Create(...)` — use `_mediator.Send(new CreateTaskCommand(...))`
- `CreateTaskCommandHandler.cs` as file name — use `CreateTask.Handler.cs`
- Multiple top-level commands dispatched sequentially from one handler — design as a single orchestrating command
- `ICommand` defined in BuildingBlocks — modules would need a BuildingBlocks reference, violating layer rules
# Check list
- [ ] `ICommand` and `ICommand<TResponse>` defined in `Shared/MediatR/ICommand.cs`
- [ ] All commands declared as `record` in `/{Module}.Interfaces/Commands`
- [ ] All commands implement `ICommand<Result<T>>`
- [ ] Result records co-located with their command in the same file
- [ ] Each feature has its own folder under `/{Module}.Application/Features`
- [ ] Handler file named `{FeatureName}.Handler.cs`
- [ ] Handler class named `{FeatureName}Handler`
- [ ] Handler implements `IRequestHandler<TCommand, Result<T>>`
- [ ] Handler injects `IRepository<T>` — never DbContext
- [ ] Handler loads entities via named specs — no inline LINQ
- [ ] Handler follows load → guard → domain call → stage → return structure
- [ ] Handler returns `Result<T>` for all outcomes — no exceptions for flow control
- [ ] Handler never calls `SaveChangesAsync`
- [ ] Cross-module writes dispatched via `_mediator.Send()`
- [ ] Module has `Register{ModuleName}Module()` extension method
- [ ] Handlers registered via `AddMediatR` assembly scan
- [ ] Pipeline behaviors registered in App.Host — not in module registration

# Unittest TestCases
- [ ] When valid command is handled Then handler returns expected `Result.Created` or `Result.Success`
- [ ] When required entity not found during load Then handler returns `Result.NotFound`
- [ ] When business guard condition is met Then handler returns `Result.Conflict` before domain call
- [ ] When domain call completes Then entity is staged in repository — not yet persisted
- [ ] When sub-command fails Then root handler returns `Result.Error` without staging own entity
- [ ] When two handlers in same module Then both are discovered by assembly scan
- [ ] When command is dispatched from API Then correct handler is invoked by MediatR
