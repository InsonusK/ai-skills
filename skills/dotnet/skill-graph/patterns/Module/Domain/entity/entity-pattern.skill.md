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
Define rules for designing **domain entities** including identity strategies and lifecycle rules.

# Core Principles
Entity is a domain object with:
- identity
- mutable state
- encapsulated behavior
- invariant enforcement
- internal `Id` is always system primary identity

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
Entities created once and never change.
__Usage:__
- state entity
__Identity rules:__
- primary key - `int/uint/short` preferred
- external identity - not required
- row version - required
__Rules:__
- editable after creation
- Implement [[entity-concurrency-pattern.skill]]
__Example:__
```CSharp
public class Currency
{
    public int Id { get; internal set; }
    public string Code { get; internal set; }
    public int RowVersion {get; internal set; }
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
- Implement [[external-created-entity.skill]]
__Example:__
```CSharp
public class Currency
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
- Implement [[entity-concurrency-pattern.skill]]
- Implement [[external-created-entity.skill]]
__Example:__
```CSharp
public class Currency
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }
    public string Code { get; internal set; }
    public int RowVersion {get; internal set; }
}
```

# Rules
- behavior must follow [[entity-behavior.skill]]
- Identity usage rules
	- used in domain logic
	- used in persistence
	- used in relationships
	- used in internal APIs

# Check list
- [ ] Entity type defined
- [ ] All linked patterns applied and check lists passed