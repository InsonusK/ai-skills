---
name: project-platform-shell
description: The single deployable Angular application — composition root, routing, selective preloading, service worker, global error handling
domain: skill
type: template
plateau: offline-app
project_kind: application
version: 20260711140000
tags:
  - skill/template/project
  - plateau/offline-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
---

# Goal

- Be the only deployable unit at this plateau: bootstrap the application, own top-level routing, register root providers, and own every piece of cross-cutting infrastructure (preloading strategy, service worker, global error handler)
- Contain no business logic of its own — every feature lives under `libs/{feature}` and is only routed to from here

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Core Principles

- Each entry in `app.routes.ts` mounts exactly one root segment — a directly-owned feature via `loadChildren`, resolved to that feature's exported `Routes`
- Preloading is selective: only segments explicitly marked `data: { preload: true }` at the mounting point are warmed up in the background
- The shell registers cross-cutting infrastructure (service worker, global error handler) exactly once, at the application root, so it applies uniformly to every feature

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Structure

## Project Structure

```
/apps/platform-shell
  /src
    /app
      app.config.ts
      app.routes.ts
      global-error-handler.ts        <- see [class-global-error-handler.skill.md](./classes/class-global-error-handler.skill.md)
    /preloading
      selective-preloading.strategy.ts
    sw-build.ts                       <- see [class-service-worker.skill.md](./classes/class-service-worker.skill.md)
    sw-src.ts                         <- see [class-service-worker.skill.md](./classes/class-service-worker.skill.md)
    main.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| app.config.ts | Root provider registration: router with `withPreloading(SelectivePreloadingStrategy)`, `{ provide: ErrorHandler, useClass: GlobalErrorHandler }` | — |
| app.routes.ts | Top-level `Routes` array; one entry per directly-owned feature's root segment, with `preload: true` only where deliberately reviewed | — |
| /preloading/selective-preloading.strategy.ts | Custom `PreloadingStrategy` preloading only routes whose `data.preload === true` | — |
| app/global-error-handler.ts | Global `ErrorHandler` routing every uncaught exception through `LoggerService.error` | [[classes/class-global-error-handler.skill.md\|class-global-error-handler.skill]] |
| sw-build.ts / sw-src.ts | Workbox-generated service worker: app-shell precache, static-asset cache-first, API-read stale-while-revalidate, auth/mutation network-only | [[classes/class-service-worker.skill.md\|class-service-worker.skill]] |
| main.ts | Bootstraps the application, then registers `/sw.js` post-bootstrap | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

## What Does NOT Belong Here

- HTTP calls, business state, feature-specific components
- Any durable, persisted mutation queue — that lives in `libs/shared/offline-sync`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

## Allowed Dependencies

- Any `libs/{feature}/feature` (tag: `type:feature`)
- `libs/shared/ui`, `libs/shared/util`, `libs/shared/state`, `libs/shared/logging` (tag: `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST
- [[../repo-offline-app.skill.md#MUST|repo-offline-app]]
- Each entry in `app.routes.ts` MUST correspond to exactly one root segment; the shell MUST NOT import a component from inside a feature directly.
- The router MUST be configured with `withPreloading(SelectivePreloadingStrategy)`, never `PreloadAllModules` and never the default `NoPreloading`.
- `GlobalErrorHandler` and the service worker registration MUST each be wired exactly once, at this project's root.
- `main.ts` MUST register `/sw.js` only after the application has bootstrapped, and only in production-like builds.

## MUST NOT
- [[../repo-offline-app.skill.md#MUST NOT|repo-offline-app]]
- The service worker MUST NOT be registered in development builds unless explicitly requested.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Anti-patterns

- **Adding a route in `app.routes.ts` that targets a specific page inside a feature**
  - Consequence: shell depends on the feature's internal route structure
  - Instead: mount only the feature's root segment
- **Marking every top-level segment `preload: true` "to be safe"**
  - Consequence: degenerates into `PreloadAllModules`
  - Instead: mark only a small, deliberately reviewed subset
- **Registering the service worker before the app bootstraps**
  - Consequence: race conditions where the SW intercepts requests before the app is ready
  - Instead: register after bootstrap in `main.ts`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Check list

- [ ] `apps/platform-shell` contains no HTTP calls, no business state, no feature-specific components
- [ ] `app.config.ts` registers `withPreloading(SelectivePreloadingStrategy)` and `GlobalErrorHandler`
- [ ] `navigator.serviceWorker.register('/sw.js')` runs after bootstrap, production builds only

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
