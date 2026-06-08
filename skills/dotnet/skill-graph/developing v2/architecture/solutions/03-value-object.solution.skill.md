---
uid: 662a73ac-4d9b-4b7b-b6f4-b7cc1da4d44f
name: value-object
description: Defines the Value Object pattern — immutable self-validating types that encode domain semantics and eliminate primitive obsession
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - ddd
  - value-object
triggers:
  - create value object
  - eliminate primitive obsession
  - encode domain concept as type
  - design immutable domain type
creates:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/ValueObjects/ValueObject.class.skill]]"
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/{Module}.Domain.csproj.skill]]"
depends_on:
  - "[[02-solution-layer-structure.solution.skill]]"
---

# Goal
- Eliminate primitive obsession by encoding domain semantics into dedicated types
- Prevent invalid domain state by making Value Objects self-validating at construction time
- Ensure equality is based on value, not reference — two VOs with same data are equal
- Define two VO shapes: single-property and multi-property, each with distinct rules

# Core Principles
- Semantics belong to types, not primitives — if a primitive carries business meaning, it is a VO
- Value Object is immutable — no property can change after construction
- Value Object is self-validating — invalid state cannot exist, constructor throws on violation
- Equality is structural — two instances with same values are equal
- Value Object has no identity — it is defined entirely by its value
- Value Object has no infrastructure or application dependencies — pure domain concept
- Multi-property VO requires a private parameterless constructor for EF Core materialization
- Single-property VO should provide implicit conversion operators for ergonomic usage

# Depend on solutions
- [[02-solution-layer-structure.solution.skill]] — Value Objects live in {Module}.Domain, which this solution defines

# Implementation

## {Module}.Domain (.csproj) (extended)

### Project extension

#### Goal
- Store all Value Object types for this bounded context

#### Structure

##### Project Structure
```
/{Module}.Domain
  /ValueObjects
    Age.cs
    Money.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /ValueObjects | All Value Object types for this module | |

#### Rules
MUST:
- All Value Objects live in /{Module}.Domain/ValueObjects

---

### Class extension

#### ValueObject (single-property)

##### Goal
- Encode a single domain primitive with business meaning and invariant enforcement

##### Core Principal
- Declared as `sealed record` — immutable and structurally equal by default
- Single public property with private or init-only setter
- Constructor validates all invariants — throws DomainException on violation
- Implicit conversion operators allow ergonomic use alongside primitives

##### Implementation changes
Single-property ValueObject must:
- Be declared as `sealed record`
- Have one public property with `get` only
- Validate invariants in constructor — throw `DomainException` on violation
- Provide implicit operators to and from the underlying primitive
- Override `ToString()` for logging and UI use

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

##### Rule changes
MUST:
- Be `sealed record`
- Be immutable — no public setters
- Validate all invariants in constructor
- Throw `DomainException` on invariant violation — never return null or bool
- Have no infrastructure or application dependencies

SHOULD:
- Provide implicit conversion operators for single-property VOs
- Override `ToString()` when used in logs or UI

MUST NOT:
- Expose public setters
- Depend on repositories, DbContext, or any service
- Contain business logic beyond invariant validation — use domain rules for that

---

#### ValueObject (multi-property)

##### Goal
- Encode a domain concept that requires multiple related values to be meaningful
- Persist as owned flat columns on the owning entity table via EF Core OwnsOne

##### Core Principal
- Declared as `sealed record` — structurally equal across all properties
- Private parameterless constructor required for EF Core materialization
- No implicit operators — multi-property VOs are used by property name
- EF OwnsOne configuration required in entity configuration — see domain-configuration solution

##### Implementation changes
Multi-property ValueObject must:
- Be declared as `sealed record`
- Have all public properties with `get` only
- Validate all invariants in constructor — throw `DomainException` on violation
- Include a `private` parameterless constructor for EF Core materialization
- Override `ToString()` for logging and UI use

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

##### Rule changes
MUST:
- Be `sealed record`
- Be immutable — no public setters
- Validate all invariants in constructor
- Throw `DomainException` on invariant violation
- Have `private` parameterless constructor for EF materialization
- Have `OwnsOne` EF configuration on the owning entity

MUST NOT:
- Expose public setters
- Depend on infrastructure or application services
- Be used without OwnsOne EF configuration

---

# Rules

MUST:
- All Value Objects declared as `sealed record`
- All Value Objects immutable — no public setters
- All Value Objects self-validating — throw `DomainException` on invalid construction
- Value Objects live in /{Module}.Domain/ValueObjects
- Multi-property VO has private parameterless constructor
- Multi-property VO has OwnsOne EF configuration on owning entity

SHOULD:
- Single-property VO has implicit conversion operators
- All VOs override ToString() when used in logs or UI
- Complex invariant logic extracted to domain rule — see domain-rule solution

MUST NOT:
- Value Object depend on infrastructure, repositories, or application services
- Value Object expose public setters
- Value Object be used to carry identity — use entity Id for that
- Primitive used in place of VO when the primitive carries business meaning

# Anti-patterns
- `string Email` on entity instead of `Email` VO — loses invariant enforcement
- VO with public setter — allows post-construction mutation, invalidates immutability guarantee
- VO that throws on ToString() when null internal state — private constructor must not leave fields unset for EF
- Multi-property VO without private parameterless constructor — EF materialization fails silently
- Multi-property VO without OwnsOne config — EF creates a shadow table or fails mapping
- VO with infrastructure dependency — couples domain to persistence layer
- Reusing same VO type across modules — each module defines its own VO types

# Check list
- [ ] Declared as `sealed record`
- [ ] All invariants validated in constructor
- [ ] DomainException thrown on violation — not null, not bool return
- [ ] No public setters
- [ ] No infrastructure or service dependencies
- [ ] Single-property VO has implicit conversion operators
- [ ] Multi-property VO has private parameterless constructor
- [ ] Multi-property VO has OwnsOne EF configuration on owning entity
- [ ] ToString() implemented when used in logs or UI
- [ ] Lives in /{Module}.Domain/ValueObjects

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
