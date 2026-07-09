# Example: building a .NET plateau from three solutions

## Input

- plateau-name: `default`
- target-stack: `dotnet`
- solutions:
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]]

## Source files discovered in Implementation/

`solution-sln-structure`:
- `Repository.create.md`
- `Shared.csproj.create.md`
- `App.Host.csproj.create.md`
- `{Module}.Api.csproj.create.md`
- `{Module}.Domain.csproj.create/{Entity}.cs.create.md`

`solution-command-integration`:
- `Shared.csproj.extend.md`
- `Shared.csproj.extend/ICommand.cs.create.md`
- `{Module}.Application.csproj.extend.md`
- `{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md`

`solution-entity-concurrency-change`:
- `Shared.csproj.extend.md`
- `Shared.csproj.extend/IVersioned.cs.create.md`
- `BuildingBlocks.csproj.extend.md`
- `BuildingBlocks.csproj.extend/ConcurrencyBehavior.cs.create.md`

## Resulting plateau structure

```
plateau/default/
├── plateau-default.skill.md
└── structure/
    ├── sln-default.skill.md
    ├── Shared/
    │   ├── artifact-shared.skill.md
    │   └── files/
    │       ├── file-i-command.skill.md
    │       └── file-i-versioned.skill.md
    ├── BuildingBlocks/
    │   ├── artifact-building-blocks.skill.md
    │   └── files/
    │       └── file-concurrency-behavior.skill.md
    ├── App.Host/
    │   └── artifact-app-host.skill.md
    ├── {Module}.Api/
    │   └── artifact-module-api.skill.md
    ├── {Module}.Application/
    │   ├── artifact-module-application.skill.md
    │   └── files/
    │       └── file-feature-handler.skill.md
    └── {Module}.Domain/
        ├── artifact-module-domain.skill.md
        └── files/
            └── file-entity.skill.md
```

## Key observations
- `Shared.csproj.create.md` + `Shared.csproj.extend.md` (from two solutions) → one `artifact-shared.skill.md`.
- `ICommand.cs.create.md` → `file-i-command.skill.md`.
- `{Module}.Api.csproj.create.md` → `artifact-module-api.skill.md`.
- `{Entity}.cs.create.md` → `file-entity.skill.md`.
- `Repository.create.md` from `solution-sln-structure` becomes the foundation of `sln-default.skill.md`.
