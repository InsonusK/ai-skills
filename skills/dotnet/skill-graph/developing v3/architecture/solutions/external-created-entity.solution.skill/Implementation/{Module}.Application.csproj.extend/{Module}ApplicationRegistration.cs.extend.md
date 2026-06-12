---
description: Register IGuidResolver in module DI
project_name: "{Module}.Application"
name: "{Module}ApplicationRegistration.cs"
element_kind: class
change_kind: extend
---

# Goals
- Register each `GuidResolver` in the module's DI registration

# Implementation changes
Module registration extended with `IGuidResolver` registrations:

```csharp
// {Module}.Application/{Module}ApplicationRegistration.cs
public static class {Module}ApplicationRegistration
{
    public static IServiceCollection Register{Module}Module(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof({Module}ApplicationRegistration).Assembly));

        services.AddValidatorsFromAssembly(
            typeof({Module}ApplicationRegistration).Assembly);

        // one registration per external-created entity type in this module
        services.AddScoped<
            IGuidResolver<Result<Create{Entity}Result>>,
            Create{Entity}GuidResolver>();

        return services;
    }
}
```

# Rules

MUST:
- Each `IGuidResolver<TResult>` registered explicitly as `Scoped` — not auto-scanned
- One registration per external-created entity type

MUST NOT:
- `IGuidResolver` registrations omitted — `GuidResolvingBehavior` will throw at runtime if resolver not found

# Anti-patterns
- `IGuidResolver` registered as open generic — breaks DI resolution per command result type

# Check list
- [ ] `IGuidResolver<Result<Create{Entity}Result>>` registered as `Scoped`
- [ ] One registration per external-created entity type

# Unittest TestCases
- [ ] WHEN applied THEN Register each GuidResolver in the module's DI registration
- [ ] WHEN verified THEN IGuidResolver<Result<Create{Entity}Result>> registered as Scoped
- [ ] WHEN verified THEN One registration per external-created entity type
