---
uid:
name: value-object-class
description: Immutable self-validating domain type that encodes business semantics and eliminates primitive obsession.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/class
  - dotnet
  - domain
  - value-object
triggers:
  - create value object
  - implement value object
  - eliminate primitive obsession
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-value-object.solution.skill]]"
extended_by:
---

# Goal
- Eliminate primitive obsession by encoding domain semantics into dedicated types
- Prevent invalid domain state by making Value Objects self-validating at construction time
- Ensure equality is based on value, not reference

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-value-object.solution.skill#ValueObject (single-property)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-value-object.solution.skill#ValueObject (multi-property)]]

# Core Principles
- Semantics belong to types, not primitives
- Value Object is immutable — no property can change after construction
- Value Object is self-validating — invalid state cannot exist, constructor throws on violation
- Equality is structural — two instances with same values are equal
- Value Object has no identity — it is defined entirely by its value
- Value Object has no infrastructure or application dependencies
- Multi-property VO requires a private parameterless constructor for EF Core materialization
- Single-property VO should provide implicit conversion operators for ergonomic usage

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-value-object.solution.skill#ValueObject (single-property)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-value-object.solution.skill#ValueObject (multi-property)]]

# Structure

## Place in csproj
Defined in [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/{Module}.Domain.csproj.skill]]
```
/{ModuleName}.Domain
  /ValueObjects
    {ValueObjectName}.cs
```

## Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Single-property VO | {Noun} | Age | {Noun}.cs | Age.cs |
| Multi-property VO | {Noun} | Money | {Noun}.cs | Money.cs |

## Implementation

### Single-property ValueObject
```csharp
public sealed record Age
{
    public int Value { get; }

    public Age(int value)
    {
        if (value <= 0 || value > 120)
            throw new DomainException("Invalid Age");
        Value = value;
    }

    public static implicit operator int(Age obj) => obj.Value;
    public static implicit operator Age(int value) => new(value);
    public override string ToString() => Value.ToString();
}
```

### Multi-property ValueObject
```csharp
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative");
        if (string.IsNullOrEmpty(currency))
            throw new DomainException("Currency required");
        Amount = amount;
        Currency = currency;
    }

    private Money() { } // EF Core materialization only

    public override string ToString() => $"{Amount} {Currency}";
}
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-value-object.solution.skill#ValueObject (single-property)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-value-object.solution.skill#ValueObject (multi-property)]]

# Rules

MUST:
- Be `sealed record`
- Be immutable — no public setters
- Validate all invariants in constructor
- Throw `DomainException` on invariant violation
- Have no infrastructure or application dependencies
- Multi-property VO has `private` parameterless constructor for EF materialization
- Multi-property VO has `OwnsOne` EF configuration on owning entity

SHOULD:
- Provide implicit conversion operators for single-property VOs
- Override `ToString()` when used in logs or UI
- Extract complex invariant logic to domain rule

MUST NOT:
- Expose public setters
- Depend on repositories, DbContext, or any service
- Contain business logic beyond invariant validation
- Be used to carry identity
- Primitive used in place of VO when the primitive carries business meaning

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-value-object.solution.skill#ValueObject (single-property)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-value-object.solution.skill#ValueObject (multi-property)]]

# Anti-patterns
- `string Email` on entity instead of `Email` VO — loses invariant enforcement
- VO with public setter — allows post-construction mutation
- VO that throws on ToString() when null internal state
- Multi-property VO without private parameterless constructor — EF materialization fails
- Multi-property VO without OwnsOne config — EF creates a shadow table
- VO with infrastructure dependency — couples domain to persistence layer
- Reusing same VO type across modules — each module defines its own VO types

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-value-object.solution.skill#ValueObject (single-property)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-value-object.solution.skill#ValueObject (multi-property)]]

# Check list
- [ ] Declared as `sealed record`
- [ ] All invariants validated in constructor
- [ ] `DomainException` thrown on violation
- [ ] No public setters
- [ ] No infrastructure or service dependencies
- [ ] Single-property VO has implicit conversion operators
- [ ] Multi-property VO has private parameterless constructor
- [ ] Multi-property VO has OwnsOne EF configuration on owning entity
- [ ] `ToString()` implemented when used in logs or UI
- [ ] Lives in /{Module}.Domain/ValueObjects

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-value-object.solution.skill#ValueObject (single-property)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-value-object.solution.skill#ValueObject (multi-property)]]

# Unittest TestCases
- [ ] When value is below lower bound Then constructor throws DomainException
- [ ] When value is above upper bound Then constructor throws DomainException
- [ ] When value is at lower boundary (min valid) Then object created successfully
- [ ] When value is at upper boundary (max valid) Then object created successfully
- [ ] When valid value provided Then object created with correct property value
- [ ] When two VOs have same value Then they are equal
- [ ] When two VOs have different values Then they are not equal
- [ ] When implicit operator used Then value round-trips losslessly (single-property only)
- [ ] When multi-property VO persisted and loaded Then all properties materialize correctly

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-value-object.solution.skill#ValueObject (single-property)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-value-object.solution.skill#ValueObject (multi-property)]]
