# Example: creating the .NET `default` plateau

This example shows how the real `skills/dotnet/architecture/artifacts/plateau/default` plateau is produced from its contributing solution skills.

## Input

- plateau-name: `default`
- target-stack: `dotnet`
- output: `skills/dotnet/architecture/artifacts/plateau/default`
- solutions:
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill|solution-validation-behavior]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill|solution-pipeline-registration]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-pipeline-registration-order.skill/solution-pipeline-registration-order.skill|solution-pipeline-registration-order]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]]

## Mapping from solution Implementation files to plateau skills

| Solution | Implementation file | Plateau skill |
| -------- | ------------------- | ------------- |
| solution-sln-structure | `Repository.create.md` | `sln-default.skill.md` |
| solution-sln-structure | `Shared.csproj.create.md` | `structure/Shared/csproj-shared.skill.md` |
| solution-sln-structure | `BuildingBlocks.csproj.create.md` | `structure/BuildingBlocks/csproj-building-blocks.skill.md` |
| solution-sln-structure | `App.Host.csproj.create.md` | `structure/App.Host/csproj-app-host.skill.md` |
| solution-sln-structure | `App.Infrastructure.csproj.create.md` | `structure/App.Infrastructure/csproj-app-infrastructure.skill.md` |
| solution-sln-structure | `App.Infrastructure.Migrations.csproj.create.md` | `structure/App.Infrastructure.Migrations/csproj-app-infrastructure-migrations.skill.md` |
| solution-sln-structure | `App.Queries.csproj.create.md` | `structure/App.Queries/csproj-app-queries.skill.md` |
| solution-sln-structure | `{Module}.Api.csproj.create.md` | `structure/{Module}.Api/csproj-module-api.skill.md` |
| solution-sln-structure | `{Module}.Application.csproj.create.md` | `structure/{Module}.Application/csproj-module-application.skill.md` |
| solution-sln-structure | `{Module}.Domain.csproj.create.md` | `structure/{Module}.Domain/csproj-module-domain.skill.md` |
| solution-sln-structure | `{Module}.Domain.csproj.create/{Entity}.cs.create.md` | `structure/{Module}.Domain/classes/class-entity.skill.md` |
| solution-sln-structure | `{Module}.Interfaces.csproj.create.md` | `structure/{Module}.Interfaces/csproj-module-interfaces.skill.md` |
| solution-command-integration | `Shared.csproj.extend.md` | `structure/Shared/csproj-shared.skill.md` |
| solution-command-integration | `Shared.csproj.extend/ICommand.cs.create.md` | `structure/Shared/classes/class-i-command.skill.md` |
| solution-command-integration | `{Module}.Interfaces.csproj.extend.md` | `structure/{Module}.Interfaces/csproj-module-interfaces.skill.md` |
| solution-command-integration | `{Module}.Interfaces.csproj.extend/{Command}.cs.create.md` | `structure/{Module}.Interfaces/classes/class-command.skill.md` |
| solution-command-integration | `{Module}.Application.csproj.extend.md` | `structure/{Module}.Application/csproj-module-application.skill.md` |
| solution-command-integration | `{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md` | `structure/{Module}.Application/classes/class-feature-handler.skill.md` |
| solution-command-integration | `{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md` | `structure/{Module}.Application/classes/class-feature-validator.skill.md` |
| solution-command-integration | `{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md` | `structure/{Module}.Application/classes/class-module-application-registration.skill.md` |
| solution-command-integration | `App.Host.csproj.extend.md` | `structure/App.Host/csproj-app-host.skill.md` |

## Resulting plateau structure

