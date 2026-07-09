---
name: class-pipeline-registration
description: Centralized pipeline behavior registration extension
domain: skill
type: template
version: 20260704153836
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill|solution-pipeline-registration]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/solution-pipeline-registration-order.skill|solution-pipeline-registration-order]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]]"
---

# Goal
- Provide the single `AddPipeline()` extension method where all MediatR pipeline behaviors are registered
- Be the authoritative record of pipeline behavior order
- Provide the complete ordered set of `IPipelineBehavior<,>` registrations inside `AddPipeline()`
- Make the execution order explicit and self-documenting
- Register `ExceptionHandlingBehavior` first so it wraps all other behaviors and catches unhandled exceptions

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill|class-pipeline-registration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/solution-pipeline-registration-order.skill|solution-pipeline-registration-order]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]

# Core Principles
- Apply ONE plateau template per class
- `PipelineRegistration` is a static class with one public extension method
- `AddPipeline()` returns `IServiceCollection` so it can be chained in `Program.cs`
- Individual behavior solutions extend this method to insert their behaviors in order
- `PipelineRegistration` remains a static class with one public extension method
- Behaviors are registered in execution order — first registered runs first
- `ExceptionHandlingBehavior` is registered first so it wraps all subsequent behaviors and the handler

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill|class-pipeline-registration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/solution-pipeline-registration-order.skill|solution-pipeline-registration-order]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Pipeline registration | `PipelineRegistration` | `PipelineRegistration` | `PipelineRegistration.cs` | `PipelineRegistration.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill|class-pipeline-registration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/solution-pipeline-registration-order.skill|solution-pipeline-registration-order]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-pipeline-registration
//Plateau: default
//Version: 20260704153836
```

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

        // 1. Catch any unhandled exception from the pipeline and return a generic Result.Error.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

        // 2. Reject invalid transport input before any other work is done.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // 3. Short-circuit duplicate external-created entities before concurrency or commit.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GuidResolvingBehavior<,>));

        // 4. Guard against stale updates before opening a unit of work.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConcurrencyBehavior<,>));

        // 5. Commit all staged changes atomically after the handler completes.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

        return services;
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill|class-pipeline-registration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/solution-pipeline-registration-order.skill|solution-pipeline-registration-order]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]

# Rules
MUST:
	- `PipelineRegistration` defined as a static class in `App.Host/DependencyInjection/PipelineRegistration.cs`
	- `AddPipeline()` is an extension method on `IServiceCollection`
	- `AddPipeline()` returns `IServiceCollection`
	- Behaviors registered in this exact order:
	- `ExceptionHandlingBehavior` registered first
	- All behavior registrations use `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`
MUST NOT:
	- Register behaviors inside module registration methods
	- Define pipeline order in multiple files
	- Change the order of the five behaviors
	- Register `ExceptionHandlingBehavior` after other pipeline behaviors

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill|class-pipeline-registration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/solution-pipeline-registration-order.skill|solution-pipeline-registration-order]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- Pipeline order scattered across multiple files
- Registering behaviors in `Program.cs` instead of inside `PipelineRegistration`
- `UnitOfWorkBehavior` registered before earlier behaviors
- `ExceptionHandlingBehavior` registered last in the pipeline — exceptions from outer behaviors (for example, `UnitOfWorkBehavior` commit failures) will not be caught

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill|class-pipeline-registration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/solution-pipeline-registration-order.skill|solution-pipeline-registration-order]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]

# Check list
- [ ] `PipelineRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] `AddPipeline()` extension method on `IServiceCollection`
- [ ] `AddPipeline()` returns `IServiceCollection`
- [ ] `ExceptionHandlingBehavior` registered first
- [ ] `ValidationBehavior` registered second
- [ ] `GuidResolvingBehavior` registered third
- [ ] `ConcurrencyBehavior` registered fourth
- [ ] `UnitOfWorkBehavior` registered fifth
- [ ] All registrations use `AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill|class-pipeline-registration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/solution-pipeline-registration-order.skill|solution-pipeline-registration-order]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN `PipelineRegistration` class exists in `App.Host/DependencyInjection`
- [ ] WHEN applied THEN `AddPipeline()` extends `IServiceCollection`
- [ ] WHEN applied THEN `AddPipeline()` returns the same `IServiceCollection` instance
- [ ] WHEN applied THEN behaviors are registered in order ExceptionHandling → Validation → GuidResolving → Concurrency → UnitOfWork
- [ ] WHEN command is invalid THEN `ValidationBehavior` short-circuits before other behaviors
- [ ] WHEN duplicate Guid is sent THEN `GuidResolvingBehavior` short-circuits before `ConcurrencyBehavior`
- [ ] WHEN version mismatch occurs THEN `ConcurrencyBehavior` short-circuits before `UnitOfWorkBehavior`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill|class-pipeline-registration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/solution-pipeline-registration-order.skill|solution-pipeline-registration-order]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration-order.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]]
