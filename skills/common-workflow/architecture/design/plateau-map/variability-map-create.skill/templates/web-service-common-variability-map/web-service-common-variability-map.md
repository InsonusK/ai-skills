# Web-service common Variability Map

The Variation Points every backend web-service catalog shares, whatever its stack. Each one is defined here once — question, Variants, Constraint, Realization depends on, and the concept behind them — and **inherited** by every bound stack map, which adds only its State, its narrowing, and its `Realized by`. Rules: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md#common-variation-points|variability-map-create — Common Variation Points]]. Derived from, and kept consistent with, [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/templates/web-service-common-features/web-service-common-features|web-service-common-features]].

A VP enters this map only through [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md#how-to-admit-a-common-variation-point|admission]]: 💡 candidate → 📐 concept agreed with the owner → ✅ detailed on each bound stack. Status icons: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md#track-vp-status|Track VP status]].

## Common Variation Points

| ID | Status | VP | Variants | Constraint | Realization depends on |
| --- | --- | --- | --- | --- | --- |
| VP-C001 | 📐 | **PersistentStore** — where does the service keep data it must never lose? | None / PostgreSQL / SQLite | — | — |
| VP-C002 | 📐 | **TransientStore** — where does the service keep data it can survive losing? | None / Redis / InMemory | — | — |

## Candidate Variation Points

Identified, not yet agreed — no ID until the concept is agreed. Listed in the order they can be discussed: a candidate comes after every VP its concept will reference.

| Status | Candidate | Covers today | Open question |
| --- | --- | --- | --- |
| 💡 | TaskBox | deferred execution: a task is stored, then run by a background worker, in the store that holds its data (PostgreSQL → library with in-transaction enqueue; Redis → shared Redis-Streams contract, enqueue inside the caller's `MULTI`) | Does a Critical/NonCritical guarantee remain a choice, or is it fully determined by the store? |
| 💡 | Outbox | outbound calls go through TaskBox, enqueued in the same atomic write, in the same store, as the business change; at-least-once + idempotency key | Go VP4 / dotnet VP14 require Kafka today — generalize to any outbound protocol? |
| 💡 | Inbound protocols | HTTP is mandatory for every backend service (owner) → baseline, not a VP; gRPC optional. Go VP1, dotnet VP8/VP9 | dotnet's family is a `Module` — can a module lack HTTP? |
| 💡 | Outbound protocols | Go VP2 `ExternalIntegration` (gRPC-only realization), dotnet VP10/VP11 | Does Go's transport-agnostic ExternalIntegration become the gRPC VP? |
| 💡 | Messaging | Kafka/RabbitMQ consume/produce; Go VP3/VP5, dotnet VP12/VP13 | Shared messaging infrastructure as a mandatory sub-feature |
| 💡 | DomainLogic | dotnet VP1; baseline in Go | Common VP with Go `Fixed: Yes`, or dotnet-only? |
| 💡 | Metric | observability | Needed now, or when a stack first needs it? |
| 💡 | Domain modelling | ValueObjects, SharedRules, concurrency control, external identity, audit timestamps — dotnet VP3–VP7 | Stay dotnet-only until a second stack needs one? |
| 💡 | Deployment | SingleInstance / MultiInstance — SQLite (VP-C001) and InMemory (VP-C002) bind a service to one instance | **Discuss with the owner first:** a real VP with a Constraint, or only the consequence already stated in VP-C001/VP-C002? |

### VP-C001 PersistentStore
The system of record: data that survives restarts and redeploys and is never deliberately discarded. Business changes are written in transactions, which later VPs (TaskBox, Outbox) rely on to stay atomic with the change they report.
- **None** — the service owns no durable state (stateless, or delegates state to other services).
- **PostgreSQL** — a separate database server; fits any number of service instances.
- **SQLite** — an embedded database file; binds the service to a single instance (one writer), for deployments that do not scale out.
- One store kind per service: the Variants are alternatives, so a service with durable state picks exactly one.
- Boundary with VP-C002: the test is whether losing the data is acceptable, not which technology holds it.

### VP-C002 TransientStore
Data the service can survive losing — temporary state, short-lived sessions, derived or recomputable values, and caches in front of slower sources. For some data this is the **primary** home, not a copy of VP-C001 data: a cache is one use of this store, not its definition.
- **None** — no transient state beyond a single request.
- **Redis** — a separate server; shared across service instances; loses data only on failover or eviction.
- **InMemory** — inside the service process; per instance, lost on every restart; binds any state that must be shared across requests to a single instance.
- One store kind per service, for the same reason as VP-C001.
- Independent of VP-C001: a service may have either, both, or neither.

## Bound stack maps

Every map below carries every row of the table above in its own `## Common Variation Points` table. Plain paths, not links — this file belongs to a stack-agnostic skill.

- `skills/go/architecture/variability-map.md`
- `skills/dotnet/architecture/variability-map.md`
