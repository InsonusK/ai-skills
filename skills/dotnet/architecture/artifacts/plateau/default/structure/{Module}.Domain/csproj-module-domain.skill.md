---
name: csproj-module-domain
description: Own the entities, value objects, rules, and domain events for this bounded context
domain: skill
type: template
version: 20260629223200
plateau: default
tags:
  - skill/template/csproj
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]"
---

# Goal
- Store all Value Object types for this bounded context
- Store all domain rule types for this bounded context
- Keep domain logic organized and discoverable
- Own the entities, value objects, rules, and domain events for this bounded context
- Store all entity types for this bounded context
- Own the business logic and invariant enforcement for all entities in this module
- Add `Guid` as an immutable property on externally-created entities
- Add unique database index on `Guid` as the final idempotency guard
- Add `Version` concurrency token to every mutable entity, implement `IVersioned`, and configure it as the PostgreSQL `xmin` system column; declare `VersionedEntityName` in the config class
- Add creation and update timestamp properties to user-initiated entities based on classification, using `DateTimeOffset` and explicit interface implementation for mutable setters
- Store all EF Core entity type configuration classes for this bounded context
- Own all persistence mapping concerns for this module's entities
- Own all entity behavior and invariant enforcement for the bounded context
- Provide a place to extract bulky entity logic without scattering mutation points
- Keep entities small and focused on single-responsibility state transitions
- Reference `{Module}.Interfaces` so Domain Value Objects can inherit from `Soft{ValueObject}` base types

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]

