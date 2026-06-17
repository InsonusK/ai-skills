---
uid:
name: module-application-csproj
description: Application orchestration project for a bounded context module — handlers, validators, and specifications. Coordinates use cases without containing business logic.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/csproj
  - dotnet
  - application
  - module
triggers:
  - create {Module}.Application project
  - add application layer
  - implement handlers
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill]]"
extended_by:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill]]"
---

# Goal
- Orchestrate use cases by connecting the API contract to the domain model
- Provide all command handlers, query handlers, validators, and specifications for this bounded context

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Application (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Application (.csproj) (extended)]]

# Core Principles
- Application coordinates — it never contains business logic
- Application knows its own Domain and its own Interfaces
- Application may reference other modules' Interfaces for cross-module dispatch
- Dependencies flow inward — outer layers depend on inner layers

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Application (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Application (.csproj) (extended)]]

# Structure

## Solution place
Defined in [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill]]
```
/src
  /Modules
    /{ModuleName}
      /{ModuleName}.Application
```

## Project Structure
```
/{ModuleName}.Application
  /Commands
  /Queries
  /Validators
  /Specifications
  {ModuleName}.Application.csproj
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Application (.csproj)]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Commands | Command handlers | |
| /Queries | Query handlers (single-module only) | |
| /Validators | Input validation | |
| /Specifications | Query specifications | |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Application (.csproj)]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Cross-module JOIN queries — belongs to App.Queries
- Infrastructure implementations — belongs to App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Application (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Queries (.csproj)]]

## Allowed Dependencies
- {Module}.Interfaces (own module)
- {Module}.Domain (own module)
- {OtherModule}.Interfaces (other modules — contracts only)
- Shared
- BuildingBlocks

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Application (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Application (.csproj) (extended)]]

# Rules

MUST:
- Application references only own Interfaces, own Domain
- Application references other modules' Interfaces only (never their Domain or Application)

MUST NOT:
- Application reference another module's Domain
- Application reference another module's Application
- Application reference App.Infrastructure
- Application reference App.Queries
- Application contain business logic — delegate to Domain

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Application (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Application (.csproj) (extended)]]

# Anti-patterns
- Calling another module's Application method directly — use MediatR dispatch through Interfaces
- Writing business rules in a handler — delegate to entity or domain service
- Cross-module JOIN logic in Application — belongs in App.Queries
- DbContext referenced from module Application — use IRepository from BuildingBlocks

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Application (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Queries (.csproj)]]

# Check list
- [ ] Application.csproj does not reference another module's Domain or Application
- [ ] Application.csproj does not reference App.Infrastructure or App.Queries
- [ ] No business logic in any handler class
- [ ] Cross-module dispatch uses MediatR through Interfaces only

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Application (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Application (.csproj) (extended)]]
