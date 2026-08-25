---
name: csproj-app-queries
description: Project App.Queries in the v1 plateau
whenToUse: when a read operation needs to JOIN entity types across two or more modules, or deciding whether new query code belongs here
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/csproj
  - plateau/v1
created_by:
  - "[[../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]]"
---

# Goal
- Create `App.Queries` — the only project with access to every module's entity types simultaneously, since nothing before this solution creates it
- Own cross-module JOIN query handlers, using `AppDbContext` directly with `AsNoTracking()`

__Applied solutions:__
- [[../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[../../../../solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend.md|App.Queries.csproj.extend]]

# Core Principles
- Single-module reads never live here — they belong in `{Module}.Application/Queries`, using `IReadRepository<T>`
- Cross-module handlers use `AppDbContext` directly — `IReadRepository<T>` is per-entity-type and cannot span a JOIN
- Query contract declared in the owning module's `{Module}.Interfaces` — App.Queries only implements it

# Structure

## Project Structure
- /App.Queries
  - /Queries
    - /{QueryName}
      - [{QueryName}.Handler.cs](./classes/plateau-v1--class-cross-module-query-handler.skill.md)
  - /Specifications
    - {CrossModule}Spec.cs (optional, `Specification<T, TResult>` projection specs)
  - [AppQueriesRegistration.cs](./classes/plateau-v1--class-app-queries-registration.skill.md)
  - App.Queries.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Queries/{QueryName} | One cross-module handler (+ optional transport validator) per folder | [[./classes/plateau-v1--class-cross-module-query-handler.skill.md\|class-cross-module-query-handler]] |
| /Specifications | Optional cross-module projection specs | |
| AppQueriesRegistration.cs | DI registration for the App.Queries assembly | [[./classes/plateau-v1--class-app-queries-registration.skill.md\|class-app-queries-registration]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | `IRequestHandler<TRequest, TResponse>` |
| `Microsoft.EntityFrameworkCore` | latest stable | `AppDbContext`, `AsNoTracking()` |
| `Ardalis.Result` | latest stable | `Result<T>`, `Result.NotFound` |

## Allowed Dependencies
- Shared — for `IQuery<T>`
- All `{ModuleName}.Domain` projects — entity types used in JOINs
- All `{ModuleName}.Interfaces` projects — query and DTO contracts
- App.Infrastructure — for `AppDbContext`

# Rules
MUST:
- All cross-module JOIN handlers live in `/App.Queries/Queries/{QueryName}/`
- Handlers use `AppDbContext` directly with `AsNoTracking()`
- Registered via assembly scan, called last from `ModuleRegistration.AddModules()`
MUST NOT:
- Host a single-module query — belongs in `{Module}.Application`
- Mutate entity state or call `SaveChangesAsync`

# Check list
- [ ] `/App.Queries/Queries/{QueryName}/` folder exists per cross-module query
- [ ] `AsNoTracking()` applied on every query
- [ ] `AppQueriesRegistration` registered last in `AddModules()`

__Applied solutions:__
- [[../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[../../../../solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend.md|App.Queries.csproj.extend]]
