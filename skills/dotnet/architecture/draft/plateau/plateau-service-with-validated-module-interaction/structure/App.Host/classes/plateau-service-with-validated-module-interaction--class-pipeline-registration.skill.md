---
name: plateau-service-with-validated-module-interaction--class-pipeline-registration
description: Class PipelineRegistration in the service-with-validated-module-interaction plateau
whenToUse: when creating or editing PipelineRegistration, or adding a new pipeline behavior to the composition root
domain: skill
type: template
plateau: service-with-validated-module-interaction
version: 20260822140000
tags:
  - skill/template/class
  - plateau/service-with-validated-module-interaction
created_by:
  - "[[../../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]]"
  - "[[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]]"
---

# Goal
- Provide the single `AddPipeline()` extension method where all MediatR pipeline behaviors are registered
- Be the authoritative record of pipeline behavior order, with `ExceptionHandlingBehavior` registered first

__Applied solutions:__
- [[../../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]
- [[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Core Principles
- Apply ONE plateau template per class
- `PipelineRegistration` is a static class with one public extension method
- `AddPipeline()` returns `IServiceCollection` so it can be chained in `Program.cs`
- `PipelineRegistration.cs` is the single source of truth for pipeline behavior order — behaviors registered in execution order, first registered runs first
- Individual behavior solutions extend this method to insert their behaviors in order; the global exception handler is registered first so it wraps every other behavior

__Applied solutions:__
- [[../../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]
- [[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Pipeline registration | `PipelineRegistration` | `PipelineRegistration` | `PipelineRegistration.cs` | `PipelineRegistration.cs` |

# Implementation
```csharp
//Skill: class-pipeline-registration
//Plateau: service-with-validated-module-interaction
//Version: 20260822140000

// App.Host/DependencyInjection/PipelineRegistration.cs
using BuildingBlocks.MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(this IServiceCollection services)
    {
        // Global exception handler must be registered first so it wraps all other behaviors.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

        // Further behavior solutions (validation, concurrency, unit-of-work, ...)
        // extend this method to append their own registration here, in order.

        return services;
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]
- [[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Rules
MUST:
- `PipelineRegistration` defined as a static class in `App.Host/DependencyInjection/PipelineRegistration.cs`
- `AddPipeline()` is an extension method on `IServiceCollection`, returns `IServiceCollection`
- All behaviors registered inside `AddPipeline()` using `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`
- `ExceptionHandlingBehavior` registered first, before any other behavior
SHOULD:
- Keep `AddPipeline()` the only method that adds `IPipelineBehavior<,>` registrations
MUST NOT:
- Register behaviors inside module registration methods
- Define pipeline order in multiple files, or create multiple pipeline registration extension methods
- Register `ExceptionHandlingBehavior` after other behaviors, or directly in `Program.cs`

__Applied solutions:__
- [[../../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]
- [[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Check list
- [ ] `PipelineRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] `AddPipeline()` extension method on `IServiceCollection`, returns the same instance
- [ ] `ExceptionHandlingBehavior` is the first behavior registered in `AddPipeline()`
- [ ] `AddPipeline()` is called from `Program.cs`

__Applied solutions:__
- [[../../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]
- [[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Unittest TestCases
- [ ] WHEN `AddPipeline()` is called THEN `ExceptionHandlingBehavior` is registered as transient, before every other behavior
- [ ] WHEN `AddPipeline()` is called THEN it returns the same `IServiceCollection` instance it received

__Applied solutions:__
- [[../../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]
- [[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]
