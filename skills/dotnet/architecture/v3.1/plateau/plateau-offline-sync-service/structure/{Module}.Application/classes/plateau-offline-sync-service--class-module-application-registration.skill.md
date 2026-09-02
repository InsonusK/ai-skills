---
name: plateau-offline-sync-service--class-module-application-registration
description: Class {Module}ApplicationRegistration in the plateau-offline-sync-service plateau — the module's one DI self-registration extension (AddMediatR + AddValidatorsFromAssembly)
whenToUse: when creating or editing a module's registration extension, or checking what a module is allowed to register
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
---

# Goal
- Self-register every handler and validator in the module's assembly by scan, and give `App.Host` a single call surface (`Register{ModuleName}Module`) with no module internals exposed.

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- One `static` extension method per module: `Register{ModuleName}Module(this IServiceCollection, IConfiguration)`.
- `AddMediatR` scans the Application assembly for `IRequestHandler` / `INotificationHandler`.
- `AddValidatorsFromAssembly` scans the same assembly — command validators, `{ValueObject}PropertyValidator`, `{Dto}Validator` all registered at once.
- Pipeline behaviors are **not** registered here — that is `App.Host`'s job.
- Also registers each versioned entity's `{Entity}VersionResolver` (VP5) and each external-created entity's `IGuidResolver<Result<Create{Entity}Result>>` → `Create{Entity}GuidResolver` (VP6), both `Scoped` — the pipeline behaviors resolve these per request.
- Never registers a `DbContext` or infrastructure service; never references another module's Application assembly.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Module registration | `{ModuleName}ApplicationRegistration` | `TaskApplicationRegistration` | `{ModuleName}ApplicationRegistration.cs` | `TaskApplicationRegistration.cs` |

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-module-application-registration
// Plateau: core
// Version: 20260902000000
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace {Module}.Application;

public static class {Module}ApplicationRegistration
{
    public static IServiceCollection Register{ModuleName}Module(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof({Module}ApplicationRegistration).Assembly));

        services.AddValidatorsFromAssembly(typeof({Module}ApplicationRegistration).Assembly);

        return services;
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs.create]]

# Rules
MUST:
- Name the method `Register{ModuleName}Module`; accept `IServiceCollection` and `IConfiguration`; return `IServiceCollection`.
- Register handlers via `AddMediatR` scan and validators via `AddValidatorsFromAssembly`.
- Never register a pipeline behavior, a `DbContext`, or an infrastructure service here.
- Never reference another module's Application assembly.
- Never apply several plateau templates per class.

# Check list
- [ ] `Register{ModuleName}Module(IServiceCollection, IConfiguration)` exists, returns `IServiceCollection`.
- [ ] `AddMediatR` + `AddValidatorsFromAssembly` both scan this module's assembly.
- [ ] No behavior / `DbContext` / infrastructure registration.

# Unittest TestCases
- [ ] WHEN `Register{ModuleName}Module` runs THEN every `IRequestHandler` in the assembly resolves from the provider.
- [ ] WHEN `Register{ModuleName}Module` runs THEN every `AbstractValidator<T>` in the assembly resolves as `IValidator<T>`.
