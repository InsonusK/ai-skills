# v3.1 plateaus

Three plateaus, built by `plateau-create-by-solutions` from the catalogue in `../solutions/`.
Flat lineage — each is `standalone` except the base, and each `parent_plateaus` entry is the single
previous plateau. A plateau's capabilities are **cumulative**: everything the parent has, plus its own delta.

## Plateau × VP matrix

Plateaus are coded per [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md#plateau-codes|plateau-map-create's plateau codes]] (`DW` = dotnet web-service; the folders still carry their pre-code names until they are renamed). Rows = plateaus by code, with the Title decoding it and the current folder name. Columns = the 9 common VPs (cell = the Variant the plateau realizes) and the 11 stack VPs (✅ = realized at that plateau, ❌ = not). Answers are **cumulative** down the chain — a plateau has every VP its parent has, plus its own. Scan a **column** for the shallowest plateau that includes a VP; read a **row** for a plateau's complete VP set.

| Code | Title | Folder | VP-C001 | VP-C002 | VP-C003 | VP-C004 | VP-C005 | VP-C006 | VP-C007 | VP-C008 | VP-C009 | VP1 | VP3 | VP4 | VP5 | VP6 | VP7 | VP8 | VP9 | VP12 | VP13 | VP14 |
|---|---|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| DW001.000 | core | plateau-core | None | None | No | No | No | No | No | No | No | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| DW008.003 | domain service | plateau-domain-service | PostgreSQL | None | No | No | Yes | No | No | No | No | ✅ | ✅ | ❌ | ✅ | ❌ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| DW008.004 | offline-sync service | plateau-offline-sync-service | PostgreSQL | None | No | No | Yes | No | No | No | No | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |

Column legend — VP-C001 PersistentStore (common) · VP-C002 TransientStore (common) · VP-C003 TaskBox (common) · VP-C004 HttpOutbound (common) · VP-C005 GrpcOutbound (common) · VP-C006–VP-C009 Kafka/RabbitMQ producer/consumer (common) · VP1 DomainLogic · VP3 ValueObjects · VP4 SharedRules ·
VP5 EntityConcurrencyControl · VP6 ExternalIdentity · VP7 AuditTimestamps · VP8 SyncInboundApi–HTTP ·
VP9 SyncInboundApi–gRPC · VP12 AsyncInboundApi ·
VP13 AsyncOutboundApi · VP14 OutboxPattern. Full descriptions, the solution that realizes each VP, and
the constraints between VPs are in [`../variability-map.md`](skills/dotnet/architecture/variability-map.md) and, for common VPs, the [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]] — the single source
of truth; this table is only the plateau-oriented view of the same answers. The `{common}` numbers come from the [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/registry/web-service-common-plateaus|common-plateau registry]].

- **VP5 / VP6 / VP7 are decided per persisted entity** — a ✅ means the plateau *enables* the capability
  (the realizing solution is composed and the example demonstrates it), not that every entity uses it.
- **VP9, VP-C004, VP12–VP14 are ❌ everywhere** — their solutions are skeletons (`> Draft contract` marker),
  ready to compose into a future plateau once a real consumer exists.
- **Planned Variants** — VP-C001 `SQLite`, VP-C002 `Redis` and `InMemory` are `planned` in the map; no plateau
  realizes them until a plateau "existing base + that Variant" is built.

## Stack VP combinations

| `{specific}` | Stack VPs |
|---|---|
| 000 | — |
| 001 | ⛔ retired — was 003's set plus the gRPC-client stack VP, which became common VP-C005 |
| 002 | ⛔ retired — was 004's set plus the gRPC-client stack VP, same reason |
| 003 | VP1, VP3, VP5, VP7, VP8 |
| 004 | VP1, VP3, VP4, VP5, VP6, VP7, VP8 |

## Lineage & new solutions

