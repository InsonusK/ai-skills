---
name: plateau-service-with-api--sln-service-with-api
description: Repository/solution-level layout of the service-with-api plateau
whenToUse: when adding, removing, or relocating a top-level project or module in this plateau's solution, or deciding where new code belongs at the repository level
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/sln
  - plateau/service-with-api
created_by:
  - "[[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
  - "[[../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]]"
  - "[[../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]"
  - "[[../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
  - "[[../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
  - "[[../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
  - "[[../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]]"
  - "[[../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]]"
---

# Structure

## Repository Structure
```
/src
  /Modules
    /{ModuleName}
      /[{ModuleName}.Api](./{Module}.Api/plateau-service-with-api--csproj-module-api.skill.md)
      /[{ModuleName}.Application](./{Module}.Application/plateau-service-with-api--csproj-module-application.skill.md)
      /[{ModuleName}.Application.Tests](./{Module}.Application.Tests/plateau-service-with-api--csproj-module-application-tests.skill.md)
      /[{ModuleName}.Domain](./{Module}.Domain/plateau-service-with-api--csproj-module-domain.skill.md)
      /[{ModuleName}.Domain.Tests](./{Module}.Domain.Tests/plateau-service-with-api--csproj-module-domain-tests.skill.md)
      /[{ModuleName}.Interfaces](./{Module}.Interfaces/plateau-service-with-api--csproj-module-interfaces.skill.md)
      /[{ModuleName}.Interfaces.Tests](./{Module}.Interfaces.Tests/plateau-service-with-api--csproj-module-interfaces-tests.skill.md)
  /App
    /[App.Host](./App.Host/plateau-service-with-api--csproj-app-host.skill.md)
  /[Shared](./Shared/plateau-service-with-api--csproj-shared.skill.md)
  /[Shared.Tests](./Shared.Tests/plateau-service-with-api--csproj-shared-tests.skill.md)
  /[BuildingBlocks](./BuildingBlocks/plateau-service-with-api--csproj-building-blocks.skill.md)
  /[BuildingBlocks.Tests](./BuildingBlocks.Tests/plateau-service-with-api--csproj-building-blocks-tests.skill.md)
/report-template
  index.html
/scripts
  unit-test.sh
  mutation-test.sh
  test-report.sh
Makefile
README.md
```

Every production project has its own dedicated test project, mirroring its Allowed Dependencies exactly — never one combined test project per module. `{ModuleName}.Api` still has no dedicated test project at this plateau, carried over from [[../../../solutions/solution-dotnet-conformance-testing.skill/adr/test-project-per-production-project.md|solution-conformance-testing's ADR]] — a decision made when Api was a bare placeholder with nothing to test. `ResultExtensions.ToProblemDetails()`/`ToRpcException()` are now real, pure, testable mapping functions with no coverage anywhere in this plateau; this is a known, disclosed gap in this plateau's own conformance story, not a silent one — closing it (an `{ModuleName}.Api.Tests` project) is deferred, not decided against.

`App.Infrastructure`, `App.Infrastructure.Migrations`, and `App.Queries` are still not part of this plateau — this plateau adds an external surface, not persistence; a persistence-introducing plateau (`plateau-statefull-service`) adds them, and is expected to already be composed (via `plateau-v1`) once this plateau's `GET`/read endpoints are meaningful — see [[../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md#boundaries|solution-http-api-publication's Boundaries]].

`App.Host` is now an ASP.NET Core web host (`WebApplication.CreateBuilder`), not the plain console host (`Host.CreateApplicationBuilder`) the foundation plateau's example used — see [[./App.Host/plateau-service-with-api--csproj-app-host.skill.md|csproj-app-host]].

