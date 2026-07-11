---
name: repo-observable
description: Nx workspace layout for the observable Angular application — adds libs/shared/logging (LoggerService, pluggable LogSinks, backend sink with retry queue) and a global ErrorHandler in apps/platform-shell, on top of the authenticated plateau's session, routing, forms, and data-access conventions
domain: skill
type: template
plateau: observable
version: 20260711160000
tags:
  - skill/template/repo
  - plateau/observable
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
  - "[[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]]"
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
---

> **Deferred scope:** carried over unchanged from [[skills/angular/architecture/plateau/navigable/plateau-navigable.skill.md|navigable]] and [[skills/angular/architecture/plateau/authenticated/plateau-authenticated.skill.md|authenticated]] — `solution-app-routing`'s embeddable-module routing slice and `solution-authentication`'s `@platform/contracts` session-sharing slice both remain excluded, deferred to a future "platform" plateau. Neither `solution-logging-base` nor `solution-logging-global` touch Module Federation at all, so they introduce no new deferred scope of their own — `GlobalErrorHandler`, once registered in `apps/platform-shell`, is noted by `solution-logging-global` as applying "to embeddable apps sharing the same Angular runtime" once the platform plateau exists, but nothing about that requires excluding any part of this plateau's own scope.

# Structure

## Workspace Structure

```
/apps
  /[platform-shell](./platform-shell/project-platform-shell.skill.md)

/libs
  /shared
    /[ui](./shared-ui/project-shared-ui.skill.md)
    /[util](./shared-util/project-shared-util.skill.md)
    /[state](./shared-state/project-shared-state.skill.md)
    /[http-core](./shared-http-core/project-shared-http-core.skill.md)
    /[auth-ui](./shared-auth-ui/project-shared-auth-ui.skill.md)
    /[logging](./shared-logging/project-shared-logging.skill.md)      <- new (solution-logging-base, solution-logging-global)
  /{feature}
    /[feature](./feature-feature/project-feature-feature.skill.md)
    /[data-access](./feature-data-access/project-feature-data-access.skill.md)
```

- `libs/shared/logging` hosts `LoggerService`, the `LogSink` extension point, `ConsoleLogSink` (always active), and `BackendLogSink` + `LogRetryQueue` (warn/error/report only, batched, retried). Tagged `type:util`, `scope:shared`.
- `apps/platform-shell` now also registers `GlobalErrorHandler` as the application-wide `ErrorHandler`.

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[platform-shell/project-platform-shell.skill.md\|project-platform-shell.skill]] | The only deployable unit at this plateau. Composition root: bootstraps the app, owns top-level routing, the selective preloading strategy, root providers (auth interceptor, global error handler). Contains no business logic of its own. |
| /libs/shared/ui | [[shared-ui/project-shared-ui.skill.md\|project-shared-ui.skill]] | Reusable, app-specific UI composed from design-system primitives. |
| /libs/shared/util | [[shared-util/project-shared-util.skill.md\|project-shared-util.skill]] | Framework-agnostic pure helpers shared across features. |
| /libs/shared/state | [[shared-state/project-shared-state.skill.md\|project-shared-state.skill]] | Classical NgRx Store for global, cross-cutting state, including the full auth session lifecycle. |
| /libs/shared/http-core | [[shared-http-core/project-shared-http-core.skill.md\|project-shared-http-core.skill]] | Base HTTP service shared by every feature's Client. |
| /libs/shared/auth-ui | [[shared-auth-ui/project-shared-auth-ui.skill.md\|project-shared-auth-ui.skill]] | Permission-based UI primitives (`*hasPermission`). |
| /libs/shared/logging | [[shared-logging/project-shared-logging.skill.md\|project-shared-logging.skill]] | `LoggerService`, pluggable `LogSink`s (`ConsoleLogSink`, `BackendLogSink`), and the bounded retry queue backing the backend sink. |
| /libs/{feature}/feature | [[feature-feature/project-feature-feature.skill.md\|project-feature-feature.skill]] | Generic template: routed, presentational + container components (including forms) for one feature, its feature-level Signal Store, its own root-relative routes (with lazy sub-splitting and permission guards). |
| /libs/{feature}/data-access | [[feature-data-access/project-feature-data-access.skill.md\|project-feature-data-access.skill]] | Generic template: this feature's Facade/Client/Mapper/Errors layering for HTTP data operations. |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

## Nx Tag Taxonomy

No new tag values are introduced; `libs/shared/logging` uses the existing `type:util`/`scope:shared` combination.

`@nx/enforce-module-boundaries` allow-list is unchanged from [[skills/angular/architecture/plateau/authenticated/plateau-authenticated.skill.md|authenticated]].

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

## Logging conventions (cross-cutting convention)

- Every part of the application logs through `LoggerService` — no direct `console.*` call is permitted outside `libs/shared/logging`'s own `ConsoleLogSink` implementation.
- A log entry MUST NOT contain an auth token, password, or other PII in its message or context, regardless of log level.
- Only `warn`/`error`/`report()` entries reach `BackendLogSink`; `debug`/`info` stay local to `ConsoleLogSink`.
- `GlobalErrorHandler` is registered exactly once, in `apps/platform-shell`, and captures every uncaught exception in the application.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

