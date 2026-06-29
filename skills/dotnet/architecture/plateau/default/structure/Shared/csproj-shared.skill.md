---
name: csproj-shared
description: Define common cross-cutting interfaces and primitives that every layer can safely depend on without creating coupling
domain: skill
type: template
version: 20260622
plateau: default
tags:
  - skill/template/csproj
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration.skill]]"
---

# Goal
- Host Value Objects and rules that are reused by two or more module Domain projects
- Prevent duplication of identical domain logic across module boundaries
- Provide a single source of truth for cross-cutting domain primitives
- Own `IUnitOfWork` — the commit contract accessible by every layer without infrastructure coupling
- Define common cross-cutting interfaces that every layer can safely depend on without creating coupling
- Provide base types and contracts used across module and infrastructure boundaries
- Make `Ardalis.Specification` interfaces available to every layer without coupling to EF Core
- Define `IReadRepository<T>` and `IRepository<T>` as thin wrappers around Ardalis base interfaces
- Own the `IQuery<TResponse>` marker interface for read-only operations
- Make the read-only operation marker available to every layer without coupling to BuildingBlocks
- Own `ConflictResult<T>` — the result type used by resolvers to express a duplicate Guid conflict
- Own `IHasGuid` — the marker interface for commands carrying a client-generated Guid
- Own `IGuidResolver<TResponse>` — the per-entity resolver contract consumed by `GuidResolvingBehavior`
- Own the common concurrency contracts that every layer can safely depend on without coupling to BuildingBlocks
- Provide `IVersioned` so domain entities can declare themselves as versioned and be discovered by infrastructure
- Provide `IEntityVersionResolverFactory` and `IEntityVersionResolver` so the pipeline behavior can check versions without knowing entity types
- Make the `ICommand` marker available to every layer without coupling to BuildingBlocks
- Enable MediatR routing and pipeline behavior constraints for write operations

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

# Core Principals
- Shared contains only cross-cutting primitives — no business logic specific to a single module
- Any project at any layer may depend on Shared
- A VO or rule moves to Shared only when at least two modules need it
- Shared VOs and rules follow the same patterns as module-local ones
- Interface only — no EF Core dependency in Shared
- Single method: `SaveChangesAsync` — nothing else
- Shared defines common interfaces and primitives — it has no implementations beyond lightweight result/contract helpers
- Shared has no business logic — only framework-level contracts and primitives
- Shared has no dependencies on any other project in this solution
- Shared has no EF Core dependencies — only the lightweight `Ardalis.Specification` package
- Interfaces are thin — they inherit all Ardalis methods, adding no new signatures unless required
- Any layer may depend on Shared safely
- Shared defines only interfaces and markers — no implementations
- `IQuery<TResponse>` extends MediatR `IRequest<TResponse>` so MediatR can route queries automatically
- All three live in Shared — they are contracts or primitives implemented/consumed by multiple layers
- `ConflictResult<T>` is accessible by module Application resolvers (created) and BuildingBlocks behavior (returned)
- `IHasGuid` is implemented by commands in `{Module}.Interfaces`
- `IGuidResolver<TResponse>` is implemented by resolvers in `{Module}.Application` and consumed by `GuidResolvingBehavior` in BuildingBlocks
- Shared defines common cross-cutting primitives only — no business logic, no pipeline implementations
- `IHasVersions` and `IEntityVersionResolverFactory` are cross-cutting contracts referenced by both Application and Api layers
- `IVersioned` is implemented by mutable entities in module Domain projects
- `IEntityVersionResolver` is implemented by module Application projects
- `ICommand<TResponse>` extends MediatR `IRequest<TResponse>` so MediatR can route commands automatically

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

# Structure

## Solution place
```
/src/Shared
```


