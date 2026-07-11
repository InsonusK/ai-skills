---
name: class-replay-orchestrator
description: Replay orchestrator — triggered by connectivity restoration, replays each feature's queue partition independently, and implements the server-wins conflict-handling seam
domain: skill
type: template
plateau: platform
artifact_type: service
version: 20260711150000
tags:
  - skill/template/class
  - plateau/platform
created_by:
  - "[[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]]"
---

# Goal

- Replay queued mutations per feature partition, FIFO within a partition, in parallel across partitions
- Implement the server-wins conflict default, while exposing a single overridable conflict-handling step

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create|OfflineSync/replay-orchestrator.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- A failure in one partition's replay never stops or delays any other partition's replay
- `handleConflict` is the single, overridable extension seam

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create|OfflineSync/replay-orchestrator.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ------------------ | -------------------- | --------- |
| Replay orchestrator | `ReplayOrchestrator` | `ReplayOrchestrator` | `replay-orchestrator.ts` | `replay-orchestrator.ts` |

# Implementation

```typescript
// Skill: class-replay-orchestrator
// Plateau: platform
// Version: 20260711150000

@Injectable({ providedIn: 'root' })
export class ReplayOrchestrator {
  constructor(
    private readonly store: Store,
    private readonly queue: MutationQueueService,
    private readonly facades: Record<string, unknown>,
  ) {
    effect(() => {
      if (this.store.selectSignal(selectIsOnline)()) this.replayAllPartitions();
    });
  }

  private async replayAllPartitions(): Promise<void> {
    const features = await this.queue.listFeatures();
    await Promise.all(features.map(feature => this.replayPartition(feature)));
  }

  private async replayPartition(feature: string): Promise<void> {
    const entries = await this.queue.pendingForFeatureOnce(feature);
    for (const entry of entries) {
      try {
        await this.replayEntry(entry);
        await this.queue.markSynced(entry.id);
      } catch (error) {
        if (error instanceof ConflictError) {
          await this.handleConflict(entry, error);
        }
        break;
      }
    }
  }

  private async replayEntry(entry: QueuedMutation): Promise<void> {
    const facade = this.facades[entry.feature];
    await facade[entry.operationName](entry.payload, { idempotencyKey: entry.idempotencyKey });
  }

  private async handleConflict(entry: QueuedMutation, error: ConflictError): Promise<void> {
    await this.queue.markSynced(entry.id);
    this.notifications.dispatch(NotificationsActions.show({
      message: `Your change to ${entry.touchedFields.join(', ')} in ${entry.feature} wasn't applied — it was changed elsewhere.`,
      detail: error.currentServerValues,
    }));
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create|OfflineSync/replay-orchestrator.ts.create]]

# Rules

## MUST
- Replay MUST process all feature partitions concurrently, and entries within one partition strictly FIFO.
- A failure in one partition's replay MUST NOT stop or delay any other partition's replay.
- `handleConflict` MUST be a single, separately named method/injection point.
- On conflict, the discarded change's notification MUST include only the specific touched fields and their current server values.

## MUST NOT
- This orchestrator MUST NOT implement per-operation or per-field custom conflict logic beyond server-wins.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create|OfflineSync/replay-orchestrator.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Inlining conflict-handling logic directly inside `replayPartition`'s loop**
  - Consequence: the future extension solution would need to modify the core replay loop itself
  - Instead: keep `handleConflict` as the single point of variation
- **Retrying the same failed entry immediately within the same replay cycle**
  - Consequence: risks a tight failure loop against a partition that is genuinely stuck
  - Instead: stop the partition on the first failure

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create|OfflineSync/replay-orchestrator.ts.create]]

# Check list

- [ ] Feature partitions replay concurrently
- [ ] Entries within one partition replay strictly FIFO
- [ ] `handleConflict` is a single, overridable method
- [ ] Conflict notifications reference only touched fields

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create|OfflineSync/replay-orchestrator.ts.create]]

# Unittest TestCases

- [ ] WHEN two features both have pending mutations and one feature's replay fails repeatedly THEN
  - [ ] the other feature's partition still completes its replay successfully
- [ ] WHEN a replayed entry receives a conflict response THEN
  - [ ] the local change is discarded, and a notification is dispatched with only the touched fields' current server values
- [ ] WHEN `isOnline` transitions from `false` to `true` THEN
  - [ ] replay is triggered automatically

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create|OfflineSync/replay-orchestrator.ts.create]]
