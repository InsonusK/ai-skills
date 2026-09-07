---
name: plateau-persisted-state-monolith--class-log-retry-queue
description: LogRetryQueue in libs/shared/logging — a bounded IndexedDB-persisted retry queue for failed log batches, evicting oldest-first once any of three limits (count, age, size) is exceeded — persisted-state-monolith plateau
domain: skill
type: template
whenToUse: when editing LogRetryQueue (VP6) — the bounded IndexedDB queue, count/age/size oldest-first eviction
plateau: persisted-state-monolith
artifact_type: service
version: 20260903190000
tags:
  - skill/template/class
  - plateau/persisted-state-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]]"

> `libs/shared/logging/src/lib/log-retry-queue.ts`. Used by `BackendLogSink` when a batch send fails. Backed by Dexie/IndexedDB (dev-tested with `fake-indexeddb`).

# Goal

- Survive a page reload with pending unsent log batches, and retry them once the network is back
- Never grow unbounded — cap by count, age AND size, evicting the oldest first

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create.md|Logging/log-retry-queue.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- IndexedDB-persisted, not `localStorage` / in-memory
- All three limits (`maxEntryCount`, `maxAgeMs`, `maxTotalBytes`) enforced independently — exceeding any one triggers oldest-first eviction until all three hold
- `retryPending()` stops at the first failure in a cycle — no burst of failing requests during an outage
- Limits configurable per deployment via `LOG_RETRY_QUEUE_LIMITS`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create.md|Logging/log-retry-queue.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Retry queue | `LogRetryQueue` | `LogRetryQueue` | `log-retry-queue.ts` | `log-retry-queue.ts` |
| Limits token | `{X}_LIMITS` | `LOG_RETRY_QUEUE_LIMITS` | — | — |

# Implementation

```typescript
// Skill: class-log-retry-queue
// Plateau: persisted-state-monolith
// Version: 20260903190000

@Injectable({ providedIn: 'root' })
export class LogRetryQueue {
  async enqueue(batch: LogEntry[]): Promise<void> {
    await this.db.pendingLogBatches.add({ batch, enqueuedAt: Date.now(), bytes: byteSizeOf(batch) });
    await this.evictIfOverLimits();  // count / age / size, oldest-first
  }
  async retryPending(): Promise<void> {
    for (const record of await this.db.pendingLogBatches.orderBy('enqueuedAt').toArray()) {
      try { await firstValueFrom(this.http.post('/logs', record.batch)); await this.db.pendingLogBatches.delete(record.id); }
      catch { break; }  // stop this cycle — retry on the next flush
    }
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create.md|Logging/log-retry-queue.ts.create]]

# Rules

## MUST
- Persist in IndexedDB, not `localStorage` or in-memory.
- Enforce all three limits independently; exceeding any one evicts oldest entries until all three are satisfied.
- `retryPending()` must stop for the current cycle at the first send failure.
- Never apply several plateau templates per class/artifact.
- Never let the queue grow without all three bounds enforced.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create.md|Logging/log-retry-queue.ts.create]]

# Check list

- [ ] Pending batches survive a full page reload
- [ ] All three limits enforced independently
- [ ] A retry cycle stops at the first failure

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create.md|Logging/log-retry-queue.ts.create]]

# Unittest TestCases

- [ ] WHEN the queue exceeds `maxEntryCount` THEN the oldest entries are evicted to the limit
- [ ] WHEN `retryPending()` runs with the network available THEN every pending batch is sent and removed
- [ ] WHEN `retryPending()`'s first send fails THEN no further sends are attempted that cycle
- [ ] WHEN a batch is enqueued and the app "reloads" THEN a fresh queue instance still reports it

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create.md|Logging/log-retry-queue.ts.create]]
