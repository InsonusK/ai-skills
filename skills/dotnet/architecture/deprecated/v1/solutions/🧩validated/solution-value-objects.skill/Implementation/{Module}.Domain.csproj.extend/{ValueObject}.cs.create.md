---
description: Create a strict Value Object type — immutable, self-validating record that inherits from Soft{ValueObject} and enforces invariants at construction, using its own local validation
project_name: "{Module}.Domain"
name: "{ValueObject}"
element_kind: class
change_kind: create
tags:
  - solution/value-objects
  - element/valueobject-cs
---

# Goals
- Encode a domain concept with business meaning and invariant enforcement
- Reuse the `Soft{ValueObject}` shape defined in `{Module}.Interfaces` instead of duplicating it
- Guarantee that invalid domain state cannot exist

# Core Principles
- Declared as `sealed record`, inherits from `Soft{ValueObject}` — never redeclares its properties
- Constructor validates via its own local predicate — a `private static` method on the same class — and throws `DomainException` on failure
- This solution does not require a shared rule abstraction: the condition is written and owned right here, next to the type it protects. A later, optional solution (`solution-domain-rules`) may centralize this condition into a reusable form — see that solution's own scope — but this file works completely on its own without it

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Single-property VO | `{Concept}` | `Email` | `{Concept}.cs` | `Email.cs` |
| Multi-property VO | `{Concept}` | `Money` | `{Concept}.cs` | `Money.cs` |

# Implementation changes

Single-property:

```csharp
// {Module}.Domain/ValueObjects/Email.cs
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Domain.ValueObjects;

public sealed record Email : SoftEmail
{
    public Email(string value) : base(value)
    {
        if (!IsValid(value))
            throw new DomainException("{ModuleName}.Email.Invalid", "Email is not valid.");
    }

    private static bool IsValid(string value) => !string.IsNullOrWhiteSpace(value) && value.Contains('@');

    public static implicit operator string(Email obj) => obj.Value;
    public static implicit operator Email(string value) => new(value);
}
```

Multi-property, with a private parameterless constructor for EF Core materialization:

```csharp
// {Module}.Domain/ValueObjects/Money.cs
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Domain.ValueObjects;

public sealed record Money : SoftMoney
{
    public Money(decimal amount, string currency) : base(amount, currency)
    {
        if (!IsValid(amount, currency))
            throw new DomainException("{ModuleName}.Money.Invalid", "Money amount/currency is not valid.");
    }

    private static bool IsValid(decimal amount, string currency)
        => amount >= 0 && !string.IsNullOrEmpty(currency);

    private Money() : base(0, string.Empty) { } // EF Core materialization only

    public override string ToString() => $"{Amount} {Currency}";
}
```

Worked example from a real module (`TaskModule`) — `Complexity : SoftComplexity`, validated by its own local predicate:

```csharp
namespace TaskUnderControl.Srv.TaskModule.Domain.ValueObjects;

using TaskUnderControl.Srv.TaskModule.Interfaces.ValueObjects;

public sealed record Complexity : SoftComplexity
{
    public Complexity(int value) : base(value)
    {
        if (!IsValid(value))
            throw new DomainException("TaskModule.Complexity.NonNegative", $"Complexity must be non-negative, but was {value}.");
    }

    private static bool IsValid(int value) => value >= 0;
}
```

# Rule changes

## MUST
- Be `sealed record`
- Inherit from `Soft{ValueObject}` — never redeclare its properties
- Be immutable — no public setters
- Validate via a `private static` predicate declared on the same class, and throw `DomainException` when it fails
- Have no infrastructure or application dependencies
- Multi-property VO has a `private` parameterless constructor for EF materialization

## SHOULD
- Provide implicit conversion operators for single-property VOs
- Override `ToString()` when used in logs or UI

## MUST NOT
- Depend on repositories, `DbContext`, or any service
- Depend on a separate rules project — the predicate is local to this file
- Expose public setters
- Redeclare a property that `Soft{ValueObject}` already declares
- Be used to carry identity — use the entity `Id` for that

# Check list
- [ ] Declared as `sealed record`, inherits from `Soft{ValueObject}`
- [ ] Constructor validates via a local `private static` predicate and throws `DomainException` on failure
- [ ] No public setters, no infrastructure dependencies, no separate rules-project dependency
- [ ] Multi-property VO has a `private` parameterless constructor
- [ ] Single-property VO has implicit conversion operators

# Unittest TestCases
- [ ] WHEN applied THEN Encode a domain concept with business meaning and invariant enforcement
- [ ] WHEN applied THEN Guarantee that invalid domain state cannot exist
- [ ] WHEN applied THEN Inherit from Soft{ValueObject} instead of duplicating its shape
- [ ] WHEN applied THEN Constructor throws DomainException when its local predicate fails
- [ ] WHEN applied THEN Has no infrastructure or application dependencies
- [ ] WHEN naming 'Single-property VO' THEN pattern matches convention
- [ ] WHEN naming 'Multi-property VO' THEN pattern matches convention
