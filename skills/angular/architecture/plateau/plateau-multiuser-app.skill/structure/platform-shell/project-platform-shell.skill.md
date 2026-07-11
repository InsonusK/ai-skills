---
name: project-platform-shell
description: The single deployable Angular application — composition root, routing, selective preloading, registered service worker, Native Federation dynamic host, and now a global ErrorHandler routing every uncaught exception to the backend log sink
domain: skill
type: template
plateau: multiuser-app
project_kind: application
version: 20260711230000
tags:
  - skill/template/project
  - plateau/multiuser-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
---

> Also depends on the `design-system` npm package, unchanged from `online-monolith`.

# Goal

- Be the only deployable unit at this plateau: bootstrap the application, own top-level routing, register root providers, and own the cross-cutting infrastructure introduced across earlier plateaus (preloading strategy, service worker, federation host)
- Load independently built and deployed embeddable apps into the shell at runtime, without rebuilding the shell
- Ensure every uncaught exception anywhere in the platform shell is routed through `LoggerService.error` and reaches `BackendLogSink`
- Contain no business logic of its own — every feature lives under `libs/{feature}` and is only routed to from here

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Core Principles

- Preloading is selective: only segments explicitly marked `data: { preload: true }` at the mounting point are warmed up in the background
- The shell registers the service worker exactly once, at the application root, after bootstrap
- The shell never knows about a specific embeddable app at build time — only the shape of the federation contract and `@platform/contracts`; the list of available remotes is resolved at runtime (Dynamic Federation), never hardcoded

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Structure

## Project Structure

```
/apps/platform-shell
  /src
    federation.config.ts              <- Native Federation host config, no static remotes
    /app
      app.config.ts                   <- also registers GlobalErrorHandler
      app.routes.ts
      global-error-handler.ts         <- see [class-global-error-handler.skill.md](./classes/class-global-error-handler.skill.md)
      /remote-registry
        [remote-registry.service.ts](./classes/class-remote-registry-service.skill.md)
      /shell
        shell.component.ts            <- mounts a resolved remote via loadRemoteModule
    /preloading
      selective-preloading.strategy.ts
    sw-build.ts                       <- see [class-service-worker.skill.md](./classes/class-service-worker.skill.md)
    sw-src.ts                         <- see [class-service-worker.skill.md](./classes/class-service-worker.skill.md)
    main.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| federation.config.ts | Native Federation config: declares `@platform/contracts` (and Angular) as `singleton: true`; declares no remotes statically (Dynamic Federation) | — |
| app.config.ts | Root provider registration: router configured with `withPreloading(SelectivePreloadingStrategy)`, `{ provide: ErrorHandler, useClass: GlobalErrorHandler }` | — |
| app.routes.ts | Top-level `Routes` array; one entry per directly-owned feature's root segment, with `preload: true` only where deliberately reviewed | — |
| app/global-error-handler.ts | Global `ErrorHandler` routing every uncaught exception through `LoggerService.error` | [[skills/angular/architecture/plateau/plateau-multiuser-app.skill/structure/platform-shell/classes/class-global-error-handler.skill\|class-global-error-handler]] |
| /remote-registry/remote-registry.service.ts | Runtime service resolving the list of available embeddable apps and their `remoteEntry` URLs from configuration | [[skills/angular/architecture/plateau/plateau-multiuser-app.skill/structure/platform-shell/classes/class-remote-registry-service.skill\|class-remote-registry-service]] |
| /shell/shell.component.ts | Mounts a resolved remote's exposed entry point into a host-provided slot, using `loadRemoteModule` | — |
| /preloading/selective-preloading.strategy.ts | Custom `PreloadingStrategy` preloading only routes whose `data.preload === true` | — |
| sw-build.ts / sw-src.ts | Workbox-generated service worker: app-shell precache, static-asset cache-first, API-read stale-while-revalidate, auth/mutation network-only, federated-remote stale-while-revalidate | [[skills/angular/architecture/plateau/plateau-multiuser-app.skill/structure/platform-shell/classes/class-service-worker.skill\|class-service-worker]] |
| main.ts | Bootstraps the application, then registers `/sw.js` post-bootstrap | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular-architects/native-federation | pinned minor, matching the Angular major version in use | Federation host runtime, build plugin, and `loadRemoteModule` |
| @platform/contracts | semver range, published from a separate repository | Shared, versioned contract for cross-app state/events, marked `singleton: true` |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

## What Does NOT Belong Here

- HTTP calls, business state, feature-specific components
- A specific embeddable app's code bundled at build time — that would defeat independent deployability
- Any import of an embeddable app's internal implementation — only `@platform/contracts` and the federation `remoteEntry` contract

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

## Allowed Dependencies

- Any `libs/{feature}/feature` (tag: `type:feature`)
- `libs/shared/ui`, `libs/shared/util`, `libs/shared/state`, `libs/shared/logging` (tag: `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Rules

