---
name: project-platform-shell
description: The single deployable Angular application, now a Native Federation dynamic host — composition root, routing, selective preloading, service worker (incl. federated-remote caching), global error handling, runtime remote registry, and a version-negotiated design-system singleton
domain: skill
type: template
plateau: platform
project_kind: application
version: 20260711150000
tags:
  - skill/template/project
  - plateau/platform
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
  - "[[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]]"
  - "[[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]]"
---

> No further deferrals: this is where `solution-platform-embeddability` and `solution-design-system-application` land in full, and where `solution-app-routing`'s embeddable-module mounting slice (deferred by every earlier plateau) is re-included.

# Goal

- Be the only deployable unit at this plateau: bootstrap the application, own top-level routing, register root providers, own every piece of cross-cutting infrastructure (preloading, service worker, global error handler)
- Load independently built and deployed embeddable apps at runtime, without rebuilding the shell
- Share a single Angular runtime and a single instance of `@platform/contracts` between the host and every loaded embeddable app
- Contain no business logic of its own — every feature lives under `libs/{feature}` and is only routed to from here

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Core Principles

- Each entry in `app.routes.ts` mounts exactly one root segment — a directly-owned feature, or (as of this plateau) an embeddable module resolved at runtime via `RemoteRegistryService` — both mounted the same way
- Preloading is selective: only segments explicitly marked `data: { preload: true }` are warmed up in the background
- The shell registers cross-cutting infrastructure (service worker, global error handler, remote registry) exactly once, at the application root
- The host never knows about a specific embeddable app at build time — only about the shape of the federation contract and `@platform/contracts`
- Remote discovery is a runtime concern, resolved from configuration, never compiled in

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Structure

## Project Structure

