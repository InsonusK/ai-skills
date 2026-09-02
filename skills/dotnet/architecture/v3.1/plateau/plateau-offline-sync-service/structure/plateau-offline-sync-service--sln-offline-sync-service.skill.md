---
name: plateau-offline-sync-service--sln-offline-sync-service
description: Repository/solution-level layout of the plateau-offline-sync-service plateau — plateau-core's layout plus {Module}.Domain, {Module}.Api, App.Infrastructure, App.Queries and their test projects
whenToUse: when adding, removing, or relocating a top-level project in a plateau-domain-service repository, deciding which existing project a new class belongs in, or reviewing the solution-level layout and the Central Package Management setup
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/sln
  - plateau/offline-sync-service
created_by:
  - "[[../../../solutions/solution-central-package-management.skill/solution-central-package-management.skill.md|solution-central-package-management]]"
  - "[[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
  - "[[../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
  - "[[../../../solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]"
  - "[[../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]]"
  - "[[../../../solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]]"
---

# Structure

## Repository Structure
```
/ (repository root)
  Directory.Packages.props        — every NuGet version, pinned once (ManagePackageVersionsCentrally)
  Directory.Build.props           — net10.0, ImplicitUsings, Nullable, TreatWarningsAsErrors
  global.json                     — test.runner = Microsoft.Testing.Platform
  {Solution}.slnx
  Makefile
/src
  /Modules/{ModuleName}
    /[{ModuleName}.Interfaces](./{Module}.Interfaces/plateau-offline-sync-service--csproj-module-interfaces.skill.md)
    /[{ModuleName}.Application](./{Module}.Application/plateau-offline-sync-service--csproj-module-application.skill.md)
    /[{ModuleName}.Domain](./{Module}.Domain/plateau-offline-sync-service--csproj-module-domain.skill.md)          — VP1
    /[{ModuleName}.Api](./{Module}.Api/plateau-offline-sync-service--csproj-module-api.skill.md)                   — VP8/VP9
  /App
    /[App.Host](./App.Host/plateau-offline-sync-service--csproj-app-host.skill.md)
    /[App.Infrastructure](./App.Infrastructure/plateau-offline-sync-service--csproj-app-infrastructure.skill.md)   — VP2 / VP5 / VP11
    /[App.Queries](./App.Queries/plateau-offline-sync-service--csproj-app-queries.skill.md)                        — VP2 (cross-module reads)
  /[Shared](./Shared/plateau-offline-sync-service--csproj-shared.skill.md)
  /[BuildingBlocks](./BuildingBlocks/plateau-offline-sync-service--csproj-building-blocks.skill.md)
/tests
  /[Shared.Tests](./Shared.Tests/plateau-offline-sync-service--csproj-shared-tests.skill.md)
  /[BuildingBlocks.Tests](./BuildingBlocks.Tests/plateau-offline-sync-service--csproj-building-blocks-tests.skill.md)
  /[{ModuleName}.Interfaces.Tests](./{Module}.Interfaces.Tests/plateau-offline-sync-service--csproj-module-interfaces-tests.skill.md)
  /[{ModuleName}.Application.Tests](./{Module}.Application.Tests/plateau-offline-sync-service--csproj-module-application-tests.skill.md)
  /[{ModuleName}.Domain.Tests](./{Module}.Domain.Tests/plateau-offline-sync-service--csproj-module-domain-tests.skill.md)     — with VP1
/scripts   unit-test.sh   mutation-test.sh   test-report.sh
/report-template   index.html
```

`{ModuleName}.Api` has no dedicated test project (thin adapter, no logic). `{ModuleName}.Domain.Tests` exists only once `{ModuleName}.Domain` does.

