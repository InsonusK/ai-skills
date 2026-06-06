---
uid: a6e0c3fa-3059-4e75-b569-bb15f5183f00
name: entity
description: rules for designing domain entities — identity strategies, type selection, and which linked patterns apply
domain: skill
type: pattern
tags:
  - dotnet
  - domain
  - ddd
  - entity
  - identity
  - skill/pattern/class
triggers:
  - design entity
  - create domain entity
  - entity identity strategy
aliases:
  - Entity
---
# Goal
Define how to declare a domain entity, select its identity strategy, and determine which companion patterns are required. An entity is a domain object with stable identity and mutable or immutable state. Selecting the wrong type leads to missing concurrency control, leaked Guids, or invalid persistence configuration.

# Core Principles
- Internal `int Id` is always the system primary identity — never Guid
- Guid appears only on externally created entities — as a correlation handle, never domain logic
- Mutable entities require a `Version` field for concurrency control
- Entity behavior and invariant enforcement are defined separately in entity-behavior.skill

# Place in csproj
Defined in [[skills/dotnet/skill-graph/developing/Module/Domain csproj/module-domain-csproj.skill#Structure|module-domain-csproj.skill]]
```
/{ModuleName}.Domain
	/Entities
		Currency.cs
		Order.cs
	{ModuleName}.Domain.csproj
```
# Entity Type Matrix

| Created by \ Editable | Immutable (no Version)  | Mutable (has Version) |
| --------------------- | ----------------------- | --------------------- |
| Internal system       | [[#Internal Immutable]] | [[#Internal Mutable]] |
| External system       | [[#External Mutable]]   | [[#External Mutable]] |

## Internal Immutable
System dictionaries, predefined types, configuration tables. Created once, never changed.
__Rules:__
- internal creation
- immutable after creation
```csharp
public class Currency
{
    public int Id { get; internal set; }
    public string Code { get; internal set; }
}
```

## Internal Mutable
State entities created and edited by the system. Requires `Version` — see entity-concurrency.skill.
__Rules:__
- internal creation
- mutable after creation
__Use additional skills__:
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/entity-concurrency-pattern.skill|entity-concurrency-pattern.skill]]
```csharp
public class Order
{
    public int Id { get; internal set; }
    public string Comment { get; internal set; }
    public uint Version { get; internal set; }
}
```

## External Immutable
Business event registrations created once by an external system. Requires `Guid` — see external-created-entity.skill.
__Rules:__
- external creation
- immutable after creation
__Use additional skills__:
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/external-created-entity.skill|external-created-entity.skill]]
```csharp
public class PaymentEvent
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }
    public string Code { get; internal set; }
}
```

## External Mutable
Entities created by external system and editable after creation. Requires both `Guid` and `Version`.
__Rules:__
- external creation
- mutable after creation
__Use additional skills__:
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/entity-concurrency-pattern.skill|entity-concurrency-pattern.skill]]
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/external-created-entity.skill|external-created-entity.skill]]
```csharp
public class ExternalOrder
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }
    public string Title { get; internal set; }
    public uint Version { get; internal set; }
}
```

# Rules
MUST:
- Primary key is `int Id` with `internal set`
- Mutable entities implement [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/entity-concurrency-pattern.skill|entity-concurrency-pattern.skill]]
- Externally created entities implement [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/external-created-entity.skill|external-created-entity.skill]]
- All setters `internal set` — no public mutation outside domain
- Entity behavior follows [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/entity-behavior.skill|entity-behavior.skill]]
MUST NOT:
- Use Guid as primary key or in domain logic
- Expose public setters
- Reference infrastructure or application layer

# Checklist
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present
- [ ] `Version` added if mutable
- [ ] `Guid` added if externally created
- [ ] entity-behavior.skill applied
- [ ] entity-concurrency.skill applied if mutable
- [ ] external-created-entity.skill applied if external
- [ ] EF configuration exists in `/Configurations`

# Unittest TestCases
- [ ] When entity created Then Id is default until persisted
- [ ] When mutable entity updated Then Version changes

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/entity-behavior.skill|entity-behavior.skill]] — how entity enforces invariants and exposes behavior
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/entity-concurrency-pattern.skill|entity-concurrency-pattern.skill]] — Version field and EF concurrency token for mutable entities
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/external-created-entity.skill|external-created-entity.skill]] — Guid field and unique index for externally created entities
- [[files/ef-configuration.skill|ef-configuration.skill]] — persistence mapping for every entity
- [[files/domain-event.skill|domain-event.skill]] — entities raise and collect domain events
