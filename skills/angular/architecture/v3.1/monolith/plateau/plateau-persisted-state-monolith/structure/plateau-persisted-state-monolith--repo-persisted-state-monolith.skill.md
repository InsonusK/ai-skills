---
name: plateau-persisted-state-monolith--repo-persisted-state-monolith
description: Nx workspace layout for the persisted-state-monolith Angular application — the multiuser-monolith workspace plus VP8 PersistedState — a persistence/ mechanism (the persistKeys() metaReducer + SENSITIVE_STATE_KEYS guard + withPersistedDraft()) and a persisted preferences slice in libs/shared/state, plus an opt-in {Feature}DraftStore per feature. The sixth and last plateau of the monolith chain. One deployable unit; no Module Federation.
domain: skill
type: template
whenToUse: when scaffolding the Nx workspace, adding a project or a boundary allow-list row, or checking whether a change follows this plateau's workspace-level rules (tags, boundaries, backend log delivery, authentication, persisted state)
plateau: persisted-state-monolith
version: 20260903190000
tags:
  - skill/template/repo
  - plateau/persisted-state-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]]"

> **Sixth and last plateau of the monolith chain.** Composes [[skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/plateau-multiuser-monolith--repo-multiuser-monolith.skill.md|plateau-multiuser-monolith]] (online + VP1 + VP4 + VP5 + VP6 + VP7) and adds **one** solution: [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] (**VP8 — PersistedState**). VP1–VP8 = Yes. **No new Nx project.** VP8 adds a `persistence/` folder to `libs/shared/state` (the `persistKeys()` metaReducer factory + `SENSITIVE_STATE_KEYS` guard + `assertPersistable()` + the `withPersistedDraft()` signal-store feature) and a persisted `preferences` slice (theme / density / last feature tab → `localStorage`, rehydrated synchronously). A feature may add an opt-in `{Feature}DraftStore` (`signalStore` + `withPersistedDraft`) so an in-progress form survives a reload. The `auth` slice is **never** given a persistence metaReducer — the in-memory token rule (VP7) stands. Still one deployable unit, no Module Federation (that is `platform-host`). The [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/plateau-design-system.skill/plateau-design-system.skill.md|design-system]] npm package remains a plain dependency of `apps/platform-shell`.

# Structure

## Workspace Structure

```
/apps
  /[platform-shell](./platform-shell/plateau-persisted-state-monolith--project-platform-shell.skill.md)
    /src/app/preloading   <- solution-performance-tuned-routing (VP1): SelectivePreloadingStrategy
    /src/sw-src.ts        <- new (solution-offline-first / VP4): Workbox runtime routing rules
    /src/sw-routes.ts     <- new: content-type routing predicates (unit-tested)
    /src/sw-build.mjs     <- new: Nx build-sw step (esbuild + workbox-build injectManifest)
    /tsconfig.sw.json     <- new: WebWorker-lib tsconfig for the SW, excluded from the app build
  /[platform-shell-e2e](./platform-shell-e2e/plateau-persisted-state-monolith--project-platform-shell-e2e.skill.md)
  /component-preview      <- solution-ui-testing, harness for behavioral/visual/a11y component specs

/apps/platform-shell/src/app
    global-error-handler.ts   <- new (VP6 / solution-logging-global): ErrorHandler → LoggerService.error

/libs
  /shared
    /[ui](./shared-ui/plateau-persisted-state-monolith--project-shared-ui.skill.md)          <- offline-banner/ (VP4) + pending-sync-indicator/ (VP5)
    /[util](./shared-util/plateau-persisted-state-monolith--project-shared-util.skill.md)
    /[state](./shared-state/plateau-persisted-state-monolith--project-shared-state.skill.md)   <- connectivity/ (VP4) + notifications/ (VP5) + auth/ (VP7) + persistence/ + preferences/ (VP8 — persisted to localStorage) slices
    /[http-core](./shared-http-core/plateau-persisted-state-monolith--project-shared-http-core.skill.md)   <- + OfflineTransportError (VP4)
    /[offline-sync](./shared-offline-sync/plateau-persisted-state-monolith--project-shared-offline-sync.skill.md)   <- VP5: Dexie mutation queue + replay orchestrator, type:store
    /[auth-ui](./shared-auth-ui/plateau-persisted-state-monolith--project-shared-auth-ui.skill.md)   <- NEW (VP7): *hasPermission, requirePermission, login form + forbidden page, type:store
    /[logging](./shared-logging/plateau-persisted-state-monolith--project-shared-logging.skill.md)   <- + backend-log-sink.ts + log-retry-queue.ts + LoggerService.report() (VP6)
  /{feature}
    /[feature](./feature-feature/plateau-persisted-state-monolith--project-feature-feature.skill.md)   <- {feature}.offline-sync.ts (VP5); a guarded sub-route via requirePermission(...) (VP7); an opt-in {feature}-draft.store.ts (VP8)
    /[data-access](./feature-data-access/plateau-persisted-state-monolith--project-feature-data-access.skill.md)   <- {feature}.client.ts status-0 → OfflineTransportError (VP4); {feature}.facade.ts enqueues (VP5)
```

