---
description: LoggerService, the LogSink extension point, and the console sink registered by this base solution
name: shared-logging
project_kind: library
element_kind: project
change_kind: create
tags:
  - solution/logging-base
  - element/shared-logging-project
---

# Goals

- Give every part of the application a single logging entry point, with structured entries and environment-based level filtering
- Provide an extension point (`LOG_SINKS`) the future "Логирование (глобальное)" solution can add a backend sink to, without touching this project or any call site

# Structure

## Project Structure

```
/libs/shared/logging
  /src
    /lib
      logger.service.ts
      log-sink.ts
      console-log-sink.ts
      log-level.token.ts
    index.ts
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| logger.service.ts | Public API: `debug/info/warn/error(message, context?)`, plus a `forFeature(name)` factory returning a logger that auto-attaches `{ feature: name }` to every entry's context. Forwards each entry to every registered `LogSink`, after checking it against the configured minimum level. |
| log-sink.ts | `LogSink` interface (`write(entry: LogEntry): void`) and the `LOG_SINKS` multi-provider injection token. This is the seam the future backend-logging solution extends. |
| console-log-sink.ts | This solution's only registered sink: writes each entry to the matching `console.*` method, formatting the structured context alongside the message. |
| log-level.token.ts | `MIN_LOG_LEVEL` injection token, provided per environment: `'debug'` in development, `'warn'` in production — filtering out `debug`/`info` in production builds. |

# NPM Packages

None beyond Angular's own DI/core APIs.

# Rules

## MUST
- `LoggerService` MUST be the only class application code interacts with for logging; `ConsoleLogSink` MUST NOT be injected or called directly by feature code.
- `MIN_LOG_LEVEL` MUST default to filtering out `debug`/`info` in production builds, keeping `warn`/`error` always enabled.
- `LogEntry`'s `context` MUST be a plain, structured object — never a pre-formatted string standing in for context.

## MUST NOT
- `LoggerService`/`ConsoleLogSink` MUST NOT ever be given a token, password, or PII value to log, regardless of level (see [[skills/angular/architecture/solutions/logging/solution-logging-base.skill/Implementation/Repository.extend#MUST]]).

# Anti-patterns

- **A feature registering its own ad hoc `console.log` wrapper instead of using `LoggerService.forFeature(...)`**
  - Consequence: recreates, inconsistently, exactly what `LoggerService` already provides, and is invisible to the future backend sink
  - Instead: call `inject(LoggerService).forFeature('orders')` once per feature and use the returned logger

# Check list

- [ ] `LoggerService` is the only logging entry point application-wide
- [ ] `MIN_LOG_LEVEL` filters `debug`/`info` out of production builds
- [ ] Every log entry's context is a structured object, not a pre-formatted string

# Unittest TestCases

- [ ] WHEN `LoggerService.debug(...)` is called with `MIN_LOG_LEVEL` set to `'warn'` THEN
  - [ ] no registered sink receives the entry
- [ ] WHEN `LoggerService.error(...)` is called THEN
  - [ ] every registered `LogSink` receives the entry, regardless of `MIN_LOG_LEVEL`
- [ ] WHEN `forFeature('orders')` is used to log THEN
  - [ ] the resulting entry's context includes `{ feature: 'orders' }` merged with any additional context passed at the call site
