---
uid:
name: shared-csproj
description: Cross-cutting primitives project — Result, Exceptions, base interfaces used by all layers without creating coupling.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/csproj
  - dotnet
  - shared
  - primitives
triggers:
  - create Shared project
  - add shared layer
  - implement cross-cutting primitives
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill]]"
extended_by:
---

# Goal
- Provide cross-cutting primitives that every layer can safely depend on without creating coupling
- Define base types used across module and infrastructure boundaries

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#Shared (.csproj)]]

# Core Principles
- Shared has no business logic — only framework-level primitives
- Shared has no dependencies on any other project in this solution
- Any project at any layer may depend on Shared

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#Shared (.csproj)]]

# Structure

## Solution place
```
/src
  /Shared
```

## Project Structure
```
/Shared
  /Events
  /Exceptions
  Shared.csproj
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#Shared (.csproj)]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Events | Base event interfaces | |
| /Exceptions | Shared exception types used across layers | |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#Shared (.csproj)]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Pipeline behaviors — belongs to BuildingBlocks
- Infrastructure implementations — belongs to App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#Shared (.csproj)]]

## Allowed Dependencies
- None — Shared has no project dependencies

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#Shared (.csproj)]]

# Rules

MUST:
- Shared has zero project references
- All types in Shared are purely cross-cutting primitives

MUST NOT:
- Shared reference any module, BuildingBlocks, or infrastructure project
- Shared contain business logic or domain rules

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#Shared (.csproj)]]

# Anti-patterns
- Placing domain entities in Shared — they belong in module Domain
- Placing pipeline behaviors in Shared — they belong in BuildingBlocks
- Adding project references to Shared.csproj

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#Shared (.csproj)]]

# Check list
- [ ] Shared.csproj has no project references
- [ ] No business logic in any Shared class

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#Shared (.csproj)]]
