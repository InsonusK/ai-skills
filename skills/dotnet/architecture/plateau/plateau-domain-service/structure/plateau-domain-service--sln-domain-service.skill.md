---
name: plateau-domain-service--sln-domain-service
description: Repository/solution-level layout of the plateau-domain-service plateau — plateau-core's layout plus {Module}.Domain, {Module}.Api, App.Infrastructure, App.Queries and their test projects
whenToUse: when adding, removing, or relocating a top-level project in a plateau-domain-service repository, deciding which existing project a new class belongs in, or reviewing the solution-level layout and the Central Package Management setup
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/sln
  - plateau/domain-service
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-central-package-management.skill/solution-central-package-management.skill|solution-central-package-management]]"
  - "[[skills/dotnet/architecture/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]]"
  - "[[skills/dotnet/architecture/solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]]"
  - "[[skills/dotnet/architecture/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill|solution-infrastructure-project]]"
  - "[[skills/dotnet/architecture/solutions/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]"
  - "[[skills/dotnet/architecture/solutions/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]]"
  - "[[skills/dotnet/architecture/solutions/solution-api-project.skill/solution-api-project.skill|solution-api-project]]"
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
    /[{ModuleName}.Interfaces](./{Module}.Interfaces/plateau-domain-service--csproj-module-interfaces.skill.md)
    /[{ModuleName}.Application](./{Module}.Application/plateau-domain-service--csproj-module-application.skill.md)
    /[{ModuleName}.Domain](./{Module}.Domain/plateau-domain-service--csproj-module-domain.skill.md)          — VP1
    /[{ModuleName}.Api](./{Module}.Api/plateau-domain-service--csproj-module-api.skill.md)                   — VP8/VP9
  /App
    /[App.Host](./App.Host/plateau-domain-service--csproj-app-host.skill.md)
    /[App.Infrastructure](./App.Infrastructure/plateau-domain-service--csproj-app-infrastructure.skill.md)   — VP2 / VP5 / VP11
    /[App.Queries](./App.Queries/plateau-domain-service--csproj-app-queries.skill.md)                        — VP2 (cross-module reads)
  /[Shared](./Shared/plateau-domain-service--csproj-shared.skill.md)
  /[BuildingBlocks](./BuildingBlocks/plateau-domain-service--csproj-building-blocks.skill.md)
/tests
  /[Shared.Tests](./Shared.Tests/plateau-domain-service--csproj-shared-tests.skill.md)
  /[BuildingBlocks.Tests](./BuildingBlocks.Tests/plateau-domain-service--csproj-building-blocks-tests.skill.md)
  /[{ModuleName}.Interfaces.Tests](./{Module}.Interfaces.Tests/plateau-domain-service--csproj-module-interfaces-tests.skill.md)
  /[{ModuleName}.Application.Tests](./{Module}.Application.Tests/plateau-domain-service--csproj-module-application-tests.skill.md)
  /[{ModuleName}.Domain.Tests](./{Module}.Domain.Tests/plateau-domain-service--csproj-module-domain-tests.skill.md)     — with VP1
