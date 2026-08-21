---
name: csproj-module-domain
description: Project {Module}.Domain in the example plateau
whenToUse: when adding or editing a value object, entity, or rule in {Module}.Domain, or deciding whether new code belongs here
domain: skill
type: template
plateau: example
version: 20260821120000
tags:
  - skill/template/csproj
  - plateau/example
created_by:
  - "[[../../../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]]"
  - "[[../../../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]]"
  - "[[../../../../solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]]"
  - "[[../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]]"
---

# Goal
- Hold every value object, entity, and centralized rule for this module.
- Keep `{Module}.Domain` free of any reference to `{Module}.Application`.

__Applied solutions:__
- [[../../../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]] - [[../../../../solutions/solution-module-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]

# Core Principles
- A property has exactly one mutation point on its owning entity.
- A validation condition starts local to its first owner and moves into `/Rules` only once a second owner needs it (see [[../../../../solutions/solution-condition-ownership.skill/solution-condition-ownership.skill.md|solution-condition-ownership]]).

__Applied solutions:__
- [[../../../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]] - [[../../../../solutions/solution-value-object.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[../../../../solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]] - [[../../../../solutions/solution-entity-invariant.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{Module}.Domain
```

## Project Structure
- /{Module}.Domain
  - /ValueObjects
    - [Email.cs](./classes/plateau-example--class-email.skill.md)
  - /Entities
    - [Customer.cs](./classes/plateau-example--class-customer.skill.md)
  - /Rules
    - [EmailRule.cs](./classes/plateau-example--class-email-rule.skill.md)
  - {Module}.Domain.csproj

__Applied solutions:__
- [[../../../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]] - [[../../../../solutions/solution-module-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[../../../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]] - [[../../../../solutions/solution-value-object.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[../../../../solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]] - [[../../../../solutions/solution-entity-invariant.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /ValueObjects | Value objects | |
| Email.cs | Self-validating email value object; calls `EmailRule` | [[./classes/plateau-example--class-email.skill.md\|plateau-example--class-email]] |
| /Entities | Entities | |
| Customer.cs | Entity with a guarded `ChangeEmail` mutation | [[./classes/plateau-example--class-customer.skill.md\|plateau-example--class-customer]] |
| /Rules | Centralized, cross-owner conditions | |
| EmailRule.cs | The centralized "valid email format" condition | [[./classes/plateau-example--class-email-rule.skill.md\|plateau-example--class-email-rule]] |

__Applied solutions:__
- [[../../../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]] - [[../../../../solutions/solution-value-object.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[../../../../solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]] - [[../../../../solutions/solution-entity-invariant.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

## Allowed Dependencies
- None. `{Module}.Domain` references no other project.

__Applied solutions:__
- [[../../../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]] - [[../../../../solutions/solution-module-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]

# Rules
MUST:
- Add every new value object under `/ValueObjects`, every new entity under `/Entities`, every centralized rule under `/Rules`
- Validate before assigning, in every method that changes state
- Apply a rule under `/Rules` only after it is genuinely duplicated across two or more owners — never speculatively
MUST NOT:
- Reference `{Module}.Application` from anywhere in `{Module}.Domain`
- Leave a local copy of a condition next to a call to its centralized `/Rules` counterpart

__Applied solutions:__
- [[../../../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]] - [[../../../../solutions/solution-module-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[../../../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]] - [[../../../../solutions/solution-value-object.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[../../../../solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]] - [[../../../../solutions/solution-entity-invariant.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
- [[../../../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - [[../../../../solutions/solution-domain-rule.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]
