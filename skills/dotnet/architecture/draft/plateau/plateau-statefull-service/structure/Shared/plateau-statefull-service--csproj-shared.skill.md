---
name: plateau-statefull-service--csproj-shared
description: Project Shared in the statefull-service plateau
whenToUse: when adding or editing a cross-cutting interface/primitive in Shared, or deciding whether new code belongs here
domain: skill
type: template
plateau: statefull-service
version: 20260824100000
tags:
  - skill/template/csproj
  - plateau/statefull-service
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
  - "[[../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]"
  - "[[../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
  - "[[../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]]"
  - "[[../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]]"
---

# Goal
- Define common cross-cutting interfaces that every layer can safely depend on without creating coupling
- Provide base types and contracts used across module and infrastructure boundaries

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

# Core Principles
- Shared defines common interfaces and primitives — it has no implementations beyond lightweight result/contract helpers
- Shared has no dependencies on any other project in this solution
- Any project at any layer may depend on Shared

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

# Structure

## Solution place
```
/src/Shared
```

## Project Structure
- /Shared
  - /Events
    - IDomainEvent.cs
  - /MediatR
    - [ICommand.cs](./classes/plateau-statefull-service--class-i-command.skill.md)
    - [IQuery.cs](./classes/plateau-statefull-service--class-i-query.skill.md)
  - /Repositories
    - [IRepository.cs, IReadRepository.cs](./classes/plateau-statefull-service--class-i-repository.skill.md)
  - /Results
    - [ConflictResult.cs](./classes/plateau-statefull-service--class-guid-contracts.skill.md)
  - /UnitOfWork
    - [IUnitOfWork.cs](./classes/plateau-statefull-service--class-i-unit-of-work.skill.md)
  - /Outbox
    - IHasDomainEvents.cs
  - /Concurrency
    - [IVersioned.cs, IHasVersions.cs, IEntityVersionResolverFactory.cs, IEntityVersionResolver.cs](./classes/plateau-statefull-service--class-concurrency-contracts.skill.md)
  - /Guid
    - [IHasGuid.cs, IGuidResolver.cs](./classes/plateau-statefull-service--class-guid-contracts.skill.md)
  - /Timestamps
    - [ICreationInfoModel(ReadOnly).cs, IUpdateInfoModel(ReadOnly).cs, ICommandWithTimestamp.cs](./classes/plateau-statefull-service--class-timestamp-contracts.skill.md)
  - Shared.csproj

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Events | Base event interfaces | |
| /MediatR | Command and query marker interfaces | [[./classes/plateau-statefull-service--class-i-command.skill.md\|class-i-command]] |
| /Repositories | Repository abstractions | |
| /Results | Result primitives and helpers | |
| /UnitOfWork | Unit of work abstraction | |
| /Outbox | Domain events marker interface | |
| /Concurrency | Version marker, version carrier, and entity resolver interfaces | |

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Provides `IRequest<T>` that `ICommand<T>`/`IQuery<T>` extend |
| `Ardalis.Result` | latest stable | Provides `Result<T>` that `ConflictResult<T>` extends |
| `Ardalis.Specification` | latest stable | Provides `IReadRepositoryBase<T>`/`IRepositoryBase<T>` that `IReadRepository<T>`/`IRepository<T>` wrap |

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Pipeline behaviors — belongs to BuildingBlocks
- Infrastructure implementations — belongs to App.Infrastructure or BuildingBlocks
- Repository or unit-of-work implementations — belong to BuildingBlocks or App.Infrastructure

## Allowed Dependencies
- None — Shared has no project references

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

# Rules
MUST:
- Shared has zero project references
- All types in Shared are purely cross-cutting primitives
MUST NOT:
- Shared reference any module, BuildingBlocks, or infrastructure project
- Shared contain business logic or domain rules
- Shared contain implementations — only interfaces and primitives

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

# Check list
- [ ] Shared.csproj has no project references
- [ ] Shared.csproj contains only interfaces and primitives
- [ ] No business logic in any Shared class

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
