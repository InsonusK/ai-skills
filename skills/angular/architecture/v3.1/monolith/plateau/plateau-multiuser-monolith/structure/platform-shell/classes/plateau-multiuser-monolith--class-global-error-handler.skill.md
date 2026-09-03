---
name: plateau-multiuser-monolith--class-global-error-handler
description: GlobalErrorHandler in apps/platform-shell — the application-root ErrorHandler routing every uncaught exception through LoggerService.error with only sanitized fields — multiuser-monolith plateau
domain: skill
type: template
plateau: multiuser-monolith
artifact_type: service
version: 20260903150000
tags:
  - skill/template/class
  - plateau/multiuser-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]]"

> `apps/platform-shell/src/app/global-error-handler.ts`. Registered once as `{ provide: ErrorHandler, useClass: GlobalErrorHandler }` in `app.config.ts`.

# Goal

- Capture every uncaught, unhandled exception and send it to the backend automatically, without every code path logging it explicitly

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/PlatformHost/global-error-handler.ts.create.md|PlatformHost/global-error-handler.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Routes every caught exception through `LoggerService.error` — never swallows it silently
- Extracts only `message` / `stack` — never the raw error object (it could carry a token in a custom property)
- Registered once, at the application root — never module- or component-scoped

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/PlatformHost/global-error-handler.ts.create.md|PlatformHost/global-error-handler.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Global error handler | `GlobalErrorHandler` | `GlobalErrorHandler` | `global-error-handler.ts` | `global-error-handler.ts` |

# Implementation

```typescript
// Skill: class-global-error-handler
// Plateau: multiuser-monolith
// Version: 20260903150000

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private readonly logger = inject(LoggerService);
  handleError(error: unknown): void {
    this.logger.error('Uncaught exception', {
      message: error instanceof Error ? error.message : String(error),
      stack: error instanceof Error ? error.stack : undefined,
    });
  }
}
// app.config.ts:  { provide: ErrorHandler, useClass: GlobalErrorHandler }
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/PlatformHost/global-error-handler.ts.create.md|PlatformHost/global-error-handler.ts.create]]

# Rules

## MUST
- Route every caught exception through `LoggerService.error` — never swallow it silently.
- Extract only `message` / `stack` — never pass the raw error object into the log context.
- Register exactly once, in `apps/platform-shell`'s root providers.
- Never apply several plateau templates per class/artifact.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/PlatformHost/global-error-handler.ts.create.md|PlatformHost/global-error-handler.ts.create]]

# Check list

- [ ] Every uncaught exception reaches `LoggerService.error`
- [ ] Only `message` / `stack` are extracted, never the raw object
- [ ] Registered exactly once, in `apps/platform-shell`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/PlatformHost/global-error-handler.ts.create.md|PlatformHost/global-error-handler.ts.create]]

# Unittest TestCases

- [ ] WHEN an uncaught `Error` is thrown THEN `handleError` calls `LoggerService.error` with its `message` and `stack`
- [ ] WHEN a non-Error value is thrown THEN it is stringified into `message` with `stack: undefined`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/PlatformHost/global-error-handler.ts.create.md|PlatformHost/global-error-handler.ts.create]]
