---
name: plateau-multiuser-monolith--project-shared-logging
description: LoggerService on the LOG_SINKS extension point, with ConsoleLogSink plus (VP6) BackendLogSink (batched warn/error/report), a bounded IndexedDB LogRetryQueue, and LoggerService.report() — multiuser-monolith plateau
domain: skill
type: template
plateau: multiuser-monolith
project_kind: library
version: 20260903150000
tags:
  - skill/template/project
  - plateau/multiuser-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]]"

> VP6 (`solution-logging-global`) adds `BackendLogSink` on the same `LOG_SINKS` multi-provider seam as `ConsoleLogSink` — no existing `LoggerService` call site changes — plus a bounded IndexedDB `LogRetryQueue` and `LoggerService.report()`. `GlobalErrorHandler` lives in `apps/platform-shell`.

# Goal

- Give every part of the application a single logging entry point, with structured entries and environment-based level filtering
- VP6: send the logs that matter (`warn` / `error` / `report()`) to the backend, batched, resilient to a brief outage, without any call-site rework

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create.md|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Repository.extend.md|Repository.extend]]

# Structure

## Project Structure

```
/libs/shared/logging/src/lib
  log-sink.ts               <- LogLevel gains 'report'
  console-log-sink.ts       <- maps 'report' → console.info
  backend-log-sink.ts       <- new (VP6): batched warn/error/report, sendBeacon on pagehide
  log-retry-queue.ts        <- new (VP6): bounded IndexedDB queue (count/age/size), oldest-first eviction
  logger.service.ts         <- LOG_SINKS factory now returns [ConsoleLogSink, BackendLogSink]; + report()
```

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| backend-log-sink.ts | The second `LogSink`. Only warn/error/report; batched; sendBeacon on unload; `LogRetryQueue` on failure. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/shared-logging/classes/plateau-multiuser-monolith--class-backend-log-sink.skill.md\|class-backend-log-sink]] |
| log-retry-queue.ts | Bounded IndexedDB retry queue. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/shared-logging/classes/plateau-multiuser-monolith--class-log-retry-queue.skill.md\|class-log-retry-queue]] |
| logger.service.ts | `report()` (always to the backend); `LOG_SINKS` factory adds `BackendLogSink`. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/shared-logging/classes/plateau-multiuser-monolith--class-logger-service.skill.md\|class-logger-service]] |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create.md|Logging/backend-log-sink.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create.md|Logging/log-retry-queue.ts.create]]

## Allowed Dependencies

- `libs/shared/http-core` (tag: `type:data-access`, `scope:shared`) — VP6: `BackendLogSink` / `LogRetryQueue` send batches through the base HTTP service

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Repository.extend.md|Repository.extend]]

# Core Principles

- All logging goes through `LoggerService` — no direct `console.*` calls anywhere else in the application
- A log entry is always structured: a message plus a context object, never a single interpolated string
- `LoggerService` forwards every entry to a pluggable list of `LogSink`s
- Sensitive data (tokens, passwords, PII) is never logged, at any level

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create.md|Logging/shared-logging.project.create]]

# Structure

## Project Structure

```
/libs/shared/logging
  /src
    /lib
      [logger.service.ts](./classes/plateau-multiuser-monolith--class-logger-service.skill.md)
      log-sink.ts
      console-log-sink.ts
      log-level.token.ts
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| logger.service.ts | Public API: `debug/info/warn/error/report(message, context?)`, plus `forFeature(name)`. Forwards each entry to every registered `LogSink`, after checking it against the configured minimum level. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/shared-logging/classes/plateau-multiuser-monolith--class-logger-service.skill\|class-logger-service]] |
| log-sink.ts | `LogSink` interface and the `LOG_SINKS` multi-provider injection token. | — |
| console-log-sink.ts | Writes each entry to the matching `console.*` method. Always registered. | — |
| log-level.token.ts | `MIN_LOG_LEVEL` injection token, `'debug'` in development, `'warn'` in production. | — |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create.md|Logging/shared-logging.project.create]]

## NPM Packages

None beyond Angular's own DI/core APIs.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create.md|Logging/shared-logging.project.create]]

## What Does NOT Belong Here

- Feature-specific log formatting or routing rules — every feature uses `LoggerService.forFeature(name)`, not a bespoke wrapper
- Any network call — this plateau logs to the console only

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create.md|Logging/shared-logging.project.create]]

## Allowed Dependencies

- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create.md|Logging/shared-logging.project.create]]

# Rules

## MUST
- `LoggerService` must be the only class application code interacts with for logging; `ConsoleLogSink` must never be injected or called directly by feature code.
- `MIN_LOG_LEVEL` must default to filtering out `debug`/`info` in production builds, keeping `warn`/`error` always enabled.
- `LogEntry`'s `context` must be a plain, structured object.

- `LoggerService`/any sink must never ever be given a token, password, or PII value to log, regardless of level.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create.md|Logging/shared-logging.project.create]]


- **A feature registering its own ad hoc `console.log` wrapper instead of using `LoggerService.forFeature(...)`**
  - Consequence: recreates, inconsistently, exactly what `LoggerService` already provides, and won't be visible to a future backend sink
  - Instead: call `inject(LoggerService).forFeature('orders')` once per feature and use the returned logger

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create.md|Logging/shared-logging.project.create]]

# Check list

- [ ] `LoggerService` is the only logging entry point application-wide
- [ ] `MIN_LOG_LEVEL` filters `debug`/`info` out of production builds

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create.md|Logging/shared-logging.project.create]]
