---
description: Extend LoggerService (from the base logging solution) with an explicit report() level, always sent to the backend regardless of severity
project_name: shared-logging
name: logger
element_kind: service
change_kind: extend
tags:
  - solution/logging-global
  - element/logger-service-ts
---

# Goals

- Let call sites intentionally mark a non-error event as worth sending to the backend, without inflating it to `warn`/`error` severity it doesn't actually have

# Implementation changes

```typescript
// logger.service.ts — additional method
export class LoggerService {
  // ...existing debug/info/warn/error from the base solution...

  report(message: string, context?: Record<string, unknown>): void {
    this.dispatch({ level: 'report', message, context, timestamp: Date.now() });
  }
}
```

# Rule changes

## MUST
- `report()` entries always reach `BackendLogSink`, regardless of `MIN_LOG_LEVEL`.
  - Risk: if `report()` were level-filtered like `info`, a deliberate "send this to the backend" call would be silently dropped in production.
  - Fix: `if (level !== 'report' && ORDER[level] < ORDER[min]) return;` — `report` bypasses the check; per [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/adr/backend-log-sink-strategy.md|backend-log-sink-strategy]].
- `report()` is still subject to the never-log-sensitive-data rule.
  - Risk: "it goes to the backend anyway" invites logging a token or PII through `report()`.
  - Fix: same restriction as every other level — identifiers and shapes only.

## SHOULD
- **Using `report()` as a substitute for `error()` to bypass expected error-handling conventions** — Consequence: blurs the meaning of severity levels, making it harder to distinguish genuine failures from intentional business events in backend log queries — Instead: use `error()` for actual failures; reserve `report()` for deliberate, non-error events worth tracking

# Check list

- [ ] `report()` calls always reach `BackendLogSink`, independent of environment-based level filtering
- [ ] No `report()` call includes sensitive data

# Unittest TestCases

- [ ] WHEN `report(...)` is called in a production build (where `debug`/`info` are filtered) THEN
  - [ ] the entry still reaches `BackendLogSink`
