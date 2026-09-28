# VP-C010 Outbox

Concept of a common Variation Point; its question, Variants, Constraint, and Realization depends on are the VP-C010 row of [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]].

Outbound calls — HTTP requests, broker publications — made by enqueuing a TaskBox task instead of calling directly; a generic handler per adapter then makes the call. Envelope, adapters, and ordering: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c010-outbox/vp-c010-outbox.contract|vp-c010-outbox.contract]].
- **Required for calls triggered by a data change** — a call or publication caused by a change of persisted data goes through Outbox, enqueued in the same transaction and the same store as that change, so it is sent exactly when the change is committed. A call not tied to a data change may still be made directly.
- **No storage of its own** — an outbox call is a VP-C003 task: stored, ordered by `queue_group`, retried, and dead-lettered by the TaskBox contract; its criticality follows the store it lives in.
- **At-least-once, recognisable duplicates** — the task `id` travels as `Idempotency-Key` (HTTP) or as the CloudEvents `id` (brokers), the same on every retry.
- **Order per receiver and key** — calls with the same target and key are sent in enqueue order; a dead call holds back the later calls with that target and key until a person resolves it.
- **Generic adapters: HTTP, Kafka, RabbitMQ.** A gRPC call, or any call whose response matters, is a service-specific handler that uses its generated client, handles the response, and may enqueue a follow-up task. Multi-service sagas are not modelled here; they run over the message brokers.
- **Addresses from configuration** — a task names its target, never its address or credentials.
