---
uid: 1622dc53-770d-4883-845d-d858864d3111
name: app-queries-csproj
description: Provide cross-module read model handlers that require JOIN queries across module boundaries
domain: skill
type: template
version: 20260616
tags:
  - skill/template/csproj
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure-solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration-solution.skill]]"
---

# Goal
- Provide cross-module read model handlers that require JOIN queries across module boundaries
- Be the only place where cross-module database joins are intentional and correct
- Own cross-module JOIN query handler implementations — the only layer permitted to JOIN across module entity types
- Use DbContext directly with `AsNoTracking()` — no repository abstraction needed for cross-module reads

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/App.Queries.csproj.create.md|App.Queries.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/App.Queries.csproj.extend.md|App.Queries.csproj.extend]]

# Core Principals
- App.Queries has direct DbContext access for cross-module JOINs
- Single-module queries belong in module Application — not here
- App.Queries implements query handlers declared in module Interfaces
- App.Queries references all module Domain projects — it is the only layer that may do this simultaneously
- Cross-module handlers use DbContext directly — `IReadRepository<T>` is per-entity-type and cannot span a JOIN
- `AsNoTracking()` applied on every query — read-only, no tracking overhead
- Query contract declared in `{Module}.Interfaces/Queries` — App.Queries only implements it, never declares it
- Handlers registered in App.Host via a dedicated `App.Queries` assembly scan — not inside any module registration

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/App.Queries.csproj.create.md|App.Queries.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/App.Queries.csproj.extend.md|App.Queries.csproj.extend]]

# Structure

## Solution place
```
/src/App/App.Queries
```


## Project Structure
```
/App.Queries
  /Queries
    /{ModuleName}
      GetTaskWithUserDetailsHandler.cs
  App.Queries.csproj
```

```
/App.Queries
  /Queries
    /GetTaskWithUserDetails
      GetTaskWithUserDetails.Handler.cs
    /GetOrderWithCustomer
      GetOrderWithCustomer.Handler.cs
      GetOrderWithCustomer.Validator.cs   ← optional transport validator
  /Specifications
    TaskWithUserDetailsSpec.cs        ← cross-module projection specs
  AppQueriesRegistration.cs
```

Each query lives in its own folder under `/Queries`. A folder may contain:
- Only the handler — when the query has no transport validation rules
- Handler and validator — when transport correctness validation is needed

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/App.Queries.csproj.create.md|App.Queries.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/App.Queries.csproj.extend.md|App.Queries.csproj.extend]]

## Directory and class skills
| `Directory|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
| /Queries/{ModuleName} | Cross-module query handlers grouped by primary module |  |
| /Queries/{QueryName} | Cross-module query handler and optional validator |  |
| /Specifications | Cross-module projection specs |  |
| AppQueriesRegistration.cs | DI registration for App.Queries assembly | [[skills/dotnet/architecture/plateau/default/App.Queries/classes/app-queries-registration-class.skill.md|AppQueriesRegistration.class.skill]] |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/App.Queries.csproj.create.md|App.Queries.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/App.Queries.csproj.extend.md|App.Queries.csproj.extend]]

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Provides `IRequestHandler<TRequest, TResponse>` |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides `DbContext`, `AsNoTracking()`, LINQ async extensions |
| `Ardalis.Result` | latest stable | Provides `Result<T>`, `Result.NotFound` |
| `Ardalis.Specification` | latest stable | Optional — for cross-module projection specs |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/App.Queries.csproj.create.md|App.Queries.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/App.Queries.csproj.extend.md|App.Queries.csproj.extend]]

## What Does NOT Belong Here
- Single-module queries — belong in module Application
- Write operations — belong in module Application handlers
- Business logic — belongs to Domain

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/App.Queries.csproj.create.md|App.Queries.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/App.Queries.csproj.extend.md|App.Queries.csproj.extend]]

## Allowed Dependencies
- App.Infrastructure (for DbContext access)
- {ModuleName}.Domain (all modules — for entity types in JOIN queries)
- {ModuleName}.Interfaces (all modules — for query and DTO contracts)
- Shared — for `IQuery<T>` marker
- All `{Module}.Domain` projects — for entity types used in JOINs
- All `{Module}.Interfaces` projects — for query and DTO contracts
- App.Infrastructure — for `AppDbContext` (or shared DbContext location)

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/App.Queries.csproj.create.md|App.Queries.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/App.Queries.csproj.extend.md|App.Queries.csproj.extend]]

# Rules
MUST:
	- Only cross-module JOIN queries live here
	- Query handlers here implement contracts declared in module Interfaces
	- All cross-module JOIN handlers live in `/App.Queries/Queries/{QueryName}/`
	- Handlers use DbContext directly with `AsNoTracking()`
	- Handlers registered via assembly scan in App.Host
	- Query contract declared in owning module's Interfaces — App.Queries only implements
MUST NOT:
	- App.Queries contain write operations
	- App.Queries contain business logic
	- Single-module queries be placed here
	- Single-module queries live here — belongs in `{Module}.Application`
	- App.Queries handlers modify entity state
	- App.Queries handlers call `SaveChangesAsync`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/App.Queries.csproj.create.md|App.Queries.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/App.Queries.csproj.extend.md|App.Queries.csproj.extend]]

# Anti-patterns
- Putting single-module queries in App.Queries — belongs in module Application
- Putting write operations in App.Queries — belongs in module Application
- Single-module query handler placed in App.Queries — adds unnecessary cross-module machinery
- Cross-module handler placed in `{Module}.Application` — Application has no access to other module's entity types

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/App.Queries.csproj.create.md|App.Queries.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/App.Queries.csproj.extend.md|App.Queries.csproj.extend]]

# Check list
- [ ] Only cross-module handlers present
- [ ] All handlers implement query contracts from module Interfaces
- [ ] No write operations in any handler
- [ ] `/App.Queries/Queries/{QueryName}/` folder exists
- [ ] Each query has its own folder under `/App.Queries/Queries`
- [ ] Folder contains `.Handler.cs`; `.Validator.cs` is optional
- [ ] Cross-module handlers use `AppDbContext` directly
- [ ] `AsNoTracking()` applied on all queries
- [ ] `AppQueriesRegistration.cs` exists in App.Queries
- [ ] Query contracts declared in `{Module}.Interfaces`, not in App.Queries

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/App.Queries.csproj.create.md|App.Queries.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/App.Queries.csproj.extend.md|App.Queries.csproj.extend]]
