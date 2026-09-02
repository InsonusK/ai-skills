---
name: plateau-core--sln-core
description: Repository/solution-level layout of the plateau-core plateau — Central Package Management, the two-project module, the App/Shared/BuildingBlocks layers, and one test project per production project
whenToUse: when adding, removing, or relocating a top-level project in a plateau-core repository, deciding which existing project a new class belongs in, or reviewing the solution-level layout and the Central Package Management setup
domain: skill
type: template
plateau: core
version: 20260902000000
tags:
  - skill/template/sln
  - plateau/core
created_by:
  - "[[../../../solutions/solution-central-package-management.skill/solution-central-package-management.skill.md|solution-central-package-management]]"
  - "[[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
---

# Structure

## Repository Structure
```
/ (repository root)
  Directory.Packages.props        — every NuGet version, pinned once (ManagePackageVersionsCentrally)
  Directory.Build.props           — net10.0, ImplicitUsings, Nullable, TreatWarningsAsErrors
  {Solution}.slnx                 — .NET 10 XML solution format
  Makefile                        — unit-test / mutation-test / test-report / test-and-report
/src
  /Modules
    /{ModuleName}
      /[{ModuleName}.Interfaces](./{Module}.Interfaces/plateau-core--csproj-module-interfaces.skill.md)
      /[{ModuleName}.Application](./{Module}.Application/plateau-core--csproj-module-application.skill.md)
  /App
    /[App.Host](./App.Host/plateau-core--csproj-app-host.skill.md)
  /[Shared](./Shared/plateau-core--csproj-shared.skill.md)
  /[BuildingBlocks](./BuildingBlocks/plateau-core--csproj-building-blocks.skill.md)
/tests
  /[Shared.Tests](./Shared.Tests/plateau-core--csproj-shared-tests.skill.md)
  /[BuildingBlocks.Tests](./BuildingBlocks.Tests/plateau-core--csproj-building-blocks-tests.skill.md)
  /[{ModuleName}.Interfaces.Tests](./{Module}.Interfaces.Tests/plateau-core--csproj-module-interfaces-tests.skill.md)
  /[{ModuleName}.Application.Tests](./{Module}.Application.Tests/plateau-core--csproj-module-application-tests.skill.md)
/scripts
  unit-test.sh   mutation-test.sh   test-report.sh
/report-template
  index.html
```

`{ModuleName}.Domain` and `{ModuleName}.Api` are **not** in the plateau-core layout — a module is exactly `Interfaces` + `Application` until a feature adds more:
- `{ModuleName}.Domain` arrives with `solution-domain-behaviour` (VP1).
- `{ModuleName}.Api` arrives with `solution-api-project` (VP8).
- `App.Infrastructure`, `App.Infrastructure.Migrations`, `App.Queries` arrive with the first persistence feature (VP2).

Because there is no `{ModuleName}.Domain`, there is no `{ModuleName}.Domain.Tests` at this plateau either — the conformance solution creates one test project per *existing* production project.

