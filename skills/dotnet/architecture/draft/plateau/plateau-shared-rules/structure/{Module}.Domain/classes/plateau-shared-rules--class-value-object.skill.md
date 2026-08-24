---
name: class-value-object
description: Class {ValueObject} in the shared-rules plateau
whenToUse: when a domain concept needs invariant enforcement — creating the strict Domain-side Value Object that inherits from its Soft{ValueObject} base
domain: skill
type: template
plateau: shared-rules
version: 20260824150000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]]"
---

# Goal
- Encode a domain concept with business meaning and invariant enforcement
- Reuse the `Soft{ValueObject}` shape defined in `{Module}.Interfaces` instead of duplicating it
- Guarantee that invalid domain state cannot exist

__Applied solutions:__
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]

# Core Principles
- Declared as `sealed record`, inherits from `Soft{ValueObject}` — never redeclares its properties
- Constructor validates via its own local `private static` predicate and throws `DomainException` on failure
- Works completely on its own — a later, optional `solution-domain-rules` may centralize the condition, but nothing here assumes that exists

__Applied solutions:__
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Single-property VO | {Concept} | Email | {Concept}.cs | Email.cs |
| Multi-property VO | {Concept} | Money | {Concept}.cs | Money.cs |

# Implementation
```csharp
//Skill: class-value-object
//Plateau: shared-rules
//Version: 20260824150000

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

public sealed record Money : SoftMoney
{
    public Money(decimal amount, string currency) : base(amount, currency)
    {
        if (!IsValid(amount, currency))
            throw new DomainException("{ModuleName}.Money.Invalid", "Money amount/currency is not valid.");
    }

    private static bool IsValid(decimal amount, string currency) => amount >= 0 && !string.IsNullOrEmpty(currency);

    private Money() : base(0, string.Empty) { } // EF Core materialization only
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]

## Once the same condition is duplicated elsewhere: redirect to a centralized rule

Optional, applied only once the same predicate is found duplicated in a second consumer (a `PropertyValidator`, a `Dto`Validator) — the local `private static IsValid` is deleted, not kept alongside:

```csharp
public sealed record Complexity : SoftComplexity
{
    public Complexity(int value) : base(value)
    {
        var result = this.Check();   // Check() now comes from {Module}.Domain.Rules

        var blocking = result.Errors.FirstOrDefault(e => e.Severity == Severity.Error);
        if (blocking is not null)
            throw new DomainException(blocking.ErrorCode, blocking.ErrorMessage);
    }
}
```

The thrown exception, its message, and behavior on invalid input do not change — only where the condition is declared. See [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] and its [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.extend.md|{ValueObject}.cs.extend]].

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.extend.md|{ValueObject}.cs.extend]]

# Rules
MUST:
- Be `sealed record`, inherit from `Soft{ValueObject}` — never redeclare its properties
- Be immutable — no public setters
- Validate via a `private static` predicate declared on the same class, or (once redirected) via `this.Check()` from `{Module}.Domain.Rules` — never both at once — and throw `DomainException` when it fails
- Multi-property VO has a `private` parameterless constructor for EF materialization
- If redirected, delete the local `private static` predicate — never keep it alongside the centralized one
MUST NOT:
- Depend on repositories, `DbContext`, or any service
- Depend on `{Module}.Domain.Rules` speculatively — only once the same condition is genuinely duplicated in a second consumer
- Redeclare a property that `Soft{ValueObject}` already declares
- Be used to carry identity — use the entity `Id` for that

__Applied solutions:__
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.extend.md|{ValueObject}.cs.extend]]

# Check list
- [ ] Declared as `sealed record`, inherits from `Soft{ValueObject}`
- [ ] Constructor validates via exactly one of a local `private static` predicate or `this.Check()`, never both, and throws `DomainException` on failure
- [ ] No public setters, no infrastructure dependencies
- [ ] Multi-property VO has a `private` parameterless constructor

__Applied solutions:__
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]
