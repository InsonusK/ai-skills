---
description: Extend libs/shared/logging with BackendLogSink and the persisted retry queue, and register a global ErrorHandler in apps/platform-shell
element_kind: repository
change_kind: extend
tags:
  - solution/logging-global
  - element/repository
---

# Structure

No new top-level directories. This extends [[skills/angular/architecture/v3.1/solutions/logging/solution-logging-base.skill/Implementation/Logging/shared-logging.project.create|libs/shared/logging]] (adding `BackendLogSink` and its retry queue) and `apps/platform-shell` (registering a global `ErrorHandler`).

## Directory and project skills

| Directory/file | Description |
| --------------- | ----------- |
| /libs/shared/logging/src/lib/backend-log-sink.ts | New: the second `LogSink`, registered alongside `ConsoleLogSink` from the base solution via the same `LOG_SINKS` multi-provider token — no existing call site changes. |
| /libs/shared/logging/src/lib/log-retry-queue.ts | New: IndexedDB-backed, bounded retry queue used by `BackendLogSink` when a batch send fails. |
| /apps/platform-shell/src/global-error-handler.ts | New: `ErrorHandler` implementation routing every uncaught exception through `LoggerService.error`. |

# Rules

## MUST
- `BackendLogSink` MUST be registered via the same `LOG_SINKS` multi-provider token the base solution already established — it MUST NOT require changing `ConsoleLogSink` or any existing `LoggerService` call site.
- The global `ErrorHandler` MUST be registered in `apps/platform-shell` via `{ provide: ErrorHandler, useClass: GlobalErrorHandler }`, so it applies to the whole application (and, per the platform-embeddability solution, to embeddable apps sharing the same Angular runtime).
- `BackendLogSink` MUST only forward `warn`/`error` level entries and explicit `report()` calls to the backend — `debug`/`info` entries MUST NOT reach this sink, per [[skills/angular/architecture/v3.1/solutions/logging/solution-logging-global.skill/adr/backend-log-sink-strategy.md|backend-log-sink-strategy]].

# Unittest TestCases

- [ ] WHEN an uncaught exception occurs anywhere in the application THEN
  - [ ] `GlobalErrorHandler` routes it through `LoggerService.error`, and it reaches `BackendLogSink`
- [ ] WHEN a `debug`/`info` entry is logged THEN
  - [ ] it never reaches `BackendLogSink`, regardless of environment

## SHOULD
- **Wiring `BackendLogSink` to also forward `debug`/`info` entries "to have more data"** — Consequence: reintroduces the noise/cost problem this solution's ADR exists to avoid — Instead: keep `debug`/`info` local to `ConsoleLogSink`; use an explicit `report()` call for anything at a lower severity that's still worth sending
