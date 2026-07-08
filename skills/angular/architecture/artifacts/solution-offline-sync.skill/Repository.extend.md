---
description: Add libs/shared/offline-sync (Dexie-backed mutation queue) and extend every feature's Facade to catch OfflineTransportError and enqueue instead of failing outright
element_kind: repository
change_kind: extend
---

# Structure

## Workspace Structure

```
/libs
  /shared
    /ui
    /util
    /state
    /http-core
    /logging
    /offline-sync      <- new
```

## Directory and project skills

| Directory | Description |
| ---------- | ----------- |
| /libs/shared/offline-sync | Dexie-backed mutation queue (`MutationQueueService`), the replay orchestrator, and the conflict-handling seam. Tagged `type:util`, `scope:shared`. |
| /libs/{feature}/data-access/src/lib/{feature}.facade.ts | Extended: catches `OfflineTransportError` thrown by the Client (per the "Offline-first" solution) and, for queueable operations, enqueues via `MutationQueueService` instead of letting the error propagate as an outright failure. |

# Rules

## MUST
- Every queued mutation MUST carry a client-generated idempotency key, sent with the replayed request, so a retry that succeeded server-side but whose response was lost does not get applied twice.
- Only mutations a Facade explicitly marks as queueable MAY be enqueued — not every `OfflineTransportError` results in queueing; a Facade MAY still choose to surface certain operations as an immediate failure (e.g. operations with no sensible "pending" state).
- Every queued mutation MUST be associated with the feature (`scope` tag) that created it, for partitioning per [[../adr/queue-partitioning-and-ordering.md]].
- A mutation endpoint's conflict (409) response MUST include the current values of the fields the original request attempted to change, per [[../adr/conflict-resolution-strategy.md]] — this is a required backend contract, not optional.

# Anti-patterns

- **Enqueueing every `OfflineTransportError` unconditionally, regardless of whether the operation makes sense as a pending action**
  - Consequence: some operations (e.g. one-time, time-sensitive actions) queued and replayed much later could produce a confusing or wrong result
  - Instead: each Facade explicitly decides which of its operations are queueable

- **Reusing the same idempotency key across a retried command's multiple replay attempts without regenerating it per logical mutation**
  - Consequence: without a stable key, the backend cannot tell "this is the same command retried" from "this is a new, different command," defeating the idempotency guarantee
  - Instead: generate the idempotency key once, at enqueue time, and reuse that same key for every replay attempt of that specific queued mutation

# Unittest TestCases

- [ ] WHEN a queueable mutation fails with `OfflineTransportError` THEN
  - [ ] the Facade enqueues it via `MutationQueueService` instead of rejecting immediately
- [ ] WHEN the same queued mutation is replayed twice due to a lost response THEN
  - [ ] both attempts carry the same idempotency key
