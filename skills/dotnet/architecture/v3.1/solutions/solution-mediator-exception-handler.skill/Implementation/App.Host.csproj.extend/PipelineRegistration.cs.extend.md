---
description: Prepend ExceptionHandlingBehavior as the first registered MediatR pipeline behavior
project_name: App.Host
name: PipelineRegistration.cs
element_kind: class
change_kind: extend
tags:
  - solution/mediator-exception-handler
  - element/pipelineregistration-cs
---

# Goals
- Extend the centralized `PipelineRegistration.AddPipeline()` method to register `ExceptionHandlingBehavior`
- Ensure `ExceptionHandlingBehavior` is the first behavior in the registration order

# Core Principles
- `PipelineRegistration.cs` is the single source of truth for pipeline behavior order
- Behaviors are registered in execution order — first registered runs first

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Pipeline registration extension | `{Name}Registration` | `PipelineRegistration` | `{Name}Registration.cs` | `PipelineRegistration.cs` |

# Implementation changes

Assume `PipelineRegistration.AddPipeline()` already registers the behaviors required by other solution skills. Prepend `ExceptionHandlingBehavior` at the beginning of the method:

```csharp
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

        // Behaviors registered by other solution skills follow, in their own order —
        // e.g. ValidationBehavior (solution-mediator-integration), then any persistence /
        // concurrency / unit-of-work behaviors added by later solutions. This solution only
        // owns the first line above.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
```

# Rule changes

## MUST
- Prepend `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));` at the beginning of `AddPipeline()`
- Keep all behavior registrations inside `PipelineRegistration.cs`
- Never register `ExceptionHandlingBehavior` after other behaviors
- Never register `ExceptionHandlingBehavior` directly in `Program.cs`
- Never register `ExceptionHandlingBehavior` inside a module registration method

## SHOULD
- Avoid registering the exception handler last in the pipeline
  - Risk: exceptions thrown by outer behaviors (for example, during `UnitOfWorkBehavior` commit) are not caught
  - Fix: register `ExceptionHandlingBehavior` first in `AddPipeline()`

# Check list
- [ ] `AddPipeline()` exists in `App.Host/DependencyInjection/PipelineRegistration.cs`
- [ ] `ExceptionHandlingBehavior` registered via `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));`
- [ ] `ExceptionHandlingBehavior` is the first behavior registered in `AddPipeline()`
- [ ] `AddPipeline()` is called from `Program.cs`

# Unittest TestCases
- [ ] WHEN AddPipeline is called THEN ExceptionHandlingBehavior is registered as transient
- [ ] WHEN AddPipeline is called THEN ExceptionHandlingBehavior is registered before all other pipeline behaviors
