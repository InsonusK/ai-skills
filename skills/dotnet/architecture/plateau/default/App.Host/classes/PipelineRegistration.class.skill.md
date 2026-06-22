---
uid: 3ad2845e-5a32-49df-9d1c-ef22bbb05a8a
name: pipelineregistration-class
description: Centralized pipeline behavior registration extension
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order.solution.skill]]"
---

# Goal
- Provide the single `AddPipeline()` extension method where all MediatR pipeline behaviors are registered
- Be the authoritative record of pipeline behavior order
- Provide the complete ordered set of `IPipelineBehavior<,>` registrations inside `AddPipeline()`
- Make the execution order explicit and self-documenting

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Core Principals
- `PipelineRegistration` is a static class with one public extension method
- `AddPipeline()` returns `IServiceCollection` so it can be chained in `Program.cs`
- Individual behavior solutions extend this method to insert their behaviors in order
- `PipelineRegistration` remains a static class with one public extension method
- Behaviors are registered in execution order — first registered runs first

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Pipeline registration | `PipelineRegistration` | `PipelineRegistration` | `PipelineRegistration.cs` | `PipelineRegistration.cs` |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Implementation
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

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Rules
MUST:
	- `PipelineRegistration` defined as a static class in `App.Host/DependencyInjection/PipelineRegistration.cs`
	- `AddPipeline()` is an extension method on `IServiceCollection`
	- `AddPipeline()` returns `IServiceCollection`
	- Behaviors registered in this exact order:
	- All behavior registrations use `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`
MUST NOT:
	- Register behaviors inside module registration methods
	- Define pipeline order in multiple files
	- Change the order of the four behaviors

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Anti-patterns
- Pipeline order scattered across multiple files
- Registering behaviors in `Program.cs` instead of inside `PipelineRegistration`
- `UnitOfWorkBehavior` registered before earlier behaviors

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Check list
- [ ] `PipelineRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] `AddPipeline()` extension method on `IServiceCollection`
- [ ] `AddPipeline()` returns `IServiceCollection`
- [ ] `ValidationBehavior` registered first
- [ ] `GuidResolvingBehavior` registered second
- [ ] `ConcurrencyBehavior` registered third
- [ ] `UnitOfWorkBehavior` registered fourth
- [ ] All registrations use `AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Unittest TestCases
- [ ] WHEN applied THEN `PipelineRegistration` class exists in `App.Host/DependencyInjection`
- [ ] WHEN applied THEN `AddPipeline()` extends `IServiceCollection`
- [ ] WHEN applied THEN `AddPipeline()` returns the same `IServiceCollection` instance
- [ ] WHEN applied THEN behaviors are registered in order Validation → GuidResolving → Concurrency → UnitOfWork
- [ ] WHEN command is invalid THEN `ValidationBehavior` short-circuits before other behaviors
- [ ] WHEN duplicate Guid is sent THEN `GuidResolvingBehavior` short-circuits before `ConcurrencyBehavior`
- [ ] WHEN version mismatch occurs THEN `ConcurrencyBehavior` short-circuits before `UnitOfWorkBehavior`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]
