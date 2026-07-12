---
name: plateau-multiuser-app--class-log-retry-queue
description: Bounded, IndexedDB-persisted retry queue for log batches that failed to send, evicting the oldest entries when any of three limits (count, age, size) is exceeded — multiuser-app plateau
domain: skill
type: template
plateau: multiuser-app
artifact_type: service
version: 20260711230000
tags:
  - skill/template/class
  - plateau/multiuser-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
---

# Goal

- Survive a page reload with pending, unsent log batches, and retry them once the network is available again
- Never grow unbounded — cap total entries by count, age, and byte size, evicting the oldest first

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- All three limits are enforced independently; exceeding any one triggers eviction

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Retry queue | `LogRetryQueue` | `LogRetryQueue` | `log-retry-queue.ts` | `log-retry-queue.ts` |

# Implementation

```typescript
// Skill: class-log-retry-queue
// Plateau: multiuser-app
// Version: 20260711230000

export interface LogRetryQueueLimits {
  maxEntryCount: number; // e.g. 500
  maxAgeMs: number;      // e.g. 7 days
  maxTotalBytes: number; // e.g. 2 MB
}

@Injectable({ providedIn: 'root' })
export class LogRetryQueue {
  constructor(
    private readonly db: IndexedDbStore,
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
    // evict oldest-first until count, age, and total-bytes limits are all satisfied
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]

# Rules

## MUST
- The queue MUST be persisted in IndexedDB, not `localStorage` or in-memory only.
- The queue MUST enforce all three limits (`maxEntryCount`, `maxAgeMs`, `maxTotalBytes`) independently.
- `retryPending()` MUST stop retrying for the current cycle as soon as one send attempt fails.

## SHOULD
- The three limit values SHOULD be configurable per deployment via `LOG_RETRY_QUEUE_LIMITS`, not hardcoded.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Letting the queue grow without any of the three bounds enforced**
  - Consequence: an extended backend outage could let the queue consume unbounded IndexedDB storage
  - Instead: always check and evict against all three limits after every enqueue
- **Retrying the entire pending queue in a tight loop even after the first failure in a cycle**
  - Consequence: floods the network with failing requests during an outage
  - Instead: stop the current retry cycle on the first failure

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]

# Check list

- [ ] Pending batches survive a full page reload
- [ ] All three limits (count, age, size) are enforced independently
- [ ] A retry cycle stops at the first failure rather than looping through remaining entries

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]

# Unittest TestCases

- [ ] WHEN the queue exceeds `maxEntryCount` THEN
  - [ ] the oldest entries are evicted until the count is back within the limit
- [ ] WHEN an entry's age exceeds `maxAgeMs` THEN
  - [ ] it is evicted on the next `enqueue` or `retryPending` call
- [ ] WHEN `retryPending()` is called and the network is available THEN
  - [ ] every pending batch is sent and removed from the queue
- [ ] WHEN `retryPending()` is called and the first send fails THEN
  - [ ] no further sends are attempted in that cycle

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]
