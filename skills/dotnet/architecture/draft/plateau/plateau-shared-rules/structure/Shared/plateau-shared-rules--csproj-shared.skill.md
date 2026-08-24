---
name: csproj-shared
description: Project Shared in the shared-rules plateau
whenToUse: when adding or editing a cross-cutting interface/primitive in Shared, or deciding whether new code belongs here
domain: skill
type: template
plateau: shared-rules
version: 20260824150000
tags:
  - skill/template/csproj
  - plateau/shared-rules
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
  - "[[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
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
    - [ICommand.cs](./classes/plateau-shared-rules--class-i-command.skill.md)
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
  - /Exceptions
    - [EntityNotLoadedException.cs](./classes/plateau-shared-rules--class-entity-not-loaded-exception.skill.md)
  - Shared.csproj

Guid/external-created-entity primitives (`IHasGuid.cs`, `IGuidResolver.cs`, `/Results/ConflictResult.cs`) are not part of this plateau — they belong to `solution-external-created-entity`, not yet composed here.

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Events | Base event interfaces | |
| /MediatR | Command and query marker interfaces | [[./classes/plateau-shared-rules--class-i-command.skill.md\|class-i-command]] |
| /Repositories | Repository abstractions | |
| /Results | Result primitives and helpers | |
| /UnitOfWork | Unit of work abstraction | |
| /Outbox | Domain events marker interface | |
| /Concurrency | Version marker, version carrier, and entity resolver interfaces | |
| /Exceptions | `EntityNotLoadedException` — a Handler defect (missing preload), never confused with `DomainException` | [[./classes/plateau-shared-rules--class-entity-not-loaded-exception.skill.md\|class-entity-not-loaded-exception]] |

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

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
- Shared contain an implementation beyond an interface, a primitive, or a lightweight result/exception type with no business logic of its own (e.g. `DomainException`, `EntityNotLoadedException`) — never a class with a method that does real work
- `EntityNotLoadedException` be caught and treated as `DomainException` anywhere in the pipeline — it maps to `500` + critical log, since it signals a caller defect, not invalid input

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

# Check list
- [ ] Shared.csproj has no project references
- [ ] Shared.csproj contains only interfaces, primitives, and lightweight result/exception types
- [ ] No business logic in any Shared class
- [ ] `EntityNotLoadedException` is used for every "required navigation not loaded" case, mapped to 500, never confused with `DomainException`

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
