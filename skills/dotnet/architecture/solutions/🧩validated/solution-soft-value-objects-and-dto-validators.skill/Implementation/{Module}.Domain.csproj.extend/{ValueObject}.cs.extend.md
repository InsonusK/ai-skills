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
- Domain VO inherits from `Soft{ValueObject}`
- Constructor validates invariants and throws `DomainException`
- Implicit conversion operators remain available for single-property VOs

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Domain single-property VO | `{Concept}` | `Email` | `{Concept}.cs` | `Email.cs` |
| Domain multi-property VO | `{Concept}` | `Money` | `{Concept}.cs` | `Money.cs` |

# Implementation changes

Single-property domain value object:

```csharp
// {Module}.Domain/ValueObjects/Email.cs
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Domain.ValueObjects;

public sealed record Email : SoftEmail
{
    public Email(string value) : base(value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
            throw new DomainException("Invalid email");
    }

    public static implicit operator string(Email obj) => obj.Value;
    public static implicit operator Email(string value) => new(value);
}
```

Multi-property domain value object:

```csharp
// {Module}.Domain/ValueObjects/Money.cs
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Domain.ValueObjects;

public sealed record Money : SoftMoney
{
    public Money(decimal amount, string currency) : base(amount, currency)
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative");
        if (string.IsNullOrEmpty(currency))
            throw new DomainException("Currency required");
    }

    private Money() : base(0, string.Empty) { } // EF Core materialization only

    public override string ToString() => $"{Amount} {Currency}";
}
```

# Rule changes
MUST:
- Inherit from `Soft{ValueObject}`
- Validate invariants in constructor
- Throw `DomainException` on invalid values

SHOULD:
- Provide implicit conversion operators for single-property VOs
- Override `ToString()` when used in logs or UI

MUST NOT:
- Allow invalid values to persist
- Reference FluentValidation

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
- [ ] When two Domain `{ValueObject}` instances have the same value Then they are equal
