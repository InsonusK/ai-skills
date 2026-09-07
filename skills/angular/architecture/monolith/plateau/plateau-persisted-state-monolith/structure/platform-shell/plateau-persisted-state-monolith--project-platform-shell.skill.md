---
name: plateau-persisted-state-monolith--project-platform-shell
description: The single deployable Angular application — composition root, top-level routing, root providers, a SelectivePreloadingStrategy, error-level bundle budgets, a Workbox service worker (registered after bootstrap, prod only) mounting the shared offline banner, plus (VP6) GlobalErrorHandler and (VP7) the auth interceptor + a single bootstrap silent-refresh with /login and /forbidden routes. — persisted-state-monolith plateau
domain: skill
type: template
whenToUse: when editing apps/platform-shell — app.config.ts, app.routes.ts, main.ts, project.json, the service worker — or checking the composition root holds no business logic and mounts each feature at one root segment
plateau: persisted-state-monolith
project_kind: application
version: 20260903190000
tags:
  - skill/template/project
  - plateau/persisted-state-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"

> `solution-app-testing` does not extend this project directly — its content lives in the sibling [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/platform-shell-e2e/plateau-persisted-state-monolith--project-platform-shell-e2e.skill.md|platform-shell-e2e]] project and in each feature's own test specs. Also depends on the `design-system` npm package (see the NPM Packages table below) — plain, non-federated consumption only.

# Goal

- Be the only deployable unit at this plateau: bootstrap the application, own top-level routing, register root providers
- Contain no business logic of its own — every feature lives under `libs/{feature}` and is only routed to from here
- Mount each directly-owned feature at a single root segment, without knowing what routes exist beneath that segment

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]

# Structure

## Project Structure