__Applied solutions:__
- [[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../solutions/solution-sln-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]], [[../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]], [[../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]], [[../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]], [[../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] — no repository-level change; see each project's own csproj skill for their contributions
- [[../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]], [[../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] — no repository-level change; see each project's own csproj skill for their contributions
- [[../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]], [[../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - independent, optional additions to `{ModuleName}.Api`/`App.Host` — see each project's own csproj skill

## Directory and class skills
| `Directory\|file` | template link | Description |
| --- | --- | --- |
| /{ModuleName}.Api | [[./{Module}.Api/plateau-service-with-api--csproj-module-api.skill.md\|csproj-module-api]] | REST Controllers, Minimal API, and/or gRPC services — thin MediatR adapters, apply either or both |
| /{ModuleName}.Application | [[./{Module}.Application/plateau-service-with-api--csproj-module-application.skill.md\|csproj-module-application]] | Orchestration — per-feature handlers/validators, reusable DTO/VO validators, module DI registration |
| /{ModuleName}.Application.Tests | [[./{Module}.Application.Tests/plateau-service-with-api--csproj-module-application-tests.skill.md\|csproj-module-application-tests]] | Tests `{ModuleName}.Application` (and, transitively, `{ModuleName}.Domain`) |
| /{ModuleName}.Domain | [[./{Module}.Domain/plateau-service-with-api--csproj-module-domain.skill.md\|csproj-module-domain]] | Entities, Value Objects, invariants, domain services |
| /{ModuleName}.Domain.Tests | [[./{Module}.Domain.Tests/plateau-service-with-api--csproj-module-domain-tests.skill.md\|csproj-module-domain-tests]] | Tests `{ModuleName}.Domain` only |
| /{ModuleName}.Interfaces | [[./{Module}.Interfaces/plateau-service-with-api--csproj-module-interfaces.skill.md\|csproj-module-interfaces]] | Public contracts — commands, Soft Value Objects |
| /{ModuleName}.Interfaces.Tests | [[./{Module}.Interfaces.Tests/plateau-service-with-api--csproj-module-interfaces-tests.skill.md\|csproj-module-interfaces-tests]] | Tests `{ModuleName}.Interfaces` only |
| /App.Host | [[./App.Host/plateau-service-with-api--csproj-app-host.skill.md\|csproj-app-host]] | Composition root |
| /Shared | [[./Shared/plateau-service-with-api--csproj-shared.skill.md\|csproj-shared]] | Cross-cutting primitives, `ICommand`/`ICommand<T>` markers |
| /Shared.Tests | [[./Shared.Tests/plateau-service-with-api--csproj-shared-tests.skill.md\|csproj-shared-tests]] | Tests `Shared` only |
| /BuildingBlocks | [[./BuildingBlocks/plateau-service-with-api--csproj-building-blocks.skill.md\|csproj-building-blocks]] | Reusable framework patterns — exception handling, request validation pipeline |
| /BuildingBlocks.Tests | [[./BuildingBlocks.Tests/plateau-service-with-api--csproj-building-blocks-tests.skill.md\|csproj-building-blocks-tests]] | Tests `BuildingBlocks` (and, transitively, `Shared`) |

__Applied solutions:__
- [[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../solutions/solution-sln-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]], [[../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]], [[../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]], [[../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]], [[../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] — no repository-level change; see each project's own csproj skill for their contributions
- [[../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]], [[../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] — no repository-level change; see each project's own csproj skill for their contributions

# Rules
MUST:
- Every module lives under `/src/Modules/{ModuleName}`
- Every module has at least the base set of four projects: Api, Application, Domain, Interfaces
- Every production project that has one (Domain, Application, Interfaces, Shared, BuildingBlocks) has exactly one dedicated test project mirroring its Allowed Dependencies exactly — never one combined test project per module, never a project with wider references than its production counterpart
- Tests live alongside their production project — not in a global `/tests` folder
- `unit-test`, `mutation-test`, `test-report`, and `test-and-report` `make` targets exist at the repository root, run across every test project, and behave exactly as [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] defines
- A module actually composing this plateau apply at least one of `solution-http-api-publication`/`solution-grpc-integration` — that's the point of the plateau existing for that module
MAY:
- A specific pattern solution add an additional project to a module when it needs project-level isolation
- A module apply both `solution-http-api-publication` and `solution-grpc-integration` — neither requires or excludes the other
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
- [[../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]], [[../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] — no repository-level change; see each project's own csproj skill for their contributions
