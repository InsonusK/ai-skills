---
name: csproj-app-infrastructure
description: Project App.Infrastructure in the v1 plateau
whenToUse: when adding or editing a persistence implementation detail (DbContext, repository, unit of work, cross-module config), or deciding whether new code belongs here
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/csproj
  - plateau/v1
created_by:
  - "[[../../../../solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]"
  - "[[../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../../solutions/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]]"
  - "[[../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]"
  - "[[../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]]"
---

# Goal
- Be the single home for outbound infrastructure integrations — persistence today, other concerns (cache, ...) later — each added by its own solution extending this same project
- Be the only project that knows EF Core implementation details

__Applied solutions:__
- [[../../../../solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]] - [[../../../../solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj.create]]

# Core Principles
- Created empty by `solution-infrastructure-project`; every concern below is a sibling solution extending it, never redefining it
- `AppDbContext` is the only `DbContext` in the service, applying every module's configurations via `ApplyConfigurationsFromAssembly`
- `Repository<T>` is the single generic implementation for all entity types
- `UnitOfWork` and `EntityVersionResolverFactory` are the only components that call `SaveChangesAsync` / resolve versions across modules

# Structure

## Project Structure
- /App.Infrastructure
  - /Persistence
    - [AppDbContext.cs](./classes/plateau-v1--class-appdbcontext.skill.md)
    - /Configurations
      - {Module1}To{Module2}Config.cs — cross-module FK configs (`solution-domain-configuration`)
  - /Repositories
    - [Repository.cs](./classes/plateau-v1--class-repository.skill.md)
  - /UnitOfWork
    - [UnitOfWork.cs](./classes/plateau-v1--class-unit-of-work.skill.md)
  - /Concurrency
    - [EntityVersionResolverFactory.cs](./classes/plateau-v1--class-entity-version-resolver-factory.skill.md)
  - App.Infrastructure.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Persistence/AppDbContext.cs | The service's single `DbContext` | [[./classes/plateau-v1--class-appdbcontext.skill.md\|class-appdbcontext]] |
| /Persistence/Configurations | Cross-module foreign-key configurations | |
| /Repositories/Repository.cs | Generic EF Core repository | [[./classes/plateau-v1--class-repository.skill.md\|class-repository]] |
| /UnitOfWork/UnitOfWork.cs | `IUnitOfWork` EF Core implementation | [[./classes/plateau-v1--class-unit-of-work.skill.md\|class-unit-of-work]] |
| /Concurrency/EntityVersionResolverFactory.cs | Maps entity names to version resolvers | [[./classes/plateau-v1--class-entity-version-resolver-factory.skill.md\|class-entity-version-resolver-factory]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Microsoft.EntityFrameworkCore` | latest stable | `DbContext`, `ApplyConfigurationsFromAssembly` |
| `Ardalis.Specification.EntityFrameworkCore` | latest stable | `RepositoryBase<T>` and EF spec evaluator |

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Pipeline behavior registration — belongs to App.Host
- Cross-module JOIN queries — belongs to App.Queries
- Intra-module entity configuration — belongs to `{Module}.Domain/Configurations`

## Allowed Dependencies
- BuildingBlocks
- Shared
- `{ModuleName}.Domain` (all modules)
- `{ModuleName}.Interfaces` (all modules)

# Rules
MUST:
- App.Infrastructure is the only project with a `DbContext`
- `Repository<T>`/`UnitOfWork`/`EntityVersionResolverFactory` registered `Scoped`
- Cross-module FK configs live in `/Persistence/Configurations`, never intra-module entity config
MUST NOT:
- Be referenced by any module `Application`/`Domain`/`Api` directly — only through Shared abstractions
- Contain business logic

# Check list
- [ ] `AppDbContext` is the only `DbContext`, applies configs via `ApplyConfigurationsFromAssembly`
- [ ] `Repository<T>`, `UnitOfWork`, `EntityVersionResolverFactory` all registered `Scoped`
- [ ] No module `Application`/`Domain` references this project directly

__Applied solutions:__
- [[../../../../solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]] - [[../../../../solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj.create]]
- [[../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[../../../../solutions/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[../../../../solutions/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] - [[../../../../solutions/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../solutions/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[../../../../solutions/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
