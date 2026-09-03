---
name: plateau-offline-full-monolith--repo-offline-full-monolith
description: Nx workspace layout for the offline-full-monolith Angular application — the offline-read-monolith workspace plus a Dexie-backed, per-feature-partitioned mutation queue (libs/shared/offline-sync) with idempotent replay and server-wins conflict handling, a notifications slice in libs/shared/state, a per-operation Facade queueing opt-in, and a shared pending-sync indicator. Reads AND writes survive offline. Still one deployable unit; no Module Federation, no authentication.
domain: skill
type: template
whenToUse: when scaffolding the Nx workspace, adding a project or a boundary allow-list row, or checking whether a change follows this plateau's workspace-level rules (tags, boundaries, the offline write queue, per-entity syncStatus)
plateau: offline-full-monolith
version: 20260903120000
tags:
  - skill/template/repo
  - plateau/offline-full-monolith
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
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]]"

> **Fourth plateau in the monolith chain — the owner's current app.** Composes [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-read-monolith/structure/plateau-offline-read-monolith--repo-offline-read-monolith.skill.md|plateau-offline-read-monolith]] (online + VP1 + VP4) and adds exactly one solution — [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] (monolith **VP5 — OfflineWriteQueue**, per feature). VP1–VP5 = Yes; VP6–VP8 = No. One **new** Nx project — `libs/shared/offline-sync` (`type:store`): a Dexie `MutationQueueService` (per-feature partitions, stable idempotency key, `touchedFields` for conflict diffing) + a `ReplayOrchestrator` (partitions replay concurrently, FIFO within a partition, server-wins conflict seam). Also: a `notifications` slice in `libs/shared/state`, `OfflineTransportError` caught in each feature's Facade to **enqueue** a queueable operation instead of failing, and `PendingSyncIndicatorComponent` in `libs/shared/ui`. **Reads and writes both survive offline.** Next: [[skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/plateau-multiuser-monolith.skill/plateau-multiuser-monolith.skill.md|plateau-multiuser-monolith]] (VP6 + VP7). No Module Federation, every user still implicitly trusted. The [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/plateau-design-system.skill/plateau-design-system.skill.md|design-system]] npm package remains a plain dependency of `apps/platform-shell`.

# Structure

## Workspace Structure

```
/apps
  /[platform-shell](./platform-shell/plateau-offline-full-monolith--project-platform-shell.skill.md)
    /src/app/preloading   <- solution-performance-tuned-routing (VP1): SelectivePreloadingStrategy
    /src/sw-src.ts        <- new (solution-offline-first / VP4): Workbox runtime routing rules
    /src/sw-routes.ts     <- new: content-type routing predicates (unit-tested)
    /src/sw-build.mjs     <- new: Nx build-sw step (esbuild + workbox-build injectManifest)
    /tsconfig.sw.json     <- new: WebWorker-lib tsconfig for the SW, excluded from the app build
  /[platform-shell-e2e](./platform-shell-e2e/plateau-offline-full-monolith--project-platform-shell-e2e.skill.md)
  /component-preview      <- solution-ui-testing, harness for behavioral/visual/a11y component specs

/libs
  /shared
    /[ui](./shared-ui/plateau-offline-full-monolith--project-shared-ui.skill.md)          <- offline-banner/ (VP4) + pending-sync-indicator/ (VP5)
    /[util](./shared-util/plateau-offline-full-monolith--project-shared-util.skill.md)
    /[state](./shared-state/plateau-offline-full-monolith--project-shared-state.skill.md)   <- connectivity/ (VP4) + notifications/ (VP5) slices
    /[http-core](./shared-http-core/plateau-offline-full-monolith--project-shared-http-core.skill.md)   <- + OfflineTransportError (VP4)
    /[offline-sync](./shared-offline-sync/plateau-offline-full-monolith--project-shared-offline-sync.skill.md)   <- NEW (VP5): Dexie mutation queue + replay orchestrator, type:store
    /[logging](./shared-logging/plateau-offline-full-monolith--project-shared-logging.skill.md)
  /{feature}
    /[feature](./feature-feature/plateau-offline-full-monolith--project-feature-feature.skill.md)   <- {feature}.routes.ts loadComponent split; {feature}.offline-sync.ts registers the replay handler in the route injector (VP5)
    /[data-access](./feature-data-access/plateau-offline-full-monolith--project-feature-data-access.skill.md)   <- {feature}.client.ts status-0 → OfflineTransportError (VP4); {feature}.facade.ts catches it and enqueues a queueable op (VP5)
```

