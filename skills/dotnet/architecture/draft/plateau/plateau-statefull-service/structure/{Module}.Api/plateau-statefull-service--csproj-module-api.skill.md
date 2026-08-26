---
name: plateau-statefull-service--csproj-module-api
description: Project {Module}.Api in the statefull-service plateau
whenToUse: when adding or editing an HTTP endpoint in {Module}.Api, or deciding whether new code belongs here
domain: skill
type: template
plateau: statefull-service
version: 20260824100000
tags:
  - skill/template/csproj
  - plateau/statefull-service
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
---

# Goal
- Expose HTTP endpoints as thin MediatR adapters for this module

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]

# Core Principles
- Api is a thin adapter — no business logic, no domain rules
- Api references only its own Interfaces project for contracts

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Api
```

## Project Structure
- /{Module}.Api
  - /Controllers
  - {Module}.Api.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Controllers | HTTP endpoints | |

## Allowed Dependencies
- {Module}.Interfaces (own module only)
- Shared

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]

# Rules
MUST:
- Every endpoint dispatches exactly one MediatR command or query
- Api references only own Interfaces and BuildingBlocks
MUST NOT:
- Api reference Domain or Application directly
- Api contain business logic, validation logic, or domain rules

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]

# Check list
- [ ] Api.csproj does not reference Domain or Application
- [ ] Every controller action dispatches exactly one MediatR request
- [ ] No business logic in any controller

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
