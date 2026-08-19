---
name: solution-sln-structure
description: Defines the full solution architecture including module boundaries, all non-module layers, their responsibilities, folder placement, and the complete dependency rules between all layers
domain: skill
type: architecture
version: 20260819
tags:
  - skill/architecture/solution
  - stack/dotnet
  - framework/ef-core
  - concern/architecture
  - module
  - bounded-context
  - structure
  - layers
  - solution/sln-structure

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
  - App.Host.DependencyInjection.ModuleRegistration.cs
  - App.Infrastructure.csproj
  - App.Infrastructure.Migrations.csproj
  - App.Queries.csproj
extends:
depends_on:
adr:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/adr/module-project-set-extensibility|Base set of four projects, extensible by pattern solutions, instead of a fixed ceiling]]"
---
# Goal
- Define a module as a self-contained bounded context that owns its domain, application logic, API surface, and public contracts
- Prevent hidden coupling between modules by enforcing interaction only through declared contracts
- Define the base project structure every module must follow, extensible by a specific pattern solution when it needs project-level isolation
- Define where modules live in the solution folder structure
- Define the full solution folder structure — where every layer lives on disk
- Define the responsibility of each non-module layer: Shared, BuildingBlocks, App.Host, App.Infrastructure, App.Queries
- Define the complete set of allowed and forbidden dependencies between all layers
- Provide a single reference for file placement decisions across the entire solution

# Capabilities
- Well-defined module boundaries and layer responsibilities
- Enforced inward dependency direction
- Prevention of hidden coupling between modules
- Clear file and project placement rules
- Foundation for architecture testing and reviews

# Core Principles
- A module is a bounded context — it owns everything inside its boundary
- Modules never depend on each other's implementation — only on Interfaces contracts
- Each module has the base set of four projects — Api, Application, Domain, Interfaces; a specific pattern solution may add an additional project to a module when it needs project-level isolation
- Interfaces is the only public surface of a module — breaking changes must be versioned
- Domain is the innermost layer — it has no dependency on any other module or infrastructure
- Tests are colocated with the module — no global tests folder
- Every project belongs to exactly one layer — no project spans multiple layers
- Dependencies flow inward — outer layers depend on inner layers, never the reverse
- App.Host is the only composition root — it is the single place that wires everything together
- Shared and BuildingBlocks have no business logic — Shared defines common contracts, BuildingBlocks implements technical patterns
- Cross-module JOIN queries belong exclusively in App.Queries — never in Application or Domain
- App.Infrastructure is the only layer that knows about persistence implementation — DbContext, EF Core, outbox

# Adr
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/adr/module-project-set-extensibility|Base set of four projects, extensible by pattern solutions, instead of a fixed ceiling]]
  - Selected variant: base set + extensibility — a specific pattern solution may add a project when it needs project-level isolation; `solution-sln-structure` does not enumerate which optional projects currently exist, that is discoverable through the solution dependency graph

# Requirements
None — this is a foundation solution.

# Template Skill Mutations

REPOSITORY
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Repository.create|App Repository structure]]

PROJECT
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]] - create - Defines common cross-cutting interfaces (e.g., IRepository, IUnitOfWork, IDomainEvent). No implementation, no business logic.
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]] - create - Implements application technical patterns (e.g., EF repositories, MediatR behaviors, outbox dispatchers). Does NOT define common interfaces — consumes interfaces from Shared.
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create|{Module}.Interfaces.csproj]] - create - Module public contracts project
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]] - create - Module business logic project
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create|class-Entity.skill]] - create - Domain entity class
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create|{Module}.Application.csproj]] - create - Module orchestration project
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create|{Module}.Api.csproj]] - create - Module HTTP adapter project
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]] - create - Persistence implementation project
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Queries.csproj.create|App.Queries.csproj]] - create - Cross-module query project
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Host.csproj.create|App.Host.csproj]] - create - Composition root project
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create|ModuleRegistration.cs]] - create - Centralized module registration extension
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.Migrations.csproj.create|App.Infrastructure.Migrations.csproj]] - create - EF Core migrations project

# Rules

## MUST:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Host.csproj.create#MUST|App.Host.csproj]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create#MUST|ModuleRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create#MUST|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.Migrations.csproj.create#MUST|App.Infrastructure.Migrations.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Queries.csproj.create#MUST|App.Queries.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create#MUST|BuildingBlocks.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Repository.create#MUST|Repository]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create#MUST|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create#MUST|{Module}.Api.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create#MUST|{Module}.Application.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create#MUST|{Module}.Domain.csproj]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create#MUST|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create#MUST|{Module}.Interfaces.csproj]]
- Each module has at least the base set of projects: Api, Application, Domain, Interfaces
- Other modules reference only {ModuleName}.Interfaces
- All cross-module writes go through MediatR command dispatch
- Tests colocated with module
- Every project belongs to exactly one layer

## MAY
- A specific pattern solution may add an additional project to a module when it needs project-level isolation that none of the base four projects can provide

## MUST NOT
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Host.csproj.create#MUST NOT|App.Host.csproj]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create#MUST NOT|ModuleRegistration.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create#MUST NOT|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.Migrations.csproj.create#MUST NOT|App.Infrastructure.Migrations.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Queries.csproj.create#MUST NOT|App.Queries.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create#MUST NOT|BuildingBlocks.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Repository.create#MUST NOT|Repository]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create#MUST NOT|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Api.csproj.create#MUST NOT|{Module}.Api.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create#MUST NOT|{Module}.Application.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create#MUST NOT|{Module}.Domain.csproj]]
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create#MUST NOT|{Entity}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create#MUST NOT|{Module}.Interfaces.csproj]]
- Module reference another module's Domain
- Module reference another module's Application
- Domain reference any other module's project
- Any module Domain reference another module's project
- Any module Api reference Domain or Application directly
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
- [ ] Module has at least the base set of projects: Api, Application, Domain, Interfaces
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
