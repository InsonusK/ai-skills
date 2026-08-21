---
name: class-change-customer-email-handler
description: Class ChangeCustomerEmailHandler in the example plateau
whenToUse: when creating or editing ChangeCustomerEmailHandler, or creating another handler that plays the same role for a different command
domain: skill
type: template
plateau: example
version: 20260821120000
tags:
  - skill/template/class
  - plateau/example
created_by:
  - "[[../../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]]"
---

# Goal
- Load `Customer`, construct `Email`, and call the guarded domain method — deciding nothing itself.

__Applied solutions:__
- [[../../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]] - [[../../../../../solutions/solution-command-handler.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailHandler.cs.create.md|ChangeCustomerEmailHandler.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- Orchestrates only: load, construct, call — never validates a business rule itself

__Applied solutions:__
- [[../../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]] - [[../../../../../solutions/solution-command-handler.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailHandler.cs.create.md|ChangeCustomerEmailHandler.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Handler | {Verb}{Entity}{Property}Handler | ChangeCustomerEmailHandler | {Verb}{Entity}{Property}Handler.cs | ChangeCustomerEmailHandler.cs |

# Implementation
```csharp
//Skill: class-change-customer-email-handler
//Plateau: example
//Version: 20260821120000

// {Module}.Application/Commands/ChangeCustomerEmailHandler.cs
public class ChangeCustomerEmailHandler
{
    public void Handle(ChangeCustomerEmailCommand command)
    {
        var customer = Load(command.CustomerId);
        var email = new Email(command.NewEmail);
        customer.ChangeEmail(email);
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]] - [[../../../../../solutions/solution-command-handler.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailHandler.cs.create.md|ChangeCustomerEmailHandler.cs.create]]

# Rules
MUST:
- Construct `Email` before calling `ChangeEmail` — never pass a raw string into the domain layer
MUST NOT:
- Re-implement a check inside the handler that `Email` or `Customer` already enforces

__Applied solutions:__
- [[../../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]] - [[../../../../../solutions/solution-command-handler.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailHandler.cs.create.md|ChangeCustomerEmailHandler.cs.create]]

# Check list
- [ ] `ChangeCustomerEmailHandler` contains no `if` that duplicates a check already made by `Email` or `Customer`

__Applied solutions:__
- [[../../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]] - [[../../../../../solutions/solution-command-handler.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailHandler.cs.create.md|ChangeCustomerEmailHandler.cs.create]]
