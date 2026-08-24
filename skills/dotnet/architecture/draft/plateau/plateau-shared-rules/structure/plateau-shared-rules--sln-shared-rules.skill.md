---
name: sln-shared-rules
description: Repository/solution-level layout of the shared-rules plateau
whenToUse: when adding, removing, or relocating a top-level project or module in this plateau's solution, or deciding where new code belongs at the repository level
domain: skill
type: template
plateau: shared-rules
version: 20260824150000
tags:
  - skill/template/sln
  - plateau/shared-rules
created_by:
  - "[[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
  - "[[../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]]"
  - "[[../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]"
  - "[[../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
  - "[[../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
  - "[[../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
  - "[[../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
  - "[[../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]]"
---

# Structure

## Repository Structure
```
/src
  /Modules
    /{ModuleName}
      /[{ModuleName}.Api](./{Module}.Api/plateau-shared-rules--csproj-module-api.skill.md)
      /[{ModuleName}.Application](./{Module}.Application/plateau-shared-rules--csproj-module-application.skill.md)
      /[{ModuleName}.Application.Tests](./{Module}.Application.Tests/plateau-shared-rules--csproj-module-application-tests.skill.md)
      /[{ModuleName}.Domain](./{Module}.Domain/plateau-shared-rules--csproj-module-domain.skill.md)
      /[{ModuleName}.Domain.Tests](./{Module}.Domain.Tests/plateau-shared-rules--csproj-module-domain-tests.skill.md)
      /[{ModuleName}.Domain.Rules](./{Module}.Domain.Rules/plateau-shared-rules--csproj-module-domain-rules.skill.md)
      /{ModuleName}.Domain.Rules.Spec   (not a project — .feature files only, see below)
      /[{ModuleName}.Domain.Rules.Tests](./{Module}.Domain.Rules.Tests/plateau-shared-rules--csproj-module-domain-rules-tests.skill.md)
      /[{ModuleName}.Interfaces](./{Module}.Interfaces/plateau-shared-rules--csproj-module-interfaces.skill.md)
      /[{ModuleName}.Interfaces.Tests](./{Module}.Interfaces.Tests/plateau-shared-rules--csproj-module-interfaces-tests.skill.md)
  /App
    /[App.Host](./App.Host/plateau-shared-rules--csproj-app-host.skill.md)
  /[Shared](./Shared/plateau-shared-rules--csproj-shared.skill.md)
  /[Shared.Tests](./Shared.Tests/plateau-shared-rules--csproj-shared-tests.skill.md)
  /[BuildingBlocks](./BuildingBlocks/plateau-shared-rules--csproj-building-blocks.skill.md)
  /[BuildingBlocks.Tests](./BuildingBlocks.Tests/plateau-shared-rules--csproj-building-blocks-tests.skill.md)
/report-template
  index.html
/scripts
  unit-test.sh
  mutation-test.sh
  test-report.sh
Makefile
README.md
```

Every production project has its own dedicated test project, mirroring its Allowed Dependencies exactly — never one combined test project per module. `{ModuleName}.Api` has no dedicated test project (thin MediatR adapter, no business logic of its own). `{ModuleName}.Domain.Rules.Spec` is not a project at all — a directory of `.feature` files, shared by `{ModuleName}.Domain.Rules.Tests`, `{ModuleName}.Domain.Tests`, and `{ModuleName}.Application.Tests` (see [[./{Module}.Domain.Rules.Spec/plateau-shared-rules--module-domain-rules-spec.skill.md|module-domain-rules-spec]]). See [[../../../solutions/solution-conformance-testing.skill/adr/test-project-per-production-project.md|solution-conformance-testing's ADR]] for the one-test-project-per-production-project baseline this plateau applies to its two new projects too.

`App.Infrastructure`, `App.Infrastructure.Migrations`, and `App.Queries` are not part of this plateau — this plateau describes a service with no persistence; a persistence-introducing plateau adds them. A Domain-classified rule's own `Load` step (via `{Feature}Check`) has nothing real to load from until that happens either — see [[../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md#boundaries|solution-domain-rules' Boundaries]].

