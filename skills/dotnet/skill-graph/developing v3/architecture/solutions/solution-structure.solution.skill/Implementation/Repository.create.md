---
description: Defines the full solution folder structure including modules, app layers, shared, and building blocks
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

## Directory and class skills
| `Directory\|file`                      | Description                                                    |
| -------------------------------------- | -------------------------------------------------------------- |
| /src/Modules                           | All bounded context modules                                    |
| /{ModuleName}                          | One folder per module                                          |
| /{ModuleName}.Api                      | HTTP endpoints, MediatR dispatch                               |
| /{ModuleName}.Application              | Orchestration — handlers, validators, specs                    |
| /{ModuleName}.Domain                   | Business logic — entities, VOs, rules, events                  |
| /{ModuleName}.Interfaces               | Public contracts — commands, queries, DTOs, events             |
| /src/App                               | Composition and infrastructure layer                           |
| /src/App/App.Host                      | Composition root — DI, pipeline, module wiring                 |
| /src/App/App.Infrastructure            | Persistence — DbContext, repos, outbox                         |
| /src/App/App.Infrastructure.Migrations | EF Core migrations only                                        |
| /src/App/App.Queries                   | Cross-module read models and JOIN queries                      |
| /src/Shared                            | Cross-cutting primitives — Result, Exceptions, base interfaces |
| /src/BuildingBlocks                    | Reusable framework patterns — pipeline behaviors, base specs   |

# Rules

MUST:
- Every module lives under `/src/Modules/{ModuleName}`
- Every module has exactly four projects: Api, Application, Domain, Interfaces
- Tests live alongside module projects — not in a global `/tests` folder

MUST NOT:
- Module projects exist outside `/src/Modules`
- Module have fewer or more than four projects without explicit architectural justification
