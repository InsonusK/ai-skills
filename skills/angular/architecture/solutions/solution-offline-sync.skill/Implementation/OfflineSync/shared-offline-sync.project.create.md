---
description: Dexie-backed mutation queue — schema, reactive queries for pending-sync UI, and per-feature partitioning
name: shared-offline-sync
project_kind: library
element_kind: project
change_kind: create
tags:
  - solution/offline-sync
  - element/shared-offline-sync-project
---

# Goals

- Durable, reactive storage for queued mutations, partitioned by feature per [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/queue-partitioning-and-ordering.md|queue-partitioning-and-ordering]]

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
| replay-orchestrator.ts | Triggered by the `connectivity` slice's `isOnline` transitioning to `true` (from the "Offline-first" solution); replays each feature's partition independently and in parallel, FIFO within a partition, per [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/queue-partitioning-and-ordering.md|queue-partitioning-and-ordering]]. |

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
- `queuedMutations` is indexed by `feature` and `enqueuedAt`.
  - Risk: a full-table scan + in-code filter to read one partition FIFO scales badly as the queue grows.
  - Fix: `this.version(1).stores({ queuedMutations: '++id, feature, enqueuedAt' })`; read via `.where('feature').equals(f).sortBy('enqueuedAt')`.
- `touchedFields` is stored per queued mutation at enqueue time, derived from the command's own payload.
  - Risk: a captured full-entity snapshot bloats the queue and leaks stale data; without `touchedFields`, conflict resolution has to compare whole entities.
  - Fix: `touchedFields: Object.keys(input)` at enqueue; the 409 handler diffs only those; per [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/conflict-resolution-strategy.md|conflict-resolution-strategy]].
- `idempotencyKey` is generated exactly once, at enqueue, and reused unchanged on every replay of that entry.
  - Risk: a fresh key per replay defeats server-side dedup — a lost-response retry double-applies.
  - Fix: set it in `enqueue()` and never touch it again.

## SHOULD
- **Querying the whole `queuedMutations` table and filtering by feature in application code instead of using the `feature` index** — Consequence: unnecessary full-table scans as the queue grows — Instead: always query through the `feature` index, as `pendingForFeature$` does

# Check list

- [ ] The queue survives a full page reload (Dexie/IndexedDB persistence)
- [ ] `pendingForFeature$` reactively updates the UI as entries are added/removed
- [ ] Every entry carries a stable `idempotencyKey` and its `touchedFields`

# Unittest TestCases

- [ ] WHEN a mutation is enqueued THEN
  - [ ] it persists across a simulated page reload (new Dexie connection reads the same data)
- [ ] WHEN `markSynced` is called for an entry THEN
  - [ ] it is removed from the queue and `pendingForFeature$` reflects the removal
