---
name: plateau-observable
description: Authenticated app gains structured, centralized logging — a single LoggerService with a pluggable sink extension point, a backend sink with batching/beacon-flush/bounded IndexedDB retry, and a global ErrorHandler capturing every uncaught exception.
domain: skill
type: template
version: 20260711160000
tags:
  - skill/template/plateau
  - plateau/observable
created_by:
  - "[[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]]"
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]]"
parent_plateau: "[[skills/angular/architecture/plateau/authenticated/plateau-authenticated.skill.md|authenticated]]"
---

> Fifth plateau in the main application's chain. Previous: [[skills/angular/architecture/plateau/authenticated/plateau-authenticated.skill.md|authenticated]]. Next: [[skills/angular/architecture/plateau/tested/plateau-tested.skill.md|tested]].

# Core Principles

- Everything from [[skills/angular/architecture/plateau/authenticated/plateau-authenticated.skill.md|authenticated]] carries over unchanged: session lifecycle, permission-based authorization, hierarchical routing, Signal Forms, and the Facade/Client-layered data-access pattern
- All logging goes through `LoggerService` — no direct `console.*` calls anywhere else in the application
- A log entry is always structured: a message plus a context object, never a single interpolated string
- `LoggerService` forwards every entry to a pluggable list of `LogSink`s; only `warn`/`error`/`report()` entries reach the backend sink, `debug`/`info` stay local to the console sink
- Sensitive data (tokens, passwords, PII) is never logged, at any level — a hard rule, not a per-call-site judgment
- A failed backend batch send is never dropped outright — it is persisted to a bounded, IndexedDB-backed retry queue and retried
- Every uncaught exception is captured by a single global `ErrorHandler` and routed through `LoggerService.error`, reaching the backend the same way any other error log does

# Capabilities

- structure, state management, routing, forms, data access, authentication & authorization
  - Unchanged from [[skills/angular/architecture/plateau/authenticated/plateau-authenticated.skill.md|authenticated]]
- logging
  - Structured log context (feature name, relevant IDs) is filterable/queryable from day one
  - Production builds automatically drop `debug`/`info` noise while keeping `warn`/`error`
  - One consistent entry point (`LoggerService`) with zero call-site rewrites when the backend sink was added
- observability
  - Backend visibility into frontend failures, without needing every code path to explicitly opt in
  - Resilience to brief network outages via a persisted, bounded retry queue — a failed batch is retried, not silently dropped
  - Uncaught exceptions are captured automatically by a global `ErrorHandler`, with no reliance on explicit instrumentation everywhere
  - A reliable unload-time flush (`sendBeacon`) so buffered entries aren't lost when a tab closes mid-batch

# Usecases

## A feature logs a structured event that reaches the backend

```mermaid
sequenceDiagram
    autonumber
    participant Feature as Feature code
    participant Logger as LoggerService
    participant Console as ConsoleLogSink
    participant Backend as BackendLogSink
    participant Api as Backend

    Feature->>Logger: error('Failed to save order', { orderId })
    Logger->>Logger: check MIN_LOG_LEVEL — error always passes
    Logger->>Console: write(entry)
    Console-->>Feature: printed to devtools console
    Logger->>Backend: write(entry)
    Backend->>Backend: buffer entry (warn/error/report only)
    Note over Backend,Api: on timer or size threshold
    Backend->>Api: POST /logs (batch)
    activate Api
    Api-->>Backend: 200 OK
    deactivate Api
```

## An uncaught exception is captured and reported

```mermaid
sequenceDiagram
    autonumber
    participant App as Running application
    participant Handler as GlobalErrorHandler
    participant Logger as LoggerService
    participant Backend as BackendLogSink

    App->>Handler: uncaught exception thrown
    Handler->>Handler: extract sanitized message/stack only
    Handler->>Logger: error('Uncaught exception', { message, stack })
    Logger->>Backend: write(entry)
    Note over Backend: flows through the same batching/retry pipeline as any other error log
```

## A backend outage does not lose log entries

```mermaid
sequenceDiagram
    autonumber
    participant Sink as BackendLogSink
    participant Api as Backend
    participant Queue as LogRetryQueue (IndexedDB)

    Sink->>Api: flush batch
    activate Api
    Api-->>Sink: network error
    deactivate Api
    Sink->>Queue: enqueue(batch)
    Queue->>Queue: evict oldest if over count/age/size limits
    Note over Sink,Api: later flush cycle, network restored
    Sink->>Queue: retryPending()
    Queue->>Api: send queued batch
    activate Api
    Api-->>Queue: success
    deactivate Api
    Queue->>Queue: remove sent batch from queue
```
