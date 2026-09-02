---
name: plateau-domain-service--class-pipeline-registration
description: Class PipelineRegistration in the plateau-domain-service plateau — the one AddPipeline() extension and the authoritative record of MediatR pipeline behavior order
whenToUse: when adding a pipeline behavior registration or changing behavior order, or checking where in the chain a new behavior belongs
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]]"
  - "[[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]]"
  - "[[../../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]"
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]"
---

# Goal
- Provide the single `AddPipeline()` extension where every MediatR pipeline behavior is registered, and be the one authoritative record of behavior execution order.

__Applied solutions:__
- [[../../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `static class`, one public `AddPipeline(this IServiceCollection) : IServiceCollection` — chainable in `Program.cs`.
- Each behavior is registered as an open generic: `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(XBehavior<,>))`, in execution order (first registered runs first).
- Order at plateau-domain-service: `ExceptionHandlingBehavior` (first) → `ValidationBehavior` → `ConcurrencyBehavior` (VP5, guards stale writes before work) → `UnitOfWorkBehavior` (VP2, **last** — commits what a fully-validated, non-stale handler staged).
- `GuidResolvingBehavior` (VP6) inserts between validation and concurrency at plateau-offline-sync-service.
- Behavior order lives **only** here — never in `Program.cs`, never in a module, never split across files.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Pipeline registration | `PipelineRegistration` | `PipelineRegistration` | `PipelineRegistration.cs` | `PipelineRegistration.cs` |

# Implementation
```csharp
// Skill: plateau-domain-service--class-pipeline-registration
// Plateau: core
// Version: 20260902000000
using BuildingBlocks.MediatR;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(this IServiceCollection services)
    {
        // 1. Global exception handler — first, so it wraps every other behavior and the handler.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

        // 2. Transport validation.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // 3. Optimistic-concurrency guard (VP5) — only IHasVersions commands.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConcurrencyBehavior<,>));

        // 4. Unit of work (VP2) — LAST, so it commits only a fully-guarded handler's staged changes.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

        return services;
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]
- [[../../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]] - [[../../../../../solutions/solution-validation-behavior.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Rules
MUST:
- Define `PipelineRegistration` as a `static class` in `App.Host/DependencyInjection/PipelineRegistration.cs`; `AddPipeline()` extends and returns `IServiceCollection`.
- Register every behavior as an open generic inside `AddPipeline()`, in execution order.
- Keep the order `ExceptionHandlingBehavior` → `ValidationBehavior` → `ConcurrencyBehavior` → `UnitOfWorkBehavior`; `UnitOfWorkBehavior` is always last.
- Never register a behavior in `Program.cs` or a module; never define pipeline order in more than one place; never create a second registration method.
- Never apply several plateau templates per class.

# Check list
- [ ] `AddPipeline()` in `App.Host/DependencyInjection/PipelineRegistration.cs`, returns `IServiceCollection`.
- [ ] `ExceptionHandlingBehavior` then `ValidationBehavior`, both open generics, in that order.
- [ ] No behavior registered anywhere else.

# Unittest TestCases
- [ ] WHEN `AddPipeline()` runs THEN the provider yields `IPipelineBehavior<,>` implementations in the order Exception, Validation.
- [ ] WHEN `AddPipeline()` runs THEN it returns the same `IServiceCollection` instance.
