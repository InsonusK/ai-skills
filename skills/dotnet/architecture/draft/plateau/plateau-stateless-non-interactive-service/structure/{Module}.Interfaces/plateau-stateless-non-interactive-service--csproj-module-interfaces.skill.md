---
name: csproj-module-interfaces
description: Project {Module}.Interfaces in the stateless-non-interactive-service plateau
whenToUse: when adding or editing a command, query, DTO, or event contract in {Module}.Interfaces, or deciding whether new code belongs here
domain: skill
type: template
plateau: stateless-non-interactive-service
version: 20260821120000
tags:
  - skill/template/csproj
  - plateau/stateless-non-interactive-service
created_by:
  - "[[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
---

# Goal
- Provide the single stable public surface through which other modules interact with this module
- Declare all write intent contracts (commands), read intent contracts (queries), response shapes (DTOs), and integration event contracts

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj.create]]

# Core Principles
- Interfaces is a declarations-only project — no business logic, no implementation

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj.create]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Interfaces
```

## Project Structure
- /{Module}.Interfaces
  - /Commands
  - /Queries
  - /DTOs
  - /Events
  - {Module}.Interfaces.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Commands | Write intent contracts | |
| /Queries | Read intent contracts | |
| /DTOs | Response shapes | |
| /Events | Integration event contracts | |

## Allowed Dependencies
- Shared

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj.create]]

# Rules
MUST:
- Interfaces contains only declarations — records, interfaces, DTOs
- All commands, queries, and integration events declared here
MUST NOT:
- Interfaces reference Domain, Application, or any infrastructure project
- Interfaces contain any implementation code

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj.create]]

# Check list
- [ ] /Commands, /Queries, /DTOs, /Events folders exist
- [ ] Interfaces.csproj references only Shared
- [ ] No implementation code in any file

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj.create]]
