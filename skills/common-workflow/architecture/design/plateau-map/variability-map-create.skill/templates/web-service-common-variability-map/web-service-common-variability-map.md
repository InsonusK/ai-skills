# Web-service common Variability Map

The Variation Points every backend web-service catalog shares, whatever its stack. Each one is defined here once — question, Variants, Constraint, Realization depends on, and the concept behind them — and **inherited** by every bound stack map, which adds only its State, its narrowing, and its `Realized by`. Rules: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md#common-variation-points|variability-map-create — Common Variation Points]]. Derived from, and kept consistent with, [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/templates/web-service-common-features/web-service-common-features|web-service-common-features]].

A VP enters this map only through [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md#how-to-admit-a-common-variation-point|admission]]: 💡 candidate → 📐 concept agreed with the owner → ✅ detailed on each bound stack. Status icons: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md#track-vp-status|Track VP status]].

## Common Variation Points

| ID | Status | VP | Variants | Constraint | Realization depends on |
| --- | --- | --- | --- | --- | --- |
| VP-C001 | 📐 | **PersistentStore** — where does the service keep data it must never lose? | None / PostgreSQL / SQLite | — | — |
| VP-C002 | 📐 | **TransientStore** — where does the service keep data that has a lifetime? | None / Redis / InMemory | — | — |
| VP-C003 | 📐 | **TaskBox** — does the service defer work: store a task now, execute it later in a background worker? | Yes / No | Yes requires (VP-C001 ≠ None OR VP-C002 ≠ None) | Tasks live in the store(s) VP-C001/VP-C002 select — see [VP-C003 TaskBox](#vp-c003-taskbox) |

## Candidate Variation Points

Identified, not yet agreed — no ID until the concept is agreed. In discussion order: a candidate is admitted only after every VP in its **Admitted after** column, because its concept will reference them. `▶` marks the one under discussion.

| Status | Candidate | Admitted after | Covers today | Agreed so far / open question |
| --- | --- | --- | --- | --- |
| 💡 ▶ | Outbound protocols | — | request/response calls to other services; Go VP2 `ExternalIntegration` (gRPC-only realization), dotnet VP10/VP11 | Open: HTTP and gRPC as separate VPs; does Go's ExternalIntegration become the gRPC one? |
| 💡 | Messaging | — | Kafka/RabbitMQ publish and consume; Go VP3/VP5, dotnet VP12/VP13 | Open: shared messaging infrastructure as a mandatory sub-feature |
| 💡 | Outbox | Outbound protocols, Messaging | outbound calls made through TaskBox (VP-C003) instead of directly; Go VP4, dotnet VP14 (Kafka + PostgreSQL today) | Agreed: no own storage — an outbound call is a TaskBox task; message key = `queue_group`; the task `id` travels as the message id; a **common envelope** — task type `outbox.<adapter>`, payload `{target, key, headers, body}`, one generic handler per adapter — fixed in a contract beside TaskBox's; a service may add its own handler that also processes the response (a saga step). Open: the exact envelope |
| 💡 | Saga | Outbox | orchestrated multi-step processes: a handler that processes a response and enqueues the next step | Open: a VP of its own (saga state, compensations, timeouts) or only a documented use of Outbox custom handlers? |
| 💡 | Inbound protocols | — | HTTP is mandatory for every backend service (owner) → baseline, not a VP; gRPC optional. Go VP1, dotnet VP8/VP9 | Open: dotnet's family is a `Module` — can a module lack HTTP? |
| 💡 | DomainLogic | — | dotnet VP1; baseline in Go | Open: common VP with Go `Fixed: Yes`, or dotnet-only? |
| 💡 | Metric | — | observability | Open: needed now, or when a stack first needs it? |
| 💡 | Domain modelling | DomainLogic | ValueObjects, SharedRules, concurrency control, external identity, audit timestamps — dotnet VP3–VP7 | Open: stay dotnet-only until a second stack needs one? |
| 💡 | Deployment | — | SingleInstance / MultiInstance — SQLite (VP-C001) and InMemory (VP-C002) bind a service to one instance | **Discuss with the owner first:** a real VP with a Constraint, or only the consequence already stated in VP-C001/VP-C002? |

### VP-C001 PersistentStore
The system of record: **data that survives restarts and redeploys and is never deliberately discarded**. Business changes are written in transactions, which later VPs (TaskBox, Outbox) rely on to stay atomic with the change they report.
- **None** — the service owns no durable state (stateless, or delegates state to other services).
- **PostgreSQL** — a separate database server; fits any number of service instances.
- **SQLite** — an embedded database file; binds the service to a single instance (one writer), for deployments that do not scale out.
- One store kind per service: the Variants are alternatives, so a service with durable state picks exactly one.
- Boundary with VP-C002: the test is whether losing the data is acceptable, not which technology holds it.

### VP-C002 TransientStore
A store in which **every entity has a lifetime** (TTL): temporary state, sessions, derived or recomputable values, caches in front of slower sources. Data that has a lifetime lives only here — never in VP-C001 — and for much of it this is the **primary** home, not a copy of VP-C001 data: a cache is one use of this store, not its definition. Losing an entry early (eviction, failover, restart) is the same event as its lifetime expiring, so the service must already survive it.
- **None** — no state with a lifetime beyond a single request.
- **Redis** — a separate server; shared across service instances; loses entries early only on failover or eviction.
- **InMemory** — inside the service process; per instance; every restart ends every lifetime early; binds any state that must be shared across requests to a single instance.
- One store kind per service, for the same reason as VP-C001.
- Independent of VP-C001: a service may have either, both, or neither.

### VP-C003 TaskBox
Deferred execution: the service stores a task and a background worker executes it later, retrying until it succeeds or is dead-lettered. Delivery is at-least-once, so every task handler is idempotent. Outbox (a candidate) builds on it.
- **Where a task lives** — in a store the service has: VP-C001 (PostgreSQL / SQLite) or VP-C002 (Redis / InMemory). A service with both may use both; which store a task type uses is the service's own choice.
- **Criticality is a property of the task type, not a VP.** It sets the least store the task may live in: a **Critical** task lives only in VP-C001; a **NonCritical** task may live in either. A task in VP-C002 has a lifetime like every entry there and may be lost before it runs — acceptable only for NonCritical work.
- **Atomic with a data change only in the same store.** A task is enqueued atomically with the business change it follows only when the task and that data live in the same store (the same transaction in VP-C001, the same `MULTI` in Redis). Data in one store and its task in the other cannot be made transactional: a crash between the two writes leaves a change without its task, or a task without its change.
- **No ordering across stores.** Tasks in different stores are drained independently; two tasks about the same entity kept in different stores may run in either order. A service that splits task types across stores owns that race.
- **Data with a lifetime needs no task to expire it** — its store's lifetime (VP-C002) does that.
- **One storage contract for every stack** — [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/contracts/vp-c003-taskbox|contracts/vp-c003-taskbox]] fixes the task record, ordering by `queue_group`, lifecycle, retention, and the schema of each store; a stack realizes it with its own client and never with a job-queue library that brings its own schema.

## Bound stack maps

Every map below carries every row of the table above in its own `## Common Variation Points` table. Plain paths, not links — this file belongs to a stack-agnostic skill.

- `skills/go/architecture/variability-map.md`
- `skills/dotnet/architecture/variability-map.md`
