---
name: class-change-customer-email-command-validator
description: Class ChangeCustomerEmailCommandValidator in the example plateau
whenToUse: when creating or editing ChangeCustomerEmailCommandValidator, or creating another transport validator that plays the same role for a different command
domain: skill
type: template
plateau: example
version: 20260821120000
tags:
  - skill/template/class
  - plateau/example
created_by:
  - "[[../../../../../solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]]"
  - "[[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]]"
---

# Goal
- Reject a malformed `ChangeCustomerEmailCommand` before its handler runs.
- Call `EmailRule.IsValid` rather than owning the format check locally, now that `Email` also needs the same condition.

__Applied solutions:__
- [[../../../../../solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]] - [[../../../../../solutions/solution-transport-validation.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommandValidator.cs.create.md|ChangeCustomerEmailCommandValidator.cs.create]]
- [[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommandValidator.cs.extend.md|ChangeCustomerEmailCommandValidator.cs.extend]]

# Core Principles
- Apply ONE plateau template per class
- Checks input shape only — never a business decision that belongs to `Customer`

__Applied solutions:__
- [[../../../../../solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]] - [[../../../../../solutions/solution-transport-validation.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommandValidator.cs.create.md|ChangeCustomerEmailCommandValidator.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Command validator | {Command}Validator | ChangeCustomerEmailCommandValidator | {Command}Validator.cs | ChangeCustomerEmailCommandValidator.cs |

# Implementation
```csharp
//Skill: class-change-customer-email-command-validator
//Plateau: example
//Version: 20260821120000

// {Module}.Application/Commands/ChangeCustomerEmailCommandValidator.cs
public class ChangeCustomerEmailCommandValidator : AbstractValidator<ChangeCustomerEmailCommand>
{
    public ChangeCustomerEmailCommandValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .Must(EmailRule.IsValid)
            .WithMessage("Email must contain '@'.");
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]] - [[../../../../../solutions/solution-transport-validation.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommandValidator.cs.create.md|ChangeCustomerEmailCommandValidator.cs.create]]
- [[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommandValidator.cs.extend.md|ChangeCustomerEmailCommandValidator.cs.extend]]

# Rules
MUST:
- Register this validator via assembly scan, not manual construction
MUST NOT:
- Check anything about `Customer`'s current state
- Keep a local `.Must(...)` lambda next to the call to `EmailRule.IsValid` — the local copy was removed when `solution-domain-rule` was applied

__Applied solutions:__
- [[../../../../../solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]] - [[../../../../../solutions/solution-transport-validation.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommandValidator.cs.create.md|ChangeCustomerEmailCommandValidator.cs.create]]
- [[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommandValidator.cs.extend.md|ChangeCustomerEmailCommandValidator.cs.extend]]

# Check list
- [ ] `ChangeCustomerEmailCommandValidator` rejects an empty `NewEmail`
- [ ] `ChangeCustomerEmailCommandValidator` contains no inline format check of its own

__Applied solutions:__
- [[../../../../../solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]] - [[../../../../../solutions/solution-transport-validation.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommandValidator.cs.create.md|ChangeCustomerEmailCommandValidator.cs.create]]
- [[../../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommandValidator.cs.extend.md|ChangeCustomerEmailCommandValidator.cs.extend]]
