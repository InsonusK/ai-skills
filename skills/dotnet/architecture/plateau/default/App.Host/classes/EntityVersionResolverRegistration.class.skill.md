---
uid: 351cf444-d5bb-41a8-b7e1-de34ceb25149
name: entityversionresolverregistration-class
description: Register IEntityVersionResolverFactory and module IEntityVersionResolver implementations in App.Host
domain: skill
type: template
version: 20260622
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change.solution.skill]]"
---

# Goal
- Register `IEntityVersionResolverFactory` without changing the existing `RepositoryRegistration.AddRepositories` signature
- Feed the factory module Domain assemblies (for validation) and module Application assemblies (for resolver discovery)
- Register every module `IEntityVersionResolver` implementation so the factory can resolve them from DI

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]

# Core Principals
- `IEntityVersionResolverFactory` registered as `Scoped` — it creates `Scoped` resolvers that depend on `IReadRepository<T>`
- Module assemblies are supplied explicitly from the composition root — App.Host is the only project that references all modules
- Keep repository registration separate from concurrency resolver registration

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity version resolver registration | `EntityVersionResolverRegistration` | `EntityVersionResolverRegistration` | `EntityVersionResolverRegistration.cs` | `EntityVersionResolverRegistration.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]

# Implementation
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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]

# Rules
MUST:
- `IEntityVersionResolverFactory` registered as `Scoped`
- `EntityVersionResolverFactory` receives Domain assemblies and Application assemblies
- Register every concrete `IEntityVersionResolver` implementation from Application assemblies as `Scoped`
- Live under `/App.Host/DependencyInjection`

MUST NOT:
- `IEntityVersionResolverFactory` registered as `Singleton` — would create captive dependencies on `Scoped` repositories
- Modify the signature of `RepositoryRegistration.AddRepositories`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]

# Anti-patterns
- `IEntityVersionResolverFactory` registered as `Singleton` — resolver instances depend on `Scoped` repositories
- Passing Infrastructure or Api assemblies instead of Application assemblies — would scan unrelated types

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]

# Check list
- [ ] `EntityVersionResolverRegistration` defined in `App.Host/DependencyInjection/EntityVersionResolverRegistration.cs`
- [ ] `IEntityVersionResolverFactory` registered as `Scoped`
- [ ] `EntityVersionResolverFactory` receives Domain and Application assemblies
- [ ] All module `IEntityVersionResolver` implementations registered as `Scoped`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Register `IEntityVersionResolverFactory` without changing `RepositoryRegistration.AddRepositories` signature
- [ ] WHEN applied THEN Feed the factory module Domain assemblies and module Application assemblies
- [ ] WHEN applied THEN `IEntityVersionResolverFactory` registered as `Scoped`
- [ ] WHEN applied THEN Register every module `IEntityVersionResolver` implementation as `Scoped`
- [ ] WHEN verified THEN `EntityVersionResolverRegistration` defined in `App.Host/DependencyInjection/EntityVersionResolverRegistration.cs`
- [ ] WHEN verified THEN `IEntityVersionResolverFactory` registered as `Scoped`
- [ ] WHEN verified THEN `EntityVersionResolverFactory` receives Domain and Application assemblies
- [ ] WHEN naming 'Entity version resolver registration' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]
