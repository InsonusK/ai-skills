# Inbox contract (VP-C011)

The stack-agnostic contract for **only-once** processing of inbound messages and calls: the input is validated, stored as a TaskBox task, and acknowledged at once; processing runs separately. Like Outbox, Inbox owns no storage — every inbox input is a task stored, ordered, retried, and dead-lettered by [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/contracts/vp-c003-taskbox|the TaskBox contract]]. Concept: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map#VP-C011 Inbox|VP-C011 Inbox]].

## 1. Two processing modes

| Mode | Flow | When the acknowledgement is lost |
| --- | --- | --- |
| **At-least-once** (default, no Inbox) | Process at once; reply / acknowledge after processing | The sender repeats, the input is processed again — the handler must be idempotent |
| **Only-once** (Inbox) | Validate → store as a task → reply `202` / acknowledge → process separately | The sender repeats, the duplicate is recognised by its key and not stored again |

The service chooses the mode per endpoint and per message type. Only-once means: delivery stays at-least-once, but an input is **processed at most once per deduplication key** within the key's window (the task's effective retention).

## 2. Deduplication key

The sender supplies the key; without it a repeat cannot be told from a new input.

| Input | Key | Missing key |
| --- | --- | --- |
| Broker message (VP-C007, VP-C009) | CloudEvents `source` + `/` + `id` | Not possible — every message is a CloudEvent |
| HTTP request | `Idempotency-Key` header | `400` |
| gRPC call | `idempotency-key` metadata — defined when inbound gRPC is admitted | — |

For an authenticated endpoint the key is scoped to the caller: `idempotency_key = <caller identity>:<key>`, so two callers choosing the same key never collide.

## 3. Flow

1. **Validate synchronously** — everything checkable without processing (schema, required fields, authorization). A failure is answered at once (`400`, `403`, …); only a valid input becomes a task.
2. **Enqueue** — TaskBox `enqueue` with `type` = the CloudEvents `type` (broker) or the operation name (HTTP/gRPC); `payload` = the CloudEvent or the request body plus the headers the handler needs; `idempotency_key` per §2; `queue_group` = `<broker>:<topic or queue>:<message key>` for a broker message, or the ordering key the service defines for a call (`null` = no order); `status_key` per §4 for calls.
3. **Acknowledge after the commit** — HTTP `202 Accepted` with `Location: <base>/tasks/<status_key>`; a broker message is acknowledged (offset committed) only after the task is committed.
4. **Process** — a TaskBox worker runs the handler registered for `type`; retries, dead-lettering, and group order are TaskBox's.

## 4. Repeats and status

- **Same key, same body** → no new task; the reply is the same `202` with the same `Location`. A repeated broker message is acknowledged and dropped.
- **Same key, different body** → `422` (the key is being reused for another request). Bodies are compared as normalized JSON against the stored `payload`.
- **Status** — `GET <base>/tasks/<status_key>` → `200` with `{status, last_status, created_at, finished_at}`; an unknown or already removed task → `404`.
- **`status_key` is a capability, not the task id.** It is a random UUIDv4 from a cryptographically secure generator (122 random bits), so knowing one's own key gives no way to reach another caller's task. The task `id` (UUIDv7, partly time-ordered, and already sent to Outbox receivers) is never exposed. Why a separate key: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/inbox-status-key-not-task-id|adr/inbox-status-key-not-task-id]].

## 5. Conformance scenarios

On top of the TaskBox scenarios, per input kind the service supports:
- A valid input is answered `202` (or acknowledged) only after its task is committed; an invalid one is rejected without a task.
- A repeat with the same key and body creates no second task and returns the same `Location`.
- A repeat with the same key and a different body is answered `422`.
- An HTTP request to an only-once endpoint without `Idempotency-Key` is answered `400`.
- Two callers using the same key on an authenticated endpoint get two tasks.
- `GET …/tasks/<status_key>` reports the task's status; a guessed or removed key returns `404`.
- A broker message whose processing fails is not redelivered by the broker — the task is retried by TaskBox.