__Applied solutions:__
- [[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../solutions/solution-sln-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[../../../solutions/solution-central-package-management.skill/solution-central-package-management.skill.md|solution-central-package-management]] - [[../../../solutions/solution-central-package-management.skill/Implementation/Directory.Packages.props.create.md|Directory.Packages.props.create]]
- [[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Repository.extend.md|Repository.extend]]

## Directory and class skills
| `Directory\|file` | template link | Description |
| --- | --- | --- |
| /src/Modules/{ModuleName}.Interfaces | [[./{Module}.Interfaces/plateau-core--csproj-module-interfaces.skill.md\|csproj-module-interfaces]] | Public contracts — commands, queries, notifications, Soft VOs, DTOs |
| /src/Modules/{ModuleName}.Application | [[./{Module}.Application/plateau-core--csproj-module-application.skill.md\|csproj-module-application]] | Orchestration — handlers, validators, module registration |
| /src/App/App.Host | [[./App.Host/plateau-core--csproj-app-host.skill.md\|csproj-app-host]] | Composition root — logging, modules, pipeline |
| /src/Shared | [[./Shared/plateau-core--csproj-shared.skill.md\|csproj-shared]] | Cross-cutting contracts — MediatR markers, LogEvents |
| /src/BuildingBlocks | [[./BuildingBlocks/plateau-core--csproj-building-blocks.skill.md\|csproj-building-blocks]] | Reusable framework patterns — pipeline behaviors |
| /tests/Shared.Tests | [[./Shared.Tests/plateau-core--csproj-shared-tests.skill.md\|csproj-shared-tests]] | Tests `Shared` only |
| /tests/BuildingBlocks.Tests | [[./BuildingBlocks.Tests/plateau-core--csproj-building-blocks-tests.skill.md\|csproj-building-blocks-tests]] | Tests `BuildingBlocks` (and transitively `Shared`) |
| /tests/{ModuleName}.Interfaces.Tests | [[./{Module}.Interfaces.Tests/plateau-core--csproj-module-interfaces-tests.skill.md\|csproj-module-interfaces-tests]] | Tests `{ModuleName}.Interfaces` only |
| /tests/{ModuleName}.Application.Tests | [[./{Module}.Application.Tests/plateau-core--csproj-module-application-tests.skill.md\|csproj-module-application-tests]] | Tests `{ModuleName}.Application` (and transitively `{ModuleName}.Interfaces`) |

__Applied solutions:__
- [[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../solutions/solution-sln-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Repository.extend.md|Repository.extend]]

## NuGet Packages
Every version is declared once in `Directory.Packages.props`; every `<PackageReference>` in every csproj is versionless.

| Package | Group | Purpose |
| --- | --- | --- |
| MediatR | MediatR | `ISender`/`IPublisher`, `IRequest<T>`, `IPipelineBehavior` |
| FluentValidation | Validation | `AbstractValidator<T>`, `IValidator<T>` |
| FluentValidation.DependencyInjectionExtensions | Validation | `AddValidatorsFromAssembly` |
| Ardalis.Result | Result | `Result` / `Result<T>` / `IResult` |
| Microsoft.Extensions.Hosting | Hosting | `Host.CreateApplicationBuilder` |
| Microsoft.Extensions.Logging.Abstractions | Logging | `ILogger<T>`, `EventId` |
| Microsoft.Extensions.Logging.Console | Logging | console provider (App.Host only) |
| Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio, Reqnroll.xUnit, coverlet.collector | Test | test projects only |

__Applied solutions:__
- [[../../../solutions/solution-central-package-management.skill/solution-central-package-management.skill.md|solution-central-package-management]] - [[../../../solutions/solution-central-package-management.skill/Implementation/Directory.Packages.props.create.md|Directory.Packages.props.create]]

# Rules
MUST:
- Put every module under `/src/Modules/{ModuleName}`, with exactly `{ModuleName}.Interfaces` + `{ModuleName}.Application` at creation — never a pre-scaffolded empty `Domain` or `Api`.
- Keep the dependency arrows: `{Module}.Application → {Module}.Interfaces, Shared, BuildingBlocks`; `{Module}.Interfaces → Shared`; `BuildingBlocks → Shared`; `App.Host → every {Module}.Application, BuildingBlocks`; `Shared → nothing`. Across modules, reference only `{Module-B}.Interfaces`.
- Declare every NuGet version once in `Directory.Packages.props` with `ManagePackageVersionsCentrally` true; keep every `<PackageReference>` versionless. Add the central `<PackageVersion>` in the same change as the reference.
- Give every production project that has one exactly one dedicated test project mirroring its Allowed Dependencies — never one combined test project per module, never a test project reaching wider than its production counterpart.
- Expose `unit-test`, `mutation-test`, `test-report`, and `test-and-report` `make` targets at the repository root that behave exactly as [[../../../../../../common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] defines; never call `dotnet test` / `dotnet-stryker` directly from CI or scripts.
- Never let `Shared` take a project reference, and never let a cross-module reference target anything but `{Module}.Interfaces`.
MAY:
- A pattern solution may add a project to a module (e.g. `{Module}.Domain.Rules`) when it needs isolation the base projects cannot give.

__Applied solutions:__
- [[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../solutions/solution-sln-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[../../../solutions/solution-central-package-management.skill/solution-central-package-management.skill.md|solution-central-package-management]] - [[../../../solutions/solution-central-package-management.skill/Implementation/Directory.Packages.props.create.md|Directory.Packages.props.create]]
- [[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Repository.extend.md|Repository.extend]]

# Check list
- [ ] `Directory.Packages.props` at the root, `ManagePackageVersionsCentrally` true, every referenced package has a `<PackageVersion>`, no csproj carries a `Version=` on a `<PackageReference>`.
- [ ] Every module is `/src/Modules/{ModuleName}/{ModuleName}.Interfaces` + `.Application` and nothing else.
- [ ] No `{ModuleName}.Domain` / `{ModuleName}.Api` / `App.Infrastructure` / `App.Queries`.
- [ ] `Shared` has zero project references; no cross-module reference targets anything but `{Module}.Interfaces`.
- [ ] `Shared.Tests`, `BuildingBlocks.Tests`, `{ModuleName}.Interfaces.Tests`, `{ModuleName}.Application.Tests` exist; no `{ModuleName}.Domain.Tests`.
- [ ] `make unit-test` is green.
