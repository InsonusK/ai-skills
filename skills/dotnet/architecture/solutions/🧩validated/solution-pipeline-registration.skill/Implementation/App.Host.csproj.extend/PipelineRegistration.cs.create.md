---
description: Centralized pipeline behavior registration extension
project_name: App.Host
name: PipelineRegistration.cs
element_kind: class
change_kind: create
---

# Goals
- Provide the single `AddPipeline()` extension method where all MediatR pipeline behaviors are registered
- Be the authoritative record of pipeline behavior order

# Core Principles
- `PipelineRegistration` is a static class with one public extension method
- `AddPipeline()` returns `IServiceCollection` so it can be chained in `Program.cs`
- Individual behavior solutions extend this method to insert their behaviors in order

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
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(
        this IServiceCollection services)
    {
        // Pipeline behaviors are registered here in execution order.
        // Each behavior solution extends this method to add its own behavior.

        return services;
    }
}
```
# Rule changes

## MUST
- `PipelineRegistration` defined as a static class in `App.Host/DependencyInjection/PipelineRegistration.cs`
- `AddPipeline()` is an extension method on `IServiceCollection`
- `AddPipeline()` returns `IServiceCollection`
- `PipelineRegistration.cs` defined in `App.Host/DependencyInjection/PipelineRegistration.cs`
- All behaviors registered inside `AddPipeline()` using `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`
- Behaviors registered in intended execution order

## MUST NOT
- Register behaviors inside module registration methods
- Define pipeline order in multiple files
- Create multiple pipeline registration extension methods

## SHOULD
- Keep `AddPipeline()` the only method that adds `IPipelineBehavior<,>` registrations

# Anti-patterns
- Pipeline order scattered across multiple files
- Registering behaviors in `Program.cs` instead of inside `PipelineRegistration`

# Check list
- [ ] `PipelineRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] `AddPipeline()` extension method on `IServiceCollection`
- [ ] `AddPipeline()` returns `IServiceCollection`

# Unittest TestCases
- [ ] WHEN applied THEN `PipelineRegistration` class exists in `App.Host/DependencyInjection`
- [ ] WHEN applied THEN `AddPipeline()` extends `IServiceCollection`
- [ ] WHEN applied THEN `AddPipeline()` returns the same `IServiceCollection` instance
