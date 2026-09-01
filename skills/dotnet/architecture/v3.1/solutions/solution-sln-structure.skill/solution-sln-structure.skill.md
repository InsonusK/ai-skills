---
name: solution-sln-structure
description: Defines the full solution architecture including module boundaries, all non-module layers, their responsibilities, folder placement, and the complete dependency rules between all layers
whenToUse: when creating a new module, deciding a bounded-context boundary, or deciding which project a new file belongs in
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - concern/architecture
  - module
  - bounded-context
  - structure
  - layers
  - solution/sln-structure
  - stack/dotnet

creates:
  - "{Module}.Interfaces.csproj"
  - "{Module}.Application.csproj"
  - Shared.csproj
  - BuildingBlocks.csproj
  - App.Host.csproj
  - App.Host.DependencyInjection.ModuleRegistration.cs
extends:
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-central-package-management.skill/solution-central-package-management.skill.md|solution-central-package-management]]"
built_on_plateau:
adr:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/adr/module-project-set-extensibility.md|Base set of two projects (Interfaces, Application), extensible by pattern solutions]]"
---
# Goal
- Define a module as a self-contained bounded context that owns its domain, application logic, API surface, and public contracts
- Prevent hidden coupling between modules by enforcing interaction only through declared contracts
- Define the base project structure every module must follow, extensible by a specific pattern solution when it needs project-level isolation
- Define where modules live in the solution folder structure
- Define the full solution folder structure — where every layer lives on disk
- Define the responsibility of each non-module layer this solution creates: Shared, BuildingBlocks, App.Host — persistence-bearing layers (App.Infrastructure, App.Infrastructure.Migrations, App.Queries) are added by whichever solution first introduces persistence, not by this one
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
- Each module has the base set of **two** projects — `Interfaces` and `Application`. `{Module}.Domain` is added by [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] (DomainLogic), `{Module}.Api` by [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]] (an inbound-sync API); a pattern solution may add further projects when it needs project-level isolation
- Interfaces is the only public surface of a module — breaking changes must be versioned
- A module with no `{Module}.Domain` has no domain layer — its `Application` handlers orchestrate other modules and shape data, but hold no entities
- Tests are colocated with the module — no global tests folder
- Every project belongs to exactly one layer — no project spans multiple layers
- Dependencies flow inward — outer layers depend on inner layers, never the reverse
- App.Host is the only composition root — it is the single place that wires everything together
- Shared and BuildingBlocks have no business logic — Shared defines common contracts, BuildingBlocks implements technical patterns
- Every project references NuGet packages versionlessly, per [[skills/dotnet/architecture/v3.1/solutions/solution-central-package-management.skill/solution-central-package-management.skill.md|solution-central-package-management]]
- This solution alone describes a service with no domain layer, no persistence and no API surface — those projects appear only when the matching feature is selected

# Adr
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/adr/module-project-set-extensibility.md|Base set of two projects (Interfaces, Application), extensible by pattern solutions]]
  - Selected variant: guaranteed base = `Interfaces` + `Application`; `Domain` and `Api` are pattern-solution additions, discoverable through the solution dependency graph, not enumerated here

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-central-package-management.skill/solution-central-package-management.skill.md|solution-central-package-management]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-central-package-management.skill/Implementation/Directory.Packages.props.create.md|Directory.Packages.props]] - every csproj this solution creates carries versionless `<PackageReference>` only

# Template Skill Mutations

REPOSITORY
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/Repository.create.md|App Repository structure]]

PROJECT
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj]] - create - Common cross-cutting contracts (Result, Exceptions, base interfaces). No implementation, no business logic.
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj]] - create - Reusable technical patterns (MediatR behaviors). Consumes interfaces from Shared.
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj]] - create - Module public contracts project
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj]] - create - Module orchestration project
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj]] - create - Composition root project
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs]] - create - Centralized module registration extension

`{Module}.Domain` is created by `solution-domain-behaviour`, `{Module}.Api` by `solution-api-project`. `App.Infrastructure`, `App.Infrastructure.Migrations`, and `App.Queries` are created by whichever solution first introduces persistence. None of them are created here.

# Rules

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md#MUST|App.Host.csproj]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md#MUST|ModuleRegistration.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create.md#MUST|BuildingBlocks.csproj]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/Repository.create.md#MUST|Repository]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md#MUST|Shared.csproj]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md#MUST|{Module}.Application.csproj]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md#MUST|{Module}.Interfaces.csproj]]
- Give every module at least `Interfaces` and `Application`; add `Domain`/`Api`/others only through the pattern solution that owns them.
  - Risk: an empty `Domain` project scaffolded up front invites anemic entities and blurs whether the module actually has a domain layer.
  - Fix: create the two base projects; let `solution-domain-behaviour` / `solution-api-project` add theirs when their feature is selected.
- Let another module reference only `{Module}.Interfaces`, never its `Application` or `Domain`.
  - Risk: a compile-time reference into another module's internals couples the two and defeats the bounded-context boundary.
  - Fix: cross-module interaction goes through MediatR against `{Module}.Interfaces` contracts only.
- Route every cross-module write through MediatR dispatch, never a direct call.
  - Risk: a direct method call bypasses the pipeline (validation, exception handling) and creates a hidden dependency.
  - Fix: send a Command/Notification defined in the target module's `Interfaces`.
- Keep tests colocated with their module — no global `/tests` folder.
  - Risk: a global test folder detaches a module's tests from its lifecycle and its ownership.
  - Fix: one test project per production project, next to it.
- Keep every project in exactly one layer.
  - Risk: a project spanning layers makes the dependency rules unenforceable.
  - Fix: split it.
- Reference NuGet packages versionlessly; declare versions in `Directory.Packages.props`.
  - Risk: inline versions drift between projects.
  - Fix: per [[skills/dotnet/architecture/v3.1/solutions/solution-central-package-management.skill/solution-central-package-management.skill.md|solution-central-package-management]].
- Never let `Shared` or `BuildingBlocks` hold business logic; never let `App.Host` hold anything but wiring.
  - Risk: business rules leak into cross-cutting layers where no module owns them.
  - Fix: `Shared` = contracts, `BuildingBlocks` = technical patterns, `App.Host` = composition only.

## MAY
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md#MAY|App.Host.csproj]]
- A pattern solution may add a project to a module when it needs project-level isolation the base projects cannot give.

# Check list
- [ ] Module folder exists under `/src/Modules/{ModuleName}`.
- [ ] Module has at least `Interfaces` and `Application`; `Domain`/`Api` exist only if their feature is selected.
- [ ] `Interfaces` has no project dependencies.
- [ ] No project references another module's `Application` or `Domain`.
- [ ] Tests colocated with module projects.
- [ ] Solution folder structure matches the defined layout.
- [ ] `Shared` has no project references and contains only contracts.
- [ ] `BuildingBlocks` references only `Shared` and contains only pattern implementations.
- [ ] `App.Host` is the only project referencing all modules; it holds wiring only.
- [ ] Pipeline behaviors registered in `App.Host` only.
- [ ] Every `<PackageReference>` is versionless (`Directory.Packages.props` owns versions).
- [ ] No `{Module}.Domain`, `{Module}.Api`, `App.Infrastructure`, `App.Infrastructure.Migrations`, or `App.Queries` project exists unless the solution that owns it created it.