__Applied solutions:__
- [[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../solutions/solution-sln-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../solutions/solution-conformance-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]], [[../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]], [[../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]], [[../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]], [[../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] — no repository-level change; see each project's own csproj skill for their contributions
- [[../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - adds `{ModuleName}.Domain.Rules`, `{ModuleName}.Domain.Rules.Spec`, `{ModuleName}.Domain.Rules.Tests`
- [[../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - adds `{ModuleName}.Domain.Tests/Architecture`, no new project

## Directory and class skills
| `Directory\|file` | template link | Description |
| --- | --- | --- |
| /{ModuleName}.Api | [[./{Module}.Api/plateau-shared-rules--csproj-module-api.skill.md\|csproj-module-api]] | HTTP endpoints, thin MediatR adapters |
| /{ModuleName}.Application | [[./{Module}.Application/plateau-shared-rules--csproj-module-application.skill.md\|csproj-module-application]] | Orchestration — per-feature handlers/validators, reusable DTO/VO validators, module DI registration |
| /{ModuleName}.Application.Tests | [[./{Module}.Application.Tests/plateau-shared-rules--csproj-module-application-tests.skill.md\|csproj-module-application-tests]] | Tests `{ModuleName}.Application` (and, transitively, `{ModuleName}.Domain`) |
| /{ModuleName}.Domain | [[./{Module}.Domain/plateau-shared-rules--csproj-module-domain.skill.md\|csproj-module-domain]] | Entities, Value Objects, invariants, domain services |
| /{ModuleName}.Domain.Tests | [[./{Module}.Domain.Tests/plateau-shared-rules--csproj-module-domain-tests.skill.md\|csproj-module-domain-tests]] | Tests `{ModuleName}.Domain`, plus Cecil architecture tests over `{ModuleName}.Domain`/`{ModuleName}.Domain.Rules` |
| /{ModuleName}.Domain.Rules | [[./{Module}.Domain.Rules/plateau-shared-rules--csproj-module-domain-rules.skill.md\|csproj-module-domain-rules]] | Centralized business predicates (`{Rule}.cs`), reusable outside this service |
| /{ModuleName}.Domain.Rules.Spec | [[./{Module}.Domain.Rules.Spec/plateau-shared-rules--module-domain-rules-spec.skill.md\|module-domain-rules-spec]] | `.feature` files per rule — not a project, shared by three test projects |
| /{ModuleName}.Domain.Rules.Tests | [[./{Module}.Domain.Rules.Tests/plateau-shared-rules--csproj-module-domain-rules-tests.skill.md\|csproj-module-domain-rules-tests]] | Tests `{ModuleName}.Domain.Rules` only, isolated mutation-testing surface |
| /{ModuleName}.Interfaces | [[./{Module}.Interfaces/plateau-shared-rules--csproj-module-interfaces.skill.md\|csproj-module-interfaces]] | Public contracts — commands, Soft Value Objects |
| /{ModuleName}.Interfaces.Tests | [[./{Module}.Interfaces.Tests/plateau-shared-rules--csproj-module-interfaces-tests.skill.md\|csproj-module-interfaces-tests]] | Tests `{ModuleName}.Interfaces` only |
| /App.Host | [[./App.Host/plateau-shared-rules--csproj-app-host.skill.md\|csproj-app-host]] | Composition root |
| /Shared | [[./Shared/plateau-shared-rules--csproj-shared.skill.md\|csproj-shared]] | Cross-cutting primitives, `ICommand`/`ICommand<T>` markers |
| /Shared.Tests | [[./Shared.Tests/plateau-shared-rules--csproj-shared-tests.skill.md\|csproj-shared-tests]] | Tests `Shared` only |
| /BuildingBlocks | [[./BuildingBlocks/plateau-shared-rules--csproj-building-blocks.skill.md\|csproj-building-blocks]] | Reusable framework patterns — exception handling, request validation pipeline |
| /BuildingBlocks.Tests | [[./BuildingBlocks.Tests/plateau-shared-rules--csproj-building-blocks-tests.skill.md\|csproj-building-blocks-tests]] | Tests `BuildingBlocks` (and, transitively, `Shared`) |

__Applied solutions:__
- [[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../solutions/solution-sln-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../solutions/solution-conformance-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]], [[../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]], [[../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]], [[../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]], [[../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] — no repository-level change; see each project's own csproj skill for their contributions
- [[../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]], [[../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] — see each project's own csproj skill for their contributions

# Rules
MUST:
- Every module lives under `/src/Modules/{ModuleName}`
- Every module has at least the base set of four projects: Api, Application, Domain, Interfaces
- Every production project that has one (Domain, Application, Interfaces, Shared, BuildingBlocks, Domain.Rules) has exactly one dedicated test project mirroring its Allowed Dependencies exactly — never one combined test project per module, never a project with wider references than its production counterpart
- Tests live alongside their production project — not in a global `/tests` folder
- `unit-test`, `mutation-test`, `test-report`, and `test-and-report` `make` targets exist at the repository root, run across every test project, and behave exactly as [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] defines
- Every `{ModuleName}.Domain.Rules.Spec/{Rule}.feature` scenario is linked into and proven by every test project whose adapter it describes (`{ModuleName}.Domain.Rules.Tests` always; `{ModuleName}.Domain.Tests`/`{ModuleName}.Application.Tests` per its `@format`/`@semantic`/`@domain` tag)
MAY:
- A specific pattern solution add an additional project to a module when it needs project-level isolation
MUST NOT:
- Module projects exist outside `/src/Modules`
- Module have fewer than the base four projects
- Module have an additional project that no specific pattern solution defines
- `{ModuleName}.Api` have a dedicated test project
- `{ModuleName}.Domain.Rules.Spec` be treated as a project — it has no `.csproj`, is never referenced, and holds no `.cs` file
- CI or a developer call `dotnet test`/`dotnet-stryker` directly instead of through the `Makefile`'s `make` targets

__Applied solutions:__
- [[../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../solutions/solution-sln-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../solutions/solution-conformance-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]], [[../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]], [[../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]], [[../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]], [[../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] — no repository-level change; see each project's own csproj skill for their contributions
- [[../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]], [[../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] — see each project's own csproj skill for their contributions
