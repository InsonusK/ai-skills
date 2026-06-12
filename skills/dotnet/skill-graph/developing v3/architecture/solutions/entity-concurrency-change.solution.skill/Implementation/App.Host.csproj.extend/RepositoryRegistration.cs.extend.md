---
description: Register IEntityVersionResolver as Singleton
project_name: App.Host
name: RepositoryRegistration.cs
element_kind: class
change_kind: extend
---

# Goals
- Register `EntityVersionResolver` as `Singleton` alongside repository registrations

# Core Principles
- `EntityVersionResolver` registered as `Singleton` — static map, safe for singleton lifetime

# Implementation changes

```csharp
// App.Host/DependencyInjection/RepositoryRegistration.cs
public static class RepositoryRegistration
{
    public static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<UnitOfWorkContext>();
        services.AddSingleton<IEntityVersionResolver, EntityVersionResolver>();

        return services;
    }
}
```

# Rules

MUST:
- `EntityVersionResolver` registered as `Singleton` — static map, safe for singleton lifetime

# Anti-patterns
- `EntityVersionResolver` registered as `Scoped` or `Transient` — unnecessary overhead for a static map

# Check list
- [ ] `EntityVersionResolver` registered as `Singleton`

# Unittest TestCases
- [ ] WHEN applied THEN Register EntityVersionResolver as Singleton alongside repository registrations
- [ ] WHEN applied THEN EntityVersionResolver registered as Singleton — static map, safe for singleton lifetime
- [ ] WHEN verified THEN EntityVersionResolver registered as Singleton
