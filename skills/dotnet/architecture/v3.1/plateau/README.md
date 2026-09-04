# v3.1 plateaus

Three plateaus, built by `plateau-create-by-solutions` from the catalogue in `../solutions/`.
Flat lineage — each is `standalone` except the base, and each `parent_plateaus` entry is the single
previous plateau. A plateau's capabilities are **cumulative**: everything the parent has, plus its own delta.

## Plateau × VP matrix

Rows = plateaus, columns = the 14 Variation Points. ✅ = the VP is realized at that plateau, ❌ = it is
not. Answers are **cumulative** down the chain — a plateau has every VP its parent has, plus its own.
Scan a **column** for the shallowest plateau that includes a VP; read a **row** for a plateau's
complete VP set.

| Plateau | VP1 | VP2 | VP3 | VP4 | VP5 | VP6 | VP7 | VP8 | VP9 | VP10 | VP11 | VP12 | VP13 | VP14 |
|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| plateau-core                 | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| plateau-domain-service       | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |
| plateau-offline-sync-service | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |

Column legend — VP1 DomainLogic · VP2 Persistence · VP3 ValueObjects · VP4 CentralizedRules ·
VP5 EntityConcurrencyControl · VP6 ExternalIdentity · VP7 AuditTimestamps · VP8 SyncInboundApi–HTTP ·
VP9 SyncInboundApi–gRPC · VP10 SyncOutboundApi–HTTP · VP11 SyncOutboundApi–gRPC · VP12 AsyncInboundApi ·
VP13 AsyncOutboundApi · VP14 OutboxPattern. Full descriptions, the solution that realizes each VP, and
the constraints between VPs are in [`../variability-map.md`](../variability-map.md) — the single source
of truth; this table is only the plateau-oriented view of the same answers.

- **VP5 / VP6 / VP7 are decided per persisted entity** — a ✅ means the plateau *enables* the capability
  (the realizing solution is composed and the example demonstrates it), not that every entity uses it.
- **VP9, VP10, VP12–VP14 are ❌ everywhere** — their solutions are skeletons (`> Draft contract` marker),
  ready to compose into a future plateau once a real consumer exists.

## Lineage & new solutions

| # | Plateau | `standalone` | Parent | New solutions in its `created_by` (on top of the parent chain) |
|---|---------|--------------|--------|-----------------------------------|
| 1 | **plateau-core** | `false` | — | `solution-central-package-management`, `solution-sln-structure`, `solution-mediator-integration`, `solution-validation-behavior`, `solution-mediator-exception-handler`, `solution-pipeline-registration`, `solution-soft-value-objects`, `solution-dto-property-validators`, `solution-app-logging`, `solution-dotnet-conformance-testing` |
| 2 | **plateau-domain-service** | `true` | plateau-core | VP1 `solution-domain-behaviour` · VP2 `solution-infrastructure-project` + `solution-domain-configuration` + `solution-repository-integration` + `solution-unit-of-work` + `solution-query-integration` · VP3 `solution-value-objects` · VP5 `solution-entity-concurrency-change` · VP7 `solution-entity-edit-timestamp` · VP8 `solution-api-project` + `solution-http-api-publication` · VP11 `solution-grpc-client` |
| 3 | **plateau-offline-sync-service** | `true` | plateau-domain-service | VP4 `solution-domain-rules` + `solution-cecil-architecture-tests` · VP6 `solution-external-created-entity` · `solution-entity-classification` (the VP5×VP6 combination-resolver) |

The build scaffolding (anchor contract, mechanical check, decisions log) lives in
[`../agent/`](../agent/) — run `bash skills/dotnet/architecture/v3.1/agent/check.sh` after any change.

## What each plateau folder holds

```
plateau-{name}/
  plateau-{name}.skill/
    plateau-{name}.skill.md      the plateau summary an agent reads before writing code
    example/                     a runnable Sample service — `dotnet build` + `make unit-test` green
  structure/                     one skill per project + per class (prefix `plateau-{name}--`)
  registry/                      delta-conflict-detection ordering records (offline-sync-service only)
  adr/                           plateau-level decisions, if any
```

| Plateau | structure skills | example: `make unit-test` |
|---|---|---|
| plateau-core | 34 | 7 scenarios, 4 test projects |
| plateau-domain-service | 65 | 10 scenarios, 5 test projects |
| plateau-offline-sync-service | 76 | 13 scenarios, 6 test projects |

## `registry/` (plateau-offline-sync-service)

When two or more solutions modify the same code element and the interaction is only about **ordering**
(not a real semantic conflict), `delta-conflict-detection` records a per-element file in the `registry/`
folder of the shallowest plateau where all the intersecting solutions are present together.

- **`command-cs`** — VP5, VP6, VP7 each append a property to a command `record`. The fixed order
  (business fields → `Guid` → `ActionTimeStamp` → version token) is declared once in
  `solution-mediator-integration`; no VP claims "first". `source: ordering-only` — resolved by convention,
  no resolver solution.
- **`pipelineregistration-cs`** — five pipeline behaviours register into one ordered list
  (`ExceptionHandling → Validation → Concurrency → GuidResolving → UnitOfWork`). `GuidResolvingBehavior`'s
  position relative to `ConcurrencyBehavior` is `source: ordering-only` (a duplicate-Guid short-circuit
  must precede any commit; there is no Feature-Model constraint between VP5 and VP6).

Full classification is in [`../delta-conflict-analysis.md`](../delta-conflict-analysis.md).
