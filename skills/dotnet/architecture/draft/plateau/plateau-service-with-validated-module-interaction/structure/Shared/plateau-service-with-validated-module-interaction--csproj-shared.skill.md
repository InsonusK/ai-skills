---
name: csproj-shared
description: Project Shared in the service-with-validated-module-interaction plateau
whenToUse: when adding or editing a cross-cutting interface/primitive in Shared, or deciding whether new code belongs here
domain: skill
type: template
plateau: service-with-validated-module-interaction
version: 20260822140000
tags:
  - skill/template/csproj
  - plateau/service-with-validated-module-interaction
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
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
    - [ICommand.cs](./classes/plateau-service-with-validated-module-interaction--class-i-command.skill.md)
    - IQuery.cs (still a placeholder — a future `solution-query-integration` fills it in)
  - /Repositories
    - IRepository.cs
    - IReadRepository.cs
  - /Results
  - /UnitOfWork
    - IUnitOfWork.cs
  - /Outbox
    - IHasDomainEvents.cs
  - /Concurrency
    - IVersioned.cs
    - IHasVersions.cs
    - IEntityVersionResolver.cs
  - Shared.csproj

Guid/external-created-entity primitives (`IHasGuid.cs`, `IGuidResolver.cs`, `/Results/ConflictResult.cs`) are not part of this plateau — they belong to `solution-external-created-entity`, not yet composed here.

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Events | Base event interfaces | |
| /MediatR | Command and query marker interfaces | [[./classes/plateau-service-with-validated-module-interaction--class-i-command.skill.md\|class-i-command]] |
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
| `MediatR` | latest stable | Provides `IRequest<T>` that `ICommand<T>` extends |

`Ardalis.Result` is not referenced here — `solution-external-created-entity` is what needs it, not yet composed in.

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
