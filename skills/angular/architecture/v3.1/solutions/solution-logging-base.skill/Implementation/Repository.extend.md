---
description: Add libs/shared/logging hosting LoggerService, the LogSink extension point, and the console sink used by this base solution
element_kind: repository
change_kind: extend
tags:
  - solution/logging-base
  - element/repository
---

# Structure

## Workspace Structure

```
/libs
  /shared
    /ui
    /util
    /state
    /http-core
    /logging        <- new
```

## Directory and project skills

| Directory | Description |
| ---------- | ----------- |
| /libs/shared/logging | `LoggerService`, the `LogSink` interface, the `LOG_SINKS` multi-provider token, and `ConsoleLogSink` (this solution's only registered sink). Tagged `type:util`, `scope:shared`. The future `solution-logging-global` extends this project by adding a `BackendLogSink`, without changing anything here. |

# Rules

## MUST
- Every part of the application MUST log through `LoggerService` — no direct `console.*` call is permitted outside `libs/shared/logging`'s own `ConsoleLogSink` implementation.
- A log entry MUST NOT contain an auth token, password, or other PII in its message or context, regardless of log level — this applies unconditionally, per `solution-authentication`'s token-handling rules.

# Unittest TestCases

- [ ] WHEN the codebase is searched for direct `console.*` calls outside `libs/shared/logging` THEN
  - [ ] none are found

## SHOULD
- **Calling `console.log`/`console.warn`/`console.error` directly from feature code** — Consequence: bypasses the single seam the future backend-sending extension relies on, and bypasses the environment-based level filtering and PII safeguard `LoggerService` centralizes — Instead: always call `LoggerService`, even for a quick debug print during development
- **Interpolating a token, password, or other sensitive value into a log message "just for local debugging"** — Consequence: sensitive data can end up in browser devtools history, shared screenshots, or — once the backend sink exists — in a persisted log store — Instead: log a non-sensitive identifier (e.g. a user ID) instead of the sensitive value itself