## Project Structure
- /Shared
  - /ValueObjects
    - Email.cs
    - Money.cs
  - /Rules
    - StringRules.cs
    - IntRules.cs
  - /Exceptions
    - DomainException.cs
  - /Events
    - IDomainEvent.cs
  - /Guid
    - [IHasGuid.cs](skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-has-guid.skill.md)
    - [IGuidResolver.cs](skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-guid-resolver.skill.md)
  - /MediatR
    - [ICommand.cs](skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-command.skill.md)
    - [IQuery.cs](skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-query.skill.md)
  - /Repositories
    - [IReadRepository.cs](skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-read-repository.skill.md)
    - [IRepository.cs](skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-repository.skill.md)
  - /Results
    - [ConflictResult.cs](skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-conflict-result.skill.md)
  - /UnitOfWork
    - [IUnitOfWork.cs](skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-unit-of-work.skill.md)
  - /Outbox
    - IHasDomainEvents.cs
  - /Concurrency
    - [IHasVersions.cs](skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-has-versions.skill.md)
    - [IEntityVersionResolverFactory.cs](skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-entity-version-resolver-factory.skill.md)
    - [IEntityVersionResolver.cs](skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-entity-version-resolver.skill.md)
    - [IVersioned.cs](skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-versioned.skill.md)
  - Shared.csproj

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

