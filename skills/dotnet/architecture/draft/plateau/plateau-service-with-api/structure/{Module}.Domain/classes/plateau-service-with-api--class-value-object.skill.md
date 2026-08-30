---
name: plateau-service-with-api--class-value-object
description: Class {ValueObject} in the service-with-api plateau
whenToUse: when a domain concept needs invariant enforcement — creating the strict Domain-side Value Object that inherits from its Soft{ValueObject} base
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/class
  - plateau/service-with-api
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
//Plateau: service-with-api
//Version: 20260825120000

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

# Rules
MUST:
- Be `sealed record`, inherit from `Soft{ValueObject}` — never redeclare its properties
- Be immutable — no public setters
- Validate via a `private static` predicate declared on the same class, and throw `DomainException` when it fails
- Multi-property VO has a `private` parameterless constructor for EF materialization
MUST NOT:
- Depend on repositories, `DbContext`, or any service
- Depend on a separate rules project — the predicate is local to this file
- Redeclare a property that `Soft{ValueObject}` already declares
- Be used to carry identity — use the entity `Id` for that

__Applied solutions:__
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]

# Check list
- [ ] Declared as `sealed record`, inherits from `Soft{ValueObject}`
- [ ] Constructor validates via a local `private static` predicate and throws `DomainException` on failure
- [ ] No public setters, no infrastructure dependencies, no separate rules-project dependency
- [ ] Multi-property VO has a `private` parameterless constructor

__Applied solutions:__
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]
