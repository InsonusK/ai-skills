---
name: class-value-object
description: Create a Value Object type — immutable self-validating record that encodes domain semantics
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators.skill]]"
---

# Goal
- Encode a domain concept with business meaning and invariant enforcement
- Eliminate primitive obsession by replacing raw primitives with semantic types
- Guarantee that invalid domain state cannot exist

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.extend.md|{ValueObject}.cs.extend]]

# Core Principals
- Apply ONE plateau template per class
- Declared as `sealed record` — immutable and structurally equal by default
- Constructor validates all invariants — throws DomainException on violation
- Has no infrastructure or application dependencies — pure domain concept
- Single-property VO provides implicit conversion operators for ergonomic usage
- Multi-property VO requires a private parameterless constructor for EF Core materialization
- When the module exposes a value object to other modules, the Domain VO inherits from `Soft{ValueObject}` declared in `{Module}.Interfaces`
- `Soft{ValueObject}` allows invalid values; the Domain VO enforces invariants at construction

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.extend.md|{ValueObject}.cs.extend]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Single-property VO | {Concept} | Age | {Concept}.cs | Age.cs |
| Multi-property VO | {Concept} | Money | {Concept}.cs | Money.cs |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.extend.md|{ValueObject}.cs.extend]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-value-object
//Plateau: default
//Version: 20260628
```

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

## Value Object inheriting from Soft{ValueObject}
When the value object shape must be shared with other modules, declare `Soft{ValueObject}` in `{Module}.Interfaces` and inherit from it in Domain.

Single-property domain value object:

```csharp
// {Module}.Domain/Rules/EmailRules.cs
public static class EmailRules
{
    public static bool IsValidEmail(this string value)
        => !string.IsNullOrWhiteSpace(value) && value.Contains('@'); // delegates to StringRules
}

// {Module}.Domain/ValueObjects/Email.cs
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Domain.ValueObjects;

public sealed record Email : SoftEmail
{
    public Email(string value) : base(value)
    {
        if (!value.IsValidEmail())
            throw new DomainException("Invalid email");
    }

    public static implicit operator string(Email obj) => obj.Value;
    public static implicit operator Email(string value) => new(value);
}
```

Multi-property domain value object:

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
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Domain.ValueObjects;

public sealed record Money : SoftMoney
{
    public Money(decimal amount, string currency) : base(amount, currency)
    {
        if (!amount.IsNonNegative())
            throw new DomainException("Amount cannot be negative");
        if (!currency.IsValidCurrency())
            throw new DomainException("Currency required");
    }

    private Money() : base(0, string.Empty) { } // EF Core materialization only

    public override string ToString() => $"{Amount} {Currency}";
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.extend.md|{ValueObject}.cs.extend]]

# Rules
MUST:
	- Be `sealed record`
	- Be immutable — no public setters
	- Validate all invariants in constructor by calling a Rule — inline validation logic is forbidden
	- Throw `DomainException` on invariant violation — never return null or bool
	- Have no infrastructure or application dependencies
	- Multi-property VO has `private` parameterless constructor for EF materialization
	- Inherit from `Soft{ValueObject}` when the value object is exposed to other modules
	- Multi-property VO has `OwnsOne` EF configuration on owning entity (see [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration.skill]])
SHOULD:
	- Provide implicit conversion operators for single-property VOs
	- Override `ToString()` when used in logs or UI
MUST NOT:
	- Expose public setters
	- Depend on repositories, DbContext, or any service
	- Contain business logic beyond invariant validation — use domain rules for that
	- Contain inline validation logic — always delegate to a Rule
	- Duplicate the `Soft{ValueObject}` shape instead of inheriting from it

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.extend.md|{ValueObject}.cs.extend]]

# Unittest TestCases
- [ ] WHEN applied THEN Encode a domain concept with business meaning and invariant enforcement
- [ ] WHEN applied THEN Eliminate primitive obsession by replacing raw primitives with semantic types
- [ ] WHEN applied THEN Guarantee that invalid domain state cannot exist
- [ ] WHEN applied THEN Declared as sealed record — immutable and structurally equal by default
- [ ] WHEN applied THEN Constructor validates all invariants by calling Rules — throws DomainException on violation
- [ ] WHEN applied THEN Has no infrastructure or application dependencies — pure domain concept
- [ ] WHEN applied THEN Single-property VO provides implicit conversion operators for ergonomic usage
- [ ] WHEN applied THEN Multi-property VO requires a private parameterless constructor for EF Core materialization
- [ ] WHEN naming 'Single-property VO' THEN pattern matches convention
- [ ] WHEN naming 'Multi-property VO' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill.md|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.extend.md|{ValueObject}.cs.extend]]
