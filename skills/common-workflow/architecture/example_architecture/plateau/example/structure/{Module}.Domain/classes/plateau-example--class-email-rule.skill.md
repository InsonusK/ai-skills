---
name: class-email-rule
description: Class EmailRule in the example plateau
whenToUse: when creating or editing EmailRule, or creating another centralized rule that plays the same role for a different condition
domain: skill
type: template
plateau: example
version: 20260821120000
tags:
  - skill/template/class
  - plateau/example
created_by:
  - "[[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]]"
---

# Goal
- Give the "valid email format" condition exactly one place it is declared, callable unmodified from both `Email`'s constructor and `ChangeCustomerEmailCommandValidator`.

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Domain.csproj.extend/EmailRule.cs.create.md|EmailRule.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- `IsValid` returns `bool` only; `Check` wraps it with the throw
- No reference to `{Module}.Application` or to any entity

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Domain.csproj.extend/EmailRule.cs.create.md|EmailRule.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Centralized rule | {Condition}Rule | EmailRule | {Condition}Rule.cs | EmailRule.cs |

# Implementation
```csharp
//Skill: class-email-rule
//Plateau: example
//Version: 20260821120000

// {Module}.Domain/Rules/EmailRule.cs
public static class EmailRule
{
    public static bool IsValid(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Contains('@');

    public static void Check(string value)
    {
        if (!IsValid(value))
            throw new DomainException("{ModuleName}.Email.InvalidFormat", "Email must contain '@'.");
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Domain.csproj.extend/EmailRule.cs.create.md|EmailRule.cs.create]]

# Rules
MUST:
- Keep `IsValid` free of any throw — only `Check` throws
- Exist only once a condition is genuinely duplicated across two or more owners

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Domain.csproj.extend/EmailRule.cs.create.md|EmailRule.cs.create]]

# Check list
- [ ] `EmailRule.IsValid` never throws
- [ ] `EmailRule` has no reference to `{Module}.Application` or to any entity

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Domain.csproj.extend/EmailRule.cs.create.md|EmailRule.cs.create]]
