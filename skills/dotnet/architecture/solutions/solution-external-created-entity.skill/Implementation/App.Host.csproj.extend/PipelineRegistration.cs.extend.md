---
description: Register GuidResolvingBehavior in the centralized MediatR pipeline
project_name: App.Host
name: PipelineRegistration.cs
element_kind: class
change_kind: extend
tags:
  - solution/external-created-entity
  - element/pipelineregistration-cs
---

# Goals
- Extend the centralized `PipelineRegistration.AddPipeline()` method to register `GuidResolvingBehavior`
- Insert it after `ConcurrencyBehavior` and before `UnitOfWorkBehavior`

# Core Principles
- `PipelineRegistration.cs` is the single source of truth for pipeline behavior order — this solution extends the existing method, it never creates a second registration file
- `GuidResolvingBehavior` must run before `UnitOfWorkBehavior` — a detected duplicate short-circuits with the resolver's `ConflictResult<T>` and the handler (and any commit) never runs

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs
```

# Implementation changes

Insert `GuidResolvingBehavior` right after `ConcurrencyBehavior`'s registration (see `solution-entity-concurrency-change`), or after `ValidationBehavior` if concurrency control isn't applied:

```csharp
// App.Host/DependencyInjection/PipelineRegistration.cs
using BuildingBlocks.MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(this IServiceCollection services)
    {
        // ExceptionHandlingBehavior, ValidationBehavior, ConcurrencyBehavior (if applied) precede this line.

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GuidResolvingBehavior<,>));

        // UnitOfWorkBehavior (if applied) follows this line.

        return services;
    }
}
```

# Rule changes

## MUST
- Register `GuidResolvingBehavior` after `ValidationBehavior`/`ConcurrencyBehavior` (whichever are applied) and before `UnitOfWorkBehavior` (when applied)
- Keep the registration inside the existing `PipelineRegistration.cs` — never a second registration file
- Never register `GuidResolvingBehavior` after `UnitOfWorkBehavior` — a duplicate-Guid short-circuit must happen before any commit is staged

# Check list
- [ ] `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GuidResolvingBehavior<,>));` present in `AddPipeline()`
- [ ] It follows `ConcurrencyBehavior`'s registration, when that solution is applied
- [ ] It precedes `UnitOfWorkBehavior`'s registration, when that solution is applied

# Unittest TestCases
- [ ] WHEN AddPipeline is called THEN GuidResolvingBehavior is registered as transient
- [ ] WHEN AddPipeline is called THEN GuidResolvingBehavior is registered before UnitOfWorkBehavior
