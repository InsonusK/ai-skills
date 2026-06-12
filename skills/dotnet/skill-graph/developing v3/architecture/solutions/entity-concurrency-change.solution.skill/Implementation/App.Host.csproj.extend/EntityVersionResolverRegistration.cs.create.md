---
description: Register IEntityVersionResolver as Singleton with module Domain assemblies
project_name: App.Host
name: EntityVersionResolverRegistration.cs
element_kind: class
change_kind: create
---

# Goals
- Register `EntityVersionResolver` as `Singleton` without changing the existing `RepositoryRegistration.AddRepositories` signature
- Feed the resolver the module Domain assemblies so it can discover `IVersioned` entities automatically

# Core Principles
- `EntityVersionResolver` registered as `Singleton` — map is built once at startup, safe for singleton lifetime
- Module Domain assemblies are supplied explicitly from the composition root — App.Host is the only project that references all modules
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
        IEnumerable<Assembly> versionedEntityAssemblies)
    {
        services.AddSingleton<IEntityVersionResolver>(
            _ => new EntityVersionResolver(versionedEntityAssemblies));

        return services;
    }
}
```

> **Note:** Call this from `Program.cs` alongside `AddRepositories()`:
> ```csharp
> builder.Services
>     .AddRepositories()
>     .AddEntityVersionResolver(new[]
>     {
>         typeof(Task.Domain.Entities.TodoTask).Assembly,
>         typeof(Users.Domain.Entities.User).Assembly,
>     });
> ```

# Rules

MUST:
- `EntityVersionResolver` registered as `Singleton`
- `EntityVersionResolver` receives `IEnumerable<Assembly>` containing all module Domain assemblies with versioned entities
- Live under `/App.Host/DependencyInjection`

MUST NOT:
- `EntityVersionResolver` registered as `Scoped` or `Transient`
- Modify the signature of `RepositoryRegistration.AddRepositories`

# Anti-patterns
- `EntityVersionResolver` registered as `Scoped` or `Transient` — unnecessary overhead for a read-only map
- Passing Application or Infrastructure assemblies instead of Domain assemblies — would scan non-entity types

# Check list
- [ ] `EntityVersionResolverRegistration` defined in `App.Host/DependencyInjection/EntityVersionResolverRegistration.cs`
- [ ] `EntityVersionResolver` registered as `Singleton`
- [ ] `EntityVersionResolver` receives module Domain assemblies

# Unittest TestCases
- [ ] WHEN applied THEN Register EntityVersionResolver as Singleton without changing RepositoryRegistration.AddRepositories signature
- [ ] WHEN applied THEN Feed the resolver the module Domain assemblies so it can discover IVersioned entities automatically
- [ ] WHEN applied THEN EntityVersionResolver registered as Singleton — map is built once at startup, safe for singleton lifetime
- [ ] WHEN verified THEN EntityVersionResolverRegistration defined in App.Host/DependencyInjection/EntityVersionResolverRegistration.cs
- [ ] WHEN verified THEN EntityVersionResolver registered as Singleton
- [ ] WHEN verified THEN EntityVersionResolver receives module Domain assemblies
- [ ] WHEN naming 'Entity version resolver registration' THEN pattern matches convention