- **No new Nx project vs `plateau-multiuser-monolith`.** VP8 (`solution-persisted-state`) adds `libs/shared/state/src/lib/persistence/` (`persisted-state.ts` — `persistKeys()` + `SENSITIVE_STATE_KEYS` + `assertPersistable()`; `with-persisted-draft.ts` — the `signalStoreFeature`) and `libs/shared/state/src/lib/preferences/` (the persisted slice), and registers `preferences` in `store.config.ts` via the three-arg `provideState(name, reducer, { metaReducers })`. A feature opts in by adding its own `{feature}-draft.store.ts`.
- `apps/platform-shell-e2e` hosts Playwright scenario-level specs. Tagged `type:e2e`, `scope:platform`.
- Every project's unit/component tests run via Vitest — Karma and Jest are not permitted as a project's test runner.

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/platform-shell/plateau-persisted-state-monolith--project-platform-shell.skill\|project-platform-shell]] | The only deployable unit at this plateau. Composition root: bootstraps the app, owns top-level routing, registers root providers. |
| /apps/platform-shell-e2e | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/platform-shell-e2e/plateau-persisted-state-monolith--project-platform-shell-e2e.skill\|project-platform-shell-e2e]] | Playwright end-to-end scenario specs against the real built application. |
| /apps/component-preview | — | Minimal harness rendering components in isolation with static example data — the target for visual/a11y specs. Tagged `type:preview`, `scope:platform`. Excluded from production deploy. |
| /libs/shared/ui | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/shared-ui/plateau-persisted-state-monolith--project-shared-ui.skill\|project-shared-ui]] | Reusable, app-specific UI composed from design-system primitives. |
| /libs/shared/util | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/shared-util/plateau-persisted-state-monolith--project-shared-util.skill\|project-shared-util]] | Framework-agnostic pure helpers shared across features. |
| /libs/shared/state | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/shared-state/plateau-persisted-state-monolith--project-shared-state.skill\|project-shared-state]] | Classical NgRx Store. Hosts `connectivity` (VP4), `notifications` (VP5), the `auth` slice + `AuthFacade` + `authInterceptor` (VP7), plus (VP8) the `persistence/` mechanism (`persistKeys()` + `SENSITIVE_STATE_KEYS` + `withPersistedDraft()`) and the persisted `preferences` slice. |
| /libs/shared/http-core | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/shared-http-core/plateau-persisted-state-monolith--project-shared-http-core.skill\|project-shared-http-core]] | Base HTTP service + the shared `OfflineTransportError` every feature's Client throws on a network-level failure. |
| /libs/shared/offline-sync | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/shared-offline-sync/plateau-persisted-state-monolith--project-shared-offline-sync.skill\|project-shared-offline-sync]] | VP5. `type:store`. Dexie `MutationQueueService` + `ReplayOrchestrator` + `MutationReplayRegistry`. |
| /libs/shared/auth-ui | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/shared-auth-ui/plateau-persisted-state-monolith--project-shared-auth-ui.skill\|project-shared-auth-ui]] | NEW (VP7). `type:store`. `*hasPermission` directive, `requirePermission` guard factory, login form + forbidden page. Reads the `auth` slice; never imports a feature. |
| /libs/shared/logging | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/shared-logging/plateau-persisted-state-monolith--project-shared-logging.skill\|project-shared-logging]] | `LoggerService` + `ConsoleLogSink`, plus (VP6) `BackendLogSink` (batched warn/error/report), a bounded IndexedDB `LogRetryQueue`, and `LoggerService.report()`. |
| /libs/{feature}/feature | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/feature-feature/plateau-persisted-state-monolith--project-feature-feature.skill\|project-feature-feature]] | Generic template: routed, presentational + container components (including forms and their component tests), feature-level Signal Store, own root-relative routes. |
| /libs/{feature}/data-access | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/feature-data-access/plateau-persisted-state-monolith--project-feature-data-access.skill\|project-feature-data-access]] | Generic template: this feature's Facade/Client/Mapper/Errors layering, each with its own unit-test pattern. |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/platform-shell-e2e.project.create.md|platform-shell-e2e.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/PlatformComponents/component-preview.project.create.md|PlatformComponents/component-preview.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create.md|shared-auth-ui.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/Repository.extend.md|Repository.extend]]

