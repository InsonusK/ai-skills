---
description: Extend entity to type properties with invariant state as Value Objects instead of primitives
project_name: "{Module}.Domain"
name: "{Entity}"
element_kind: class
change_kind: extend
tags:
  - solution/value-objects
  - element/entity-cs
---

# Goals
- Encapsulate invariant state on Entity properties into dedicated Value Object types
- Keep the Entity's own code free of inline validation — an invalid value cannot be assigned to a VO-typed property in the first place

# Core Principles
- Value Object immutability guarantees that once an Entity holds a value, that value cannot be mutated into an invalid state
- Equality of value properties on Entities is evaluated by Value Object structural equality
- How the Entity mutates these properties (behavior methods, invariant orchestration across several properties) is not this solution's concern — see the sibling solution `solution-domain-behaviour`

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity class | {EntityName} | Order | {EntityName}.cs | Order.cs |

# Implementation changes

**AS IS** (from `plateau-stateless-non-interactive-service`'s [[skills/dotnet/architecture/v3/plateau/plateau-stateless-non-interactive-service/structure/{Module}.Domain/classes/plateau-stateless-non-interactive-service--class-entity.skill.md|class-entity]] template — properties are primitives with inline validation in the setter):
```csharp
public class Order
{
    public int Id { get; internal set; }
    private decimal _total;
    public decimal Total
    {
        get => _total;
        set
        {
            if (value < 0)
                throw new DomainException("Invalid total");
            _total = value;
        }
    }
    public string CustomerEmail { get; internal set; }
}
```

**TO BE** (after this solution) — properties that carry invariant state or business semantics are typed as Value Objects, not primitives; the inline `if`-checks move into the Value Object's own constructor:

```csharp
public class Order
{
    public int Id { get; internal set; }
    public Money Total { get; internal set; }
    public Email CustomerEmail { get; internal set; }
}
```

# Rule changes

## MUST
- Use a Value Object type on an Entity property when the value has invariant state or carries business semantics
- Entity properties other than `Id` and `Version` must be Value Object types, unless they are unconstrained generic parameters
- If a property has any validation rule beyond the generic type's contract, the generic type must be replaced with a Value Object

## MUST NOT
- Use a primitive type on an Entity property when the value carries business meaning or invariant constraints
- Expose a primitive Entity property when a Value Object could enforce the same invariants

# Check list
- [ ] All properties except `Id`/`Version`/GUID foreign keys that have invariant checks use a `{ValueObject}` per [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create|{ValueObject}.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Entity properties that carry business meaning or invariant constraints use Value Objects instead of primitives
- [ ] WHEN applied THEN Value Object immutability guarantees that once an Entity holds a value, that value cannot be mutated into an invalid state
- [ ] WHEN applied THEN Equality of value properties on Entities is evaluated by Value Object structural equality
- [ ] WHEN naming 'Entity class' THEN pattern matches convention
