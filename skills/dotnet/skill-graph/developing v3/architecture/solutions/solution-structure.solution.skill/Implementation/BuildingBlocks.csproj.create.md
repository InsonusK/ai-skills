---
description: Provide reusable framework-level patterns used by Application layer and infrastructure across all modules
name: BuildingBlocks.csproj
change_kind: create
---

# Goals
- Provide reusable framework-level patterns used by Application layer and infrastructure across all modules
- Define pipeline behavior contracts and base implementations

# Core Principals
- BuildingBlocks contains reusable technical patterns, not business logic
- BuildingBlocks depends only on Shared
- All pipeline behaviors live here — registered once in App.Host, used by all modules

# Structure

## Project Structure
```
/BuildingBlocks
  /MediatR
    ICommand.cs
    IQuery.cs
    UnitOfWorkContext.cs
  /Repositories
    IRepository.cs
    IReadRepository.cs
  /UnitOfWork
    IUnitOfWork.cs
  /Outbox
    OutboxMessage.cs
    IHasDomainEvents.cs
  /Concurrency
    IHasVersions.cs
    IEntityVersionResolver.cs
    ETagEncoder.cs
  BuildingBlocks.csproj
```

## Directory and class skills
| `Directory\|file` | Description                                             |
| ----------------- | ------------------------------------------------------- |
| /MediatR          | Command/Query markers, pipeline behavior base contracts |
| /Repositories     | IRepository and IReadRepository abstractions            |
| /UnitOfWork       | IUnitOfWork abstraction                                 |
| /Outbox           | OutboxMessage and IHasDomainEvents                      |
| /Concurrency      | ETag, version interfaces                                |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Business logic — belongs to Domain
- Infrastructure implementations — belongs to App.Infrastructure
- Module-specific handlers or validators — belong to module Application

# Allowed Dependencies
- Shared

# Rules

MUST:
- All pipeline behavior contracts defined here
- IRepository, IReadRepository, IUnitOfWork defined here
- BuildingBlocks depends only on Shared

MUST NOT:
- BuildingBlocks reference any module project
- BuildingBlocks reference App.Infrastructure or App.Queries
- BuildingBlocks contain business logic

# Anti-patterns
- Placing domain entities in BuildingBlocks — they belong in module Domain
- Placing infrastructure implementations in BuildingBlocks — they belong in App.Infrastructure
- Adding module-specific handlers or validators in BuildingBlocks

# Check list
- [ ] BuildingBlocks.csproj references only Shared
- [ ] IRepository, IReadRepository, IUnitOfWork present
- [ ] ICommand, IQuery markers present
