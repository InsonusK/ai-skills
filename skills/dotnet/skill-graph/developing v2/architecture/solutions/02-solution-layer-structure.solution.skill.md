---
uid: f35f923e-960f-42d4-a724-24ee9e70bf23
order: 2
name: solution-layer-structure
description: Defines the full solution folder structure, all non-module layers, their responsibilities, and the complete dependency rules between all layers
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - architecture
  - structure
  - layers
triggers:
  - initialize solution
  - add new layer
  - decide file placement
  - define project dependencies
creates:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Shared cpsroj/Shared.csproj.skill|Shared.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/BuildingBlocks.csproj.skill|BuildingBlocks.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App Layer/App.Host csproj/App.Host.csproj.skill|App.Host.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App Layer/App.Infrastructure/App.Infrastructure.csproj.skill|App.Infrastructure.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App Layer/App.Infrastructure.Migrations/App.Infrastructure.Migrations.csproj.skill|App.Infrastructure.Migrations.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App Layer/App.Queries/App.Queries.csproj.skill|App.Queries.csproj.skill]]"
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/{Module}.Domain.csproj.skill|{Module}.Domain.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Interface csproj/{Module}.Interfaces.csproj.skill|{Module}.Interfaces.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/{Module}.Application.csproj.skill|{Module}.Application.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Api csproj/{Module}.Api.csproj.skill|{Module}.Api.csproj.skill]]"
depends_on:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill|01-module-boundary.solution.skill]]"
---

# Goal
- Define the full solution folder structure — where every layer lives on disk
- Define the responsibility of each non-module layer: Shared, BuildingBlocks, App.Host, App.Infrastructure, App.Queries
- Define the complete set of allowed and forbidden dependencies between all layers
- Provide a single reference for file placement decisions across the entire solution

# Core Principles
- Every project belongs to exactly one layer — no project spans multiple layers
- Dependencies flow inward — outer layers depend on inner layers, never the reverse
- App.Host is the only composition root — it is the single place that wires everything together
- Shared and BuildingBlocks have no business logic — they are framework primitives and cross-cutting utilities
- Cross-module JOIN queries belong exclusively in App.Queries — never in Application or Domain
- App.Infrastructure is the only layer that knows about persistence implementation — DbContext, EF Core, outbox

# Depend on solutions
- definition of `Module project` - [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill|01-module-boundary.solution.skill]] defines the module projects that this solution places and connects within the full solution structure

# Implementation

## App Repository (.sln)

### Structure

#### Project Structure
```
/src
  /Modules
    /{ModuleName}             ← defined by module-boundary solution
  /App
    /App.Host
    /App.Infrastructure
    /App.Infrastructure.Migrations
    /App.Queries
  /Shared
  /BuildingBlocks
```

#### Directory and class skills
| `Directory\|file`                      | Description                                                    | Pattern skill                                                                                                                                      |
| -------------------------------------- | -------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| /src/Modules                           | All bounded context modules                                    | defined by [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill\|01-module-boundary.solution.skill]] |
| /src/App                               | Composition and infrastructure layer                           |                                                                                                                                                    |
| /src/App/App.Host                      | Composition root — DI, pipeline, module wiring                 |                                                                                                                                                    |
| /src/App/App.Infrastructure            | Persistence — DbContext, repos, outbox                         |                                                                                                                                                    |
| /src/App/App.Infrastructure.Migrations | EF Core migrations only                                        |                                                                                                                                                    |
| /src/App/App.Queries                   | Cross-module read models and JOIN queries                      |                                                                                                                                                    |
| /src/Shared                            | Cross-cutting primitives — Result, Exceptions, base interfaces |                                                                                                                                                    |
| /src/BuildingBlocks                    | Reusable framework patterns — pipeline behaviors, base specs   |                                                                                                                                                    |

---

## Shared (.csproj)

### Project extension

#### Goal
- Provide cross-cutting primitives that every layer can safely depend on without creating coupling
- Define base types used across module and infrastructure boundaries

#### Core Principal
- Shared has no business logic — only framework-level primitives
- Shared has no dependencies on any other project in this solution
- Any project at any layer may depend on Shared

#### Structure

