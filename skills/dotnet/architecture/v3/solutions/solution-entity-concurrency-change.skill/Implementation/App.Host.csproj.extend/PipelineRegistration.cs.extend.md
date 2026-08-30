---
description: Register ConcurrencyBehavior in the centralized MediatR pipeline
project_name: App.Host
name: PipelineRegistration.cs
element_kind: class
change_kind: extend
tags:
  - solution/entity-concurrency-change
  - element/pipelineregistration-cs
---

# Goals
- Extend the centralized `PipelineRegistration.AddPipeline()` method to register `ConcurrencyBehavior`
- Insert it after `ValidationBehavior` (so it never runs against a transport-invalid request) and before `GuidResolvingBehavior`/`UnitOfWorkBehavior`

# Core Principles
- `PipelineRegistration.cs` is the single source of truth for pipeline behavior order — this solution extends the existing method, it never creates a second registration file
- `ConcurrencyBehavior` must run before `UnitOfWorkBehavior` — a stale-version write must never reach the commit stage

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs
```

# Implementation changes

Insert `ConcurrencyBehavior` right after `ValidationBehavior`'s registration (see `solution-validation-behavior`):

```csharp
// App.Host/DependencyInjection/PipelineRegistration.cs
using BuildingBlocks.MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(this IServiceCollection services)
    {
        // ExceptionHandlingBehavior, ValidationBehavior (if applied) precede this line.

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConcurrencyBehavior<,>));

        // GuidResolvingBehavior, UnitOfWorkBehavior (if applied) follow this line.

        return services;
    }
}
```

# Rule changes

## MUST
- Register `ConcurrencyBehavior` after `ValidationBehavior` (when applied) and before `GuidResolvingBehavior`/`UnitOfWorkBehavior` (whichever are applied)
- Keep the registration inside the existing `PipelineRegistration.cs` — never a second registration file

## MUST NOT
- Register `ConcurrencyBehavior` after `UnitOfWorkBehavior` — a version mismatch must be caught before any commit is staged

# Check list
- [ ] `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConcurrencyBehavior<,>));` present in `AddPipeline()`
- [ ] It follows `ValidationBehavior`'s registration, when that solution is applied
- [ ] It precedes `GuidResolvingBehavior`/`UnitOfWorkBehavior`'s registrations, wherever those are applied

# Unittest TestCases
- [ ] WHEN AddPipeline is called THEN ConcurrencyBehavior is registered as transient
- [ ] WHEN AddPipeline is called THEN ConcurrencyBehavior is registered after ValidationBehavior and before UnitOfWorkBehavior
