---
description: Create a Value Object type — immutable self-validating record that encodes domain semantics
project_name: "{Module}.Domain"
name: "{ValueObject}"
element_kind: class
change_kind: create
---

# Goals
- Encode a domain concept with business meaning and invariant enforcement
- Eliminate primitive obsession by replacing raw primitives with semantic types
- Guarantee that invalid domain state cannot exist

# Core Principals
- Declared as `sealed record` — immutable and structurally equal by default
- Constructor validates all invariants — throws DomainException on violation
- Has no infrastructure or application dependencies — pure domain concept
- Single-property VO provides implicit conversion operators for ergonomic usage
- Multi-property VO requires a private parameterless constructor for EF Core materialization

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Single-property VO | {Concept} | Age | {Concept}.cs | Age.cs |
| Multi-property VO | {Concept} | Money | {Concept}.cs | Money.cs |

# Implementation changes

## Single-property ValueObject
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

## Multi-property ValueObject
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

# Rule changes

MUST:
- Be `sealed record`
- Be immutable — no public setters
- Validate all invariants in constructor
- Throw `DomainException` on invariant violation — never return null or bool
- Have no infrastructure or application dependencies
- Multi-property VO has `private` parameterless constructor for EF materialization
- Multi-property VO has `OwnsOne` EF configuration on owning entity (see [[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration.solution.skill]])

SHOULD:
- Provide implicit conversion operators for single-property VOs
- Override `ToString()` when used in logs or UI
- Extract complex invariant logic to domain rule

MUST NOT:
- Expose public setters
- Depend on repositories, DbContext, or any service
- Contain business logic beyond invariant validation — use domain rules for that

# Unittest TestCases
- [ ] WHEN applied THEN Encode a domain concept with business meaning and invariant enforcement
- [ ] WHEN applied THEN Eliminate primitive obsession by replacing raw primitives with semantic types
- [ ] WHEN applied THEN Guarantee that invalid domain state cannot exist
- [ ] WHEN applied THEN Declared as sealed record — immutable and structurally equal by default
- [ ] WHEN applied THEN Constructor validates all invariants — throws DomainException on violation
- [ ] WHEN applied THEN Has no infrastructure or application dependencies — pure domain concept
- [ ] WHEN applied THEN Single-property VO provides implicit conversion operators for ergonomic usage
- [ ] WHEN applied THEN Multi-property VO requires a private parameterless constructor for EF Core materialization
- [ ] WHEN naming 'Single-property VO' THEN pattern matches convention
- [ ] WHEN naming 'Multi-property VO' THEN pattern matches convention
