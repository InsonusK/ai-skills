---
description: Make Domain Value Object inherit from Soft{ValueObject} and enforce invariants
project_name: "{Module}.Domain"
name: "{ValueObject}.cs"
element_kind: class
change_kind: extend
---

# Goals
- Reuse the `Soft{ValueObject}` shape defined in Interfaces
- Keep strict invariant enforcement in Domain

# Core Principles
- Constructor validates invariants by calling Rules and throws `DomainException`
- Implicit conversion operators remain available for single-property VOs

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Domain single-property VO | `{Concept}` | `Email` | `{Concept}.cs` | `Email.cs` |
| Domain multi-property VO | `{Concept}` | `Money` | `{Concept}.cs` | `Money.cs` |

# Implementation changes

Single-property domain value object:

```csharp
// {Module}.Domain/Rules/EmailRules.cs
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Domain.Rules;

public static class EmailRules
{
    // primitive overload — single source of truth
    public static bool IsValidEmail(this string value)
        => !string.IsNullOrWhiteSpace(value) && value.Contains('@');

    // SoftValueObject overload — delegates to primitive overload
    public static bool IsValidEmail(this SoftEmail email)
        => email.Value.IsValidEmail();
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
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Domain.Rules;

public static class MoneyRules
{
    // primitive overloads — single source of truth
    public static bool IsNonNegative(this decimal amount)
        => amount >= 0;

    public static bool IsValidCurrency(this string currency)
        => !string.IsNullOrEmpty(currency);

    // SoftValueObject overload — delegates to primitive overloads
    public static bool IsValidMoney(this SoftMoney money)
        => money.Amount.IsNonNegative() && money.Currency.IsValidCurrency();
}

// {Module}.Domain/ValueObjects/Money.cs
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Domain.ValueObjects;

public sealed record Money : SoftMoney
{
    public Money(decimal amount, string currency) : base(amount, currency)
    {
        if (!this.IsValidMoney())
            throw new DomainException("Invalid money");
    }

    private Money() : base(0, string.Empty) { } // EF Core materialization only

    public override string ToString() => $"{Amount} {Currency}";
}
```

# Rule changes

## MUST
- Inherit from `Soft{ValueObject}`
- Validate invariants in constructor by calling Rules
- Throw `DomainException` on invalid values
- For every `{ValueObject}` in `/{Module}.Domain/ValueObjects` there is a `Soft{ValueObject}` in `/{Module}.Interfaces/ValueObjects`
- `Soft{ValueObject}` does not validate values in its constructor or properties
- For every `Soft{ValueObject}` there is a `{ValueObject}PropertyValidator` in `/{Module}.Application/Validators/Property` extending `AbstractValidator<Soft{ValueObject}>`
- DTO value-concept properties are `Soft{ValueObject}` types, not primitives
- DTO validators use `SetValidator(IValidator<Soft{ValueObject}>)` for every value-concept property
- Rule provides a `Soft{ValueObject}` overload in addition to the primitive overload
- `{Module}.Domain.csproj` references `{Module}.Interfaces.csproj` for the `Soft{ValueObject}` base types

## SHOULD
- Provide implicit conversion operators for single-property VOs
- Override `ToString()` when used in logs or UI
- Keep `Soft{ValueObject}` immutable except for allowing invalid values (use `init` setters or public setters only when necessary)
- Name property validator `{ValueObject}PropertyValidator`

## MUST NOT
- Allow invalid values to persist
- Reference FluentValidation
- `Soft{ValueObject}` throw exceptions for invalid values

# Anti-patterns
- Domain VO duplicating `Soft{ValueObject}` shape instead of inheriting
- Domain VO silently accepting invalid values

# Check list
- [ ] Inherits from `Soft{ValueObject}`
- [ ] Constructor throws `DomainException` on invalid values
- [ ] No public setters

# Unittest TestCases
- [ ] When Domain `{ValueObject}` is created with a valid value Then the object is created
- [ ] When Domain `{ValueObject}` is created with an invalid value Then `DomainException` is thrown
- [ ] When Domain `{ValueObject}` is created Then it calls a Rule for validation
- [ ] When two Domain `{ValueObject}` instances have the same value Then they are equal
