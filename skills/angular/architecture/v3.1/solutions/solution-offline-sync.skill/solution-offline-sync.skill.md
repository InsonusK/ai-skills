---
name: solution-offline-sync
description: Dexie-backed, per-feature-partitioned mutation queue with idempotent replay, built on top of Offline-first's OfflineTransportError hook and connectivity signal — server-wins conflict resolution with field-scoped diffing, and a designed extension seam for future per-operation conflict logic
domain: skill
type: architecture
version: 20260902000000
tags:
  - skill/architecture/solution
  - stack/typescript
  - offline
  - sync
  - dexie
  - framework/angular
  - concern/architecture
  - solution/offline-sync

whenToUse: when deciding whether a mutation should be queueable when offline, reviewing how a replay conflict is surfaced, or adding a feature's mutations to the offline queue
creates:
  - libs/shared/offline-sync
extends:
  - libs/{feature}/data-access (Facade queueing)
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]]"
adr:
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/queue-storage-mechanism.md|Queue Storage Mechanism ADR]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/queue-partitioning-and-ordering.md|Queue Partitioning And Ordering ADR]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/conflict-resolution-strategy.md|Conflict Resolution Strategy ADR]]"
---

# Goal

- Let mutations attempted while offline (the scenario the "Offline-first" solution deliberately left as an immediate failure) be queued and automatically replayed once connectivity returns
- Give the user visibility into pending, unsynced actions, and clear information when a queued action conflicts with a server-side change
- Avoid one struggling feature's replay blocking every other feature's sync
- Keep the queue's replay logic aligned with this application's actual command-oriented backend, not a generic document-sync model

# Capabilities

- Mutations attempted offline are queued instead of lost or immediately failed, for operations a Facade explicitly opts in
- Reactive "pending sync" UI, updating automatically as the queue changes
- A struggling feature (e.g. one dependent on a flaky external service) only stalls its own queue partition, not the whole application's sync
- Idempotent replay — a retried command that actually succeeded server-side but lost its response is not double-applied
- Server-wins conflict resolution with field-scoped detail, telling the user specifically what didn't apply and why — without the cost of storing full entity snapshots client-side

# Core Principles

- Dexie.js is a storage/reactivity layer only — the queue's replay logic calls this application's own Facade/Client methods directly, never a generic document-replication protocol
- The queue is partitioned by feature; FIFO ordering is preserved within a partition, and partitions replay independently and in parallel
- Every queued mutation carries a stable idempotency key, generated once at enqueue time and reused across every replay attempt
- Only operations a Facade explicitly marks as queueable get enqueued on `OfflineTransportError` — queueing is never implicit
- Conflict resolution defaults to server-wins, compared only against the fields the queued command itself intended to change — no full entity snapshot is ever stored
- The conflict-handling step is a single, clearly separated seam in the replay orchestrator, reserved for a future solution to plug in smarter, per-operation resolution logic

# Adr

- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/queue-storage-mechanism.md|Dexie.js + custom orchestration instead of RxDB's document-replication engine or raw IndexedDB]]
  - Selected variant: Dexie.js — chosen because this application needs a typed, reactive local store with custom command-replay logic, not a generic document-sync protocol
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/queue-partitioning-and-ordering.md|Partition by feature instead of global FIFO or per-entity partitioning]]
  - Selected variant: per-feature partitioning — chosen to directly prevent one struggling feature from blocking others, while staying meaningfully simple to reason about
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/conflict-resolution-strategy.md|Server wins with field-scoped diff, extension point deferred, instead of full-snapshot diffing, client-wins, or mandatory manual resolution]]
  - Selected variant: server-wins + field-scoped diff — chosen to give the user real information about what conflicted without requiring full entity snapshots client-side

# Requirements

SOLUTION:
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]
  - `OfflineTransportError` (Client-level network-failure distinction) and the `connectivity` slice's `isOnline` signal are both consumed directly by this solution
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]
  - The replay orchestrator calls existing Facade methods directly; no new HTTP transport path is introduced
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]]
  - Conflict notifications are surfaced via the `notifications` global-state slice, alongside the existing `auth`/`connectivity`

