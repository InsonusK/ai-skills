---
uid: b990cd13-23ea-424d-8bf7-a4886e44575e
name: module-domain-csproj
description: Own the entities, value objects, rules, and domain events for this bounded context
domain: skill
type: template
version: 20260616
tags:
  - skill/template/csproj
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour.solution.skill]]"
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
- Store all EF Core entity type configuration classes for this bounded context
- Own all persistence mapping concerns for this module's entities
- Own all entity behavior and invariant enforcement for the bounded context
- Provide a place to extract bulky entity logic without scattering mutation points
- Keep entities small and focused on single-responsibility state transitions

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

# Core Principals
- Value Objects define correctness — they encode domain semantics and enforce invariants at construction time
- Rules define predicates — they encode reusable business conditions without deciding enforcement
- Entities define consistency — they decide when and how to enforce invariants using VOs and rules
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
- Entity methods are the primary gatekeepers of state change
- Domain rules encode reusable predicates; entities decide when and how to enforce them
- Static domain service extension methods hold complex or multi-step logic that does not fit naturally inside the entity
- A single property must not be mutated from many independent public entry points

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Domain
```


## Project Structure
```
/{Module}.Domain
  /ValueObjects
    Age.cs
    Money.cs
    Email.cs
  /Rules
    IntRules.cs
    StringRules.cs
    AgeRules.cs
    CanDriveCarRule.cs
  /Entities
    Order.cs
    Driver.cs
```

```
/{Module}.Domain
  /Entities
    InternalImmutableEntity.cs
    InternalMutableEntity.cs
    ExternalImmutableEntity.cs
    ExternalMutableEntity.cs
```

```
/{Module}.Domain
  /Entities
    {EntityName}.cs
  /Configurations
    {EntityName}Config.cs
```

```
/{Module}.Domain
  /Entities
    {EntityName}.cs          ← extended with Version property
  /Configurations
    {EntityName}Config.cs    ← extended with Version mapping
```

```
/{Module}.Domain
  /Configurations
    TodoTaskConfig.cs
    OrderConfig.cs
```

```
/{ModuleName}.Domain
  /Entities
  /ValueObjects
  /Rules
  /Services
  /Events
  /Configurations
  {ModuleName}.Domain.csproj
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

## Directory and class skills
| `Directory|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
| /ValueObjects | All Value Object types for this module |  |
| /Rules | All domain rule static classes for this module |  |
| /Entities | Domain entities that use Value Objects and rules |  |
| /Entities | All entity types for this module |  |
| /Entities/{EntityName}.cs | External-created entity with Guid property | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Domain/classes/Entity.class.skill.md|Entity.class.skill]] |
| /Configurations/{EntityName}Config.cs | Unique index on Guid configuration | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Domain/classes/EntityConfig.class.skill.md|EntityConfig.class.skill]] |
| /Entities/{EntityName}.cs | Mutable entity with uint Version property implementing IVersioned | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Domain/classes/Entity.class.skill.md|Entity.class.skill]] |
| /Configurations/{EntityName}Config.cs | EF configuration mapping Version to xmin and declaring VersionedEntityName | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Domain/classes/EntityConfig.class.skill.md|EntityConfig.class.skill]] |
| /Configurations | One EF config class per entity |  |
| /Entities | All entity types for this module |  |
| /ValueObjects | All Value Object types for this module |  |
| /Rules | All domain rule static classes for this module |  |
| /Services | Static domain service extension methods for bulky entity behavior |  |
| /Events | Domain events raised by this module |  |
| /Configurations | One EF config class per entity |  |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| Microsoft.EntityFrameworkCore | * | IEntityTypeConfiguration only |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides `IsConcurrencyToken()`, `ValueGeneratedOnAddOrUpdate()`, `HasColumnName()` |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

## What Does NOT Belong Here
- Infrastructure implementations — belong to App.Infrastructure or BuildingBlocks
- Application orchestration — belong to {Module}.Application
- Cross-module reusable VOs/rules — belong in Shared when used by 2+ modules
- EF Core configuration classes — belong in {Module}.Domain/Configurations per [[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration.solution.skill]]
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
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

## Allowed Dependencies
- Shared
- Microsoft.EntityFrameworkCore (for multi-property VO `OwnsOne` configuration only)
- Microsoft.EntityFrameworkCore (IEntityTypeConfiguration only)
- EF Core configuration packages
- Microsoft.EntityFrameworkCore (`IEntityTypeConfiguration` only)

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

# Rules
MUST:
	- All Value Objects live in `/{Module}.Domain/ValueObjects`
	- All domain rules live in `/{Module}.Domain/Rules`
	- Only module-specific VOs and rules live here — cross-cutting ones belong in Shared
	- Domain depends only on Shared and EF Core (for IEntityTypeConfiguration only)
	- All entities live in /{Module}.Domain/Entities
	- `Guid` declared as `public Guid Guid { get; internal set; }`
	- Set exactly once in the entity factory method — never reassigned
	- Present on External Immutable and External Mutable entity types only
	- Every mutable entity has a `Version` property
	- Every mutable entity implements `IVersioned`
	- Every mutable entity config class declares `VersionedEntityName`
	- `Version` configured as `IsConcurrencyToken()` mapping to `xmin` in EF configuration
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
	- Domain use EF Core beyond IEntityTypeConfiguration
	- `Guid` used in domain logic, domain events, or as a foreign key in relationships
	- `Guid` reassigned after entity creation
	- Internal entity types (no external creation) have `Guid`
	- Application code set or read `Version` for any purpose other than concurrency checking — it is a database concern
	- Place EF config classes outside /Configurations folder
	- Use EF data annotations on domain entity classes
	- Put mapping logic directly in `DbContext.OnModelCreating`
	- Configure cross-module foreign keys here
	- Duplicate invariant logic across setters, entity methods, or service extensions
	- Mutate entity state in a service extension without going through the entity's own guarded method or setter
	- Allow public setters that bypass rule validation
	- Let a service extension introduce a second independent mutation point for the same property

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

# Anti-patterns
- Scattering VO or rule classes across arbitrary folders in Domain
- Putting cross-module VO in {Module}.Domain instead of Shared
- Keeping duplicated copies of the same VO/rule in multiple module Domain projects
- Injecting DbContext into a domain class — domain has no persistence dependency
- Referencing another module's Domain for shared entity types — each module owns its own entities
- Using EF Core attributes on domain entities — use configuration classes instead
- Placing entities outside /Entities folder — breaks navigation and discoverability
- Defining entities in Application or Interfaces — entities belong in Domain only
- `Guid` with `public set` — application code must never modify it
- `Guid` used in domain method logic — it is a correlation handle only
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
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

# Check list
- [ ] /ValueObjects folder exists in {Module}.Domain
- [ ] /Rules folder exists in {Module}.Domain
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
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