# Core Principles
- Value Objects define correctness — they encode domain semantics and enforce invariants at construction time
- Rules define predicates — they encode reusable business conditions without deciding enforcement
- Entities define consistency — they decide when and how to enforce invariants using VOs and rules
- When a value object shape is shared across modules, the Domain VO inherits from `Soft{ValueObject}` declared in `{Module}.Interfaces`
- Domain is the innermost layer — pure business logic, no infrastructure dependencies
- Domain has no knowledge of other modules
- All entities live in /{Module}.Domain/Entities
- Domain is the only layer that contains entity definitions
- `Guid` declared with `internal set` — set once during entity creation factory method, never changed
- Entity creation factory method receives `Guid` as a parameter
- No domain method ever reads `Guid` after creation — only the resolver and the entity factory use it
- `xmin` is a PostgreSQL system column — automatically incremented on every row update
- `IsConcurrencyToken()` tells EF to include `Version` in `WHERE` clause on `UPDATE`
- `ValueGeneratedOnAddOrUpdate()` tells EF the value comes from the database — never from application code
- One config class per entity — lives in /{Module}.Domain/Configurations
- Config class is the only place that defines column names, index names, and constraints for this entity
- Domain entities have no EF attributes — all mapping is in the config class
- Timestamp columns are required because they are always assigned before the row is persisted
- `Internal Immutable` entities have no timestamp interfaces or columns
- `External Immutable` entities implement `ICreationInfoModel` and map only creation timestamps
- `Internal Mutable` and `External Mutable` entities implement `ICreationInfoModel` and `IUpdateInfoModel`, and map both creation and update timestamps
- Class-level timestamp properties use `internal set`; mutable interface setters are implemented explicitly
- `DateTimeOffset` maps to PostgreSQL `timestamp with time zone` by default
- Timestamp columns are required because they are always assigned before the row is persisted
- `Internal Immutable` entities have no timestamp interfaces or columns
- `External Immutable` entities implement `ICreationInfoModel` and map only creation timestamps
- `Internal Mutable` and `External Mutable` entities implement `ICreationInfoModel` and `IUpdateInfoModel`, and map both creation and update timestamps
- Class-level timestamp properties use `internal set`; mutable interface setters are implemented explicitly
- `DateTimeOffset` maps to PostgreSQL `timestamp with time zone` by default
- Timestamp columns are required because they are always assigned before the row is persisted
- `Internal Immutable` entities have no timestamp interfaces or columns
- `External Immutable` entities implement `ICreationInfoModel` and map only creation timestamps
- `Internal Mutable` and `External Mutable` entities implement `ICreationInfoModel` and `IUpdateInfoModel`, and map both creation and update timestamps
- Class-level timestamp properties use `internal set`; mutable interface setters are implemented explicitly
- `DateTimeOffset` maps to PostgreSQL `timestamp with time zone` by default
- Entity methods are the primary gatekeepers of state change
- Domain rules encode reusable predicates; entities decide when and how to enforce them
- Static domain service extension methods hold complex or multi-step logic that does not fit naturally inside the entity
- A single property must not be mutated from many independent public entry points

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Domain
```


## Project Structure
- /{Module}.Domain
  - /Entities
    - [{EntityName}.cs](skills/dotnet/architecture/artifacts/plateau/default/structure/{Module}.Domain/classes/class-entity.skill.md)
  - /ValueObjects
    - [Age.cs](skills/dotnet/architecture/artifacts/plateau/default/structure/{Module}.Domain/classes/class-value-object.skill.md)
    - [Money.cs](skills/dotnet/architecture/artifacts/plateau/default/structure/{Module}.Domain/classes/class-value-object.skill.md)
    - [Email.cs](skills/dotnet/architecture/artifacts/plateau/default/structure/{Module}.Domain/classes/class-value-object.skill.md)
  - /Rules
    - [IntRules.cs](skills/dotnet/architecture/artifacts/plateau/default/structure/{Module}.Domain/classes/class-rule.skill.md)
    - [StringRules.cs](skills/dotnet/architecture/artifacts/plateau/default/structure/{Module}.Domain/classes/class-rule.skill.md)
    - [AgeRules.cs](skills/dotnet/architecture/artifacts/plateau/default/structure/{Module}.Domain/classes/class-rule.skill.md)
    - [CanDriveCarRule.cs](skills/dotnet/architecture/artifacts/plateau/default/structure/{Module}.Domain/classes/class-rule.skill.md)
  - /Services
    - [{Behavior}Service.cs](skills/dotnet/architecture/artifacts/plateau/default/structure/{Module}.Domain/classes/class-behavior-service.skill.md)
  - /Events
    - {DomainEvent}.cs
  - /Configurations
    - [{EntityName}Config.cs](skills/dotnet/architecture/artifacts/plateau/default/structure/{Module}.Domain/classes/class-entity-config.skill.md)
  - {Module}.Domain.csproj

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]

## Classification variants

Apply entity classification at the domain layer by choosing the correct entity class and configuration variant for each `{EntityName}`.

- Extend `{EntityName}.cs` with the properties and marker interfaces required by the selected classification.
- Extend `{EntityName}Config.cs` with the EF Core mappings required by the selected classification.
- Do not add `Version`, `Guid`, or their configurations unless the classification explicitly requires them.

| Classification | `{EntityName}.cs` changes | `{EntityName}Config.cs` changes |
|---|---|---|
| **Internal Immutable** | Only `int Id` | Standard Id mapping; no timestamps |
| **External Immutable** | Add `Guid Guid { get; internal set; }`; implement `ICreationInfoModel` | Add unique index on `Guid`; map creation timestamps as required |
| **Internal Mutable** | Add `uint Version { get; internal set; }` and `IVersioned`; implement `ICreationInfoModel` and `IUpdateInfoModel` | Map `Version` to `xmin` with `IsConcurrencyToken()`; map creation and update timestamps as required |
| **External Mutable** | Add both `Guid` and `Version` + `IVersioned`; implement `ICreationInfoModel` and `IUpdateInfoModel` | Add unique index on `Guid` and map `Version` to `xmin`; map creation and update timestamps as required |

- Internal Immutable and Internal Mutable entities do not implement `solution-external-created-entity.skill`.
- External Immutable and External Mutable entities implement `solution-external-created-entity.skill`.
- Internal Mutable and External Mutable entities implement `solution-entity-concurrency-change.skill`.
- Internal Immutable entities implement neither dependency solution nor `solution-entity-edit-timestamp.skill`.
- External Immutable entities implement `solution-entity-edit-timestamp.skill` with creation timestamps only.
- Internal Mutable and External Mutable entities implement `solution-entity-edit-timestamp.skill` with creation and update timestamps.

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]

## Directory and class skills
| `Directory                            | file`                                                                                                                                  | Description                                                                                                                                    | Pattern skill |
| ------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- | ------------- |
| /ValueObjects                         | All Value Object types for this module; VOs exposed to other modules inherit from `Soft{ValueObject}` defined in `{Module}.Interfaces` |                                                                                                                                                |               |
| /Rules                                | All domain rule static classes for this module                                                                                         |                                                                                                                                                |               |
| /Entities                             | Domain entities that use Value Objects and rules                                                                                       |                                                                                                                                                |               |
| /Entities                             | All entity types for this module                                                                                                       |                                                                                                                                                |               |
| /Entities/{EntityName}.cs             | External-created entity with Guid property                                                                                             | [[skills/dotnet/architecture/artifacts/plateau/default/structure/{Module}.Domain/classes/class-entity.skill\|class-Entity.skill]]              |               |
| /Configurations/{EntityName}Config.cs | Unique index on Guid configuration                                                                                                     | [[skills/dotnet/architecture/artifacts/plateau/default/structure/{Module}.Domain/classes/class-entity-config.skill\|class-EntityConfig.skill]] |               |
| /Entities/{EntityName}.cs             | Mutable entity with uint Version property implementing IVersioned                                                                      | [[skills/dotnet/architecture/artifacts/plateau/default/structure/{Module}.Domain/classes/class-entity.skill\|class-Entity.skill]]              |               |
| /Configurations/{EntityName}Config.cs | EF configuration mapping Version to xmin and declaring VersionedEntityName                                                             | [[skills/dotnet/architecture/artifacts/plateau/default/structure/{Module}.Domain/classes/class-entity-config.skill\|class-EntityConfig.skill]] |               |
| /Configurations                       | One EF config class per entity                                                                                                         |                                                                                                                                                |               |
| /Entities                             | All entity types for this module                                                                                                       |                                                                                                                                                |               |
| /Rules                                | All domain rule static classes for this module                                                                                         |                                                                                                                                                |               |
| /Services                             | Static domain service extension methods for bulky entity behavior                                                                      |                                                                                                                                                |               |
| /Events                               | Domain events raised by this module                                                                                                    |                                                                                                                                                |               |
| /Configurations                       | One EF config class per entity                                                                                                         |                                                                                                                                                |               |

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| Microsoft.EntityFrameworkCore | * | IEntityTypeConfiguration only |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides `IsConcurrencyToken()`, `ValueGeneratedOnAddOrUpdate()`, `HasColumnName()` |

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]

## What Does NOT Belong Here
- Infrastructure implementations — belong to App.Infrastructure or BuildingBlocks
- Application orchestration — belong to {Module}.Application
- Cross-module reusable VOs/rules — belong in Shared when used by 2+ modules
- EF Core configuration classes — belong in {Module}.Domain/Configurations per [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration.skill]]
- Business logic orchestration — belongs to Application
- Infrastructure implementations — belongs to App.Infrastructure
- Cross-module queries — belongs to App.Queries
- Intra-module entity configurations — belong in respective `{Module}.Domain/Configurations`
- Domain entities — belong in `{Module}.Domain/Entities`
- Value Object definitions — belong in `{Module}.Domain/ValueObjects`
- Cross-module foreign key configurations — belong in App.Infrastructure/Persistence/Configurations
- Transport validation — belongs to module Application validators
- Pipeline behaviors — belongs to BuildingBlocks
- Command/Query handlers — belong to module Application
- Cross-module workflow orchestration — belongs in Application

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]

## Allowed Dependencies
- Shared
- `{Module}.Interfaces` (for `Soft{ValueObject}` base types)
- Microsoft.EntityFrameworkCore (for multi-property VO `OwnsOne` configuration only)
- Microsoft.EntityFrameworkCore (IEntityTypeConfiguration only)
- EF Core configuration packages
- Microsoft.EntityFrameworkCore (`IEntityTypeConfiguration` only)

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]

# Rules
MUST:
	- All Value Objects live in `/{Module}.Domain/ValueObjects`
	- All domain rules live in `/{Module}.Domain/Rules`
	- Only module-specific VOs and rules live here — cross-cutting ones belong in Shared
	- Domain depends only on Shared, its own `{Module}.Interfaces` (for `Soft{ValueObject}` base types), and EF Core (for IEntityTypeConfiguration only)
	- Every Domain Value Object exposed to other modules inherits from `Soft{ValueObject}`
	- All entities live in /{Module}.Domain/Entities
	- `Guid` declared as `public Guid Guid { get; internal set; }`
	- Set exactly once in the entity factory method — never reassigned
	- Present on External Immutable and External Mutable entity types only
	- Every mutable entity has a `Version` property
	- Every mutable entity implements `IVersioned`
	- Every mutable entity config class declares `VersionedEntityName`
	- `Version` configured as `IsConcurrencyToken()` mapping to `xmin` in EF configuration
	- Timestamp properties are `DateTimeOffset` with `internal set`
	- Mutable entities implement `ICreationInfoModel` and `IUpdateInfoModel` with explicit interface setters
	- External Immutable entities implement `ICreationInfoModel` only
	- Timestamp properties are mapped as required in the entity configuration
	- Internal Immutable entities have no timestamp interfaces or columns
	- Timestamp properties are `DateTimeOffset` with `internal set`
	- Mutable entities implement `ICreationInfoModel` and `IUpdateInfoModel` with explicit interface setters
	- External Immutable entities implement `ICreationInfoModel` only
	- Timestamp properties are mapped as required in the entity configuration
	- Internal Immutable entities have no timestamp interfaces or columns
	- Timestamp properties are `DateTimeOffset` with `internal set`
	- Mutable entities implement `ICreationInfoModel` and `IUpdateInfoModel` with explicit interface setters
	- External Immutable entities implement `ICreationInfoModel` only
	- Timestamp properties are mapped as required in the entity configuration
	- Internal Immutable entities have no timestamp interfaces or columns
	- All EF configuration classes live in /{Module}.Domain/Configurations
	- One config class per entity — no shared configs
	- All configurations registered via `ApplyConfigurationsFromAssembly` in DbContext
	- Every property mutation validates state through domain rules before assigning
	- Entity methods throw `DomainException` when a rule returns `false`
	- Static service extension methods live in `{Module}.Domain/Services`
	- Static service extension methods use existing domain rules from `{Module}.Domain/Rules`
	- A single entity property must not have multiple uncoordinated public mutation points
SHOULD:
	- Prefer thin entity methods that delegate rule checks and then call a single setter
	- Name service files after the behavior they encapsulate, e.g. `OrderPricingService.cs`
MUST NOT:
	- Place Value Object definitions outside /ValueObjects folder
	- Place rule definitions outside /Rules folder
	- Put module-specific VO or rule in Shared
	- Domain reference any other module's project
	- Domain Value Object not inherit from `Soft{ValueObject}` when the shape is shared across modules
	- Use FluentValidation types directly in Domain
	- Domain use EF Core beyond IEntityTypeConfiguration
	- `Guid` used in domain logic, domain events, or as a foreign key in relationships
	- `Guid` reassigned after entity creation
	- Internal entity types (no external creation) have `Guid`
	- Application code set or read `Version` for any purpose other than concurrency checking — it is a database concern
	- Timestamp interfaces added to `Internal Immutable` entities
	- Update timestamps added to `External Immutable` entities
	- Timestamp properties use `public set` or are mutable from outside the domain
	- `DateTime` used instead of `DateTimeOffset` for timestamps
	- Timestamp interfaces added to `Internal Immutable` entities
	- Update timestamps added to `External Immutable` entities
	- Timestamp properties use `public set` or are mutable from outside the domain
	- `DateTime` used instead of `DateTimeOffset` for timestamps
	- Timestamp interfaces added to `Internal Immutable` entities
	- Update timestamps added to `External Immutable` entities
	- Timestamp properties use `public set` or are mutable from outside the domain
	- `DateTime` used instead of `DateTimeOffset` for timestamps
	- Place EF config classes outside /Configurations folder
	- Use EF data annotations on domain entity classes
	- Put mapping logic directly in `DbContext.OnModelCreating`
	- Configure cross-module foreign keys here
	- Duplicate invariant logic across setters, entity methods, or service extensions
	- Mutate entity state in a service extension without going through the entity's own guarded method or setter
	- Allow public setters that bypass rule validation
	- Let a service extension introduce a second independent mutation point for the same property

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]

# Anti-patterns
- Scattering VO or rule classes across arbitrary folders in Domain
- Putting cross-module VO in {Module}.Domain instead of Shared
- Duplicating `Soft{ValueObject}` shape in Domain instead of inheriting
- Keeping duplicated copies of the same VO/rule in multiple module Domain projects
- Injecting DbContext into a domain class — domain has no persistence dependency
- Referencing another module's Domain for shared entity types — each module owns its own entities
- Using EF Core attributes on domain entities — use configuration classes instead
- Placing entities outside /Entities folder — breaks navigation and discoverability
- Defining entities in Application or Interfaces — entities belong in Domain only
- `Guid` with `public set` — application code must never modify it
- `Guid` used in domain method logic — it is a correlation handle only
- Inconsistent timestamp mappings across entities
- Nullable timestamp columns
- Using `DateTime` instead of `DateTimeOffset`
- Inconsistent timestamp mappings across entities
- Nullable timestamp columns
- Using `DateTime` instead of `DateTimeOffset`
- Inconsistent timestamp mappings across entities
- Nullable timestamp columns
- Using `DateTime` instead of `DateTimeOffset`
- `HasDefaultValue` or `HasComputedColumnSql` used on `Version` — `xmin` is managed entirely by PostgreSQL
- Annotating domain entity with `[Column]`, `[Index]`, `[ForeignKey]` — all mapping belongs in config class
- Registering configs manually one by one in DbContext — use `ApplyConfigurationsFromAssembly`
- Sharing one config class across multiple entity types
- Placing cross-module FK config in Domain — belongs in App.Infrastructure
- Entity has several points changing the same property with separate validation
- Service extension bypasses entity methods and writes to `internal set` properties directly
- Property mutated from both the entity and multiple service extensions
- Inline rule logic inside entity methods instead of calling rules from `{Module}.Domain/Rules`

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]

# Check list
- [ ] /ValueObjects folder exists in {Module}.Domain
- [ ] /Rules folder exists in {Module}.Domain
- [ ] `{Module}.Domain.csproj` references `{Module}.Interfaces.csproj`
- [ ] Every Domain Value Object exposed to other modules inherits from `Soft{ValueObject}`
- [ ] All VOs are in /ValueObjects
- [ ] All rules are in /Rules
- [ ] No cross-module VO/rule duplicated here when it already exists in Shared
- [ ] Domain.csproj references only EF Core
- [ ] No DbContext reference in any domain class
- [ ] No cross-module domain references
- [ ] /Entities folder exists in {Module}.Domain
- [ ] All entity classes placed in /Entities
- [ ] `Guid Guid { get; internal set; }` on every external-created entity
- [ ] `Guid` set in entity factory method — never reassigned
- [ ] `uint Version { get; internal set; }` present on every mutable entity
- [ ] Every mutable entity implements `IVersioned`
- [ ] Every mutable entity config class declares `VersionedEntityName`
- [ ] `Version` mapped to `xmin` with `IsConcurrencyToken()` and `ValueGeneratedOnAddOrUpdate()` in entity configuration
- [ ] `Internal Immutable` entities have no timestamp interfaces or columns
- [ ] `External Immutable` entities implement `ICreationInfoModel` and map creation timestamps only
- [ ] `Internal Mutable` and `External Mutable` entities implement both timestamp interfaces and map creation and update timestamps
- [ ] Timestamp properties use `DateTimeOffset` with `internal set`
- [ ] Mutable interface setters are implemented explicitly
- [ ] Timestamp columns are mapped as required in entity configuration
- [ ] /Configurations folder exists in {Module}.Domain
- [ ] One config class per entity
- [ ] No EF attributes on any entity class in this module
- [ ] Configurations registered via `ApplyConfigurationsFromAssembly`
- [ ] Every entity state change is validated by domain rules
- [ ] `DomainException` thrown when a rule returns `false`
- [ ] Bulky logic extracted to static extension methods in `{Module}.Domain/Services`
- [ ] No property has multiple uncoordinated mutation points
- [ ] Service extensions do not duplicate rule logic
- [ ] Service extensions mutate state only through entity methods or guarded setters

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]]

