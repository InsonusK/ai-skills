---
name: class-infrastructure-registration
description: Classes InfrastructureRegistration/RepositoryRegistration/EntityVersionResolverRegistration in the statefull-service plateau
whenToUse: when wiring AppDbContext, repositories, unit of work, or entity version resolvers into the composition root
domain: skill
type: template
plateau: statefull-service
version: 20260824100000
tags:
  - skill/template/class
  - plateau/statefull-service
created_by:
  - "[[../../../../../solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]"
  - "[[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]"
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
---

# Goal
- Give the composition root exactly one entry point (`AddInfrastructure()`) for everything App.Infrastructure needs registered — `AppDbContext`, repositories, unit of work, and the entity version resolver factory

# Core Principles
- `AddInfrastructure()` is the only call site — `Program.cs` never calls `AddRepositories()`/version-resolver registration directly, it all happens inside `AddInfrastructure()`
- `Repository<T>`/`IReadRepository<T>`, `IUnitOfWork`, `UnitOfWorkContext`, and `EntityVersionResolverFactory` all registered `Scoped`

# Implementation
```csharp
//Skill: class-infrastructure-registration
//Plateau: statefull-service
//Version: 20260824100000

// App.Host/DependencyInjection/InfrastructureRegistration.cs
public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddRepositories();
        services.AddEntityVersionResolvers();

        return services;
    }
}

// App.Host/DependencyInjection/RepositoryRegistration.cs
public static class RepositoryRegistration
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<UnitOfWorkContext>();

        return services;
    }
}

// App.Host/DependencyInjection/EntityVersionResolverRegistration.cs
public static class EntityVersionResolverRegistration
{
    public static IServiceCollection AddEntityVersionResolvers(this IServiceCollection services)
    {
        // Scans module Application assemblies for IEntityVersionResolver implementations.
        services.Scan(scan => scan.FromAssemblies(/* module Application assemblies */)
            .AddClasses(c => c.AssignableTo<IEntityVersionResolver>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddScoped<IEntityVersionResolverFactory, EntityVersionResolverFactory>();

        return services;
    }
}
```

# Rules
MUST:
- `AddInfrastructure()` be the only place `AppDbContext`, repositories, unit of work, and the resolver factory get registered
- All of the above registered `Scoped`
MUST NOT:
- `Program.cs` call `AddRepositories()`/`AddEntityVersionResolvers()` directly — only `AddInfrastructure()` does
- Register `UnitOfWorkContext`/`IUnitOfWork` as `Singleton` or `Transient`

# Check list
- [ ] `AddInfrastructure()` registers `AppDbContext`, repositories, unit of work, and `EntityVersionResolverFactory`
- [ ] Everything registered `Scoped`
- [ ] `Program.cs` calls only `AddInfrastructure(builder.Configuration)`, nothing more specific

__Applied solutions:__
- [[../../../../../solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]] - [[../../../../../solutions/solution-infrastructure-project.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] - [[../../../../../solutions/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]
