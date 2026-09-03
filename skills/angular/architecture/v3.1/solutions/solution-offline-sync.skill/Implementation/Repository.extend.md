---
description: Add libs/shared/offline-sync (Dexie-backed mutation queue) and extend every feature's Facade to catch OfflineTransportError and enqueue instead of failing outright
element_kind: repository
change_kind: extend
tags:
  - solution/offline-sync
  - element/monolith-repository
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
- Every queued mutation carries a client-generated idempotency key, sent with the replayed request.
  - Risk: a retry of a request that actually succeeded (its response was lost) applies the mutation twice.
  - Fix: `crypto.randomUUID()` at enqueue, reused on every replay; the backend dedupes on it.
- Only mutations a Facade explicitly marks as queueable are enqueued.
  - Risk: auto-queueing every `OfflineTransportError` queues one-time or time-sensitive actions that produce a wrong result when replayed much later.
  - Fix: the Facade decides per operation — some `OfflineTransportError`s surface as an immediate failure with no "pending" state.
- Every queued mutation records the feature (`scope`) that created it, for partitioning.
  - Risk: without a partition key one struggling feature's stuck entry blocks every other feature's replay.
  - Fix: `enqueue({ feature: 'orders', ... })`; the queue is read/replayed per `feature` index; per [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/queue-partitioning-and-ordering.md|queue-partitioning-and-ordering]].
- A mutation endpoint's 409 response includes the current values of the fields the original request tried to change.
  - Risk: without them the client cannot tell the user *what* conflicted, and server-wins resolution is a silent data loss.
  - Fix: this is a required backend contract; the Client maps it to `ReplayConflictError(currentServerValues)`; per [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/conflict-resolution-strategy.md|conflict-resolution-strategy]].

# Unittest TestCases

- [ ] WHEN a queueable mutation fails with `OfflineTransportError` THEN
  - [ ] the Facade enqueues it via `MutationQueueService` instead of rejecting immediately
- [ ] WHEN the same queued mutation is replayed twice due to a lost response THEN
  - [ ] both attempts carry the same idempotency key

## SHOULD
- **Enqueueing every `OfflineTransportError` unconditionally, regardless of whether the operation makes sense as a pending action** — Consequence: some operations (e.g. one-time, time-sensitive actions) queued and replayed much later could produce a confusing or wrong result — Instead: each Facade explicitly decides which of its operations are queueable
- **Reusing the same idempotency key across a retried command's multiple replay attempts without regenerating it per logical mutation** — Consequence: without a stable key, the backend cannot tell "this is the same command retried" from "this is a new, different command," defeating the idempotency guarantee — Instead: generate the idempotency key once, at enqueue time, and reuse that same key for every replay attempt of that specific queued mutation
