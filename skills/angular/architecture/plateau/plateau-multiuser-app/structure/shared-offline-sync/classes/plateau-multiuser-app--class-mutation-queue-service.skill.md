---
name: plateau-multiuser-app--class-mutation-queue-service
description: Dexie database definition and public MutationQueueService API — schema, per-feature partitioning, reactive queries for pending-sync UI — multiuser-app plateau
domain: skill
type: template
plateau: multiuser-app
artifact_type: service
version: 20260711230000
tags:
  - skill/template/class
  - plateau/multiuser-app
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]]"
---

# Goal

- Durable, reactive storage for queued mutations, partitioned by feature

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create|OfflineSync/shared-offline-sync.project.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- `touchedFields` is derived directly from the command's own payload at enqueue time, never a separately captured entity snapshot
- `idempotencyKey` is generated exactly once, at enqueue time, and reused unchanged across every replay attempt

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create|OfflineSync/shared-offline-sync.project.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ------------------ | -------------------- | --------- |
| Dexie DB | `MutationQueueDb` | `MutationQueueDb` | `mutation-queue.db.ts` | `mutation-queue.db.ts` |
| Queue service | `MutationQueueService` | `MutationQueueService` | `mutation-queue.service.ts` | `mutation-queue.service.ts` |

# Implementation

```typescript
// Skill: class-mutation-queue-service
// Plateau: multiuser-app
// Version: 20260711230000

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

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create|OfflineSync/shared-offline-sync.project.create]]

# Rules

## MUST
- `queuedMutations` MUST be indexed by `feature` and `enqueuedAt` to support efficient per-partition FIFO reads.
- `touchedFields` MUST be stored per queued mutation at enqueue time, derived directly from the command's own payload.
- `idempotencyKey` MUST be generated exactly once, at enqueue time, and reused unchanged across every replay attempt of that entry.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create|OfflineSync/shared-offline-sync.project.create]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Querying the whole `queuedMutations` table and filtering by feature in application code instead of using the `feature` index**
  - Consequence: unnecessary full-table scans as the queue grows
  - Instead: always query through the `feature` index, as `pendingForFeature$` does
- **Reusing the same idempotency key across a retried command's multiple attempts without regenerating it per logical mutation**
  - Consequence: the backend cannot tell "same command retried" from "new, different command"
  - Instead: generate the key once, at enqueue time, and reuse it for every replay of that entry

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create|OfflineSync/shared-offline-sync.project.create]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]

# Check list

- [ ] The queue survives a full page reload (Dexie/IndexedDB persistence)
- [ ] `pendingForFeature$` reactively updates the UI as entries are added/removed
- [ ] Every entry carries a stable `idempotencyKey` and its `touchedFields`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create|OfflineSync/shared-offline-sync.project.create]]

# Unittest TestCases

- [ ] WHEN a mutation is enqueued THEN
  - [ ] it persists across a simulated page reload (new Dexie connection reads the same data)
- [ ] WHEN `markSynced` is called for an entry THEN
  - [ ] it is removed from the queue and `pendingForFeature$` reflects the removal
- [ ] WHEN the same queued mutation is replayed twice due to a lost response THEN
  - [ ] both attempts carry the same idempotency key

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create|OfflineSync/shared-offline-sync.project.create]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]
