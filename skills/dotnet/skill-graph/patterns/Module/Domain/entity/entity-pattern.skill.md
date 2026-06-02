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

# 2. Identity types

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
- primary key: `int/uint/short` preferred
- no external identity required
- no row version 
__Rules:__
- immutable after creation
- no business mutation
- no lifecycle transitions
__Example:__
```C#
public class Currency
{
    public int Id { get; private set; }
    public string Code { get; private set; }
}
```
## Internal Mutable Entity
Entities created once and never change.
__Usage:__
- state entity
__Identity rules:__
- primary key: `int/uint/short` preferred
- no external identity required
- has row version 
__Rules:__
- editable after creation
__Example:__
```C#
public class Currency
{
    public int Id { get; private set; }
    public string Code { get; private set; }
    public int RowVersion {get; private set; }
}
```

## External Immutable Entity
Entities created once by external system and never change.
__Usage:__
- business event registration
__Identity rules:__
- primary key: `int/uint/short` preferred
- has external correlation key `Guid`
- external GUID is provided at creation time
- GUID must be unique
- no row version 
__Rules:__
- immutable after creation
- no business mutation
- no lifecycle transitions
- system uses `Id` internally
- external systems use `Guid`
- mapping between them is mandatory
- creation must be idempotent via GUID
__Example:__
```C#
public class Currency
{
    public int Id { get; private set; }
    public Guid Guid { get; private set; }
    public string Code { get; private set; }
}
```

## External Mutable Entity
Entities created once by external system and never change.
__Usage:__
- business event registration
__Identity rules:__
- primary key: `int/uint/short` preferred
- has external correlation key `Guid`
- external GUID is provided at creation time
- GUID must be unique
- has row version 
__Rules:__
- editable by any system after creation
- system uses `Id` internally
- external systems use `Guid`
- mapping between them is mandatory
- creation must be idempotent via GUID
__Example:__
```C#
public class Currency
{
    public int Id { get; private set; }
    public Guid Guid { get; private set; }
    public string Code { get; private set; }
    public int RowVersion {get; private set; }
}
```

# 3. Identity usage rules
## 3.1 ID is system identity
- used in domain logic
- used in persistence
- used in relationships
- used in internal APIs
## 3.2 GUID is correlation identity only
- used only for external systems
- used only for lookup
- never used in domain logic
## 3.3 Conversion rule (mandatory)
- No operation is executed directly on GUID.
```
External GUID → always resolved to internal ID first
```
- internal ID resolve BEFORE entering Application Layer or Concurrency validation
# 4. Concurrency model (RowVersion)
## 4.1 Definition
System uses:
> RowVersion as optimistic concurrency token
## 4.2 Responsibility split
**Domain layer MUST NOT handle concurrency**
**Domain:**
- does NOT validate RowVersion
- does NOT compare versions
- is unaware of persistence concurrency model

**Infrastructure responsibility**
Concurrency validation is handled by:
> VersionValidationPipeline (Infrastructure layer)

## 4.3 Pipeline behavior
Before executing update:
1. Load entity by ID
2. Compare RowVersion
3. If mismatch → throw ConcurrencyException
4. Proceed to handler

## 4.4 Example
```C#
public class VersionValidationPipeline<TRequest, TResponse>
{
    public async Task Handle(...)
    {
        var entity = await db.FindAsync(request.Id);

        if (!entity.RowVersion.SequenceEqual(request.RowVersion))
            throw new ConcurrencyException();

        return await next();
    }
}
```

# 5. Invariant enforcement
Entity must never enter invalid state.
```C#
public void Rename(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        throw new DomainException("Invalid name");

    Name = name;
}
```

**Entity mutation rules (unchanged but clarified)**
- mutations only via internals methods
- invariants enforced inside entity
- no awareness of RowVersion
- no awareness of GUID resolution
# 6. Critical rule: identity precedence
- internal `Id` is always system primary identity
- external `Guid` is only correlation key
- never use GUID as domain identity internally
- 
# 7. Anti-patterns
- using GUID as PK internally  
- skipping GUID → ID resolution step
- using GUID for domain decisions
- performing concurrency checks inside domain
- exposing RowVersion to domain behavior  
- mixing GUID and ID in business rules
- mutating ExternalId after creation  
- treating Constant entities as mutable