## Nx Tag Taxonomy

| Axis | Values | Meaning |
| ----- | ------- | ------- |
| `type` | `app`, `e2e`, `preview`, `feature`, `data-access`, `ui`, `util`, `store` | What role the project plays |
| `scope` | `platform`, `shared`, `{feature-name}` (e.g. `orders`) | Which business area the project belongs to |

`@nx/enforce-module-boundaries` allow-list:

| type | may depend on |
| ----- | -------------- |
| `app` | `type:feature`, `type:ui`, `type:util`, `type:store` (the composition root is not scope-restricted) |
| `e2e` | nothing |
| `preview` | `type:feature`, `type:ui`, `type:util`, `type:store`, `type:data-access` (a preview provides a stubbed Facade) |
| `feature` | `type:data-access` (same `scope`), `type:ui`/`type:util`/`type:store` (`scope:shared`) |
| `data-access` | `type:util` (`scope:shared`), `type:data-access` (`scope:shared` — the base `http-core`), `type:store` (`scope:shared` — the Facade enqueues via `libs/shared/offline-sync`, VP5) |
| `ui` (scope:shared) | `type:ui`, `type:util` (`scope:shared`) |
| `util` (scope:shared) | `type:util`; `type:data-access` / `type:store` (`scope:shared` — VP6: `libs/shared/logging`'s `BackendLogSink` uses the base http-core service) |
| `store` (scope:shared) | `type:util`, `type:data-access`, `type:store` (`scope:shared` — VP5: `offline-sync` reads slices; VP7: `auth-ui` reads the auth slice, `shared-state`'s `AuthFacade` uses http-core) |

Scope axis: `scope:shared` → only `scope:shared`; a feature scope (`scope:orders`) → its own scope + `scope:shared`; `scope:platform` adds no scope constraint (it is the composition root). The gap rows (`data-access → data-access`, `preview → data-access`) and every `type:store` / `type:util → data-access|store` addition (VP5–VP7) are places the V1 `solution-repository-structure` allow-list did not state — the `scope:shared → scope:shared` rule keeps each to the shared primitives. See the [example README](../plateau-persisted-state-monolith.skill/example/README.md).

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/PlatformComponents/component-preview.project.create.md|PlatformComponents/component-preview.project.create]]

