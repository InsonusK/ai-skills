---
name: sln-example
description: Repository/solution-level layout of the example plateau
whenToUse: when adding, removing, or relocating a project in this example solution, or deciding which existing project a new class belongs to
domain: skill
type: template
plateau: example
version: 20260821120000
tags:
  - skill/template/sln
  - plateau/example
created_by:
  - "[[../../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]]"
---

# Structure

## Repository Structure
- /src/Modules/{ModuleName}
  - /[{Module}.Domain](./{Module}.Domain/plateau-example--csproj-module-domain.skill.md)
  - /[{Module}.Application](./{Module}.Application/plateau-example--csproj-module-application.skill.md)

__Applied solutions:__
- [[../../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]] - [[../../../solutions/solution-module-structure.skill/Implementation/Repository.create.md|Repository.create]]

## Directory and class skills
| `Directory\|file` | template link | Description |
| --- | --- | --- |
| /{Module}.Domain | [[./{Module}.Domain/plateau-example--csproj-module-domain.skill.md\|plateau-example--csproj-module-domain]] | Value objects, entities, and centralized rules |
| /{Module}.Application | [[./{Module}.Application/plateau-example--csproj-module-application.skill.md\|plateau-example--csproj-module-application]] | Commands, handlers, validators |

__Applied solutions:__
- [[../../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]] - [[../../../solutions/solution-module-structure.skill/Implementation/Repository.create.md|Repository.create]]

# Rules
MUST:
- Place every module under `/src/Modules/{ModuleName}`
- Keep `{Module}.Domain` free of any reference to `{Module}.Application`

__Applied solutions:__
- [[../../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]] - [[../../../solutions/solution-module-structure.skill/Implementation/Repository.create.md|Repository.create]]