## Session, authorization, routing, forms, data-access conventions

Unchanged from [[skills/angular/architecture/plateau/authenticated/plateau-authenticated.skill.md|authenticated]].

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]

# Rules

## MUST
- Every Nx project MUST declare exactly one `type:*` tag and exactly one `scope:*` tag.
- Every lib MUST expose its public API through a single `index.ts` barrel.
- Business logic MUST NOT live in `apps/platform-shell`.
- Every routable `type:feature` project MUST export its `Routes` array from `index.ts`.
- New forms MUST use Signal Forms by default.
- A Client MUST catch every `HttpErrorResponse` and rethrow a typed domain error.
- The access token MUST only ever be held in the `shared-state` auth slice's in-memory field.
- A permission guard MUST be attached inside the feature's own routes.
- Every part of the application MUST log through `LoggerService` — no direct `console.*` call outside `libs/shared/logging`'s own `ConsoleLogSink`.
- A log entry MUST NOT contain an auth token, password, or other PII, regardless of log level.
- `BackendLogSink` MUST be registered via the same `LOG_SINKS` multi-provider token the base solution established.
- `BackendLogSink` MUST only forward `warn`/`error` entries and explicit `report()` calls — `debug`/`info` MUST NOT reach it.
- On a failed flush, the batch MUST be handed to `LogRetryQueue.enqueue(...)` — never silently discarded.
- The unload flush MUST use `navigator.sendBeacon`, not a regular `fetch`/`HttpClient` call.
- The retry queue MUST enforce all three limits (count, age, size) independently.
- The global `ErrorHandler` MUST be registered in `apps/platform-shell` via `{ provide: ErrorHandler, useClass: GlobalErrorHandler }`.
- `GlobalErrorHandler` MUST route every caught exception through `LoggerService.error`, never swallow it silently.
- `GlobalErrorHandler` MUST NOT log the raw error object — only sanitized fields (`message`, `stack`).

## SHOULD
- New business features SHOULD be scaffolded as a `{feature}/feature` + `{feature}/data-access` pair from the start.
- The retry queue's three limit values SHOULD be configurable per deployment, not hardcoded.

## SHOULD NOT
- Existing Reactive Forms code SHOULD NOT be migrated to Signal Forms purely for consistency.

## MUST NOT
- MUST NOT place a routed business feature directly under `/apps`.
- A component or Signal Store method MUST NOT import a feature's Client directly, bypassing the Facade.
- A feature MUST NOT set `preload: true` on its own routes.
- `accessToken` MUST NOT be written to any persistent client storage.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/global-error-handler.ts.create|PlatformHost/global-error-handler.ts.create]]

# Anti-patterns

- **Single flat lib per feature instead of `feature` + `data-access` split**
  - Consequence: UI and HTTP/data concerns become entangled
  - Instead: always split into at least `feature` and `data-access` from the start
- **Calling `console.log`/`console.warn`/`console.error` directly from feature code**
  - Consequence: bypasses the single seam the backend-sending extension relies on, and bypasses environment-based level filtering and the PII safeguard `LoggerService` centralizes
  - Instead: always call `LoggerService`, even for a quick debug print during development
- **Interpolating a token, password, or other sensitive value into a log message "just for local debugging"**
  - Consequence: sensitive data can end up in browser devtools history, shared screenshots, or the persisted backend log store
  - Instead: log a non-sensitive identifier (e.g. a user ID) instead of the sensitive value itself
- **Wiring `BackendLogSink` to also forward `debug`/`info` entries "to have more data"**
  - Consequence: reintroduces the noise/cost problem the backend sink's ADR exists to avoid
  - Instead: keep `debug`/`info` local to `ConsoleLogSink`; use `report()` for anything at a lower severity that's still worth sending
- **Letting the retry queue grow without any of the three bounds enforced**
  - Consequence: an extended backend outage could let the queue consume unbounded IndexedDB storage
  - Instead: always check and evict against all three limits after every enqueue
- **Passing the raw caught error object directly into the log context**
  - Consequence: if the error carries sensitive data in a custom property, it would be logged unsanitized
  - Instead: extract only `message`/`stack`, never the whole error object

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/log-retry-queue.ts.create|Logging/log-retry-queue.ts.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/global-error-handler.ts.create|PlatformHost/global-error-handler.ts.create]]

# Unittest TestCases

- [ ] WHEN `nx run-many -t lint` is executed THEN
  - [ ] `@nx/enforce-module-boundaries` reports no violations
- [ ] WHEN the codebase is searched for direct `console.*` calls outside `libs/shared/logging` THEN
  - [ ] none are found
- [ ] WHEN an uncaught exception occurs anywhere in the application THEN
  - [ ] `GlobalErrorHandler` routes it through `LoggerService.error`, and it reaches `BackendLogSink`
- [ ] WHEN a `debug`/`info` entry is logged THEN
  - [ ] it never reaches `BackendLogSink`, regardless of environment

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]
