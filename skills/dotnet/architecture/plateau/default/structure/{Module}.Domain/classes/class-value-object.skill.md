---
name: class-value-object
description: Create a Value Object type in two strengths — a permissive Soft{ValueObject} in {Module}.Interfaces, and a strict {ValueObject} in {Module}.Domain that inherits from it and validates via a centralized Check()
domain: skill
type: template
version: 20260821
plateau: default
tags:
  - skill/template/class
  - plateau/default
  - stack/dotnet
  - concern/architecture

created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]]"
---

# Goal
- Encode a domain concept with business meaning and invariant enforcement, in both a permissive (Interfaces) and a strict (Domain) strength
- Eliminate primitive obsession by replacing raw primitives with semantic types
- Guarantee that invalid domain state cannot exist

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create|{ValueObject}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.extend|{ValueObject}.cs]]

# Core Principles
- Apply ONE plateau template per class
- Declared as `sealed record` — immutable and structurally equal by default
- Inherits from `Soft{ValueObject}` declared in `{Module}.Interfaces` — never redeclares its properties
- `Soft{ValueObject}` allows invalid values; the Domain VO enforces invariants at construction
- Constructor validates by calling `Check()` — a centralized condition declared once in `{Module}.Domain.Rules`, shared with `{ValueObject}PropertyValidator` and any Entity method that needs the same condition
- Has no infrastructure or application dependencies — pure domain concept
- Single-property VO provides implicit conversion operators for ergonomic usage
- Multi-property VO requires a private parameterless constructor for EF Core materialization

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create|{ValueObject}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.extend|{ValueObject}.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Single-property VO | {Concept} | Age | {Concept}.cs | Age.cs |
| Multi-property VO | {Concept} | Money | {Concept}.cs | Money.cs |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create|{ValueObject}.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-value-object
//Plateau: default
//Version: 20260821
```

## Single-property ValueObject

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

## Multi-property ValueObject

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

`this.Check()` calls an extension method defined in `{Module}.Domain.Rules` (see `class-rule.skill.md`) — the exact same condition a `{ValueObject}PropertyValidator` for the matching `Soft{ValueObject}` also calls, so the invariant is declared exactly once regardless of which layer enforces it.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create|{ValueObject}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.extend|{ValueObject}.cs]]

# Rules
MUST:
	- Be `sealed record`, inherit from `Soft{ValueObject}` — never redeclare its properties
	- Be immutable — no public setters
	- Constructor calls `Check()` and throws `DomainException` on the first `Error`-severity failure — never on bare `!result.IsValid`
	- Have no infrastructure or application dependencies
	- Multi-property VO has `private` parameterless constructor for EF materialization
	- Multi-property VO has `OwnsOne` EF configuration on owning entity (see [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration.skill]])
SHOULD:
	- Provide implicit conversion operators for single-property VOs
	- Override `ToString()` when used in logs or UI
MUST NOT:
	- Expose public setters
	- Depend on repositories, DbContext, or any service
	- Contain inline validation logic — always delegate to `Check()`
	- Duplicate the `Soft{ValueObject}` shape instead of inheriting from it

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create|{ValueObject}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.extend|{ValueObject}.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Encode a domain concept with business meaning and invariant enforcement
- [ ] WHEN applied THEN Eliminate primitive obsession by replacing raw primitives with semantic types
- [ ] WHEN applied THEN Guarantee that invalid domain state cannot exist
- [ ] WHEN applied THEN Declared as sealed record — immutable and structurally equal by default
- [ ] WHEN applied THEN Constructor calls Check() — throws DomainException on the first Error-severity failure
- [ ] WHEN applied THEN Has no infrastructure or application dependencies — pure domain concept
- [ ] WHEN applied THEN Single-property VO provides implicit conversion operators for ergonomic usage
- [ ] WHEN applied THEN Multi-property VO requires a private parameterless constructor for EF Core materialization
- [ ] WHEN naming 'Single-property VO' THEN pattern matches convention
- [ ] WHEN naming 'Multi-property VO' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/solution-value-objects.skill|solution-value-objects]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create|{ValueObject}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.extend|{ValueObject}.cs]]
