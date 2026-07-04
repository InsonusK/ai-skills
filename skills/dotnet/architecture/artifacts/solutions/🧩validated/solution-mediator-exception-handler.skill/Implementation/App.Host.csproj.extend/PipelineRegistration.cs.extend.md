---
description: Append ExceptionHandlingBehavior as the last registered MediatR pipeline behavior
project_name: App.Host
name: PipelineRegistration.cs
element_kind: class
change_kind: extend
---

# Goals
- Extend the centralized `PipelineRegistration.AddPipeline()` method to register `ExceptionHandlingBehavior`
- Ensure `ExceptionHandlingBehavior` is the last behavior in the registration order

# Core Principles
- `PipelineRegistration.cs` is the single source of truth for pipeline behavior order
- Behaviors are registered in execution order — first registered runs first
- `ExceptionHandlingBehavior` registered last wraps the handler and any behaviors registered before it

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

Assume `PipelineRegistration.AddPipeline()` already registers the behaviors required by other solution skills. Append `ExceptionHandlingBehavior` at the end of the method:

```csharp
// App.Host/DependencyInjection/PipelineRegistration.cs
using BuildingBlocks.MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(this IServiceCollection services)
    {
        // Behaviors registered by other solution skills:
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GuidResolvingBehavior<,>));
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConcurrencyBehavior<,>));
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

        // Global exception handler must be registered last.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

        return services;
    }
}
```

If the project also uses `solution-pipeline-registration-order.skill`, update that skill's ordering so `ExceptionHandlingBehavior` follows `UnitOfWorkBehavior`:

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GuidResolvingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConcurrencyBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
```

# Rule changes

## MUST
- Append `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));` at the end of `AddPipeline()`
- Keep all behavior registrations inside `PipelineRegistration.cs`

## MUST NOT
- Register `ExceptionHandlingBehavior` before other behaviors
- Register `ExceptionHandlingBehavior` directly in `Program.cs`
- Register `ExceptionHandlingBehavior` inside a module registration method

# Anti-patterns
- **Registering the exception handler first**
  - Consequence: exceptions thrown by outer behaviors (for example, during `UnitOfWorkBehavior` commit) are not caught
  - Instead: register `ExceptionHandlingBehavior` last in `AddPipeline()`

# Check list
- [ ] `AddPipeline()` exists in `App.Host/DependencyInjection/PipelineRegistration.cs`
- [ ] `ExceptionHandlingBehavior` registered via `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));`
- [ ] `ExceptionHandlingBehavior` is the last behavior registered in `AddPipeline()`
- [ ] `AddPipeline()` is called from `Program.cs`

# Unittest TestCases
- [ ] WHEN AddPipeline is called THEN ExceptionHandlingBehavior is registered as transient
- [ ] WHEN AddPipeline is called THEN ExceptionHandlingBehavior is registered after all other pipeline behaviors
