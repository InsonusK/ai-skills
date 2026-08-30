---
name: solution-query-integration
description: Defines IQuery<TResponse> in Shared as the read-only operation marker, query records and DTOs in {Module}.Interfaces, single-module query handlers in {Module}.Application using IReadRepository and named specs, cross-module query handlers in App.Queries using DbContext directly with AsNoTracking, and App.Queries DI registration in App.Host
whenToUse: when implementing a new read operation — declaring a query record and its handler, including a cross-module projection or list query
domain: skill
type: architecture
version: 20260611
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
  - Shared.IQuery.cs
  - "{Module}.Interfaces.Queries.{Query}.cs"
  - "{Module}.Interfaces.DTOs.{Dto}.cs"
  - "{Module}.Application.Queries.{FeatureName}.{FeatureName}.Handler.cs"
  - App.Queries.csproj
  - App.Queries.AppQueriesRegistration.cs
  - App.Queries.Queries.{ModuleName}.{CrossModuleQueryHandler}.cs
extends:
  - Shared.csproj
  - "{Module}.Interfaces.csproj"
  - "{Module}.Application.csproj"
  - App.Queries.csproj
  - App.Host.csproj
depends_on:
  - "[[skills/dotnet/architecture/v3/solutions/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]"
built_on_plateau: "[[skills/dotnet/architecture/v3/plateau/plateau-service-with-validated-module-interaction/plateau-service-with-validated-module-interaction.skill/plateau-service-with-validated-module-interaction.skill.md|plateau-service-with-validated-module-interaction]]"
---

# Goal
- Create `App.Queries` — the cross-module query project, referenced by App.Host and no one else — since nothing composed before this solution creates it
- Define `IQuery<TResponse>` in Shared as the marker that identifies read-only operations and keeps them distinct from write-side markers
- Define where Queries and DTOs are declared — as records in `/{Module}.Interfaces/Queries` and `/{Module}.Interfaces/DTOs`
- Define two handler locations: single-module handlers in `/{Module}.Application/Queries` using `IReadRepository<T>` and specs, cross-module handlers in `App.Queries` using DbContext directly
- Define when to use projection spec vs in-handler mapping — projection spec for flat DTOs, in-handler mapping for computed or conditional DTOs
- Register App.Queries handlers via assembly scan in App.Host

# Capabilities
- Clear distinction between read and write operations via `IQuery` marker
- Standardized query/DTO/handler placement
- Cross-module read support without breaking module boundaries
- Read-only enforcement via `IReadRepository<T>`
- Consistent `Result<T>`-based response contract for all read operations

# Core Principles
- `IQuery<TResponse>` lives in Shared — consistent placement of all `MediatR` markers
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
- [[skills/dotnet/architecture/v3/solutions/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]
  - [[skills/dotnet/architecture/v3/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - provides `IReadRepository<T>` and specification patterns
    - [[skills/dotnet/architecture/v3/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create|IReadRepository.cs]] - used by single-module handlers
  - [[skills/dotnet/architecture/v3/solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.create|AppDbContext.cs]] - cross-module handlers reference this `DbContext` directly, with `AsNoTracking()`

NUGET:
- `MediatR` {version} - provides `IRequest<T>`, `IRequestHandler<TRequest, TResponse>`, `ISender`
- `Ardalis.Result` {version} - provides `Result<T>`, `Result.Success`, `Result.NotFound`
- `Microsoft.EntityFrameworkCore` {version} - provides `DbContext`, `AsNoTracking`, LINQ extensions used in App.Queries

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - extend - Add the `IQuery<TResponse>` marker interface
  - [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/Shared.csproj.extend/IQuery.cs.create|IQuery.cs]] - create - Read-only operation marker interface
- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend|{Module}.Interfaces.csproj]] - extend - Add query record conventions in `/Queries` and DTO shapes in `/DTOs`
  - [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Query}.cs.create|{Query}.cs]] - create - Query record declaration
  - [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create|{Dto}.cs]] - create - DTO response shape declaration
- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend|{Module}.Application.csproj]] - extend - Add single-module query handler and optional transport validator in feature folder
  - [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create|{FeatureName}.Handler.cs]] - create - Single-module query handler implementation
  - [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create|{FeatureName}.Validator.cs]] - create - Optional transport validator for query input
- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend|App.Queries.csproj]] - extend - Add cross-module query handlers and DI registration
  - [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create|AppQueriesRegistration.cs]] - create - App.Queries assembly scan registration
  - [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create|CrossModuleQueryHandler.cs]] - create - Cross-module JOIN query handler implementation
- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - extend - Wire App.Queries registration into the composition root

# Rules

## MUST:
- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/App.Host.csproj.extend#MUST|App.Host.csproj]]
- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend#MUST|App.Queries.csproj]]
	- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create#MUST|AppQueriesRegistration.cs]]
	- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create#MUST|CrossModuleQueryHandler.cs]]
- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/Shared.csproj.extend#MUST|Shared.csproj]]
	- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/Shared.csproj.extend/IQuery.cs.create#MUST|IQuery.cs]]
- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend#MUST|{Module}.Application.csproj]]
	- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create#MUST|{FeatureName}.Handler.cs]]
	- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create#MUST|{FeatureName}.Validator.cs]]
- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend#MUST|{Module}.Interfaces.csproj]]
	- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create#MUST|{Dto}.cs]]
	- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Query}.cs.create#MUST|{Query}.cs]]

## MUST NOT:
- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/App.Host.csproj.extend#MUST NOT|App.Host.csproj]]
- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend#MUST NOT|App.Queries.csproj]]
	- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create#MUST NOT|AppQueriesRegistration.cs]]
	- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create#MUST NOT|CrossModuleQueryHandler.cs]]
- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/Shared.csproj.extend#MUST NOT|Shared.csproj]]
	- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/Shared.csproj.extend/IQuery.cs.create#MUST NOT|IQuery.cs]]
- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend#MUST NOT|{Module}.Application.csproj]]
	- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create#MUST NOT|{FeatureName}.Handler.cs]]
	- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create#MUST NOT|{FeatureName}.Validator.cs]]
- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend#MUST NOT|{Module}.Interfaces.csproj]]
	- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create#MUST NOT|{Dto}.cs]]
	- [[skills/dotnet/architecture/v3/solutions/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Query}.cs.create#MUST NOT|{Query}.cs]]

# Anti-patterns
- `IRepository<T>` injected into query handler — use `IReadRepository<T>`
- Cross-module JOIN in `{Module}.Application` — Application has no cross-module DB access
- Single-module query implemented in App.Queries — unnecessary cross-module machinery
- Inline LINQ in single-module handler — use named spec
- DTO returning domain entity directly — always project to DTO record
- Query handler dispatching a command — queries are read-only
- Duplicating Soft{ValueObject} or DTO validation rules in a query validator instead of using `IValidator<T>`

# Check list
- [ ] `IQuery<TResponse>` defined in `Shared/MediatR/IQuery.cs`
- [ ] `IQuery` does not extend `ICommand` — queries remain distinct from write-side markers
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
