import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { liveQuery } from 'dexie';
import { MutationQueueDb, QueuedMutation } from './mutation-queue.db';

export type NewMutation = Omit<QueuedMutation, 'id' | 'enqueuedAt' | 'idempotencyKey'>;
export type PersistedMutation = QueuedMutation & { id: number };

/**
 * The public API of the Dexie-backed mutation queue. Feature Facades call
 * `enqueue()` on `OfflineTransportError`; the replay orchestrator drains it;
 * pending-sync UI reads `pendingForFeature$()`.
 */
@Injectable({ providedIn: 'root' })
export class MutationQueueService {
  private readonly db = new MutationQueueDb();

  async enqueue(entry: NewMutation): Promise<PersistedMutation> {
    const row: QueuedMutation = {
      ...entry,
      idempotencyKey: crypto.randomUUID(),
      enqueuedAt: Date.now(),
    };
    const id = await this.db.queuedMutations.add(row);
    return { ...row, id };
  }

  /** Reactive per-partition view for the pending-sync indicator. */
  pendingForFeature$(feature: string): Observable<PersistedMutation[]> {
    return liveQuery(() =>
      this.db.queuedMutations.where('feature').equals(feature).sortBy('enqueuedAt'),
    ) as unknown as Observable<PersistedMutation[]>;
  }

  /** One-shot FIFO read of a partition, for the replay orchestrator. */
  pendingForFeatureOnce(feature: string): Promise<PersistedMutation[]> {
    return this.db.queuedMutations
      .where('feature')
      .equals(feature)
      .sortBy('enqueuedAt') as Promise<PersistedMutation[]>;
  }

  async listFeatures(): Promise<string[]> {
    return this.db.queuedMutations.orderBy('feature').uniqueKeys() as unknown as Promise<string[]>;
  }

  async markSynced(id: number): Promise<void> {
    await this.db.queuedMutations.delete(id);
  }

  /** Test-support: empty the queue table (keeps the connection open). */
  async clear(): Promise<void> {
    await this.db.queuedMutations.clear();
  }
}