- **One new Nx project vs `plateau-offline-read-monolith`: `libs/shared/offline-sync`** (`type:store`, `scope:shared`). `solution-offline-sync` (VP5) also adds the `notifications` slice to `libs/shared/state`, `PendingSyncIndicatorComponent` to `libs/shared/ui`, the queueing branch to every feature's Facade, and a per-feature `{feature}.offline-sync.ts` replay registration in the feature lib.
- `apps/platform-shell-e2e` hosts Playwright scenario-level specs. Tagged `type:e2e`, `scope:platform`.
- Every project's unit/component tests run via Vitest — Karma and Jest are not permitted as a project's test runner.

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/platform-shell/plateau-offline-full-monolith--project-platform-shell.skill\|project-platform-shell]] | The only deployable unit at this plateau. Composition root: bootstraps the app, owns top-level routing, registers root providers. |
| /apps/platform-shell-e2e | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/platform-shell-e2e/plateau-offline-full-monolith--project-platform-shell-e2e.skill\|project-platform-shell-e2e]] | Playwright end-to-end scenario specs against the real built application. |
| /apps/component-preview | — | Minimal harness rendering components in isolation with static example data — the target for visual/a11y specs. Tagged `type:preview`, `scope:platform`. Excluded from production deploy. |
| /libs/shared/ui | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/shared-ui/plateau-offline-full-monolith--project-shared-ui.skill\|project-shared-ui]] | Reusable, app-specific UI composed from design-system primitives. |
| /libs/shared/util | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/shared-util/plateau-offline-full-monolith--project-shared-util.skill\|project-shared-util]] | Framework-agnostic pure helpers shared across features. |
| /libs/shared/state | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/shared-state/plateau-offline-full-monolith--project-shared-state.skill\|project-shared-state]] | Classical NgRx Store. Hosts the `connectivity` slice (`selectIsOnline`, VP4) and the `notifications` slice (`selectNotifications`, VP5); `auth` arrives with VP7. |
| /libs/shared/http-core | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/shared-http-core/plateau-offline-full-monolith--project-shared-http-core.skill\|project-shared-http-core]] | Base HTTP service + the shared `OfflineTransportError` every feature's Client throws on a network-level failure. |
| /libs/shared/offline-sync | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/shared-offline-sync/plateau-offline-full-monolith--project-shared-offline-sync.skill\|project-shared-offline-sync]] | NEW (VP5). `type:store`. Dexie `MutationQueueService` (per-feature partitions, idempotency keys, `touchedFields`) + `ReplayOrchestrator` (concurrent partitions, FIFO within, server-wins conflict seam) + `MutationReplayRegistry`. |
| /libs/shared/logging | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/shared-logging/plateau-offline-full-monolith--project-shared-logging.skill\|project-shared-logging]] | `LoggerService` with a `ConsoleLogSink` — console-only at this plateau. |
| /libs/{feature}/feature | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-feature/plateau-offline-full-monolith--project-feature-feature.skill\|project-feature-feature]] | Generic template: routed, presentational + container components (including forms and their component tests), feature-level Signal Store, own root-relative routes. |
| /libs/{feature}/data-access | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-data-access/plateau-offline-full-monolith--project-feature-data-access.skill\|project-feature-data-access]] | Generic template: this feature's Facade/Client/Mapper/Errors layering, each with its own unit-test pattern. |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/platform-shell-e2e.project.create.md|platform-shell-e2e.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/PlatformComponents/component-preview.project.create.md|PlatformComponents/component-preview.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/shared-offline-sync.project.create.md|OfflineSync/shared-offline-sync.project.create]]

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
| `util` (scope:shared) | `type:util` (leaf) |
| `store` (scope:shared) | `type:util`, `type:data-access`, `type:store` (`scope:shared` — `offline-sync` reads the `connectivity`/`notifications` slices, VP5) |

Scope axis: `scope:shared` → only `scope:shared`; a feature scope (`scope:orders`) → its own scope + `scope:shared`; `scope:platform` adds no scope constraint (it is the composition root). The `data-access`/`preview` gap rows, and the two `type:store` additions above (VP5), are places the V1 `solution-repository-structure` allow-list did not state — see the [example README](../plateau-offline-full-monolith.skill/example/README.md).

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
- **Offline write queue** *(new — VP5)*: `libs/shared/offline-sync` holds a Dexie `MutationQueueService` — the queue is partitioned by feature, FIFO within a partition, each entry carries a stable idempotency key (generated once at enqueue) and its `touchedFields`. A feature's Facade **explicitly opts an operation into queueing**: on `OfflineTransportError` it enqueues and returns `{ queued: true }` instead of throwing. `ReplayOrchestrator` replays every partition concurrently on connectivity restoration (a stuck partition never blocks another); conflicts default to **server-wins**, surfaced field-scoped via the `notifications` slice — never a full entity snapshot. Each feature registers its replay handler with `MutationReplayRegistry` from its own route `providers` (so no feature code enters the initial bundle). Each feature-store row carries a **per-entity `syncStatus`** (`queued → sending → synced | failed | conflict`) driven by the orchestrator's `onReplayStart` / `onReplayResult` callbacks; `PendingSyncIndicatorComponent`'s count is *derived* from those rows, and `{feature}.store.ts`'s `hydratePending()` rebuilds them from the persisted queue on a cold start.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/Repository.extend.md|Repository.extend]]
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

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/Repository.extend.md|Repository.extend]]

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

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/Repository.extend.md|Repository.extend]]
