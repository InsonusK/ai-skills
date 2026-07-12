---
name: plateau-async-monolith--project-platform-shell
description: The single deployable Angular application — composition root, routing, now with a selective preloading strategy and a registered service worker. No global error handler yet. — async-monolith plateau
domain: skill
type: template
plateau: async-monolith
project_kind: application
version: 20260711190000
tags:
  - skill/template/project
  - plateau/async-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
---

> No global error handler yet — `GlobalErrorHandler` arrives with [[skills/angular/architecture/plateau/plateau-monitored-app/plateau-monitored-app.skill|monitored-app]]. Also depends on the `design-system` npm package, unchanged from `online-monolith`.

# Goal

- Be the only deployable unit at this plateau: bootstrap the application, own top-level routing, register root providers, and own the cross-cutting infrastructure introduced at this plateau (preloading strategy, service worker)
- Contain no business logic of its own — every feature lives under `libs/{feature}` and is only routed to from here

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Core Principles

- Preloading is selective: only segments explicitly marked `data: { preload: true }` at the mounting point are warmed up in the background
- The shell registers the service worker exactly once, at the application root, after bootstrap

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Structure

## Project Structure

```
/apps/platform-shell
  /src
    /app
      app.config.ts
      app.routes.ts
    /preloading
      selective-preloading.strategy.ts
    sw-build.ts                       <- see [plateau-async-monolith--class-service-worker.skill.md](./classes/plateau-async-monolith--class-service-worker.skill.md)
    sw-src.ts                         <- see [plateau-async-monolith--class-service-worker.skill.md](./classes/plateau-async-monolith--class-service-worker.skill.md)
    main.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| app.config.ts | Root provider registration: router now configured with `withPreloading(SelectivePreloadingStrategy)` | — |
| app.routes.ts | Top-level `Routes` array; one entry per directly-owned feature's root segment, with `preload: true` only where deliberately reviewed | — |
| /preloading/selective-preloading.strategy.ts | Custom `PreloadingStrategy` preloading only routes whose `data.preload === true` | — |
| sw-build.ts / sw-src.ts | Workbox-generated service worker: app-shell precache, static-asset cache-first, API-read stale-while-revalidate, auth/mutation network-only | [[skills/angular/architecture/plateau/plateau-async-monolith/structure/platform-shell/classes/plateau-async-monolith--class-service-worker.skill\|class-service-worker]] |
| main.ts | Bootstraps the application, then registers `/sw.js` post-bootstrap | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

## What Does NOT Belong Here

- HTTP calls, business state, feature-specific components
- Any durable, persisted mutation queue — that arrives with [[skills/angular/architecture/plateau/plateau-offline-monolith/plateau-offline-monolith.skill|offline-monolith]]

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

## Allowed Dependencies

- Any `libs/{feature}/feature` (tag: `type:feature`)
- `libs/shared/ui`, `libs/shared/util`, `libs/shared/state`, `libs/shared/logging` (tag: `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Rules

## MUST
- [[skills/angular/architecture/plateau/plateau-async-monolith/structure/plateau-async-monolith--repo-async-monolith.skill#MUST|repo-async-monolith]]
- The router MUST be configured with `withPreloading(SelectivePreloadingStrategy)`, never `PreloadAllModules` and never the default `NoPreloading`.
- The service worker registration MUST be wired exactly once, at this project's root.
- `main.ts` MUST register `/sw.js` only after the application has bootstrapped, and only in production-like builds.

## MUST NOT
- [[skills/angular/architecture/plateau/plateau-async-monolith/structure/plateau-async-monolith--repo-async-monolith.skill#MUST NOT|repo-async-monolith]]
- The service worker MUST NOT be registered in development builds unless explicitly requested.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Anti-patterns

- **Marking every top-level segment `preload: true` "to be safe"**
  - Consequence: degenerates into `PreloadAllModules`
  - Instead: mark only a small, deliberately reviewed subset
- **Registering the service worker before the app bootstraps**
  - Consequence: race conditions where the SW intercepts requests before the app is ready
  - Instead: register after bootstrap in `main.ts`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Check list

- [ ] `apps/platform-shell` contains no HTTP calls, no business state, no feature-specific components
- [ ] `app.config.ts` registers `withPreloading(SelectivePreloadingStrategy)`
- [ ] `navigator.serviceWorker.register('/sw.js')` runs after bootstrap, production builds only

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
