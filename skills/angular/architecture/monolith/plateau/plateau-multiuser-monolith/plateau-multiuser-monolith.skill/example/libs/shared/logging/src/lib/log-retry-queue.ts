import { inject, Injectable, InjectionToken } from '@angular/core';
import Dexie, { Table } from 'dexie';
import { BaseHttpService } from '@org/shared-http-core';
import { LogEntry } from './log-sink';

export interface LogRetryQueueLimits {
  readonly maxEntryCount: number; // batches
  readonly maxAgeMs: number;
  readonly maxTotalBytes: number;
}

export const LOG_RETRY_QUEUE_LIMITS = new InjectionToken<LogRetryQueueLimits>('LOG_RETRY_QUEUE_LIMITS', {
  providedIn: 'root',
  factory: () => ({ maxEntryCount: 500, maxAgeMs: 7 * 24 * 3600_000, maxTotalBytes: 2 * 1024 * 1024 }),
});

interface PendingBatch {
  id?: number;
  batch: LogEntry[];
  enqueuedAt: number;
  bytes: number;
}

class LogRetryDb extends Dexie {
  pendingLogBatches!: Table<PendingBatch, number>;
  constructor(name = 'log-retry') {
    super(name);
    this.version(1).stores({ pendingLogBatches: '++id, enqueuedAt' });
  }
}

const byteSizeOf = (b: unknown) => new Blob([JSON.stringify(b)]).size;

/**
 * Bounded, IndexedDB-persisted retry queue for log batches that failed to send.
 * Pending batches survive a page reload. Evicts oldest-first once ANY of three
 * independent limits (count, age, size) is exceeded.
 */
@Injectable({ providedIn: 'root' })
export class LogRetryQueue {
  private readonly db = new LogRetryDb();
  private readonly http = inject(BaseHttpService);
  private readonly limits = inject(LOG_RETRY_QUEUE_LIMITS);

  async enqueue(batch: LogEntry[]): Promise<void> {
    await this.db.pendingLogBatches.add({ batch, enqueuedAt: Date.now(), bytes: byteSizeOf(batch) });
    await this.evictIfOverLimits();
  }

  async retryPending(): Promise<void> {
    const pending = await this.db.pendingLogBatches.orderBy('enqueuedAt').toArray();
    for (const record of pending) {
      if (record.id == null) continue;
      try {
        await new Promise<void>((res, rej) =>
          this.http.post('/logs', record.batch).subscribe({ next: () => res(), error: rej }),
        );
        await this.db.pendingLogBatches.delete(record.id);
      } catch {
        break; // network still down — stop this cycle, retry next flush
      }
    }
  }

  /** test / diagnostics */
  count(): Promise<number> {
    return this.db.pendingLogBatches.count();
  }
  clear(): Promise<void> {
    return this.db.pendingLogBatches.clear();
  }

  private async evictIfOverLimits(): Promise<void> {
    const now = Date.now();
    for (;;) {
      const all = await this.db.pendingLogBatches.orderBy('enqueuedAt').toArray();
      const totalBytes = all.reduce((s, r) => s + r.bytes, 0);
      const oldestOverAge = all[0] && now - all[0].enqueuedAt > this.limits.maxAgeMs;
      const overCount = all.length > this.limits.maxEntryCount;
      const overBytes = totalBytes > this.limits.maxTotalBytes;
      if (!oldestOverAge && !overCount && !overBytes) return;
      const oldestId = all[0]?.id;
      if (oldestId == null) return;
      await this.db.pendingLogBatches.delete(oldestId); // evict oldest
    }
  }
}
