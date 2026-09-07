---
name: registry-platform-shell-project
description: Conflict Detection result for the `platform-shell-project` element at plateau-multiuser-monolith — the composition root, now also extended by solution-logging-global (GlobalErrorHandler) and solution-authentication (interceptor + bootstrap refresh)
tags:
  - concern/architecture
  - stack/typescript
  - element/platform-shell-project
---

# Element
`element/platform-shell-project` — `apps/platform-shell`, the single deployable Angular application: `app.config.ts` (root providers), `app.routes.ts` (top-level segments), `main.ts` (bootstrap).

# Involved solutions
Project created by [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] as part of the workspace. `.extend`ed by:
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] — `app.routes.ts` mounts each feature at one root segment
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] (VP1) — `withPreloading(SelectivePreloadingStrategy)` + the production bundle budgets
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] (VP4) — the post-bootstrap, prod-only `navigator.serviceWorker.register('/sw.js')` + `provideGlobalStore()` + the `<ui-offline-banner>` mount
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] (VP5) — `provideOfflineSync()` (the replay orchestrator only)
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] (VP6) — `{ provide: ErrorHandler, useClass: GlobalErrorHandler }` (`PlatformHost/platform-shell.project.extend` + `PlatformHost/global-error-handler.ts.create`)
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] (VP7) — `provideHttpClient(withFetch(), withInterceptors([authInterceptor]))` + one `provideAppInitializer` dispatching `Silent Refresh Requested` + the `/login` and `/forbidden` `loadComponent` routes

This is the shallowest `monolith` plateau where `logging-global` and `authentication` coexist with the rest. (`solution-federation-host` also extends this element — that intersection lives in a `platform-host` plateau, per [delta-conflict-analysis Finding 5](skills/angular/architecture/v3.1/delta-conflict-analysis.md#findings).)

# Classification
`FMN` / `TMN` — Category `M` (code changes to `app.config.ts` / `app.routes.ts`). Kind `N` (independent): each `.extend` adds one distinct bootstrap wiring — a router option, a service-worker registration, a store provider, an `ErrorHandler` binding, an HTTP-client interceptor, an app initializer, two lazy routes. No two edit the same statement. VP6 and VP7 both carry `depends_on` edges that fix their order after the baseline wiring.

# Ordering
`source: ordering-only` — there is no VP↔VP Feature-Model constraint between these shell extensions; the create-then-extend order is recorded by each solution's `depends_on solution-repository-structure` (and, for VP6/VP7, `depends_on solution-api-http-layer` / `solution-global-store`). The relative order of the extends within `providers: [...]` does not matter — `provideAppInitializer` runs after all providers are constructed regardless of array position.

# Resolution
**Canonical — no resolver.** The composition root is a list of independent provider registrations by design. The example's `app.config.ts` carries all six wirings; `store.config.spec.ts` and `app.spec.ts` confirm the app bootstraps with every provider in place, and `auth.interceptor.spec.ts` confirms exactly one `Silent Refresh Requested` per 401.

# Architectural signal
**N ≥ 5 here. Benign.** The composition root is expected to accumulate one bootstrap wiring per cross-cutting capability — that is what a composition root is for. Each `.extend` is member-disjoint at the statement level. Not a mis-drawn VP.
