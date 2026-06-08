---
uid:
name: app-infrastructure-csproj
description: Persistence implementation project — DbContext, repositories, outbox, EF configurations. The only layer that knows EF Core details.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/csproj
  - dotnet
  - infrastructure
  - persistence
triggers:
  - create App.Infrastructure project
  - add infrastructure layer
  - implement persistence
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill]]"
extended_by:
aliases:
  - App.Infrastructure (.csproj)
---
# Goal
- Provide all persistence implementation — DbContext, repository implementations, outbox interceptor, background dispatcher
- Be the only layer that knows EF Core implementation details

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure (.csproj)]]

# Core Principles
- App.Infrastructure is the only project with a concrete DbContext
- App.Infrastructure implements interfaces defined in BuildingBlocks
- No module Application or Domain layer references App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure (.csproj)]]

# Structure

## Solution place
```
/src
  /App
    /App.Infrastructure
```

## Project Structure
```
/App.Infrastructure
  /Persistence
    /Configurations
  /Repositories
  /UnitOfWork
  /Outbox
  /Concurrency
  App.Infrastructure.csproj
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure (.csproj)]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Persistence | DbContext and EF configurations | |
| /Repositories | Generic Repository<T> implementation | |
| /UnitOfWork | UnitOfWork implementation | |
| /Outbox | EF interceptor and background dispatcher | |
| /Concurrency | EntityVersionResolver mapping strings to types | |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure (.csproj)]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Pipeline behavior registration — belongs to App.Host
- Cross-module JOIN queries — belongs to App.Queries

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure (.csproj)]]

## Allowed Dependencies
- BuildingBlocks
- Shared
- {ModuleName}.Domain (all modules)
- {ModuleName}.Interfaces (all modules)

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure (.csproj)]]

# Rules

MUST:
- App.Infrastructure is the only project with DbContext
- Repository<T> generic implementation registered here
- DomainEventInterceptor registered on DbContext here

MUST NOT:
- App.Infrastructure be referenced by any module Application, Domain, or Api
- App.Infrastructure be referenced by App.Queries directly for DbContext

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure (.csproj)]]

# Anti-patterns
- Module Application referencing App.Infrastructure — use repository abstractions from BuildingBlocks
- Putting cross-module JOIN queries in App.Infrastructure — belongs in App.Queries

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure (.csproj)]]

# Check list
- [ ] AppDbContext defined here
- [ ] Generic Repository<T> implemented and registered
- [ ] DomainEventInterceptor registered on DbContext
- [ ] No module Application references this project

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure (.csproj)]]
