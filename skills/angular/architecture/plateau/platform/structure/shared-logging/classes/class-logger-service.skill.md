---
name: class-logger-service
description: The single logging entry point for the whole application — structured entries, level filtering, forwarding to every registered sink, plus an explicit report() level
domain: skill
type: template
plateau: platform
artifact_type: service
version: 20260711150000
tags:
  - skill/template/class
  - plateau/platform
created_by:
  - "[[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]]"
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
---

> The base `debug/info/warn/error/forFeature` API is established implicitly by [[../project-shared-logging.skill.md|project-shared-logging]]'s own directory table. `solution-logging-global` explicitly extends it with `report()`.

# Goal

- Give every part of the application a single logging entry point, with structured entries and environment-based level filtering
- Let call sites intentionally mark a non-error event as worth sending to the backend

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- `context` is always a plain, structured object
- `report()` bypasses `MIN_LOG_LEVEL` filtering

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Logging service | `LoggerService` | `LoggerService` | `logger.service.ts` | `logger.service.ts` |

# Implementation

```typescript
// Skill: class-logger-service
// Plateau: platform
// Version: 20260711150000

@Injectable({ providedIn: 'root' })
export class LoggerService {
  debug(message: string, context?: Record<string, unknown>): void { /* ... */ }
  info(message: string, context?: Record<string, unknown>): void { /* ... */ }
  warn(message: string, context?: Record<string, unknown>): void { /* ... */ }
  error(message: string, context?: Record<string, unknown>): void { /* ... */ }
  report(message: string, context?: Record<string, unknown>): void {
    this.dispatch({ level: 'report', message, context, timestamp: Date.now() });
  }
  forFeature(name: string): LoggerService { /* ... */ }
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Rules

## MUST
- `LoggerService` MUST be the only class application code interacts with for logging.
- `context` MUST be a plain, structured object.
- `report()` entries MUST always reach `BackendLogSink`, regardless of `MIN_LOG_LEVEL`.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|Logging/shared-logging.project.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Using `report()` as a substitute for `error()` to bypass expected error-handling conventions**
  - Consequence: blurs severity levels
  - Instead: use `error()` for actual failures

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Check list

- [ ] Every log entry's context is a structured object
- [ ] `report()` calls always reach `BackendLogSink`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]

# Unittest TestCases

- [ ] WHEN `LoggerService.debug(...)` is called with `MIN_LOG_LEVEL` set to `'warn'` THEN
  - [ ] no registered sink receives the entry
- [ ] WHEN `report(...)` is called in a production build THEN
  - [ ] the entry still reaches `BackendLogSink`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]
