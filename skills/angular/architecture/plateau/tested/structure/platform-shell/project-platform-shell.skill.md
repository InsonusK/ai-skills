---
name: project-platform-shell
description: The single deployable Angular application — composition root, top-level root-relative routing, selective preloading, root providers (auth interceptor, global error handler), now also declaring apps/platform-shell-e2e as its scenario-test counterpart
domain: skill
type: template
plateau: tested
project_kind: application
version: 20260711170000
tags:
  - skill/template/project
  - plateau/tested
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
---

> **Deferred scope:** carried over from [[skills/angular/architecture/plateau/navigable/plateau-navigable.skill.md|navigable]] — the embeddable-module/federation-based mounting mechanism (`remoteRegistry.loadRemoteRoutes(...)`) remains deferred to a future "platform" plateau. `app.routes.ts` only mounts directly-owned features via static `loadChildren` imports. Also depends on the `design-system` npm package (see the NPM Packages table below) — plain, non-federated consumption only; version-negotiated federation sharing is added by the future "platform" plateau via `solution-design-system-application`.
>
> `solution-testing` does not extend this project directly — its content lives in the sibling [[../platform-shell-e2e/project-platform-shell-e2e.skill.md|platform-shell-e2e]] project and in each feature's own test specs.

# Goal

- Be the only deployable unit at this plateau: bootstrap the application, own top-level routing, register root providers
- Contain no business logic of its own — every feature lives under `libs/{feature}` and is only routed to from here
- Mount each directly-owned feature at a single root segment, without knowing what routes exist beneath that segment
- Preload only the top-level features explicitly marked as worth warming up in the background
- Ensure every uncaught exception anywhere in the application is captured and routed through `LoggerService.error`, reaching the backend log sink

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Structure

## Project Structure

```
/apps/platform-shell
  /src
    /app
      /preloading
        selective-preloading.strategy.ts
      [global-error-handler.ts](./classes/class-global-error-handler.skill.md)
      app.config.ts
      app.routes.ts
    main.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| app.config.ts | Root provider registration: `withPreloading(SelectivePreloadingStrategy)`, `provideHttpClient(withInterceptors([authInterceptor]))`, and `{ provide: ErrorHandler, useClass: GlobalErrorHandler }`. | — |
| app.routes.ts | Top-level `Routes` array — one `loadChildren` entry per directly-owned feature's root segment, some carrying `data: { preload: true }`. | — |
| /preloading/selective-preloading.strategy.ts | Custom `PreloadingStrategy` implementation that preloads a route only if `route.data?.['preload'] === true`. | — |
| global-error-handler.ts | Application-wide `ErrorHandler`, capturing every uncaught exception and routing it through `LoggerService.error`. | [[classes/class-global-error-handler.skill.md\|class-global-error-handler.skill]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| design-system | latest compatible, per [[skills/angular/architecture/plateau/design-system/plateau-design-system.skill.md|design-system]] | The [[skills/angular/architecture/plateau/design-system/plateau-design-system.skill.md|design-system]] plateau's published component library. `theme.scss` is applied once at the application root. Plain dependency at this plateau — no federation version-negotiation yet (added later by the "platform" plateau's `solution-design-system-application`). |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST
- [[../repo-tested.skill.md#MUST|repo-tested]]
- `app.config.ts` MUST provide `GlobalErrorHandler` under the `ErrorHandler` token.

## MUST NOT
- [[../repo-tested.skill.md#MUST NOT|repo-tested]]
- `GlobalErrorHandler` MUST NOT be registered only in a module or component-level provider — it must be at the application root.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Anti-patterns

- **Adding a route in `app.routes.ts` that targets a specific page inside a feature (e.g. `path: 'feature1/page'`)**
  - Consequence: shell now depends on the feature's internal route structure
  - Instead: mount only `feature1` as a segment; the feature's own routes define `page` beneath it
- **Marking every top-level segment `preload: true` "to be safe"**
  - Consequence: degenerates into the equivalent of `PreloadAllModules`
  - Instead: mark only the small number of genuinely high-traffic segments
- **Catching exceptions in the handler without sending them to the backend**
  - Consequence: production errors disappear, making incidents impossible to diagnose
  - Instead: always route through `LoggerService.error` so `BackendLogSink` receives them

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Check list

- [ ] `apps/platform-shell` contains no HTTP calls, no business state, no feature-specific components
- [ ] Every entry in `app.routes.ts` is a single root segment with no nested path
- [ ] `app.config.ts` registers `withPreloading(SelectivePreloadingStrategy)`
- [ ] `ErrorHandler` token is provided with `GlobalErrorHandler` in `app.config.ts`
- [ ] `design-system`'s `theme.scss` is applied exactly once, at the application root

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Unittest TestCases

- [ ] WHEN a route has `data: { preload: true }` THEN
  - [ ] `SelectivePreloadingStrategy.preload` invokes `load()`
- [ ] WHEN an uncaught exception is thrown in the platform shell THEN
  - [ ] `GlobalErrorHandler.handleError` is invoked
  - [ ] `LoggerService.error` is called with the exception details
  - [ ] `BackendLogSink` eventually receives the entry

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
