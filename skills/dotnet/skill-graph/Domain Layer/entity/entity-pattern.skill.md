---
uid: dcb71200-682c-4a66-b1ca-f0b227a8a387
status: draft
name: entity-pattern
description: rules for designing domain entities including identity strategies and invariants
domain: skill
type: pattern
tags:
  - entity
  - ddd
  - identity
  - concurrency
  - rowversion
triggers:
  - entity design
  - aggregate design
  - identity strategy
  - concurrency control
aliases:
  - Entity
  - Entities
---
# Goal
An Entity is a domain object with stable identity and mutable state over time. Unlike Value Objects, identity — not value — determines equality. This skill defines identity strategies, mutability rules, and which linked patterns apply based on entity type. Choosing the wrong type leads to missing concurrency control, leaked GUIDs in domain logic, or invalid persistence configuration.

# Core Principles
Entity is a domain object with:
- identity
- mutable state
- encapsulated behavior
- invariant enforcement
- internal `Id` is always system primary identity
## Project structure
```
/Domain
	/Entities
		Currency.cs
		Order.cs
```
# Entity types

| Create Source\IsEditable             | IsConstant<br>(No RowVersion)  | IsEditable<br>(Has RowVersion)   |
| ------------------------------------ | ------------------------------ | -------------------------------- |
| Create by Master system (No Guid)    | [[#Internal Immutable Entity]] | [[#### Internal Mutable Entity]] |
| Create by External system (Has Guid) | [[#External Immutable Entity]] | [[#External Mutable Entity]]     |

## Internal Immutable Entity
Entities created once and never change.
__Usage:__
- system dictionaries
- predefined types
- configuration tables
__Identity rules:__
- primary key - `int/uint/short` preferred
- external identity - not required
- row version - not required
__Rules:__
- immutable after creation
- no business mutation
- no lifecycle transitions
__Example:__
```CSharp
public class Currency
{
    public int Id { get; internal set; }
    public string Code { get; internal set; }
}
```

## Internal Mutable Entity
Entities created internally and can be modified after creation.
__Usage:__
- state entity
__Identity rules:__
- primary key - `int/uint/short` preferred
- external identity - not required
- row version - required
__Rules:__
- editable after creation
- Implement [[skills/dotnet/skill-graph/Domain Layer/entity/entity-concurrency-pattern.skill|entity-concurrency-pattern.skill]]
__Example:__
```CSharp
public class Order
{
    public int Id { get; internal set; }
    public string Comment { get; internal set; }
    public uint RowVersion {get; internal set; }
}
```

## External Immutable Entity
Entities created once by external system and never change.
__Usage:__
- business event registration
__Identity rules:__
- primary key - `int/uint/short` preferred
- external identity - required
- row version - not required
__Rules:__
- immutable after creation
- no business mutation
- no lifecycle transitions
- Implement [[skills/dotnet/skill-graph/Domain Layer/entity/external-created-entity.skill|external-created-entity.skill]]
__Example:__
```CSharp
public class PaymentEvent
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }
    public string Code { get; internal set; }
}
```

## External Mutable Entity
Entities created once by external system and never change.
__Usage:__
- business event registration
__Identity rules:__
- primary key - `int/uint/short` preferred
- external identity - required
- row version - required
__Rules:__
- editable by any system after creation
- system uses `Id` internally
- Implement [[skills/dotnet/skill-graph/Domain Layer/entity/entity-concurrency-pattern.skill|entity-concurrency-pattern.skill]]
- Implement [[skills/dotnet/skill-graph/Domain Layer/entity/external-created-entity.skill|external-created-entity.skill]]
__Example:__
```CSharp
public class ExternalOrder
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }
    public string Title { get; internal set; }
    public uint RowVersion {get; internal set; }
}
```

# Rules
- behavior must follow [[skills/dotnet/skill-graph/Domain Layer/entity/entity-behavior.skill|entity-behavior.skill]]
- Identity usage rules
	- used in domain logic
	- used in persistence
	- used in relationships
	- used in internal APIs
- Entities owning multi-property Value Objects must configure OwnsOne in EF mapping — see [[skills/dotnet/skill-graph/Domain Layer/domain-configuration-pattern.skill|domain-configuration-pattern.skill]]

# Check list
- [ ] Entity type selected from the type matrix
- [ ] Identity strategy matches type (int Id / Guid presence)
- [ ] [[skills/dotnet/skill-graph/Domain Layer/entity/entity-behavior.skill|entity-behavior.skill]] checklist passed
- [ ] [[skills/dotnet/skill-graph/Domain Layer/entity/entity-concurrency-pattern.skill|entity-concurrency-pattern.skill]] applied if mutable
- [ ] [[skills/dotnet/skill-graph/Domain Layer/entity/external-created-entity.skill|external-created-entity.skill]] applied if external
- [ ] OwnsOne configured for any multi-property VO properties

# Relations
- [[skills/dotnet/skill-graph/Domain Layer/entity/entity-concurrency-pattern.skill|entity-concurrency-pattern.skill]] - define implementation for mutable entities
- [[skills/dotnet/skill-graph/Domain Layer/entity/external-created-entity.skill|external-created-entity.skill]] - define implementation of external id for external created entities
- [[skills/dotnet/skill-graph/Domain Layer/entity/entity-behavior.skill|entity-behavior.skill]] - define implementation of entity befavior
- [[skills/dotnet/skill-graph/Domain Layer/domain-configuration-pattern.skill|domain-configuration-pattern.skill]] - define implementation of Entity Framework configuration