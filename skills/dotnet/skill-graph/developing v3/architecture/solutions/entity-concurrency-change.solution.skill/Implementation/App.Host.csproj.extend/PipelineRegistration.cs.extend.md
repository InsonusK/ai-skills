---
description: Insert ConcurrencyBehavior between ValidationBehavior and UnitOfWorkBehavior
project_name: App.Host
name: PipelineRegistration.cs
element_kind: class
change_kind: extend
---

# Goals
- Insert `ConcurrencyBehavior` between `ValidationBehavior` and `UnitOfWorkBehavior`

# Core Principles
- `ValidationBehavior` runs first — rejects invalid input before anything else
- `ConcurrencyBehavior` runs second — rejects stale versions before unit of work opens
- `UnitOfWorkBehavior` runs third — commits after handler completes

# Implementation changes

Extend `PipelineRegistration` from [[pipeline-registration.solution.skill]] with `ConcurrencyBehavior`:

```csharp
// App.Host/DependencyInjection/PipelineRegistration.cs
public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(
        this IServiceCollection services)
    {
        // 1. validation — rejects invalid input before anything else
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        // 2. concurrency — rejects stale versions before unit of work opens
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ConcurrencyBehavior<,>));

        // 3. unit of work — commits staged changes after handler completes
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(UnitOfWorkBehavior<,>));

        return services;
    }
}
```

# Rules

MUST:
- `ConcurrencyBehavior` registered after `ValidationBehavior` and before `UnitOfWorkBehavior`

MUST NOT:
- `ConcurrencyBehavior` registered after `UnitOfWorkBehavior` — stale commands would open a unit of work

# Anti-patterns
- `ConcurrencyBehavior` registered after `UnitOfWorkBehavior` — stale commands open a unit of work unnecessarily

# Check list
- [ ] `ValidationBehavior` registered first
- [ ] `ConcurrencyBehavior` registered second
- [ ] `UnitOfWorkBehavior` registered third

# Unittest TestCases
- [ ] WHEN applied THEN Insert ConcurrencyBehavior between ValidationBehavior and UnitOfWorkBehavior
- [ ] WHEN applied THEN ValidationBehavior runs first — rejects invalid input before anything else
- [ ] WHEN applied THEN ConcurrencyBehavior runs second — rejects stale versions before unit of work opens
- [ ] WHEN applied THEN UnitOfWorkBehavior runs third — commits after handler completes
- [ ] WHEN verified THEN ValidationBehavior registered first
- [ ] WHEN verified THEN ConcurrencyBehavior registered second
- [ ] WHEN verified THEN UnitOfWorkBehavior registered third