/scripts   unit-test.sh   mutation-test.sh   test-report.sh
/report-template   index.html
```

`{ModuleName}.Api` has no dedicated test project (thin adapter, no logic). `{ModuleName}.Domain.Tests` exists only once `{ModuleName}.Domain` does.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/Implementation/Repository.create|Repository]]
- [[skills/dotnet/architecture/solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill|solution-domain-behaviour]] - [[skills/dotnet/architecture/solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]]
- [[skills/dotnet/architecture/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill|solution-infrastructure-project]] - [[skills/dotnet/architecture/solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/solution-api-project.skill/solution-api-project.skill|solution-api-project]] - [[skills/dotnet/architecture/solutions/solution-api-project.skill/Implementation/{Module}.Api.csproj.create|{Module}.Api.csproj]]
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/Implementation/Repository.extend|Repository]]

## Directory and class skills
| `Directory\|file` | template link | Description |
| --- | --- | --- |
| /src/Modules/{ModuleName}.Interfaces | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/{Module}.Interfaces/plateau-domain-service--csproj-module-interfaces.skill\|csproj-module-interfaces]] | Public contracts (+ concurrency / timestamp interfaces on commands) |
| /src/Modules/{ModuleName}.Application | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/{Module}.Application/plateau-domain-service--csproj-module-application.skill\|csproj-module-application]] | Orchestration — handlers (load/stage), validators, specs, version resolvers |
| /src/Modules/{ModuleName}.Domain | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/{Module}.Domain/plateau-domain-service--csproj-module-domain.skill\|csproj-module-domain]] | Entities, strict Value Objects, domain services, EF configs |
| /src/Modules/{ModuleName}.Api | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/{Module}.Api/plateau-domain-service--csproj-module-api.skill\|csproj-module-api]] | Thin inbound-API adapters (HTTP / gRPC) |
| /src/App/App.Host | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/App.Host/plateau-domain-service--csproj-app-host.skill\|csproj-app-host]] | Composition root — logging, modules, pipeline, infrastructure, API |
| /src/App/App.Infrastructure | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/App.Infrastructure/plateau-domain-service--csproj-app-infrastructure.skill\|csproj-app-infrastructure]] | AppDbContext, Repository, UnitOfWork, version-resolver factory, gRPC clients |
| /src/App/App.Queries | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/App.Queries/plateau-domain-service--csproj-app-queries.skill\|csproj-app-queries]] | Cross-module JOIN projection specs |
| /src/Shared | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/Shared/plateau-domain-service--csproj-shared.skill\|csproj-shared]] | Cross-cutting contracts — markers, LogEvents, concurrency / timestamp / repository / unit-of-work / client interfaces |
| /src/BuildingBlocks | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/BuildingBlocks/plateau-domain-service--csproj-building-blocks.skill\|csproj-building-blocks]] | Pipeline behaviors (validation, exception, concurrency, unit-of-work) |
| /tests/Shared.Tests | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/Shared.Tests/plateau-domain-service--csproj-shared-tests.skill\|csproj-shared-tests]] | Tests `Shared` only |
| /tests/BuildingBlocks.Tests | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/BuildingBlocks.Tests/plateau-domain-service--csproj-building-blocks-tests.skill\|csproj-building-blocks-tests]] | Tests `BuildingBlocks` |
| /tests/{ModuleName}.Interfaces.Tests | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/{Module}.Interfaces.Tests/plateau-domain-service--csproj-module-interfaces-tests.skill\|csproj-module-interfaces-tests]] | Tests `{ModuleName}.Interfaces` |
| /tests/{ModuleName}.Application.Tests | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/{Module}.Application.Tests/plateau-domain-service--csproj-module-application-tests.skill\|csproj-module-application-tests]] | Tests `{ModuleName}.Application` (+ `{ModuleName}.Domain`) |
| /tests/{ModuleName}.Domain.Tests | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/{Module}.Domain.Tests/plateau-domain-service--csproj-module-domain-tests.skill\|csproj-module-domain-tests]] | Tests `{ModuleName}.Domain` only |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/Implementation/Repository.create|Repository]]
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/Implementation/Repository.extend|Repository]]

## NuGet Packages
Every version is declared once in `Directory.Packages.props`; every `<PackageReference>` is versionless. On top of plateau-core's set (MediatR, FluentValidation, Ardalis.Result, Hosting, Logging, test packages):

| Package | Group | Purpose |
| --- | --- | --- |
| Microsoft.EntityFrameworkCore (+ provider) | Persistence | `AppDbContext`, entity configs, migrations |
| Ardalis.Specification, Ardalis.Specification.EntityFrameworkCore | Persistence | `Specification<T>`, `RepositoryBase<T>` |
| Grpc.Net.ClientFactory, Google.Protobuf, Grpc.Tools | gRPC | outbound client stubs |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/solution-central-package-management.skill/solution-central-package-management.skill|solution-central-package-management]] - [[skills/dotnet/architecture/solutions/solution-central-package-management.skill/Implementation/Directory.Packages.props.create|Directory.Packages.props]]

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
- [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/Implementation/Repository.create|Repository]]
- [[skills/dotnet/architecture/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill|solution-infrastructure-project]] - [[skills/dotnet/architecture/solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]]

# Check list
- [ ] `Directory.Packages.props` at the root; `ManagePackageVersionsCentrally` true; no `Version=` on any `<PackageReference>`.
- [ ] `{Module}.Domain` present for a domain-bearing module; `App.Infrastructure` + `App.Queries` present; exactly one `DbContext`.
- [ ] `App.Infrastructure` referenced only by `App.Host`; `Shared` has zero project references.
- [ ] One test project per production project except `{Module}.Api`; `{Module}.Domain.Tests` present with `{Module}.Domain`.
- [ ] `make unit-test` is green.