## Cross-cutting conventions

- **Three-tier state placement**: component Signal → feature Signal Store → global NgRx Store, promoted upward only when a second, unrelated consumer genuinely needs it.
- **Hierarchical route ownership**: the shell only knows first-level root segments; a feature only knows paths relative to its own root; the parent assigns the mount segment.
- **Facade/Client/Mapper layering**: every feature's `data-access` lib is Facade (public API, business validation) → Client (internal transport + DTO mapping) → shared `libs/shared/http-core`.
- **Single logging seam**: everything logs through `LoggerService`, currently forwarding only to `ConsoleLogSink` — no direct `console.*` call anywhere else.
- **Business-layer testing**: every Nx project runs unit tests via Vitest; end-to-end tests are Playwright specs in `apps/platform-shell-e2e`; `HttpTestingController` is used only inside a feature's own `{feature}.client.ts` spec — every other business layer fakes the layer directly beneath it; CI enforces a minimum coverage threshold as a hard error.
- **UI-layer testing**: a component is tested independently of business logic, at three layers — behavioral (Testing Library), visual (Playwright screenshot against `apps/component-preview`), accessibility (`@axe-core/playwright`) — never with a faked Facade/Client, per [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]].
- **Code-loading strategy** *(VP1)*: every feature is lazy via `loadChildren` at its mount point (baseline); on top of that, the shell opts a reviewed subset of top-level segments into background preloading via `data: { preload: true }` + `SelectivePreloadingStrategy`, and a feature splits a genuinely heavy or rarely-visited sub-route into its own `loadComponent` chunk. Bundle budgets (`error`-level) on the app build catch an accidental non-lazy import before it ships.
- **Offline read resilience** *(VP4)*: a Workbox service worker precaches the app shell + lazy chunks and applies four content-type rules — precache (shell), cache-first (images/fonts), stale-while-revalidate (API GETs), network-only (auth + every non-GET). `libs/shared/state`'s `connectivity` slice derives `isOnline` from `navigator.onLine` events **and** a periodic `HEAD /health` (either reporting offline is enough). Every feature's Client maps an `HttpErrorResponse` with `status === 0` to the shared `OfflineTransportError` before any status-code handling. `OfflineBannerComponent` is mounted once at the shell.
- **Offline write queue** *(VP5)*: `libs/shared/offline-sync` holds a Dexie `MutationQueueService` — partitioned by feature, FIFO within a partition, each entry with a stable idempotency key and its `touchedFields`. A Facade **explicitly opts an operation into queueing**: on `OfflineTransportError` it enqueues and returns `{ queued: true }`. `ReplayOrchestrator` replays partitions concurrently on connectivity restoration; conflicts default to **server-wins**, surfaced field-scoped via the `notifications` slice. Each feature registers its replay handler with `MutationReplayRegistry` from its own route `providers`. Each feature-store row carries a **per-entity `syncStatus`** driven by the orchestrator's `onReplayStart` / `onReplayResult` callbacks; the indicator count is *derived* from those rows, rebuilt from the queue on a cold start via `hydratePending()`.
- **Backend log delivery** *(new — VP6)*: `BackendLogSink` joins `ConsoleLogSink` on the `LOG_SINKS` seam — no existing call site changes. Only `warn` / `error` / `report()` reach it; entries are batched and flushed on a timer / size threshold, with a `navigator.sendBeacon` flush on `pagehide`. A failed flush goes to `LogRetryQueue` — a bounded IndexedDB queue (count / age / size limits, oldest-first eviction, retry stops on the first failure per cycle). `LoggerService.report()` always reaches the backend regardless of `MIN_LOG_LEVEL`. A `GlobalErrorHandler` routes every uncaught exception through `LoggerService.error` with only sanitized fields (`message`, `stack`).
- **Authentication** *(VP7)*: the `auth` slice in `libs/shared/state` holds the access token **in memory only** (never `localStorage`/`sessionStorage`); the refresh token is an `HttpOnly` cookie the client never reads. Bootstrap dispatches exactly one `Silent Refresh Requested` before any authenticated request. `authInterceptor` attaches the bearer and, on a 401, dispatches one silent refresh (never intercepting the refresh call itself). Every authorization check is a **permission string**, never a role — via `requirePermission('...')` (a functional guard attached at the feature's own route) or `*hasPermission="'...'"` (a directive). Hiding UI is a convenience, not a security boundary.
- **Persisted state** *(new — VP8)*: persistence is opt-in per slice / per feature store, declaring a **finite key allow-list** — never `*`. `libs/shared/state/src/lib/persistence/` holds the mechanism: `persistKeys()` (a per-feature `MetaReducer` that rehydrates synchronously on store init and writes the allow-listed keys back debounced) and `withPersistedDraft()` (the feature-tier `signalStoreFeature`, rehydrating in `withHooks({ onInit })`). Both call `assertPersistable()`, which **throws at construction** if the allow-list names any key on `SENSITIVE_STATE_KEYS` (`accessToken`, `refreshToken`). The `auth` slice is never given a persistence metaReducer. `localStorage` is the default backend (`sessionStorage` for per-tab, Dexie for a large draft). The persisted `preferences` slice (theme / density / last feature tab) is the reference consumer; a feature's `{Feature}DraftStore` keeps an in-progress form across a reload, cleared on submit success.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/PlatformComponents/component-preview.project.create.md|PlatformComponents/component-preview.project.create]]

# Rules

## MUST
- Every Nx project must declare exactly one `type:*` tag and exactly one `scope:*` tag.
- Every lib must expose its public API through a single `index.ts` barrel.
- Business logic must never live in `apps/platform-shell`.
- Every routable `type:feature` project must export its `Routes` array from `index.ts`.
- New forms must use Signal Forms by default.
- A Client must catch every `HttpErrorResponse` and rethrow a typed domain error.
- Every part of the application must log through `LoggerService` — no direct `console.*` call outside `libs/shared/logging`'s own `ConsoleLogSink` and each app's `src/main.ts` bootstrap catch.
- Every Nx project must run its unit tests via Vitest — no project may configure Karma or Jest as its test runner.
- End-to-end tests must be written with Playwright, in the dedicated `type:e2e` project.
- `HttpTestingController` must be used only inside a feature's own `{feature}.client.ts` unit tests.
- MSW must be used only for tests that deliberately span more than one architectural layer.
- CI must enforce a minimum code-coverage threshold per project as a hard `error`.
- Every UI component must have a behavioral (Testing Library), visual (Playwright screenshot), and accessibility (`@axe-core/playwright`) spec, per [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] — none of the three substitutes for another.
- A component test must never fake a Facade/Client or use `HttpTestingController` — that concern belongs to `solution-app-testing`, not UI-level tests.
- The router must be configured with `withPreloading(SelectivePreloadingStrategy)` — never `PreloadAllModules` and never the default `NoPreloading`.
- `data: { preload: true }` must appear only at a segment's mounting point (the shell's `app.routes.ts`) — never inside a feature's own exported `Routes`; a feature must never opt itself into preloading.
- `apps/platform-shell` must declare an initial-bundle budget and a per-script budget, both with an `error` threshold (not just `maximumWarning`), on the production build.
- The service worker must be generated via Workbox's programmatic build API integrated into the Nx build pipeline — never `@angular/service-worker` (ngsw), never a webpack plugin.
- Auth endpoints and every non-GET request must be `network-only` in the SW routing rules — never cached — and that rule must be registered so it wins over the API-reads (stale-while-revalidate) rule.
- Every feature's `{feature}.client.ts` must check for a network-level failure (`HttpErrorResponse` with `status === 0`) **before** any status-code-specific handling, and throw the shared `OfflineTransportError` (defined once in `libs/shared/http-core`, never per feature).
- `selectIsOnline` must require both `navigator.onLine` and the latest `HEAD /health` result to agree; feature code must read `selectIsOnline`, never `navigator.onLine` directly.
- A Facade must opt an operation into queueing **explicitly** — catch `OfflineTransportError` and call `MutationQueueService.enqueue`; queueing is never automatic for every method. A queued op's return type must distinguish "queued for later" from a completed result (e.g. `{ queued: true }`).
- Business validation must always run and fail **before** any queueing decision — an operation whose validation already failed must never be enqueued.
- Every queued mutation must carry a client-generated idempotency key, generated once at enqueue and reused unchanged across every replay; the queue must be partitioned by feature and read/replayed FIFO within a partition.
- Replay must process feature partitions concurrently; a failure in one partition must never stop or delay another's replay.
- `handleConflict` must be a single, separately named seam in `ReplayOrchestrator` — not inlined into the replay loop — so a future solution can override it; on conflict the discarded change's notification must include only the touched fields' current server values, never the full entity.
- `libs/shared/offline-sync` must never implement per-operation or per-field conflict logic beyond server-wins (deferred to a future solution); the replay must call the app's own Facade methods, never a generic document-sync protocol.
- Feature code must never read `navigator.onLine` or the Dexie table directly — `selectIsOnline` for connectivity; the per-entity `syncStatus` on the feature store's rows (set from the Facade's `{ queued: true }` result + the replay callbacks) for pending state, with the indicator count *derived* from it.
- `BackendLogSink` must forward `warn` / `error` / `report` entries only — `debug` / `info` never reach it. A failed flush must go to `LogRetryQueue`, never be dropped. The unload flush must use `navigator.sendBeacon`.
- `LogRetryQueue` must be IndexedDB-persisted and bounded by count, age AND size (each enforced independently, oldest-first eviction); a retry cycle stops at the first failure.
- `LoggerService.report()` must always reach `BackendLogSink` regardless of `MIN_LOG_LEVEL`, and is still subject to the never-log-sensitive-data rule.
- `GlobalErrorHandler` must be registered exactly once, at `apps/platform-shell`'s root, and extract only `message` / `stack` — never the raw error object.
- The access token must live only in the `auth` slice's in-memory field — never `localStorage` / `sessionStorage` / any persistent storage. Reload recovery is silent-refresh-on-bootstrap, not persistence.
- Bootstrap must dispatch exactly one `Silent Refresh Requested` before any authenticated request.
- `authInterceptor` must be the only place an outgoing request gets an `Authorization` header, must dispatch a single silent refresh on a 401 (not an immediate logout), and must never intercept the silent-refresh request itself.
- Every authorization check (guard or directive) must be a **permission string**, never a role name. A permission guard must live inside the feature it protects — never centralized in `apps/platform-shell`'s root routes.
- Persistence must be opt-in per slice / per feature store with a **finite key allow-list** — never `*`, never the whole slice. `persistKeys()` / `withPersistedDraft()` must call `assertPersistable()`, which throws at construction on any `SENSITIVE_STATE_KEYS` key.
- The `auth` slice must never be given a persistence metaReducer; no token key may appear on any allow-list.
- A persisted slice must be rehydrated synchronously before the first render that reads it — a metaReducer for a slice, `withHooks({ onInit })` for a feature store — never a post-render patch.
- The `persistKeys` metaReducer must be attached via the three-arg `provideState(feature.name, feature.reducer, { metaReducers })` — the two-arg `provideState(feature, { metaReducers })` form silently ignores the config.

