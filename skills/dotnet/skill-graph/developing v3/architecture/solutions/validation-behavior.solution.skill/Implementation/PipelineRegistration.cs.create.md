---
description: Pipeline behavior registration extension
project_name: App.Host
name: PipelineRegistration.cs
change_kind: create
---

# Goals
- Register all MediatR pipeline behaviors in the single correct order
- Be the authoritative record of what behaviors exist and in what sequence they run

# Core Principles
- Behaviors registered in execution order — first registered runs first
- `ValidationBehavior` is registered first so invalid requests are rejected before any other behavior activates
- `Transient` lifetime — new behavior instance per pipeline invocation

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
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(
        this IServiceCollection services)
    {
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        return services;
    }
}
```

# Rules

MUST:
- `ValidationBehavior` registered as the first `IPipelineBehavior` entry
- All behaviors registered as `AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`
- Behaviors registered in intended execution order

MUST NOT:
- Register behaviors inside module registration methods
- Change pipeline order in multiple files
