---
description: Bounded, IndexedDB-persisted retry queue for log batches that failed to send, evicting the oldest entries when any of three limits (count, age, size) is exceeded
project_name: shared-logging
name: log-retry-queue
element_kind: service
change_kind: create
tags:
  - solution/logging-global
  - element/log-retry-queue-ts
---

# Goals

- Survive a page reload with pending, unsent log batches, and retry them once the network is available again
- Never grow unbounded — cap total entries by count, age, and byte size, evicting the oldest first

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Retry queue | LogRetryQueue | LogRetryQueue | log-retry-queue.ts | log-retry-queue.ts |

# Implementation changes

```typescript
export interface LogRetryQueueLimits {
  maxEntryCount: number; // e.g. 500
  maxAgeMs: number;      // e.g. 7 days
  maxTotalBytes: number; // e.g. 2 MB
}

@Injectable({ providedIn: 'root' })
export class LogRetryQueue {
  constructor(
    private readonly db: IndexedDbStore, // small internal wrapper around the IndexedDB API
    @Inject(LOG_RETRY_QUEUE_LIMITS) private readonly limits: LogRetryQueueLimits,
  ) {}

  async enqueue(batch: LogEntry[]): Promise<void> {
    await this.db.add('pending-log-batches', { batch, enqueuedAt: Date.now() });
    await this.evictIfOverLimits();
  }

  async retryPending(): Promise<void> {
    const pending = await this.db.getAll('pending-log-batches');
    for (const record of pending) {
      try {
        await this.http.post('/logs', record.batch);
        await this.db.delete('pending-log-batches', record.id);
      } catch {
        break; // network still unavailable — stop and try again next cycle
      }
    }
  }

  private async evictIfOverLimits(): Promise<void> {
    const all = await this.db.getAll('pending-log-batches');
    const now = Date.now();
    const overAge = all.filter(r => now - r.enqueuedAt > this.limits.maxAgeMs);
    const totalBytes = all.reduce((sum, r) => sum + byteSizeOf(r.batch), 0);
    const overCount = all.length > this.limits.maxEntryCount;
    if (overAge.length || totalBytes > this.limits.maxTotalBytes || overCount) {
      // evict oldest-first until all three limits are satisfied
    }
  }
}
```

# Rule changes

## MUST
- The queue is persisted in IndexedDB — not `localStorage`, not in-memory only.
  - Risk: in-memory loses pending batches on reload; `localStorage` is small, synchronous, and string-only.
  - Fix: a Dexie table `pendingLogBatches: '++id, enqueuedAt'`.
- All three limits (`maxEntryCount`, `maxAgeMs`, `maxTotalBytes`) are enforced independently — exceeding any one evicts oldest-first until all three hold.
  - Risk: a single limit lets the queue grow unbounded along another axis (many tiny old entries, or few huge ones).
  - Fix: an `evictIfOverLimits` loop checks all three each enqueue and deletes `all[0]` until satisfied.
- `retryPending()` stops for the current cycle at the first failed send.
  - Risk: continuing to drain the queue against a still-down network is a burst of failing requests.
  - Fix: `for (const b of ordered) { try { await post(b); await delete(b.id); } catch { break; } }`.

## SHOULD
- The three limit values should be configurable per deployment via `LOG_RETRY_QUEUE_LIMITS`, not hardcoded, since the right values depend on expected log volume and acceptable storage footprint.

- **Letting the queue grow without any of the three bounds enforced** — Consequence: an extended backend outage could let the queue consume unbounded IndexedDB storage — Instead: always check and evict against all three limits after every enqueue
- **Retrying the entire pending queue in a tight loop even after the first failure in a cycle** — Consequence: floods the network with failing requests during an outage, worsening the situation instead of backing off — Instead: stop the current retry cycle on the first failure; the next scheduled flush will try again
# Check list

- [ ] Pending batches survive a full page reload
- [ ] All three limits (count, age, size) are enforced independently
- [ ] A retry cycle stops at the first failure rather than looping through remaining entries

# Unittest TestCases

- [ ] WHEN the queue exceeds `maxEntryCount` THEN
  - [ ] the oldest entries are evicted until the count is back within the limit
- [ ] WHEN an entry's age exceeds `maxAgeMs` THEN
  - [ ] it is evicted on the next `enqueue` or `retryPending` call
- [ ] WHEN `retryPending()` is called and the network is available THEN
  - [ ] every pending batch is sent and removed from the queue
- [ ] WHEN `retryPending()` is called and the first send fails THEN
  - [ ] no further sends are attempted in that cycle
