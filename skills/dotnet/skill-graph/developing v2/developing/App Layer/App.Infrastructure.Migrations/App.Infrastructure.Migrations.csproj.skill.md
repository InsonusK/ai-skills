---
uid:
name: app-infrastructure-migrations-csproj
description: EF Core migrations project — contains all database migration files separated from the infrastructure implementation.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/csproj
  - dotnet
  - migrations
  - ef-core
triggers:
  - create App.Infrastructure.Migrations project
  - add migrations layer
  - implement ef migrations
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill]]"
extended_by:
---

# Goal
- Separate EF Core migration artifacts from the infrastructure implementation project

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure.Migrations (.csproj)]]

# Core Principles
- Migrations live in their own project to keep infrastructure clean
- This project references App.Infrastructure for the DbContext

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure.Migrations (.csproj)]]

# Structure

## Solution place
```
/src
  /App
    /App.Infrastructure.Migrations
```

## Project Structure
```
/App.Infrastructure.Migrations
  /Migrations
  App.Infrastructure.Migrations.csproj
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure.Migrations (.csproj)]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Migrations | EF Core migration files | |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure.Migrations (.csproj)]]

## Allowed Dependencies
- App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure.Migrations (.csproj)]]

# Rules

MUST:
- Reference App.Infrastructure for DbContext
- Contain only migration files

MUST NOT:
- Contain business logic
- Be referenced by any module

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure.Migrations (.csproj)]]

# Check list
- [ ] References App.Infrastructure only
- [ ] Contains only migration files

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure.Migrations (.csproj)]]
