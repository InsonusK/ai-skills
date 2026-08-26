---
name: plateau-service-with-validated-module-interaction--sln-service-with-validated-module-interaction
description: Repository/solution-level layout of the service-with-validated-module-interaction plateau
whenToUse: when adding, removing, or relocating a top-level project or module in this plateau's solution, or deciding where new code belongs at the repository level
domain: skill
type: template
plateau: service-with-validated-module-interaction
version: 20260822140000
tags:
  - skill/template/sln
  - plateau/service-with-validated-module-interaction
created_by:
  - "[[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
  - "[[../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]]"
  - "[[../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]"
  - "[[../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
  - "[[../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
  - "[[../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
---

# Structure

## Repository Structure
```
/src
  /Modules
    /{ModuleName}
      /[{ModuleName}.Api](./{Module}.Api/plateau-service-with-validated-module-interaction--csproj-module-api.skill.md)
      /[{ModuleName}.Application](./{Module}.Application/plateau-service-with-validated-module-interaction--csproj-module-application.skill.md)
      /[{ModuleName}.Application.Tests](./{Module}.Application.Tests/plateau-service-with-validated-module-interaction--csproj-module-application-tests.skill.md)
      /[{ModuleName}.Domain](./{Module}.Domain/plateau-service-with-validated-module-interaction--csproj-module-domain.skill.md)
      /[{ModuleName}.Domain.Tests](./{Module}.Domain.Tests/plateau-service-with-validated-module-interaction--csproj-module-domain-tests.skill.md)
      /[{ModuleName}.Interfaces](./{Module}.Interfaces/plateau-service-with-validated-module-interaction--csproj-module-interfaces.skill.md)
      /[{ModuleName}.Interfaces.Tests](./{Module}.Interfaces.Tests/plateau-service-with-validated-module-interaction--csproj-module-interfaces-tests.skill.md)
  /App
    /[App.Host](./App.Host/plateau-service-with-validated-module-interaction--csproj-app-host.skill.md)
  /[Shared](./Shared/plateau-service-with-validated-module-interaction--csproj-shared.skill.md)
  /[Shared.Tests](./Shared.Tests/plateau-service-with-validated-module-interaction--csproj-shared-tests.skill.md)
  /[BuildingBlocks](./BuildingBlocks/plateau-service-with-validated-module-interaction--csproj-building-blocks.skill.md)
  /[BuildingBlocks.Tests](./BuildingBlocks.Tests/plateau-service-with-validated-module-interaction--csproj-building-blocks-tests.skill.md)
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
- [[../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]], [[../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]], [[../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]], [[../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]], [[../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] — no repository-level change; see each project's own csproj skill for their contributions

## Directory and class skills
| `Directory\|file` | template link | Description |
| --- | --- | --- |
| /{ModuleName}.Api | [[./{Module}.Api/plateau-service-with-validated-module-interaction--csproj-module-api.skill.md\|csproj-module-api]] | HTTP endpoints, thin MediatR adapters |
| /{ModuleName}.Application | [[./{Module}.Application/plateau-service-with-validated-module-interaction--csproj-module-application.skill.md\|csproj-module-application]] | Orchestration — per-feature handlers/validators, reusable DTO/VO validators, module DI registration |
| /{ModuleName}.Application.Tests | [[./{Module}.Application.Tests/plateau-service-with-validated-module-interaction--csproj-module-application-tests.skill.md\|csproj-module-application-tests]] | Tests `{ModuleName}.Application` (and, transitively, `{ModuleName}.Domain`) |
| /{ModuleName}.Domain | [[./{Module}.Domain/plateau-service-with-validated-module-interaction--csproj-module-domain.skill.md\|csproj-module-domain]] | Entities, Value Objects, invariants, domain services |
| /{ModuleName}.Domain.Tests | [[./{Module}.Domain.Tests/plateau-service-with-validated-module-interaction--csproj-module-domain-tests.skill.md\|csproj-module-domain-tests]] | Tests `{ModuleName}.Domain` only |
| /{ModuleName}.Interfaces | [[./{Module}.Interfaces/plateau-service-with-validated-module-interaction--csproj-module-interfaces.skill.md\|csproj-module-interfaces]] | Public contracts — commands, Soft Value Objects |
| /{ModuleName}.Interfaces.Tests | [[./{Module}.Interfaces.Tests/plateau-service-with-validated-module-interaction--csproj-module-interfaces-tests.skill.md\|csproj-module-interfaces-tests]] | Tests `{ModuleName}.Interfaces` only |
| /App.Host | [[./App.Host/plateau-service-with-validated-module-interaction--csproj-app-host.skill.md\|csproj-app-host]] | Composition root |
| /Shared | [[./Shared/plateau-service-with-validated-module-interaction--csproj-shared.skill.md\|csproj-shared]] | Cross-cutting primitives, `ICommand`/`ICommand<T>` markers |
| /Shared.Tests | [[./Shared.Tests/plateau-service-with-validated-module-interaction--csproj-shared-tests.skill.md\|csproj-shared-tests]] | Tests `Shared` only |
| /BuildingBlocks | [[./BuildingBlocks/plateau-service-with-validated-module-interaction--csproj-building-blocks.skill.md\|csproj-building-blocks]] | Reusable framework patterns — exception handling, request validation pipeline |
| /BuildingBlocks.Tests | [[./BuildingBlocks.Tests/plateau-service-with-validated-module-interaction--csproj-building-blocks-tests.skill.md\|csproj-building-blocks-tests]] | Tests `BuildingBlocks` (and, transitively, `Shared`) |

__Applied solutions:__
- [[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../solutions/solution-sln-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]], [[../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]], [[../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]], [[../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]], [[../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] — no repository-level change; see each project's own csproj skill for their contributions

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
- [[../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]], [[../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]], [[../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]], [[../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]], [[../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] — no repository-level change; see each project's own csproj skill for their contributions
