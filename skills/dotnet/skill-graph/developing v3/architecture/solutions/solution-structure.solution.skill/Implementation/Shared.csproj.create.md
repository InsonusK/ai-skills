---
description: Define common cross-cutting interfaces and primitives that every layer can safely depend on without creating coupling
name: Shared.csproj
change_kind: create
---

# Goals
- Define common cross-cutting interfaces that every layer can safely depend on without creating coupling
- Provide base types and contracts used across module and infrastructure boundaries

# Core Principals
- Shared defines common interfaces — it has no implementations
- Shared has no business logic — only framework-level contracts and primitives
- Shared has no dependencies on any other project in this solution
- Any project at any layer may depend on Shared

# Structure

## Project Structure
```
/Shared
  /Events
    IDomainEvent.cs
  /Exceptions
    DomainException.cs
    ConflictException.cs
  /MediatR
    ICommand.cs
    IQuery.cs
  /Repositories
    IRepository.cs
    IReadRepository.cs
  /UnitOfWork
    IUnitOfWork.cs
  /Outbox
    IHasDomainEvents.cs
  /Concurrency
    IHasVersions.cs
    IEntityVersionResolver.cs
  Shared.csproj
```

## Directory and class skills
| `Directory\|file` | Description                               |
| ----------------- | ----------------------------------------- |
| /Events           | Base event interfaces                     |
| /Exceptions       | Shared exception types used across layers |
| /MediatR          | Command and query marker interfaces       |
| /Repositories     | Repository abstractions                   |
| /UnitOfWork       | Unit of work abstraction                  |
| /Outbox           | Domain events marker interface            |
| /Concurrency      | Version and ETag resolver interfaces      |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Business logic — belongs to Domain
- Pipeline behaviors — belongs to BuildingBlocks
- Infrastructure implementations — belongs to App.Infrastructure or BuildingBlocks
- Repository or unit-of-work implementations — belong to BuildingBlocks or App.Infrastructure

# Allowed Dependencies
- None — Shared has no project dependencies

# Rules

MUST:
- Shared has zero project references
- All types in Shared are purely cross-cutting primitives

MUST NOT:
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
