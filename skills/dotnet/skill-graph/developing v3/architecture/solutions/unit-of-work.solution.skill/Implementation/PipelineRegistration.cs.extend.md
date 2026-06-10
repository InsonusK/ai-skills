---
description: Add UnitOfWorkBehavior registration after ValidationBehavior in pipeline
project_name: App.Host
name: PipelineRegistration.cs
change_kind: extend
---

# Goals
- Add `UnitOfWorkBehavior` as the last pipeline behavior — after `ValidationBehavior`

# Core Principles
- Behaviors registered in execution order — first registered runs first
- `ValidationBehavior` runs first so invalid requests are rejected before `UnitOfWorkBehavior` opens a unit of work
- `UnitOfWorkBehavior` runs last so it commits after all other behaviors and the handler complete
- `Transient` lifetime — new behavior instance per pipeline invocation

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs
```

# Implementation changes

Extend `PipelineRegistration` from [[validation-behavior.solution.skill]] with `UnitOfWorkBehavior`:

```csharp
// App.Host/DependencyInjection/PipelineRegistration.cs
using BuildingBlocks.MediatR;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(
        this IServiceCollection services)
    {
        // 1. validation — rejects invalid commands before anything else runs
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        // 2. unit of work — commits staged changes after handler completes
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(UnitOfWorkBehavior<,>));

        // future behaviors are inserted between Validation and UnitOfWork

        return services;
    }
}
```

# Rules

MUST:
- `UnitOfWorkBehavior` registered after `ValidationBehavior` — invalid commands never open a unit of work
- All behaviors registered as `AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`
- Behaviors registered in intended execution order
- Pipeline behaviors registered in App.Host — never inside a module's registration method

MUST NOT:
- `UnitOfWorkBehavior` registered before `ValidationBehavior` — would waste a commit attempt on invalid input
- Register behaviors inside module registration methods
- Change pipeline order in multiple files

# Anti-patterns
- `UnitOfWorkBehavior` registered before `ValidationBehavior` — invalid commands trigger unnecessary commit attempts
- Pipeline order scattered across multiple files — single source of truth must be `PipelineRegistration.cs`

# Check list
- [ ] `UnitOfWorkBehavior` registered after `ValidationBehavior`
- [ ] All behaviors registered as `AddTransient(typeof(IPipelineBehavior<,>), typeof(...))`
- [ ] Pipeline registration centralized in App.Host
