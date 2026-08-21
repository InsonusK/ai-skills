---
name: class-change-customer-email-command
description: Class ChangeCustomerEmailCommand in the example plateau
whenToUse: when creating or editing ChangeCustomerEmailCommand, or creating another command record that plays the same role for a different write operation
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
- Carry the caller's intent to change a customer's email as an immutable record.

__Applied solutions:__
- [[../../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]] - [[../../../../../solutions/solution-command-handler.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommand.cs.create.md|ChangeCustomerEmailCommand.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- Immutable record — no logic beyond carrying data

__Applied solutions:__
- [[../../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]] - [[../../../../../solutions/solution-command-handler.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommand.cs.create.md|ChangeCustomerEmailCommand.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Command | {Verb}{Entity}{Property}Command | ChangeCustomerEmailCommand | {Verb}{Entity}{Property}Command.cs | ChangeCustomerEmailCommand.cs |

# Implementation
```csharp
//Skill: class-change-customer-email-command
//Plateau: example
//Version: 20260821120000

// {Module}.Application/Commands/ChangeCustomerEmailCommand.cs
public sealed record ChangeCustomerEmailCommand(int CustomerId, string NewEmail);
```

__Applied solutions:__
- [[../../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]] - [[../../../../../solutions/solution-command-handler.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommand.cs.create.md|ChangeCustomerEmailCommand.cs.create]]

# Check list
- [ ] `ChangeCustomerEmailCommand` is an immutable record with no methods

__Applied solutions:__
- [[../../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]] - [[../../../../../solutions/solution-command-handler.skill/Implementation/{Module}.Application.csproj.extend/ChangeCustomerEmailCommand.cs.create.md|ChangeCustomerEmailCommand.cs.create]]
