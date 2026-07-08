---
name: backend-log-sink-strategy
description: How logs reach the backend — which levels, how they are batched, how failed sends are retried, and how uncaught exceptions are captured
problem: Sending every log entry to the backend individually and unconditionally is noisy and costly; we need a strategy for what gets sent, how it's batched, what happens when the network is unavailable at send time, and whether uncaught exceptions are captured automatically
decision: Send only warn/error entries plus explicit report() calls, batched and flushed periodically (with a beacon-based flush on page unload), backed by a bounded, IndexedDB-persisted retry queue, with a global ErrorHandler routing uncaught exceptions through LoggerService.error
---

# Problem

The base "Логирование (база)" solution established `LoggerService` with a pluggable `LogSink` extension point but only registered `ConsoleLogSink`. This solution adds the second sink — sending logs to the backend — and needs to decide: which log levels justify a network request; whether entries are sent individually or batched; what happens to a batch if the network is unavailable at the moment of sending; and whether uncaught, unhandled exceptions are captured automatically or only ever reported via explicit code.

# Selected variant

**Selected variant:** [[#Selective levels, batched sending, bounded IndexedDB-persisted retry queue, global ErrorHandler]]

Only `warn`/`error` level entries, plus entries from an explicit `LoggerService.report(...)` call (for intentional, non-error events worth tracking), are forwarded to `BackendLogSink`. Entries are accumulated and flushed periodically (time-based) or once a batch size threshold is reached, with a `navigator.sendBeacon`-based flush on page unload for reliability. If a batch's send fails (network unavailable), it is not dropped — it is persisted to IndexedDB and retried, subject to three bounds: a maximum entry count, a maximum age, and a maximum total size; whichever bound is first exceeded evicts the oldest entries. A global `ErrorHandler` catches every uncaught exception and routes it through `LoggerService.error`, so it flows through the same sink pipeline as any other error log.

# Searched variants

## Selective levels, batched sending, bounded IndexedDB-persisted retry queue, global ErrorHandler

### Description

- **Levels sent**: `warn`, `error`, and explicit `report()` calls only — `debug`/`info` never leave the browser via this sink (they may still reach `ConsoleLogSink` per the base solution's level filtering).
- **Batching**: entries destined for the backend are buffered and flushed either on a timer or once a size threshold is hit, whichever comes first; an additional flush is triggered via `navigator.sendBeacon` on `visibilitychange`/`pagehide` so a batch in progress at tab-close is not silently lost.
- **Retry**: a failed batch send is persisted to an IndexedDB-backed queue and retried on the next flush cycle (e.g. with backoff), rather than dropped. The queue is bounded by three independent limits — max entry count, max age, max total size in bytes — and evicts the oldest entries first whenever any limit is exceeded.
- **Uncaught exceptions**: a custom `ErrorHandler` (registered in `apps/platform-shell`) intercepts every uncaught exception Angular's own error handling would otherwise only log to the console, and routes it through `LoggerService.error`, so it is captured with the same structured context and reaches the backend the same way any other error-level log does.

### Benefits

- Matches the cost/value trade-off used by comparable production tools: shipping every `debug`/`info` entry to a backend produces a large volume of low-value traffic; `warn`/`error` plus explicit `report()` calls capture what actually matters
- Batching plus a beacon-based unload flush significantly reduces the number of network requests compared to sending every entry individually, while still reliably delivering a batch that was in flight when the tab closes
- A bounded, persisted retry queue means a transient network outage does not silently lose error reports — but the three independent bounds (count/age/size) keep the queue from growing unbounded if the backend is unreachable for an extended period, without requiring the full durable offline-sync machinery the future "Синхронизация offline-данных" solution will build for business data
- A global `ErrorHandler` closes the gap this solution exists to close: uncaught exceptions — often the most important signal that something broke — are captured automatically, with no reliance on every code path remembering to call `LoggerService.error` explicitly

### Costs

- More moving parts than "just send everything immediately": a batching timer, an IndexedDB-backed queue, and eviction logic to build and test
- Choosing concrete values for the three eviction bounds (entry count, age, size) requires a deliberate, environment-specific decision — this ADR does not fix those numbers; they are configurable parameters tuned per deployment
- A very long-lived network outage still eventually loses the oldest queued entries once a bound is exceeded — this is an explicit trade-off, not a guarantee of eventual delivery for every single entry

## Send every log level immediately, no batching, no retry

### Description

Every `LoggerService` call at any level immediately fires its own HTTP request to the backend; a failed send is simply dropped.

### Benefits

- Simplest possible implementation — no batching, queueing, or eviction logic at all
- Zero delay between a log call and the backend receiving it, when the network is available

### Costs

- Sending every `debug`/`info` call individually creates a large volume of tiny requests with little corresponding value, and is exactly the noise/cost problem this ADR exists to avoid
- No resilience to transient network issues — a failed send during a brief outage is lost immediately, including error reports that matter most
- Many more network round-trips than a batched approach for the same total volume of data

## Batched sending, but drop a batch entirely on send failure (no retry queue)

### Description

Same batching/flush strategy as the selected variant, but a batch that fails to send is discarded rather than persisted and retried.

### Benefits

- Simpler than the selected variant — no IndexedDB-backed queue, no eviction bounds to design or tune
- Avoids any risk of the retry queue itself growing unbounded or accumulating stale data

### Costs

- Exactly the failure mode this ADR was raised to avoid: an error report generated during a network outage — often precisely when something has gone wrong (e.g. the backend itself is down) — is the one most likely to be silently lost
- Provides no resilience against anything beyond momentary blips; any outage longer than the time between flush attempts loses data permanently

## No global ErrorHandler — only explicit LoggerService.error calls

### Description

Uncaught exceptions are left to Angular's default error handling (console output only); the backend only ever receives what code explicitly logs via `LoggerService.error`/`.report()`.

### Benefits

- No custom `ErrorHandler` to write, test, or maintain
- No risk of the error handler itself introducing a bug that masks or duplicates Angular's own default error reporting

### Costs

- Misses precisely the failures most worth knowing about: unhandled exceptions are often symptoms of bugs nobody anticipated well enough to wrap in an explicit `try/catch` and log call
- Directly contradicts the motivation for adding a backend sink at all — knowing when something breaks on the frontend, including failures the team did not think to instrument explicitly
