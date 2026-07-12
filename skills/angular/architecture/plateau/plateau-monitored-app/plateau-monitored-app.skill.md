---
name: plateau-monitored-app
description: The platform-monolith application extended with backend log delivery — warn/error/report entries are batched, sent to the backend, and retried on failure — plus a global ErrorHandler capturing every uncaught exception. The application's monitoring level is raised.
domain: skill
type: template
version: 20260711220000
tags:
  - skill/template/plateau
  - plateau/monitored-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]]"
parent_plateau: "[[skills/angular/architecture/plateau/plateau-platform-monolith/plateau-platform-monolith.skill.md|platform-monolith]]"
---

> Parent: [[skills/angular/architecture/plateau/plateau-platform-monolith/plateau-platform-monolith.skill.md|platform-monolith]] (the monolith-as-platform — 12 solutions applied so far in the main chain). This plateau adds `solution-logging-global` on top, unchanged otherwise. Next: [[skills/angular/architecture/plateau/plateau-multiuser-app/plateau-multiuser-app.skill.md|multiuser-app]], the final plateau in the chain. Still no authentication — every user remains implicitly trusted until then.

# Core Principles

- `warn`/`error`/`report()` log entries reach the backend, batched, without any call site needing to change
- A failed batch send is never silently dropped — it is handed to a persisted retry queue and replayed on the next successful flush
- `report()` lets a call site intentionally mark a non-error event as worth tracking on the backend, without inflating its severity to `warn`/`error`
- Every uncaught exception anywhere in the platform shell is captured by a single, application-root `GlobalErrorHandler` and routed through `LoggerService.error` — nothing crashes silently in production
- Sensitive data (tokens, passwords, PII) is never logged, at any level, by any sink — unchanged from earlier plateaus

# Capabilities

- backend log delivery
  - `BackendLogSink` batches and POSTs `warn`/`error`/`report()` entries, flushing on a timer, a size threshold, or page unload (via `navigator.sendBeacon`)
  - a failed flush is retried via a persisted `LogRetryQueue`, not dropped
- global error capture
  - `GlobalErrorHandler`, registered once at the application root, routes every uncaught exception through `LoggerService.error`
- everything the `platform-monolith` plateau already provides — a Native Federation dynamic host, version-negotiated design-system sharing, and federated-remote read resilience — unchanged
- everything earlier plateaus already provide — Nx module boundaries, three-tier state, hierarchical routing, Signal Forms, Facade/Client/Mapper HTTP layering, lazy loading, offline read/write resilience, and a layered Vitest/Playwright test strategy — unchanged

# Usecases

## An uncaught exception is captured and reaches the backend

```mermaid
sequenceDiagram
    autonumber
    participant Component
    participant Handler as GlobalErrorHandler
    participant Logger as LoggerService
    participant Sink as BackendLogSink
    participant Queue as LogRetryQueue
    participant Api as Backend

    Component->>Component: throws an uncaught exception
    Handler->>Handler: handleError(error) invoked by Angular's error zone
    Handler->>Logger: error('Uncaught exception', { error })
    Logger->>Sink: write(entry)
    Sink->>Sink: buffer.push(entry)
    Note over Sink: buffer flushed on timer/size-threshold/unload
    Sink->>Api: POST /logs (batch)
    alt flush succeeds
        Api-->>Sink: 200 OK
    else flush fails
        Sink->>Queue: enqueue(batch)
        Note over Queue: retried on the next successful flush
    end
```
