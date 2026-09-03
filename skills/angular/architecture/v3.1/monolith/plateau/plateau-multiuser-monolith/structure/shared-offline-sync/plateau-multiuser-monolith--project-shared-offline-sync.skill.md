---
name: plateau-multiuser-monolith--project-shared-offline-sync
description: The Dexie-backed offline mutation queue — MutationQueueService (per-feature partitions), ReplayOrchestrator (concurrent replay, server-wins conflict seam), and MutationReplayRegistry — multiuser-monolith plateau
domain: skill
type: template
plateau: multiuser-monolith
project_kind: library
version: 20260903150000
tags:
  - skill/template/project
  - plateau/multiuser-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]]"

> NEW at this plateau (VP5). Tagged `type:store`, `scope:shared` — it holds durable cross-cutting state (the queue) and a store-triggered orchestrator, sibling to `libs/shared/state`. The V1 `solution-offline-sync` Repository.extend tags it `type:util`; that cannot hold — the lib reads the `connectivity`/`notifications` slices and is imported by feature Facades. See the [example README](../../plateau-multiuser-monolith.skill/example/README.md).

# Goal

- Durable, reactive storage for queued mutations, partitioned by feature, with idempotent replay and a single server-wins conflict seam
- Never import a feature lib — features register their replay handler at runtime, so nothing forces feature code into the initial bundle

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create.md|OfflineSync/shared-offline-sync.project.create]]

# Structure

## Project Structure

```
/libs/shared/offline-sync
  /src
    /lib
      mutation-queue.db.ts        <- Dexie DB: queuedMutations table, indexed by feature + enqueuedAt
      mutation-queue.service.ts   <- enqueue / pendingForFeature$ / pendingForFeatureOnce / listFeatures / markSynced
      replay-orchestrator.ts      <- ReplayOrchestrator + MutationReplayRegistry + ReplayConflictError + FeatureReplay
      provide-offline-sync.ts     <- provideOfflineSync() (shell) + provideFeatureReplay(factory) (feature route)
      *.spec.ts
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| mutation-queue.db.ts / mutation-queue.service.ts | Dexie schema + the queue's public API. `enqueue` generates the idempotency key; `pendingForFeature$` is a `liveQuery` observable for the indicator. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/shared-offline-sync/classes/plateau-multiuser-monolith--class-mutation-queue-service.skill.md\|class-mutation-queue-service]] |
| replay-orchestrator.ts | `ReplayOrchestrator` (connectivity-triggered, concurrent partitions, `handleConflict` seam), `MutationReplayRegistry`, `ReplayConflictError`. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/shared-offline-sync/classes/plateau-multiuser-monolith--class-replay-orchestrator.skill.md\|class-replay-orchestrator]] |
| provide-offline-sync.ts | `provideOfflineSync()` — shell registers + eagerly instantiates the orchestrator. `provideFeatureReplay(factory)` — a feature's route `providers` registers its handler. | — |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create.md|OfflineSync/shared-offline-sync.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create.md|OfflineSync/replay-orchestrator.ts.create]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| dexie | latest compatible | Typed, reactive IndexedDB storage for the queue |
| fake-indexeddb | latest compatible (dev) | IndexedDB shim so the queue specs run under Vitest/jsdom |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create.md|OfflineSync/shared-offline-sync.project.create]]

## Allowed Dependencies

- `libs/shared/state` (tag: `type:store`, `scope:shared`) — reads `selectIsOnline`, dispatches `NotificationsActions`
- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/Repository.extend.md|Repository.extend]]

## What Does NOT Belong Here

- Any `import` from a `type:feature` or `type:data-access` project — features register replay handlers at runtime via `MutationReplayRegistry`
- Per-operation or per-field conflict logic — server-wins is the only strategy here; a future solution overrides `handleConflict`
- A generic document-sync protocol — replay calls the app's own Facade methods

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create.md|OfflineSync/replay-orchestrator.ts.create]]

# Rules

## MUST
- The queue must be partitioned by `feature` and indexed on `feature` + `enqueuedAt`; reads and replay are FIFO within a partition.
- Every entry carries a stable `idempotencyKey` (generated once at enqueue) and its `touchedFields` (derived from the command payload, never a separate snapshot).
- `ReplayOrchestrator` must replay partitions concurrently (`Promise.all`); a stuck partition stops on its first failure and never blocks another.
- `handleConflict` must be one separately-named method — never inlined — and must dispatch a `notifications` entry carrying only the touched fields' current server values.
- This project must never `import` a `type:feature` or `type:data-access` project; feature replay handlers come through `MutationReplayRegistry` at runtime.
- `provideFeatureReplay` must be placed in a feature's route `providers` (not the shell) — the shell only calls `provideOfflineSync()`.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create.md|OfflineSync/shared-offline-sync.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create.md|OfflineSync/replay-orchestrator.ts.create]]

# Check list

- [ ] `queuedMutations` is indexed by `feature` and `enqueuedAt`
- [ ] Every entry has a stable `idempotencyKey` and its `touchedFields`
- [ ] Partitions replay concurrently; a stuck partition does not delay others
- [ ] `handleConflict` is a single overridable method, not inlined
- [ ] No import of a `type:feature`/`type:data-access` project
- [ ] The queue survives a full page reload (Dexie persistence)

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create.md|OfflineSync/shared-offline-sync.project.create]]

# Unittest TestCases

- [ ] WHEN a mutation is enqueued THEN it persists across a simulated page reload (a new Dexie connection reads the same data)
- [ ] WHEN two features both have pending mutations and one repeatedly fails THEN the other feature's partition still completes
- [ ] WHEN a replayed entry receives a conflict THEN the local change is discarded and a field-scoped notification is dispatched
- [ ] WHEN `isOnline` transitions `false → true` THEN replay is triggered automatically

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create.md|OfflineSync/replay-orchestrator.ts.create]]
