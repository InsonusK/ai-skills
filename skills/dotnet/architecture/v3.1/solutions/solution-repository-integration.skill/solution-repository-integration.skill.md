---
name: solution-repository-integration
description: Merges the Ardalis Specification pattern with repository abstractions — defines IReadRepository<T> and IRepository<T> as thin wrappers around Ardalis base interfaces in Shared, provides a generic Repository<T> implementation in App.Infrastructure inheriting Ardalis.RepositoryBase<T>, and governs specification placement across Application and App.Queries
whenToUse: when a handler needs to load, filter, or project a persisted entity — defining IRepository/IReadRepository usage or writing an Ardalis specification
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - stack/dotnet
  - domain
  - application
  - infrastructure
  - repository
  - ardalis
  - specification
  - query
  - framework/ef-core
  - concern/architecture
  - solution/repository-integration

creates:
  - Shared.Repositories.IReadRepository.cs
  - Shared.Repositories.IRepository.cs
  - App.Infrastructure.Persistence.AppDbContext.cs
  - App.Infrastructure.Repositories.Repository.cs
  - App.Host.DependencyInjection.RepositoryRegistration.cs
  - "{Module}.Application.Specifications.{Entity}ByIdSpec.cs"
  - "{Module}.Application.Specifications.{Entity}SummarySpec.cs"
extends:
  - Shared.csproj
  - App.Infrastructure.csproj
  - App.Host.csproj
  - "{Module}.Application.csproj"
  - "{Module}.Domain.csproj"
  - "{Module}.Application.Validators.Async.{Feature}Check.cs"
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]"
built_on_plateau:
---
 # Goal
- Decouple Application layer handlers from EF Core and DbContext by providing repository abstractions powered by Ardalis
- Define `IReadRepository<T>` and `IRepository<T>` in Shared as thin wrappers around `IReadRepositoryBase<T>` and `IRepositoryBase<T>` from `Ardalis.Specification`
- Define `AppDbContext` in App.Infrastructure — the single `DbContext` for the service, applying every module's entity configurations via `ApplyConfigurationsFromAssembly`
- Provide a single generic `Repository<T>` in App.Infrastructure that inherits `RepositoryBase<T>` from `Ardalis.Specification.EntityFrameworkCore`
- Encapsulate all query criteria, ordering, includes, and projections into named reusable specification classes
- Keep raw LINQ out of handlers — query intent is expressed by name, not by inline lambda chains
- Define three spec shapes: entity filter (`Specification<T>`), DTO projection (`Specification<T, TResult>`), and their correct placement across Application and App.Queries

# Capabilities
- Decoupling of the Application layer from EF Core/`DbContext`
- Reusable named specifications for all queries
- Single generic repository implementation for all entities
- Clear separation between read and write repository contracts
- Consistent data access patterns across modules

# Core Principles
- Application layer never references DbContext — only `IReadRepository<T>` and `IRepository<T>` from Shared
- `IRepository<T>` stages changes in the EF tracker — it never commits
- `IReadRepository<T>` is strictly read-only — no write methods, no SaveChanges
- One generic `Repository<T>` implementation serves all entity types — registered once in DI
- All read queries accept `ISpecification<T>` — no raw LINQ parameters on repository methods
- `SaveChangesAsync` is intentionally absent from both interfaces — committing is the responsibility of the Unit of Work
- Specification encodes query intent — the repository executes it, the spec never touches the database
- `Specification<T>` returns entities — used when the handler needs to work with the full domain object
- `Specification<T, TResult>` projects to DTO inside the query — used when only read data is needed
- All specifications for a module live in `{Module}.Application/Specifications` — single place of discovery
- Simple single-condition specs are reusable across features and event handlers within the module
- Multi-condition or feature-specific specs belong to one use case
- Cross-module JOIN projection specs live in `App.Queries` — the only place that has access to multiple module entity types
- Spec name reflects intent, not implementation — `TaskByIdSpec` not `TaskWhereId`

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj]] - hosts generic `Repository<T>` implementation
  - [[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - hosts `AddInfrastructure()`, extended here for repository DI registration
- [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md|{Feature}Check.cs]] - the abstract shape this solution gives a concrete `Load` body to, once composed together

NUGET:
- `Ardalis.Specification` {version} - provides `ISpecification<T>`, `Specification<T>`, `IReadRepositoryBase<T>`, `IRepositoryBase<T>`
- `Ardalis.Specification.EntityFrameworkCore` {version} - provides `RepositoryBase<T>` for the generic `Repository<T>` implementation

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - extend - Add Ardalis.Specification package and repository abstractions
  - [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs]] - create - Read-only repository contract inheriting Ardalis `IReadRepositoryBase<T>`
  - [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create.md|IRepository.cs]] - create - Read-write repository contract inheriting Ardalis `IRepositoryBase<T>`
- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - extend - Add Ardalis.Specification.EntityFrameworkCore package
  - [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.create.md|AppDbContext.cs]] - create - The service's single `DbContext`, applying every module's entity configurations
  - [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/Repository.cs.create.md|Repository.cs]] - create - Generic EF Core repository inheriting Ardalis `RepositoryBase<T>`
- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Wire repository DI registration
  - [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.create.md|RepositoryRegistration.cs]] - create - Open-generic DI registration extension
- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - extend - Enforce repository usage in handlers and host all specifications
  - [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create.md|{Entity}ByIdSpec.cs]] - create - Example single-condition spec loading entity by Id
  - [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}SummarySpec.cs.create.md|{Entity}SummarySpec.cs]] - create - Example Application projection spec
  - [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.extend.md|{Feature}Check.cs]] - extend - Give `solution-dto-property-validators`'s `{Feature}Check.Load` a real `IReadRepository<T>` implementation

# Rules

## MUST:
- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Host.csproj.extend.md#MUST|App.Host.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.create.md#MUST|RepositoryRegistration.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md#MUST|App.Infrastructure.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.create.md#MUST|AppDbContext.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/Repository.cs.create.md#MUST|Repository.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md#MUST|Shared.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md#MUST|IReadRepository.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create.md#MUST|IRepository.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend.md#MUST|{Module}.Application.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create.md#MUST|{Entity}ByIdSpec.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}SummarySpec.cs.create.md#MUST|{Entity}SummarySpec.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.extend.md#MUST|{Feature}Check.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Host.csproj.extend.md#MUST|App.Host.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.create.md#MUST|RepositoryRegistration.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md#MUST|App.Infrastructure.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.create.md#MUST|AppDbContext.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/Repository.cs.create.md#MUST|Repository.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md#MUST|Shared.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md#MUST|IReadRepository.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create.md#MUST|IRepository.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend.md#MUST|{Module}.Application.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create.md#MUST|{Entity}ByIdSpec.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}SummarySpec.cs.create.md#MUST|{Entity}SummarySpec.cs]]
- Never application layer reference DbContext directly

## SHOULD
- Avoid `TaskRepository : Repository<TodoTask>` — unnecessary subclass, open generic covers all types
- Avoid handler injects DbContext directly — use `IRepository<T>` or `IReadRepository<T>`
- Avoid `IRepository<T>` used in query handler — signals wrong intent, use `IReadRepository<T>`
- Avoid repository method accepts `Expression<Func<T, bool>>` — all filtering goes through specs
- Avoid inline LINQ in handler: `_repository.FirstOrDefaultAsync(x => x.Id == id)` — define `TaskByIdSpec` instead
- Avoid `GetByIdSpec` shared across entity types — each entity has its own `TaskByIdSpec`, `OrderByIdSpec`
- Avoid business rule inside spec: `Where(t => t.Price * 0.9m > threshold)` — rule belongs in Domain, not spec
- Avoid specs scattered across Domain and Application — all specs belong in Application
- Avoid cross-module JOIN spec placed in a module's Application — App.Queries is the only correct location
- Avoid `{Feature}Check.Load` still throwing `NotSupportedException` once this solution is composed — the stub must be replaced, not left alongside a working `IReadRepository<T>` elsewhere in the module

# Check list
- [ ] `AppDbContext` defined in `App.Infrastructure/Persistence`, applies every module's configurations via `ApplyConfigurationsFromAssembly`
- [ ] `AppDbContext` registered via `AddDbContext<AppDbContext>` inside `AddInfrastructure()`
- [ ] `IReadRepository<T>` defined in `Shared/Repositories`, inherits `IReadRepositoryBase<T>`
- [ ] `IRepository<T>` defined in `Shared/Repositories`, inherits `IRepositoryBase<T>` and `IReadRepository<T>`
- [ ] `IRepository<T>` has no `SaveChangesAsync`
- [ ] `Repository<T>` implemented in `App.Infrastructure/Repositories/Repository.cs`
- [ ] `Repository<T>` inherits `RepositoryBase<T>` from Ardalis
- [ ] `Repository<T>` implements `IRepository<T>`
- [ ] `Repository<T>` never calls `SaveChangesAsync`
- [ ] Open generic DI registration in `App.Host` for both interfaces
- [ ] Registered with `Scoped` lifetime
- [ ] No per-entity repository subclass exists
- [ ] Command handlers inject `IRepository<T>`
- [ ] Query handlers inject `IReadRepository<T>`
- [ ] No DbContext reference in any Application class
- [ ] Every entity loaded by Id has `{Entity}ByIdSpec` in `/{Module}.Application/Specifications`
- [ ] All specifications for the module live in `/{Module}.Application/Specifications`
- [ ] All cross-module JOIN specs live in `/App.Queries/Specifications`
- [ ] All projection specs use `Specification<T, TResult>`
- [ ] All entity filter specs use `Specification<T>`
- [ ] No inline LINQ in any handler
- [ ] Spec names reflect intent — not field names or implementation detail
- [ ] Every `{Feature}Check` composed alongside this solution has its `Load` implemented via `IReadRepository<T>` and a named spec — no `NotSupportedException` stub left in place
