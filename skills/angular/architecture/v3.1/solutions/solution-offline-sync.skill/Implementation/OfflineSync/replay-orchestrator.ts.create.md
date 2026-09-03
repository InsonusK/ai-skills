---
description: Replay orchestrator — triggered by connectivity restoration, replays each feature's queue partition independently, and implements the server-wins conflict-handling seam
project_name: shared-offline-sync
name: replay-orchestrator
element_kind: service
change_kind: create
tags:
  - solution/offline-sync
  - element/replay-orchestrator-ts
---

# Goals

- Replay queued mutations per feature partition, FIFO within a partition, in parallel across partitions
- Implement the server-wins conflict default, while exposing a single, clearly separated conflict-handling step a future solution can override
- Drive a **per-entity sync status** in each feature — `queued → sending → (synced | failed | conflict)` — so the user sees *which* row is in flight, not just a count, and so an optimistic row can be rebuilt after a cold restart

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ------------------ | -------------------- | --------- |
| Replay orchestrator | ReplayOrchestrator | ReplayOrchestrator | replay-orchestrator.ts | replay-orchestrator.ts |

# Implementation changes

The orchestrator never imports a feature. A feature registers a `FeatureReplay` handler (from its own route `providers`, so no feature code enters the initial bundle); the handler carries `replay()` plus two **optional lifecycle callbacks** the orchestrator calls around every replay, which the feature uses to drive a per-entity `syncStatus`.

