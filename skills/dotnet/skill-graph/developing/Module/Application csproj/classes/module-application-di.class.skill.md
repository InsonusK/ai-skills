---
uid: 0b9aba27-b707-4b9b-85a5-52e899afe18e
name: module-application-di
description: defines how to implement the DI registration class for a module's Application layer
domain: skill
type: class
tags:
  - skill/pattern/class
  - dotnet
  - application
  - dependency-injection
  - registration
triggers:
  - implement module registration
  - register module in DI
  - application DI registration
---
# Goal
Define how to implement the single DI registration entry point for a module's Application layer. Each module exposes one extension method that App.Host calls during startup. Handlers and validators are auto-registered via assembly scan. Module-specific services (resolvers, etc.) are registered explicitly here.

# Core Principles
- One registration class per module — [[input heap/app-host-di-registration.class.skill|app-host-di-registration.class.skill]] calls it, knows nothing else about the module internals
- Handlers and validators registered via assembly scan — never manually one by one
- Pipeline behaviors NOT registered here — that belongs in [[input heap/app-host-di-registration.class.skill|app-host-di-registration.class.skill]]
- `services.AddDbContext` NOT registered here — that belongs in [[app-infrastructure.csproj.skill|App.Infrastructure]] registration
- Module-specific services ([[skills/dotnet/skill-graph/developing/Module/Application csproj/classes/module-application-resolver.class.skill|IGuidResolver Implementation]]) registered explicitly here

# Structure
## Place in csproj
Defined in [[skills/dotnet/skill-graph/developing/Module/Application csproj/module-application.csproj.skill|module-application-csproj.skill]]
```
/{ModuleName}.Application
  {ModuleName}ApplicationRegistration.cs
```

## Naming convention
class name: `{ModuleName}ApplicationRegistration`
file name: `{ModuleName}ApplicationRegistration.cs`
rule: Module name with ApplicationRegistration suffix

## Implementation
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
            cfg.RegisterServicesFromAssembly(
                typeof(TaskApplicationRegistration).Assembly));

        // auto-registers all AbstractValidator<> in this assembly
        services.AddValidatorsFromAssembly(
            typeof(TaskApplicationRegistration).Assembly);

        // explicit registration for module-specific services
        services.AddScoped<
            IGuidResolver<CreateTaskCommand, Result<CreateTaskResult>>,
            CreateTaskGuidResolver>();

        return services;
    }
}
```

## App.Host calls it
Called in [[app-host-di-registration.class.skill]]


## What Gets Registered Here

| Service                               | How                                           |
| ------------------------------------- | --------------------------------------------- |
| `IRequestHandler<,>` implementations  | Assembly scan via `AddMediatR`                |
| `AbstractValidator<>` implementations | Assembly scan via `AddValidatorsFromAssembly` |
| `IGuidResolver<TRequest, TResponse>`  | Explicit `AddScoped` per command              |

## What Does NOT Get Registered Here

- Pipeline behaviors (`ValidationBehavior`, `UnitOfWorkBehavior`) — register once in App.Host
- `DbContext` — belongs in App.Infrastructure registration
- `IRepository<T>`, `IUnitOfWork` — belongs in App.Infrastructure registration

# Rules
MUST:
- Class is `public static`
- Extension method named `Register{ModuleName}Module`
- Accepts `IServiceCollection` and `IConfiguration`
- Returns `IServiceCollection` for chaining
- Handlers and validators registered via assembly scan
- One `IGuidResolver` registration per command that implements `IHasGuid` 
MUST NOT:
- Register pipeline behaviors
- Register `DbContext` or infrastructure services
- Reference another module's Application assembly

# Anti-patterns
- Registering handlers manually: `services.AddScoped<CreateTaskHandler>()` — use assembly scan
- Registering `ValidationBehavior` here — belongs in App.Host, registered once for all modules
- Calling `services.AddDbContext` here — infrastructure concern

# Checklist
- [ ] Class named `{ModuleName}ApplicationRegistration`
- [ ] Method named `Register{ModuleName}Module`
- [ ] `AddMediatR` with assembly scan present
- [ ] `AddValidatorsFromAssembly` with assembly scan present
- [ ] One `IGuidResolver` registration per `IHasGuid` command
- [ ] No pipeline behavior registrations
- [ ] No infrastructure registrations

# Unittest TestCases
- [ ] When `RegisterTaskModule` called Then all handlers resolvable from DI
- [ ] When `RegisterTaskModule` called Then all validators resolvable from DI
- [ ] When `RegisterTaskModule` called Then IGuidResolver resolvable for each IHasGuid command

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/classes/module-application-resolver.class.skill|module-application-resolver.class.skill]] — resolvers registered explicitly here
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/module-application.csproj.skill|module-application.csproj.skill]] — this class lives in the Application project
- [[input heap/app-host-di-registration.class.skill|app-host-di-registration.class.skill]] — Host calls this registration method