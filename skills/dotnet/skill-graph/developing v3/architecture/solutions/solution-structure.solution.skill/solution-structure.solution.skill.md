---
uid: 743c1e5b-ff89-4a73-9b0c-71688d0c4e40
name: solution-structure
description: Defines the full solution architecture including module boundaries, all non-module layers, their responsibilities, folder placement, and the complete dependency rules between all layers
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - architecture
  - module
  - bounded-context
  - structure
  - layers
triggers:
  - create new module
  - design module boundary
  - define bounded context structure
  - initialize solution
  - add new layer
  - decide file placement
  - define project dependencies
creates:
  - "{Module}.Domain.csproj"
  - "{Module}.Interfaces.csproj"
  - "{Module}.Application.csproj"
  - "{Module}.Api.csproj"
  - "{Module}.Domain.Entities.{EntityName}.cs"
  - Shared.csproj
  - BuildingBlocks.csproj
  - App.Host.csproj
  - App.Infrastructure.csproj
  - App.Infrastructure.Migrations.csproj
  - App.Queries.csproj
extends:
depends_on:
---
# Goal
- Define a module as a self-contained bounded context that owns its domain, application logic, API surface, and public contracts
- Prevent hidden coupling between modules by enforcing interaction only through declared contracts
- Define the four-project internal structure every module must follow
- Define where modules live in the solution folder structure
- Define the full solution folder structure — where every layer lives on disk
- Define the responsibility of each non-module layer: Shared, BuildingBlocks, App.Host, App.Infrastructure, App.Queries
- Define the complete set of allowed and forbidden dependencies between all layers
- Provide a single reference for file placement decisions across the entire solution

# Core Principals
- A module is a bounded context — it owns everything inside its boundary
- Modules never depend on each other's implementation — only on Interfaces contracts
- Each module has exactly four projects: Api, Application, Domain, Interfaces
- Interfaces is the only public surface of a module — breaking changes must be versioned
- Domain is the innermost layer — it has no dependency on any other module or infrastructure
- Tests are colocated with the module — no global tests folder
- Every project belongs to exactly one layer — no project spans multiple layers
- Dependencies flow inward — outer layers depend on inner layers, never the reverse
- App.Host is the only composition root — it is the single place that wires everything together
- Shared and BuildingBlocks have no business logic — Shared defines common contracts, BuildingBlocks implements technical patterns
- Cross-module JOIN queries belong exclusively in App.Queries — never in Application or Domain
- App.Infrastructure is the only layer that knows about persistence implementation — DbContext, EF Core, outbox

# Requirements
None — this is a foundation solution.

# Template Skill Mutations

REPOSITORY
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/Repository.create|App Repository structure]]

PROJECT
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/Shared.csproj.create|Shared.csproj]] - create - Defines common cross-cutting interfaces (e.g., IRepository, IUnitOfWork, IDomainEvent). No implementation, no business logic.
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]] - create - Implements application technical patterns (e.g., EF repositories, MediatR behaviors, outbox dispatchers). Does NOT define common interfaces — consumes interfaces from Shared.
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/{Module}.Interfaces.csproj.create|{Module}.Interfaces.csproj]] - create - Module public contracts project
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]] - create - Module business logic project
	- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/{Entity}.cs.create|Entity.class.skill]] - create - Domain entity class
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/{Module}.Application.csproj.create|{Module}.Application.csproj]] - create - Module orchestration project
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/{Module}.Api.csproj.create|{Module}.Api.csproj]] - create - Module HTTP adapter project
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]] - create - Persistence implementation project
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/App.Queries.csproj.create|App.Queries.csproj]] - create - Cross-module query project
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/App.Host.csproj.create|App.Host.csproj]] - create - Composition root project
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/App.Infrastructure.Migrations.csproj.create|App.Infrastructure.Migrations.csproj]] - create - EF Core migrations project

# Rules

MUST:
- Each module has exactly Api, Application, Domain, Interfaces projects
- Other modules reference only {ModuleName}.Interfaces
- All cross-module writes go through MediatR command dispatch
- All cross-module reads go through MediatR query dispatch or App.Queries
- Tests colocated with module
- Every project belongs to exactly one layer
- App.Host is the only composition root
- App.Infrastructure is the only project with DbContext
- App.Queries is the only place for cross-module JOIN queries
- Shared has no project dependencies
- BuildingBlocks depends only on Shared
- BuildingBlocks does not define common interfaces — only implements patterns using interfaces from Shared
- App.Host references BuildingBlocks; modules and other layers reference Shared directly to implement or consume interfaces
- Pipeline behaviors registered once in App.Host

MUST NOT:
- Module reference another module's Domain
- Module reference another module's Application
- Domain reference any other module's project
- Api reference Domain or Application directly
- Any module Application reference App.Infrastructure
- Any module Application reference App.Queries
- Any module Domain reference another module's project
- Any module Api reference Domain or Application directly
- App.Queries be referenced by module Application or Domain

# Anti-patterns
- Shared domain model across modules — each module owns its own entities
- Direct method call into another module's Application — use MediatR
- Depending on another module's Domain for entity types — use DTOs from Interfaces
- Cross-module JOIN logic in Application — belongs in App.Queries
- Global tests folder — tests live next to their module
- Cross-module JOIN in module Application — belongs in App.Queries
- DbContext referenced from module Application — use IRepository from Shared (implemented in BuildingBlocks)
- Pipeline behaviors registered inside module registration — register once in App.Host
- Business logic in App.Host — wiring only

# Check list
- [ ] Module folder exists under /src/Modules/{ModuleName}
- [ ] Module has exactly four projects: Api, Application, Domain, Interfaces
- [ ] Interfaces has no project dependencies
- [ ] Domain depends only on Shared and EF Core config
- [ ] Application does not reference Infrastructure or App.Queries
- [ ] Api does not reference Domain or Application directly
- [ ] No direct dependency on another module's Application or Domain
- [ ] Tests colocated with module projects
- [ ] Solution folder structure matches defined layout
- [ ] Shared.csproj has no project references and contains only interface definitions
- [ ] BuildingBlocks.csproj references only Shared and contains only pattern implementations
- [ ] App.Host references BuildingBlocks and does not directly reference Shared
- [ ] App.Infrastructure is the only project with DbContext
- [ ] App.Queries contains only cross-module JOIN handlers
- [ ] App.Host is the only project referencing all modules
- [ ] No module Application references App.Infrastructure or App.Queries
- [ ] No module Domain references another module
- [ ] Pipeline behaviors registered in App.Host only
- [ ] EF entity configurations live in module Domain/Configurations
- [ ] Cross-module FK configurations live in App.Infrastructure only

# Unittest TestCases
Not applicable — architecture is validated via architecture tests, not runtime unit tests.

- [ ] When any project references another module's Domain Then architecture test fails
- [ ] When any project references another module's Application Then architecture test fails
- [ ] When Interfaces project has a project reference Then architecture test fails
- [ ] When Api references Domain directly Then architecture test fails
- [ ] When module Application references App.Infrastructure Then architecture test fails
- [ ] When module Application references App.Queries Then architecture test fails
- [ ] When module Domain references another module Then architecture test fails
- [ ] When module Api references Domain directly Then architecture test fails
- [ ] When Shared has any project reference Then architecture test fails
- [ ] When BuildingBlocks references anything other than Shared Then architecture test fails
- [ ] When App.Host directly references Shared Then architecture test fails
