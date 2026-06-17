---
uid: 2a2570c1-3d97-43e7-bdc0-420f9a823c29
name: app-infrastructure-migrations-csproj
description: EF Core migrations only — separated from App.Infrastructure to keep the main infrastructure project clean
domain: skill
type: template
version: 20260616
tags:
  - skill/template/csproj
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
---

# Goal
- Keep EF Core migrations in a separate project from App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.Migrations.csproj.create.md|App.Infrastructure.Migrations.csproj.create]]

# Core Principals
- Migrations are the only code that directly references DbContext for schema changes
- This project is referenced only by App.Host at startup

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.Migrations.csproj.create.md|App.Infrastructure.Migrations.csproj.create]]

# Structure

## Solution place
```
/src/App/App.Infrastructure.Migrations
```


## Project Structure
```
/App.Infrastructure.Migrations
  App.Infrastructure.Migrations.csproj
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.Migrations.csproj.create.md|App.Infrastructure.Migrations.csproj.create]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Repository implementations — belong to App.Infrastructure
- Query handlers — belong to App.Queries or module Application

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.Migrations.csproj.create.md|App.Infrastructure.Migrations.csproj.create]]

## Allowed Dependencies
- App.Infrastructure
- {ModuleName}.Domain (all modules)

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.Migrations.csproj.create.md|App.Infrastructure.Migrations.csproj.create]]

# Rules
MUST:
	- Contain only EF Core migrations
MUST NOT:
	- Contain business logic
	- Be referenced by any module project

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.Migrations.csproj.create.md|App.Infrastructure.Migrations.csproj.create]]

# Check list
- [ ] Only migration classes present
- [ ] No business logic in any file

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.Migrations.csproj.create.md|App.Infrastructure.Migrations.csproj.create]]