## SHOULD
- New business features should be scaffolded as a `{feature}/feature` + `{feature}/data-access` pair from the start.
- Never migrate working Reactive Forms code to Signal Forms purely for consistency.
- Never place a routed business feature directly under `/apps` — scaffold a lib under `/libs/{feature}` and route to it lazily; a feature under `/apps` can no longer be reused or lazy-loaded independently.
- Never let a component or Signal Store method import a feature's Client directly, bypassing the Facade — the same HTTP concern then gets asserted two inconsistent ways.
- Never call `console.*` directly from feature code — it bypasses `LoggerService`'s single seam for level filtering and the future backend sink; call `LoggerService` instead.
- Never use `HttpTestingController` inside a Facade or Signal Store test "to save faking the Client" — fake the Client directly in a Facade test, and the Facade directly in a Signal Store test.
- Never lower a coverage threshold or raise a bundle budget just to clear a CI failure without investigating the cause — a genuine regression then goes unnoticed; adjust the limit only as a deliberate, reviewed decision.
- Never mark every top-level segment `preload: true` "to be safe" — that degenerates into `PreloadAllModules`, including prefetching remote chunks the shell never intended to warm up.
- Never split every sub-route via `loadComponent` by default — each split is an extra network round-trip; split only genuinely heavy or rarely-visited sub-routes.
- Never treat a `status === 0` failure the same as a server error — the Facade needs "we're offline, retryable" apart from "the server rejected this".
- Never let a feature build its own local offline / pending indicator — mount the shared `OfflineBannerComponent` (shell) and `PendingSyncIndicatorComponent` (per feature); the per-row `syncStatus` badge is the feature's own, from `<ui-status-badge>`.
- Never rely on `navigator.onLine` alone — it misrepresents reachability behind a captive portal or a backend outage.
- Never enqueue every `OfflineTransportError` unconditionally — a one-time or time-sensitive action queued and replayed much later can produce a wrong result; each Facade decides which of its operations are queueable.
- Never retry a stuck partition's failed entry immediately within the same cycle — stop that partition; the next connectivity-restoration event retries.
- Never query the whole `queuedMutations` table and filter in code — always go through the `feature` index.
- Never use `report()` as a substitute for `error()` — it blurs severity in backend log queries.
- Never centralize permission guards in the shell "for visibility" — reintroduces the coupling hierarchical route ownership prevents.
- Never rely on `*hasPermission` alone to protect a destructive action with no server-side check.
- Never retry the original request indefinitely on repeated 401s — one silent refresh, then treat as logged out.
- Never persist a slice "to be safe" when nothing needs to survive a reload — a future sensitive field added to that slice is then one review away from leaking; persist only state a user would be annoyed to lose.
- Never reach for a generic third-party `localStorage` sync library — it turns the mandatory allow-list and the `SENSITIVE_STATE_KEYS` guard back into configuration discipline.
- Never add `withPersistedDraft` to a feature's main list/detail store — it is a draft mechanism; persisted server data goes stale with no invalidation. Use a dedicated `{Feature}DraftStore`.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Repository.extend.md|Repository.extend]]

