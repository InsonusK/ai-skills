---
name: csproj-shared
description: Project Shared in the stateless-non-interactive-service plateau
whenToUse: when adding or editing a cross-cutting interface/primitive in Shared, or deciding whether new code belongs here
domain: skill
type: template
plateau: stateless-non-interactive-service
version: 20260821120000
tags:
  - skill/template/csproj
  - plateau/stateless-non-interactive-service
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
---

# Goal
- Define common cross-cutting interfaces that every layer can safely depend on without creating coupling
- Provide base types and contracts used across module and infrastructure boundaries

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]

# Core Principles
- Shared defines common interfaces and primitives — it has no implementations beyond lightweight result/contract helpers
- Shared has no dependencies on any other project in this solution
- Any project at any layer may depend on Shared

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]

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
    - ICommand.cs
    - IQuery.cs
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

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Events | Base event interfaces | |
| /MediatR | Command and query marker interfaces | |
| /Repositories | Repository abstractions | |
| /Results | Result primitives and helpers | |
| /UnitOfWork | Unit of work abstraction | |
| /Outbox | Domain events marker interface | |
| /Concurrency | Version marker, version carrier, and entity resolver interfaces | |

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| — | — | No package is required by anything in this plateau's slice of Shared; `Ardalis.Result` is only needed once `solution-external-created-entity` is composed in. |

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Pipeline behaviors — belongs to BuildingBlocks
- Infrastructure implementations — belongs to App.Infrastructure or BuildingBlocks
- Repository or unit-of-work implementations — belong to BuildingBlocks or App.Infrastructure

## Allowed Dependencies
- None — Shared has no project references

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]

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

# Check list
- [ ] Shared.csproj has no project references
- [ ] Shared.csproj contains only interfaces and primitives
- [ ] No business logic in any Shared class

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
