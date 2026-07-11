---
description: Dexie-backed mutation queue — schema, reactive queries for pending-sync UI, and per-feature partitioning
name: shared-offline-sync
project_kind: library
element_kind: project
change_kind: create
---

# Goals

- Durable, reactive storage for queued mutations, partitioned by feature per [[skills/angular/architecture/solutions/solution-offline-sync.skill/adr/queue-partitioning-and-ordering]]

# Structure

## Project Structure

```
/libs/shared/offline-sync
  /src
    /lib
      mutation-queue.db.ts
      mutation-queue.service.ts
      replay-orchestrator.ts
    index.ts
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| mutation-queue.db.ts | Dexie database definition: a `queuedMutations` table, indexed by `feature` (partition key) and `enqueuedAt` (for FIFO ordering within a partition). |
| mutation-queue.service.ts | Public API: `enqueue()`, `pendingForFeature$()` (a `liveQuery`-backed observable for reactive UI), `markSynced()`, `markConflict()`. |
| replay-orchestrator.ts | Triggered by the `connectivity` slice's `isOnline` transitioning to `true` (from the "Offline-first" solution); replays each feature's partition independently and in parallel, FIFO within a partition, per [[skills/angular/architecture/solutions/solution-offline-sync.skill/adr/queue-partitioning-and-ordering]]. |

# Implementation changes

```typescript
// mutation-queue.db.ts
export interface QueuedMutation {
  id?: number;
  feature: string;             // partition key
  idempotencyKey: string;      // generated once at enqueue time
  operationName: string;       // e.g. "addOrder", "setComplete"
  payload: unknown;            // arguments for the Facade method
  touchedFields: string[];     // fields this mutation intends to change — used for conflict diffing
  enqueuedAt: number;
}

export class MutationQueueDb extends Dexie {
  queuedMutations!: Table<QueuedMutation, number>;
  constructor() {
    super('offline-sync');
    this.version(1).stores({
      queuedMutations: '++id, feature, enqueuedAt',
    });
  }
}
```

```typescript
// mutation-queue.service.ts
@Injectable({ providedIn: 'root' })
export class MutationQueueService {
  private readonly db = new MutationQueueDb();

  async enqueue(entry: Omit<QueuedMutation, 'id' | 'enqueuedAt' | 'idempotencyKey'>): Promise<void> {
    await this.db.queuedMutations.add({
      ...entry,
      idempotencyKey: crypto.randomUUID(),
      enqueuedAt: Date.now(),
    });
  }

  pendingForFeature$(feature: string) {
    return liveQuery(() =>
      this.db.queuedMutations.where('feature').equals(feature).sortBy('enqueuedAt'),
    );
  }

  async markSynced(id: number): Promise<void> {
    await this.db.queuedMutations.delete(id);
  }
}
```

# Rule changes

## MUST
- `queuedMutations` MUST be indexed by `feature` and `enqueuedAt` to support efficient per-partition FIFO reads.
- `touchedFields` MUST be stored per queued mutation at enqueue time — it is derived directly from the command's own payload, never a separately captured entity snapshot, per [[skills/angular/architecture/solutions/solution-offline-sync.skill/adr/conflict-resolution-strategy]].
- `idempotencyKey` MUST be generated exactly once, at enqueue time, and reused unchanged across every replay attempt of that entry.

# Anti-patterns

- **Querying the whole `queuedMutations` table and filtering by feature in application code instead of using the `feature` index**
  - Consequence: unnecessary full-table scans as the queue grows
  - Instead: always query through the `feature` index, as `pendingForFeature$` does

# Check list

- [ ] The queue survives a full page reload (Dexie/IndexedDB persistence)
- [ ] `pendingForFeature$` reactively updates the UI as entries are added/removed
- [ ] Every entry carries a stable `idempotencyKey` and its `touchedFields`

# Unittest TestCases

- [ ] WHEN a mutation is enqueued THEN
  - [ ] it persists across a simulated page reload (new Dexie connection reads the same data)
- [ ] WHEN `markSynced` is called for an entry THEN
  - [ ] it is removed from the queue and `pendingForFeature$` reflects the removal
