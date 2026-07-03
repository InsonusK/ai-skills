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

# Core Principles
- Declared as `sealed record` — immutable and structurally equal by default
- Constructor validates all invariants by calling Rules — throws DomainException on violation; inline validation logic is forbidden
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
- Validate invariants in constructor by calling Rules — throw `DomainException` on violation
- Provide implicit operators to and from the underlying primitive
- Override `ToString()` for logging and UI use

```csharp
// {Module}.Domain/Rules/AgeRules.cs
public static class AgeRules
{
    public static bool IsValidAge(this int age)
        => age.IsInRange(1, 120); // delegates to IntRules
}

// {Module}.Domain/ValueObjects/Age.cs
public sealed record Age
{
    public int Value { get; }

    public Age(int value)
    {
        if (!value.IsValidAge())
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
- Validate all invariants in constructor by calling Rules — throw `DomainException` on violation
- Include a `private` parameterless constructor for EF Core materialization
- Override `ToString()` for logging and UI use

```csharp
// {Module}.Domain/Rules/MoneyRules.cs
public static class MoneyRules
{
    public static bool IsNonNegative(this decimal amount)
        => amount >= 0; // delegates to DecimalRules

    public static bool IsValidCurrency(this string currency)
        => !string.IsNullOrEmpty(currency); // delegates to StringRules
}

// {Module}.Domain/ValueObjects/Money.cs
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (!amount.IsNonNegative())
            throw new DomainException("Amount cannot be negative");
        if (!currency.IsValidCurrency())
            throw new DomainException("Currency required");
        Amount = amount;
        Currency = currency;
    }

    private Money() { } // EF Core materialization only

    public override string ToString() => $"{Amount} {Currency}";
}
```

# Rule changes

## MUST
- Be `sealed record`
- Be immutable — no public setters
- Validate all invariants in constructor by calling Rules
- Throw `DomainException` on invariant violation — never return null or bool
- Have no infrastructure or application dependencies
- Multi-property VO has `private` parameterless constructor for EF materialization
- Multi-property VO has `OwnsOne` EF configuration on owning entity (see [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration.skill]])
- All Value Objects declared as `sealed record`
- All Value Objects immutable — no public setters
- All Value Objects self-validating — throw `DomainException` on invalid construction

## SHOULD
- Provide implicit conversion operators for single-property VOs
- Override `ToString()` when used in logs or UI

## MUST NOT
- Expose public setters
- Depend on repositories, DbContext, or any service
- Contain business logic beyond invariant validation — use domain rules for that
- Contain inline validation logic — always delegate to a Rule
- Value Object depend on infrastructure, repositories, or application services
- Value Object expose public setters

# Check list
- [ ] All invariant checks are made by [{Rule}](./{Rule}.cs.create.md)
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
