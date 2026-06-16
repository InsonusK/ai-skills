---
uid: eb0d94ce-ac79-4d31-ad62-14378feaa5f8
name: default-sln
description: Default plateau — full solution architecture composed from all validated v3 architecture solutions
domain: skill
type: template
version: 20260616
tags:
  - skill/template/sln
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]]"
---

# Structure

## Project Structure
```
/src
  /Modules
    /{ModuleName}
      /{ModuleName}.Api
      /{ModuleName}.Application
      /{ModuleName}.Domain
      /{ModuleName}.Interfaces
      /{ModuleName}.Api.Tests
      /{ModuleName}.Application.Tests
      /{ModuleName}.Domain.Tests
      /{ModuleName}.Integration.Tests
  /App
    /App.Host
    /App.Infrastructure
    /App.Infrastructure.Migrations
    /App.Queries
  /Shared
  /BuildingBlocks
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/Repository.create.md|Repository.create]]

## Directory and class skills
| `Directory\|file`              | template link                                                                                                                                                                                 | Description                                                    |
| ------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| /Shared                        | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/Shared/Shared.csproj.skill\|Shared.csproj.skill]]                                                                      | Cross-cutting primitives — Result, Exceptions, base interfaces |
| /BuildingBlocks                | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/BuildingBlocks.csproj.skill\|BuildingBlocks.csproj.skill]]                                              | Reusable framework patterns — pipeline behaviors, base specs   |
| /App.Host                      | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Host/App.Host.csproj.skill\|App.Host.csproj.skill]]                                                                | Composition root — DI, pipeline, module wiring                 |
| /App.Infrastructure            | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Infrastructure/App.Infrastructure.csproj.skill\|App.Infrastructure.csproj.skill]]                                  | Persistence — DbContext, repos, outbox                         |
| /App.Infrastructure.Migrations | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Infrastructure.Migrations/App.Infrastructure.Migrations.csproj.skill\|App.Infrastructure.Migrations.csproj.skill]] | EF Core migrations only                                        |
| /App.Queries                   | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Queries/App.Queries.csproj.skill\|App.Queries.csproj.skill]]                                                       | Cross-module read models and JOIN queries                      |
| /{Module}.Interfaces           | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Interfaces/{Module}.Interfaces.csproj.skill\|{Module}.Interfaces.csproj.skill]]                               | Public contracts — commands, queries, DTOs, events             |
| /{Module}.Domain               | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Domain/{Module}.Domain.csproj.skill\|{Module}.Domain.csproj.skill]]                                           | Business logic — entities, VOs, rules, events                  |
| /{Module}.Application          | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Application/{Module}.Application.csproj.skill\|{Module}.Application.csproj.skill]]                            | Orchestration — handlers, validators, specs                    |
| /{Module}.Api                  | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Api/{Module}.Api.csproj.skill\|{Module}.Api.csproj.skill]]                                                    | HTTP endpoints, MediatR dispatch                               |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/Repository.create.md|Repository.create]]

# Rules
MUST:
- Every module lives under `/src/Modules/{ModuleName}`
- Every module has exactly four projects: Api, Application, Domain, Interfaces
- Tests live alongside module projects — not in a global `/tests` folder

MUST NOT:
- Module projects exist outside `/src/Modules`
- Module have fewer or more than four projects without explicit architectural justification

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/Repository.create.md|Repository.create]]
