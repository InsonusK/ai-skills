---
description: Register IEntityVersionResolverFactory and module IEntityVersionResolver implementations
project_name: App.Host
name: EntityVersionResolverRegistration.cs
element_kind: class
change_kind: create
tags:
  - solution/entity-concurrency-change
  - element/entityversionresolverregistration-cs
---

# Goals
- Register `IEntityVersionResolverFactory` without changing the existing `RepositoryRegistration.AddRepositories` signature
- Feed the factory module Domain assemblies (for validation) and module Application assemblies (for resolver discovery)
- Register every module `IEntityVersionResolver` implementation so the factory can resolve them from DI

# Core Principles
- `IEntityVersionResolverFactory` registered as `Scoped` — it creates `Scoped` resolvers that depend on `IReadRepository<T>`
- Module assemblies are supplied explicitly from the composition root — App.Host is the only project that references all modules
- Keep repository registration separate from concurrency resolver registration

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Entity version resolver registration | `EntityVersionResolverRegistration` | `EntityVersionResolverRegistration` | `EntityVersionResolverRegistration.cs` | `EntityVersionResolverRegistration.cs` |

# Implementation changes

```csharp
// App.Host/DependencyInjection/EntityVersionResolverRegistration.cs
using System.Reflection;
using App.Infrastructure.Concurrency;
using Microsoft.Extensions.DependencyInjection;
using Shared.Concurrency;

namespace App.Host.DependencyInjection;

public static class EntityVersionResolverRegistration
{
    public static IServiceCollection AddEntityVersionResolver(
        this IServiceCollection services,
        IEnumerable<Assembly> domainAssemblies,
        IEnumerable<Assembly> applicationAssemblies)
    {
        services.AddScoped<IEntityVersionResolverFactory>(
            sp => new EntityVersionResolverFactory(sp, domainAssemblies, applicationAssemblies));

        foreach (var assembly in applicationAssemblies)
        {
            var resolverTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract
                    && typeof(IEntityVersionResolver).IsAssignableFrom(t));

            foreach (var resolverType in resolverTypes)
            {
                services.AddScoped(resolverType);
            }
        }

        return services;
    }
}
```

> **Note:** Call this from `Program.cs` alongside `AddRepositories()`. Pass the module Domain assemblies that contain the `{Entity}Config` classes and the module Application assemblies that contain the `{Entity}VersionResolver` classes:
> ```csharp
> builder.Services
>     .AddRepositories()
>     .AddEntityVersionResolver(
>         new[]
>         {
>             typeof(Task.Domain.Entities.TodoTask).Assembly,
>             typeof(Users.Domain.Entities.User).Assembly,
>         },
>         new[]
>         {
>             typeof(Task.Application.Concurrency.TodoTaskVersionResolver).Assembly,
>             typeof(Users.Application.Concurrency.UserVersionResolver).Assembly,
>         });
> ```
# Rule changes

## MUST
- `IEntityVersionResolverFactory` registered as `Scoped`
- `EntityVersionResolverFactory` receives Domain assemblies and Application assemblies
- Register every concrete `IEntityVersionResolver` implementation from Application assemblies as `Scoped`
- Live under `/App.Host/DependencyInjection`
- Never `IEntityVersionResolverFactory` registered as `Singleton` — would create captive dependencies on `Scoped` repositories
- Never modify the signature of `RepositoryRegistration.AddRepositories`

## SHOULD
- Avoid `IEntityVersionResolverFactory` registered as `Singleton` — resolver instances depend on `Scoped` repositories
- Avoid passing Infrastructure or Api assemblies instead of Application assemblies — would scan unrelated types

# Check list
- [ ] `EntityVersionResolverRegistration` defined in `App.Host/DependencyInjection/EntityVersionResolverRegistration.cs`
- [ ] `EntityVersionResolverFactory` receives Domain and Application assemblies
- [ ] All module `IEntityVersionResolver` implementations registered as `Scoped`

# Unittest TestCases
- [ ] WHEN applied THEN Register IEntityVersionResolverFactory without changing RepositoryRegistration.AddRepositories signature
- [ ] WHEN applied THEN Feed the factory module Domain assemblies and module Application assemblies
- [ ] WHEN applied THEN IEntityVersionResolverFactory registered as Scoped
- [ ] WHEN applied THEN Register every module IEntityVersionResolver implementation as Scoped
- [ ] WHEN verified THEN EntityVersionResolverRegistration defined in App.Host/DependencyInjection/EntityVersionResolverRegistration.cs
- [ ] WHEN verified THEN IEntityVersionResolverFactory registered as Scoped
- [ ] WHEN verified THEN EntityVersionResolverFactory receives Domain and Application assemblies
- [ ] WHEN naming 'Entity version resolver registration' THEN pattern matches convention
