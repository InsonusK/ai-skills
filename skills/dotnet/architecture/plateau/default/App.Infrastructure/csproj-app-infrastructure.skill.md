---
name: csproj-app-infrastructure
description: Provide all persistence implementation — DbContext, repository implementations, outbox interceptor, background dispatcher
domain: skill
type: template
version: 20260622
plateau: default
tags:
  - skill/template/csproj
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration.skill]]"
---

# Goal
- Provide the `UnitOfWork` EF Core implementation that wraps `AppDbContext.SaveChangesAsync`
- Provide all persistence implementation — DbContext, repository implementations, outbox interceptor, background dispatcher
- Be the only layer that knows EF Core implementation details
- Provide the single generic `Repository<T>` EF Core implementation
- Leverage `RepositoryBase<T>` from Ardalis to eliminate boilerplate spec evaluation code
- Own `EntityVersionResolverFactory` — the factory that maps stable string entity names to Application-layer `IEntityVersionResolver` implementations
- Discover versioned entities from Domain config classes and resolver implementations from Application assemblies
- Host cross-module foreign key configurations that span multiple bounded contexts
- Register all module entity configurations via `ApplyConfigurationsFromAssembly` in AppDbContext

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

# Core Principals
- `UnitOfWork` delegates directly to `AppDbContext.SaveChangesAsync` — no additional logic
- Registered as `Scoped` — shares the same `DbContext` instance as `Repository<T>`
- App.Infrastructure is the only project with a concrete DbContext
- App.Infrastructure implements interfaces defined in Shared
- No module Application or Domain layer references App.Infrastructure
- `RepositoryBase<T>` from Ardalis handles all `SpecificationEvaluator` logic internally
- App.Infrastructure remains the only layer that knows about EF Core implementation details
- One generic `Repository<T>` class covers all entity types
- Read-only map — populated once (static/lazy) at first use, no runtime modification
- Keys are stable business names declared in `{Entity}Config.VersionedEntityName` and `{Entity}VersionResolver.VersionedEntityName`
- Domain assemblies supply the list of valid versioned entities
- Application assemblies supply the resolver implementations
- Returns `null` for unknown names — `ConcurrencyBehavior` returns `Result.Error` on null
- App.Infrastructure is the only place where cross-module foreign key relationships are configured
- DbContext uses `ApplyConfigurationsFromAssembly` to automatically discover all `IEntityTypeConfiguration<T>` implementations from module Domain assemblies
- App.Infrastructure references all module Domain projects to access entities for cross-module configuration

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

# Structure

## Solution place
```
/src/App/App.Infrastructure
```


## Project Structure
- /App.Infrastructure
  - /Persistence
    - AppDbContext.cs
    - /Configurations
      - [CrossModuleFkConfig.cs](./classes/class-module-to-module-config.skill.md)
      - OutboxMessageConfig.cs
  - /Repositories
    - [Repository.cs](./classes/class-repository.skill.md)
  - /UnitOfWork
    - [UnitOfWork.cs](./classes/class-unit-of-work.skill.md)
  - /Outbox
    - DomainEventInterceptor.cs
    - OutboxDispatcher.cs
  - /Concurrency
    - [EntityVersionResolverFactory.cs](./classes/class-entity-version-resolver-factory.skill.md)
  - App.Infrastructure.csproj

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

