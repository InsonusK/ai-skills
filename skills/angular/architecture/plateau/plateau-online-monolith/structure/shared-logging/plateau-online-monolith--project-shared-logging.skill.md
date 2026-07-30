---
name: plateau-online-monolith--project-shared-logging
description: LoggerService with a pluggable LogSink extension point — ConsoleLogSink is the only sink registered at this plateau. No backend delivery yet. — online-monolith plateau
domain: skill
type: template
plateau: online-monolith
project_kind: library
version: 20260711180000
tags:
  - skill/template/project
  - plateau/online-monolith
created_by:
  - "[[skills/angular/architecture/solutions/logging/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]]"
---

> No backend log delivery yet — `BackendLogSink` and its retry queue arrive with [[skills/angular/architecture/plateau/plateau-monitored-app/plateau-monitored-app.skill.md|monitored-app]].

# Goal

- Give every part of the application a single logging entry point, with structured entries and environment-based level filtering
- Provide an extension point (`LOG_SINKS`) a future backend-sending sink can be added to, without touching this project's console sink or any call site

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]

# Core Principles

- All logging goes through `LoggerService` — no direct `console.*` calls anywhere else in the application
- A log entry is always structured: a message plus a context object, never a single interpolated string
- `LoggerService` forwards every entry to a pluggable list of `LogSink`s
- Sensitive data (tokens, passwords, PII) is never logged, at any level

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]

# Structure

## Project Structure

```
/libs/shared/logging
  /src
    /lib
      [logger.service.ts](./classes/plateau-online-monolith--class-logger-service.skill.md)
      log-sink.ts
      console-log-sink.ts
      log-level.token.ts
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| logger.service.ts | Public API: `debug/info/warn/error/report(message, context?)`, plus `forFeature(name)`. Forwards each entry to every registered `LogSink`, after checking it against the configured minimum level. | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/shared-logging/classes/plateau-online-monolith--class-logger-service.skill\|class-logger-service]] |
| log-sink.ts | `LogSink` interface and the `LOG_SINKS` multi-provider injection token. | — |
| console-log-sink.ts | Writes each entry to the matching `console.*` method. Always registered. | — |
| log-level.token.ts | `MIN_LOG_LEVEL` injection token, `'debug'` in development, `'warn'` in production. | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]

## NPM Packages

None beyond Angular's own DI/core APIs.

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]

## What Does NOT Belong Here

- Feature-specific log formatting or routing rules — every feature uses `LoggerService.forFeature(name)`, not a bespoke wrapper
- Any network call — this plateau logs to the console only

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]

## Allowed Dependencies

- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]

# Rules

## MUST
- `LoggerService` MUST be the only class application code interacts with for logging; `ConsoleLogSink` MUST NOT be injected or called directly by feature code.
- `MIN_LOG_LEVEL` MUST default to filtering out `debug`/`info` in production builds, keeping `warn`/`error` always enabled.
- `LogEntry`'s `context` MUST be a plain, structured object.

## MUST NOT
- `LoggerService`/any sink MUST NOT ever be given a token, password, or PII value to log, regardless of level.

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]

# Anti-patterns

- **A feature registering its own ad hoc `console.log` wrapper instead of using `LoggerService.forFeature(...)`**
  - Consequence: recreates, inconsistently, exactly what `LoggerService` already provides, and won't be visible to a future backend sink
  - Instead: call `inject(LoggerService).forFeature('orders')` once per feature and use the returned logger

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]

# Check list

- [ ] `LoggerService` is the only logging entry point application-wide
- [ ] `MIN_LOG_LEVEL` filters `debug`/`info` out of production builds

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