## MUST
- [[skills/angular/architecture/plateau/plateau-multiuser-app.skill/structure/repo-multiuser-app.skill#MUST|repo-multiuser-app]]
- The router MUST be configured with `withPreloading(SelectivePreloadingStrategy)`, never `PreloadAllModules` and never the default `NoPreloading`.
- The service worker registration MUST be wired exactly once, at this project's root.
- `main.ts` MUST register `/sw.js` only after the application has bootstrapped, and only in production-like builds.
- `apps/platform-shell` MUST declare the `type:host` tag in addition to its existing `type:app`/`scope:platform` tags.
- `federation.config.ts` MUST mark `@platform/contracts` (and Angular) as `singleton: true`, and MUST NOT list embeddable apps' remote entries statically.
- Any code in `platform-shell` that needs to talk to an embedded app MUST do so exclusively through `@platform/contracts` — never by reaching into a remote's internals.
- `app.config.ts` MUST provide `GlobalErrorHandler` under the `ErrorHandler` token — no other error handler may silently swallow uncaught exceptions.

## MUST NOT
- [[skills/angular/architecture/plateau/plateau-multiuser-app.skill/structure/repo-multiuser-app.skill#MUST NOT|repo-multiuser-app]]
- The service worker MUST NOT be registered in development builds unless explicitly requested.
- The shell MUST NOT bundle a specific embeddable app's code at build time.
- `GlobalErrorHandler` MUST NOT be registered only in a module or component-level provider — it must be at the application root.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Anti-patterns

- **Marking every top-level segment `preload: true` "to be safe"**
  - Consequence: degenerates into `PreloadAllModules`
  - Instead: mark only a small, deliberately reviewed subset
- **Registering the service worker before the app bootstraps**
  - Consequence: race conditions where the SW intercepts requests before the app is ready
  - Instead: register after bootstrap in `main.ts`
- **Hardcoding a remote's URL or version into the host's source**
  - Consequence: the platform must be rebuilt and redeployed every time an embeddable app ships a new version
  - Instead: resolve remotes from `RemoteRegistryService`'s runtime configuration
- **Reaching into a loaded remote's internal exports instead of the shared contract**
  - Consequence: couples the shell to one remote's internal structure, breaking silently when that remote refactors
  - Instead: all cross-boundary interaction goes through `@platform/contracts`
- **Catching exceptions in the handler without sending them to the backend**
  - Consequence: production errors disappear, making incidents impossible to diagnose
  - Instead: always route through `LoggerService.error` so `BackendLogSink` receives them

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Check list

- [ ] `apps/platform-shell` contains no HTTP calls, no business state, no feature-specific components
- [ ] `app.config.ts` registers `withPreloading(SelectivePreloadingStrategy)`
- [ ] `navigator.serviceWorker.register('/sw.js')` runs after bootstrap, production builds only
- [ ] `federation.config.ts` marks `@platform/contracts` and Angular as `singleton: true`
- [ ] No embeddable app's remote entry is hardcoded in `platform-shell`'s source or build config
- [ ] `ErrorHandler` token is provided with `GlobalErrorHandler` in `app.config.ts`
- [ ] Uncaught exceptions in the platform shell reach the backend log sink

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
