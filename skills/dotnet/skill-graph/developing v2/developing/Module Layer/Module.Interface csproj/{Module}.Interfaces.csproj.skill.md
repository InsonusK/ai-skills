---
uid:
name: module-interfaces-csproj
description: Public contracts project for a bounded context module — commands, queries, DTOs, and integration events. Declarations-only with no implementation.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/csproj
  - dotnet
  - interfaces
  - module
triggers:
  - create {Module}.Interfaces project
  - add interfaces layer
  - implement public contracts
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill]]"
extended_by:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill]]"
---

# Goal
- Provide the single stable public surface through which other modules interact with this module
- Declare all write intent contracts (commands), read intent contracts (queries), response shapes (DTOs), and integration event contracts

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Interfaces (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Interface (.csproj) (extended)]]

# Core Principles
- Interfaces is a declarations-only project — no business logic, no implementation
- Changes to Interfaces are breaking changes and must be versioned
- Other modules depend on this project only — never on Application or Domain
- Shared has no business logic — only framework-level primitives

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Interfaces (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Interface (.csproj) (extended)]]

# Structure

## Solution place
Defined in [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill]]
```
/src
  /Modules
    /{ModuleName}
      /{ModuleName}.Interfaces
```

## Project Structure
```
/{ModuleName}.Interfaces
  /Commands
  /Queries
  /DTOs
  /Events
  {ModuleName}.Interfaces.csproj
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Interfaces (.csproj)]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Commands | Write intent contracts | |
| /Queries | Read intent contracts | |
| /DTOs | Response shapes | |
| /Events | Integration event contracts | |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Interfaces (.csproj)]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Implementation — belongs to Application
- Infrastructure concerns — belongs to App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Interfaces (.csproj)]]

## Allowed Dependencies
- Shared

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Interfaces (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Interface (.csproj) (extended)]]

# Rules

MUST:
- Interfaces contains only declarations — records, interfaces, DTOs
- All commands declared here
- All queries declared here
- All integration events declared here

MUST NOT:
- Interfaces reference Domain, Application, or any infrastructure project
- Interfaces contain any implementation code

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Interfaces (.csproj)]]

# Anti-patterns
- Placing command handlers in Interfaces — handlers belong in Application
- Placing domain entities in Interfaces — use DTOs for cross-module data shapes
- Referencing another module's Domain from Interfaces

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Interfaces (.csproj)]]

# Check list
- [ ] /Commands folder exists
- [ ] /Queries folder exists
- [ ] /DTOs folder exists
- [ ] /Events folder exists
- [ ] No project references beyond Shared in Interfaces.csproj
- [ ] No implementation code in any file

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Interfaces (.csproj)]]