##### Project Structure
```
/Shared
  /Events
    IDomainEvent.cs
  /Exceptions
    DomainException.cs
    ConflictException.cs
  Shared.csproj
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Events | Base event interfaces | |
| /Exceptions | Shared exception types used across layers | |

#### What Does NOT Belong Here
- Business logic — belongs to Domain
- Pipeline behaviors — belongs to BuildingBlocks
- Infrastructure implementations — belongs to App.Infrastructure

#### Allowed Dependencies
- None — Shared has no project dependencies

#### Rules
MUST:
- Shared has zero project references
- All types in Shared are purely cross-cutting primitives

MUST NOT:
- Shared reference any module, BuildingBlocks, or infrastructure project
- Shared contain business logic or domain rules

#### Anti-patterns
- Placing domain entities in Shared — they belong in module Domain
- Placing pipeline behaviors in Shared — they belong in BuildingBlocks
- Adding project references to Shared.csproj

#### Check list
- [ ] Shared.csproj has no project references
- [ ] No business logic in any Shared class

---

## BuildingBlocks (.csproj)

### Project extension

#### Goal
- Provide reusable framework-level patterns used by Application layer and infrastructure across all modules
- Define pipeline behavior contracts and base implementations

#### Core Principal
- BuildingBlocks contains reusable technical patterns, not business logic
- BuildingBlocks depends only on Shared
- All pipeline behaviors live here — registered once in App.Host, used by all modules

#### Structure

##### Project Structure
```
/BuildingBlocks
  /MediatR
    ICommand.cs
    IQuery.cs
    UnitOfWorkContext.cs
  /Repositories
    IRepository.cs
    IReadRepository.cs
  /UnitOfWork
    IUnitOfWork.cs
  /Outbox
    OutboxMessage.cs
    IHasDomainEvents.cs
  /Concurrency
    IHasVersions.cs
    IEntityVersionResolver.cs
    ETagEncoder.cs
  BuildingBlocks.csproj
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /MediatR | Command/Query markers, pipeline behavior base contracts | |
| /Repositories | IRepository and IReadRepository abstractions | |
| /UnitOfWork | IUnitOfWork abstraction | |
| /Outbox | OutboxMessage and IHasDomainEvents | |
| /Concurrency | ETag, version interfaces | |

#### What Does NOT Belong Here
- Business logic — belongs to Domain
- Infrastructure implementations — belongs to App.Infrastructure
- Module-specific handlers or validators — belong to module Application

#### Allowed Dependencies
- Shared

#### Rules
MUST:
- All pipeline behavior contracts defined here
- IRepository, IReadRepository, IUnitOfWork defined here
- BuildingBlocks depends only on Shared

MUST NOT:
- BuildingBlocks reference any module project
- BuildingBlocks reference App.Infrastructure or App.Queries
- BuildingBlocks contain business logic

#### Check list
- [ ] BuildingBlocks.csproj references only Shared
- [ ] IRepository, IReadRepository, IUnitOfWork present
- [ ] ICommand, IQuery markers present

---

## App.Infrastructure (.csproj)

### Project extension

#### Goal
- Provide all persistence implementation — DbContext, repository implementations, outbox interceptor, background dispatcher
- Be the only layer that knows EF Core implementation details

#### Core Principal
- App.Infrastructure is the only project with a concrete DbContext
- App.Infrastructure implements interfaces defined in BuildingBlocks
- No module Application or Domain layer references App.Infrastructure

#### Structure

##### Project Structure
```
/App.Infrastructure
  /Persistence
    AppDbContext.cs
    /Configurations
      OutboxMessageConfig.cs
  /Repositories
    Repository.cs
  /UnitOfWork
    UnitOfWork.cs
  /Outbox
    DomainEventInterceptor.cs
    OutboxDispatcher.cs
  /Concurrency
    EntityVersionResolver.cs
  App.Infrastructure.csproj
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Persistence | DbContext and EF configurations | |
| /Repositories | Generic Repository<T> implementation | |
| /UnitOfWork | UnitOfWork implementation | |
| /Outbox | EF interceptor and background dispatcher | |
| /Concurrency | EntityVersionResolver mapping strings to types | |

#### What Does NOT Belong Here
- Business logic — belongs to Domain
- Pipeline behavior registration — belongs to App.Host
- Cross-module JOIN queries — belongs to App.Queries

#### Allowed Dependencies
- BuildingBlocks
- Shared
- {ModuleName}.Domain (all modules)
- {ModuleName}.Interfaces (all modules)

#### Rules
MUST:
- App.Infrastructure is the only project with DbContext
- `Repository<T>` generic implementation registered here
- DomainEventInterceptor registered on DbContext here

MUST NOT:
- App.Infrastructure be referenced by any module Application, Domain, or Api
- App.Infrastructure be referenced by App.Queries directly for DbContext

#### Anti-patterns
- Module Application referencing App.Infrastructure — use repository abstractions from BuildingBlocks
- Putting cross-module JOIN queries in App.Infrastructure — belongs in App.Queries

#### Check list
- [ ] AppDbContext defined here
- [ ] Generic `Repository<T>` implemented and registered
- [ ] DomainEventInterceptor registered on DbContext
- [ ] No module Application references this project

---

## App.Queries (.csproj)

### Project extension

#### Goal
- Provide cross-module read model handlers that require JOIN queries across module boundaries
- Be the only place where cross-module database joins are intentional and correct

#### Core Principal
- App.Queries has direct DbContext access for cross-module JOINs
- Single-module queries belong in module Application — not here
- App.Queries implements query handlers declared in module Interfaces

#### Structure

##### Project Structure
```
/App.Queries
  /Queries
    /{ModuleName}
      GetTaskWithUserDetailsHandler.cs
  App.Queries.csproj
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Queries/{ModuleName} | Cross-module query handlers grouped by primary module | |

