---
name: plateau-offline-full-monolith--class-mutation-queue-service
description: The Dexie-backed mutation queue service — enqueue (with idempotency key), per-feature FIFO reads, a liveQuery observable for pending-sync UI, and markSynced — offline-full-monolith plateau
domain: skill
type: template
plateau: offline-full-monolith
artifact_type: service
version: 20260903120000
tags:
  - skill/template/class
  - plateau/offline-full-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]]"

> `libs/shared/offline-sync/src/lib/mutation-queue.{db,service}.ts`. Dexie is a storage/reactivity layer only.

# Goal

- Give the queue one durable, reactive home — feature Facades `enqueue()`, the orchestrator drains it, the indicator reads `pendingForFeature$()`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create.md|OfflineSync/shared-offline-sync.project.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- `queuedMutations` is indexed by `feature` (partition key) and `enqueuedAt` (FIFO within a partition) — never full-table-scan and filter in code
- `idempotencyKey` is generated exactly once, at enqueue time, and stored with the entry
- `touchedFields` is derived from the command payload at enqueue time — never a separately captured entity snapshot

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create.md|OfflineSync/shared-offline-sync.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/queue-storage-mechanism.md|Queue Storage Mechanism ADR]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Dexie DB | `{Name}Db` | `MutationQueueDb` | `{name}.db.ts` | `mutation-queue.db.ts` |
| Queue service | `{Name}Service` | `MutationQueueService` | `{name}.service.ts` | `mutation-queue.service.ts` |
| Queued row | — | `QueuedMutation` / `PersistedMutation` | — | — |

# Implementation

```typescript
// Skill: class-mutation-queue-service
// Plateau: offline-full-monolith
// Version: 20260903120000

// mutation-queue.db.ts
export interface QueuedMutation {
  id?: number;
  feature: string;            // partition key
  idempotencyKey: string;     // once, at enqueue
  operationName: string;      // e.g. "addOrder"
  payload: unknown;
  touchedFields: string[];    // from the payload — for conflict diffing
  enqueuedAt: number;
}
export class MutationQueueDb extends Dexie {
  queuedMutations!: Table<QueuedMutation, number>;
  constructor(name = 'offline-sync') {
    super(name);
    this.version(1).stores({ queuedMutations: '++id, feature, enqueuedAt' });
  }
}

// mutation-queue.service.ts
@Injectable({ providedIn: 'root' })
export class MutationQueueService {
  private readonly db = new MutationQueueDb();
  async enqueue(e: NewMutation): Promise<PersistedMutation> {
    const row = { ...e, idempotencyKey: crypto.randomUUID(), enqueuedAt: Date.now() };
    return { ...row, id: await this.db.queuedMutations.add(row) };
  }
  pendingForFeature$(feature: string) {  // liveQuery-backed, for the indicator
    return liveQuery(() => this.db.queuedMutations.where('feature').equals(feature).sortBy('enqueuedAt'));
  }
  pendingForFeatureOnce(feature: string) { /* one-shot FIFO read for the orchestrator */ }
  listFeatures() { return this.db.queuedMutations.orderBy('feature').uniqueKeys(); }
  markSynced(id: number) { return this.db.queuedMutations.delete(id); }
}
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create.md|OfflineSync/shared-offline-sync.project.create]]

# Rules

## MUST
- `queuedMutations` must be indexed by `feature` and `enqueuedAt`; every read goes through the `feature` index.
- `idempotencyKey` must be generated once, at enqueue, and reused unchanged across every replay attempt.
- `touchedFields` must come from the command payload, never a separate entity snapshot.
- Never apply several plateau templates per class/artifact.
- Never expose a way to read the Dexie table other than `pendingForFeature$` / `pendingForFeatureOnce`.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create.md|OfflineSync/shared-offline-sync.project.create]]

# Check list

- [ ] `++id, feature, enqueuedAt` schema; reads use the `feature` index
- [ ] `enqueue` sets `idempotencyKey` + `enqueuedAt`; returns the persisted row
- [ ] `pendingForFeature$` is `liveQuery`-backed
- [ ] The queue survives a page reload (a new connection reads the same data)

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create.md|OfflineSync/shared-offline-sync.project.create]]

# Unittest TestCases

- [ ] WHEN a mutation is enqueued THEN it persists across a simulated page reload
- [ ] WHEN entries exist for two features THEN each `pendingForFeatureOnce` returns only that partition, FIFO
- [ ] WHEN `markSynced(id)` is called THEN the entry is removed and `pendingForFeature$` reflects it

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create.md|OfflineSync/shared-offline-sync.project.create]]
