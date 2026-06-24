---
uid: 2b7d4e9f-8c3a-4d1b-a6f0-d5e9c2b8a1f3
name: query-integration-solution
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
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure-solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration-solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/validation-behavior-solution.skill/validation-behavior-solution.skill.md|validation-behavior-solution.skill]]"
---

# Goal
- Define `IQuery<TResponse>` in Shared as the marker that identifies read-only operations and keeps them distinct from write-side markers
- Define where Queries and DTOs are declared — as records in `/{Module}.Interfaces/Queries` and `/{Module}.Interfaces/DTOs`
- Define two handler locations: single-module handlers in `/{Module}.Application/Queries` using `IReadRepository<T>` and specs, cross-module handlers in `App.Queries` using DbContext directly
- Define when to use projection spec vs in-handler mapping — projection spec for flat DTOs, in-handler mapping for computed or conditional DTOs
- Register App.Queries handlers via assembly scan in App.Host

# Core Principles
- `IQuery<TResponse>` lives in Shared — consistent placement of all MediatR markers
- Query handlers are strictly read-only — no entity mutation, no `SaveChangesAsync`, no `IRepository<T>`
- `ValidationBehavior` activates for queries — transport correctness is validated before the handler runs because queries implement `IRequest<TResponse>`
- Single-module handlers use `IReadRepository<T>` from Shared — never DbContext, never `IRepository<T>`
- Cross-module handlers live in `App.Queries` — the only layer with access to all module entity types simultaneously
- Cross-module handlers use DbContext directly with `AsNoTracking()` — no repository abstraction needed here
- All single-module entity loading goes through named specs — no inline LINQ in handlers
- DTOs are the only data shape that crosses module boundaries for read operations — never domain entities
- Query handlers may have transport validators — `ValidationBehavior` validates structural correctness before the handler runs

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure-solution.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/Shared.csproj.create.md|Shared.csproj]] - hosts `IQuery<TResponse>` marker interface
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj]] - hosts query records and DTOs
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj]] - hosts single-module query handlers
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/App.Queries.csproj.create.md|App.Queries.csproj]] - hosts cross-module query handlers
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj]] - hosts App.Queries registration
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration-solution.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - provides `IReadRepository<T>` and specification patterns
    - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs]] - used by single-module handlers
- [[skills/dotnet/architecture/solutions/🧩validated/validation-behavior-solution.skill/validation-behavior-solution.skill.md|validation-behavior-solution.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/validation-behavior-solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - provides `ValidationBehavior` that activates for any `IRequest<TResponse>` including queries

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
- [[./Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - extend - Add single-module query handler in feature folder
  - [[./Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs]] - create - Single-module query handler implementation
- [[./Implementation/App.Queries.csproj.extend.md|App.Queries.csproj]] - extend - Add cross-module query handlers and DI registration
  - [[./Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create.md|AppQueriesRegistration.cs]] - create - App.Queries assembly scan registration
  - [[./Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create.md|CrossModuleQueryHandler.cs]] - create - Cross-module JOIN query handler implementation
- [[./Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Wire App.Queries registration into the composition root

# Rules

MUST:
- `IQuery<TResponse>` defined in Shared — not BuildingBlocks, not any module
- `IQuery` does not extend `ICommand` — queries are read-only operations and must remain distinct from write-side markers
- All queries implement `IQuery<Result<T>>` — not `IRequest<T>` directly
- Queries declared as `record` in `/{Module}.Interfaces/Queries`
- DTOs declared as `record` in `/{Module}.Interfaces/DTOs`
- Single-module handlers in `/{Module}.Application/Queries` — inject `IReadRepository<T>`
- Single-module handlers load via named specs — no inline LINQ
- Cross-module handlers in `/App.Queries/Queries/{QueryName}` — inject DbContext directly
- Cross-module handlers apply `AsNoTracking()` on all queries
- App.Queries handlers registered via `RegisterAppQueries()` assembly scan in App.Host
- Query handlers return `Result.NotFound()` when entity is missing
- `RegisterAppQueries()` called from App.Host — after all module registrations

MUST NOT:
- Query handler inject `IRepository<T>` — signals write intent, use `IReadRepository<T>`
- Query handler inject `IUnitOfWork` or call `SaveChangesAsync`
- Query handler modify entity state or dispatch commands
- Single-module handler use DbContext directly — use `IReadRepository<T>`
- Cross-module handler live in `{Module}.Application` — Application has no multi-module DB access
- DTOs expose domain entity types
- Query handlers may have transport validators — `ValidationBehavior` validates structural correctness before the handler runs
- `IQuery` extend `ICommand` — queries must remain distinct from write-side markers
- Cross-module handlers do not use `Include()` — all mapping is done in handler via `Select()` or manual projection

SHOULD:
- Use projection spec when DTO maps directly from entity fields — avoids loading full entity
- Use in-handler mapping when DTO requires computed fields, conditional logic, or nested structure

# Anti-patterns
- `IRepository<T>` injected into query handler — use `IReadRepository<T>`
- Cross-module JOIN in `{Module}.Application` — Application has no cross-module DB access
- Single-module query implemented in App.Queries — unnecessary cross-module machinery
- Inline LINQ in single-module handler — use named spec
- DTO returning domain entity directly — always project to DTO record
- Query handler dispatching a command — queries are read-only

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
- [ ] Query handlers return `Result.NotFound()` when entity is missing
- [ ] No `SaveChangesAsync` call in any query handler

# Unittest TestCases
- [ ] When entity exists Then single-module handler returns `Result.Success` with correct DTO fields
- [ ] When entity not found Then single-module handler returns `Result.NotFound`
- [ ] When collection query runs Then all matching entities returned as DTOs
- [ ] When projection spec used Then DTO fields correctly mapped without loading full entity
- [ ] When in-handler mapping used Then computed fields correctly populated in DTO
- [ ] When cross-module query runs Then data from both modules correctly joined in single DTO
- [ ] When cross-module entity not found Then handler returns `Result.NotFound`
- [ ] When query with invalid transport data is sent Then `ValidationBehavior` returns `Result.Invalid` before handler runs
- [ ] When query with all valid fields is sent Then handler executes normally
- [ ] When App.Queries assembly scanned Then all cross-module handlers discovered by MediatR