# Unittest TestCases

- [ ] WHEN `nx run-many -t lint` is executed THEN
  - [ ] `@nx/enforce-module-boundaries` reports no violations
- [ ] WHEN the codebase is searched for direct `console.*` calls outside `libs/shared/logging` and `**/src/main.ts` THEN
  - [ ] none are found
- [ ] WHEN a project's test configuration is inspected THEN
  - [ ] it runs via Vitest, not Karma or Jest
- [ ] WHEN the codebase is searched for `HttpTestingController` usage THEN
  - [ ] every occurrence is inside a `{feature}.client.ts` spec file
- [ ] WHEN CI runs on a PR that drops coverage below the configured threshold THEN
  - [ ] the build fails, not just warns
- [ ] WHEN a non-lazy import accidentally pulls feature code into the initial bundle THEN
  - [ ] `nx build platform-shell --configuration=production` fails on the budget with an `error`, not a warning
- [ ] WHEN every feature's exported `Routes` are inspected THEN
  - [ ] none set `data: { preload: true }` on themselves
- [ ] WHEN the SW routing config is inspected THEN
  - [ ] every auth/mutation request resolves to `network-only`, never to stale-while-revalidate
- [ ] WHEN a Client's HTTP call fails with `status === 0` (no response) THEN
  - [ ] it throws the shared `OfflineTransportError`, not a feature domain error
