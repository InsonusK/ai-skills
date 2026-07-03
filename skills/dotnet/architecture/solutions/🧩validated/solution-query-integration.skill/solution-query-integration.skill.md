---
name: solution-query-integration
description: Defines IQuery<TResponse> in Shared as the read-only operation marker, query records and DTOs in {Module}.Interfaces, single-module query handlers in {Module}.Application using IReadRepository and named specs, cross-module query handlers in App.Queries using DbContext directly with AsNoTracking, and App.Queries DI registration in App.Host
domain: skill
type: architecture
version: 20260611
tags:
  - skill/architecture/solution
  - dotnet
  - application
  - cqrs
  - mediatr
  - query
  - handler
  - read
  - dto
  - projection
triggers:
  - implement query handler
  - create query
  - write query handler
  - handle read operation
  - get entity
  - list entities
  - cross-module read
  - projection query
  - read-only operation
creates:
  - Shared.MediatR.IQuery.cs
  - "{Module}.Interfaces.Queries.{Query}.cs"
  - "{Module}.Interfaces.DTOs.{Dto}.cs"
  - "{Module}.Application.Queries.{FeatureName}.{FeatureName}.Handler.cs"
  - App.Queries.AppQueriesRegistration.cs
  - App.Queries.Queries.{ModuleName}.{CrossModuleQueryHandler}.cs
extends:
  - Shared.csproj
  - "{Module}.Interfaces.csproj"
  - "{Module}.Application.csproj"
  - App.Queries.csproj
  - App.Host.csproj
depends_on:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators.skill]]"
---

# Goal
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
- DTO validators are owned by `solution-soft-value-objects-and-dto-validators.skill` and live in `{Module}.Application/Validators`
- Query validators reuse `IValidator<Soft{ValueObject}>` and `IValidator<{Dto}>` from `solution-soft-value-objects-and-dto-validators.skill` instead of duplicating cross-module validation rules

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]] - hosts `IQuery<TResponse>` marker interface
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create|{Module}.Interfaces.csproj]] - hosts query records and DTOs
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create|{Module}.Application.csproj]] - hosts single-module query handlers
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Queries.csproj.create|App.Queries.csproj]] - hosts cross-module query handlers
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Host.csproj.create|App.Host.csproj]] - hosts App.Queries registration
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - provides `IReadRepository<T>` and specification patterns
    - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs]] - used by single-module handlers
- [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - provides `ValidationBehavior` that activates for any `IRequest<TResponse>` including queries
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - provides `{ValueObject}PropertyValidator` and `{Dto}Validator` that query validators reuse through `IValidator<T>`

NUGET:
- `MediatR` {version} - provides `IRequest<T>`, `IRequestHandler<TRequest, TResponse>`, `ISender`
- `Ardalis.Result` {version} - provides `Result<T>`, `Result.Success`, `Result.NotFound`
- `Microsoft.EntityFrameworkCore` {version} - provides `DbContext`, `AsNoTracking`, LINQ extensions used in App.Queries

# Template Skill Mutations

PROJECT:
- [[./Implementation/Shared.csproj.extend.md|Shared.csproj]] - extend - Add the `IQuery<TResponse>` marker interface
  - [[./Implementation/Shared.csproj.extend/IQuery.cs.create.md|IQuery.cs]] - create - Read-only operation marker interface
- [[./Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj]] - extend - Add query record conventions in `/Queries` and DTO shapes in `/DTOs`
  - [[./Implementation/{Module}.Interfaces.csproj.extend/{Query}.cs.create.md|{Query}.cs]] - create - Query record declaration
  - [[./Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md|{Dto}.cs]] - create - DTO response shape declaration
- [[./Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - extend - Add single-module query handler and optional transport validator in feature folder
  - [[./Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs]] - create - Single-module query handler implementation
  - [[./Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs]] - create - Optional transport validator for query input
- [[./Implementation/App.Queries.csproj.extend.md|App.Queries.csproj]] - extend - Add cross-module query handlers and DI registration
  - [[./Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create.md|AppQueriesRegistration.cs]] - create - App.Queries assembly scan registration
  - [[./Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create.md|CrossModuleQueryHandler.cs]] - create - Cross-module JOIN query handler implementation
- [[./Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Wire App.Queries registration into the composition root

# Rules

## MUST:
- [[./Implementation/App.Host.csproj.extend.md#MUST|App.Host.csproj.extend]]
- [[./Implementation/App.Queries.csproj.extend.md#MUST|App.Queries.csproj.extend]]
	- [[./Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create.md#MUST|AppQueriesRegistration.cs.create]]
	- [[./Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create.md#MUST|CrossModuleQueryHandler.cs.create]]
- [[./Implementation/Shared.csproj.extend.md#MUST|Shared.csproj.extend]]
	- [[./Implementation/Shared.csproj.extend/IQuery.cs.create.md#MUST|IQuery.cs.create]]
- [[./Implementation/{Module}.Application.csproj.extend.md#MUST|{Module}.Application.csproj.extend]]
	- [[./Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md#MUST|{FeatureName}.Handler.cs.create]]
	- [[./Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md#MUST|{FeatureName}.Validator.cs.create]]
- [[./Implementation/{Module}.Interfaces.csproj.extend.md#MUST|{Module}.Interfaces.csproj.extend]]
	- [[./Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md#MUST|{Dto}.cs.create]]
	- [[./Implementation/{Module}.Interfaces.csproj.extend/{Query}.cs.create.md#MUST|{Query}.cs.create]]
- Queries declared as `record` in `/{Module}.Interfaces/Queries`
- Single-module handlers in `/{Module}.Application/Queries` — inject `IReadRepository<T>`
- Single-module handlers load via named specs — no inline LINQ

## MUST NOT:
- [[./Implementation/App.Host.csproj.extend.md#MUST NOT|App.Host.csproj.extend]]
- [[./Implementation/App.Queries.csproj.extend.md#MUST NOT|App.Queries.csproj.extend]]
	- [[./Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create.md#MUST NOT|AppQueriesRegistration.cs.create]]
	- [[./Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create.md#MUST NOT|CrossModuleQueryHandler.cs.create]]
- [[./Implementation/Shared.csproj.extend.md#MUST NOT|Shared.csproj.extend]]
	- [[./Implementation/Shared.csproj.extend/IQuery.cs.create.md#MUST NOT|IQuery.cs.create]]
- [[./Implementation/{Module}.Application.csproj.extend.md#MUST NOT|{Module}.Application.csproj.extend]]
	- [[./Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md#MUST NOT|{FeatureName}.Handler.cs.create]]
	- [[./Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md#MUST NOT|{FeatureName}.Validator.cs.create]]
- [[./Implementation/{Module}.Interfaces.csproj.extend.md#MUST NOT|{Module}.Interfaces.csproj.extend]]
	- [[./Implementation/{Module}.Interfaces.csproj.extend/{Dto}.cs.create.md#MUST NOT|{Dto}.cs.create]]
	- [[./Implementation/{Module}.Interfaces.csproj.extend/{Query}.cs.create.md#MUST NOT|{Query}.cs.create]]
- Single-module handler use DbContext directly — use `IReadRepository<T>`

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
