---
name: project-platform-shell
description: The single deployable Angular application — composition root, top-level root-relative routing, selective preloading, root providers (auth interceptor, global error handler)
domain: skill
type: template
plateau: observable
project_kind: application
version: 20260711160000
tags:
  - skill/template/project
  - plateau/observable
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
---

> **Deferred scope:** carried over from navigable — the embeddable-module/federation-based mounting mechanism (`remoteRegistry.loadRemoteRoutes(...)`) remains deferred to a future "platform" plateau. `app.routes.ts` only mounts directly-owned features via static `loadChildren` imports. `solution-logging-global` notes that once registered here, `GlobalErrorHandler` will also apply "to embeddable apps sharing the same Angular runtime" once that future platform plateau exists — nothing about that requires any change to this plateau's own scope.

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

# Rules

## MUST
- [[../repo-observable.skill.md#MUST|repo-observable]]
- `app.config.ts` MUST provide `GlobalErrorHandler` under the `ErrorHandler` token — no other error handler may silently swallow uncaught exceptions.

## MUST NOT
- [[../repo-observable.skill.md#MUST NOT|repo-observable]]
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
- [ ] Uncaught exceptions in the platform shell reach the backend log sink

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
