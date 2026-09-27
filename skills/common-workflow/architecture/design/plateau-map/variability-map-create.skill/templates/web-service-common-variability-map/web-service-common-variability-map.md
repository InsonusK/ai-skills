# Web-service common Variability Map

The Variation Points every backend web-service catalog shares, whatever its stack. Each one is defined here once — question, Variants, Constraint, Realization depends on, and the concept behind them — and **inherited** by every bound stack map, which adds only its State, its narrowing, and its `Realized by`. Rules: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md#common-variation-points|variability-map-create — Common Variation Points]]. Derived from, and kept consistent with, [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/templates/web-service-common-features/web-service-common-features|web-service-common-features]].

A VP enters this map only through [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md#how-to-admit-a-common-variation-point|admission]]: 💡 candidate → 📐 concept agreed with the owner → ✅ detailed on each bound stack. Status icons: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md#track-vp-status|Track VP status]].

## Common Variation Points

| ID | Status | VP | Variants | Constraint | Realization depends on |
| --- | --- | --- | --- | --- | --- |
| VP-C001 | 📐 | **PersistentStore** — where does the service keep data it must never lose? | None / PostgreSQL / SQLite | — | — |
| VP-C002 | 📐 | **TransientStore** — where does the service keep data that has a lifetime? | None / Redis / InMemory | — | — |
| VP-C003 | 📐 | **TaskBox** — does the service defer work: store a task now, execute it later in a background worker? | Yes / No | Yes requires (VP-C001 ≠ None OR VP-C002 ≠ None) | Tasks live in the store(s) VP-C001/VP-C002 select — see [VP-C003 TaskBox](#vp-c003-taskbox) |
| VP-C004 | 📐 | **HttpOutbound** — does the service call other services over HTTP? | Yes / No | — | Call rules shared with VP-C005 — see [VP-C004 HttpOutbound](#vp-c004-httpoutbound) |
| VP-C005 | 📐 | **GrpcOutbound** — does the service call other services over gRPC? | Yes / No | — | Every rule of VP-C004's concept applies; independent of VP-C004 — see [VP-C005 GrpcOutbound](#vp-c005-grpcoutbound) |
| VP-C006 | 📐 | **KafkaProducer** — does the service publish events to Kafka? | Yes / No | — | Messaging rules shared with VP-C007–VP-C009 — see [VP-C006 KafkaProducer](#vp-c006-kafkaproducer) |
| VP-C007 | 📐 | **KafkaConsumer** — does the service consume events from Kafka? | Yes / No | — | Messaging rules of VP-C006 — see [VP-C007 KafkaConsumer](#vp-c007-kafkaconsumer) |
| VP-C008 | 📐 | **RabbitMqProducer** — does the service publish messages to RabbitMQ? | Yes / No | — | Messaging rules of VP-C006 — see [VP-C008 RabbitMqProducer](#vp-c008-rabbitmqproducer) |
| VP-C009 | 📐 | **RabbitMqConsumer** — does the service consume messages from RabbitMQ? | Yes / No | — | Messaging rules of VP-C006 — see [VP-C009 RabbitMqConsumer](#vp-c009-rabbitmqconsumer) |
| VP-C010 | 📐 | **Outbox** — are outbound calls made through TaskBox instead of directly? | Yes / No | Yes requires VP-C003 = Yes AND (VP-C004 = Yes OR VP-C005 = Yes OR VP-C006 = Yes OR VP-C008 = Yes) | Tasks and handlers of VP-C003; adapters of VP-C004/VP-C006/VP-C008 — see [VP-C010 Outbox](#vp-c010-outbox) |

## Candidate Variation Points

Identified, not yet agreed — no ID until the concept is agreed. In discussion order: a candidate is admitted only after every VP in its **Admitted after** column, because its concept will reference them. `▶` marks the one under discussion.

| Status | Candidate | Admitted after | Covers today | Agreed so far / open question |
| --- | --- | --- | --- | --- |
| 💡 ▶ | Inbox | — | optional consumer-side mirror of Outbox for messages that must be processed **only once**: the consumer enqueues the message as a TaskBox task (`idempotency_key` = CloudEvents `id`, `queue_group` = message key) and acknowledges only after the commit; dedup, retry, per-key order, and stop-at-dead then come from TaskBox. Direct handling stays the default (simpler) | Open: when to require it; relation to the handler status-code outcome |
| 💡 | Saga | — | orchestrated multi-step processes: a handler that processes a response and enqueues the next step | Open: a VP of its own (saga state, compensations, timeouts) or only a documented use of Outbox custom handlers? |
| 💡 | Inbound protocols | — | HTTP is mandatory for every backend service (owner) → baseline, not a VP; gRPC optional. Go VP1, dotnet VP8/VP9 | **Idea to consider:** one `.proto` defines the API and grpc-gateway (`google.api.http` annotations, plus OpenAPI via `protoc-gen-openapiv2`) serves the same API over HTTP/JSON — gRPC as an optional second entry generated from the same definition, not a second server (Go `solution-grpc-api` runs a separate gRPC server today). Open: dotnet's family is a `Module` — can a module lack HTTP? |
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

### VP-C004 HttpOutbound
Synchronous request/response calls from this service to another service over HTTP. The rules below apply to every outbound call, whatever its protocol (VP-C005 inherits them):
- **Domain-named port** — the port is named for what the domain needs (`ReputationChecker`), never for the dependency or the technology (`IReputationServiceClient`), and has one method per operation the service actually uses. Replacing the provider does not rename the port.
- **Transport stays in the adapter** — the dependency's contract (OpenAPI, `.proto`) is copied into this service and generated; generated and transport types never leave the adapter.
- **Outcome is an HTTP status code** — a failed call returns a failure carrying an HTTP status code as its category; the domain branches on the code, never on transport types. A call that got no response: connection failure → `503`, deadline exceeded → `504`.
- **Retry classification** — retryable: `408`, `429`, `502`, `503`, `504` (honouring `Retry-After` on `429`/`503`); `500` only for an idempotent operation; any other `4xx` never. The same classification drives retries inside a call and, with Outbox, TaskBox's retry-or-dead decision.
- **Every call has a deadline** — configured per dependency, overridable per call; a call without one can hang on an unresponsive peer forever.
- **Retries inside a call only for idempotent operations**, with backoff — repeating a non-idempotent `POST` may apply it twice.
- **Circuit breaker** — recommended; whether and how is the stack's choice.
- Independent of VP-C005: a service may call one dependency over HTTP and another over gRPC.

### VP-C005 GrpcOutbound
Synchronous request/response calls from this service to another service over gRPC. Every rule of [VP-C004 HttpOutbound](#vp-c004-httpoutbound) applies, with two gRPC specifics:
- **Status mapping** — a gRPC status becomes the HTTP status code of the standard gRPC↔HTTP mapping used by grpc-gateway (`NOT_FOUND` → `404`, `INVALID_ARGUMENT` → `400`, `UNAVAILABLE` → `503`, `DEADLINE_EXCEEDED` → `504`, `RESOURCE_EXHAUSTED` → `429`, …).
- **Separate generated packages** — a dependency's generated contract never shares a package with this service's own exposed gRPC contract.
- Independent of VP-C004.

### VP-C006 KafkaProducer
Publishing events to Kafka. The messaging rules below apply to every broker VP (VP-C006–VP-C009); the four are independent, so a service may publish and consume over either broker in any combination.
- **CloudEvents 1.0 envelope** — every message is a CloudEvent (`id`, `source`, `type`, `time`, `datacontenttype`, `data`; trace context through the distributed-tracing extension). Kafka uses the CloudEvents Kafka protocol binding; RabbitMQ uses structured mode (`application/cloudevents+json`), which works over AMQP 0-9-1 and 1.0 alike.
- **Order only within a key** — the message key (Kafka partition key, RabbitMQ routing to one queue) is the only ordering unit.
- **Producer outcome** — by [VP-C004's outcome and retry rules](#vp-c004-httpoutbound): broker unavailable → `503`, message rejected (too large, invalid) → `400`. A retried publish keeps the same CloudEvents `id`, so a duplicate is recognisable.
- **Consumer semantics (baseline, detailed when the solutions are written)** — at-least-once delivery; handlers are idempotent and deduplicate by CloudEvents `id`; a handler's outcome is an HTTP status code: retryable per VP-C004's classification → redelivered, otherwise → dead-letter topic/queue; the message is acknowledged (offset committed) only after it is handled.
- **Shared messaging infrastructure** — envelope, serialization, and tracing are one stack-level building block that every broker VP depends on (a mandatory sub-feature), not repeated per VP.

### VP-C007 KafkaConsumer
Consuming events from Kafka. Every rule of [VP-C006 KafkaProducer](#vp-c006-kafkaproducer) applies; the consumer group is the unit of parallelism, one partition per consumer at a time.

### VP-C008 RabbitMqProducer
Publishing messages to RabbitMQ. Every rule of [VP-C006 KafkaProducer](#vp-c006-kafkaproducer) applies; publisher confirms are on, so a publish counts as done only when the broker confirmed it.

### VP-C009 RabbitMqConsumer
Consuming messages from RabbitMQ. Every rule of [VP-C006 KafkaProducer](#vp-c006-kafkaproducer) applies; manual acknowledgement, and a dead-letter exchange receives messages the handler rejects as non-retryable.

### VP-C010 Outbox
Outbound calls — HTTP requests, broker publications — made by enqueuing a TaskBox task instead of calling directly; a generic handler per adapter then makes the call. Envelope, adapters, and ordering: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/contracts/vp-c010-outbox|contracts/vp-c010-outbox]].
- **Required for calls triggered by a data change** — a call or publication caused by a change of persisted data goes through Outbox, enqueued in the same transaction and the same store as that change, so it is sent exactly when the change is committed. A call not tied to a data change may still be made directly.
- **No storage of its own** — an outbox call is a VP-C003 task: stored, ordered by `queue_group`, retried, and dead-lettered by the TaskBox contract; its criticality follows the store it lives in.
- **At-least-once, recognisable duplicates** — the task `id` travels as `Idempotency-Key` (HTTP) or as the CloudEvents `id` (brokers), the same on every retry.
- **Order per receiver and key** — calls with the same target and key are sent in enqueue order; a dead call holds back the later calls with that target and key until a person resolves it.
- **Generic adapters: HTTP, Kafka, RabbitMQ.** A gRPC call, or any call whose response matters, is a service-specific handler that uses its generated client, handles the response, and may enqueue the next step — one step of an orchestrated saga.
- **Addresses from configuration** — a task names its target, never its address or credentials.

## Bound stack maps

Every map below carries every row of the table above in its own `## Common Variation Points` table. Plain paths, not links — this file belongs to a stack-agnostic skill.

- `skills/go/architecture/variability-map.md`
- `skills/dotnet/architecture/variability-map.md`
