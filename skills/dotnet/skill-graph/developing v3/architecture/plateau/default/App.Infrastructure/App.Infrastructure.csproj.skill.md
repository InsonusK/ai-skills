---
uid: 9d9e08d7-0d36-4750-a2f2-b85049ba567b
name: app-infrastructure-csproj
description: Provide all persistence implementation — DbContext, repository implementations, outbox interceptor, background dispatcher
domain: skill
type: template
version: 20260616
tags:
  - skill/template/csproj
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration.solution.skill]]"
---

# Goal
- Provide the `UnitOfWork` EF Core implementation that wraps `AppDbContext.SaveChangesAsync`
- Provide all persistence implementation — DbContext, repository implementations, outbox interceptor, background dispatcher
- Be the only layer that knows EF Core implementation details
- Provide the single generic `Repository<T>` EF Core implementation
- Leverage `RepositoryBase<T>` from Ardalis to eliminate boilerplate spec evaluation code
- Own `EntityVersionResolver` — the mapping from stable string entity names to C# entity types
- Discover versioned entities automatically by scanning config classes in supplied assemblies
- Host cross-module foreign key configurations that span multiple bounded contexts
- Register all module entity configurations via `ApplyConfigurationsFromAssembly` in AppDbContext

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

# Core Principals
- `UnitOfWork` delegates directly to `AppDbContext.SaveChangesAsync` — no additional logic
- Registered as `Scoped` — shares the same `DbContext` instance as `Repository<T>`
- App.Infrastructure is the only project with a concrete DbContext
- App.Infrastructure implements interfaces defined in Shared
- No module Application or Domain layer references App.Infrastructure
- `RepositoryBase<T>` from Ardalis handles all `SpecificationEvaluator` logic internally
- App.Infrastructure remains the only layer that knows about EF Core implementation details
- One generic `Repository<T>` class covers all entity types
- Static readonly dictionary — populated at startup, no runtime modification
- Keys are stable business names declared in `{Entity}Config.VersionedEntityName` — changing a key is a breaking API change
- Entity types are discovered from `IEntityTypeConfiguration<T>` config classes in assemblies supplied during registration — typically module Domain assemblies
- Returns `null` for unknown names — `ConcurrencyBehavior` returns `Result.Error` on null
- App.Infrastructure is the only place where cross-module foreign key relationships are configured
- DbContext uses `ApplyConfigurationsFromAssembly` to automatically discover all `IEntityTypeConfiguration<T>` implementations from module Domain assemblies
- App.Infrastructure references all module Domain projects to access entities for cross-module configuration

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

# Structure

## Solution place
```
/src/App/App.Infrastructure
```


## Project Structure
```
/App.Infrastructure
  /UnitOfWork
    UnitOfWork.cs
```

```
/App.Infrastructure
  /Persistence
    AppDbContext.cs
    /Configurations
      OutboxMessageConfig.cs
  /Repositories
    Repository.cs
  /UnitOfWork
    UnitOfWork.cs
  /Outbox
    DomainEventInterceptor.cs
    OutboxDispatcher.cs
  /Concurrency
    EntityVersionResolver.cs
  App.Infrastructure.csproj
```

```
/App.Infrastructure
  /Repositories
    Repository.cs
```

```
/App.Infrastructure
  /Concurrency
    EntityVersionResolver.cs
```

