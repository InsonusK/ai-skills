---
name: plateau-domain-service--class-infrastructure-registration
description: Class InfrastructureRegistration in the plateau-domain-service plateau — the one AddInfrastructure() extension that wires the DbContext, repositories, unit of work, version-resolver factory, and gRPC clients
whenToUse: when wiring a new outbound infrastructure integration into the composition root, or checking how persistence / concurrency / gRPC clients are registered
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]"
  - "[[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]"
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../../../solutions/solution-grpc-client.skill/solution-grpc-client.skill.md|solution-grpc-client]]"
---

# Goal
- Provide `AddInfrastructure(this IServiceCollection, IConfiguration)` — the single, centralized extension point every outbound-integration solution extends, parallel to `AddModules()` / `AddPipeline()`. `Program.cs` calls it once.

__Applied solutions:__
- [[../../../../../solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]] - [[../../../../../solutions/solution-infrastructure-project.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

# Core Principles
- Apply ONE plateau template per class.
- `static class InfrastructureRegistration` in `App.Host/DependencyInjection`. `solution-infrastructure-project` creates it empty; each concern extends it:
  - persistence: `AddDbContext<AppDbContext>(...)` (provider chosen here), `AddScoped<IUnitOfWork, UnitOfWork>()`, `AddScoped<UnitOfWorkContext>()`, `services.AddRepositories()` (open generics, `Scoped`).
  - concurrency: `AddScoped<IEntityVersionResolverFactory>(sp => new EntityVersionResolverFactory(sp, domainAssemblies, applicationAssemblies))` with explicit assembly lists.
  - gRPC clients: `AddGrpcClient<{Dependency}.{Dependency}Client>(...)` + `.AddStandardResilienceHandler()`, `AddScoped<I{Dependency}Client, {Dependency}GrpcClient>()`.
- Called exactly once from `Program.cs`, alongside `AddModules()` / `AddPipeline()` — never from a module, never twice.

# Implementation
```csharp
// Skill: plateau-domain-service--class-infrastructure-registration
// Plateau: domain-service
// Version: 20260902000000
public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<UnitOfWorkContext>();
        services.AddRepositories();

        var domain = new[] { typeof({Module}.Domain.Configurations.{Entity}Config).Assembly };
        var application = new[] { typeof({Module}.Application.SampleModuleRegistration).Assembly };
        services.AddScoped<IEntityVersionResolverFactory>(sp => new EntityVersionResolverFactory(sp, domain, application));

        // gRPC clients: AddGrpcClient<...>().AddStandardResilienceHandler(); AddScoped<I{Dep}Client, {Dep}GrpcClient>();
        return services;
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.create.md|RepositoryRegistration.cs.create]]
- [[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] - [[../../../../../solutions/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.extend.md|RepositoryRegistration.cs.extend]]
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs.create]]
- [[../../../../../solutions/solution-grpc-client.skill/solution-grpc-client.skill.md|solution-grpc-client]] - [[../../../../../solutions/solution-grpc-client.skill/Implementation/App.Host.csproj.extend/GrpcClientRegistration.cs.create.md|GrpcClientRegistration.cs.create]]

# Rules
MUST:
- Expose one `AddInfrastructure(IServiceCollection, IConfiguration)`; call it exactly once from `Program.cs`.
- Register `AppDbContext` via `AddDbContext` (provider chosen here); `IUnitOfWork` / `UnitOfWorkContext` / repositories `Scoped`; the version-resolver factory `Scoped` with explicit assembly lists.
- Register gRPC clients via `AddGrpcClient<T>()` + resilience; never hand-build a channel.
- Never call `AddInfrastructure()` from a module or more than once.
- Never apply several plateau templates per class.

# Check list
- [ ] `AddInfrastructure(IServiceCollection, IConfiguration)` under `App.Host/DependencyInjection`, called once from `Program.cs`.
- [ ] `AppDbContext` + `IUnitOfWork` + `UnitOfWorkContext` + repositories + version-resolver factory registered `Scoped`.
- [ ] Version-resolver factory constructed with explicit Domain/Application assembly lists.

# Unittest TestCases
- [ ] WHEN `AddInfrastructure()` runs THEN `IRepository<>`, `IReadRepository<>`, `IUnitOfWork`, `IEntityVersionResolverFactory` all resolve.
