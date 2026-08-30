---
name: plateau-service-with-api--class-module-application-registration
description: Class {Module}ApplicationRegistration in the service-with-api plateau
whenToUse: when wiring a module's handlers and validators into DI, or adding this module's registration call to App.Host
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/class
  - plateau/service-with-api
created_by:
  - "[[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
---

# Goal
- Self-register all handlers and validators in this module's assembly via scan
- Give App.Host a single call surface for wiring up the module — no module internals exposed

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs.create]]

# Core Principles
- One static extension method per module — `Register{ModuleName}Module`
- `AddMediatR` scans the Application assembly — all `IRequestHandler` implementations registered automatically
- `AddValidatorsFromAssembly` scans the Application assembly — registers this module's command validators plus `{ValueObject}PropertyValidator`/`{Dto}Validator` from `solution-dto-property-validators`
- Pipeline behaviors are NOT registered here — that is App.Host's responsibility

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Module registration | {ModuleName}ApplicationRegistration | TaskApplicationRegistration | {ModuleName}ApplicationRegistration.cs | TaskApplicationRegistration.cs |

# Implementation
```csharp
//Skill: class-module-application-registration
//Plateau: service-with-api
//Version: 20260825120000

public static class {Module}ApplicationRegistration
{
    public static IServiceCollection Register{ModuleName}Module(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof({Module}ApplicationRegistration).Assembly));

        services.AddValidatorsFromAssembly(typeof({Module}ApplicationRegistration).Assembly);

        return services;
    }
}
```

Called from `App.Host/DependencyInjection/ModuleRegistration.cs`'s `AddModules()` — see `class-module-registration`.

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs.create]]

# Rules
MUST:
- Method named `Register{ModuleName}Module`, accept `IServiceCollection` and `IConfiguration`
- Register handlers via `AddMediatR` assembly scan
- Register validators via `AddValidatorsFromAssembly`
MUST NOT:
- Register pipeline behaviors — belongs in App.Host
- Reference another module's Application assembly

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs.create]]

# Check list
- [ ] `Register{ModuleName}Module` exists, called from `ModuleRegistration.AddModules()`
- [ ] Handlers and validators both registered via assembly scan
- [ ] No pipeline-behavior registration here

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs.create]]