```
plateau/default/
├── plateau-default.skill.md
└── structure/
    ├── sln-default.skill.md
    ├── App.Host/
    │   ├── csproj-app-host.skill.md
    │   └── classes/
    │       ├── class-api-registration.skill.md
    │       ├── class-entity-version-resolver-registration.skill.md
    │       ├── class-module-registration.skill.md
    │       ├── class-pipeline-registration.skill.md
    │       ├── class-repository-registration.skill.md
    │       └── ...
    ├── App.Infrastructure/
    │   ├── csproj-app-infrastructure.skill.md
    │   └── classes/
    │       ├── class-appdbcontext.skill.md
    │       ├── class-entity-version-resolver-factory.skill.md
    │       ├── class-module-to-module-config.skill.md
    │       ├── class-repository.skill.md
    │       ├── class-unit-of-work.skill.md
    │       └── ...
    ├── App.Infrastructure.Migrations/
    │   └── csproj-app-infrastructure-migrations.skill.md
    ├── App.Queries/
    │   ├── csproj-app-queries.skill.md
    │   └── classes/
    │       ├── class-app-queries-registration.skill.md
    │       ├── class-cross-module-query-handler.skill.md
    │       └── ...
    ├── BuildingBlocks/
    │   ├── csproj-building-blocks.skill.md
    │   └── classes/
    │       ├── class-concurrency-behavior.skill.md
    │       ├── class-etag-encoder.skill.md
    │       ├── class-exception-handling-behavior.skill.md
    │       ├── class-guid-resolving-behavior.skill.md
    │       ├── class-unit-of-work-behavior.skill.md
    │       ├── class-unit-of-work-context.skill.md
    │       ├── class-validation-behavior.skill.md
    │       └── ...
    ├── Shared/
    │   ├── csproj-shared.skill.md
    │   └── classes/
    │       ├── class-conflict-result.skill.md
    │       ├── class-i-command-with-timestamp.skill.md
    │       ├── class-i-command.skill.md
    │       ├── class-i-creation-info-model-read-only.skill.md
    │       ├── class-i-creation-info-model.skill.md
    │       ├── class-i-entity-version-resolver-factory.skill.md
    │       ├── class-i-entity-version-resolver.skill.md
    │       ├── class-i-guid-resolver.skill.md
    │       ├── class-i-has-guid.skill.md
    │       ├── class-i-has-versions.skill.md
    │       ├── class-i-query.skill.md
    │       ├── class-i-read-repository.skill.md
    │       ├── class-i-repository.skill.md
    │       ├── class-i-unit-of-work.skill.md
    │       ├── class-i-update-info-model-read-only.skill.md
    │       ├── class-i-update-info-model.skill.md
    │       ├── class-i-versioned.skill.md
    │       └── ...
    ├── {Module}.Api/
    │   ├── csproj-module-api.skill.md
    │   └── classes/
    │       ├── class-conflict-result-extensions.skill.md
    │       ├── class-entity-controller.skill.md
    │       ├── class-entity-related-controller.skill.md
    │       ├── class-result-extensions.skill.md
    │       ├── class-single-entity-controller.skill.md
    │       ├── class-single-entity-property-controller.skill.md
    │       ├── class-single-entity-related-controller.skill.md
    │       ├── class-system-endpoints.skill.md
    │       └── ...
    ├── {Module}.Application/
    │   ├── csproj-module-application.skill.md
    │   └── classes/
    │       ├── class-create-entity-guid-resolver.skill.md
    │       ├── class-dto-validator.skill.md
    │       ├── class-entity-by-guid-spec.skill.md
    │       ├── class-entity-by-id-spec.skill.md
    │       ├── class-entity-summary-spec.skill.md
    │       ├── class-entity-version-resolver.skill.md
    │       ├── class-feature-handler.skill.md
    │       ├── class-feature-validator.skill.md
    │       ├── class-module-application-registration.skill.md
    │       ├── class-property-validator.skill.md
    │       └── ...
    ├── {Module}.Domain/
    │   ├── csproj-module-domain.skill.md
    │   └── classes/
    │       ├── class-behavior-service.skill.md
    │       ├── class-entity-config.skill.md
    │       ├── class-entity.skill.md
    │       ├── class-rule.skill.md
    │       ├── class-value-object.skill.md
    │       └── ...
    └── {Module}.Interfaces/
        ├── csproj-module-interfaces.skill.md
        └── classes/
            ├── class-command.skill.md
            ├── class-dto.skill.md
            ├── class-query.skill.md
            └── class-soft-value-object.skill.md
```

## Key observations

- `Repository.create.md` from `solution-sln-structure` becomes the foundation of `sln-default.skill.md`.
- Each concrete project (`Shared`, `BuildingBlocks`, `App.Host`, etc.) maps to one `csproj-{name}.skill.md`.
- Each generic module project (`{Module}.Api`, `{Module}.Application`, `{Module}.Domain`, `{Module}.Interfaces`) maps to one generic module skill (`csproj-module-*.skill.md`).
- Class files map to `class-{normalized-name}.skill.md` inside the corresponding project folder.
- Multiple `.create.md` and `.extend.md` files for the same project or class are merged into a single plateau skill.
- Every plateau skill lists all contributing solutions in `created_by` and `__Applied solutions__`.
