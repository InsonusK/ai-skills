---
description: Defines the v3.1 solution folder structure — modules with a two-project base, app layers, Shared, BuildingBlocks
element_kind: repository
change_kind: create
tags:
  - solution/sln-structure
  - element/repository
---

# Structure

## Project Structure
```
/ (repository root)
  Directory.Packages.props        — created by solution-central-package-management
  Directory.Build.props
  {Solution}.sln
/src
  /Modules
    /{ModuleName}
      /{ModuleName}.Interfaces     — public contracts (commands, queries, DTOs, notifications, Soft VOs)
      /{ModuleName}.Application     — orchestration (handlers, validators)
  /App
    /App.Host                      — composition root
  /Shared                          — cross-cutting contracts (Result, Exceptions, base interfaces)
  /BuildingBlocks                  — technical patterns (MediatR behaviors)
```

`{ModuleName}.Domain` and `{ModuleName}.Api` are **not** part of the base layout:
- `{ModuleName}.Domain` is added by [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] when the module has a domain layer.
- `{ModuleName}.Api` is added by [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]] when the module exposes an inbound sync API.
- `App.Infrastructure`, `App.Infrastructure.Migrations`, `App.Queries` are added by the first persistence solution.

Test projects (one per production project) are defined by [[skills/dotnet/architecture/v3.1/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]].

## Directory and class skills
| `Directory\|file` | Description |
| ----------------- | ----------- |
| /src/Modules | All bounded-context modules |
| /{ModuleName} | One folder per module |
| /{ModuleName}.Interfaces | Public contracts — the module's only external surface |
| /{ModuleName}.Application | Orchestration — handlers, validators, specs |
| /src/App/App.Host | Composition root — DI, pipeline, module wiring |
| /src/Shared | Cross-cutting contracts |
| /src/BuildingBlocks | Reusable framework patterns |

# Allowed Dependencies

```
{Module}.Application  ->  {Module}.Interfaces, Shared, BuildingBlocks
{Module}.Interfaces   ->  Shared
App.Host              ->  every {Module}.Application, BuildingBlocks
BuildingBlocks        ->  Shared
Shared                ->  (nothing)
```

Cross-module: `{Module-A}.Application` may reference `{Module-B}.Interfaces` only.

# Rules

## MUST
- Put every module under `/src/Modules/{ModuleName}`.
  - Risk: modules outside the folder escape the layout tooling and reviews assume.
  - Fix: one folder per module under `/src/Modules`.
- Give a module exactly `Interfaces` + `Application` at creation; add other projects only via their owning pattern solution.
  - Risk: a pre-scaffolded empty `Domain`/`Api` hides whether a feature is actually present.
  - Fix: two projects now; the rest arrive with their feature.
- Keep the dependency arrows above — never reference another module's `Application`/`Domain`, never let `Shared` depend on anything.
  - Risk: an inward-pointing violation makes the whole layering unenforceable.
  - Fix: reference `{Module}.Interfaces` across modules; keep `Shared` reference-free.

## MAY
- A pattern solution may add a project (e.g. `{Module}.Domain.Rules`) when it needs isolation the base projects cannot give.

# Check list
- [ ] `/src/Modules/{ModuleName}/{ModuleName}.Interfaces` and `.Application` exist for every module.
- [ ] No `{ModuleName}.Domain` / `{ModuleName}.Api` unless its feature solution created it.
- [ ] `Shared` has zero project references.
- [ ] No cross-module reference targets anything but `{Module}.Interfaces`.
- [ ] `Directory.Packages.props` exists at the root (from `solution-central-package-management`).
