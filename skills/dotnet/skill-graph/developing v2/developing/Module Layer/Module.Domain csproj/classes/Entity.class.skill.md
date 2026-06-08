---
uid:
name: entity-class
description: Domain object with stable identity, mutable state, encapsulated behavior, and invariant enforcement. Must remain free of all EF attributes.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/class
  - dotnet
  - domain
  - entity
triggers:
  - create entity
  - implement domain entity
  - design aggregate
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill]]"
extended_by:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill]]"
aliases:
  - Entity
---

# Goal
- Represent a domain object with stable identity, mutable state, encapsulated behavior, and invariant enforcement
- Select the correct entity type from the type matrix before implementation
- Ensure domain entity remains free of EF attributes — all persistence mapping delegated to config class

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill#Entity]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill#Entity (extended)]]

# Core Principles
- Entity has stable identity — `int Id` is always the system primary identity
- Entity has mutable state — unlike Value Objects, state changes over time
- Entity encapsulates behavior — state changes happen through methods, not direct property assignment from outside
- Entity enforces invariants — invalid state must never be reachable
- `Id` is always `internal set` — only persistence layer assigns it
- Entity type selected from matrix before coding begins
- Entity has zero EF attributes — `[Column]`, `[Index]`, `[ForeignKey]`, `[ConcurrencyCheck]` are all forbidden
- Entity does not know about its own table name, column names, or constraint names

Entity type matrix:

| | No RowVersion (Immutable) | Has RowVersion (Mutable) |
| --- | --- | --- |
| Internal (no Guid) | Internal Immutable | Internal Mutable |
| External (has Guid) | External Immutable | External Mutable |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill#Entity]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill#Entity (extended)]]

# Structure

## Place in csproj
Defined in [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/{Module}.Domain.csproj.skill]]
```
/{ModuleName}.Domain
  /Entities
    {EntityName}.cs
```

## Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity | {Noun} | Order | {Noun}.cs | Order.cs |

## Implementation

### Internal Immutable
```csharp
public class Currency
{
    public int Id { get; internal set; }
    public string Code { get; internal set; }
}
```

### Internal Mutable
```csharp
public class Order
{
    public int Id { get; internal set; }
    public string Comment { get; internal set; }
    public uint Version { get; internal set; }
}
```

### External Immutable
```csharp
public class PaymentEvent
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }
    public string Code { get; internal set; }
}
```

### External Mutable
```csharp
public class ExternalOrder
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }
    public string Title { get; internal set; }
    public uint Version { get; internal set; }
}
```

Entity must not contain any EF attributes:
```csharp
// CORRECT — no EF attributes
public class TodoTask
{
    public int Id { get; internal set; }
    public string Title { get; internal set; }
    public uint Version { get; internal set; }
}

// WRONG — EF attributes on entity
[Table("TodoTasks")]
public class TodoTask
{
    [Key]
    public int Id { get; internal set; }
    [Column("task_title")]
    public string Title { get; internal set; }
}
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill#Entity]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill#Entity (extended)]]

# Rules

MUST:
- Entity has `int Id` with `internal set`
- Entity type selected from the four-type matrix
- Mutable entities have `uint Version` with `internal set`
- External entities have `Guid Guid` with `internal set`
- All property setters are `internal` or `private`
- All entities live in /{Module}.Domain/Entities

MUST NOT:
- Use `public` setters on entity properties
- Use `Guid` as primary identity
- Place entities outside the Domain project
- Use `long` or `string` as primary key without explicit justification
- Entity have any EF attributes (`[Table]`, `[Column]`, `[Key]`, `[Index]`, `[ForeignKey]`, `[ConcurrencyCheck]`)

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill#Entity]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill#Entity (extended)]]

# Anti-patterns
- `public string Title { get; set; }` — public setter allows invalid state from any caller
- Using `Guid` as the primary key — internal `int Id` is always primary
- Skipping the type matrix — leads to missing Version on mutable or missing Guid on external
- Placing entity in Application or Interfaces project — entities belong in Domain only
- `[Column("task_title")]` on entity property — column mapping belongs in config class
- `[Table("TodoTasks")]` on entity class — table naming belongs in config class

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill#Entity]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill#Entity (extended)]]

# Check list
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present
- [ ] All property setters are `internal` or `private`
- [ ] Mutable entity has `uint Version` with `internal set`
- [ ] External entity has `Guid Guid` with `internal set`
- [ ] Entity placed in /{Module}.Domain/Entities
- [ ] No EF attributes present on entity class or any of its properties

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill#Entity]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill#Entity (extended)]]

# Unittest TestCases
- [ ] When entity created Then Id is default (0) until persisted
- [ ] When property setter called from outside domain Then compiler prevents access
- [ ] When Internal Immutable entity loaded Then no Version property present
- [ ] When Internal Mutable entity loaded Then Version property present
- [ ] When External entity created Then Guid property present and immutable after creation

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill#Entity]]
