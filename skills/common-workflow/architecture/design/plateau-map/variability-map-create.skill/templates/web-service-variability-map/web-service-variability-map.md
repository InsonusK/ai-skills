# Web-service common Variation Points (stack-agnostic reference)

A candidate VP list, not a ready-made Variability Map. It derives VP/Variants/Constraint/Realization-depends-on directly from [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/templates/web-service-common-features/web-service-common-features|web-service-common-features]]'s Feature Model — the same relations, re-expressed the way [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]] tables them. `Realized by` is left blank everywhere: it is always per-stack, filled by that catalog's own [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]] pass, never copied from here. When building a real catalog's own `{catalog}/variability-map.md`, copy the rows that apply, drop the ones that don't fit that family's baseline, and confirm every Constraint against that catalog's own solutions per `variability-map-create`'s "Constraints from evidence only" rule — nothing here is binding until that check has run.

| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| VP1 | HasPersistentStorage | Yes / No | — | (per-stack) | — | No |
| VP2 | PersistentStoreKind | PostgreSQL / SQLite | Applicable only when VP1=Yes | (per-stack) | — | No |
| VP3 | HasCacheStorage | Yes / No | — | (per-stack) | — | No |
| VP4 | CacheStoreKind | Redis / InMemory | Applicable only when VP3=Yes | (per-stack) | — | No |
| VP5 | HasHttpInboundApi | Yes / No | — | (per-stack) | — | No |
| VP6 | HasGrpcInboundApi | Yes / No | (independent of VP5) | (per-stack) | — | No |
| VP7 | HasKafkaConsumer | Yes / No | — | (per-stack) | — | No |
| VP8 | HasRabbitMqConsumer | Yes / No | (independent of VP7) | (per-stack) | — | No |
| VP9 | HasTaskBox | Yes / No | Yes requires (VP1=Yes OR VP3=Yes) | (per-stack) | — | No |
| VP10 | HasHttpOutboundClient | Yes / No | — | (per-stack) | — | No |
| VP11 | HasGrpcOutboundClient | Yes / No | (independent of VP10) | (per-stack) | — | No |
| VP12 | HasKafkaProducer | Yes / No | (independent of VP10, VP11) | (per-stack) | — | No |
| VP13 | HasRabbitMqProducer | Yes / No | (independent of VP10–VP12) | (per-stack) | — | No |
| VP14 | HasOutbox | Yes / No | Yes requires VP9=Yes AND (VP10=Yes OR VP11=Yes OR VP12=Yes OR VP13=Yes) | (per-stack) | See [Outbox's realization mirrors its storage VP](#outboxs-realization-mirrors-its-storage-vp) | No |
| VP15 | HasCriticalGuaranteeMessages | Yes / No | Applicable only when VP14=Yes; Yes requires VP1=Yes | (per-stack) | See [Outbox's realization mirrors its storage VP](#outboxs-realization-mirrors-its-storage-vp) | No |
| VP16 | HasNonCriticalGuaranteeMessages | Yes / No | Applicable only when VP14=Yes; Yes requires (VP1=Yes OR VP3=Yes); (independent of VP15 — a service can route some message types through each guarantee level at once) | (per-stack) | See [Outbox's realization mirrors its storage VP](#outboxs-realization-mirrors-its-storage-vp) | No |
| VP17 | HasMetric | Yes / No | — | (per-stack) | — | No |

## Grouping features and selectors that are not VPs
`Storage`, `Inbound`, `Outbound`, `InboundSync`, `InboundAsync`, `OutboundSync`, `MessageConsumer` — every umbrella node in the Feature Model — get no VP row of their own, per [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/feature-map-create.skill.md|feature-map-create]]'s "Selectors get prose, not rows" rule: none of them is ever chosen independently of its children, each is fully determined as the OR of the VPs beneath it (e.g. `Inbound` ≡ `VP5=Yes OR VP6=Yes OR VP7=Yes OR VP8=Yes OR VP9=Yes`). Recording a VP row for the umbrella itself would double-count a decision the leaf VPs already make.

## Outbox's realization mirrors its storage VP
This is the finding that motivated this reference file: a durable outbox and a best-effort (cache-only) outbox are not two `Realized by` Variants of one VP realized freely in combination — the guarantee only holds when the outbox row is written in the same transaction as the guarded business write, so **which store backs the outbox is dictated by which storage VP is actually selected, not chosen independently**. `HasCriticalGuaranteeMessages` (VP15) and `HasNonCriticalGuaranteeMessages` (VP16) are independent booleans, not two Variants of one categorical VP — a single service routinely needs both at once (e.g. guaranteed order-confirmation events alongside best-effort analytics events), each routed through its own outbox instance. Concretely: VP15=Yes's realization always binds to whichever concrete store VP2 (`PersistentStoreKind`) selected; VP16=Yes's realization binds to whichever of VP2/VP4 is present, and each guarantee level present gets its own realized instance rather than sharing one. This is exactly what the **Realization depends on** column is for (a cross-feature interaction that changes code *shape*, not a legality gate — see [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/orthogonal-variability-model|orthogonal-variability-model]]) — never encode it as a free `Realized by` combination, and never let a single outbox solution silently assume one storage kind without stating the dependency here.

## Dispatch protocol is FDN, not a VP explosion
VP10–VP13 (the outbound protocol choices) are the same leaves whether reached directly (`HasHttpOutboundClient=Yes` used synchronously) or drained from `Outbox` — `Outbox` itself carries no separate protocol VP, it only requires that at least one of VP10–VP13 is present (see the Constraint on VP14). Once a real catalog reaches `delta-conflict-detection`, each protocol adapter classifies `FDN` (DI substitution, composition-root wiring) — per [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md#fdn-never-forks-the-plateau-tree|FDN never forks the plateau tree]], adding a fifth protocol later is one new boolean VP and one new adapter solution, never a new plateau.

## Out of scope
- No `Realized by` is filled — that is always per-stack, per-catalog, produced by that catalog's own `delta-conflict-detection` pass.
- No plateau↔VP view — that is `plateau-map-create`'s output, never this file's.
- Every Constraint here is carried over from [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/templates/web-service-common-features/web-service-common-features|web-service-common-features]]'s Feature Model as a starting hypothesis — a real catalog must still verify each one against its own solutions' actual `depends_on`/prose per `variability-map-create`'s "Constraints from evidence only" rule, not accept it on this file's say-so.
- Does not cover a CLI/batch/worker-only Program Family, for the same reason its companion Feature Model doesn't.
