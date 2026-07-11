---
name: project-shared-logging
description: LoggerService with a pluggable LogSink extension point — ConsoleLogSink always active, BackendLogSink (warn/error/report only, batched, retried) added on top
domain: skill
type: template
plateau: tested
project_kind: library
version: 20260711170000
tags:
  - skill/template/project
  - plateau/tested
created_by:
  - "[[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]]"
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
---

# Goal

- Give every part of the application a single logging entry point, with structured entries and environment-based level filtering
- Provide an extension point (`LOG_SINKS`) a backend-sending sink can be added to, without touching this project's console sink or any call site
- Send the logs that matter (`warn`/`error`, plus explicit `report()` events) to the backend, without the cost/noise of shipping every `debug`/`info` call
- Never silently lose an error report to a transient network outage, within a bounded storage budget

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

# Core Principles

- All logging goes through `LoggerService` — no direct `console.*` calls anywhere else in the application
- A log entry is always structured: a message plus a context object, never a single interpolated string
- `LoggerService` forwards every entry to a pluggable list of `LogSink`s
- Sensitive data (tokens, passwords, PII) is never logged, at any level
- Only `warn`/`error`/`report()` entries reach `BackendLogSink`; `debug`/`info` stay local to `ConsoleLogSink`
- Entries are batched and flushed on a timer/size threshold, with a `sendBeacon`-based flush on page unload for reliability
- A failed batch send is never dropped outright — it is persisted to a bounded IndexedDB queue and retried, evicting the oldest entries first once count/age/size limits are exceeded

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

# Structure

## Project Structure

```
/libs/shared/logging
  /src
    /lib
      logger.service.ts        (extended, see class-logger-service.skill.md)
      log-sink.ts
      console-log-sink.ts
      log-level.token.ts
      backend-log-sink.ts       (see class-backend-log-sink.skill.md)
      log-retry-queue.ts        (see class-log-retry-queue.skill.md)
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| logger.service.ts | Public API: `debug/info/warn/error/report(message, context?)`, plus `forFeature(name)`. Forwards each entry to every registered `LogSink`, after checking it against the configured minimum level (`report()` bypasses that filter). | [[classes/class-logger-service.skill.md\|class-logger-service.skill]] |
| log-sink.ts | `LogSink` interface and the `LOG_SINKS` multi-provider injection token. | — |
| console-log-sink.ts | Writes each entry to the matching `console.*` method. Always registered. | — |
| log-level.token.ts | `MIN_LOG_LEVEL` injection token, `'debug'` in development, `'warn'` in production. | — |
| backend-log-sink.ts | Second `LogSink`: batches `warn`/`error`/`report` entries and sends them to the backend, with a beacon-based flush on unload and retry-queue fallback on failure. | [[classes/class-backend-log-sink.skill.md\|class-backend-log-sink.skill]] |
| log-retry-queue.ts | Bounded, IndexedDB-persisted retry queue for batches `BackendLogSink` failed to send. | [[classes/class-log-retry-queue.skill.md\|class-log-retry-queue.skill]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]

## NPM Packages

None beyond Angular's own DI/core APIs and the browser's native IndexedDB and `navigator.sendBeacon` APIs.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]

## What Does NOT Belong Here

- Feature-specific log formatting or routing rules — every feature uses `LoggerService.forFeature(name)`, not a bespoke wrapper
- DTO mapping, HTTP request business logic — `BackendLogSink` only ever sends already-structured log entries

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]

## Allowed Dependencies

- `libs/shared/http-core` (tag: `type:util`, `scope:shared`) — `BackendLogSink` sends batches through the base HTTP service
- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

# Rules

## MUST
- `LoggerService` MUST be the only class application code interacts with for logging; `ConsoleLogSink`/`BackendLogSink` MUST NOT be injected or called directly by feature code.
- `MIN_LOG_LEVEL` MUST default to filtering out `debug`/`info` in production builds, keeping `warn`/`error` always enabled.
- `LogEntry`'s `context` MUST be a plain, structured object.
- `BackendLogSink` MUST be registered via the same `LOG_SINKS` multi-provider token the base sink uses — no existing call site changes.
- `BackendLogSink` MUST only forward `warn`/`error` level entries and explicit `report()` calls — `debug`/`info` MUST NOT reach it.

## MUST NOT
- `LoggerService`/any sink MUST NOT ever be given a token, password, or PII value to log, regardless of level.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

- **A feature registering its own ad hoc `console.log` wrapper instead of using `LoggerService.forFeature(...)`**
  - Consequence: recreates, inconsistently, exactly what `LoggerService` already provides, and is invisible to the backend sink
  - Instead: call `inject(LoggerService).forFeature('orders')` once per feature and use the returned logger
- **Wiring `BackendLogSink` to also forward `debug`/`info` entries "to have more data"**
  - Consequence: reintroduces the noise/cost problem the backend sink exists to avoid
  - Instead: keep `debug`/`info` local to `ConsoleLogSink`; use `report()` for anything at a lower severity worth sending

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

# Check list

- [ ] `LoggerService` is the only logging entry point application-wide
- [ ] `MIN_LOG_LEVEL` filters `debug`/`info` out of production builds
- [ ] `debug`/`info` entries never appear in a batch sent to the backend
- [ ] A failed batch send is always retried via `LogRetryQueue`, never dropped outright

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]
