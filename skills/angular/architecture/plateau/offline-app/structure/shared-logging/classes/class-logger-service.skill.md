---
name: class-logger-service
description: The single logging entry point for the whole application — structured entries, level filtering, forwarding to every registered sink, plus an explicit report() level
domain: skill
type: template
plateau: offline-app
artifact_type: service
version: 20260711140000
tags:
  - skill/template/class
  - plateau/offline-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]]"
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
---

> The base `debug/info/warn/error/forFeature` API is established implicitly by [[../project-shared-logging.skill.md|project-shared-logging]]'s own directory table (no separate base `.create` file exists). `solution-logging-global` explicitly extends it with `report()`.

# Goal

- Give every part of the application a single logging entry point, with structured entries and environment-based level filtering
- Let call sites intentionally mark a non-error event as worth sending to the backend, without inflating it to `warn`/`error` severity it doesn't actually have

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- `context` is always a plain, structured object — never a pre-formatted string
- `report()` bypasses `MIN_LOG_LEVEL` filtering — it always reaches `BackendLogSink`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Logging service | `LoggerService` | `LoggerService` | `logger.service.ts` | `logger.service.ts` |

# Implementation

```typescript
// Skill: class-logger-service
// Plateau: offline-app
// Version: 20260711140000

@Injectable({ providedIn: 'root' })
export class LoggerService {
  debug(message: string, context?: Record<string, unknown>): void { /* ... */ }
  info(message: string, context?: Record<string, unknown>): void { /* ... */ }
  warn(message: string, context?: Record<string, unknown>): void { /* ... */ }
  error(message: string, context?: Record<string, unknown>): void { /* ... */ }

  // solution-logging-global addition — always sent to the backend, independent of MIN_LOG_LEVEL
  report(message: string, context?: Record<string, unknown>): void {
    this.dispatch({ level: 'report', message, context, timestamp: Date.now() });
  }

  forFeature(name: string): LoggerService {
    // returns a logger that auto-attaches { feature: name } to every entry's context
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Rules

## MUST
- `LoggerService` MUST be the only class application code interacts with for logging.
- `LogEntry`'s `context` MUST be a plain, structured object — never a pre-formatted string.
- `report()` entries MUST always reach `BackendLogSink`, regardless of `MIN_LOG_LEVEL`.
- `report()` MUST still be subject to the never-log-sensitive-data rule.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Using `report()` as a substitute for `error()` to bypass expected error-handling conventions**
  - Consequence: blurs the meaning of severity levels, making it harder to distinguish genuine failures from intentional business events
  - Instead: use `error()` for actual failures; reserve `report()` for deliberate, non-error events worth tracking

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Check list

- [ ] Every log entry's context is a structured object, not a pre-formatted string
- [ ] `report()` calls always reach `BackendLogSink`, independent of environment-based level filtering
- [ ] No `report()` call includes sensitive data

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Unittest TestCases

- [ ] WHEN `LoggerService.debug(...)` is called with `MIN_LOG_LEVEL` set to `'warn'` THEN
  - [ ] no registered sink receives the entry
- [ ] WHEN `LoggerService.error(...)` is called THEN
  - [ ] every registered `LogSink` receives the entry, regardless of `MIN_LOG_LEVEL`
- [ ] WHEN `report(...)` is called in a production build (where `debug`/`info` are filtered) THEN
  - [ ] the entry still reaches `BackendLogSink`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]