## Directory and class skills
| `Directory|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
| /UnitOfWork | Unit of work implementations | [[skills/dotnet/architecture/plateau/default/App.Infrastructure/classes/class-unit-of-work.skill.md|class-UnitOfWork.skill]] |
| UnitOfWork.cs | EF Core SaveChangesAsync implementation | [[skills/dotnet/architecture/plateau/default/App.Infrastructure/classes/class-unit-of-work.skill.md|class-UnitOfWork.skill]] |
| /Persistence | DbContext and EF configurations |  |
| /Repositories | Generic Repository<T> implementation |  |
| /UnitOfWork | UnitOfWork implementation | [[skills/dotnet/architecture/plateau/default/App.Infrastructure/classes/class-unit-of-work.skill.md|class-UnitOfWork.skill]] |
| /Outbox | EF interceptor and background dispatcher |  |
| /Concurrency | EntityVersionResolverFactory mapping entity names to Application-layer resolvers |  |
| /Repositories/Repository.cs | Generic EF Core repository implementation inheriting Ardalis `RepositoryBase<T>` | [[skills/dotnet/architecture/plateau/default/App.Infrastructure/classes/class-repository.skill.md|class-Repository.skill]] |
| /Concurrency/EntityVersionResolverFactory.cs | Maps string entity names to Application-layer `IEntityVersionResolver` implementations | [[skills/dotnet/architecture/plateau/default/App.Infrastructure/classes/class-entity-version-resolver-factory.skill.md|class-EntityVersionResolverFactory.skill]] |
| /Persistence/Configurations | Cross-module foreign key and relationship configurations |  |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides `DbContext.SaveChangesAsync` |
| `Ardalis.Specification.EntityFrameworkCore` | latest stable | Provides `RepositoryBase<T>` and EF Core spec evaluator |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides `IEntityTypeConfiguration<>` used to discover config classes |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Pipeline behavior registration — belongs to App.Host
- Cross-module JOIN queries — belongs to App.Queries
- Intra-module entity configurations — belong in respective `{Module}.Domain/Configurations`
- Domain entities — belong in `{Module}.Domain`
- Value Object definitions — belong in `{Module}.Domain/ValueObjects`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

## Allowed Dependencies
- Shared
- App.Domain (implicit via AppDbContext)
- BuildingBlocks
- {ModuleName}.Domain (all modules)
- {ModuleName}.Interfaces (all modules)
- `{ModuleName}.Domain` (all modules)
- `{ModuleName}.Interfaces` (all modules)
- {Module}.Domain (for config classes)
- {ModuleName}.Domain (all modules) — required to access entities and their configurations

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

# Rules
MUST:
	- `UnitOfWork` registered as `Scoped` — must share the same `DbContext` instance as `Repository<T>`
	- App.Infrastructure is the only project with DbContext
	- `Repository<T>` generic implementation registered here
	- DomainEventInterceptor registered on DbContext here
	- Reference `Ardalis.Specification.EntityFrameworkCore`
	- Single generic `Repository<T>` inheriting `RepositoryBase<T>`
	- Constructor accept `AppDbContext` and pass it to base
	- Implement `IRepository<T>` from Shared
	- `EntityVersionResolverFactory` scans Domain assemblies for `IEntityTypeConfiguration<T>` configs where `T` implements `IVersioned`
		- `EntityVersionResolverFactory` scans Application assemblies for concrete `IEntityVersionResolver` implementations
	- Every mutable entity implements `IVersioned`
	- Every mutable entity config class declares `public const string VersionedEntityName`
		- Every `{Entity}VersionResolver` declares `public const string VersionedEntityName` matching its config
	- Keys are stable business string names — same strings used in `IHasVersions` commands and ETag encoding
	- Constructor accepts `IServiceProvider`, `IEnumerable<Assembly>` domainAssemblies, and `IEnumerable<Assembly>` applicationAssemblies
- Build the resolver-type map only once (static, lazy, thread-safe)
	- Register all configurations via `ApplyConfigurationsFromAssembly` scanning all module Domain assemblies in DbContext
	- Place cross-module foreign key configurations in `/Persistence/Configurations`
MUST NOT:
	- `UnitOfWork` expose any method beyond `SaveChangesAsync`
	- `UnitOfWork` contain transaction management logic — EF manages transactions implicitly
	- App.Infrastructure be referenced by any module Application, Domain, or Api
	- App.Infrastructure be referenced by App.Queries directly for DbContext
	- Call `SaveChangesAsync` inside `Repository<T>`
	- Create per-entity repository subclasses
	- Keys be C# type names, namespaces, or assembly-qualified names as the public contract — breaks when entities are renamed
	- Rely on a hardcoded dictionary of resolver types
	- Define intra-module entity configurations here — those belong in `{Module}.Domain/Configurations`
	- Register configurations manually one by one in `OnModelCreating`
	- Reference BuildingBlocks directly

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

# Anti-patterns
- `UnitOfWork` containing retry logic — belongs in a decorator or policy, not the UoW
- `UnitOfWork` exposing `BeginTransaction` — violates single-responsibility contract
- Module Application referencing App.Infrastructure — use repository abstractions from Shared
- Putting cross-module JOIN queries in App.Infrastructure — belongs in App.Queries
- `TaskRepository : Repository<TodoTask>` — unnecessary subclass, generic handles all types
- Manual `SpecificationEvaluator.Default.GetQuery(...)` calls — Ardalis base handles this
- `EntityVersionResolverFactory` key using `nameof(TodoTask)` — fragile, breaks on class rename
- Hardcoded dictionary of resolver types — duplicates the entity list and is easy to forget when adding new entities
- Scanning `AppDomain.CurrentDomain.GetAssemblies()` without an explicit allow-list — includes unrelated assemblies
- Putting `VersionedEntityName` on the entity class instead of the config — spreads configuration across the domain
- Putting module-internal entity configuration in App.Infrastructure — violates separation of concerns
- Manually registering each config class in `OnModelCreating` instead of using assembly scan

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

# Check list
- [ ] `UnitOfWork` implemented in `App.Infrastructure/UnitOfWork/UnitOfWork.cs`
- [ ] Registered as `Scoped`
- [ ] AppDbContext defined here
- [ ] Generic `Repository<T>` implemented and registered
- [ ] DomainEventInterceptor registered on DbContext
- [ ] No module Application references this project
- [ ] `Ardalis.Specification.EntityFrameworkCore` referenced
- [ ] `Repository<T>` inherits `RepositoryBase<T>`
- [ ] Constructor forwards `AppDbContext` to base
- [ ] `Repository<T>` implements `IRepository<T>`
- [ ] No `SaveChangesAsync` calls in repository
- [ ] `EntityVersionResolverFactory` defined in `App.Infrastructure/Concurrency/EntityVersionResolverFactory.cs`
- [ ] Constructor accepts `IServiceProvider`, Domain assemblies, and Application assemblies
- [ ] Resolver-type map is built only once and thread-safe
- [ ] Scans Domain assemblies for `IEntityTypeConfiguration<T>` configs where `T` implements `IVersioned`
- [ ] Scans Application assemblies for `IEntityVersionResolver` implementations
- [ ] Every `{Entity}VersionResolver` declares matching `VersionedEntityName`
- [ ] Every mutable entity implements `IVersioned`
- [ ] Every mutable entity config class declares `VersionedEntityName`
- [ ] Keys are stable business strings
- [ ] DbContext uses `ApplyConfigurationsFromAssembly` on all module Domain assemblies
- [ ] Cross-module FK configs live in `/Persistence/Configurations`
- [ ] No intra-module entity config placed in App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
