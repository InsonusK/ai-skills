---
tags:
  - concern/architecture
  - stack/go
---

# skills/go/architecture Variability Map

Built per [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill|variability-map-create]], from the non-common features of [[skills/go/architecture/feature/feature-model|feature/feature-model.md]]. This map is the input to [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]].

**Status of this catalog.** `solutions/` holds this catalog's own solution skills, authored fresh (no prior catalog to migrate from — see `agent/DECISIONS.md`). Every **Realized by** cell links into `solutions/`. Rows VP3–VP5 (`AsyncOutboundApi`, `OutboxPattern`, `AsyncInboundApi`) are **aspirational**: their solutions are skeletons with a draft-contract marker — no plateau in this catalog's first build realizes them yet. `plateau/` holds the five plateaus built from this map; the plateau↔VP view lives in `plateau/plateau-repository.md`, maintained per [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md|plateau-map-create]].

## Variation Points

Each row is one axis on which two Go web-services built on this family could legitimately answer differently. Common baseline features from the Feature Model (`DomainLogic`, `HttpApi`, `AppLogging`, `TestConformance` + its four children) are **not** rows here — every path through the family includes them. See [Why AsyncInboundApi/AsyncOutboundApi are single rows](#why-asyncinboundapiasyncoutboundapi-are-single-rows).

| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| VP1 | **GrpcApi** — does the module expose a second inbound entry point over gRPC, alongside the common `HttpApi`? | Yes / No | — | Yes → [solution-grpc-api](skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill) | — | No |
| VP2 | **ExternalIntegration** — does the module call out to another service through a domain-declared outbound port? | Yes / No | — | Yes → [solution-external-integration](skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill) | — | No |
| VP3 | **AsyncOutboundApi** — does the module publish asynchronous messages for other services to react to? | Yes / No | — (gates VP4) | **skeleton** → [[skills/go/architecture/solutions/solution-messaging-infrastructure.skill/solution-messaging-infrastructure.skill\|solution-messaging-infrastructure]] + [[skills/go/architecture/solutions/solution-kafka-producer.skill/solution-kafka-producer.skill\|solution-kafka-producer]] (draft) | Mandatory sub-feature: [[skills/go/architecture/solutions/solution-messaging-infrastructure.skill/solution-messaging-infrastructure.skill\|solution-messaging-infrastructure]] (shared with VP5) | No |
| VP4 | **OutboxPattern** — write outgoing messages to a transactional outbox in the same transaction as the persisted business change, then relay them, instead of publishing directly? | Yes / No | **Yes requires (VP3=Yes AND VP7=Yes)** — jointly AND — see [note](#vp4s-constraint--the-owners-own-rule) | **skeleton** → [[skills/go/architecture/solutions/solution-transactional-outbox.skill/solution-transactional-outbox.skill\|solution-transactional-outbox]] (draft) | Cross-feature interaction with VP3: changes *how* the message is written (staged in the persistent store first, relayed by a background process), not whether VP3 is legal | No |
| VP5 | **AsyncInboundApi** — does the module react to asynchronous messages from other services? | Yes / No | — | **skeleton** → `solution-messaging-infrastructure` + [[skills/go/architecture/solutions/solution-kafka-consumer.skill/solution-kafka-consumer.skill\|solution-kafka-consumer]] (draft) | Mandatory sub-feature: `solution-messaging-infrastructure` (shared with VP3) | No |
| VP6 | **CachedDb** — does the module depend on a narrow, business-named outbound port backed by a cache-capable store? | Yes / No | — | Yes → [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill\|solution-cached-db]] | — | No |
| VP7 | **PersistentDb** — does the module depend on a narrow, business-named outbound port backed by a durable system-of-record store? | Yes / No | — (gates VP4) | Yes → [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill\|solution-persistent-db]] | — | No |

### VP4's constraint — the owner's own rule

The Feature Model draws a single `Requires` edge, `OutboxPattern -> PersistentDb`; combined with `OutboxPattern` being drawn as a child of `AsyncOutboundApi` in the diagram (so it is only selectable once VP3=Yes), the table's Constraint states both as a joint AND, matching the dotnet catalog's equivalent VP14 shape.

This catalog's owner stated the rule directly, not as architectural inference: *"если есть БД и публикация связана с изменением данных в БД, то делается через pattern outbox"* — if the module has a persistent DB **and** the publication is tied to a change in that DB's data, it is done via the outbox pattern. Two things follow that the Constraint column's Yes/No shape cannot express on its own:
- The precondition is **VP3=Yes AND VP7=Yes**, exactly as stated in the table.
- Once both hold, whether a *specific* publication uses the outbox is not itself a further catalog-level choice — it is **required** for any publication that is triggered by a persisted-data change (direct `solution-kafka-producer` publish is still legal for a publication that is not tied to a persisted change, e.g. a pure computed/derived event). `solution-transactional-outbox`'s own Boundaries state this precisely; the Variability Map records only the legality gate.

### Why AsyncInboundApi/AsyncOutboundApi are single rows

Following [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]]'s "Alternatives share a VP, combinables split" rule and the Feature Model's own framing: `AsyncInboundApi` and `AsyncOutboundApi` are each 1:1 with a single `Mandatory` realization (`KafkaConsumer`, `KafkaProducer` respectively) today, not an "at least one of N" group — so each stays one boolean VP (matching the dotnet catalog's identical VP12/VP13 reasoning), rather than being expanded into an umbrella-plus-variant shape. A second broker realization later is a change to that VP's `Realized by`, not a new row.

`GrpcApi` and `ExternalIntegration` are likewise not grouped with anything — per `feature-model.md`'s "Modeling choices" section, this family's `HttpApi` is common (so `GrpcApi` has no sibling to form an "at least one" pair with), and `ExternalIntegration` was deliberately kept as one feature rather than split by transport.

## Plateau ↔ VP view

The plateau↔VP matrix lives in [plateau/plateau-repository.md](skills/go/architecture/plateau/plateau-repository.md), maintained per [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md|plateau-map-create]]. This map intentionally carries no Plateau Map derivation.

## Out of scope

- **VP3–VP5 are skeletons.** Their solution skills exist in `solutions/` with a `> Draft contract` marker and one shape-only Implementation file; full authoring is deferred until a real consumer (a sixth plateau) exists. Their Constraints/notes come from the Feature Model and the owner's stated rule, not from working code.
- **Constraint evidence.** VP4's `requires VP3 AND VP7` is drawn from the Feature Model's `Requires` edge plus the owner's own stated business rule (quoted above) — not inferred. No other VP carries a constraint; the absence reflects this catalog having no per-entity axes or DomainLogic-gating the way the dotnet catalog does (`DomainLogic` here is common, not a VP, so nothing gates on it).
- **Migration is `No` everywhere** — this is a brand-new catalog; no service built on it has yet been observed changing a VP answer after being composed. Per the parent skill, `Migration` is set `Yes` only on a real observed transition, never speculatively.
- **The plateau↔VP view lives outside this map** — see [plateau/plateau-repository.md](skills/go/architecture/plateau/plateau-repository.md); this file intentionally ends at the VP↔solution binding.
- **No categorical (multi-variant) VP in this catalog** — every row is boolean (Yes/No). Nothing in the current feature set is a mutually-exclusive-alternatives choice; if a second realization of `GrpcApi`-shaped inbound or `AsyncInboundApi`-shaped consumption is added later, revisit whether it stays a boolean addition to `Realized by` or needs a categorical Variant split.
