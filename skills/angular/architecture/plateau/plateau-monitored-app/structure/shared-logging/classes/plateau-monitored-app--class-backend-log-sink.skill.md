---
name: plateau-monitored-app--class-backend-log-sink
description: BackendLogSink — batches warn/error/report entries and sends them to the backend, with a beacon-based flush on unload and retry-queue fallback on failure — monitored-app plateau
domain: skill
type: template
plateau: monitored-app
artifact_type: service
version: 20260711220000
tags:
  - skill/template/class
  - plateau/monitored-app
created_by:
  - "[[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
---

# Goal

- Send only `warn`/`error`/`report()` entries to the backend, batched, without adding a new network request per log call

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Registered via the same `LOG_SINKS` multi-provider token the base logging solution established

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ------------------ | -------------------- | --------- |
| Backend sink | `BackendLogSink` | `BackendLogSink` | `backend-log-sink.ts` | `backend-log-sink.ts` |

# Implementation

```typescript
// Skill: class-backend-log-sink
// Plateau: monitored-app
// Version: 20260711220000

@Injectable()
export class BackendLogSink implements LogSink {
  private buffer: LogEntry[] = [];

  constructor(
    private readonly http: BaseHttpService,
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
      await this.retryQueue.retryPending();
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
- [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]

# Rules

## MUST
- `BackendLogSink` MUST filter to `warn`/`error`/`report` entries only, discarding `debug`/`info` before they are ever buffered.
- On a failed flush, the batch MUST be handed to `LogRetryQueue.enqueue(...)` — never silently discarded.
- The unload flush MUST use `navigator.sendBeacon`, not a regular `fetch`/`HttpClient` call.

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Sending each entry as its own HTTP request instead of buffering**
  - Consequence: reintroduces the request-volume problem batching exists to solve
  - Instead: always buffer and flush on a timer/size threshold/unload, never per-entry

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]

# Check list

- [ ] `debug`/`info` entries never appear in a batch sent to the backend
- [ ] A failed flush always results in the batch being enqueued via `LogRetryQueue`, never dropped
- [ ] The unload path uses `navigator.sendBeacon`

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]

# Unittest TestCases

- [ ] WHEN the buffer reaches `MAX_BATCH_SIZE` THEN
  - [ ] a flush is triggered immediately, without waiting for the timer
- [ ] WHEN a flush's HTTP call fails THEN
  - [ ] the batch is passed to `LogRetryQueue.enqueue(...)`
- [ ] WHEN the page is unloaded with entries still buffered THEN
  - [ ] `navigator.sendBeacon` is called with the remaining buffer

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]
