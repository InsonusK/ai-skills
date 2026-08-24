---
name: csproj-module-domain
description: Project {Module}.Domain in the shared-rules plateau
whenToUse: when adding or editing an entity in {Module}.Domain, or deciding whether new code belongs here
domain: skill
type: template
plateau: shared-rules
version: 20260824150000
tags:
  - skill/template/csproj
  - plateau/shared-rules
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]]"
  - "[[../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
  - "[[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Own the entities, value objects, rules, and domain events for this bounded context
- Store all entity types for this bounded context
- Own the business logic and invariant enforcement for all entities in this module
- Give every module a strict, self-validating Value Object type per domain concept that needs invariant enforcement
- Give every module a place for bulky or multi-step entity behavior that doesn't fit naturally inside the Entity itself

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

# Core Principles
- All entities live in `/{Module}.Domain/Entities`
- Domain is the only layer that contains entity definitions
- Properties with invariant state or business semantics are Value Object types, inheriting the `Soft{ValueObject}` base from `{Module}.Interfaces`
- Bulky or multi-step entity behavior is extracted to static domain services in `/{Module}.Domain/Services`
- Once a VO's or Entity's own condition is found duplicated in a second consumer (a `PropertyValidator`, a `Dto`Validator, another Entity), it redirects to a centralized `{Rule}.Check()` in `{Module}.Domain.Rules` instead of keeping its own local copy — optional, applied only on genuine observed duplication, never speculatively

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Domain
```

## Project Structure
- /{Module}.Domain
  - /Entities
    - [{Entity}.cs](./classes/plateau-shared-rules--class-entity.skill.md)
  - /ValueObjects
    - [{ValueObject}.cs](./classes/plateau-shared-rules--class-value-object.skill.md)
  - /Services
    - [{Behavior}Service.cs](./classes/plateau-shared-rules--class-behavior-service.skill.md)
  - {Module}.Domain.csproj

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Entities | All entity types for this module | [[./classes/plateau-shared-rules--class-entity.skill.md\|class-entity]] |
| /ValueObjects | Strict, self-validating Value Object types | [[./classes/plateau-shared-rules--class-value-object.skill.md\|class-value-object]] |
| /Services | Static domain service extensions for bulky entity behavior | [[./classes/plateau-shared-rules--class-behavior-service.skill.md\|class-behavior-service]] |

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
- `{Module}.Interfaces` — for the `Soft{ValueObject}` base types `{ValueObject}` inherits from
- `{Module}.Domain.Rules` — new at this plateau, only once a condition has actually been centralized
- Microsoft.EntityFrameworkCore (`IEntityTypeConfiguration` only)

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

# Rules
MUST:
- Domain depends only on Shared, `{Module}.Interfaces` (for `Soft{ValueObject}` base types), and EF Core (for `IEntityTypeConfiguration` only)
- All entities live in `/{Module}.Domain/Entities`
- Properties that carry invariant state or business semantics are `{ValueObject}` types, not primitives
- Bulky or multi-step entity behavior lives in `/{Module}.Domain/Services` as static domain service extensions
MUST NOT:
- Domain reference any other module's project
- Domain use EF Core beyond `IEntityTypeConfiguration`
- A domain service extension write to an entity property directly, bypassing the entity's own guarded methods

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

# Check list
- [ ] Domain.csproj references only `{Module}.Interfaces`, Shared, and EF Core
- [ ] No DbContext reference in any domain class
- [ ] No cross-module domain references
- [ ] `/Entities`, `/ValueObjects`, `/Services` folders exist, classes placed correctly

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