#### What Does NOT Belong Here
- Single-module queries — belong in module Application
- Write operations — belong in module Application handlers
- Business logic — belongs to Domain

#### Allowed Dependencies
- App.Infrastructure (for DbContext access)
- {ModuleName}.Domain (all modules — for entity types in JOIN queries)
- {ModuleName}.Interfaces (all modules — for query and DTO contracts)

#### Rules
MUST:
- Only cross-module JOIN queries live here
- Query handlers here implement contracts declared in module Interfaces

MUST NOT:
- App.Queries contain write operations
- App.Queries contain business logic
- Single-module queries be placed here

#### Check list
- [ ] Only cross-module handlers present
- [ ] All handlers implement query contracts from module Interfaces
- [ ] No write operations in any handler

---

## App.Host (.csproj)

### Project extension

#### Goal
- Be the single composition root — wire all modules, infrastructure, pipeline behaviors, and DI registrations together
- Be the only project that knows about all other projects simultaneously

#### Core Principal
- App.Host references everything — it is the only project allowed to do so
- App.Host contains no business logic — only wiring
- Pipeline behaviors are registered once here — not inside individual modules

#### Structure

##### Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs
    InfrastructureRegistration.cs
  Program.cs
  App.Host.csproj
```

#### What Does NOT Belong Here
- Business logic — belongs to Domain
- Handler implementations — belong to module Application
- Infrastructure implementations — belong to App.Infrastructure

#### Allowed Dependencies
- {ModuleName}.Api (all modules)
- {ModuleName}.Application (all modules — for registration methods)
- App.Infrastructure
- App.Queries
- BuildingBlocks

#### Rules
MUST:
- Pipeline behaviors registered once here in correct order
- Each module registration method called here
- App.Host is the only project referencing all modules simultaneously

MUST NOT:
- App.Host contain business logic
- App.Host contain handler implementations

#### Check list
- [ ] All module registration methods called
- [ ] Pipeline behaviors registered in correct order
- [ ] No business logic in App.Host

---

## {Module}.Interface (.csproj) (extended)

### Project extension

#### Allowed Dependencies
- Shared

---

## {Module}.Application (.csproj) (extended)

### Project extension

#### Allowed Dependencies
- {Module}.Interfaces (own module)
- {Module}.Domain (own module)
- {OtherModule}.Interfaces (other modules — contracts only)
- Shared
- BuildingBlocks

---

## {Module}.Domain (.csproj) (extended)

### Project extension

#### Allowed Dependencies
- Shared
- Microsoft.EntityFrameworkCore (IEntityTypeConfiguration only)

---

## {Module}.Api (.csproj) (extended)

### Project extension

#### Allowed Dependencies
- BuildingBlocks

---

# Rules

MUST:
- Every project belongs to exactly one layer
- App.Host is the only composition root
- App.Infrastructure is the only project with DbContext
- App.Queries is the only place for cross-module JOIN queries
- Shared has no project dependencies
- BuildingBlocks depends only on Shared
- Pipeline behaviors registered once in App.Host

MUST NOT:
- Any module Application reference App.Infrastructure
- Any module Application reference App.Queries
- Any module Domain reference another module's project
- Any module Api reference Domain or Application directly
- App.Queries be referenced by module Application or Domain

# Anti-patterns
- Cross-module JOIN in module Application — belongs in App.Queries
- DbContext referenced from module Application — use IRepository from BuildingBlocks
- Pipeline behaviors registered inside module registration — register once in App.Host
- Business logic in App.Host — wiring only
- Global tests folder — tests colocated with modules

# Check list
- [ ] Solution folder structure matches defined layout
- [ ] Shared.csproj has no project references
- [ ] BuildingBlocks.csproj references only Shared
- [ ] App.Infrastructure is the only project with DbContext
- [ ] App.Queries contains only cross-module JOIN handlers
- [ ] App.Host is the only project referencing all modules
- [ ] No module Application references App.Infrastructure or App.Queries
- [ ] No module Domain references another module
- [ ] Pipeline behaviors registered in App.Host only
- [ ] EF entity configurations live in module Domain/Configurations
- [ ] Cross-module FK configurations live in App.Infrastructure only

# Unittest TestCases
Not applicable — layer structure is validated via architecture tests, not runtime tests.

- [ ] When module Application references App.Infrastructure Then architecture test fails
- [ ] When module Application references App.Queries Then architecture test fails
- [ ] When module Domain references another module Then architecture test fails
- [ ] When module Api references Domain directly Then architecture test fails
- [ ] When Shared has any project reference Then architecture test fails
- [ ] When BuildingBlocks references anything other than Shared Then architecture test fails
