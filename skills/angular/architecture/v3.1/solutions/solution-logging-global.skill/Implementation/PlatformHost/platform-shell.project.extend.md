---
description: Register the global ErrorHandler in apps/platform-shell so uncaught exceptions are captured and sent to the backend via LoggerService
name: platform-shell
project_kind: application
element_kind: project
change_kind: extend
tags:
  - solution/logging-global
  - element/platform-shell-project
---

# Goals

- Ensure every uncaught exception anywhere in the platform shell is routed through `LoggerService.error` and reaches `BackendLogSink`

# Structure

## Project Structure

```
/apps/platform-shell
  /src
    /app
      app.config.ts
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| app.config.ts | Extended: registers `GlobalErrorHandler` as the application-wide `ErrorHandler` provider. |

# Implementation changes

```typescript
// apps/platform-shell/src/app/app.config.ts
import { ApplicationConfig, ErrorHandler } from '@angular/core';
import { GlobalErrorHandler } from '@platform/logging/global';

export const appConfig: ApplicationConfig = {
  providers: [
    // ... existing providers
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
  ],
};
```

# Rules

## MUST
- `app.config.ts` must provide `GlobalErrorHandler` under the `ErrorHandler` token — no other error handler may silently swallow uncaught exceptions.

- `GlobalErrorHandler` must never be registered only in a module or component-level provider — it must be at the application root.
## SHOULD
- **Catching exceptions in the handler without sending them to the backend** — Consequence: production errors disappear, making incidents impossible to diagnose — Instead: always route through `LoggerService.error` so `BackendLogSink` receives them

# Check list

- [ ] `ErrorHandler` token is provided with `GlobalErrorHandler` in `app.config.ts`
- [ ] Uncaught exceptions in the platform shell reach the backend log sink

# Unittest TestCases

- [ ] WHEN an uncaught exception is thrown in the platform shell THEN
  - [ ] `GlobalErrorHandler.handleError` is invoked
  - [ ] `LoggerService.error` is called with the exception details
  - [ ] `BackendLogSink` eventually receives the entry
