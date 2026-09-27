# VP-C011 Inbox

Concept of a common Variation Point; its question, Variants, Constraint, and Realization depends on are the VP-C011 row of [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]].

Only-once processing of inbound broker messages and calls. Every input is processed in one of two modes, chosen by the service per endpoint and per message type:
- **At-least-once** (default, no Inbox) — process at once, reply or acknowledge after processing; a lost acknowledgement means a repeat, so the handler is idempotent (the consumer baseline of VP-C006).
- **Only-once** (this VP) — validate synchronously, store the input as a VP-C003 task, answer `202` / acknowledge, process separately. A repeat carries the same deduplication key and is not stored again, so the input is processed at most once per key within the task's retention.
- **The sender supplies the key** — CloudEvents `source` + `id`, the HTTP `Idempotency-Key` header, gRPC metadata; scoped to the caller on authenticated endpoints. Same key with a different body → `422`.
- **Status by capability** — a `202` carries `Location: …/tasks/<status_key>`; `status_key` is a random UUIDv4, never the task id, so one caller cannot reach another caller's task.
- HTTP inbound is baseline, so Inbox depends on no inbound VP; inbound gRPC is added when it is admitted.
- Contract: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c011-inbox/vp-c011-inbox.contract|vp-c011-inbox.contract]].
