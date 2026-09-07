---
description: Ensure App.Host calls AddRepositories during composition
name: App.Host.csproj
element_kind: project
change_kind: extend
tags:
  - solution/repository-integration
  - element/app-host-csproj
---

# Goals
- Wire repository registrations into the application composition root, through the centralized `AddInfrastructure()` extension point

# Core Principles
- App.Host is the single composition root — all DI registration happens here or via extension methods called from here
- Repository registration is a concern of `InfrastructureRegistration.AddInfrastructure()` — this solution extends that method, it never adds a second, independent call site in `Program.cs`

# Implementation changes

**AS IS** (from `solution-infrastructure-project`):
```csharp
// App.Host/DependencyInjection/InfrastructureRegistration.cs
public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Concern-specific solutions (e.g. solution-repository-integration) extend this method.
        return services;
    }
}
```

**TO BE** (after this solution):
```csharp
// App.Host/DependencyInjection/InfrastructureRegistration.cs
public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddRepositories();
        return services;
    }
}
```
`AddRepositories()` itself is defined in `RepositoryRegistration.cs` (see the nested class file). `Program.cs` is unchanged — it already calls `AddInfrastructure(builder.Configuration)` once, per `solution-infrastructure-project`.

# Rules

## MUST
- `AddInfrastructure()` calls `AddRepositories()` — no separate call added to `Program.cs`
- Open generic DI registration in App.Host for both `IRepository<>` and `IReadRepository<>` pointing to `Repository<>`

# Check list
- [ ] `InfrastructureRegistration.AddInfrastructure()` calls `AddRepositories()`
- [ ] `Program.cs` itself is unchanged by this solution