- [ ] WHEN a Client's HTTP call fails with a real server response (e.g. 500) THEN
  - [ ] it throws the feature's own typed error, unaffected by VP4
- [ ] WHEN the browser fires `offline`, OR the health check fails THEN
  - [ ] `selectIsOnline` becomes `false`
- [ ] WHEN `nx build-sw platform-shell` completes THEN
  - [ ] `dist/apps/platform-shell/browser/sw.js` exists and carries the four routing rules
- [ ] WHEN a queueable operation's Client call throws `OfflineTransportError` THEN
  - [ ] the Facade enqueues it (with a generated idempotency key) and returns `{ queued: true }`, not a throw
- [ ] WHEN business validation fails before the Client is called THEN
  - [ ] the Facade throws its validation error and enqueues nothing
- [ ] WHEN two features have pending mutations and one feature's replay fails repeatedly THEN
  - [ ] the other feature's partition still completes its replay
- [ ] WHEN a replayed entry receives a conflict response THEN
  - [ ] the local change is discarded (server wins) and a notification is dispatched with only the touched fields
- [ ] WHEN a mutation is enqueued and the app reloads THEN
  - [ ] it persists (Dexie/IndexedDB); `hydratePending()` rebuilds the optimistic row with `syncStatus: 'queued'` on the next cold start
