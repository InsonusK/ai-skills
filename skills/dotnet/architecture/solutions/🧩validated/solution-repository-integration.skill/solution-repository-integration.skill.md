---
name: solution-repository-integration
description: Merges the Ardalis Specification pattern with repository abstractions — defines IReadRepository<T> and IRepository<T> as thin wrappers around Ardalis base interfaces in Shared, provides a generic Repository<T> implementation in App.Infrastructure inheriting Ardalis.RepositoryBase<T>, and governs specification placement across Application and App.Queries
domain: skill
type: architecture
version: 20260611
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
  - concern/architecture

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
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
---
# Goal
- Decouple Application layer handlers from EF Core and DbContext by providing repository abstractions powered by Ardalis
- Define `IReadRepository<T>` and `IRepository<T>` in Shared as thin wrappers around `IReadRepositoryBase<T>` and `IRepositoryBase<T>` from `Ardalis.Specification`
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
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]] - hosts repository abstractions
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]] - hosts generic `Repository<T>` implementation
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Host.csproj.create|App.Host.csproj]] - hosts repository DI registration
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create|{Module}.Application.csproj]] - hosts module specifications
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]] - hosts entity pattern filtered by specifications

NUGET:
- `Ardalis.Specification` {version} - provides `ISpecification<T>`, `Specification<T>`, `IReadRepositoryBase<T>`, `IRepositoryBase<T>`
- `Ardalis.Specification.EntityFrameworkCore` {version} - provides `RepositoryBase<T>` for the generic `Repository<T>` implementation

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - extend - Add Ardalis.Specification package and repository abstractions
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create|IReadRepository.cs]] - create - Read-only repository contract inheriting Ardalis `IReadRepositoryBase<T>`
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create|IRepository.cs]] - create - Read-write repository contract inheriting Ardalis `IRepositoryBase<T>`
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend|App.Infrastructure.csproj]] - extend - Add Ardalis.Specification.EntityFrameworkCore package
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/Repository.cs.create|Repository.cs]] - create - Generic EF Core repository inheriting Ardalis `RepositoryBase<T>`
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - extend - Wire repository DI registration
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.create|RepositoryRegistration.cs]] - create - Open-generic DI registration extension
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend|{Module}.Application.csproj]] - extend - Enforce repository usage in handlers and host all specifications
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create|{Entity}ByIdSpec.cs]] - create - Example single-condition spec loading entity by Id
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}SummarySpec.cs.create|{Entity}SummarySpec.cs]] - create - Example Application projection spec

# Rules

## MUST:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Host.csproj.extend#MUST|App.Host.csproj]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.create#MUST|RepositoryRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend#MUST|App.Infrastructure.csproj]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/Repository.cs.create#MUST|Repository.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend#MUST|Shared.csproj]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create#MUST|IReadRepository.cs]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create#MUST|IRepository.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend#MUST|{Module}.Application.csproj]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create#MUST|{Entity}ByIdSpec.cs]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}SummarySpec.cs.create#MUST|{Entity}SummarySpec.cs]]

## MUST NOT:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Host.csproj.extend#MUST NOT|App.Host.csproj]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.create#MUST NOT|RepositoryRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend#MUST NOT|App.Infrastructure.csproj]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/Repository.cs.create#MUST NOT|Repository.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend#MUST NOT|Shared.csproj]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create#MUST NOT|IReadRepository.cs]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create#MUST NOT|IRepository.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend#MUST NOT|{Module}.Application.csproj]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create#MUST NOT|{Entity}ByIdSpec.cs]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}SummarySpec.cs.create#MUST NOT|{Entity}SummarySpec.cs]]
- Application layer reference DbContext directly

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
