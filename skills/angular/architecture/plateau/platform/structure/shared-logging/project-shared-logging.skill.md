---
name: project-shared-logging
description: LoggerService, the LogSink extension point, console + backend sinks, and the persisted retry queue for failed backend sends
domain: skill
type: template
plateau: platform
project_kind: library
version: 20260711150000
tags:
  - skill/template/project
  - plateau/platform
created_by:
  - "[[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]]"
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
---

# Goal

- Give every part of the application a single logging entry point, with structured entries, environment-based level filtering, and delivery to a backend log store
- Never lose a warn/error entry due to a transient network failure — persist and retry

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]

# Core Principles

- `LoggerService` is the only class application code interacts with for logging
- `MIN_LOG_LEVEL` defaults to filtering out `debug`/`info` in production builds
- A log entry never contains an auth token, password, or other PII, regardless of level
- Only `warn`/`error`/`report()` entries ever reach the backend

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

# Structure

## Project Structure

```
/libs/shared/logging
  /src
    /lib
      [logger.service.ts](./classes/class-logger-service.skill.md)
      log-sink.ts
      console-log-sink.ts
      log-level.token.ts
      [backend-log-sink.ts](./classes/class-backend-log-sink.skill.md)
      [log-retry-queue.ts](./classes/class-log-retry-queue.skill.md)
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| logger.service.ts | Public API: `debug/info/warn/error/report(message, context?)` plus `forFeature(name)` | [[classes/class-logger-service.skill.md\|class-logger-service.skill]] |
| log-sink.ts | `LogSink` interface and the `LOG_SINKS` multi-provider injection token | — |
| console-log-sink.ts | Writes each entry to the matching `console.*` method | — |
| log-level.token.ts | `MIN_LOG_LEVEL` injection token | — |
| backend-log-sink.ts | Batches `warn`/`error`/`report()` entries and sends them to the backend | [[classes/class-backend-log-sink.skill.md\|class-backend-log-sink.skill]] |
| log-retry-queue.ts | Bounded, IndexedDB-persisted retry queue for failed log batches | [[classes/class-log-retry-queue.skill.md\|class-log-retry-queue.skill]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

## NPM Packages

None beyond Angular's own DI/core APIs.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]

## What Does NOT Belong Here

- Direct `console.*` calls anywhere else in the codebase
- Any auth token, password, or PII value in a logged message or context

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Repository.extend|Repository.extend]]

## Allowed Dependencies

- `libs/shared/http-core` (for `BackendLogSink`'s HTTP calls)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]

# Rules

## MUST
- `LoggerService` MUST be the only class application code interacts with for logging.
- `MIN_LOG_LEVEL` MUST default to filtering out `debug`/`info` in production builds.
- `LoggerService`/any sink MUST NOT ever be given a token, password, or PII value to log.
- `BackendLogSink` MUST be registered via the `LOG_SINKS` multi-provider token.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

- **A feature registering its own ad hoc `console.log` wrapper instead of using `LoggerService.forFeature(...)`**
  - Consequence: recreates, inconsistently, exactly what `LoggerService` already provides
  - Instead: call `inject(LoggerService).forFeature('orders')` once per feature
- **Wiring `BackendLogSink` to also forward `debug`/`info` entries "to have more data"**
  - Consequence: reintroduces the noise/cost problem the backend-sink ADR exists to avoid
  - Instead: keep `debug`/`info` local to `ConsoleLogSink`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

# Check list

- [ ] `LoggerService` is the only logging entry point application-wide
- [ ] `MIN_LOG_LEVEL` filters `debug`/`info` out of production builds
- [ ] `debug`/`info` entries never appear in a batch sent to the backend

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]
