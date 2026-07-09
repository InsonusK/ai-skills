---
description: Define common cross-cutting interfaces and primitives that every layer can safely depend on without creating coupling
name: Shared.csproj
element_kind: project
change_kind: create
---

# Goals
- Define common cross-cutting interfaces that every layer can safely depend on without creating coupling
- Provide base types and contracts used across module and infrastructure boundaries

# Core Principles
- Shared defines common interfaces and primitives — it has no implementations beyond lightweight result/contract helpers
- Shared has no business logic — only framework-level contracts and primitives
- Shared has no dependencies on any other project in this solution
- Any project at any layer may depend on Shared

# Structure

## Project Structure
```
/Shared
  /Events
    IDomainEvent.cs
  /Guid
    IHasGuid.cs           ← solution-external-created-entity.skill
    IGuidResolver.cs      ← solution-external-created-entity.skill
  /MediatR
    ICommand.cs
    IQuery.cs
  /Repositories
    IRepository.cs
    IReadRepository.cs
  /Results
    ConflictResult.cs     ← solution-external-created-entity.skill
  /UnitOfWork
    IUnitOfWork.cs
  /Outbox
    IHasDomainEvents.cs
  /Concurrency
    IVersioned.cs
    IHasVersions.cs
    IEntityVersionResolver.cs
  Shared.csproj
```

## Directory and class skills
| `Directory\|file` | Description                               |
| ----------------- | ----------------------------------------- |
| /Events           | Base event interfaces                     |
| /Guid             | External-created entity marker and resolver contracts |
| /MediatR          | Command and query marker interfaces       |
| /Repositories     | Repository abstractions                   |
| /Results          | Result primitives and helpers             |
| /UnitOfWork       | Unit of work abstraction                  |
| /Outbox           | Domain events marker interface            |
| /Concurrency      | Version marker, version carrier, and entity resolver interfaces |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| Ardalis.Result | {version} | Provides `Result<T>` base for `ConflictResult<T>` |

# What Does NOT Belong Here
- Business logic — belongs to Domain
- Pipeline behaviors — belongs to BuildingBlocks
- Infrastructure implementations — belongs to App.Infrastructure or BuildingBlocks
- Repository or unit-of-work implementations — belong to BuildingBlocks or App.Infrastructure

# Allowed Dependencies
- `Ardalis.Result` — required by `ConflictResult<T>` from solution-external-created-entity.skill
- None — Shared has no project references

# Rules

## MUST
- Shared has zero project references
- All types in Shared are purely cross-cutting primitives
- `Shared.csproj` references `Ardalis.Result` when `ConflictResult<T>` is used
- Shared has no project dependencies

## MUST NOT
- Shared reference any module, BuildingBlocks, or infrastructure project
- Shared contain business logic or domain rules
- Shared contain implementations — only interfaces and primitives

# Anti-patterns
- Placing domain entities in Shared — they belong in module Domain
- Placing pipeline behaviors in Shared — they belong in BuildingBlocks
- Adding project references to Shared.csproj
- Placing implementations in Shared — they belong in BuildingBlocks or App.Infrastructure

# Check list
- [ ] Shared.csproj has no project references
- [ ] Shared.csproj contains only interfaces and primitives
- [ ] No business logic in any Shared class
- [ ] Shared.csproj references `Ardalis.Result` when solution-external-created-entity.skill is used
