---
name: class-repository-registration
description: Add IUnitOfWork and UnitOfWorkContext scoped registrations
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]"
---

# Goal
- Add `IUnitOfWork` and `UnitOfWorkContext` Scoped registrations alongside the repository registrations
- Centralise repository DI registration in one extension method called from the composition root
- Register open generics for both `IRepository<>` and `IReadRepository<>` pointing to `Repository<>`

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.extend|RepositoryRegistration.cs]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.create|RepositoryRegistration.cs]]

# Core Principles
- Apply ONE plateau template per class
- Open generic registration eliminates per-entity DI configuration
- `Scoped` lifetime ensures repositories, `IUnitOfWork`, and `UnitOfWorkContext` share the same DbContext instance within a request
- `Scoped` lifetime ensures repositories share the same DbContext instance within a request

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.extend|RepositoryRegistration.cs]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.create|RepositoryRegistration.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-repository-registration
//Plateau: default
//Version: 20260628
```

Extend `RepositoryRegistration` from [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration.skill]] with `IUnitOfWork` and `UnitOfWorkContext`:

```csharp
// App.Host/DependencyInjection/RepositoryRegistration.cs
using App.Infrastructure.Repositories;
using App.Infrastructure.UnitOfWork;
using BuildingBlocks.MediatR;
using Shared.Repositories;
using Shared.UnitOfWork;

namespace App.Host.DependencyInjection;

public static class RepositoryRegistration
{
    public static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<UnitOfWorkContext>();

        return services;
    }
}
```

```csharp
// App.Host/DependencyInjection/RepositoryRegistration.cs
using App.Infrastructure.Repositories;
using Shared.Repositories;

namespace App.Host.DependencyInjection;

public static class RepositoryRegistration
{
    public static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(Repository<>));

        return services;
    }
}
```

Called from the composition root:

```csharp
// App.Host/Program.cs
builder.Services.AddRepositories();
```

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.extend|RepositoryRegistration.cs]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.create|RepositoryRegistration.cs]]

# Rules
MUST:
	- `IUnitOfWork` and `UnitOfWorkContext` registered as `Scoped` — same lifetime as DbContext and Repository
	- Register `IRepository<>` as open generic pointing to `Repository<>`
	- Register `IReadRepository<>` as open generic pointing to `Repository<>`
	- Use `Scoped` lifetime
MUST NOT:
	- Register as `Singleton` or `Transient`
	- Register per-entity closed generics
	- Register as `Singleton`

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.extend|RepositoryRegistration.cs]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.create|RepositoryRegistration.cs]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- `services.AddTransient<IUnitOfWork, UnitOfWork>()` — breaks DbContext scope sharing
- `services.AddSingleton<UnitOfWorkContext>()` — depth counter leaks across requests
- `services.AddScoped<IRepository<TodoTask>, Repository<TodoTask>>()` — use open generic instead
- `services.AddSingleton(...)` — DbContext is Scoped, repository must match

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.extend|RepositoryRegistration.cs]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.create|RepositoryRegistration.cs]]

# Check list
- [ ] `IUnitOfWork` registered as `Scoped`
- [ ] `UnitOfWorkContext` registered as `Scoped`
- [ ] `AddRepositories` extension method exists
- [ ] Both `IRepository<>` and `IReadRepository<>` registered as open generics
- [ ] `Scoped` lifetime used
- [ ] Called from `Program.cs`

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.extend|RepositoryRegistration.cs]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.create|RepositoryRegistration.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Add IUnitOfWork and UnitOfWorkContext Scoped registrations alongside the repository registrations
- [ ] WHEN applied THEN Open generic registration eliminates per-entity DI configuration
- [ ] WHEN applied THEN Scoped lifetime ensures repositories, IUnitOfWork, and UnitOfWorkContext share the same DbContext instance within a request
- [ ] WHEN verified THEN IUnitOfWork registered as Scoped
- [ ] WHEN verified THEN UnitOfWorkContext registered as Scoped
- [ ] WHEN applied THEN Centralise repository DI registration in one extension method called from the composition root
- [ ] WHEN applied THEN Register open generics for both IRepository<> and IReadRepository<> pointing to Repository<>
- [ ] WHEN applied THEN Scoped lifetime ensures repositories share the same DbContext instance within a request
- [ ] WHEN verified THEN AddRepositories extension method exists
- [ ] WHEN verified THEN Both IRepository<> and IReadRepository<> registered as open generics
- [ ] WHEN verified THEN Scoped lifetime used
- [ ] WHEN verified THEN Called from Program.cs

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.extend|RepositoryRegistration.cs]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.create|RepositoryRegistration.cs]]
