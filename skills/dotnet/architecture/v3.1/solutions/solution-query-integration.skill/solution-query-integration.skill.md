---
name: solution-query-integration
description: The repository-backed read side of MediatR (part of Persistence, VP2) — single-module query handlers in {Module}.Application using IReadRepository and named specs, cross-module query handlers in App.Queries using DbContext with AsNoTracking, and App.Queries DI. The IQuery<TResponse> marker and dispatch are common (solution-mediator-integration); this solution only adds handlers that read from a store.
whenToUse: when implementing a read operation that loads from the database — a single-module query handler over IReadRepository, or a cross-module projection/list query in App.Queries
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - stack/dotnet
  - application
  - cqrs
  - framework/mediatr
  - framework/ef-core
  - query
  - handler
  - read
  - dto
  - projection
  - concern/architecture
  - solution/query-integration

creates:
  - "{Module}.Application.Queries.{FeatureName}.{FeatureName}.Handler.cs"
  - App.Queries.csproj
  - App.Queries.AppQueriesRegistration.cs
  - App.Queries.Queries.{ModuleName}.{CrossModuleQueryHandler}.cs
extends:
  - "{Module}.Application.csproj"
  - App.Queries.csproj
  - App.Host.csproj
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
built_on_plateau:
---

> The `IQuery<TResponse>` marker and dispatch are **common** (`solution-mediator-integration`). This solution is VP2 — it adds the *repository-backed* query handlers, `App.Queries` cross-module read models, and `AppDbContext` reads. A module without persistence still has queries (answered in-memory or by dispatch); it just has no handlers from this solution.

# Goal
- Add the **repository-backed** read side, once a module has persistence (VP2): single-module query handlers over `IReadRepository<T>` + named specs, and cross-module read models in `App.Queries` over `AppDbContext`.
- Create `App.Queries` — the cross-module query project, referenced by `App.Host` and no one else.
- Define when to use a projection spec vs in-handler mapping — projection spec for flat DTOs, in-handler mapping for computed/conditional DTOs.
- Register `App.Queries` handlers via assembly scan in `App.Host`.

The `IQuery<TResponse>` marker, the query/DTO record conventions in `{Module}.Interfaces`, and MediatR dispatch are **not** defined here — they are common, from [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]. This solution only adds the handlers that read from a store.

# Capabilities
- Standardized query/DTO/handler placement
- Cross-module read support without breaking module boundaries
- Read-only enforcement via `IReadRepository<T>`
- Consistent `Result<T>`-based response contract for all read operations

# Core Principles
- Query handlers are strictly read-only — no entity mutation, no `SaveChangesAsync`, no `IRepository<T>`
- `ValidationBehavior` activates for queries — transport correctness is validated before the handler runs because queries implement `IRequest<TResponse>`
- Single-module handlers use `IReadRepository<T>` from Shared — never `DbContext`, never `IRepository<T>`
- Cross-module handlers live in `App.Queries` — the only layer with access to all module entity types simultaneously
- Cross-module handlers use DbContext directly with `AsNoTracking()` — no repository abstraction needed here
- All single-module entity loading goes through named specs — no inline LINQ in handlers
- DTOs are the only data shape that crosses module boundaries for read operations — never domain entities
- Query handlers may have transport validators — `ValidationBehavior` validates structural correctness before the handler runs
- DTO validators are owned by `solution-dto-property-validators.skill` and live in `{Module}.Application/Validators`
- Query validators reuse `IValidator<Soft{ValueObject}>` and `IValidator<{Dto}>` from `solution-dto-property-validators.skill` instead of duplicating cross-module validation rules

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - provides `IReadRepository<T>` and specification patterns
    - [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs]] - used by single-module handlers
  - [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.create.md|AppDbContext.cs]] - cross-module handlers reference this `DbContext` directly, with `AsNoTracking()`

NUGET:
- `MediatR` {version} - provides `IRequest<T>`, `IRequestHandler<TRequest, TResponse>`, `ISender`
- `Ardalis.Result` {version} - provides `Result<T>`, `Result.Success`, `Result.NotFound`
- `Microsoft.EntityFrameworkCore` {version} - provides `DbContext`, `AsNoTracking`, LINQ extensions used in App.Queries

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - extend - Add single-module query handler and optional transport validator in feature folder
  - [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs]] - create - Single-module query handler implementation
  - [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs]] - create - Optional transport validator for query input
- [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend.md|App.Queries.csproj]] - extend - Add cross-module query handlers and DI registration
  - [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create.md|AppQueriesRegistration.cs]] - create - App.Queries assembly scan registration
  - [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create.md|CrossModuleQueryHandler.cs]] - create - Cross-module JOIN query handler implementation
- [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Wire App.Queries registration into the composition root

# Rules

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/Implementation/App.Host.csproj.extend.md#MUST|App.Host.csproj]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend.md#MUST|App.Queries.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create.md#MUST|AppQueriesRegistration.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create.md#MUST|CrossModuleQueryHandler.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend.md#MUST|{Module}.Application.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md#MUST|{FeatureName}.Handler.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md#MUST|{FeatureName}.Validator.cs]]

## SHOULD
- Avoid `IRepository<T>` injected into query handler — use `IReadRepository<T>`
- Avoid cross-module JOIN in `{Module}.Application` — Application has no cross-module DB access
- Avoid single-module query implemented in App.Queries — unnecessary cross-module machinery
- Avoid inline LINQ in single-module handler — use named spec
- Avoid DTO returning domain entity directly — always project to DTO record
- Avoid query handler dispatching a command — queries are read-only
- Avoid duplicating Soft{ValueObject} or DTO validation rules in a query validator instead of using `IValidator<T>`

# Check list
- [ ] All queries declared as `record` implementing `IQuery<Result<T>>`
- [ ] All queries in `/{Module}.Interfaces/Queries`
- [ ] All DTOs declared as `record` in `/{Module}.Interfaces/DTOs`
- [ ] DTOs have no domain entity type properties
- [ ] Single-module handlers in `/{Module}.Application/Queries/{FeatureName}`
- [ ] Single-module handlers inject `IReadRepository<T>` — never `IRepository<T>` or DbContext
- [ ] Single-module handlers load via named specs — no inline LINQ
- [ ] Cross-module handlers in `/App.Queries/Queries/{QueryName}`
- [ ] Cross-module handlers inject `AppDbContext` directly
- [ ] Cross-module handlers apply `AsNoTracking()`
- [ ] Cross-module handlers do not use `Include()` — mapping done in handler
- [ ] `AppQueriesRegistration` defined in App.Queries
- [ ] `RegisterAppQueries()` called from App.Host
- [ ] Query transport validators (when present) check structural correctness only
- [ ] Query transport validators use `IValidator<Soft{ValueObject}>` for cross-module Soft VO properties via `SetValidator`
- [ ] Query transport validators use `IValidator<{Dto}>` for cross-module DTO properties via `SetValidator`
- [ ] Query validator does not duplicate rules already defined in `{ValueObject}PropertyValidator` or `{Dto}Validator`
- [ ] Query handlers return `Result.NotFound()` when entity is missing
- [ ] No `SaveChangesAsync` call in any query handler
