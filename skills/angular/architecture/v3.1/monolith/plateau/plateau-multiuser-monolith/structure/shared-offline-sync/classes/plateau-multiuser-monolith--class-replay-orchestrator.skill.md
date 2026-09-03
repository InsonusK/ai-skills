---
name: plateau-multiuser-monolith--class-replay-orchestrator
description: The replay orchestrator — connectivity-triggered, replays feature partitions concurrently (FIFO within a partition), with a single overridable server-wins handleConflict seam — plus MutationReplayRegistry and ReplayConflictError — multiuser-monolith plateau
domain: skill
type: template
whenToUse: when editing ReplayOrchestrator or the FeatureReplay contract (VP5) — the partition replay loop, the handleConflict seam, the onReplayStart/onReplayResult callbacks
plateau: multiuser-monolith
artifact_type: service
version: 20260903150000
tags:
  - skill/template/class
  - plateau/multiuser-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]]"

> `libs/shared/offline-sync/src/lib/replay-orchestrator.ts`. Registered + eagerly instantiated by `provideOfflineSync()` at the shell so its connectivity effect is live from bootstrap.

# Goal

- Replay queued mutations per feature partition, FIFO within a partition, in parallel across partitions
- Default to server-wins, while exposing a single clearly-separated `handleConflict` a future solution can override
- Drive a **per-entity sync status** in each feature (`queued → sending → synced | failed | conflict`) through the `onReplayStart` / `onReplayResult` callbacks — so the user sees *which* row is in flight, not only a count

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create.md|OfflineSync/replay-orchestrator.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Replay is triggered by the `connectivity` slice's `isOnline` becoming `true` — no manual trigger
- Partitions replay concurrently; a partition stops on its first failure (no tight retry loop) and other partitions are unaffected
- The orchestrator never imports a feature — it looks handlers up in `MutationReplayRegistry`, which features populate from their route `providers`
- `handleConflict` is the one point of variation: server-wins here, overridable later

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create.md|OfflineSync/replay-orchestrator.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/conflict-resolution-strategy.md|Conflict Resolution Strategy ADR]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/queue-partitioning-and-ordering.md|Queue Partitioning And Ordering ADR]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Orchestrator | `ReplayOrchestrator` | `ReplayOrchestrator` | `replay-orchestrator.ts` | `replay-orchestrator.ts` |
| Registry | `{X}Registry` | `MutationReplayRegistry` | (same file) | — |
| Conflict error | `{X}Error` | `ReplayConflictError` | (same file) | — |
| Feature handler | `{X}` | `FeatureReplay` (interface) | — | — |

# Implementation

```typescript
// Skill: class-replay-orchestrator
// Plateau: multiuser-monolith
// Version: 20260903120000

export type SyncStatus = 'queued' | 'sending' | 'failed' | 'conflict';

export class ReplayConflictError extends Error {
  constructor(readonly currentServerValues: Record<string, unknown>) { super('replay conflict'); }
}
export interface FeatureReplay {
  readonly feature: string;
  replay(entry: PersistedMutation): Promise<void>; // throw ReplayConflictError on a field conflict
  onReplayStart?(entry: PersistedMutation): void;  // feature sets its row's syncStatus 'sending'
  onReplayResult?(entry: PersistedMutation, result: 'synced' | 'failed' | 'conflict'): void;
}

@Injectable({ providedIn: 'root' })
export class MutationReplayRegistry {
  private readonly handlers = new Map<string, FeatureReplay>();
  register(h: FeatureReplay) { this.handlers.set(h.feature, h); }
  handlerFor(feature: string) { return this.handlers.get(feature); }
}

@Injectable({ providedIn: 'root' })
export class ReplayOrchestrator {
  private readonly online = inject(Store).selectSignal(selectIsOnline);
  private tail: Promise<void> = Promise.resolve();       // serialise replay requests
  constructor() { effect(() => { if (this.online()) void this.replayAllPartitions(); }); }

  replayAllPartitions(): Promise<void> {
    this.tail = this.tail.then(() => (this.online() ? this.runReplay() : undefined));
    return this.tail;
  }
  private async runReplay() {
    const features = await this.queue.listFeatures();
    await Promise.all(features.map((f) => this.replayPartition(f)));  // concurrent
  }
  private async replayPartition(feature: string) {
    const handler = this.registry.handlerFor(feature);
    if (!handler) return;                              // registered when its feature loads
    for (const entry of await this.queue.pendingForFeatureOnce(feature)) {
      handler.onReplayStart?.(entry);                  // -> row syncStatus 'sending'
      try {
        await handler.replay(entry);
        await this.queue.markSynced(entry.id);
        handler.onReplayResult?.(entry, 'synced');     // -> clear the row's syncStatus
      } catch (e) {
        if (e instanceof ReplayConflictError) {
          await this.handleConflict(entry, e);
          handler.onReplayResult?.(entry, 'conflict'); // -> row syncStatus 'conflict'
          continue;
        }
        handler.onReplayResult?.(entry, 'failed');     // -> row syncStatus 'failed'
        break;                                         // transient — stop; next online event retries
      }
    }
  }
  private async handleConflict(entry: PersistedMutation, e: ReplayConflictError) {
    await this.queue.markSynced(entry.id);             // server wins — discard local
    inject(Store).dispatch(NotificationsActions.show({
      message: `Your change to ${entry.touchedFields.join(', ')} in ${entry.feature} wasn't applied — it was changed elsewhere.`,
      detail: e.currentServerValues,                   // touched fields only
    }));
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create.md|OfflineSync/replay-orchestrator.ts.create]]

# Rules

## MUST
- Replay must process partitions concurrently (`Promise.all`) and entries within a partition strictly FIFO.
- A failure in one partition must never stop or delay another's replay; a partition stops on its first transient failure — never a tight in-cycle retry.
- `handleConflict` must be one separately-named method, not inlined into `replayPartition`, so a future solution overrides just it.
- On conflict the notification must carry only the touched fields' current server values — never the full entity.
- The orchestrator must never `import` a feature lib — handlers come from `MutationReplayRegistry`.
- No conflict strategy beyond server-wins may be implemented here.
- The orchestrator must call `onReplayStart` before, and `onReplayResult` after, every `replay(entry)` — `'synced'` on success, `'conflict'` after `handleConflict`, `'failed'` on a transient error. Both callbacks are optional.
- Never apply several plateau templates per class/artifact.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create.md|OfflineSync/replay-orchestrator.ts.create]]

# Check list

- [ ] Partitions replay concurrently; a stuck partition doesn't delay others
- [ ] Entries within one partition replay strictly FIFO
- [ ] `handleConflict` is a single overridable method, not inlined
- [ ] Conflict notifications reference only touched fields
- [ ] No import of any feature/data-access lib

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create.md|OfflineSync/replay-orchestrator.ts.create]]

# Unittest TestCases

- [ ] WHEN two features have pending mutations and one repeatedly fails THEN the other completes its replay
- [ ] WHEN a replayed entry gets a conflict THEN the local change is discarded and a field-scoped notification is dispatched
- [ ] WHEN `isOnline` transitions `false → true` THEN replay is triggered automatically
- [ ] WHEN a partition has no registered handler THEN it is skipped (retried once its feature loads)
- [ ] WHEN an entry is replayed THEN `onReplayStart` fires first, then `onReplayResult` with `'synced'` / `'conflict'` / `'failed'` for the three outcomes

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create.md|OfflineSync/replay-orchestrator.ts.create]]
