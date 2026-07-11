---
name: class-log-retry-queue
description: Bounded, IndexedDB-persisted retry queue for log batches that failed to send, evicting the oldest entries when any of three limits (count, age, size) is exceeded
domain: skill
type: template
plateau: platform
artifact_type: service
version: 20260711150000
tags:
  - skill/template/class
  - plateau/platform
created_by:
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
---

# Goal

- Survive a page reload with pending, unsent log batches, and retry them once the network is available again
- Never grow unbounded

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- All three limits are enforced independently

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Retry queue | `LogRetryQueue` | `LogRetryQueue` | `log-retry-queue.ts` | `log-retry-queue.ts` |

# Implementation

```typescript
// Skill: class-log-retry-queue
// Plateau: platform
// Version: 20260711150000

export interface LogRetryQueueLimits {
  maxEntryCount: number;
  maxAgeMs: number;
  maxTotalBytes: number;
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
        break;
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
- The queue MUST be persisted in IndexedDB.
- The queue MUST enforce all three limits independently.
- `retryPending()` MUST stop retrying on the first failure in a cycle.

## SHOULD
- The three limit values SHOULD be configurable via `LOG_RETRY_QUEUE_LIMITS`.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Letting the queue grow without any of the three bounds enforced**
  - Consequence: unbounded IndexedDB storage during an extended outage
  - Instead: always check and evict against all three limits
- **Retrying the entire pending queue in a tight loop even after the first failure**
  - Consequence: floods the network with failing requests during an outage
  - Instead: stop the current retry cycle on the first failure

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]

# Check list

- [ ] Pending batches survive a full page reload
- [ ] All three limits are enforced independently
- [ ] A retry cycle stops at the first failure

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]

# Unittest TestCases

- [ ] WHEN the queue exceeds `maxEntryCount` THEN
  - [ ] the oldest entries are evicted
- [ ] WHEN `retryPending()` is called and the network is available THEN
  - [ ] every pending batch is sent and removed
- [ ] WHEN `retryPending()` is called and the first send fails THEN
  - [ ] no further sends are attempted in that cycle

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]
