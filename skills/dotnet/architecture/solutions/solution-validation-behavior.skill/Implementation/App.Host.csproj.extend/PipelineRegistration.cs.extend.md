---
description: Register ValidationBehavior in the centralized MediatR pipeline
project_name: App.Host
name: PipelineRegistration.cs
element_kind: class
change_kind: extend
tags:
  - solution/validation-behavior
  - element/pipelineregistration-cs
---

# Goals
- Extend the centralized `PipelineRegistration.AddPipeline()` method to register `ValidationBehavior`
- Insert it right after `ExceptionHandlingBehavior`, before every behavior that assumes a request already passed transport validation

# Core Principles
- `PipelineRegistration.cs` is the single source of truth for pipeline behavior order — this solution extends the existing method, it never creates a second registration file
- `ValidationBehavior` must run before `ConcurrencyBehavior`, `GuidResolvingBehavior`, and `UnitOfWorkBehavior` — none of them should run against a request that failed transport validation

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs
```

# Implementation changes

Insert `ValidationBehavior` immediately after `ExceptionHandlingBehavior` (if `solution-mediator-exception-handler` is applied) or as the first registration (if it is not):

```csharp
// App.Host/DependencyInjection/PipelineRegistration.cs
using BuildingBlocks.MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(this IServiceCollection services)
    {
        // ExceptionHandlingBehavior, if applied, precedes this line — see solution-mediator-exception-handler.

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Behaviors registered by other solution skills follow this line, in their own documented order.

        return services;
    }
}
```

# Rule changes

## MUST
- Register `ValidationBehavior` after `ExceptionHandlingBehavior` (when applied) and before `ConcurrencyBehavior`, `GuidResolvingBehavior`, and `UnitOfWorkBehavior` (whichever are applied)
- Keep the registration inside the existing `PipelineRegistration.cs` — never a second registration file
- Never register `ValidationBehavior` after any behavior that assumes a validated request (`ConcurrencyBehavior`, `GuidResolvingBehavior`, `UnitOfWorkBehavior`)

# Check list
- [ ] `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));` present in `AddPipeline()`
- [ ] It precedes `ConcurrencyBehavior`/`GuidResolvingBehavior`/`UnitOfWorkBehavior`'s registrations, wherever those are applied
- [ ] It follows `ExceptionHandlingBehavior`'s registration, when that solution is applied

# Unittest TestCases
- [ ] WHEN AddPipeline is called THEN ValidationBehavior is registered as transient
- [ ] WHEN AddPipeline is called THEN ValidationBehavior is registered before ConcurrencyBehavior, GuidResolvingBehavior, and UnitOfWorkBehavior
