---
name: plateau-multiuser-app--class-global-error-handler
description: Global ErrorHandler catching every uncaught exception and routing it through LoggerService.error — multiuser-app plateau
domain: skill
type: template
plateau: multiuser-app
artifact_type: service
version: 20260711230000
tags:
  - skill/template/class
  - plateau/multiuser-app
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
---

# Goal

- Ensure uncaught, unhandled exceptions anywhere in the application are captured and sent to the backend automatically, without relying on every code path to log them explicitly

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/Implementation/PlatformHost/global-error-handler.ts.create|PlatformHost/global-error-handler.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Never pass the raw caught error object into the log context — extract only sanitized fields

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/Implementation/PlatformHost/global-error-handler.ts.create|PlatformHost/global-error-handler.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------------- | -------------------- | --------- |
| Global error handler | `GlobalErrorHandler` | `GlobalErrorHandler` | `global-error-handler.ts` | `global-error-handler.ts` |

# Implementation

```typescript
// Skill: class-global-error-handler
// Plateau: multiuser-app
// Version: 20260711230000

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

// app.config.ts
providers: [
  { provide: ErrorHandler, useClass: GlobalErrorHandler },
];
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/Implementation/PlatformHost/global-error-handler.ts.create|PlatformHost/global-error-handler.ts.create]]

# Rules

## MUST
- `GlobalErrorHandler` MUST route every caught exception through `LoggerService.error`, never swallow it silently.
- `GlobalErrorHandler` MUST NOT log the raw error object if it could contain sensitive data — only structured, sanitized fields (`message`, `stack`) are extracted.
- `GlobalErrorHandler` MUST be registered exactly once, in `apps/platform-shell`'s root providers.

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/Implementation/PlatformHost/global-error-handler.ts.create|PlatformHost/global-error-handler.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Passing the raw caught error object directly into the log context**
  - Consequence: if the error carries sensitive data in a custom property, it would be logged unsanitized
  - Instead: extract only `message`/`stack` (or other explicitly safe fields)

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/Implementation/PlatformHost/global-error-handler.ts.create|PlatformHost/global-error-handler.ts.create]]

# Check list

- [ ] Every uncaught exception reaches `LoggerService.error`
- [ ] Only sanitized fields are extracted from the caught error
- [ ] `GlobalErrorHandler` is registered exactly once, in `apps/platform-shell`

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/Implementation/PlatformHost/global-error-handler.ts.create|PlatformHost/global-error-handler.ts.create]]

# Unittest TestCases

- [ ] WHEN an uncaught exception is thrown anywhere in the application THEN
  - [ ] `GlobalErrorHandler.handleError` is invoked and calls `LoggerService.error` with the exception's message and stack
  - [ ] `BackendLogSink` eventually receives the entry

__Applied solutions:__
- [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/Implementation/PlatformHost/global-error-handler.ts.create|PlatformHost/global-error-handler.ts.create]]
- [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/logging/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
