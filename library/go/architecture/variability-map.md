---
tags:
  - concern/architecture
  - stack/go
---

# library/go/architecture Variability Map

Built per [[library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill|variability-map-create]], from the non-common features of [[library/go/architecture/feature/feature-model|feature/feature-model.md]]. This map is the input to [[library/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]].

**Status of this catalog.** `solutions/` holds this catalog's own solution skills, authored fresh (no prior catalog to migrate from — see `agent/DECISIONS.md`). Every **Realized by** cell links into `solutions/`. The common rows VP-C006, VP-C007, VP-C010 (Kafka publish, Kafka consume, outbox) are **aspirational**: their solutions are skeletons with a draft-contract marker — no plateau in this catalog's first build realizes them yet. `plateau/` holds the five plateaus built from this map; the plateau↔VP view lives in `plateau/plateau-repository.md`, maintained per [[library/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md|plateau-map-create]].

## Common Variation Points

Inherited from the [[library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]], one row per common VP. A stack VP below moves here, re-IDed to its `VP-C###` ID, when this stack details the common VP covering it.

| ID | VP | Status | State | Stack delta | Realized by | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| [VP-C001](library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c001-persistentstore/vp-c001-persistentstore.md) | PersistentStore | ✅ | Inherited | — | PostgreSQL → [solution-persistent-db](library/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill), optionally paired with [solution-go-db-migrations](library/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill) (see [note](#solution-go-db-migrations-is-part-of-vp-c001-postgresql-not-a-new-vp)); SQLite → planned — `modernc.org/sqlite` (pure Go, no CGO) behind `solution-persistent-db`'s narrow port | No |
| [VP-C002](library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c002-transientstore/vp-c002-transientstore.md) | TransientStore | ✅ | Inherited | — | Redis → [solution-cached-db](library/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill); InMemory → planned — stdlib `map` + `sync.RWMutex` with per-entry TTL behind `solution-cached-db`'s narrow port, no dependency | No |
| [VP-C003](library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c003-taskbox/vp-c003-taskbox.md) | TaskBox | ✅ | Inherited | — | Yes → own realization of the [contract](library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c003-taskbox/vp-c003-taskbox.contract.md), per store: library `taskbox-go` → [solution-taskbox-in-go](library/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill) (pointer; base [solution-taskbox](library/common-workflow/architecture/solutions/solution-taskbox.skill/solution-taskbox.skill) → `taskbox-spec`). PostgreSQL → `pgx`, enqueue inside the data adapter's `pgx.Tx` — proven, pre-release copy in GW009.001 until v0.1.0; SQLite → planned — `modernc.org/sqlite` through `database/sql`; Redis → planned — `go-redis` in `taskbox-go`, enqueue in the caller's `TxPipeline`, Lua scripts (Redis questions resolved in the spec 2026-09-29); InMemory → planned — stdlib only (goroutines, `container/heap`) | No |
| [VP-C004](library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c004-httpoutbound/vp-c004-httpoutbound.md) | HttpOutbound | ✅ | Inherited | — | Yes → planned — `net/http` client behind a domain-named port; deadline through `context`; own backoff retry for idempotent operations; circuit breaker via `sony/gobreaker` where wanted | No |
| [VP-C005](library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c005-grpcoutbound/vp-c005-grpcoutbound.md) | GrpcOutbound | ✅ | Inherited | — | Yes → [solution-external-integration](library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill) (alignment pending: its adapter maps failures to sentinel errors today, not to HTTP status codes) | No |
| [VP-C006](library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c006-kafkaproducer/vp-c006-kafkaproducer.md) | KafkaProducer | ✅ | Inherited | — | Yes → **skeleton** [solution-go-messaging-infrastructure](library/go/architecture/solutions/solution-go-messaging-infrastructure.skill/solution-go-messaging-infrastructure.skill) + [solution-go-kafka-producer](library/go/architecture/solutions/solution-go-kafka-producer.skill/solution-go-kafka-producer.skill) — client `twmb/franz-go`; own CloudEvents Kafka binary binding (`ce_*` headers) in the messaging infrastructure over `cloudevents/sdk-go`'s event model (alignment pending: the skeleton leaves the client undecided and has its own envelope) | No |
| [VP-C007](library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c007-kafkaconsumer/vp-c007-kafkaconsumer.md) | KafkaConsumer | ✅ | Inherited | — | Yes → **skeleton** `solution-go-messaging-infrastructure` + [solution-go-kafka-consumer](library/go/architecture/solutions/solution-go-kafka-consumer.skill/solution-go-kafka-consumer.skill) — `twmb/franz-go` consumer group (cooperative-sticky), offset committed after handling (alignment pending: as VP-C006) | No |
| [VP-C008](library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c008-rabbitmqproducer/vp-c008-rabbitmqproducer.md) | RabbitMqProducer | ✅ | Inherited | — | Yes → planned — `rabbitmq/amqp091-go` with publisher confirms; CloudEvents structured JSON through `cloudevents/sdk-go`; on the shared messaging infrastructure | No |
| [VP-C009](library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c009-rabbitmqconsumer/vp-c009-rabbitmqconsumer.md) | RabbitMqConsumer | ✅ | Inherited | — | Yes → planned — `rabbitmq/amqp091-go`, manual ack, dead-letter exchange; CloudEvents structured JSON | No |
| [VP-C010](library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c010-outbox/vp-c010-outbox.md) | Outbox | ✅ | Inherited | — | Yes → **skeleton** [solution-go-transactional-outbox](library/go/architecture/solutions/solution-go-transactional-outbox.skill/solution-go-transactional-outbox.skill) (alignment pending: it keeps its own Kafka-only outbox table; it becomes the TaskBox-based envelope of the contract) — generic handlers on the VP-C004/VP-C006/VP-C008 clients (`net/http`, `franz-go`, `amqp091-go`) | No |
| [VP-C011](library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c011-inbox/vp-c011-inbox.md) | Inbox | ✅ | Inherited | — | Yes → planned — own realization of the Inbox contract: a `net/http` middleware for only-once endpoints on the baseline HTTP API (`solution-go-http-api`) plus the status endpoint; broker inputs enqueued by the VP-C007/VP-C009 consumers (`franz-go`, `amqp091-go`); `status_key` via `google/uuid` `NewRandom` (crypto/rand) | No |

## Stack Variation Points

Each row is one axis on which two Go web-services built on this family could legitimately answer differently. Common baseline features from the Feature Model (`DomainLogic`, `HttpApi`, `AppLogging`, `TestConformance` + its four children) are **not** rows here — every path through the family includes them. See [Why AsyncInboundApi/AsyncOutboundApi are single rows](#why-asyncinboundapiasyncoutboundapi-are-single-rows).

| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| VP1 | **GrpcApi** — does the module expose a second inbound entry point over gRPC, alongside the common `HttpApi`? | Yes / No | — | Yes → [solution-grpc-api](library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill) | — | No |

### VP1 is to be revisited

> **To revisit — deferred by the owner, 2026-10-12.** The VP1 row above and every statement in this catalog that `GrpcApi` is purely an addition describe the catalog before issue #206. Since then `solution-grpc-api` also replaces the baseline's hand-written `openapi.json` by the proto file and moves the HTTP handlers onto the generated messages, so:
> - VP1's realization is no longer additive, and its `Realization depends on` cell is not `—` in substance: it changes `internal/api/http/` and `docs/integration/api/`.
> - `{VP1, VP-C005}` and `{VP1, VP-C001}` intersect on `internal/api/http/server.go` as `FMC`. No resolver solution exists; `solution-external-integration` and `solution-persistent-db` state their HTTP delta in two forms, with and without VP1 — see [registry/internal-api-http-server-go.md](library/go/architecture/registry/internal-api-http-server-go.md).
> - Only the with-VP1 forms are verified: no plateau composes VP-C005 or VP-C001 without VP1.
>
> Ways out, none chosen: a contract-first baseline (the proto file from GW001.000, VP1 additive again, both pairs back to `FMN`); a resolver solution per pair; a Constraint that VP-C005 and VP-C001 require VP1. Until then this map, [plateau/plateau-repository.md](library/go/architecture/plateau/plateau-repository.md), and `feature/feature-model.md`'s "Modeling choices" keep their earlier wording on VP1.

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

The plateau↔VP matrix lives in [plateau/plateau-repository.md](library/go/architecture/plateau/plateau-repository.md), maintained per [[library/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md|plateau-map-create]]. This map intentionally carries no Plateau Map derivation.

## Out of scope

- **VP-C006, VP-C007, VP-C010 are skeletons.** Their solution skills exist in `solutions/` with a `> Draft contract` marker and one shape-only Implementation file; full authoring is deferred until a real consumer (a sixth plateau) exists. Their Constraints/notes come from the Feature Model and the owner's stated rule, not from working code.
- **Constraint evidence.** No stack VP carries a constraint; the outbox Constraint is common VP-C010's.
- **Migration is `No` everywhere** — this is a brand-new catalog; no service built on it has yet been observed changing a VP answer after being composed. Per the parent skill, `Migration` is set `Yes` only on a real observed transition, never speculatively.
- **The plateau↔VP view lives outside this map** — see [plateau/plateau-repository.md](library/go/architecture/plateau/plateau-repository.md); this file intentionally ends at the VP↔solution binding.
- **No categorical (multi-variant) stack VP in this catalog** — every stack row is boolean (Yes/No); the categorical storage questions are the common VP-C001/VP-C002. Nothing in the current feature set is a mutually-exclusive-alternatives choice; if a second realization of `GrpcApi`-shaped inbound or `AsyncInboundApi`-shaped consumption is added later, revisit whether it stays a boolean addition to `Realized by` or needs a categorical Variant split.
- **`solution-go-db-migrations` was added to VP-C001's `Realized by` after this build's five plateaus were already composed.** The five earlier plateaus were not retrofitted; `GW009.001` (plateau `gw009-001`) is the first to compose it, and running it there for real found and fixed a defect the compile-only check had missed (goose was given the embed root instead of `migrations/`) — see [solution-go-db-migrations is part of VP-C001 PostgreSQL, not a new VP](#solution-go-db-migrations-is-part-of-vp-c001-postgresql-not-a-new-vp) above.
