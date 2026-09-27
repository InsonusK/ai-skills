---
tags:
  - concern/architecture
  - stack/go
---

# skills/go/architecture Variability Map

Built per [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill|variability-map-create]], from the non-common features of [[skills/go/architecture/feature/feature-model|feature/feature-model.md]]. This map is the input to [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]].

**Status of this catalog.** `solutions/` holds this catalog's own solution skills, authored fresh (no prior catalog to migrate from — see `agent/DECISIONS.md`). Every **Realized by** cell links into `solutions/`. The common rows VP-C006, VP-C007, VP-C010 (Kafka publish, Kafka consume, outbox) are **aspirational**: their solutions are skeletons with a draft-contract marker — no plateau in this catalog's first build realizes them yet. `plateau/` holds the five plateaus built from this map; the plateau↔VP view lives in `plateau/plateau-repository.md`, maintained per [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md|plateau-map-create]].

## Common Variation Points

Inherited from the [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]], one row per common VP. A stack VP below moves here, re-IDed to its `VP-C###` ID, when this stack details the common VP covering it.

| ID | VP | Status | State | Stack delta | Realized by | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| [VP-C001](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map.md#vp-c001-persistentstore) | PersistentStore | ✅ | Inherited | — | PostgreSQL → [solution-persistent-db](skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill), optionally paired with [solution-go-db-migrations](skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill) (see [note](#solution-go-db-migrations-is-part-of-vp-c001-postgresql-not-a-new-vp)); SQLite → planned — `modernc.org/sqlite` (pure Go, no CGO) behind `solution-persistent-db`'s narrow port | No |
| [VP-C002](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map.md#vp-c002-transientstore) | TransientStore | ✅ | Inherited | — | Redis → [solution-cached-db](skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill); InMemory → planned — stdlib `map` + `sync.RWMutex` with per-entry TTL behind `solution-cached-db`'s narrow port, no dependency | No |
| [VP-C003](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map.md#vp-c003-taskbox) | TaskBox | ✅ | Inherited | — | Yes → own realization of the [contract](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/contracts/vp-c003-taskbox.md), per store: PostgreSQL → planned — `pgx`, enqueue inside the caller's `pgx.Tx`; SQLite → planned — `modernc.org/sqlite` through `database/sql`; Redis → planned — `go-redis`, enqueue in the caller's `TxPipeline`, Lua scripts for keyed enqueue, due mover and requeue/cancel; InMemory → planned — stdlib only (goroutines, `container/heap`) | No |
| [VP-C004](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map.md#vp-c004-httpoutbound) | HttpOutbound | ✅ | Inherited | — | Yes → planned — `net/http` client behind a domain-named port; deadline through `context`; own backoff retry for idempotent operations; circuit breaker via `sony/gobreaker` where wanted | No |
| [VP-C005](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map.md#vp-c005-grpcoutbound) | GrpcOutbound | ✅ | Inherited | — | Yes → [solution-external-integration](skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill) (alignment pending: its adapter maps failures to sentinel errors today, not to HTTP status codes) | No |
| [VP-C006](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map.md#vp-c006-kafkaproducer) | KafkaProducer | ✅ | Inherited | — | Yes → **skeleton** [solution-go-messaging-infrastructure](skills/go/architecture/solutions/solution-go-messaging-infrastructure.skill/solution-go-messaging-infrastructure.skill) + [solution-go-kafka-producer](skills/go/architecture/solutions/solution-go-kafka-producer.skill/solution-go-kafka-producer.skill) — client `twmb/franz-go`; own CloudEvents Kafka binary binding (`ce_*` headers) in the messaging infrastructure over `cloudevents/sdk-go`'s event model (alignment pending: the skeleton leaves the client undecided and has its own envelope) | No |
| [VP-C007](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map.md#vp-c007-kafkaconsumer) | KafkaConsumer | ✅ | Inherited | — | Yes → **skeleton** `solution-go-messaging-infrastructure` + [solution-go-kafka-consumer](skills/go/architecture/solutions/solution-go-kafka-consumer.skill/solution-go-kafka-consumer.skill) — `twmb/franz-go` consumer group (cooperative-sticky), offset committed after handling (alignment pending: as VP-C006) | No |
| [VP-C008](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map.md#vp-c008-rabbitmqproducer) | RabbitMqProducer | ✅ | Inherited | — | Yes → planned — `rabbitmq/amqp091-go` with publisher confirms; CloudEvents structured JSON through `cloudevents/sdk-go`; on the shared messaging infrastructure | No |
| [VP-C009](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map.md#vp-c009-rabbitmqconsumer) | RabbitMqConsumer | ✅ | Inherited | — | Yes → planned — `rabbitmq/amqp091-go`, manual ack, dead-letter exchange; CloudEvents structured JSON | No |
| [VP-C010](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map.md#vp-c010-outbox) | Outbox | ✅ | Inherited | — | Yes → **skeleton** [solution-go-transactional-outbox](skills/go/architecture/solutions/solution-go-transactional-outbox.skill/solution-go-transactional-outbox.skill) (alignment pending: it keeps its own Kafka-only outbox table; it becomes the TaskBox-based envelope of the contract) — generic handlers on the VP-C004/VP-C006/VP-C008 clients (`net/http`, `franz-go`, `amqp091-go`) | No |

## Stack Variation Points

Each row is one axis on which two Go web-services built on this family could legitimately answer differently. Common baseline features from the Feature Model (`DomainLogic`, `HttpApi`, `AppLogging`, `TestConformance` + its four children) are **not** rows here — every path through the family includes them. See [Why AsyncInboundApi/AsyncOutboundApi are single rows](#why-asyncinboundapiasyncoutboundapi-are-single-rows).

| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| VP1 | **GrpcApi** — does the module expose a second inbound entry point over gRPC, alongside the common `HttpApi`? | Yes / No | — | Yes → [solution-grpc-api](skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill) | — | No |

### The owner's outbox rule is now common

This catalog's owner rule — *"если есть БД и публикация связана с изменением данных в БД, то делается через pattern outbox"* — is part of common VP-C010's concept for every stack; the Constraint is VP-C010's.

### solution-go-db-migrations is part of VP-C001 PostgreSQL, not a new VP

`solution-persistent-db`'s own `# Boundaries` names a real gap: its adapter ensures its table with a
bare `CREATE TABLE IF NOT EXISTS`, adequate only for this catalog's own runnable examples, with no
versioning and no way to run schema changes independently of starting the service.
`solution-go-db-migrations` (`depends_on` `solution-persistent-db`; see its own ADR for the
goose-based tool choice) closes that gap. It is **not** modeled as its own Variation Point: it
answers no question a team could legitimately decide independently of VP-C001 itself — it only
exists, and only makes sense, once PersistentStore is `PostgreSQL`. It is recorded directly in
VP-C001's own `Realized by` cell instead, the same way VP-C006's row names two solutions (`solution-go-messaging-
infrastructure` + `solution-go-kafka-producer`) without splitting into two rows.

Unlike VP-C006's pairing, `solution-go-db-migrations` is not *mandatory* whenever VP-C001 is
`PostgreSQL` — `plateau-persistent-service` (built before this solution existed) composes
`solution-persistent-db` alone and remains a legal, correct realization of VP-C001; a team wanting versioned migrations applies
`solution-go-db-migrations` on top. This asymmetry is deliberate: forcing it mandatory would make
the map disagree with `plateau-persistent-service`'s own already-verified `created_by`, which
`plateau/plateau-repository.md`'s Constraint check would then have to flag as a real violation
instead of a legal, simply-uncomposed combination.

### Stack VPs are single rows

The Feature Model's `AsyncOutboundApi`/`AsyncInboundApi` (each 1:1 with `KafkaProducer`/`KafkaConsumer`) are now carried as common VP-C006/VP-C007; RabbitMQ is common VP-C008/VP-C009.

`GrpcApi` is likewise not grouped with anything — per `feature-model.md`'s "Modeling choices" section, this family's `HttpApi` is common, so `GrpcApi` has no sibling to form an "at least one" pair with. The Feature Model's `ExternalIntegration` was kept as one feature rather than split by transport; its only realization is gRPC, so it is carried as common VP-C005 GrpcOutbound, and HTTP outbound calls are common VP-C004.

## Plateau ↔ VP view

The plateau↔VP matrix lives in [plateau/plateau-repository.md](skills/go/architecture/plateau/plateau-repository.md), maintained per [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md|plateau-map-create]]. This map intentionally carries no Plateau Map derivation.

## Out of scope

- **VP-C006, VP-C007, VP-C010 are skeletons.** Their solution skills exist in `solutions/` with a `> Draft contract` marker and one shape-only Implementation file; full authoring is deferred until a real consumer (a sixth plateau) exists. Their Constraints/notes come from the Feature Model and the owner's stated rule, not from working code.
- **Constraint evidence.** No stack VP carries a constraint; the outbox Constraint is common VP-C010's.
- **Migration is `No` everywhere** — this is a brand-new catalog; no service built on it has yet been observed changing a VP answer after being composed. Per the parent skill, `Migration` is set `Yes` only on a real observed transition, never speculatively.
- **The plateau↔VP view lives outside this map** — see [plateau/plateau-repository.md](skills/go/architecture/plateau/plateau-repository.md); this file intentionally ends at the VP↔solution binding.
- **No categorical (multi-variant) stack VP in this catalog** — every stack row is boolean (Yes/No); the categorical storage questions are the common VP-C001/VP-C002. Nothing in the current feature set is a mutually-exclusive-alternatives choice; if a second realization of `GrpcApi`-shaped inbound or `AsyncInboundApi`-shaped consumption is added later, revisit whether it stays a boolean addition to `Realized by` or needs a categorical Variant split.
- **`solution-go-db-migrations` was added to VP-C001's `Realized by` after this build's five plateaus were already composed.** It is fully authored (not a skeleton) and ground-truth-checked at the solution level (its `Migrate` code compiles and vets against the real `github.com/pressly/goose/v3 v3.28.0` release), but no existing plateau has been retrofitted to compose it alongside `solution-persistent-db` — see [solution-go-db-migrations is part of VP-C001 PostgreSQL, not a new VP](#solution-go-db-migrations-is-part-of-vp-c001-postgresql-not-a-new-vp) above.
