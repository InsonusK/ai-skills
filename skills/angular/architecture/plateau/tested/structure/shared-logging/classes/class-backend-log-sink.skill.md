---
name: class-backend-log-sink
description: BackendLogSink — batches warn/error/report entries and sends them to the backend, with a beacon-based flush on unload and retry-queue fallback on failure
domain: skill
type: template
plateau: tested
artifact_type: service
version: 20260711170000
tags:
  - skill/template/class
  - plateau/tested
created_by:
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
---

# Goal

- Send only `warn`/`error`/`report()` entries to the backend, batched, without adding a new network request per log call

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Entries are batched and flushed on a timer/size threshold, with a `sendBeacon`-based flush on page unload for reliability
- A failed batch send is never dropped outright — it is handed to `LogRetryQueue`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ------------------ | -------------------- | --------- |
| Backend sink | `BackendLogSink` | `BackendLogSink` | `backend-log-sink.ts` | `backend-log-sink.ts` |

# Implementation

```typescript
// Skill: class-backend-log-sink
// Plateau: observable
// Version: 20260711170000

@Injectable()
export class BackendLogSink implements LogSink {
  private buffer: LogEntry[] = [];

  constructor(
    private readonly http: BaseHttpService, // from libs/shared/http-core
    private readonly retryQueue: LogRetryQueue,
  ) {
    setInterval(() => this.flush(), FLUSH_INTERVAL_MS);
    window.addEventListener('pagehide', () => this.flushViaBeacon());
  }

  write(entry: LogEntry): void {
    if (entry.level === 'warn' || entry.level === 'error' || entry.level === 'report') {
      this.buffer.push(entry);
      if (this.buffer.length >= MAX_BATCH_SIZE) this.flush();
    }
  }

  private async flush(): Promise<void> {
    if (this.buffer.length === 0) return;
    const batch = this.buffer;
    this.buffer = [];
    try {
      await firstValueFrom(this.http.post('/logs', batch));
      await this.retryQueue.retryPending(); // opportunistically drain previously-queued batches too
    } catch {
      await this.retryQueue.enqueue(batch); // never dropped
    }
  }

  private flushViaBeacon(): void {
    if (this.buffer.length === 0) return;
    navigator.sendBeacon('/logs', JSON.stringify(this.buffer));
    this.buffer = [];
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]

# Rules

## MUST
- `BackendLogSink` MUST filter to `warn`/`error`/`report` entries only, discarding `debug`/`info` before they are ever buffered.
- On a failed flush, the batch MUST be handed to `LogRetryQueue.enqueue(...)` — it MUST NOT be silently discarded.
- The unload flush MUST use `navigator.sendBeacon`, not a regular `fetch`/`HttpClient` call, since a normal request can be cancelled by the browser when the page is unloading.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Sending each entry as its own HTTP request instead of buffering**
  - Consequence: reintroduces the request-volume problem batching exists to solve
  - Instead: always buffer and flush on a timer/size threshold/unload, never per-entry

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]

# Check list

- [ ] `debug`/`info` entries never appear in a batch sent to the backend
- [ ] A failed flush always results in the batch being enqueued via `LogRetryQueue`, never dropped outright
- [ ] The unload path uses `navigator.sendBeacon`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]

# Unittest TestCases

- [ ] WHEN the buffer reaches `MAX_BATCH_SIZE` THEN
  - [ ] a flush is triggered immediately, without waiting for the timer
- [ ] WHEN a flush's HTTP call fails THEN
  - [ ] the batch is passed to `LogRetryQueue.enqueue(...)`
- [ ] WHEN the page is unloaded with entries still buffered THEN
  - [ ] `navigator.sendBeacon` is called with the remaining buffer

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]
