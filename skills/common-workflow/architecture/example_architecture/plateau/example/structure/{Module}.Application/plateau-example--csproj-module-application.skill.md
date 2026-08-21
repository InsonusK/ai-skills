---
name: csproj-module-application
description: Project {Module}.Application in the example plateau
whenToUse: when adding or editing a command, handler, or validator in {Module}.Application, or deciding whether new code belongs here
domain: skill
type: template
plateau: example
version: 20260821120000
tags:
  - skill/template/csproj
  - plateau/example
created_by:
  - "[[../../../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]]"
  - "[[../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]]"
  - "[[../../../../solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]]"
  - "[[../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]]"
---

# Goal
- Hold every command, handler, and validator for this module.
- Never re-implement inside a handler or validator a check the domain layer already enforces.

__Applied solutions:__
- [[../../../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]] - [[../../../../solutions/solution-module-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]] - [[../../../../solutions/solution-command-handler.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]

# Core Principles
- A handler orchestrates only: it loads state, constructs value objects, and calls one guarded domain method — it never decides validity itself.
- A validator checks input shape only; a business invariant is never duplicated here once it is already enforced in `{Module}.Domain`.

__Applied solutions:__
- [[../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]] - [[../../../../solutions/solution-command-handler.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[../../../../solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]] - [[../../../../solutions/solution-transport-validation.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{Module}.Application
```

## Project Structure
- /{Module}.Application
  - /Commands
    - [ChangeCustomerEmailCommand.cs](./classes/plateau-example--class-change-customer-email-command.skill.md)
    - [ChangeCustomerEmailHandler.cs](./classes/plateau-example--class-change-customer-email-handler.skill.md)
    - [ChangeCustomerEmailCommandValidator.cs](./classes/plateau-example--class-change-customer-email-command-validator.skill.md)
  - {Module}.Application.csproj

__Applied solutions:__
- [[../../../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]] - [[../../../../solutions/solution-module-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]] - [[../../../../solutions/solution-command-handler.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[../../../../solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]] - [[../../../../solutions/solution-transport-validation.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Commands | Write-operation records, handlers, and validators | |
| ChangeCustomerEmailCommand.cs | Immutable write-intent record | [[./classes/plateau-example--class-change-customer-email-command.skill.md\|plateau-example--class-change-customer-email-command]] |
| ChangeCustomerEmailHandler.cs | Loads `Customer`, calls its guarded method | [[./classes/plateau-example--class-change-customer-email-handler.skill.md\|plateau-example--class-change-customer-email-handler]] |
| ChangeCustomerEmailCommandValidator.cs | Transport-shape check on `NewEmail`; calls `EmailRule` | [[./classes/plateau-example--class-change-customer-email-command-validator.skill.md\|plateau-example--class-change-customer-email-command-validator]] |

__Applied solutions:__
- [[../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]] - [[../../../../solutions/solution-command-handler.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[../../../../solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]] - [[../../../../solutions/solution-transport-validation.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]

## Allowed Dependencies
- {Module}.Domain

__Applied solutions:__
- [[../../../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]] - [[../../../../solutions/solution-module-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]

# Rules
MUST:
- Reference `{Module}.Domain` and no other module's Domain project
- Register every validator via assembly scan, never manual construction
- Construct a value object before passing data into a domain method — never pass a raw primitive across the boundary
MUST NOT:
- Duplicate inside a handler or validator a check already enforced by `{Module}.Domain`

__Applied solutions:__
- [[../../../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]] - [[../../../../solutions/solution-module-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[../../../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]] - [[../../../../solutions/solution-command-handler.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[../../../../solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]] - [[../../../../solutions/solution-transport-validation.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
