---
name: plateau-stateless-non-interactive-service--sln-stateless-non-interactive-service
description: Repository/solution-level layout of the stateless-non-interactive-service plateau
whenToUse: when adding, removing, or relocating a top-level project or module in this plateau's solution, or deciding where new code belongs at the repository level
domain: skill
type: template
plateau: stateless-non-interactive-service
version: 20260822120000
tags:
  - skill/template/sln
  - plateau/stateless-non-interactive-service
created_by:
  - "[[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
---

# Structure

## Repository Structure
```
/src
  /Modules
    /{ModuleName}
      /[{ModuleName}.Api](./{Module}.Api/plateau-stateless-non-interactive-service--csproj-module-api.skill.md)
      /[{ModuleName}.Application](./{Module}.Application/plateau-stateless-non-interactive-service--csproj-module-application.skill.md)
      /[{ModuleName}.Application.Tests](./{Module}.Application.Tests/plateau-stateless-non-interactive-service--csproj-module-application-tests.skill.md)
      /[{ModuleName}.Domain](./{Module}.Domain/plateau-stateless-non-interactive-service--csproj-module-domain.skill.md)
      /[{ModuleName}.Domain.Tests](./{Module}.Domain.Tests/plateau-stateless-non-interactive-service--csproj-module-domain-tests.skill.md)
      /[{ModuleName}.Interfaces](./{Module}.Interfaces/plateau-stateless-non-interactive-service--csproj-module-interfaces.skill.md)
      /[{ModuleName}.Interfaces.Tests](./{Module}.Interfaces.Tests/plateau-stateless-non-interactive-service--csproj-module-interfaces-tests.skill.md)
  /App
    /[App.Host](./App.Host/plateau-stateless-non-interactive-service--csproj-app-host.skill.md)
  /[Shared](./Shared/plateau-stateless-non-interactive-service--csproj-shared.skill.md)
  /[Shared.Tests](./Shared.Tests/plateau-stateless-non-interactive-service--csproj-shared-tests.skill.md)
  /[BuildingBlocks](./BuildingBlocks/plateau-stateless-non-interactive-service--csproj-building-blocks.skill.md)
  /[BuildingBlocks.Tests](./BuildingBlocks.Tests/plateau-stateless-non-interactive-service--csproj-building-blocks-tests.skill.md)
/report-template
  index.html
/scripts
  unit-test.sh
  mutation-test.sh
  test-report.sh
Makefile
README.md
```

Every production project has its own dedicated test project, mirroring its Allowed Dependencies exactly — never one combined test project per module. `{ModuleName}.Api` has no dedicated test project (thin MediatR adapter, no business logic of its own). See [[../../../solutions/solution-dotnet-conformance-testing.skill/adr/test-project-per-production-project.md|solution-conformance-testing's ADR]] for why.

`App.Infrastructure`, `App.Infrastructure.Migrations`, and `App.Queries` are not part of this plateau — this plateau describes a service with no persistence; a persistence-introducing plateau adds them.

__Applied solutions:__
- [[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../solutions/solution-sln-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Repository.extend.md|Repository.extend]]

## Directory and class skills
| `Directory\|file` | template link | Description |
| --- | --- | --- |
| /{ModuleName}.Api | [[./{Module}.Api/plateau-stateless-non-interactive-service--csproj-module-api.skill.md\|csproj-module-api]] | HTTP endpoints, thin MediatR adapters |
| /{ModuleName}.Application | [[./{Module}.Application/plateau-stateless-non-interactive-service--csproj-module-application.skill.md\|csproj-module-application]] | Orchestration — handlers, validators, specs |
| /{ModuleName}.Application.Tests | [[./{Module}.Application.Tests/plateau-stateless-non-interactive-service--csproj-module-application-tests.skill.md\|csproj-module-application-tests]] | Tests `{ModuleName}.Application` (and, transitively, `{ModuleName}.Domain`) |
| /{ModuleName}.Domain | [[./{Module}.Domain/plateau-stateless-non-interactive-service--csproj-module-domain.skill.md\|csproj-module-domain]] | Entities, invariants |
| /{ModuleName}.Domain.Tests | [[./{Module}.Domain.Tests/plateau-stateless-non-interactive-service--csproj-module-domain-tests.skill.md\|csproj-module-domain-tests]] | Tests `{ModuleName}.Domain` only |
| /{ModuleName}.Interfaces | [[./{Module}.Interfaces/plateau-stateless-non-interactive-service--csproj-module-interfaces.skill.md\|csproj-module-interfaces]] | Public contracts |
| /{ModuleName}.Interfaces.Tests | [[./{Module}.Interfaces.Tests/plateau-stateless-non-interactive-service--csproj-module-interfaces-tests.skill.md\|csproj-module-interfaces-tests]] | Tests `{ModuleName}.Interfaces` only |
| /App.Host | [[./App.Host/plateau-stateless-non-interactive-service--csproj-app-host.skill.md\|csproj-app-host]] | Composition root |
| /Shared | [[./Shared/plateau-stateless-non-interactive-service--csproj-shared.skill.md\|csproj-shared]] | Cross-cutting primitives |
| /Shared.Tests | [[./Shared.Tests/plateau-stateless-non-interactive-service--csproj-shared-tests.skill.md\|csproj-shared-tests]] | Tests `Shared` only |
| /BuildingBlocks | [[./BuildingBlocks/plateau-stateless-non-interactive-service--csproj-building-blocks.skill.md\|csproj-building-blocks]] | Reusable framework patterns |
| /BuildingBlocks.Tests | [[./BuildingBlocks.Tests/plateau-stateless-non-interactive-service--csproj-building-blocks-tests.skill.md\|csproj-building-blocks-tests]] | Tests `BuildingBlocks` (and, transitively, `Shared`) |

__Applied solutions:__
- [[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../solutions/solution-sln-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Repository.extend.md|Repository.extend]]

# Rules
MUST:
- Every module lives under `/src/Modules/{ModuleName}`
- Every module has at least the base set of four projects: Api, Application, Domain, Interfaces
- Every production project that has one (Domain, Application, Interfaces, Shared, BuildingBlocks) has exactly one dedicated test project mirroring its Allowed Dependencies exactly — never one combined test project per module, never a project with wider references than its production counterpart
- Tests live alongside their production project — not in a global `/tests` folder
- `unit-test`, `mutation-test`, `test-report`, and `test-and-report` `make` targets exist at the repository root, run across every test project, and behave exactly as [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] defines
MAY:
- A specific pattern solution add an additional project to a module when it needs project-level isolation
MUST NOT:
- Module projects exist outside `/src/Modules`
- Module have fewer than the base four projects
- Module have an additional project that no specific pattern solution defines
- `{ModuleName}.Api` have a dedicated test project
- CI or a developer call `dotnet test`/`dotnet-stryker` directly instead of through the `Makefile`'s `make` targets

__Applied solutions:__
- [[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../solutions/solution-sln-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
