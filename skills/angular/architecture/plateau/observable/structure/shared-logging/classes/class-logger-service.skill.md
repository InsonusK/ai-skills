---
name: class-logger-service
description: Central logging entry point — structured debug/info/warn/error/report calls forwarded to every registered LogSink, with environment-based level filtering
domain: skill
type: template
plateau: observable
artifact_type: service
version: 20260711160000
tags:
  - skill/template/class
  - plateau/observable
created_by:
  - "[[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]]"
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
---

> Introduced (as project-level description only, no dedicated implementation file) by `solution-logging-base`; `solution-logging-global` is the first solution to give `logger.service.ts` its own artifact-level implementation file, adding `report()`.

# Goal

- Give every part of the application a single logging entry point, with structured entries and environment-based level filtering
- Let call sites intentionally mark a non-error event as worth sending to the backend, without inflating it to `warn`/`error` severity it doesn't actually have

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- A log entry is always structured: a message plus a context object, never a single interpolated string
- `report()` bypasses `MIN_LOG_LEVEL` filtering — it always reaches the backend sink, unlike `debug`/`info`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Logger service | `LoggerService` | `LoggerService` | `logger.service.ts` | `logger.service.ts` |

# Implementation

```typescript
// Skill: class-logger-service
// Plateau: observable
// Version: 20260711160000

@Injectable({ providedIn: 'root' })
export class LoggerService {
  constructor(
    @Inject(LOG_SINKS) private readonly sinks: LogSink[],
    @Inject(MIN_LOG_LEVEL) private readonly minLevel: LogLevel,
  ) {}

  debug(message: string, context?: Record<string, unknown>): void {
    this.dispatch({ level: 'debug', message, context, timestamp: Date.now() });
  }
  info(message: string, context?: Record<string, unknown>): void {
    this.dispatch({ level: 'info', message, context, timestamp: Date.now() });
  }
  warn(message: string, context?: Record<string, unknown>): void {
    this.dispatch({ level: 'warn', message, context, timestamp: Date.now() });
  }
  error(message: string, context?: Record<string, unknown>): void {
    this.dispatch({ level: 'error', message, context, timestamp: Date.now() });
  }

  // report() always reaches every sink, bypassing MIN_LOG_LEVEL filtering
  report(message: string, context?: Record<string, unknown>): void {
    this.dispatch({ level: 'report', message, context, timestamp: Date.now() });
  }

  forFeature(feature: string): LoggerService {
    // returns a logger that auto-attaches { feature } to every entry's context
    return this; // illustrative — real implementation wraps dispatch with merged context
  }

  private dispatch(entry: LogEntry): void {
    if (entry.level !== 'report' && !this.passesMinLevel(entry.level)) return;
    for (const sink of this.sinks) sink.write(entry);
  }

  private passesMinLevel(level: LogLevel): boolean { /* ... */ return true; }
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Rules

## MUST
- `LoggerService` MUST be the only class application code interacts with for logging; sinks MUST NOT be injected or called directly by feature code.
- `MIN_LOG_LEVEL` MUST default to filtering out `debug`/`info` in production builds, keeping `warn`/`error` always enabled.
- `LogEntry`'s `context` MUST be a plain, structured object — never a pre-formatted string.
- `report()` entries MUST always reach `BackendLogSink`, regardless of `MIN_LOG_LEVEL`.
- `report()` MUST still be subject to the never-log-sensitive-data rule — the same PII/token restrictions apply.

## MUST NOT
- `LoggerService` MUST NOT ever be given a token, password, or PII value to log, regardless of level.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **A feature registering its own ad hoc `console.log` wrapper instead of using `LoggerService.forFeature(...)`**
  - Consequence: recreates, inconsistently, exactly what `LoggerService` already provides
  - Instead: call `inject(LoggerService).forFeature('orders')` once per feature
- **Using `report()` as a substitute for `error()` to bypass expected error-handling conventions**
  - Consequence: blurs the meaning of severity levels, making it harder to distinguish genuine failures from intentional business events in backend log queries
  - Instead: use `error()` for actual failures; reserve `report()` for deliberate, non-error events worth tracking

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Check list

- [ ] `LoggerService` is the only logging entry point application-wide
- [ ] `MIN_LOG_LEVEL` filters `debug`/`info` out of production builds
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
- [ ] WHEN `forFeature('orders')` is used to log THEN
  - [ ] the resulting entry's context includes `{ feature: 'orders' }` merged with any additional context passed at the call site
- [ ] WHEN `report(...)` is called in a production build (where `debug`/`info` are filtered) THEN
  - [ ] the entry still reaches `BackendLogSink`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]