## Directory and class skills
| `Directory|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
| /ValueObjects | Cross-module reusable Value Object types |  |
| /Rules | Cross-module reusable domain rule static classes |  |
| /UnitOfWork | Unit of work contracts |  |
| IUnitOfWork.cs | Commit contract — single SaveChangesAsync method | [[skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-unit-of-work.skill.md|class-IUnitOfWork.skill]] |
| /Events | Base event interfaces |  |
| /Guid | External-created entity marker and resolver contracts |  |
| /MediatR | Command and query marker interfaces |  |
| /Repositories | Repository abstractions |  |
| /Results | Result primitives and helpers |  |
| /UnitOfWork | Unit of work abstraction |  |
| /Outbox | Domain events marker interface |  |
| /Concurrency | Version marker, version carrier, and entity resolver interfaces |  |
| /MediatR | MediatR marker interfaces |  |
| IQuery.cs | Read-only operation marker interface | [[skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-query.skill.md|class-IQuery.skill]] |
| /Results/ConflictResult.cs | `Result<T>` subclass carrying the existing entity result for 409 responses | [[skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-conflict-result.skill.md|class-ConflictResult.skill]] |
| /Guid/IHasGuid.cs | Marker interface for commands carrying a client-generated Guid | [[skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-has-guid.skill.md|class-IHasGuid.skill]] |
| /Guid/IGuidResolver.cs | Per-entity resolver contract — resolves Guid to existing command response | [[skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-guid-resolver.skill.md|class-IGuidResolver.skill]] |
| /Concurrency/IHasVersions.cs | Interface carried by all update commands | [[skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-has-versions.skill.md|class-IHasVersions.skill]] |
| /Concurrency/IEntityVersionResolverFactory.cs | Factory that resolves an entity name to an `IEntityVersionResolver` | [[skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-entity-version-resolver-factory.skill.md|class-IEntityVersionResolverFactory.skill]] |
| /Concurrency/IEntityVersionResolver.cs | Reads the current version for one versioned entity | [[skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-entity-version-resolver.skill.md|class-IEntityVersionResolver.skill]] |
| /Concurrency/IVersioned.cs | Marker interface for versioned domain entities | [[skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-versioned.skill.md|class-IVersioned.skill]] |
| ICommand.cs | Write operation marker interfaces | [[skills/dotnet/architecture/plateau/default/structure/Shared/classes/class-i-command.skill.md|class-ICommand.skill]] |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| Ardalis.Result | {version} | Provides `Result<T>` base for `ConflictResult<T>` |
| `Ardalis.Specification` | latest stable | Provides `IReadRepositoryBase<T>` and `IRepositoryBase<T>` |
| `MediatR` | latest stable | Provides `IRequest<T>` that `IQuery<T>` extends |
| None | — | Interfaces only — no external dependencies |
| `MediatR` | latest stable | Provides `IRequest<T>` that `ICommand<T>` extends |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

## What Does NOT Belong Here
- Module-specific Value Objects — belong in respective `{Module}.Domain/ValueObjects`
- Module-specific rules — belong in respective `{Module}.Domain/Rules`
- Business logic — belongs to Domain
- Infrastructure implementations — belong to BuildingBlocks or App.Infrastructure
- Pipeline behaviors — belongs to BuildingBlocks
- Infrastructure implementations — belongs to App.Infrastructure or BuildingBlocks
- Repository or unit-of-work implementations — belong to BuildingBlocks or App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

## Allowed Dependencies
- None — Shared has no project dependencies
- `Ardalis.Result` — required by `ConflictResult<T>` from solution-external-created-entity.skill
- None — Shared has no project references
- `Ardalis.Result` — required for `ConflictResult<T>` to inherit from `Result<T>`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

# Rules
MUST:
	- A VO or rule lives in Shared only when used by two or more modules
	- Shared VOs follow the same `sealed record`, immutable, self-validating rules as module VOs
	- Shared rules follow the same static extension method, bool-return, stateless rules as module rules
	- `IUnitOfWork` defined in Shared — not BuildingBlocks, not App.Infrastructure
	- Shared has zero project references
	- All types in Shared are purely cross-cutting primitives
	- `Shared.csproj` references `Ardalis.Result` when `ConflictResult<T>` is used
	- `Ardalis.Specification` package referenced in `Shared.csproj`
	- `IReadRepository<T>` inherit `IReadRepositoryBase<T>`
	- `IRepository<T>` inherit `IRepositoryBase<T>` and `IReadRepository<T>`
	- Both interfaces placed in `/Shared/Repositories`
	- `MediatR` package referenced in `Shared.csproj`
	- `IQuery<TResponse>` placed in `/Shared/MediatR`
	- `IQuery<TResponse>` extends MediatR `IRequest<TResponse>`
	- `IQuery<TResponse>` does NOT extend `ICommand` or `ICommand<TResponse>`
	- `ConflictResult<T>`, `IHasGuid`, `IGuidResolver<TResponse>` defined in Shared
	- `ConflictResult<T>` inherits from `Ardalis.Result.Result<T>` and sets `Status` to `ResultStatus.Conflict`
	- `IGuidResolver<TResponse>` returns `Task<TResponse?>` — null means not found, non-null means conflict
	- `TResponse` of `IGuidResolver` matches the command handler response type exactly
	- `IHasVersions`, `IEntityVersionResolverFactory`, `IEntityVersionResolver`, and `IVersioned` defined in Shared
	- All three types are interfaces or markers only — no implementation code
	- `ICommand` and `ICommand<TResponse>` placed in `/Shared/MediatR`
	- Both interfaces extend MediatR `IRequest` / `IRequest<TResponse>`
MUST NOT:
	- Place module-specific VO or rule in Shared
	- Add project references to Shared.csproj
	- Put business logic in Shared VOs or rules
	- Shared reference EF Core
	- Shared reference any module, BuildingBlocks, or infrastructure project
	- Shared contain business logic or domain rules
	- Shared contain implementations — only interfaces and primitives
	- Reference `Ardalis.Specification.EntityFrameworkCore` in Shared
	- Add custom method signatures to the interfaces unless they are cross-cutting concerns
	- Add FluentValidation, Ardalis.Result, or EF Core packages to Shared
	- Add implementation code to Shared
	- `IQuery` extend `ICommand` — queries must remain distinct from write-side markers
	- `ConflictResult<T>`, `IHasGuid`, or `IGuidResolver<TResponse>` defined in BuildingBlocks — they are consumed by multiple layers
	- Shared reference any other project
	- Add MediatR, EF Core, or JSON serialization dependencies to Shared for these contracts
	- Place implementations in Shared

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

# Anti-patterns
- Putting every VO/rule in Shared "just in case" — Shared should stay minimal
- Duplicating a Shared VO/rule back into a module Domain project
- Adding module-specific behavior to a Shared VO or rule
- `IUnitOfWork` defined in BuildingBlocks — creates unnecessary coupling
- Adding transaction methods to `IUnitOfWork` — only `SaveChangesAsync` belongs here
- Placing domain entities in Shared — they belong in module Domain
- Placing pipeline behaviors in Shared — they belong in BuildingBlocks
- Adding project references to Shared.csproj
- Placing implementations in Shared — they belong in BuildingBlocks or App.Infrastructure
- Duplicating Ardalis method signatures manually instead of inheriting the base interfaces
- Adding infrastructure concerns to the repository interfaces
- Defining `IQuery` in BuildingBlocks — forces modules to reference BuildingBlocks for contracts
- `IQuery` extending `ICommand` — would blur the boundary between read and write operations
- Guid contracts defined in BuildingBlocks — forces `{Module}.Interfaces` and `{Module}.Application` to reference BuildingBlocks for contracts
- `IGuidResolver` returning a different response type than the command handler
- Defining `IHasVersions` or `IEntityVersionResolver` in BuildingBlocks — forces modules to reference BuildingBlocks for contracts
- Defining `ICommand` in BuildingBlocks — forces modules to reference BuildingBlocks for contracts
- Adding behavior logic to a marker interface

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

# Check list
- [ ] /ValueObjects folder exists in Shared
- [ ] /Rules folder exists in Shared
- [ ] Every VO in /Shared/ValueObjects is referenced by at least two modules
- [ ] Every rule in /Shared/Rules is referenced by at least two modules
- [ ] Shared.csproj has no project references
- [ ] `IUnitOfWork` defined in `Shared/UnitOfWork/IUnitOfWork.cs`
- [ ] No EF Core reference in Shared
- [ ] Shared.csproj contains only interfaces and primitives
- [ ] No business logic in any Shared class
- [ ] Shared.csproj references `Ardalis.Result` when solution-external-created-entity.skill is used
- [ ] `Ardalis.Specification` referenced in `Shared.csproj`
- [ ] `/Repositories` folder exists
- [ ] `IReadRepository<T>` inherits `IReadRepositoryBase<T>`
- [ ] `IRepository<T>` inherits `IRepositoryBase<T>` and `IReadRepository<T>`
- [ ] `MediatR` referenced in `Shared.csproj`
- [ ] `/Shared/MediatR/IQuery.cs` exists
- [ ] `IQuery<TResponse>` extends `IRequest<TResponse>`
- [ ] `IQuery<TResponse>` does not extend `ICommand`
- [ ] `ConflictResult<T>` defined in `Shared/Results/ConflictResult.cs`
- [ ] `IHasGuid` defined in `Shared/Guid/IHasGuid.cs`
- [ ] `IGuidResolver<TResponse>` defined in `Shared/Guid/IGuidResolver.cs`
- [ ] `Shared.csproj` references `Ardalis.Result`
- [ ] `IHasVersions` defined in `Shared/Concurrency/IHasVersions.cs`
- [ ] `IEntityVersionResolverFactory` defined in `Shared/Concurrency/IEntityVersionResolverFactory.cs`
- [ ] `IEntityVersionResolver` defined in `Shared/Concurrency/IEntityVersionResolver.cs`
- [ ] `IVersioned` defined in `Shared/Concurrency/IVersioned.cs`
- [ ] Shared.csproj has no project references and no new NuGet packages for these contracts
- [ ] `/Shared/MediatR/ICommand.cs` exists
- [ ] `ICommand` extends `IRequest`
- [ ] `ICommand<TResponse>` extends `IRequest<TResponse>`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
