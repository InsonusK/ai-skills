---
description: Global ErrorHandler catching every uncaught exception and routing it through LoggerService.error, so it reaches BackendLogSink like any other error-level log
project_name: platform-shell
name: global-error-handler
element_kind: service
change_kind: create
tags:
  - solution/logging-global
  - element/global-error-handler-ts
---

# Goals

- Ensure uncaught, unhandled exceptions anywhere in the application are captured and sent to the backend automatically, without relying on every code path to log them explicitly

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------------- | -------------------- | --------- |
| Global error handler | GlobalErrorHandler | GlobalErrorHandler | global-error-handler.ts | global-error-handler.ts |

# Implementation changes

```typescript
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  constructor(private readonly logger: LoggerService) {}

  handleError(error: unknown): void {
    this.logger.error('Uncaught exception', {
      message: error instanceof Error ? error.message : String(error),
      stack: error instanceof Error ? error.stack : undefined,
    });
  }
}
```

```typescript
// app.config.ts
providers: [
  { provide: ErrorHandler, useClass: GlobalErrorHandler },
];
```

# Rule changes

## MUST
- `GlobalErrorHandler` MUST route every caught exception through `LoggerService.error`, never swallow it silently.
- `GlobalErrorHandler` MUST NOT log the raw error object if it could contain sensitive data (e.g. an error thrown with a token embedded in its message) — only structured, sanitized fields (`message`, `stack`) are extracted, never the original error object passed through unchanged.
- `GlobalErrorHandler` MUST be registered once, in `apps/platform-shell`'s root providers, applying to the whole running application (including embeddable apps sharing the same Angular runtime, per the platform-embeddability solution).

# Anti-patterns

- **Passing the raw caught error object directly into the log context**
  - Consequence: if the error happens to carry sensitive data in a custom property, it would be logged unsanitized
  - Instead: extract only `message`/`stack` (or other explicitly safe fields), never the whole error object

# Check list

- [ ] Every uncaught exception reaches `LoggerService.error`
- [ ] Only sanitized fields (`message`, `stack`) are extracted from the caught error, never the raw object
- [ ] `GlobalErrorHandler` is registered exactly once, in `apps/platform-shell`

# Unittest TestCases

- [ ] WHEN an uncaught exception is thrown anywhere in the application THEN
  - [ ] `GlobalErrorHandler.handleError` is invoked and calls `LoggerService.error` with the exception's message and stack
