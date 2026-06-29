---
description: DI registration extension for repository open generics
project_name: App.Host
name: RepositoryRegistration.cs
element_kind: class
change_kind: create
---

# Goals
- Centralise repository DI registration in one extension method called from the composition root
- Register open generics for both `IRepository<>` and `IReadRepository<>` pointing to `Repository<>`

# Core Principles
- Open generic registration eliminates per-entity DI configuration
- `Scoped` lifetime ensures repositories share the same DbContext instance within a request

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    RepositoryRegistration.cs
```

# Implementation changes

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

# Rules

MUST:
- Register `IRepository<>` as open generic pointing to `Repository<>`
- Register `IReadRepository<>` as open generic pointing to `Repository<>`
- Use `Scoped` lifetime

MUST NOT:
- Register per-entity closed generics
- Register as `Singleton`

# Anti-patterns
- `services.AddScoped<IRepository<TodoTask>, Repository<TodoTask>>()` — use open generic instead
- `services.AddSingleton(...)` — DbContext is Scoped, repository must match

# Check list
- [ ] `AddRepositories` extension method exists
- [ ] Both `IRepository<>` and `IReadRepository<>` registered as open generics
- [ ] `Scoped` lifetime used
- [ ] Called from `Program.cs`

# Unittest TestCases
- [ ] WHEN applied THEN Centralise repository DI registration in one extension method called from the composition root
- [ ] WHEN applied THEN Register open generics for both IRepository<> and IReadRepository<> pointing to Repository<>
- [ ] WHEN applied THEN Open generic registration eliminates per-entity DI configuration
- [ ] WHEN applied THEN Scoped lifetime ensures repositories share the same DbContext instance within a request
- [ ] WHEN verified THEN AddRepositories extension method exists
- [ ] WHEN verified THEN Both IRepository<> and IReadRepository<> registered as open generics
- [ ] WHEN verified THEN Scoped lifetime used
- [ ] WHEN verified THEN Called from Program.cs