| # | Plateau | `standalone` | Parent | New solutions in its `created_by` (on top of the parent chain) |
|---|---------|--------------|--------|-----------------------------------|
| 1 | **DW001.000** core | `false` | — | `solution-central-package-management`, `solution-sln-structure`, `solution-mediator-integration`, `solution-validation-behavior`, `solution-mediator-exception-handler`, `solution-pipeline-registration`, `solution-soft-value-objects`, `solution-dto-property-validators`, `solution-app-logging`, `solution-dotnet-conformance-testing` |
| 2 | **DW008.003** domain service | `true` | DW001.000 | VP1 `solution-domain-behaviour` · VP-C001 `PostgreSQL` `solution-infrastructure-project` + `solution-domain-configuration` + `solution-repository-integration` + `solution-unit-of-work` + `solution-query-integration` · VP3 `solution-value-objects` · VP5 `solution-entity-concurrency-change` · VP7 `solution-entity-edit-timestamp` · VP8 `solution-api-project` + `solution-http-api-publication` · VP-C005 `solution-grpc-client` |
| 3 | **DW008.004** offline-sync service | `true` | DW008.003 | VP4 `solution-domain-shared-rules` + `solution-cecil-architecture-tests` · VP6 `solution-external-created-entity` · `solution-entity-classification` (the VP5×VP6 combination-resolver) |

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

Full classification is in [`../delta-conflict-analysis.md`](skills/dotnet/architecture/delta-conflict-analysis.md).

## Reference: v3 plateaus in v3.1 VP terms

The [v3 catalog](skills/dotnet/architecture/v3/README.md) realized a staged subset of this space. Its plateaus, expressed in v3.1 VP IDs (v3 numbered its own VPs differently: Persistence 0, EntityKind 1, Http/Grpc 2/3, SharedRules 4):

| v3 plateau | v3.1 VP answers |
| --- | --- |
| [[skills/dotnet/architecture/v3/plateau/plateau-stateless-non-interactive-service/plateau-stateless-non-interactive-service.skill/plateau-stateless-non-interactive-service.skill.md\|plateau-stateless-non-interactive-service]] | all VPs = No (common baseline only) — note: in v3.1, `MediatorModuleIntegration`, `ValidationPipeline` and `ExceptionHandlingPipeline` are common, so this plateau's line moves partly into shared core |
| [[skills/dotnet/architecture/v3/plateau/plateau-service-with-validated-module-interaction/plateau-service-with-validated-module-interaction.skill/plateau-service-with-validated-module-interaction.skill.md\|plateau-service-with-validated-module-interaction]] | VP1=Yes, VP3=Yes; VP-C001 = None; VP4–VP9/VP12–VP14 = No; VP-C004/VP-C005 = No |
| [[skills/dotnet/architecture/v3/plateau/plateau-statefull-service/plateau-statefull-service.skill/plateau-statefull-service.skill.md\|plateau-statefull-service]] | VP1=Yes, VP-C001 ≠ None, VP3=Yes; VP5/VP6/VP7 decided per entity (plateau enables, does not fix); VP4/VP8/VP9/VP12–VP14 = No; VP-C004/VP-C005 = No |
| [[skills/dotnet/architecture/v3/plateau/plateau-shared-rules/plateau-shared-rules.skill/plateau-shared-rules.skill.md\|plateau-shared-rules]] | as `plateau-statefull-service` + VP4=Yes |
| [[skills/dotnet/architecture/v3/plateau/plateau-service-with-api/plateau-service-with-api.skill/plateau-service-with-api.skill.md\|plateau-service-with-api]] | VP1=Yes, VP3=Yes, (VP8=Yes and/or VP9=Yes — at least one); VP-C001 = None; VP4–VP7/VP12–VP14 = No; VP-C004/VP-C005 = No |
| [[skills/dotnet/architecture/v3/plateau/plateau-v1/plateau-v1.skill/plateau-v1.skill.md\|plateau-v1]] | union: VP1=Yes, VP-C001 ≠ None, VP3=Yes, VP4=Yes, (VP8 and/or VP9); VP5–VP7 per entity; VP12–VP14 = No; VP-C004/VP-C005 = No |

Every reference row above is consistent with every stated Constraint: no row sets VP5–VP7 without VP-C001 ≠ None, no row sets VP-C001 ≠ None without VP1=Yes, no row sets VP3=Yes without VP1=Yes, and `plateau-service-with-api` correctly does not require VP-C001.

### Combinations the family now allows that v3 has no plateau for

The Feature Model adds axes v3 never modelled — **VP-C004, VP-C005, VP12–VP14 (outbound sync + async messaging)** are entirely new and aspirational, so any plateau touching them is future work once their solutions exist. Within the axes v3 already had, the [v3 map's open combination](skills/dotnet/architecture/v3/variability-map.md#Plateau Map derivation) still stands: `VP-C001 ≠ None + `(VP8 or VP9)` + `VP4=No` — a persisted module with an external API but no shared rules — has no dedicated plateau, because `plateau-v1` always brings `VP4=Yes`.
