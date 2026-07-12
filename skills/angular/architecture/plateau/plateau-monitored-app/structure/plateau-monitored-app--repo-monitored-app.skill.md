---
name: plateau-monitored-app--repo-monitored-app
description: Nx workspace layout for the monitored-app plateau — platform-monolith plus backend log delivery, batched and retried, and a global ErrorHandler ensuring every uncaught exception is captured
domain: skill
type: template
plateau: monitored-app
version: 20260711220000
tags:
  - skill/template/repo
  - plateau/monitored-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
---

> Fifth plateau in the main application's chain. Parent: [[skills/angular/architecture/plateau/plateau-platform-monolith/plateau-platform-monolith.skill|platform-monolith]]. Next: [[skills/angular/architecture/plateau/plateau-multiuser-app/plateau-multiuser-app.skill|multiuser-app]], the final plateau in the chain. This is the **"monitored-app"** milestone: the application's monitoring level is raised — `warn`/`error`/`report()` log entries now reach the backend, batched and resilient to transient network failures, and every uncaught exception is captured by a global `ErrorHandler` instead of silently crashing the app. Still no authentication (that arrives at `multiuser-app`).

# Structure

## Workspace Structure

```
/apps
  /[platform-shell](./platform-shell/plateau-monitored-app--project-platform-shell.skill.md)
  /[platform-shell-e2e](./platform-shell-e2e/plateau-monitored-app--project-platform-shell-e2e.skill.md)

/libs
  /shared
    /[ui](./shared-ui/plateau-monitored-app--project-shared-ui.skill.md)
    /[util](./shared-util/plateau-monitored-app--project-shared-util.skill.md)
    /[state](./shared-state/plateau-monitored-app--project-shared-state.skill.md)
    /[http-core](./shared-http-core/plateau-monitored-app--project-shared-http-core.skill.md)
    /[logging](./shared-logging/plateau-monitored-app--project-shared-logging.skill.md)
    /[offline-sync](./shared-offline-sync/plateau-monitored-app--project-shared-offline-sync.skill.md)
  /{feature}
    /[feature](./feature-feature/plateau-monitored-app--project-feature-feature.skill.md)
    /[data-access](./feature-data-access/plateau-monitored-app--project-feature-data-access.skill.md)
```

- No new top-level project — `libs/shared/logging` gains `BackendLogSink` and `LogRetryQueue`, `apps/platform-shell` gains `GlobalErrorHandler`. Everything else already existed at `platform-monolith` and is unchanged.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[skills/angular/architecture/plateau/plateau-monitored-app/structure/platform-shell/plateau-monitored-app--project-platform-shell.skill\|project-platform-shell]] | Composition root, now also registering `GlobalErrorHandler`. |
| /apps/platform-shell-e2e | [[skills/angular/architecture/plateau/plateau-monitored-app/structure/platform-shell-e2e/plateau-monitored-app--project-platform-shell-e2e.skill\|project-platform-shell-e2e]] | Playwright end-to-end scenario specs — unchanged. |
| /libs/shared/ui | [[skills/angular/architecture/plateau/plateau-monitored-app/structure/shared-ui/plateau-monitored-app--project-shared-ui.skill\|project-shared-ui]] | Reusable, app-specific UI — unchanged. |
| /libs/shared/util | [[skills/angular/architecture/plateau/plateau-monitored-app/structure/shared-util/plateau-monitored-app--project-shared-util.skill\|project-shared-util]] | Framework-agnostic pure helpers shared across features. |
| /libs/shared/state | [[skills/angular/architecture/plateau/plateau-monitored-app/structure/shared-state/plateau-monitored-app--project-shared-state.skill\|project-shared-state]] | Classical NgRx Store — unchanged. |
| /libs/shared/http-core | [[skills/angular/architecture/plateau/plateau-monitored-app/structure/shared-http-core/plateau-monitored-app--project-shared-http-core.skill\|project-shared-http-core]] | Base HTTP service and `OfflineTransportError` — unchanged. |
| /libs/shared/logging | [[skills/angular/architecture/plateau/plateau-monitored-app/structure/shared-logging/plateau-monitored-app--project-shared-logging.skill\|project-shared-logging]] | `LoggerService`, now with `BackendLogSink` and a persisted `LogRetryQueue` alongside `ConsoleLogSink`. |
| /libs/shared/offline-sync | [[skills/angular/architecture/plateau/plateau-monitored-app/structure/shared-offline-sync/plateau-monitored-app--project-shared-offline-sync.skill\|project-shared-offline-sync]] | Mutation queue and replay orchestrator — unchanged. |
| /libs/{feature}/feature | [[skills/angular/architecture/plateau/plateau-monitored-app/structure/feature-feature/plateau-monitored-app--project-feature-feature.skill\|project-feature-feature]] | Generic template — unchanged. |
| /libs/{feature}/data-access | [[skills/angular/architecture/plateau/plateau-monitored-app/structure/feature-data-access/plateau-monitored-app--project-feature-data-access.skill\|project-feature-data-access]] | Generic template — unchanged. |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/backend-log-sink.ts.create|Logging/backend-log-sink.ts.create]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

