---
uid:
name: buildingblocks-csproj
description: Reusable framework patterns project — pipeline behaviors, base repository specs, ICommand, IQuery, IUnitOfWork.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/csproj
  - dotnet
  - buildingblocks
  - framework
triggers:
  - create BuildingBlocks project
  - add building blocks layer
  - implement framework patterns
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill]]"
extended_by:
---

# Goal
- Provide reusable framework-level patterns used by Application layer and infrastructure across all modules
- Define pipeline behavior contracts and base implementations

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#BuildingBlocks (.csproj)]]

# Core Principles
- BuildingBlocks contains reusable technical patterns, not business logic
- BuildingBlocks depends only on Shared
- All pipeline behaviors live here — registered once in App.Host, used by all modules

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#BuildingBlocks (.csproj)]]

# Structure

## Solution place
```
/src
  /BuildingBlocks
```

## Project Structure
```
/BuildingBlocks
  /MediatR
  /Repositories
  /UnitOfWork
  /Outbox
  /Concurrency
  BuildingBlocks.csproj
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#BuildingBlocks (.csproj)]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /MediatR | Command/Query markers, pipeline behavior base contracts | |
| /Repositories | IRepository and IReadRepository abstractions | |
| /UnitOfWork | IUnitOfWork abstraction | |
| /Outbox | OutboxMessage and IHasDomainEvents | |
| /Concurrency | ETag, version interfaces | |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#BuildingBlocks (.csproj)]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Infrastructure implementations — belongs to App.Infrastructure
- Module-specific handlers or validators — belong to module Application

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#BuildingBlocks (.csproj)]]

## Allowed Dependencies
- Shared

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#BuildingBlocks (.csproj)]]

# Rules

MUST:
- All pipeline behavior contracts defined here
- IRepository, IReadRepository, IUnitOfWork defined here
- BuildingBlocks depends only on Shared

MUST NOT:
- BuildingBlocks reference any module project
- BuildingBlocks reference App.Infrastructure or App.Queries
- BuildingBlocks contain business logic

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#BuildingBlocks (.csproj)]]

# Anti-patterns
- Placing domain entities in BuildingBlocks — they belong in module Domain
- Adding infrastructure references to BuildingBlocks

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#BuildingBlocks (.csproj)]]

# Check list
- [ ] BuildingBlocks.csproj references only Shared
- [ ] IRepository, IReadRepository, IUnitOfWork present
- [ ] ICommand, IQuery markers present

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#BuildingBlocks (.csproj)]]