__Applied solutions:__
- [[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../solutions/solution-sln-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[../../../solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]] - [[../../../solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj.create]]
- [[../../../solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]] - [[../../../solutions/solution-api-project.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]
- [[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Repository.extend.md|Repository.extend]]

## Directory and class skills
| `Directory\|file` | template link | Description |
| --- | --- | --- |
| /src/Modules/{ModuleName}.Interfaces | [[./{Module}.Interfaces/plateau-offline-sync-service--csproj-module-interfaces.skill.md\|csproj-module-interfaces]] | Public contracts (+ concurrency / timestamp interfaces on commands) |
| /src/Modules/{ModuleName}.Application | [[./{Module}.Application/plateau-offline-sync-service--csproj-module-application.skill.md\|csproj-module-application]] | Orchestration — handlers (load/stage), validators, specs, version resolvers |
| /src/Modules/{ModuleName}.Domain | [[./{Module}.Domain/plateau-offline-sync-service--csproj-module-domain.skill.md\|csproj-module-domain]] | Entities, strict Value Objects, domain services, EF configs |
| /src/Modules/{ModuleName}.Api | [[./{Module}.Api/plateau-offline-sync-service--csproj-module-api.skill.md\|csproj-module-api]] | Thin inbound-API adapters (HTTP / gRPC) |
| /src/App/App.Host | [[./App.Host/plateau-offline-sync-service--csproj-app-host.skill.md\|csproj-app-host]] | Composition root — logging, modules, pipeline, infrastructure, API |
| /src/App/App.Infrastructure | [[./App.Infrastructure/plateau-offline-sync-service--csproj-app-infrastructure.skill.md\|csproj-app-infrastructure]] | AppDbContext, Repository, UnitOfWork, version-resolver factory, gRPC clients |
| /src/App/App.Queries | [[./App.Queries/plateau-offline-sync-service--csproj-app-queries.skill.md\|csproj-app-queries]] | Cross-module JOIN projection specs |
| /src/Shared | [[./Shared/plateau-offline-sync-service--csproj-shared.skill.md\|csproj-shared]] | Cross-cutting contracts — markers, LogEvents, concurrency / timestamp / repository / unit-of-work / client interfaces |
| /src/BuildingBlocks | [[./BuildingBlocks/plateau-offline-sync-service--csproj-building-blocks.skill.md\|csproj-building-blocks]] | Pipeline behaviors (validation, exception, concurrency, unit-of-work) |
| /tests/Shared.Tests | [[./Shared.Tests/plateau-offline-sync-service--csproj-shared-tests.skill.md\|csproj-shared-tests]] | Tests `Shared` only |
| /tests/BuildingBlocks.Tests | [[./BuildingBlocks.Tests/plateau-offline-sync-service--csproj-building-blocks-tests.skill.md\|csproj-building-blocks-tests]] | Tests `BuildingBlocks` |
| /tests/{ModuleName}.Interfaces.Tests | [[./{Module}.Interfaces.Tests/plateau-offline-sync-service--csproj-module-interfaces-tests.skill.md\|csproj-module-interfaces-tests]] | Tests `{ModuleName}.Interfaces` |
| /tests/{ModuleName}.Application.Tests | [[./{Module}.Application.Tests/plateau-offline-sync-service--csproj-module-application-tests.skill.md\|csproj-module-application-tests]] | Tests `{ModuleName}.Application` (+ `{ModuleName}.Domain`) |
| /tests/{ModuleName}.Domain.Tests | [[./{Module}.Domain.Tests/plateau-offline-sync-service--csproj-module-domain-tests.skill.md\|csproj-module-domain-tests]] | Tests `{ModuleName}.Domain` only |

__Applied solutions:__
- [[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../solutions/solution-sln-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Repository.extend.md|Repository.extend]]

## NuGet Packages
Every version is declared once in `Directory.Packages.props`; every `<PackageReference>` is versionless. On top of plateau-core's set (MediatR, FluentValidation, Ardalis.Result, Hosting, Logging, test packages):

| Package | Group | Purpose |
| --- | --- | --- |
| Microsoft.EntityFrameworkCore (+ provider) | Persistence | `AppDbContext`, entity configs, migrations |
| Ardalis.Specification, Ardalis.Specification.EntityFrameworkCore | Persistence | `Specification<T>`, `RepositoryBase<T>` |
| Grpc.Net.ClientFactory, Google.Protobuf, Grpc.Tools | gRPC | outbound client stubs |

__Applied solutions:__
- [[../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[../../../solutions/solution-central-package-management.skill/solution-central-package-management.skill.md|solution-central-package-management]] - [[../../../solutions/solution-central-package-management.skill/Implementation/Directory.Packages.props.create.md|Directory.Packages.props.create]]

# Rules
MUST:
- Keep plateau-core's dependency arrows, and add: `{Module}.Domain → Shared, {Module}.Interfaces` (+ EF Core for configs); `{Module}.Application → {Module}.Domain`; `{Module}.Api → {Module}.Interfaces, Shared, BuildingBlocks` only; `App.Infrastructure → Shared, BuildingBlocks, every {Module}.Domain/Interfaces`; `App.Queries → Shared, every {Module}.Domain/Interfaces`; `App.Host → App.Infrastructure`. `App.Infrastructure` is referenced only by `App.Host`.
- Keep exactly one `DbContext` (`AppDbContext` in `App.Infrastructure`); `{Module}.Application`/`Domain` never reference it.
- Give every production project one dedicated test project mirroring its Allowed Dependencies; `{Module}.Api` has none.
- Declare every NuGet version once in `Directory.Packages.props`; keep every `<PackageReference>` versionless.
- Never let `Shared` take a project reference; never let a module project reference `App.Infrastructure`; never let a cross-module reference target anything but `{Module}.Interfaces`.
MAY:
- A pattern solution may add a project to a module (e.g. `{Module}.Domain.Rules`) when it needs isolation the base projects cannot give.

__Applied solutions:__
- [[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../solutions/solution-sln-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[../../../solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]] - [[../../../solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj.create]]

# Check list
- [ ] `Directory.Packages.props` at the root; `ManagePackageVersionsCentrally` true; no `Version=` on any `<PackageReference>`.
- [ ] `{Module}.Domain` present for a domain-bearing module; `App.Infrastructure` + `App.Queries` present; exactly one `DbContext`.
- [ ] `App.Infrastructure` referenced only by `App.Host`; `Shared` has zero project references.
- [ ] One test project per production project except `{Module}.Api`; `{Module}.Domain.Tests` present with `{Module}.Domain`.
- [ ] `make unit-test` is green.
