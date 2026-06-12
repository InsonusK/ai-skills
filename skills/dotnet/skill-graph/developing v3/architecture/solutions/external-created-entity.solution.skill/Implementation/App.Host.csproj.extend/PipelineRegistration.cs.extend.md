---
description: Insert GuidResolvingBehavior between ValidationBehavior and ConcurrencyBehavior
project_name: App.Host
name: PipelineRegistration.cs
element_kind: class
change_kind: extend
---

# Goals
- Insert `GuidResolvingBehavior` as the second behavior — after `ValidationBehavior`, before `ConcurrencyBehavior`

# Core Principles
- `ValidationBehavior` runs first — rejects invalid input before anything else
- `GuidResolvingBehavior` runs second — rejects duplicate Guid on create commands
- `ConcurrencyBehavior` runs third — rejects stale versions on update commands
- `UnitOfWorkBehavior` runs fourth — commits after handler completes

# Final pipeline order after all solutions applied

```
1. ValidationBehavior      ← validation-behavior.solution.skill — rejects invalid input
2. GuidResolvingBehavior   ← this solution — rejects duplicate Guid (create only)
3. ConcurrencyBehavior     ← entity-concurrency-change.solution.skill — rejects stale versions (update only)
4. UnitOfWorkBehavior      ← unit-of-work.solution.skill — commits after handler
```

# Implementation changes

Extend `PipelineRegistration` from [[pipeline-registration.solution.skill]] with `GuidResolvingBehavior`:

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

        // 2. guid resolving — rejects duplicate Guid on create commands
        //    only activates for commands implementing IHasGuid
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(GuidResolvingBehavior<,>));

        // 3. concurrency — rejects stale versions on update commands
        //    only activates for commands implementing IHasVersions
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ConcurrencyBehavior<,>));

        // 4. unit of work — commits staged changes after handler completes
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(UnitOfWorkBehavior<,>));

        return services;
    }
}
```

# Rules

MUST:
- `GuidResolvingBehavior` registered after `ValidationBehavior` — invalid commands rejected before DB lookup
- `GuidResolvingBehavior` registered before `ConcurrencyBehavior` — duplicate creation caught before version check
- `GuidResolvingBehavior` registered before `UnitOfWorkBehavior` — duplicate commands never open a unit of work

MUST NOT:
- `GuidResolvingBehavior` registered after `UnitOfWorkBehavior` — duplicate commands would open a unit of work

# Anti-patterns
- `GuidResolvingBehavior` registered after `UnitOfWorkBehavior` — duplicate commands open a unit of work unnecessarily

# Check list
- [ ] `ValidationBehavior` registered first
- [ ] `GuidResolvingBehavior` registered second
- [ ] `ConcurrencyBehavior` registered third
- [ ] `UnitOfWorkBehavior` registered fourth

# Unittest TestCases
- [ ] WHEN applied THEN Insert GuidResolvingBehavior as the second behavior — after ValidationBehavior, before ConcurrencyBehavior
- [ ] WHEN applied THEN ValidationBehavior runs first — rejects invalid input before anything else
- [ ] WHEN applied THEN GuidResolvingBehavior runs second — rejects duplicate Guid on create commands
- [ ] WHEN applied THEN ConcurrencyBehavior runs third — rejects stale versions on update commands
- [ ] WHEN applied THEN UnitOfWorkBehavior runs fourth — commits after handler completes
- [ ] WHEN verified THEN ValidationBehavior registered first
- [ ] WHEN verified THEN GuidResolvingBehavior registered second
- [ ] WHEN verified THEN ConcurrencyBehavior registered third
- [ ] WHEN verified THEN UnitOfWorkBehavior registered fourth
