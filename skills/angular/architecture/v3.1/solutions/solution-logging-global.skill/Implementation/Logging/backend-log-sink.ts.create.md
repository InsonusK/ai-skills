---
description: BackendLogSink — batches warn/error/report entries and sends them to the backend, with a beacon-based flush on unload and retry-queue fallback on failure
project_name: shared-logging
name: backend-log-sink
element_kind: service
change_kind: create
tags:
  - solution/logging-global
  - element/backend-log-sink-ts
---

# Goals

- Send only `warn`/`error`/`report()` entries to the backend, batched, without adding a new network request per log call

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ------------------ | -------------------- | --------- |
| Backend sink | BackendLogSink | BackendLogSink | backend-log-sink.ts | backend-log-sink.ts |

# Implementation changes

```typescript
@Injectable()
export class BackendLogSink implements LogSink {
  private buffer: LogEntry[] = [];

  constructor(
    private readonly http: BaseHttpService, // from `solution-api-http-layer`
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
      await this.retryQueue.enqueue(batch); // never dropped — see log-retry-queue.ts.create.md
    }
  }

  private flushViaBeacon(): void {
    if (this.buffer.length === 0) return;
    navigator.sendBeacon('/logs', JSON.stringify(this.buffer));
    this.buffer = [];
  }
}
```

# Rule changes

## MUST
- `BackendLogSink` must filter to `warn`/`error`/`report` entries only, discarding `debug`/`info` before they are ever buffered.
- On a failed flush, the batch must be handed to `LogRetryQueue.enqueue(...)` — it must never be silently discarded.
- The unload flush must use `navigator.sendBeacon`, not a regular `fetch`/`HttpClient` call, since a normal request can be cancelled by the browser when the page is unloading.

## SHOULD
- **Sending each entry as its own HTTP request instead of buffering** — Consequence: reintroduces the request-volume problem batching exists to solve — Instead: always buffer and flush on a timer/size threshold/unload, never per-entry

# Check list

- [ ] `debug`/`info` entries never appear in a batch sent to the backend
- [ ] A failed flush always results in the batch being enqueued via `LogRetryQueue`, never dropped outright
- [ ] The unload path uses `navigator.sendBeacon`

# Unittest TestCases

- [ ] WHEN the buffer reaches `MAX_BATCH_SIZE` THEN
  - [ ] a flush is triggered immediately, without waiting for the timer
- [ ] WHEN a flush's HTTP call fails THEN
  - [ ] the batch is passed to `LogRetryQueue.enqueue(...)`
- [ ] WHEN the page is unloaded with entries still buffered THEN
  - [ ] `navigator.sendBeacon` is called with the remaining buffer
