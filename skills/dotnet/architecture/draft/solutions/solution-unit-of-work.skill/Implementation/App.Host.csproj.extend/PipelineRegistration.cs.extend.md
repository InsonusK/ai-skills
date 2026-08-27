---
description: Register UnitOfWorkBehavior as the last MediatR pipeline behavior
project_name: App.Host
name: PipelineRegistration.cs
element_kind: class
change_kind: extend
tags:
  - solution/unit-of-work
  - element/pipelineregistration-cs
---

# Goals
- Extend the centralized `PipelineRegistration.AddPipeline()` method to register `UnitOfWorkBehavior`
- Ensure `UnitOfWorkBehavior` is the last behavior in the registration order, so every guard/validation behavior already ran before it commits

# Core Principles
- `PipelineRegistration.cs` is the single source of truth for pipeline behavior order — this solution extends the existing method, it never creates a second registration file
- `UnitOfWorkBehavior` must run last — it commits whatever the handler staged, so every behavior that can reject the request first (`ValidationBehavior`, `ConcurrencyBehavior`, `GuidResolvingBehavior`) must already have run

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs
```

# Implementation changes

Append `UnitOfWorkBehavior` after every other applied behavior:

```csharp
// App.Host/DependencyInjection/PipelineRegistration.cs
using BuildingBlocks.MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(this IServiceCollection services)
    {
        // ExceptionHandlingBehavior, ValidationBehavior, ConcurrencyBehavior, GuidResolvingBehavior
        // (whichever are applied) precede this line.

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

        return services;
    }
}
```

# Rule changes

## MUST
- Register `UnitOfWorkBehavior` last — after every other applied pipeline behavior
- Keep the registration inside the existing `PipelineRegistration.cs` — never a second registration file

## MUST NOT
- Register `UnitOfWorkBehavior` before `ValidationBehavior`, `ConcurrencyBehavior`, or `GuidResolvingBehavior` — a rejected request must never reach the commit stage

# Check list
- [ ] `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));` present in `AddPipeline()`
- [ ] It is the last behavior registered, after every other applied behavior

# Unittest TestCases
- [ ] WHEN AddPipeline is called THEN UnitOfWorkBehavior is registered as transient
- [ ] WHEN AddPipeline is called THEN UnitOfWorkBehavior is registered after every other applied pipeline behavior