```typescript
export type SyncStatus = 'queued' | 'sending' | 'failed' | 'conflict';

export interface FeatureReplay {
  readonly feature: string;
  /** Replay the operation. Throw `ReplayConflictError` on a field conflict. */
  replay(entry: PersistedMutation): Promise<void>;
  /** Called just before this entry is replayed — the feature sets the row's `syncStatus: 'sending'`. */
  onReplayStart?(entry: PersistedMutation): void;
  /** Called with the outcome — the feature clears `syncStatus` on `'synced'`, else sets `'failed'`/`'conflict'`. */
  onReplayResult?(entry: PersistedMutation, result: 'synced' | 'failed' | 'conflict'): void;
}

@Injectable({ providedIn: 'root' })
export class ReplayOrchestrator {
  private readonly store = inject(Store);            // connectivity slice
  private readonly queue = inject(MutationQueueService);
  private readonly registry = inject(MutationReplayRegistry);
  private readonly online = this.store.selectSignal(selectIsOnline);

  constructor() {
    effect(() => { if (this.online()) void this.replayAllPartitions(); });
  }

  private async replayAllPartitions(): Promise<void> {
    const features = await this.queue.listFeatures();
    await Promise.all(features.map((feature) => this.replayPartition(feature)));
  }

  private async replayPartition(feature: string): Promise<void> {
    const handler = this.registry.handlerFor(feature);
    if (!handler) return; // retried once its feature loads
    const entries = await this.queue.pendingForFeatureOnce(feature);
    for (const entry of entries) {
      handler.onReplayStart?.(entry);                       // -> row syncStatus 'sending'
      try {
        await handler.replay(entry);
        await this.queue.markSynced(entry.id);
        handler.onReplayResult?.(entry, 'synced');          // -> clear the row's syncStatus
      } catch (error) {
        if (error instanceof ReplayConflictError) {
          await this.handleConflict(entry, error);          // the single, designed extension seam
          handler.onReplayResult?.(entry, 'conflict');      // -> row syncStatus 'conflict'
          continue;
        }
        handler.onReplayResult?.(entry, 'failed');          // -> row syncStatus 'failed'
        break; // stop this partition's replay on failure; other partitions unaffected
      }
    }
  }

  // Server-wins default. A future solution replaces or wraps ONLY this method.
  private async handleConflict(entry: PersistedMutation, error: ReplayConflictError): Promise<void> {
    await this.queue.markSynced(entry.id); // discard the local change — server wins
    this.store.dispatch(NotificationsActions.show({
      message: `Your change to ${entry.touchedFields.join(', ')} in ${entry.feature} wasn't applied — it was changed elsewhere.`,
      detail: error.currentServerValues, // current values of the touched fields only
    }));
  }
}
```

The feature side: on `OfflineTransportError` the Facade returns `{ queued: true, idempotencyKey, optimistic }`; the feature store appends `optimistic` with `syncStatus: 'queued'` and, on a cold start, calls a `hydratePending()` that reads `MutationQueueService.pendingForFeatureOnce(feature)` and rebuilds those rows. `PendingSyncIndicatorComponent`'s count is *derived* from the rows carrying a `syncStatus`.

# Rule changes

## MUST
- Replay processes feature partitions concurrently (`Promise.all`) and entries within a partition strictly FIFO.
  - Risk: a global FIFO lets one stuck feature block every other feature's sync; out-of-order replay within a feature breaks "create then update".
  - Fix: `Promise.all(features.map(replayPartition))`; each partition loops its entries in `enqueuedAt` order; per [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/queue-partitioning-and-ordering.md|queue-partitioning-and-ordering]].
- A failure in one partition never stops or delays another's replay.
  - Risk: one feature's outage stalls the whole app's sync.
  - Fix: a partition stops on its first transient failure (no tight retry) and retries on the next connectivity event; siblings are untouched.
- `handleConflict` is one separately named method, not inlined into `replayPartition`.
  - Risk: a future smarter-resolution solution would have to modify the core replay loop to plug in.
  - Fix: keep `handleConflict` a single overridable seam; the loop calls it and continues.
- On conflict the notification carries only the touched fields' current server values — never the full entity.
  - Risk: dumping the whole entity leaks unrelated fields and can't tell the user precisely what didn't apply.
  - Fix: `error.currentServerValues` holds only `touchedFields`; per [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/conflict-resolution-strategy.md|conflict-resolution-strategy]].
- The orchestrator calls `onReplayStart` before and `onReplayResult` after every `replay(entry)` — `'synced'` / `'conflict'` / `'failed'` for the three outcomes.
  - Risk: without the callbacks the feature cannot move a row from `queued` to `sending` to done, and the user only ever sees a count.
  - Fix: `handler.onReplayStart?.(entry)` then `handler.onReplayResult?.(entry, result)`; both are optional.
- The user-facing pending surface is a **per-entity `syncStatus`** on the feature's rows, driven by those callbacks — the indicator count is *derived* from the rows.
  - Risk: an opaque "3 pending" tells the user something is syncing but not *what* — after a cold restart it can even lack a visible row.
  - Fix: `syncStatus` per row (`queued`/`sending`/`failed`/`conflict`); `pendingSyncCount` is a `computed` over them.
- A feature that queues creates rebuilds its optimistic rows from the persisted queue on a cold start.
  - Risk: the in-memory optimistic row is lost on reload, leaving a pending count with no row — looks like data loss.
  - Fix: `hydratePending()` reads `MutationQueueService.pendingForFeatureOnce(feature)` on init and re-adds `syncStatus: 'queued'` rows.
- Never implement per-operation or per-field conflict logic beyond server-wins here.
  - Risk: a half-built resolution strategy pre-empts the future solution meant to own it.
  - Fix: server-wins only; `handleConflict` is the seam a later solution overrides; per [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/adr/conflict-resolution-strategy.md|conflict-resolution-strategy]].
## SHOULD
- **Inlining conflict-handling logic directly inside `replayPartition`'s loop instead of a separate `handleConflict` method** — Consequence: the future extension solution would need to modify the core replay loop itself to add smarter resolution, instead of overriding one well-defined seam — Instead: keep `handleConflict` as the single point of variation
- **Retrying the same failed entry immediately within the same replay cycle instead of stopping the partition** — Consequence: risks a tight failure loop against a partition that is genuinely stuck (e.g. a persistently failing operation), consuming resources without making progress — Instead: stop the partition on the first failure; the next connectivity-restoration event (or a future periodic retry trigger) tries again

# Check list

- [ ] Feature partitions replay concurrently; a stuck partition does not delay others
- [ ] Entries within one partition replay strictly FIFO
- [ ] `handleConflict` is a single, overridable method, not inlined logic
- [ ] Conflict notifications reference only touched fields, never the full entity
- [ ] `onReplayStart` / `onReplayResult` are called around every replay, with `'synced'`/`'failed'`/`'conflict'`
- [ ] The feature shows a per-entity `syncStatus`; the indicator count is derived from it
- [ ] A cold restart rebuilds optimistic rows from `pendingForFeatureOnce` — no invisible pending count

# Unittest TestCases

- [ ] WHEN two features both have pending mutations and one feature's replay fails repeatedly THEN
  - [ ] the other feature's partition still completes its replay successfully
- [ ] WHEN a replayed entry receives a conflict response THEN
  - [ ] the local change is discarded, and a notification is dispatched with only the touched fields' current server values
- [ ] WHEN `isOnline` transitions from `false` to `true` THEN
  - [ ] replay is triggered automatically, without manual intervention
- [ ] WHEN an entry is replayed THEN
  - [ ] `onReplayStart` fires first, then `onReplayResult` with `'synced'` (success), `'conflict'` (409), or `'failed'` (transient error)
- [ ] WHEN the app cold-starts with mutations still in the queue THEN
  - [ ] `hydratePending()` rebuilds each optimistic row with `syncStatus: 'queued'`, and the pending count matches the visible rows
