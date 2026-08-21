# Draft: regrouping dotnet solutions into plateaus

Non-destructive staging area. Nothing under `skills/dotnet/architecture/solutions/🧩validated/` or `skills/dotnet/architecture/plateau/default/` is touched until this draft is reviewed and promoted.

Layout follows [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] and [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]] exactly — a flat `solutions/` catalog plus a `plateau/{name}/` per plateau, linked via `created_by`/`parent_plateaus`, never nested inside each other, so promotion to production is a plain folder move with no re-layout.

```
draft/
  solutions/          — flat, one solution-*.skill/ per folder, unchanged shape
  plateau/
    {plateau-name}/
      plateau-{plateau-name}.skill.md
      structure/
      adr/
```

## Proposed plateau composition

Reconstructed from the user's `Untitled.canvas` sketch, mapped onto the 19 solutions currently in `solutions/🧩validated/`. **Pending confirmation before any file is generated.**

| Plateau | `parent_plateaus` | `standalone` | Existing solutions in `created_by` | Not-yet-existing units the sketch names |
| --- | --- | --- | --- | --- |
| `plateau-stateless-non-interactive-service` | — (root) | ? | `solution-sln-structure` (= sketch's "solution-service-platform"), `solution-pipeline-registration`, `solution-mediator-exception-handler` | `solution-module`, `solution-module-test`, `solution-testing-approach` |
| `plateau-service-with-validated-module-interaction` | `plateau-stateless-non-interactive-service` | ? | `solution-validation-behavior`, `solution-dto-property-validators`, `solution-value-objects`, `solution-domain-behaviour` | `solution-domain-module` (descriptive text — may fold into an existing solution's Goal instead of becoming its own) |
| `plateau-statefull-service` | `plateau-service-with-validated-module-interaction` | ? | `solution-domain-configuration`, `solution-entity-classification`, `solution-external-created-entity`, `solution-repository-integration`, `solution-entity-concurrency-change`, `solution-unit-of-work`, `solution-query-integration`, `solution-entity-edit-timestamp` | — |
| `plateau-shared-rules` *(sketch called it "service-with-share-module-tests", no `plateau-` prefix — confirmed typo)* | `plateau-service-with-validated-module-interaction` | ? | `solution-domain-rules` | `solution-shared-rules-test` |
| `plateau-service-with-api` | `plateau-stateless-non-interactive-service` | ? | `solution-http-api-publication`, `solution-command-integration` | `solution-grpc-integrated-service` |
| `plateau-async-integrated-service` | `plateau-stateless-non-interactive-service` | ? | — | `solution-outbox-pattern` — **sketch note: "just an example," not confirmed in scope** |
| `plateau-v1` | `plateau-service-with-api`, `plateau-shared-rules`, `plateau-statefull-service`, `plateau-service-with-validated-module-interaction` | `true` | — (pure composition) | — |

## Open questions
1. Does the mapping above match intent, or does any existing solution belong in a different plateau?
2. Not-yet-existing units (`solution-module`, `solution-module-test`, `solution-testing-approach`, `solution-domain-module`, `solution-shared-rules-test`, `solution-grpc-integrated-service`, `solution-outbox-pattern`) — stub as placeholder solutions now, or migrate only the 19 existing solutions this pass and leave these as follow-up work?
3. Is `plateau-async-integrated-service` in scope for this pass, given it was flagged as illustrative only?
4. `standalone` value for each plateau above `plateau-v1` — all `false` (pure ingredients) or does any of them (e.g. `plateau-stateless-non-interactive-service`) count as independently usable?
