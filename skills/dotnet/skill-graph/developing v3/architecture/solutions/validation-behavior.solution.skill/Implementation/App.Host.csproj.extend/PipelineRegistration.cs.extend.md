---
description: Add ValidationBehavior as the first pipeline behavior
project_name: App.Host
name: PipelineRegistration.cs
element_kind: class
change_kind: extend
---

# Goals
- Register `ValidationBehavior` as the first MediatR pipeline behavior

# Core Principles
- Behaviors registered in execution order — first registered runs first
- `ValidationBehavior` registered first so invalid requests are rejected before any other behavior activates
- `Transient` lifetime — new behavior instance per pipeline invocation

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs      ← extended with ValidationBehavior
```

# Implementation changes

Extend `PipelineRegistration` from [[pipeline-registration.solution.skill]] with `ValidationBehavior`:

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
        // 1. validation — rejects invalid commands and queries before anything else runs
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        // future behaviors are inserted after ValidationBehavior

        return services;
    }
}
```

# Rules

MUST:
- `ValidationBehavior` registered as the first `IPipelineBehavior` entry
- All behaviors registered as `AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`
- Behaviors registered in intended execution order
- Pipeline behaviors registered in App.Host — never inside a module's registration method

MUST NOT:
- Register behaviors inside module registration methods
- Change pipeline order in multiple files

SHOULD:
- `Transient` lifetime — new behavior instance per pipeline invocation

# Anti-patterns
- `ValidationBehavior` registered after other behaviors — invalid requests must be rejected before any side-effect behavior runs
- Pipeline order scattered across multiple files

# Check list
- [ ] `ValidationBehavior` registered first in `AddPipeline()`
- [ ] All behaviors registered as `AddTransient(typeof(IPipelineBehavior<,>), typeof(...))`
- [ ] Pipeline registration centralized in App.Host

# Unittest TestCases
- [ ] WHEN applied THEN Register ValidationBehavior as the first pipeline behavior
- [ ] WHEN applied THEN Behaviors registered in execution order — first registered runs first
- [ ] WHEN applied THEN ValidationBehavior is registered first so invalid requests are rejected before any other behavior activates
- [ ] WHEN applied THEN Transient lifetime — new behavior instance per pipeline invocation
- [ ] WHEN verified THEN ValidationBehavior registered first
- [ ] WHEN verified THEN All behaviors registered as AddTransient(typeof(IPipelineBehavior<,>), typeof(...))
