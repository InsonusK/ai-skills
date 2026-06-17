---
uid:
name: app-queries-csproj
description: Cross-module read model project — JOIN query handlers across module boundaries. The only place where cross-module database joins are intentional.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/csproj
  - dotnet
  - queries
  - read-models
triggers:
  - create App.Queries project
  - add queries layer
  - implement cross-module reads
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill]]"
extended_by:
---

# Goal
- Provide cross-module read model handlers that require JOIN queries across module boundaries
- Be the only place where cross-module database joins are intentional and correct

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Queries (.csproj)]]

# Core Principles
- App.Queries has direct DbContext access for cross-module JOINs
- Single-module queries belong in module Application — not here
- App.Queries implements query handlers declared in module Interfaces

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Queries (.csproj)]]

# Structure

## Solution place
```
/src
  /App
    /App.Queries
```

## Project Structure
```
/App.Queries
  /Queries
    /{ModuleName}
  App.Queries.csproj
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Queries (.csproj)]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Queries/{ModuleName} | Cross-module query handlers grouped by primary module | |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Queries (.csproj)]]

## What Does NOT Belong Here
- Single-module queries — belong in module Application
- Write operations — belong in module Application handlers
- Business logic — belongs to Domain

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Queries (.csproj)]]

## Allowed Dependencies
- App.Infrastructure (for DbContext access)
- {ModuleName}.Domain (all modules — for entity types in JOIN queries)
- {ModuleName}.Interfaces (all modules — for query and DTO contracts)

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Queries (.csproj)]]

# Rules

MUST:
- Only cross-module JOIN queries live here
- Query handlers here implement contracts declared in module Interfaces

MUST NOT:
- App.Queries contain write operations
- App.Queries contain business logic
- Single-module queries be placed here

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Queries (.csproj)]]

# Anti-patterns
- Cross-module JOIN in module Application — belongs in App.Queries

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Queries (.csproj)]]

# Check list
- [ ] Only cross-module handlers present
- [ ] All handlers implement query contracts from module Interfaces
- [ ] No write operations in any handler

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Queries (.csproj)]]
