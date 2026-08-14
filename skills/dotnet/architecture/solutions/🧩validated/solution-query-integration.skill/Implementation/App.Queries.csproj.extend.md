---
description: Add cross-module query handlers and DI registration to App.Queries
name: App.Queries.csproj
element_kind: project
change_kind: extend
---

# Goals
- Own cross-module JOIN query handler implementations — the only layer permitted to JOIN across module entity types
- Use DbContext directly with `AsNoTracking()` — no repository abstraction needed for cross-module reads

# Core Principles
- Cross-module handlers use DbContext directly — `IReadRepository<T>` is per-entity-type and cannot span a JOIN
- `AsNoTracking()` applied on every query — read-only, no tracking overhead
- Query contract declared in `{Module}.Interfaces/Queries` — App.Queries only implements it, never declares it
- Handlers registered in App.Host via a dedicated `App.Queries` assembly scan — not inside any module registration

# Structure

## Project Structure
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

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Queries/{QueryName} | Cross-module query handler and optional validator |
| /Specifications | Cross-module projection specs |
| AppQueriesRegistration.cs | DI registration for App.Queries assembly |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Provides `IRequestHandler<TRequest, TResponse>` |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides `DbContext`, `AsNoTracking()`, LINQ async extensions |
| `Ardalis.Result` | latest stable | Provides `Result<T>`, `Result.NotFound` |
| `Ardalis.Specification` | latest stable | Optional — for cross-module projection specs |

# Allowed Dependencies
- Shared — for `IQuery<T>` marker
- All `{Module}.Domain` projects — for entity types used in JOINs
- All `{Module}.Interfaces` projects — for query and DTO contracts
- App.Infrastructure — for `AppDbContext` (or shared DbContext location)

# Rules

## MUST
- All cross-module JOIN handlers live in `/App.Queries/Queries/{QueryName}/`
- Handlers use DbContext directly with `AsNoTracking()`
- Handlers registered via assembly scan in App.Host
- Query contract declared in owning module's Interfaces — App.Queries only implements

## MUST NOT
- Single-module queries live here — belongs in `{Module}.Application`
- App.Queries handlers modify entity state
- App.Queries handlers call `SaveChangesAsync`

# Anti-patterns
- Single-module query handler placed in App.Queries — adds unnecessary cross-module machinery
- Cross-module handler placed in `{Module}.Application` — Application has no access to other module's entity types

# Check list
- [ ] `/App.Queries/Queries/{QueryName}/` folder exists
- [ ] Each query has its own folder under `/App.Queries/Queries`
- [ ] Folder contains `.Handler.cs`; `.Validator.cs` is optional
- [ ] Cross-module handlers use `AppDbContext` directly
- [ ] `AsNoTracking()` applied on all queries
- [ ] `AppQueriesRegistration.cs` exists in App.Queries
- [ ] Query contracts declared in `{Module}.Interfaces`, not in App.Queries
