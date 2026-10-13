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
- `app.config.ts` provides `GlobalErrorHandler` under the `ErrorHandler` token.
  - Risk: leaving Angular's default `ErrorHandler` (or another custom one) means uncaught exceptions only reach the console — invisible to production diagnostics.
  - Fix: `{ provide: ErrorHandler, useClass: GlobalErrorHandler }` in the root `providers`.
- `GlobalErrorHandler` is registered only at the application root, never in a module- or component-level provider.
  - Risk: a scoped handler covers only that injector's subtree; exceptions elsewhere are unhandled.
  - Fix: one registration in `apps/platform-shell/src/app/app.config.ts`.
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
