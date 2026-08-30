---
name: plateau-stateless-non-interactive-service--csproj-module-domain
description: Project {Module}.Domain in the stateless-non-interactive-service plateau
whenToUse: when adding or editing an entity in {Module}.Domain, or deciding whether new code belongs here
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
- Own the entities, value objects, rules, and domain events for this bounded context
- Store all entity types for this bounded context
- Own the business logic and invariant enforcement for all entities in this module

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]

# Core Principles
- All entities live in `/{Module}.Domain/Entities`
- Domain is the only layer that contains entity definitions

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Domain
```

## Project Structure
- /{Module}.Domain
  - /Entities
    - [{Entity}.cs](./classes/plateau-stateless-non-interactive-service--class-entity.skill.md)
  - {Module}.Domain.csproj

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Entities | All entity types for this module | [[./classes/plateau-stateless-non-interactive-service--class-entity.skill.md\|class-entity]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Microsoft.EntityFrameworkCore` | * | `IEntityTypeConfiguration` only |

## What Does NOT Belong Here
- Business logic orchestration — belongs to Application
- Infrastructure implementations — belongs to App.Infrastructure
- Cross-module queries — belongs to App.Queries

## Allowed Dependencies
- Shared
- Microsoft.EntityFrameworkCore (`IEntityTypeConfiguration` only)

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]

# Rules
MUST:
- Domain depends only on Shared and EF Core (for `IEntityTypeConfiguration` only)
- All entities live in `/{Module}.Domain/Entities`
MUST NOT:
- Domain reference any other module's project
- Domain use EF Core beyond `IEntityTypeConfiguration`

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]

# Check list
- [ ] Domain.csproj references only EF Core
- [ ] No DbContext reference in any domain class
- [ ] No cross-module domain references
- [ ] `/Entities` folder exists, all entity classes placed there

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
