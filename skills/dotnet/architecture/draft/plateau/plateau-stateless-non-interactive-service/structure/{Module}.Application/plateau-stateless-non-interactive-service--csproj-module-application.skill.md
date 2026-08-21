---
name: csproj-module-application
description: Project {Module}.Application in the stateless-non-interactive-service plateau
whenToUse: when adding or editing a handler, validator, or specification in {Module}.Application, or deciding whether new code belongs here
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
- Orchestrate use cases by connecting the API contract to the domain model

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]

# Core Principles
- Application coordinates — it never contains business logic
- Application knows its own Domain and its own Interfaces
- Application may reference other modules' Interfaces for cross-module dispatch

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Application
```

## Project Structure
- /{Module}.Application
  - /Handlers
  - /Validators
  - /Specifications
  - {Module}.Application.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Handlers | Command and query handlers | |
| /Validators | Input validation | |
| /Specifications | Query specifications | |

## Allowed Dependencies
- {Module}.Interfaces (own module)
- {Module}.Domain (own module)
- {OtherModule}.Interfaces (other modules — contracts only)
- Shared

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]

# Rules
MUST:
- Application references only own Interfaces, own Domain
MUST NOT:
- Application reference another module's Domain or Application
- Application contain business logic — delegate to Domain

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]

# Check list
- [ ] Application.csproj does not reference another module's Domain or Application
- [ ] No business logic in any handler class

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
