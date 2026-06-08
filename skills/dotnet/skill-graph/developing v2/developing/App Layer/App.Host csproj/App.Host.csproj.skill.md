---
uid:
name: app-host-csproj
description: Composition root project — DI registration, pipeline wiring, module registration. The only project that references all others.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/csproj
  - dotnet
  - host
  - composition-root
triggers:
  - create App.Host project
  - add host layer
  - implement composition root
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill]]"
extended_by:
---

# Goal
- Be the single composition root — wire all modules, infrastructure, pipeline behaviors, and DI registrations together
- Be the only project that knows about all other projects simultaneously

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Host (.csproj)]]

# Core Principles
- App.Host references everything — it is the only project allowed to do so
- App.Host contains no business logic — only wiring
- Pipeline behaviors are registered once here — not inside individual modules

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Host (.csproj)]]

# Structure

## Solution place
```
/src
  /App
    /App.Host
```

## Project Structure
```
/App.Host
  /DependencyInjection
  Program.cs
  App.Host.csproj
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Host (.csproj)]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /DependencyInjection | DI registration classes | |
| Program.cs | Application entry point | |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Host (.csproj)]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Handler implementations — belong to module Application
- Infrastructure implementations — belong to App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Host (.csproj)]]

## Allowed Dependencies
- {ModuleName}.Api (all modules)
- {ModuleName}.Application (all modules — for registration methods)
- App.Infrastructure
- App.Queries
- BuildingBlocks

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Host (.csproj)]]

# Rules

MUST:
- Pipeline behaviors registered once here in correct order
- Each module registration method called here
- App.Host is the only project referencing all modules simultaneously

MUST NOT:
- App.Host contain business logic
- App.Host contain handler implementations

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Host (.csproj)]]

# Anti-patterns
- Business logic in App.Host — wiring only
- Pipeline behaviors registered inside module registration — register once in App.Host

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Host (.csproj)]]

# Check list
- [ ] All module registration methods called
- [ ] Pipeline behaviors registered in correct order
- [ ] No business logic in App.Host

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Host (.csproj)]]
