# Example: building a .NET plateau from three solutions

## Input
- plateau-name: `default`
- stack: `dotnet`
- solutions:
  - [[skills/dotnet/architecture/draft/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]]
  - [[skills/dotnet/architecture/draft/solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]]

## Source files discovered in Implementation/

`solution-sln-structure`:
- `Repository.create.md`
- `Shared.csproj.create.md`
- `BuildingBlocks.csproj.create.md`
- `App.Host.csproj.create.md`
- `App.Infrastructure.csproj.create.md`
- `{Module}.Api.csproj.create.md`
- `{Module}.Application.csproj.create.md`
- `{Module}.Domain.csproj.create.md`
- `{Module}.Domain.csproj.create/{Entity}.cs.create.md`
- `{Module}.Interfaces.csproj.create.md`

`solution-command-integration`:
- `Shared.csproj.extend.md`
- `Shared.csproj.extend/ICommand.cs.create.md`
- `{Module}.Interfaces.csproj.extend.md`
- `{Module}.Interfaces.csproj.extend/{Command}.cs.create.md`
- `{Module}.Application.csproj.extend.md`
- `{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md`
- `{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md`
- `{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md`
- `App.Host.csproj.extend.md`

`solution-entity-concurrency-change`:
- `Shared.csproj.extend.md`
- `Shared.csproj.extend/IVersioned.cs.create.md`
- `Shared.csproj.extend/IHasVersions.cs.create.md`
- `BuildingBlocks.csproj.extend.md`
- `BuildingBlocks.csproj.extend/ConcurrencyBehavior.cs.create.md`
- `BuildingBlocks.csproj.extend/ETagEncoder.cs.create.md`
- `{Module}.Domain.csproj.extend.md`
- `{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md`
- `{Module}.Application.csproj.extend.md`
- `{Module}.Application.csproj.extend/{Entity}VersionResolver.cs.create.md`
- `{Module}.Interfaces.csproj.extend.md`
- `{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md`
- `{Module}.Api.csproj.extend.md`
- `{Module}.Api.csproj.extend/Single{Entity}Controller.cs.extend.md`
- `App.Host.csproj.extend.md`
- `App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md`
- `App.Infrastructure.csproj.extend.md`
- `App.Infrastructure.csproj.extend/EntityVersionResolverFactory.cs.create.md`

## Resulting plateau structure

```
plateau/default/
├── plateau-default.skill.md
└── structure/
    ├── sln-default.skill.md
    ├── Shared/
    │   ├── csproj-shared.skill.md
    │   └── classes/
    │       ├── class-i-command.skill.md
    │       ├── class-i-has-versions.skill.md
    │       ├── class-i-entity-version-resolver.skill.md
    │       └── class-i-versioned.skill.md
    ├── BuildingBlocks/
    │   ├── csproj-building-blocks.skill.md
    │   └── classes/
    │       ├── class-concurrency-behavior.skill.md
    │       └── class-etag-encoder.skill.md
    ├── App.Host/
    │   ├── csproj-app-host.skill.md
    │   └── classes/
    │       ├── class-entity-version-resolver-registration.skill.md
    │       └── class-module-registration.skill.md
    ├── App.Infrastructure/
    │   ├── csproj-app-infrastructure.skill.md
    │   └── classes/
    │       └── class-entity-version-resolver-factory.skill.md
    ├── {Module}.Api/
    │   ├── csproj-module-api.skill.md
    │   └── classes/
    │       └── class-single-entity-controller.skill.md
    ├── {Module}.Application/
    │   ├── csproj-module-application.skill.md
    │   └── classes/
    │       ├── class-entity-version-resolver.skill.md
    │       ├── class-feature-handler.skill.md
    │       ├── class-feature-validator.skill.md
    │       └── class-module-application-registration.skill.md
    ├── {Module}.Domain/
    │   ├── csproj-module-domain.skill.md
    │   └── classes/
    │       ├── class-entity.skill.md
    │       └── class-entity-config.skill.md
    └── {Module}.Interfaces/
        ├── csproj-module-interfaces.skill.md
        └── classes/
            ├── class-command.skill.md
            └── class-query.skill.md
```

## Resulting `plateau-default.skill.md` frontmatter

```yaml
---
name: default
description: Default .NET plateau with solution structure, command integration and entity concurrency control
domain: skill
type: template
version: 20250101120000
tags:
  - skill/template/plateau
parent_plateau:
created_by:
  - [[skills/dotnet/architecture/draft/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]
  - [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]
  - [[skills/dotnet/architecture/draft/solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]
---
```

## Key observations
- `Shared.csproj.create.md` + `Shared.csproj.extend.md` (from two solutions) → one `csproj-shared.skill.md`.
- `ICommand.cs.create.md` → `class-i-command.skill.md`; `IVersioned.cs.create.md` → `class-i-versioned.skill.md`.
- `{Module}.Api.csproj.create.md` + `{Module}.Api.csproj.extend.md` → one `csproj-module-api.skill.md`.
- `{Entity}.cs.create.md` + `{EntityName}.cs.extend.md` → one `class-entity.skill.md`.
- `Repository.create.md` from `solution-sln-structure` becomes the foundation of `sln-default.skill.md`.
