import { effect, inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { NotificationsActions, selectIsOnline } from '@org/shared-state';
import { MutationQueueService, PersistedMutation } from './mutation-queue.service';

/**
 * A conflict surfaced during replay: the server changed one of the fields this
 * queued mutation touched while the client was offline. Carries only the
 * current server values of the touched fields — never a full entity snapshot.
 */
export class ReplayConflictError extends Error {
  constructor(readonly currentServerValues: Record<string, unknown>) {
    super('replay conflict — the server changed a touched field');
    this.name = 'ReplayConflictError';
  }
}

/** A feature registers how to replay one of its queued operations. */
export interface FeatureReplay {
  readonly feature: string;
  /** Replay the operation. Throw `ReplayConflictError` on a field conflict. */
  replay(entry: PersistedMutation): Promise<void>;
}

/**
 * Root registry of per-feature replay handlers. A lazy feature registers its
 * handler when its route loads — the orchestrator never imports a feature, so
 * there is no cycle and nothing forces a feature into the initial bundle.
 */
@Injectable({ providedIn: 'root' })
export class MutationReplayRegistry {
  private readonly handlers = new Map<string, FeatureReplay>();

  register(handler: FeatureReplay): void {
    this.handlers.set(handler.feature, handler);
  }

  handlerFor(feature: string): FeatureReplay | undefined {
    return this.handlers.get(feature);
  }
}

@Injectable({ providedIn: 'root' })
export class ReplayOrchestrator {
  private readonly store = inject(Store);
  private readonly queue = inject(MutationQueueService);
  private readonly registry = inject(MutationReplayRegistry);
  private readonly online = this.store.selectSignal(selectIsOnline);
  /** replay requests run one after another — connectivity events and feature
   *  registrations can both ask for a replay concurrently without racing */
  private tail: Promise<void> = Promise.resolve();

  constructor() {
    // Kick a replay whenever connectivity is (re)gained.
    effect(() => {
      if (this.online()) void this.replayAllPartitions();
    });
  }

  replayAllPartitions(): Promise<void> {
    this.tail = this.tail.then(() => (this.online() ? this.runReplay() : undefined));
    return this.tail;
  }

  private async runReplay(): Promise<void> {
    const features = await this.queue.listFeatures();
    // partitions replay concurrently — a stuck one never blocks the others
    await Promise.all(features.map((f) => this.replayPartition(f)));
  }

  private async replayPartition(feature: string): Promise<void> {
    const handler = this.registry.handlerFor(feature);
    if (!handler) return; // no handler registered yet — retried once its feature loads
    const entries = await this.queue.pendingForFeatureOnce(feature);
    for (const entry of entries) {
      try {
        await handler.replay(entry);
        await this.queue.markSynced(entry.id);
      } catch (error) {
        if (error instanceof ReplayConflictError) {
          await this.handleConflict(entry, error);
          continue; // conflict resolved (server wins) — keep draining this partition
        }
        break; // transient failure — stop this partition; the next online event retries
      }
    }
  }

  // Server-wins default. A future solution replaces or wraps ONLY this method
  // to plug in per-operation resolution — `replayPartition`'s control flow stays.
  private async handleConflict(entry: PersistedMutation, error: ReplayConflictError): Promise<void> {
    await this.queue.markSynced(entry.id); // discard local — server wins
    this.store.dispatch(
      NotificationsActions.show({
        message: `Your change to ${entry.touchedFields.join(', ')} in ${entry.feature} wasn't applied — it was changed elsewhere.`,
        detail: error.currentServerValues,
      }),
    );
  }
}