```
/apps/platform-shell
  /src
    federation.config.ts
    /app
      app.config.ts
      app.routes.ts
      global-error-handler.ts        <- see [class-global-error-handler.skill.md](./classes/class-global-error-handler.skill.md)
      /remote-registry
        remote-registry.service.ts   <- see [class-remote-registry-service.skill.md](./classes/class-remote-registry-service.skill.md)
      /shell
        shell.component.ts           <- mounts a resolved remote via loadRemoteModule
    /preloading
      selective-preloading.strategy.ts
    sw-build.ts                       <- see [class-service-worker.skill.md](./classes/class-service-worker.skill.md)
    sw-src.ts                         <- see [class-service-worker.skill.md](./classes/class-service-worker.skill.md)
    styles.scss                       <- imports the design-system's theme.scss + custom-tokens.scss
    main.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| federation.config.ts | Native Federation config: `@platform/contracts`, Angular, and the design-system package all declared `singleton: true` shared dependencies (design system additionally `strictVersion: false`); no remotes declared statically (Dynamic Federation). | — |
| app.config.ts | Root provider registration: router with `withPreloading(SelectivePreloadingStrategy)`, `{ provide: ErrorHandler, useClass: GlobalErrorHandler }`. | — |
| app.routes.ts | Top-level `Routes` array: one entry per directly-owned feature's root segment, plus embeddable-module segments resolved via `RemoteRegistryService`. | — |
| app/remote-registry/remote-registry.service.ts | Runtime service resolving available embeddable apps and their `remoteEntry` URLs from a config manifest. | [[classes/class-remote-registry-service.skill.md\|class-remote-registry-service.skill]] |
| app/shell/shell.component.ts | Mounts a resolved remote's exposed entry point into a host-provided slot via `loadRemoteModule`. | — |
| app/global-error-handler.ts | Global `ErrorHandler` routing every uncaught exception through `LoggerService.error`. | [[classes/class-global-error-handler.skill.md\|class-global-error-handler.skill]] |
| sw-build.ts / sw-src.ts | Workbox-generated service worker: app-shell precache, static-asset cache-first, API-read stale-while-revalidate, auth/mutation network-only, federated-remote-chunk stale-while-revalidate. | [[classes/class-service-worker.skill.md\|class-service-worker.skill]] |
| styles.scss | Imports the design system's `theme.scss`/`custom-tokens.scss` at the root — the only production consumer required to. | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/PlatformHost/platform-shell.federation.extend|PlatformHost/platform-shell.federation.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular-architects/native-federation | pinned minor | Federation build plugin and `loadRemoteModule` runtime API |
| @platform/contracts | semver range | Shared EventBus/session contract, declared as singleton |
| design-system | semver range, `strictVersion: false` | Version-negotiated federation singleton, per [[skills/angular/architecture/plateau/design-system/plateau-design-system.skill.md|design-system]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/PlatformHost/platform-shell.federation.extend|PlatformHost/platform-shell.federation.extend]]

## What Does NOT Belong Here

- HTTP calls, business state, feature-specific components
- Any durable, persisted mutation queue — lives in `libs/shared/offline-sync`
- A specific embeddable app's code, bundled at build time — remotes are always resolved at runtime
- Any Angular Material type/selector — the shell consumes only the `design-system` package's public API

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/Repository.extend|PlatformHost/Repository.extend]]

## Allowed Dependencies

- Any `libs/{feature}/feature` (tag: `type:feature`)
- `libs/shared/ui`, `libs/shared/util`, `libs/shared/state`, `libs/shared/logging` (tag: `scope:shared`)
- `@platform/contracts` (external, singleton)
- `design-system` (external, singleton, version-negotiated)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST
- [[../repo-platform.skill.md#MUST|repo-platform]]
- Each entry in `app.routes.ts` MUST correspond to exactly one root segment — a feature or an embeddable module — never a nested path.
- The router MUST be configured with `withPreloading(SelectivePreloadingStrategy)`.
- `GlobalErrorHandler` and the service worker registration MUST each be wired exactly once, at this project's root.
- `federation.config.ts` MUST NOT list embeddable apps' remote entries statically — resolved by `RemoteRegistryService` at runtime.
- `federation.config.ts` MUST mark `@platform/contracts`, Angular, and the design system as `singleton: true` (design system additionally `strictVersion: false`).
- Any component/service in `platform-shell` that talks to an embedded app MUST do so exclusively through `@platform/contracts` — never by reaching into a remote's internals.
- `styles.scss` MUST import both `theme.scss` and `custom-tokens.scss` from the design system package.

## SHOULD
- `RemoteRegistryService` SHOULD treat a failure to resolve or load a given remote as a recoverable error (fallback UI slot), not fatal for the whole shell.

## MUST NOT
- [[../repo-platform.skill.md#MUST NOT|repo-platform]]
- The service worker MUST NOT be registered in development builds unless explicitly requested.
- `apps/platform-shell` MUST NOT bundle a specific embeddable app's code at build time.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/PlatformHost/platform-shell.federation.extend|PlatformHost/platform-shell.federation.extend]]

# Anti-patterns

- **Adding a route in `app.routes.ts` that targets a specific page inside a feature or module**
  - Consequence: shell depends on internal route structure it shouldn't know about
  - Instead: mount only the root segment
- **Registering the service worker before the app bootstraps**
  - Consequence: race conditions where the SW intercepts requests before the app is ready
  - Instead: register after bootstrap in `main.ts`
- **Reaching into a loaded remote's internal exports instead of the shared contract**
  - Consequence: couples the shell to one remote's internal structure, breaking silently when that remote refactors
  - Instead: all cross-boundary interaction goes through `@platform/contracts`
- **Hardcoding a remote's URL or version into the host's source**
  - Consequence: the platform must be rebuilt and redeployed every time an embeddable app ships a new version
  - Instead: resolve remotes from a runtime configuration/manifest
- **Setting `strictVersion: true` for the design system shared dependency**
  - Consequence: reintroduces lockstep-upgrade coupling
  - Instead: always `strictVersion: false`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/PlatformHost/platform-shell.federation.extend|PlatformHost/platform-shell.federation.extend]]

# Check list

- [ ] `apps/platform-shell` contains no HTTP calls, no business state, no feature-specific components
- [ ] `app.config.ts` registers `withPreloading(SelectivePreloadingStrategy)` and `GlobalErrorHandler`
- [ ] `federation.config.ts` marks `@platform/contracts`, Angular, and the design system as `singleton: true`
- [ ] No embeddable app's remote entry is hardcoded in source or build config
- [ ] `styles.scss` applies the design system's theme exactly once

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/PlatformHost/platform-shell.federation.extend|PlatformHost/platform-shell.federation.extend]]