- [ ] WHEN `nx run-many -t lint` runs THEN
  - [ ] no static import of a lazy-loaded library (the shell never statically imports a feature)
- [ ] WHEN a `debug` / `info` entry is logged THEN
  - [ ] it never reaches `BackendLogSink`, only `ConsoleLogSink`
- [ ] WHEN a `BackendLogSink` flush fails THEN
  - [ ] the batch is enqueued in `LogRetryQueue`; on the next flush with the network back, it is sent and removed
- [ ] WHEN `LogRetryQueue` exceeds any of its count / age / size limits THEN
  - [ ] the oldest entries are evicted until all three are satisfied
- [ ] WHEN an uncaught exception is thrown THEN
  - [ ] `GlobalErrorHandler` routes it through `LoggerService.error` with only `message` / `stack`
- [ ] WHEN the codebase is searched for a token value written to `localStorage` / `sessionStorage` THEN
  - [ ] none are found
- [ ] WHEN a request is made with a valid access token THEN
  - [ ] it carries `Authorization: Bearer <token>`; a 401 dispatches exactly one `Silent Refresh Requested`; the silent-refresh request itself carries no bearer
- [ ] WHEN a user without the required permission navigates to a guarded route THEN
  - [ ] `requirePermission(...)` redirects to `/forbidden`; `*hasPermission` clears the element
- [ ] WHEN any guard / directive is inspected THEN
  - [ ] its authorization check references a permission string, never a role name
- [ ] WHEN `persistKeys()` / `withPersistedDraft()` is constructed with an allow-list containing `accessToken` THEN
  - [ ] it throws at construction, failing the suite
- [ ] WHEN `localStorage` holds `app:preferences` = `{"theme":"dark"}` and the store initializes THEN
  - [ ] the initial `preferences.theme` selector is `'dark'` before any component subscribes
- [ ] WHEN the codebase is searched for a persistence metaReducer on the `auth` feature THEN
  - [ ] none is found
- [ ] WHEN a persisted form draft is filled and the page reloads THEN
  - [ ] the `{Feature}DraftStore` rehydrates the fields; a successful submit then clears it

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/Repository.extend.md|Repository.extend]]