```
/App.Infrastructure
  /Persistence
    /Configurations
      CrossModuleFkConfig.cs
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

## Directory and class skills
| `Directory|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
| /UnitOfWork | Unit of work implementations | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Infrastructure/classes/UnitOfWork.class.skill.md|UnitOfWork.class.skill]] |
| UnitOfWork.cs | EF Core SaveChangesAsync implementation | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Infrastructure/classes/UnitOfWork.class.skill.md|UnitOfWork.class.skill]] |
| /Persistence | DbContext and EF configurations |  |
| /Repositories | Generic Repository<T> implementation |  |
| /UnitOfWork | UnitOfWork implementation | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Infrastructure/classes/UnitOfWork.class.skill.md|UnitOfWork.class.skill]] |
| /Outbox | EF interceptor and background dispatcher |  |
| /Concurrency | EntityVersionResolver mapping strings to types |  |
| /Repositories/Repository.cs | Generic EF Core repository implementation inheriting Ardalis `RepositoryBase<T>` | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Infrastructure/classes/Repository.class.skill.md|Repository.class.skill]] |
| /Concurrency/EntityVersionResolver.cs | Maps string entity names to C# types for ConcurrencyBehavior by scanning config classes | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Infrastructure/classes/EntityVersionResolver.class.skill.md|EntityVersionResolver.class.skill]] |
| /Persistence/Configurations | Cross-module foreign key and relationship configurations |  |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides `DbContext.SaveChangesAsync` |
| `Ardalis.Specification.EntityFrameworkCore` | latest stable | Provides `RepositoryBase<T>` and EF Core spec evaluator |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides `IEntityTypeConfiguration<>` used to discover config classes |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Pipeline behavior registration — belongs to App.Host
- Cross-module JOIN queries — belongs to App.Queries
- Intra-module entity configurations — belong in respective `{Module}.Domain/Configurations`
- Domain entities — belong in `{Module}.Domain`
- Value Object definitions — belong in `{Module}.Domain/ValueObjects`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

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
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

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
	- `EntityVersionResolver` scans supplied assemblies for `IEntityTypeConfiguration<T>` configs where `T` implements `IVersioned`
	- Every mutable entity implements `IVersioned`
	- Every mutable entity config class declares `public const string VersionedEntityName`
	- Keys are stable business string names — same strings used in `IHasVersions` commands and ETag encoding
	- Constructor accepts `IEnumerable<Assembly>` from the composition root
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
	- Rely on a hardcoded dictionary of entity types
	- Define intra-module entity configurations here — those belong in `{Module}.Domain/Configurations`
	- Register configurations manually one by one in `OnModelCreating`
	- Reference BuildingBlocks directly

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

# Anti-patterns
- `UnitOfWork` containing retry logic — belongs in a decorator or policy, not the UoW
- `UnitOfWork` exposing `BeginTransaction` — violates single-responsibility contract
- Module Application referencing App.Infrastructure — use repository abstractions from Shared
- Putting cross-module JOIN queries in App.Infrastructure — belongs in App.Queries
- `TaskRepository : Repository<TodoTask>` — unnecessary subclass, generic handles all types
- Manual `SpecificationEvaluator.Default.GetQuery(...)` calls — Ardalis base handles this
- `EntityVersionResolver` key using `nameof(TodoTask)` — fragile, breaks on class rename
- Hardcoded dictionary of entity types — duplicates the entity list and is easy to forget when adding new entities
- Scanning `AppDomain.CurrentDomain.GetAssemblies()` without an explicit allow-list — includes unrelated assemblies
- Putting `VersionedEntityName` on the entity class instead of the config — spreads configuration across the domain
- Putting module-internal entity configuration in App.Infrastructure — violates separation of concerns
- Manually registering each config class in `OnModelCreating` instead of using assembly scan

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]

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
- [ ] `EntityVersionResolver` defined in `App.Infrastructure/Concurrency/EntityVersionResolver.cs`
- [ ] Constructor accepts `IEnumerable<Assembly>`
- [ ] Scans supplied assemblies for `IEntityTypeConfiguration<T>` configs where `T` implements `IVersioned`
- [ ] Every mutable entity implements `IVersioned`
- [ ] Every mutable entity config class declares `VersionedEntityName`
- [ ] Keys are stable business strings
- [ ] DbContext uses `ApplyConfigurationsFromAssembly` on all module Domain assemblies
- [ ] Cross-module FK configs live in `/Persistence/Configurations`
- [ ] No intra-module entity config placed in App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
