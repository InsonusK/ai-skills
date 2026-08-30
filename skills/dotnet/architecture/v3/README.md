# v3: solutions/plateaus with an explicit Variability Map

Same layout as [[skills/dotnet/architecture/draft/README.md|draft]] — a flat `solutions/` catalog plus a `plateau/{name}/` per plateau, linked via `created_by`/`parent_plateaus` — plus one addition: [[skills/dotnet/architecture/v3/variability-map.md|variability-map.md]], built per [[skills/common-workflow/architecture/design/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]], and a `registry/` folder inside any plateau where two or more solutions were found to intersect on the same element, classified per [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]].

```
v3/
  variability-map.md   — every Variation Point across the catalog, its Variants, Constraints, Realized-by solutions, and the Plateau Map derivation
  solutions/            — flat, one solution-*.skill/ per folder, unchanged from draft
  plateau/
    {plateau-name}/
      plateau-{plateau-name}.skill.md
      structure/
      adr/
      registry/          — present only where an intersection was actually found and classified; see variability-map.md's own note on where it does and doesn't appear
```

This supersedes `skills/dotnet/architecture/draft/` for the reason recorded in that draft's own `plateau-shared-rules/adr/rebind-to-statefull-service-parent.md`: it explicitly deferred a variability redesign "once real prior art (OVM, delta-oriented programming, GenVoca/AHEAD) has been studied" — that study is `variability-map.md` plus the two meta-skills above. `draft/` is kept unchanged, as historical reference; this catalog's internal cross-references were rewritten to point at `v3/` throughout, so it stands on its own rather than silently pointing back into `draft/`.

Two corrective fixes were made to the copied catalog while building the Variability Map, each backed by an ADR:
- `solution-http-api-publication` was missing the `depends_on` its own description already stated in prose ("requires at least one of query-integration or command-integration") — see [[skills/dotnet/architecture/v3/plateau/plateau-service-with-api/adr/require-at-least-one-mediatr-source.md|the ADR]].
- `solution-cecil-architecture-tests`' `built_on_plateau` was shallower than its own `solution-domain-rules` dependency's floor — see [[skills/dotnet/architecture/v3/plateau/plateau-shared-rules/adr/fix-cecil-built-on-plateau-floor.md|the ADR]].

One real, currently-unresolved conflict was found applying Conflict Detection for the first time: `solution-external-created-entity` and `solution-entity-edit-timestamp` both require their own property to be the first on a Create command — see [[skills/dotnet/architecture/v3/plateau/plateau-statefull-service/registry/command-cs.md|the registry entry]] for the finding and the recommended (not yet built) resolver.

## Plateau hierarchy

| Plateau | `parent_plateaus` | `standalone` | `created_by` |
| --- | --- | --- | --- |
| `plateau-stateless-non-interactive-service` | — (root) | `false` | `solution-sln-structure`, `solution-pipeline-registration`, `solution-mediator-exception-handler`, `solution-dotnet-conformance-testing` |
| `plateau-service-with-validated-module-interaction` | `plateau-stateless-non-interactive-service` | `false` | `solution-value-objects`, `solution-validation-behavior`, `solution-domain-behaviour`, `solution-dto-property-validators`, `solution-command-integration` |
| `plateau-statefull-service` | `plateau-service-with-validated-module-interaction` | `false` | `solution-infrastructure-project`, `solution-domain-configuration`, `solution-repository-integration`, `solution-unit-of-work`, `solution-entity-concurrency-change`, `solution-external-created-entity`, `solution-entity-classification`, `solution-query-integration`, `solution-entity-edit-timestamp` |
| `plateau-shared-rules` | `plateau-statefull-service` | `false` | `solution-domain-rules`, `solution-cecil-architecture-tests` |
| `plateau-service-with-api` | `plateau-service-with-validated-module-interaction` | `true` | `solution-http-api-publication`, `solution-grpc-integration` |
| `plateau-v1` | `plateau-service-with-api`, `plateau-shared-rules`, `plateau-statefull-service` | `true` | — (pure composition; `plateau-statefull-service` is also reached transitively through `plateau-shared-rules` — see `plateau-v1`'s own root skill for why it's still listed explicitly) |

`plateau-v1` is the current terminal, deployable baseline. See [[skills/dotnet/architecture/v3/variability-map.md#Plateau Map derivation|variability-map.md's Plateau Map derivation]] for which Variation-Point answers each plateau actually fixes, and for one legal combination this table has no named plateau for yet.
