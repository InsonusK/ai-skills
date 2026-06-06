---
uid: 0bc550f2-f583-45ca-8c81-ba86cae3e241
status: draft
name: module-application
description: rules for structuring the Application layer of a module — handlers, validators, specs, and DI registration
domain: skill
type: pattern
tags:
  - dotnet
  - application
  - cqrs
  - module
  - mediatr
  - registration
triggers:
  - implement application layer
  - structure module application
  - register module
  - add feature to module
aliases:
  - Application Layer
  - Module Application
---
# Goal
Define the structure, responsibilities, and DI registration rules for a module's Application layer. The Application layer is the orchestration boundary — it connects the API contract (Interfaces) to the domain model, coordinates repositories, dispatches sub-commands, and registers all module components into DI. Without this pattern, handler placement, naming, and registration become inconsistent across modules and features are hard to locate.

# Core Principles
- Application layer orchestrates — it never contains business logic
- Each feature is a self-contained vertical slice — command/query, handler, validator in one folder
- Each module owns its DI registration — Host only calls `Register{ModuleName}Module()`
- Handlers implement contracts declared in `{Module}.Interfaces`
- Pipeline behaviors (ValidationBehavior, UnitOfWorkBehavior) registered once in BuildingBlocks — not per module
- No DbContext in Application — only `IRepository<T>`, `IReadRepository<T>`, and `IUnitOfWork`

# Structure / Contracts

## Full Application layer structure

```
/{Module}.Application
    /Features
        /{FeatureName}
            {FeatureName}.Handler.cs
            {FeatureName}.Validator.cs      ← only for commands
        /CreateTask
            CreateTask.Handler.cs
            CreateTask.Validator.cs
        /AssignTask
            AssignTask.Handler.cs
            AssignTask.Validator.cs
        /GetTask
            GetTask.Handler.cs              ← no validator for queries
        /GetTasks
            GetTasks.Handler.cs
    /EventHandlers
        {EventName}Handler.cs
        TaskAssignedEventHandler.cs
    /Specifications
        ActiveTasksByAssigneeSpec.cs        ← complex multi-condition specs
    {Module}ApplicationRegistration.cs
    {Module}.Application.csproj
```

## DI Registration — {Module}.Application

Each module exposes one extension method. Host calls it during startup. Handlers and validators auto-registered via assembly scan.

```csharp
// Task.Application/TaskApplicationRegistration.cs
public static class TaskApplicationRegistration
{
    public static IServiceCollection RegisterTaskModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // auto-registers all IRequestHandler<,> in this assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(TaskApplicationRegistration).Assembly));

        // auto-registers all AbstractValidator<> in this assembly
        services.AddValidatorsFromAssembly(
            typeof(TaskApplicationRegistration).Assembly);

        return services;
    }
}
```

## Pipeline behaviors registration — BuildingBlocks or App.Host

Behaviors are registered once for all modules — not per module. Order matters: ValidationBehavior must run before UnitOfWorkBehavior.

```csharp
// App.Host/DependencyInjection/PipelineRegistration.cs
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
```

## App.Host composition

Host calls each module's registration method — no module internals referenced directly.

```csharp
// App.Host/Program.cs
builder.Services
    .RegisterTaskModule(builder.Configuration)
    .RegisterTimeLogModule(builder.Configuration)
    .RegisterUserModule(builder.Configuration);
```

## Feature naming conventions

|Artifact|Pattern|Example|
|---|---|---|
|Feature folder|`{VerbNoun}`|`CreateTask`, `AssignTask`, `GetTask`|
|Handler file|`{FeatureName}.Handler.cs`|`CreateTask.Handler.cs`|
|Validator file|`{FeatureName}.Validator.cs`|`CreateTask.Validator.cs`|
|Handler class|`{FeatureName}Handler`|`CreateTaskHandler`|
|Validator class|`{FeatureName}Validator`|`CreateTaskValidator`|
|Event handler file|`{EventName}Handler.cs`|`TaskAssignedEventHandler.cs`|
|Event handler class|`{EventName}Handler`|`TaskAssignedEventHandler`|

## What belongs in Application vs Domain

|Concern|Layer|Reason|
|---|---|---|
|Load entity, call domain, return result|Application|Orchestration|
|Business invariant check|Domain|Entity or domain service|
|Transport validation (not empty, max length)|Application/Validator|FluentValidation|
|Complex multi-condition specification|Application/Specifications|Feature-specific query|
|Simple single-condition specification|Domain/Specifications|Reusable across features|
|Cross-module event reaction|Application/EventHandlers|Module side effect|

# Rules

MUST:

- Each feature in its own folder under `/Features`
- Handler file named `{FeatureName}.Handler.cs`
- Validator file named `{FeatureName}.Validator.cs` — only for commands
- Each module has `Register{ModuleName}Module()` extension method
- Handlers and validators registered via assembly scan — not manually
- Pipeline behaviors registered once in Host — not inside module registration
- No DbContext in Application — use repository abstractions only
- Event handlers live in `/EventHandlers` — not inside `/Features` 
SHOULD:
- Registration method accept `IConfiguration` for module-specific settings 
MUST NOT:
- Module registration reference another module's Application assembly
- Handler contain business logic — delegate to domain
- Validator contain business rules — transport correctness only
- Query handler have a validator — queries are read-only, validation not needed

# Anti-patterns
- Handler file named `CreateTaskCommandHandler.cs` — use `CreateTask.Handler.cs`
- All handlers registered manually one by one — use assembly scan
- Pipeline behaviors registered inside module registration — register once in Host
- Event handler placed inside `/Features` — belongs in `/EventHandlers`
- Module registration calls `services.AddDbContext` — infrastructure concern, belongs in App.Infrastructure registration

# Checklist
- [ ] Each feature has its own folder under `/Features`
- [ ] Handler and validator named `{FeatureName}.Handler.cs` / `{FeatureName}.Validator.cs`
- [ ] No validator for query handlers
- [ ] Event handlers in `/EventHandlers`
- [ ] Complex specs in `/Specifications`
- [ ] `Register{ModuleName}Module()` extension method defined
- [ ] Handlers and validators registered via assembly scan
- [ ] Pipeline behaviors registered in Host, not in module
- [ ] No DbContext referenced in any Application class

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/command-handler-pattern.skill]] — structure and rules for command handlers
- [[skills/dotnet/skill-graph/developing/Module/Application Layer/query-handler-pattern.skill]] — structure and rules for query handlers
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-event-handler-pattern.skill]] — event handlers in /EventHandlers
- [[skills/dotnet/skill-graph/developing/Module/Application Layer/ardalis-specification-pattern.skill]] — complex specs in /Specifications
- [[skills/dotnet/skill-graph/developing/Module/Application Layer/repository-pattern.skill]] — repository and UnitOfWork abstractions used here
- [[skills/dotnet/skill-graph/developing/Architecture/backend-project-structure.skill]] — Application layer in module structure