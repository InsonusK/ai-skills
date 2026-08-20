---
description: Create a strict Value Object type — immutable, self-validating record that inherits from Soft{ValueObject} and enforces invariants at construction
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
- Constructor calls `Check()` (an extension method on `Soft{ValueObject}`, provided by `solution-domain-rules`) and throws `DomainException` on the first `Error`-severity failure — this solution does not define `Check()` or any rule condition itself

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
        var result = this.Check();
        var blocking = result.Errors.FirstOrDefault(e => e.Severity == Severity.Error);
        if (blocking is not null)
            throw new DomainException(blocking.ErrorCode, blocking.ErrorMessage);
    }

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
        var result = this.Check();
        var blocking = result.Errors.FirstOrDefault(e => e.Severity == Severity.Error);
        if (blocking is not null)
            throw new DomainException(blocking.ErrorCode, blocking.ErrorMessage);
    }

    private Money() : base(0, string.Empty) { } // EF Core materialization only

    public override string ToString() => $"{Amount} {Currency}";
}
```

Worked example from a real module (`TaskModule`) — `Complexity : SoftComplexity`:

```csharp
namespace TaskUnderControl.Srv.TaskModule.Domain.ValueObjects;

using TaskUnderControl.Srv.TaskModule.Domain.Rules;   // Check() extension — defined by solution-domain-rules
using TaskUnderControl.Srv.TaskModule.Interfaces.ValueObjects;

public sealed record Complexity : SoftComplexity
{
    public Complexity(int value) : base(value)
    {
        var result = this.Check();

        // Errors.Any(Severity == Error), НЕ !result.IsValid — ValidationResult.IsValid игнорирует
        // Severity, так что смешанный Error/Warning результат ошибочно заблокировал бы конструктор.
        var blocking = result.Errors.FirstOrDefault(e => e.Severity == Severity.Error);
        if (blocking is not null)
            throw new DomainException(blocking.ErrorCode, blocking.ErrorMessage);
    }
}
```

# Rule changes

## MUST
- Be `sealed record`
- Inherit from `Soft{ValueObject}` — never redeclare its properties
- Be immutable — no public setters
- Constructor calls `Check()` and throws `DomainException` on the first `Error`-severity failure — never on bare `!result.IsValid`
- Have no infrastructure or application dependencies
- Multi-property VO has a `private` parameterless constructor for EF materialization

## SHOULD
- Provide implicit conversion operators for single-property VOs
- Override `ToString()` when used in logs or UI

## MUST NOT
- Depend on repositories, `DbContext`, or any service
- Contain inline validation logic — always delegate to `Check()`
- Expose public setters
- Redeclare a property that `Soft{ValueObject}` already declares
- Be used to carry identity — use the entity `Id` for that

# Check list
- [ ] Declared as `sealed record`, inherits from `Soft{ValueObject}`
- [ ] Constructor calls `Check()`, throws `DomainException` on the first `Error`-severity failure
- [ ] No public setters, no infrastructure dependencies
- [ ] Multi-property VO has a `private` parameterless constructor
- [ ] Single-property VO has implicit conversion operators

# Unittest TestCases
- [ ] WHEN applied THEN Encode a domain concept with business meaning and invariant enforcement
- [ ] WHEN applied THEN Guarantee that invalid domain state cannot exist
- [ ] WHEN applied THEN Inherit from Soft{ValueObject} instead of duplicating its shape
- [ ] WHEN applied THEN Constructor throws DomainException on the first Error-severity Check() failure
- [ ] WHEN applied THEN Has no infrastructure or application dependencies
- [ ] WHEN naming 'Single-property VO' THEN pattern matches convention
- [ ] WHEN naming 'Multi-property VO' THEN pattern matches convention
