---
description: Add IUnitOfWork and UnitOfWorkContext scoped registrations
project_name: App.Host
name: RepositoryRegistration.cs
element_kind: class
change_kind: extend
---

# Goals
- Add `IUnitOfWork` and `UnitOfWorkContext` Scoped registrations alongside the repository registrations

# Core Principles
- Open generic registration eliminates per-entity DI configuration
- `Scoped` lifetime ensures repositories, `IUnitOfWork`, and `UnitOfWorkContext` share the same DbContext instance within a request

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    RepositoryRegistration.cs
```

# Implementation changes

Extend `RepositoryRegistration` from [[skills/dotnet/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill|repository-integration.solution.skill]] with `IUnitOfWork` and `UnitOfWorkContext`:

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

# Rules

MUST:
- `IUnitOfWork` and `UnitOfWorkContext` registered as `Scoped` — same lifetime as DbContext and Repository

MUST NOT:
- Register as `Singleton` or `Transient`

# Anti-patterns
- `services.AddTransient<IUnitOfWork, UnitOfWork>()` — breaks DbContext scope sharing
- `services.AddSingleton<UnitOfWorkContext>()` — depth counter leaks across requests

# Check list
- [ ] `IUnitOfWork` registered as `Scoped`
- [ ] `UnitOfWorkContext` registered as `Scoped`

# Unittest TestCases
- [ ] WHEN applied THEN Add IUnitOfWork and UnitOfWorkContext Scoped registrations alongside the repository registrations
- [ ] WHEN applied THEN Open generic registration eliminates per-entity DI configuration
- [ ] WHEN applied THEN Scoped lifetime ensures repositories, IUnitOfWork, and UnitOfWorkContext share the same DbContext instance within a request
- [ ] WHEN verified THEN IUnitOfWork registered as Scoped
- [ ] WHEN verified THEN UnitOfWorkContext registered as Scoped
