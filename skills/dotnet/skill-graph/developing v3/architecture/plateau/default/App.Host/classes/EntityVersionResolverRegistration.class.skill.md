---
uid: 351cf444-d5bb-41a8-b7e1-de34ceb25149
name: entityversionresolverregistration-class
description: Register IEntityVersionResolver as Singleton with module Domain assemblies
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change.solution.skill]]"
---

# Goal
- Register `EntityVersionResolver` as `Singleton` without changing the existing `RepositoryRegistration.AddRepositories` signature
- Feed the resolver the module Domain assemblies so it can discover `IVersioned` entities automatically

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]

# Core Principals
- `EntityVersionResolver` registered as `Singleton` — map is built once at startup, safe for singleton lifetime
- Module Domain assemblies are supplied explicitly from the composition root — App.Host is the only project that references all modules
- Keep repository registration separate from concurrency resolver registration

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity version resolver registration | `EntityVersionResolverRegistration` | `EntityVersionResolverRegistration` | `EntityVersionResolverRegistration.cs` | `EntityVersionResolverRegistration.cs` |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]

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
        IEnumerable<Assembly> versionedEntityAssemblies)
    {
        services.AddSingleton<IEntityVersionResolver>(
            _ => new EntityVersionResolver(versionedEntityAssemblies));

        return services;
    }
}
```

> **Note:** Call this from `Program.cs` alongside `AddRepositories()`. Pass the module Domain assemblies that contain the `{Entity}Config` classes:
> ```csharp
> builder.Services
>     .AddRepositories()
>     .AddEntityVersionResolver(new[]
>     {
>         typeof(Task.Domain.Entities.TodoTask).Assembly,
>         typeof(Users.Domain.Entities.User).Assembly,
>     });
> ```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]

# Rules
MUST:
	- `EntityVersionResolver` registered as `Singleton`
	- `EntityVersionResolver` receives `IEnumerable<Assembly>` containing all module Domain assemblies with versioned entities
	- Live under `/App.Host/DependencyInjection`
MUST NOT:
	- `EntityVersionResolver` registered as `Scoped` or `Transient`
	- Modify the signature of `RepositoryRegistration.AddRepositories`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]

# Anti-patterns
- `EntityVersionResolver` registered as `Scoped` or `Transient` — unnecessary overhead for a read-only map
- Passing Application or Infrastructure assemblies instead of Domain assemblies — would scan non-entity types

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]

# Check list
- [ ] `EntityVersionResolverRegistration` defined in `App.Host/DependencyInjection/EntityVersionResolverRegistration.cs`
- [ ] `EntityVersionResolver` registered as `Singleton`
- [ ] `EntityVersionResolver` receives module Domain assemblies

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Register EntityVersionResolver as Singleton without changing RepositoryRegistration.AddRepositories signature
- [ ] WHEN applied THEN Feed the resolver the module Domain assemblies so it can discover IVersioned entities automatically
- [ ] WHEN applied THEN EntityVersionResolver registered as Singleton — map is built once at startup, safe for singleton lifetime
- [ ] WHEN verified THEN EntityVersionResolverRegistration defined in App.Host/DependencyInjection/EntityVersionResolverRegistration.cs
- [ ] WHEN verified THEN EntityVersionResolver registered as Singleton
- [ ] WHEN verified THEN EntityVersionResolver receives module Domain assemblies
- [ ] WHEN naming 'Entity version resolver registration' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]
