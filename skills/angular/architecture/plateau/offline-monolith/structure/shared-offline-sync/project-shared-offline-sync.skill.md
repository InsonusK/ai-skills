---
name: project-shared-offline-sync
description: Dexie-backed mutation queue — durable, per-feature-partitioned storage for mutations attempted offline, plus the replay orchestrator that syncs them once connectivity is restored
domain: skill
type: template
plateau: offline-monolith
project_kind: library
version: 20260711200000
tags:
  - skill/template/project
  - plateau/offline-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]]"
---

# Goal

- Durable, reactive storage for queued mutations, partitioned by feature
- Replay each feature's queue independently once connectivity is restored, with a server-wins conflict default and a single, overridable extension seam

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create|OfflineSync/shared-offline-sync.project.create]]

# Core Principles

- Every queued mutation carries a client-generated idempotency key, generated once at enqueue time and reused unchanged across every replay attempt
- Only mutations a Facade explicitly marks as queueable are ever enqueued
- Replay is concurrent across feature partitions, strictly FIFO within a partition
- Conflict handling is a single, separately named seam (`handleConflict`), not inlined into the replay loop

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create|OfflineSync/replay-orchestrator.ts.create]]

# Structure

## Project Structure

```
/libs/shared/offline-sync
  /src
    /lib
      [mutation-queue.db.ts, mutation-queue.service.ts](./classes/class-mutation-queue-service.skill.md)
      [replay-orchestrator.ts](./classes/class-replay-orchestrator.skill.md)
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| mutation-queue.db.ts / mutation-queue.service.ts | Dexie schema and public API (`enqueue`, `pendingForFeature$`, `markSynced`) for the durable, per-feature-partitioned queue | [[classes/class-mutation-queue-service.skill.md\|class-mutation-queue-service.skill]] |
| replay-orchestrator.ts | Triggered by `connectivity`'s `isOnline` transitioning to `true`; replays each feature partition independently and in parallel, FIFO within a partition | [[classes/class-replay-orchestrator.skill.md\|class-replay-orchestrator.skill]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create|OfflineSync/shared-offline-sync.project.create]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| dexie | latest stable | IndexedDB wrapper: schema, `liveQuery` reactive reads |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create|OfflineSync/shared-offline-sync.project.create]]

## What Does NOT Belong Here

- Business validation — that stays in each feature's own Facade, before a mutation is ever enqueued
- Per-operation custom conflict-resolution logic beyond the server-wins default — explicitly deferred to a future solution

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create|OfflineSync/replay-orchestrator.ts.create]]

## Allowed Dependencies

- `libs/shared/state` (`connectivity` slice, tag: `type:store`, `scope:shared`)
- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create|OfflineSync/replay-orchestrator.ts.create]]

# Rules

## MUST
- Every queued mutation MUST carry a client-generated idempotency key, sent with every replay attempt.
- Only mutations a Facade explicitly marks as queueable MAY be enqueued.
- Every queued mutation MUST be associated with the feature that created it, for partitioning.
- Replay MUST process all feature partitions concurrently, and entries within one partition strictly FIFO.
- `handleConflict` MUST be a single, separately named method — not inlined into `replayPartition`'s loop.

## MUST NOT
- This project MUST NOT implement per-operation or per-field custom conflict logic beyond server-wins.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create|OfflineSync/replay-orchestrator.ts.create]]

# Anti-patterns

- **Enqueueing every `OfflineTransportError` unconditionally**
  - Consequence: some operations queued and replayed much later could produce a confusing or wrong result
  - Instead: each Facade explicitly decides which of its operations are queueable
- **Inlining conflict-handling logic directly inside `replayPartition`'s loop**
  - Consequence: a future extension solution would need to modify the core replay loop itself
  - Instead: keep `handleConflict` as the single point of variation

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create|OfflineSync/replay-orchestrator.ts.create]]

# Check list

- [ ] The queue survives a full page reload (Dexie/IndexedDB persistence)
- [ ] `pendingForFeature$` reactively updates the UI as entries are added/removed
- [ ] Feature partitions replay concurrently; a stuck partition does not delay others

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create|OfflineSync/shared-offline-sync.project.create]]
