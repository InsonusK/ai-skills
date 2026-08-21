---
name: class-email
description: Class Email in the example plateau
whenToUse: when creating or editing Email, or creating another value object that plays the same role in a different module
domain: skill
type: template
plateau: example
version: 20260821120000
tags:
  - skill/template/class
  - plateau/example
created_by:
  - "[[../../../../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]]"
  - "[[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]]"
---

# Goal
- Reject an invalid email value at the point of construction.
- Call `EmailRule` rather than owning the format check locally, now that a second owner (`ChangeCustomerEmailCommandValidator`) needs the same condition.

__Applied solutions:__
- [[../../../../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]] - [[../../../../../solutions/solution-value-object.skill/Implementation/{Module}.Domain.csproj.extend/Email.cs.create.md|Email.cs.create]]
- [[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Domain.csproj.extend/Email.cs.extend.md|Email.cs.extend]]

# Core Principles
- Apply ONE plateau template per class
- Immutable, equal by value, not by reference
- Never assign `Value` before the condition it depends on has passed

__Applied solutions:__
- [[../../../../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]] - [[../../../../../solutions/solution-value-object.skill/Implementation/{Module}.Domain.csproj.extend/Email.cs.create.md|Email.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Value object | {ValueObject} | Email | {ValueObject}.cs | Email.cs |

# Implementation
```csharp
//Skill: class-email
//Plateau: example
//Version: 20260821120000

// {Module}.Domain/ValueObjects/Email.cs
public sealed record Email
{
    public string Value { get; }

    public Email(string value)
    {
        EmailRule.Check(value);
        Value = value;
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]] - [[../../../../../solutions/solution-value-object.skill/Implementation/{Module}.Domain.csproj.extend/Email.cs.create.md|Email.cs.create]]
- [[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Domain.csproj.extend/Email.cs.extend.md|Email.cs.extend]]

# Rules
MUST:
- Throw before assigning `Value` when the value is invalid
MUST NOT:
- Keep a local format predicate next to the call to `EmailRule.Check` — the local copy was removed when `solution-domain-rule` was applied

__Applied solutions:__
- [[../../../../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]] - [[../../../../../solutions/solution-value-object.skill/Implementation/{Module}.Domain.csproj.extend/Email.cs.create.md|Email.cs.create]]
- [[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Domain.csproj.extend/Email.cs.extend.md|Email.cs.extend]]

# Check list
- [ ] `Email` cannot be constructed with a value that fails `EmailRule.IsValid`
- [ ] Two `Email` instances with the same string value are equal
- [ ] `Email` contains no local format predicate of its own

__Applied solutions:__
- [[../../../../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]] - [[../../../../../solutions/solution-value-object.skill/Implementation/{Module}.Domain.csproj.extend/Email.cs.create.md|Email.cs.create]]
- [[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Domain.csproj.extend/Email.cs.extend.md|Email.cs.extend]]
