---
name: plateau-persisted-state-monolith--class-backend-log-sink
description: BackendLogSink in libs/shared/logging — batches warn/error/report entries and sends them to the backend, with a sendBeacon flush on unload and a LogRetryQueue fallback on failure — persisted-state-monolith plateau
domain: skill
type: template
whenToUse: when editing BackendLogSink (VP6) — the batching, sendBeacon on unload, the LogRetryQueue fallback
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

> `libs/shared/logging/src/lib/backend-log-sink.ts`. Registered on the existing `LOG_SINKS` multi-provider seam alongside `ConsoleLogSink` — no existing `LoggerService` call site changes.

# Goal

- Send only `warn` / `error` / `report()` entries to the backend, batched, without a network request per log call

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create.md|Logging/backend-log-sink.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- `debug` / `info` are discarded before they are ever buffered
- Entries are buffered and flushed on a timer OR a size threshold; on `pagehide` the flush uses `navigator.sendBeacon` (a normal request is cancelled during unload)
- A failed flush hands the batch to `LogRetryQueue.enqueue(...)` — never dropped; a successful flush opportunistically drains the retry queue too

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create.md|Logging/backend-log-sink.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/adr/backend-log-sink-strategy.md|Backend Log Sink Strategy ADR]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Backend sink | `BackendLogSink` | `BackendLogSink` | `backend-log-sink.ts` | `backend-log-sink.ts` |

# Implementation

```typescript
// Skill: class-backend-log-sink
// Plateau: persisted-state-monolith
// Version: 20260903190000

@Injectable({ providedIn: 'root' })
export class BackendLogSink implements LogSink, OnDestroy {
  private buffer: LogEntry[] = [];
  private readonly timer = setInterval(() => void this.flush(), FLUSH_INTERVAL_MS);
  write(entry: LogEntry): void {
    if (entry.level === 'warn' || entry.level === 'error' || entry.level === 'report') {
      this.buffer.push(entry);
      if (this.buffer.length >= MAX_BATCH_SIZE) void this.flush();
    }
  }
  async flush(): Promise<void> {
    if (!this.buffer.length) return;
    const batch = this.buffer; this.buffer = [];
    try { await firstValueFrom(this.http.post('/logs', batch)); await this.retryQueue.retryPending(); }
    catch { await this.retryQueue.enqueue(batch); }
  }
  // pagehide → navigator.sendBeacon('/api/logs', JSON.stringify(this.buffer))
}
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create.md|Logging/backend-log-sink.ts.create]]

# Rules

## MUST
- Filter to `warn` / `error` / `report` only — `debug` / `info` never buffered.
- On a failed flush the batch must go to `LogRetryQueue.enqueue(...)` — never silently discarded.
- The unload flush must use `navigator.sendBeacon`, not `fetch`/`HttpClient`.
- Never apply several plateau templates per class/artifact.
- Never send each entry as its own request — always buffer + flush on timer/size/unload.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create.md|Logging/backend-log-sink.ts.create]]

# Check list

- [ ] `debug` / `info` never appear in a batch
- [ ] A failed flush always enqueues via `LogRetryQueue`
- [ ] The unload path uses `navigator.sendBeacon`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create.md|Logging/backend-log-sink.ts.create]]

# Unittest TestCases

- [ ] WHEN the buffer reaches `MAX_BATCH_SIZE` THEN a flush fires immediately
- [ ] WHEN a flush's HTTP call fails THEN the batch is passed to `LogRetryQueue.enqueue(...)`
- [ ] WHEN the page is unloaded with entries buffered THEN `navigator.sendBeacon` is called

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create.md|Logging/backend-log-sink.ts.create]]
