---
name: csproj-app-infrastructure-migrations
description: EF Core migrations only — separated from App.Infrastructure to keep the main infrastructure project clean
domain: skill
type: template
version: 20260616
plateau: default
tags:
  - skill/template/csproj
  - plateau/default
  - stack/dotnet
  - concern/architecture

created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
---

# Goal
- Keep EF Core migrations in a separate project from App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.Migrations.csproj.create|App.Infrastructure.Migrations.csproj]]

# Core Principles
- Migrations are the only code that directly references DbContext for schema changes
- This project is referenced only by App.Host at startup

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.Migrations.csproj.create|App.Infrastructure.Migrations.csproj]]

# Structure

## Solution place
```
/src/App/App.Infrastructure.Migrations
```


## Project Structure
- /App.Infrastructure.Migrations
  - App.Infrastructure.Migrations.csproj

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.Migrations.csproj.create|App.Infrastructure.Migrations.csproj]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Repository implementations — belong to App.Infrastructure
- Query handlers — belong to App.Queries or module Application

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.Migrations.csproj.create|App.Infrastructure.Migrations.csproj]]

## Allowed Dependencies
- App.Infrastructure
- {ModuleName}.Domain (all modules)

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.Migrations.csproj.create|App.Infrastructure.Migrations.csproj]]

# Rules
MUST:
	- Contain only EF Core migrations
MUST NOT:
	- Contain business logic
	- Be referenced by any module project

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.Migrations.csproj.create|App.Infrastructure.Migrations.csproj]]

# Check list
- [ ] Only migration classes present
- [ ] No business logic in any file

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.Migrations.csproj.create|App.Infrastructure.Migrations.csproj]]
