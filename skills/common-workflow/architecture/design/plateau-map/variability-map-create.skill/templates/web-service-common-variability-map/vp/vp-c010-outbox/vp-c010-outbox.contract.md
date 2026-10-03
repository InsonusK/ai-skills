# Outbox envelope contract (VP-C010)

The stack-agnostic contract for outbound calls made through TaskBox, so an outbox task written by one stack is dispatched identically by any other. Outbox owns no storage: an outbound call is a TaskBox task stored, ordered, retried, and dead-lettered exactly as [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c003-taskbox/vp-c003-taskbox.contract|the TaskBox contract]] defines. This contract adds only the task types, their payload, and how each generic handler dispatches it. Concept: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c010-outbox/vp-c010-outbox|VP-C010 Outbox]].

## 1. Envelope

Every outbox task has `type = outbox.<adapter>`, `queue = outbox` (unless the service configures another), and this JSON payload:

```json
{
  "target":  { "...": "adapter-specific, §2" },
  "key":     "order-42",
  "headers": { "x-tenant": "acme" },
  "body":    { "...": "adapter-specific, §2" }
}
```

| Field | Meaning |
| --- | --- |
| `target` | Where the call goes — names only, never an address or a secret (§2). |
| `key` | Ordering key, nullable. With a key, calls to the same target with the same key run in enqueue order; without one, no order. |
| `headers` | Extra transport headers, passed through as-is. |
| `body` | What is sent. |

## 2. Adapters

| Task type | `target` | `body` | Receiver's duplicate check |
| --- | --- | --- | --- |
| `outbox.http` | `{dependency, method, path}` — `method` is `POST`, `PUT`, `PATCH`, or `DELETE` | JSON request body | `Idempotency-Key` header = task `id` |
| `outbox.kafka` | `{topic}` | a CloudEvent (VP-C006), stored as structured JSON; sent in Kafka binary mode | CloudEvents `id` = task `id` |
| `outbox.rabbitmq` | `{exchange, routing_key}` | a CloudEvent (VP-C006) | CloudEvents `id` = task `id`, also the AMQP `message_id` |

- **Addresses come from configuration at send time.** `dependency` names an outbound dependency (VP-C004); its base URL and credentials are read from the service's configuration when the task runs, so changing an address or rotating a secret also applies to tasks already queued.
- **The task `id` is the message identity.** The enqueuing code sets the CloudEvent `id` to the task `id`; every retry of the task sends the same identity.
- **No generic gRPC adapter.** A generic gRPC call would need dynamic invocation from descriptors; a gRPC call through Outbox is a service-specific task type whose handler uses the generated client (§5).

## 3. Enqueue

`outbox.enqueue(tx, adapter, target, key, body, headers)` is TaskBox's `enqueue(tx, type = "outbox.<adapter>", payload, {queue: "outbox", queue_group})` with

`queue_group = key == null ? null : "<adapter>:<target id>:<key>"`, where the target id is `dependency` (http), `topic` (kafka), or `exchange/routing_key` (rabbitmq).

So order holds per receiver and key — the same scope a Kafka partition key has.

## 4. Generic handlers

Each generic handler sends the call and returns its outcome as the HTTP status code TaskBox classifies:

| Adapter | Outcome |
| --- | --- |
| `outbox.http` | The response status; no response → `503` (connection) / `504` (deadline). The response body is discarded. |
| `outbox.kafka` | Acknowledged by the broker → `200`; broker unavailable → `503`; message rejected (too large, invalid) → `400`. |
| `outbox.rabbitmq` | Publisher-confirmed → `200`; broker unavailable or not confirmed → `503`; unroutable or rejected → `400`. |

A non-retryable outcome makes the task `dead` and stops its group until a person requeues or cancels it (TaskBox contract §3).

## 5. Custom handlers

A service may register its own task types next to the generic ones — for a gRPC call, or for any call whose **response** matters. Such a handler makes the call, handles the response, and, when there is follow-up work, enqueues it through TaskBox in the same transaction as its own data change. It returns an HTTP status code like any TaskBox handler. A saga spanning several services is not built from these handlers; it runs over the message brokers.

## 6. Conformance scenarios

Every stack realization passes these, per adapter it supports, on top of the TaskBox scenarios:
- A call enqueued in a transaction that rolls back is never sent; committed, it is sent once the handler succeeds.
- Every retry of a task carries the same `Idempotency-Key` / CloudEvents `id`.
- Calls with the same target and key are sent in enqueue order; a call with another key is not held back by them.
- A `4xx` outcome (other than `408`/`429`) makes the task `dead` at once and holds back later calls with the same target and key.
- A base URL changed in configuration applies to calls already queued.
