---
description: Register all pipeline behaviors in the canonical execution order
project_name: App.Host
name: PipelineRegistration.cs
element_kind: class
change_kind: extend
---

# Goals
- Provide the complete ordered set of `IPipelineBehavior<,>` registrations inside `AddPipeline()`
- Make the execution order explicit and self-documenting

# Core Principles
- `PipelineRegistration` remains a static class with one public extension method
- `AddPipeline()` returns `IServiceCollection` so it can be chained in `Program.cs`

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
| Pipeline registration | `PipelineRegistration` | `PipelineRegistration` | `PipelineRegistration.cs` | `PipelineRegistration.cs` |

# Implementation changes

```csharp
// App.Host/DependencyInjection/PipelineRegistration.cs
using BuildingBlocks.MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(
        this IServiceCollection services)
    {
        // Pipeline behaviors are registered in execution order.
        // First registered runs first.

        // 1. Reject invalid transport input before any other work is done.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // 2. Short-circuit duplicate external-created entities before concurrency or commit.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GuidResolvingBehavior<,>));

        // 3. Guard against stale updates before opening a unit of work.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConcurrencyBehavior<,>));

        // 4. Commit all staged changes atomically after the handler completes.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

        return services;
    }
}
```
# Rule changes

## MUST
- Behaviors registered in this exact order:
  1. `ValidationBehavior`
  2. `GuidResolvingBehavior`
  3. `ConcurrencyBehavior`
  4. `UnitOfWorkBehavior`
- All behavior registrations use `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`
- `AddPipeline()` returns `IServiceCollection`
- `PipelineRegistration.cs` defined in `App.Host/DependencyInjection/PipelineRegistration.cs`

## MUST NOT
- Register behaviors inside module registration methods
- Define pipeline order in multiple files
- Change the order of the four behaviors
- Create multiple pipeline registration extension methods
- Register behaviors directly in `Program.cs`

## SHOULD
- Keep `AddPipeline()` the only method that adds `IPipelineBehavior<,>` registrations

# Anti-patterns
- Pipeline order scattered across multiple files
- Registering behaviors in `Program.cs` instead of inside `PipelineRegistration`
- `UnitOfWorkBehavior` registered before earlier behaviors

# Check list
- [ ] `ValidationBehavior` registered first
- [ ] `GuidResolvingBehavior` registered second
- [ ] `ConcurrencyBehavior` registered third
- [ ] `UnitOfWorkBehavior` registered fourth
- [ ] All registrations use `AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`
- [ ] `AddPipeline()` returns `IServiceCollection`

# Unittest TestCases
- [ ] WHEN applied THEN behaviors are registered in order Validation → GuidResolving → Concurrency → UnitOfWork
- [ ] WHEN command is invalid THEN `ValidationBehavior` short-circuits before other behaviors
- [ ] WHEN duplicate Guid is sent THEN `GuidResolvingBehavior` short-circuits before `ConcurrencyBehavior`
- [ ] WHEN version mismatch occurs THEN `ConcurrencyBehavior` short-circuits before `UnitOfWorkBehavior`
