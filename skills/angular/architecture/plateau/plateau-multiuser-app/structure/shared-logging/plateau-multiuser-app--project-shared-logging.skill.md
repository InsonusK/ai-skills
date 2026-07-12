---
name: plateau-multiuser-app--project-shared-logging
description: LoggerService with a pluggable LogSink extension point — ConsoleLogSink plus BackendLogSink, batching and forwarding logs to the backend through a persisted retry queue — multiuser-app plateau
domain: skill
type: template
plateau: multiuser-app
project_kind: library
version: 20260711230000
tags:
  - skill/template/project
  - plateau/multiuser-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]]"
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]]"
---

# Goal

- Give every part of the application a single logging entry point, with structured entries and environment-based level filtering
- Deliver `warn`/`error`/`report()` entries to the backend, batched and resilient to transient network failures, without any call site needing to change

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Core Principles

- All logging goes through `LoggerService` — no direct `console.*` calls anywhere else in the application
- A log entry is always structured: a message plus a context object, never a single interpolated string
- `LoggerService` forwards every entry to a pluggable list of `LogSink`s, including the new `BackendLogSink`
- `report()` entries always reach `BackendLogSink` regardless of `MIN_LOG_LEVEL` — the production `debug`/`info` filter does not apply to it
- Sensitive data (tokens, passwords, PII) is never logged, at any level, by any sink

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Structure

## Project Structure

```
/libs/shared/logging
  /src
    /lib
      [logger.service.ts](./classes/plateau-multiuser-app--class-logger-service.skill.md)
      log-sink.ts
      console-log-sink.ts
      [backend-log-sink.ts](./classes/plateau-multiuser-app--class-backend-log-sink.skill.md)
      [log-retry-queue.ts](./classes/plateau-multiuser-app--class-log-retry-queue.skill.md)
      log-level.token.ts
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| logger.service.ts | Public API: `debug/info/warn/error/report(message, context?)`, plus `forFeature(name)`. Forwards each entry to every registered `LogSink`. | [[skills/angular/architecture/plateau/plateau-multiuser-app/structure/shared-logging/classes/plateau-multiuser-app--class-logger-service.skill\|class-logger-service]] |
| log-sink.ts | `LogSink` interface and the `LOG_SINKS` multi-provider injection token. | — |
| console-log-sink.ts | Writes each entry to the matching `console.*` method. Always registered. | — |
| backend-log-sink.ts | Batches `warn`/`error`/`report()` entries and POSTs them to the backend. | [[skills/angular/architecture/plateau/plateau-multiuser-app/structure/shared-logging/classes/plateau-multiuser-app--class-backend-log-sink.skill\|class-backend-log-sink]] |
| log-retry-queue.ts | Durable, persisted queue that retries a failed batch send instead of dropping it. | [[skills/angular/architecture/plateau/plateau-multiuser-app/structure/shared-logging/classes/plateau-multiuser-app--class-log-retry-queue.skill\|class-log-retry-queue]] |
| log-level.token.ts | `MIN_LOG_LEVEL` injection token, `'debug'` in development, `'warn'` in production. | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]

## What Does NOT Belong Here

- Feature-specific log formatting or routing rules — every feature uses `LoggerService.forFeature(name)`, not a bespoke wrapper
- Any UI concern — this project is purely infrastructural

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]

## Allowed Dependencies

- `libs/shared/http-core` (tag: `type:util`, `scope:shared`)
- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]

# Rules

## MUST
- `LoggerService` MUST be the only class application code interacts with for logging; no sink MUST be injected or called directly by feature code.
- `MIN_LOG_LEVEL` MUST default to filtering out `debug`/`info` in production builds, keeping `warn`/`error` always enabled.
- `LogEntry`'s `context` MUST be a plain, structured object.
- `report()` entries MUST always reach `BackendLogSink`, regardless of `MIN_LOG_LEVEL`.
- A failed batch send MUST be retried via `log-retry-queue.ts`, not silently dropped.

## MUST NOT
- `LoggerService`/any sink MUST NOT ever be given a token, password, or PII value to log, regardless of level.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Anti-patterns

- **A feature registering its own ad hoc `console.log` wrapper instead of using `LoggerService.forFeature(...)`**
  - Consequence: recreates, inconsistently, exactly what `LoggerService` already provides, and stays invisible to `BackendLogSink`
  - Instead: call `inject(LoggerService).forFeature('orders')` once per feature and use the returned logger
- **Using `report()` as a substitute for `error()` to bypass expected error-handling conventions**
  - Consequence: blurs the meaning of severity levels in backend log queries
  - Instead: use `error()` for actual failures; reserve `report()` for deliberate, non-error events worth tracking

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Check list

- [ ] `LoggerService` is the only logging entry point application-wide
- [ ] `MIN_LOG_LEVEL` filters `debug`/`info` out of production builds
- [ ] `report()` calls always reach `BackendLogSink`, independent of environment-based level filtering
- [ ] A failed batch send is retried, not dropped

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]
