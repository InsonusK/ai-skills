---
name: solution-repository-integration
description: Merges the Ardalis Specification pattern with repository abstractions — defines IReadRepository<T> and IRepository<T> as thin wrappers around Ardalis base interfaces in Shared, provides a generic Repository<T> implementation in App.Infrastructure inheriting Ardalis.RepositoryBase<T>, and governs specification placement across Application and App.Queries
domain: skill
type: architecture
version: 20260611
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - application
  - infrastructure
  - repository
  - ardalis
  - specification
  - query
triggers:
  - define repository
  - add data access abstraction
  - IRepository usage
  - IReadRepository usage
  - load entity in handler
  - write a specification
  - query entity by criteria
  - filter entities
  - project entity to DTO
  - ardalis spec
creates:
  - Shared.Repositories.IReadRepository.cs
  - Shared.Repositories.IRepository.cs
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
depends_on:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure.skill]]"
---
# Goal
- Decouple Application layer handlers from EF Core and DbContext by providing repository abstractions powered by Ardalis
- Define `IReadRepository<T>` and `IRepository<T>` in Shared as thin wrappers around `IReadRepositoryBase<T>` and `IRepositoryBase<T>` from `Ardalis.Specification`
- Provide a single generic `Repository<T>` in App.Infrastructure that inherits `RepositoryBase<T>` from `Ardalis.Specification.EntityFrameworkCore`
- Encapsulate all query criteria, ordering, includes, and projections into named reusable specification classes
- Keep raw LINQ out of handlers — query intent is expressed by name, not by inline lambda chains
- Define three spec shapes: entity filter (`Specification<T>`), DTO projection (`Specification<T, TResult>`), and their correct placement across Application and App.Queries

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
- [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj]] - hosts repository abstractions
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj]] - hosts generic `Repository<T>` implementation
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj]] - hosts repository DI registration
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj]] - hosts module specifications
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj]] - hosts entity pattern filtered by specifications

NUGET:
- `Ardalis.Specification` {version} - provides `ISpecification<T>`, `Specification<T>`, `IReadRepositoryBase<T>`, `IRepositoryBase<T>`
- `Ardalis.Specification.EntityFrameworkCore` {version} - provides `RepositoryBase<T>` for the generic `Repository<T>` implementation

# Template Skill Mutations

PROJECT:
- [[./Implementation/Shared.csproj.extend.md|Shared.csproj]] - extend - Add Ardalis.Specification package and repository abstractions
  - [[./Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs]] - create - Read-only repository contract inheriting Ardalis `IReadRepositoryBase<T>`
  - [[./Implementation/Shared.csproj.extend/IRepository.cs.create.md|IRepository.cs]] - create - Read-write repository contract inheriting Ardalis `IRepositoryBase<T>`
- [[./Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - extend - Add Ardalis.Specification.EntityFrameworkCore package
  - [[./Implementation/App.Infrastructure.csproj.extend/Repository.cs.create.md|Repository.cs]] - create - Generic EF Core repository inheriting Ardalis `RepositoryBase<T>`
- [[./Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Wire repository DI registration
  - [[./Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.create.md|RepositoryRegistration.cs]] - create - Open-generic DI registration extension
- [[./Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - extend - Enforce repository usage in handlers and host all specifications
  - [[./Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create.md|{Entity}ByIdSpec.cs]] - create - Example single-condition spec loading entity by Id
  - [[./Implementation/{Module}.Application.csproj.extend/{Entity}SummarySpec.cs.create.md|{Entity}SummarySpec.cs]] - create - Example Application projection spec

# Rules

MUST:
- `IReadRepository<T>` and `IRepository<T>` defined in Shared, inheriting Ardalis base interfaces
- `IRepository<T>` extends `IReadRepository<T>` and `IRepositoryBase<T>`
- `IRepository<T>` has no `SaveChangesAsync` — committing belongs to Unit of Work
- Single generic `Repository<T>` in App.Infrastructure, inheriting `RepositoryBase<T>` from Ardalis
- `Repository<T>` implements `IRepository<T>`
- `Repository<T>` constructor receives `AppDbContext` and passes it to the Ardalis base
- Open generic DI registration in App.Host for both `IRepository<>` and `IReadRepository<>` pointing to `Repository<>`
- Registered with `Scoped` lifetime
- All repository read methods accept `ISpecification<T>` — no raw lambda or LINQ parameters
- Command handlers inject `IRepository<T>`
- Query handlers inject `IReadRepository<T>`
- All entity loading in handlers uses a named spec — no inline `Where(...)` LINQ
- All specifications for a module live in `/{Module}.Application/Specifications`
- Cross-module JOIN specs live in `/App.Queries/Specifications`
- Every entity loaded by Id has a `{Entity}ByIdSpec` in Application
- Projection specs use `Specification<T, TResult>` — entity filter specs use `Specification<T>`
- Spec name reflects query intent — not field names or implementation detail

MUST NOT:
- Application layer reference DbContext directly
- Per-entity repository subclass be created
- `Repository<T>` call `SaveChangesAsync`
- Raw LINQ predicates appear in repository method signatures
- Spec call the database or reference DbContext
- Spec contain business logic — filtering, ordering, and projection only
- Handler contain inline `Where(...)` LINQ — always delegate to a named spec
- Specs placed in `{Module}.Domain` — all specs belong in Application
- Generic spec names used across multiple entities (`GetByIdSpec`) — name per entity
- Single-module specs live in App.Queries — they belong in the module's Application

# Anti-patterns
- `TaskRepository : Repository<TodoTask>` — unnecessary subclass, open generic covers all types
- Handler injects DbContext directly — use `IRepository<T>` or `IReadRepository<T>`
- `IRepository<T>` used in query handler — signals wrong intent, use `IReadRepository<T>`
- Repository method accepts `Expression<Func<T, bool>>` — all filtering goes through specs
- Inline LINQ in handler: `_repository.FirstOrDefaultAsync(x => x.Id == id)` — define `TaskByIdSpec` instead
- `GetByIdSpec` shared across entity types — each entity has its own `TaskByIdSpec`, `OrderByIdSpec`
- Business rule inside spec: `Where(t => t.Price * 0.9m > threshold)` — rule belongs in Domain, not spec
- Specs scattered across Domain and Application — all specs belong in Application
- Cross-module JOIN spec placed in a module's Application — App.Queries is the only correct location

# Check list
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

# Unittest TestCases
Specs and repository are tested via integration tests against a real database.
- [ ] When `FirstOrDefaultAsync` called with matching spec Then correct entity returned
- [ ] When `FirstOrDefaultAsync` called with non-matching spec Then null returned
- [ ] When `ListAsync` called Then all matching entities returned
- [ ] When `AnyAsync` called with matching spec Then returns true
- [ ] When `AnyAsync` called with non-matching spec Then returns false
- [ ] When `AddAsync` called Then entity tracked by EF — not yet in database
- [ ] When `Remove` called Then entity marked for deletion — not yet removed from database
- [ ] When read query runs Then EF change tracker does not track returned entities (AsNoTracking)
- [ ] When entity matches spec criteria Then it is returned in results
- [ ] When entity does not match spec criteria Then it is not returned
- [ ] When spec has ordering Then results are in the correct order
- [ ] When projection spec is used Then DTO fields are correctly mapped from entity columns
- [ ] When idempotency spec is used with a known EventId Then duplicate is detected correctly
- [ ] When cross-module projection spec is used Then data from both modules is correctly joined