```
/apps/platform-shell
  /src
    /app
      /preloading
        selective-preloading.strategy.ts      <- VP1
        selective-preloading.strategy.spec.ts
      app.config.ts                            <- + GlobalErrorHandler (VP6), authInterceptor + bootstrap silent refresh (VP7)
      app.routes.ts                            <- + /login + /forbidden (loadComponent from @org/shared-auth-ui) (VP7)
      global-error-handler.ts                  <- new (VP6): ErrorHandler → LoggerService.error
      app.ts / app.html                        <- VP4: mounts <ui-offline-banner [isOnline]="isOnline()" />
    sw-src.ts                                  <- new (VP4): Workbox runtime routing rules
    sw-routes.ts                               <- new (VP4): content-type predicates (unit-tested)
    sw-routes.spec.ts
    sw-build.mjs                               <- new (VP4): esbuild + workbox-build injectManifest
    main.ts                                    <- VP4: registers /sw.js after bootstrap, prod only
  tsconfig.sw.json                             <- new (VP4): WebWorker lib; excluded from tsconfig.app.json
  project.json                                 <- VP1 budgets + VP4 `build-sw` target (dependsOn build)
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| app.config.ts | Root provider registration. VP1: `withPreloading(SelectivePreloadingStrategy)`. VP4: `provideGlobalStore()`. VP5: `provideOfflineSync()`. VP6: `{ provide: ErrorHandler, useClass: GlobalErrorHandler }`. VP7: `provideHttpClient(withFetch(), withInterceptors([authInterceptor]))` + `provideAppInitializer(() => inject(Store).dispatch(AuthActions.silentRefreshRequested()))` — exactly one silent-refresh at bootstrap. | — |
| app.routes.ts | Top-level `Routes` array — one `loadChildren` entry per feature root segment. VP1: a reviewed subset carries `data: { preload: true }`. VP7: `/login` + `/forbidden` `loadComponent` from `@org/shared-auth-ui`. A permission-guarded route lives inside the feature, not here. | — |
| global-error-handler.ts | VP6: `ErrorHandler` implementation routing every uncaught exception through `LoggerService.error` (only `message` / `stack`). | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/platform-shell/classes/plateau-persisted-state-monolith--class-global-error-handler.skill.md\|class-global-error-handler]] |
| app.ts / app.html | Shell component. VP4: injects `Store`, mounts `<ui-offline-banner [isOnline]="isOnline()">` once (store wiring lives here, not in `shared-ui`). | — |
| preloading/selective-preloading.strategy.ts | Custom `PreloadingStrategy` — preloads a route's chunk only when `route.data['preload'] === true`. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/platform-shell/classes/plateau-persisted-state-monolith--class-selective-preloading-strategy.skill.md\|class-selective-preloading-strategy]] |
| sw-src.ts / sw-routes.ts / sw-build.mjs | The Workbox service worker: four content-type routing rules, testable predicates, and the Nx build step. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/platform-shell/classes/plateau-persisted-state-monolith--class-service-worker.skill.md\|class-service-worker]] |
| main.ts | VP4: after `bootstrapApplication(...)` resolves and only when `!isDevMode()`, `navigator.serviceWorker.register('/sw.js')`. | — |
| project.json | VP1: `budgets` (`initial` + `anyScript`, `maximumError`). VP4: a `build-sw` target (`nx:run-commands` → `node src/sw-build.mjs`, `dependsOn: [build]`). | — |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| design-system | latest compatible, per [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/plateau-design-system.skill/plateau-design-system.skill.md|design-system]] | The [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/plateau-design-system.skill/plateau-design-system.skill.md|design-system]] plateau's published component library. `theme.scss` is applied once at the application root. |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]

# Rules

## MUST
- [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/plateau-persisted-state-monolith--repo-persisted-state-monolith.skill#must|repo-persisted-state-monolith]]

- [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/plateau-persisted-state-monolith--repo-persisted-state-monolith.skill#must never|repo-persisted-state-monolith]]
- `app.config.ts` must register the router with `withPreloading(SelectivePreloadingStrategy)`.
- `data: { preload: true }` is set only here, on a top-level `loadChildren` entry — never passed down into or set by a feature's own routes.
- `project.json`'s production build must carry an `initial` and a per-script bundle budget, each with a `maximumError` (not only `maximumWarning`).
- Never mark a top-level segment `preload: true` without a deliberate review that it is high-traffic — preloading every segment degenerates into `PreloadAllModules` and prefetches remote chunks the shell never intended to warm up.
- `main.ts` must register `/sw.js` only after `bootstrapApplication(...)` resolves, and only when `!isDevMode()` — a SW registered before bootstrap races the app; one registered in dev fights live-reload.
- The `build-sw` target must run after the production build writes the bundle (`dependsOn: [build]`), and generate `sw.js` via Workbox's programmatic API — never ngsw, never a webpack plugin.
- `<ui-offline-banner>` must be mounted exactly once, here in the shell, and fed `isOnline` from `selectIsOnline` — a feature must never mount its own.
- `GlobalErrorHandler` (VP6) must be registered exactly once, here, as `{ provide: ErrorHandler, useClass: GlobalErrorHandler }` — never module- or component-scoped. It lives in `apps/platform-shell` because it is a composition-root concern, not a `shared/logging` one.
- `authInterceptor` (VP7) must be wired here via `provideHttpClient(withFetch(), withInterceptors([authInterceptor]))` and nowhere else — a feature must never register its own auth interceptor.
- The bootstrap silent-refresh (`provideAppInitializer(() => inject(Store).dispatch(AuthActions.silentRefreshRequested()))`) must be dispatched exactly once, here — a feature must never trigger session bootstrap.
- The access token must never be read from or written to `localStorage` / `sessionStorage` anywhere in the shell — it lives only in the `auth` slice's in-memory state.
- `/login` and `/forbidden` are the only auth routes declared here (`loadComponent` from `@org/shared-auth-ui`); a permission-guarded feature route lives in that feature's own `*.routes.ts`, never here.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create.md|HttpLayer/auth.interceptor.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.create.md|GlobalStore/auth.store.ts.create]]


- **Adding a route in `app.routes.ts` that targets a specific page inside a feature (e.g. `path: 'feature1/page'`)**
  - Consequence: shell now depends on the feature's internal route structure
  - Instead: mount only `feature1` as a segment; the feature's own routes define `page` beneath it

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]

# Check list

- [ ] `apps/platform-shell` contains no HTTP calls, no business state, no feature-specific components
- [ ] Every entry in `app.routes.ts` is a single root segment with no nested path
- [ ] `design-system`'s `theme.scss` is applied exactly once, at the application root
- [ ] `app.config.ts` registers `withPreloading(SelectivePreloadingStrategy)`
- [ ] Only a deliberately reviewed subset of top-level segments carry `data: { preload: true }`
- [ ] The production build declares `initial` + `anyScript` budgets with `maximumError` set
- [ ] `main.ts` registers `/sw.js` after bootstrap and only when `!isDevMode()`
- [ ] `nx build-sw platform-shell` produces `dist/apps/platform-shell/browser/sw.js`
- [ ] `<ui-offline-banner>` appears once, in the shell template, fed by `selectIsOnline`
- [ ] `{ provide: ErrorHandler, useClass: GlobalErrorHandler }` appears exactly once, in `app.config.ts`
- [ ] `authInterceptor` is registered once via `provideHttpClient(withFetch(), withInterceptors([authInterceptor]))`
- [ ] `AuthActions.silentRefreshRequested()` is dispatched exactly once, from `provideAppInitializer` in `app.config.ts`
- [ ] no `localStorage` / `sessionStorage` access-token read or write anywhere in `apps/platform-shell`
- [ ] `/login` and `/forbidden` are the only auth routes in `app.routes.ts`; no permission guard is attached here

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]
