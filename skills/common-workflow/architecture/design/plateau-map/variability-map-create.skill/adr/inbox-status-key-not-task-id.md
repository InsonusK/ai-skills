---
name: inbox status by a separate status_key, not by the task id
description: Which identifier a caller uses to read the status of an only-once (Inbox) task answered with 202
problem: After an only-once input is answered `202`, the caller needs a way to read its task's status that no other caller can use to reach someone else's task — should that be the task `id` or a separate key?
decision: A separate `status_key` — a UUIDv4 from a cryptographically secure generator, set only on Inbox tasks answered with `202`, returned in `Location`, never derived from the task `id`; the task `id` stays internal identity and is never a capability.
tags:
  - concern/architecture
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The Inbox contract (VP-C011) answers an only-once input with `202 Accepted` and offers `GET …/tasks/<handle>` to read the task's status. The owner's requirement: a caller that knows its own handle must not be able to reach a neighbour's task. The task already has an `id` (UUIDv7) — the question is whether that `id` can be the handle, or whether the handle must be a separate field.

# Selected variant
[[#Separate random status_key (selected)]]

# Searched variants

## Separate random status_key (selected)

### Description
A `status_key` column (UUIDv4, nullable, unique) in the TaskBox task, generated from a cryptographically secure random source, set only for Inbox tasks answered with `202`. The `202` carries `Location: …/tasks/<status_key>`; the status endpoint looks tasks up by it and nothing else. The task `id` is never exposed as a way to read status.

### Benefits
- **The handle is secret by construction.** The task `id` is not: the Outbox contract sends it to receivers as `Idempotency-Key` and as the CloudEvents `id`, and it is written to logs and traces. Only the caller that received the `202` ever sees the `status_key`.
- **Unguessability does not rest on UUIDv7.** A UUIDv7 starts with a 48-bit millisecond timestamp, so ids of tasks created close together share it, and some generators (Go `google/uuid` among them) spend further bits on a monotonic counter. RFC 9562 advises against assuming UUIDs are hard to guess or using them as security capabilities. A CSPRNG UUIDv4 has 122 random bits.
- **The same guarantee in every stack.** The contract states the source (CSPRNG UUIDv4) explicitly, so the strength does not depend on how each language's UUIDv7 generator fills its bits.
- **Separate roles stay separate.** `id` is internal identity (index order, logs, receiver-side deduplication); `status_key` is an external access handle that exists only for Inbox tasks and can be withheld or replaced later without touching identity.

### Costs
- One nullable column with a unique index in the TaskBox schema.
- The status endpoint needs its own lookup by `status_key`.

## Task id as the handle

### Description
Return `Location: …/tasks/<id>` and look tasks up by their `id`.

### Benefits
- No extra column or index.
- One identifier for everything.

### Costs
- The `id` is already disclosed to third parties (Outbox receivers) and to logs, so anyone who sees it could read the status.
- Its unguessability would rest on UUIDv7 properties the RFC disclaims and that vary by generator — timestamp-prefixed, sometimes counter-filled.

## Task id plus a caller-identity check

### Description
Look tasks up by `id`, store the enqueuing caller's identity on the task, and return the status only to that same caller.

### Benefits
- No secret handle to protect; access follows authentication.
- No extra key column (but an extra caller-identity column).

### Costs
- Works only on authenticated endpoints; anonymous inputs (e.g. webhooks) have no identity to check.
- Couples TaskBox to the service's authentication model.
