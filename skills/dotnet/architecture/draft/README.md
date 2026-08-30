# Draft: dotnet solutions regrouped into plateaus

Layout follows [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] and [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]] exactly — a flat `solutions/` catalog plus a `plateau/{name}/` per plateau, linked via `created_by`/`parent_plateaus`, never nested inside each other.

```
draft/
  solutions/          — flat, one solution-*.skill/ per folder, unchanged shape
  plateau/
    {plateau-name}/
      plateau-{plateau-name}.skill.md
      structure/
      adr/
```

This supersedes `skills/dotnet/architecture/deprecated/v1/` (the old flat `solutions/🧩validated/` catalog plus the single `plateau/default`), which is kept only for historical reference and is no longer extended.

## Plateau hierarchy

| Plateau | `parent_plateaus` | `standalone` | `created_by` |
| --- | --- | --- | --- |
| `plateau-stateless-non-interactive-service` | — (root) | `false` | `solution-sln-structure`, `solution-pipeline-registration`, `solution-mediator-exception-handler`, `solution-dotnet-conformance-testing` |
| `plateau-service-with-validated-module-interaction` | `plateau-stateless-non-interactive-service` | `false` | `solution-value-objects`, `solution-validation-behavior`, `solution-domain-behaviour`, `solution-dto-property-validators`, `solution-command-integration` |
| `plateau-statefull-service` | `plateau-service-with-validated-module-interaction` | `false` | `solution-infrastructure-project`, `solution-domain-configuration`, `solution-repository-integration`, `solution-unit-of-work`, `solution-entity-concurrency-change`, `solution-external-created-entity`, `solution-entity-classification`, `solution-query-integration`, `solution-entity-edit-timestamp` |
| `plateau-shared-rules` | `plateau-statefull-service` | `false` | `solution-domain-rules`, `solution-cecil-architecture-tests` |
| `plateau-service-with-api` | `plateau-service-with-validated-module-interaction` | `true` | `solution-http-api-publication`, `solution-grpc-integration` |
| `plateau-v1` | `plateau-service-with-api`, `plateau-shared-rules`, `plateau-statefull-service` | `true` | — (pure composition; `plateau-statefull-service` is also reached transitively through `plateau-shared-rules` — see `plateau-v1`'s own root skill for why it's still listed explicitly) |

`plateau-v1` is the current terminal, deployable baseline — everything a module needs to actually ship (domain logic, persistence, optional rule-centralization, a real external surface) composes on it.
