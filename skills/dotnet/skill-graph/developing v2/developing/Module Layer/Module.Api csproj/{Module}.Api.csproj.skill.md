---
uid:
name: module-api-csproj
description: HTTP adapter project for a bounded context module — thin MediatR endpoint controllers with no business logic.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/csproj
  - dotnet
  - api
  - module
triggers:
  - create {Module}.Api project
  - add api layer
  - implement controllers
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill]]"
extended_by:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill]]"
---

# Goal
- Expose HTTP endpoints as thin MediatR adapters for this module

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Api (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Api (.csproj) (extended)]]

# Core Principles
- Api is a thin adapter — no business logic, no domain rules
- Api references only its own Interfaces project for contracts
- Dependencies flow inward

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Api (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Api (.csproj) (extended)]]

# Structure

## Solution place
Defined in [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill]]
```
/src
  /Modules
    /{ModuleName}
      /{ModuleName}.Api
```

## Project Structure
```
/{ModuleName}.Api
  /Controllers
  {ModuleName}.Api.csproj
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Api (.csproj)]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Controllers | HTTP endpoint controllers | |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Api (.csproj)]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Handler implementations — belong to module Application
- Infrastructure implementations — belong to App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Api (.csproj)]]

## Allowed Dependencies
- {Module}.Interfaces (own module only)
- BuildingBlocks (for shared API utilities)

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Api (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Api (.csproj) (extended)]]

# Rules

MUST:
- Every endpoint dispatches exactly one MediatR command or query
- Api references only own Interfaces and BuildingBlocks

MUST NOT:
- Api reference Domain directly
- Api reference Application directly
- Api contain business logic, validation logic, or domain rules

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Api (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Api (.csproj) (extended)]]

# Anti-patterns
- Injecting a repository or DbContext into a controller — use MediatR dispatch only
- Writing business logic in a controller action — belongs in Domain
- Referencing Application project from Api — Api knows only Interfaces contracts

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Api (.csproj)]]

# Check list
- [ ] Api.csproj does not reference Domain
- [ ] Api.csproj does not reference Application
- [ ] Every controller action dispatches exactly one MediatR request
- [ ] No business logic in any controller

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Api (.csproj)]]