NPM:
- dexie
  - Typed, reactive IndexedDB storage for the mutation queue, per [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/queue-storage-mechanism.md|Queue Storage Mechanism ADR]]

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository]] - extend - add `libs/shared/offline-sync`, idempotency-key requirement, Facade-queueing convention

PROJECT:
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create|libs/shared/offline-sync]] - create - Dexie schema, `MutationQueueService`, per-feature partitioning

Artifact-level:
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create|replay-orchestrator.ts]] - create - replays each feature's queue partition, triggered by `connectivity`, server-wins conflict seam
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|{feature}.facade.ts (extend)]] - extend - catches `OfflineTransportError` and enqueues queueable operations, generic pattern applied to any feature
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create|pending-sync-indicator.component.ts]] - create - shared "pending sync" indicator, mounted per feature

# Workflow

## Mutation attempted offline, later synced (happy path)

1. A user attempts to add an order while offline; `OrdersClient` throws `OfflineTransportError` (per the "Offline-first" solution).
2. `OrdersFacade` catches it, confirms `addOrder` is a queueable operation, and enqueues it via `MutationQueueService` with a freshly generated idempotency key — returning `{ queued: true }` instead of throwing.
3. `PendingSyncIndicatorComponent`, reading `pendingForFeature$('orders')`, shows "1 action waiting to sync."
4. Connectivity is restored; the `connectivity` slice's `isOnline` becomes `true`.
5. `ReplayOrchestrator` replays the `orders` partition FIFO; `addOrder` succeeds; the entry is removed from the queue; the indicator updates to 0.

![Mutation attempted offline, later synced (happy path)](./diagrams/mutation-attempted-offline-later-synced-happy-path.mmd)

## Conflict during replay (failure path)

1. While the client was offline, another user changed the same order's `priority` on the server.
2. On replay, the backend returns a 409 with the current value of `priority` (the only field this queued mutation touched).
3. `ReplayOrchestrator.handleConflict` discards the local change (server wins), removes the entry from the queue, and dispatches a notification: "Your change to priority in orders wasn't applied — it was changed elsewhere," including the current server value.
4. The user sees this via the `notifications` slice's existing UI, without the application ever comparing full entity snapshots.

## One feature's queue is stuck, others proceed (isolation, per partitioning ADR)

1. A feature depending on a flaky geolocation service repeatedly fails to replay its queued mutation.
2. `replayPartition` for that feature stops on the first failure of that cycle; other features' partitions, running concurrently, are unaffected and complete their own replay normally.
3. The stuck feature's partition retries on the next connectivity-restoration event, exactly like the failed one, without blocking anything else in the meantime.

# Rules

## MUST
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/Repository.extend#MUST|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create#MUST|OfflineSync/shared-offline-sync.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create#MUST|OfflineSync/replay-orchestrator.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend#MUST|DataAccess/{feature}.facade.ts.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create#MUST|UI/pending-sync-indicator.component.ts.create]]

## SHOULD
- Avoid — [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/Repository.extend|See Repository.extend.md]] — enqueueing every `OfflineTransportError` unconditionally; reusing idempotency keys incorrectly.
- Avoid — [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create|See shared-offline-sync.project.create.md]] — querying the queue table without using the feature index.
- Avoid — [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create|See replay-orchestrator.ts.create.md]] — inlining conflict-handling logic directly inside the replay loop instead of the separate `handleConflict` method.
- Avoid — [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|See {feature}.facade.ts.extend.md]] — enqueueing an operation whose business validation already failed before the Client was ever called.
- Avoid — [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create|See pending-sync-indicator.component.ts.create.md]] — a feature queueing mutations without ever showing a pending indicator.

# Check list

- [ ] Every queued mutation carries a stable idempotency key, generated once at enqueue time and reused across replay attempts
- [ ] Feature partitions replay concurrently; a stuck partition does not delay others
- [ ] Conflict handling is server-wins by default, surfaced with field-scoped detail, never a full entity snapshot
- [ ] Backend mutation endpoints support idempotency keys and return field-scoped conflict detail on a 409
- [ ] Every feature that enqueues mutations surfaces the pending-sync indicator somewhere in its UI
- [ ] No Facade enqueues an operation whose business validation already failed before the Client was ever called