## Nx Tag Taxonomy

Unchanged from `platform-monolith`: `type` ∈ {`app`, `host`, `e2e`, `feature`, `data-access`, `ui`, `util`, `store`}, `scope` ∈ {`platform`, `shared`, `{feature-name}`}.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

## Cross-cutting conventions

These rules apply inside every project in the workspace and have no single project of their own to live in:

- **Three-tier state placement**, **hierarchical route ownership**, **Facade/Client/Mapper layering**, **selective preloading**, **bundle budgets**, **offline-aware reads/mutations**, **federation boundary** — unchanged from `platform-monolith`.
- **Single logging seam**: everything logs through `LoggerService`; `warn`/`error`/`report()` entries now also reach the backend, batched, with a persisted retry queue on failure — no call site needs to change.
- **Global error capture**: every uncaught exception anywhere in the platform shell is routed through `LoggerService.error` by `GlobalErrorHandler`, registered exactly once at the application root.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Logging/logger.service.ts.extend|Logging/logger.service.ts.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Rules

## MUST
- `BackendLogSink` MUST filter to `warn`/`error`/`report` entries only, discarding `debug`/`info` before they are ever buffered.
- On a failed flush, the batch MUST be handed to `LogRetryQueue.enqueue(...)` — never silently discarded.
- `app.config.ts` MUST provide `GlobalErrorHandler` under the `ErrorHandler` token.
- All other rules from [[skills/angular/architecture/plateau/plateau-platform-monolith/plateau-platform-monolith.skill|platform-monolith]] continue to apply unchanged.

## MUST NOT
- `GlobalErrorHandler` MUST NOT be registered only in a module or component-level provider — it must be at the application root.
- `LoggerService`/any sink MUST NOT ever be given a token, password, or PII value to log, regardless of level.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

- **Sending each entry as its own HTTP request instead of buffering**
  - Consequence: reintroduces the request-volume problem batching exists to solve
  - Instead: always buffer and flush on a timer/size threshold/unload, never per-entry
- **Catching exceptions in the handler without sending them to the backend**
  - Consequence: production errors disappear, making incidents impossible to diagnose
  - Instead: always route through `LoggerService.error` so `BackendLogSink` receives them

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

# Unittest TestCases

- [ ] WHEN a flush's HTTP call fails THEN
  - [ ] the batch is passed to `LogRetryQueue.enqueue(...)`, never dropped
- [ ] WHEN an uncaught exception is thrown anywhere in the platform shell THEN
  - [ ] `GlobalErrorHandler.handleError` is invoked, `LoggerService.error` is called, and `BackendLogSink` eventually receives the entry

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]
