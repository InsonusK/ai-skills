---
name: plateau-offline-read-monolith--repo-offline-read-monolith
description: Nx workspace layout for the offline-read-monolith Angular application — the async-monolith workspace plus a Workbox service worker (four content-type caching rules), a connectivity slice in libs/shared/state with an accurate isOnline signal, a shared OfflineTransportError in libs/shared/http-core, and a shell-mounted offline banner. Reads survive offline; a write attempted offline still fails immediately. Still one deployable unit; no offline write queue, no Module Federation, no authentication.
domain: skill
type: template
plateau: offline-read-monolith
version: 20260903090000
tags:
  - skill/template/repo
  - plateau/offline-read-monolith
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
  - "[[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]]"

> **Third plateau in the monolith chain.** Composes [[skills/angular/architecture/v3.1/monolith/plateau/plateau-async-monolith/structure/plateau-async-monolith--repo-async-monolith.skill.md|plateau-async-monolith]] (which already carries `plateau-online-monolith` + VP1) and adds exactly one solution — [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] (monolith **VP4 — OfflineReadResilience**). No new Nx project: a Workbox service worker is added to `apps/platform-shell`, a `connectivity` slice to `libs/shared/state`, a shared `OfflineTransportError` to `libs/shared/http-core`, an `OfflineBannerComponent` to `libs/shared/ui`, and the `status === 0` branch to every feature's Client. Scope is **read resilience only** — the shell always loads, API GETs fall back to last-known data; a mutation attempted offline still **fails immediately** (`OfflineTransportError`), for which this only prepares the hook. The durable write queue is [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/plateau-offline-full-monolith.skill/plateau-offline-full-monolith.skill.md|plateau-offline-full-monolith]] (VP5). No Module Federation, no backend log delivery, no authentication ([[skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/plateau-multiuser-monolith.skill/plateau-multiuser-monolith.skill.md|multiuser-monolith]] is last), every user still implicitly trusted. The [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/plateau-design-system.skill/plateau-design-system.skill.md|design-system]] npm package remains a plain dependency of `apps/platform-shell`.

# Structure

## Workspace Structure

```
/apps
  /[platform-shell](./platform-shell/plateau-offline-read-monolith--project-platform-shell.skill.md)
    /src/app/preloading   <- solution-performance-tuned-routing (VP1): SelectivePreloadingStrategy
    /src/sw-src.ts        <- new (solution-offline-first / VP4): Workbox runtime routing rules
    /src/sw-routes.ts     <- new: content-type routing predicates (unit-tested)
    /src/sw-build.mjs     <- new: Nx build-sw step (esbuild + workbox-build injectManifest)
    /tsconfig.sw.json     <- new: WebWorker-lib tsconfig for the SW, excluded from the app build
  /[platform-shell-e2e](./platform-shell-e2e/plateau-offline-read-monolith--project-platform-shell-e2e.skill.md)
  /component-preview      <- solution-ui-testing, harness for behavioral/visual/a11y component specs

/libs
  /shared
    /[ui](./shared-ui/plateau-offline-read-monolith--project-shared-ui.skill.md)          <- + offline-banner/ (VP4)
    /[util](./shared-util/plateau-offline-read-monolith--project-shared-util.skill.md)
    /[state](./shared-state/plateau-offline-read-monolith--project-shared-state.skill.md)   <- + connectivity/ slice (VP4)
    /[http-core](./shared-http-core/plateau-offline-read-monolith--project-shared-http-core.skill.md)   <- + OfflineTransportError (VP4)
    /[logging](./shared-logging/plateau-offline-read-monolith--project-shared-logging.skill.md)
  /{feature}
    /[feature](./feature-feature/plateau-offline-read-monolith--project-feature-feature.skill.md)   <- {feature}.routes.ts gains loadComponent sub-splitting
    /[data-access](./feature-data-access/plateau-offline-read-monolith--project-feature-data-access.skill.md)   <- {feature}.client.ts gains the status-0 → OfflineTransportError branch (VP4)
```

- No new Nx project. `solution-performance-tuned-routing` (VP1) added the `preloading/` strategy + bundle budgets. `solution-offline-first` (VP4) adds the service worker + build step to `apps/platform-shell`, the `connectivity` slice to `libs/shared/state`, `OfflineTransportError` to `libs/shared/http-core`, `OfflineBannerComponent` to `libs/shared/ui`, and the network-failure branch to every feature's Client.
- `apps/platform-shell-e2e` hosts Playwright scenario-level specs. Tagged `type:e2e`, `scope:platform`.
- Every project's unit/component tests run via Vitest — Karma and Jest are not permitted as a project's test runner.

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-read-monolith/structure/platform-shell/plateau-offline-read-monolith--project-platform-shell.skill\|project-platform-shell]] | The only deployable unit at this plateau. Composition root: bootstraps the app, owns top-level routing, registers root providers. |
| /apps/platform-shell-e2e | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-read-monolith/structure/platform-shell-e2e/plateau-offline-read-monolith--project-platform-shell-e2e.skill\|project-platform-shell-e2e]] | Playwright end-to-end scenario specs against the real built application. |
| /apps/component-preview | — | Minimal harness rendering components in isolation with static example data — the target for visual/a11y specs. Tagged `type:preview`, `scope:platform`. Excluded from production deploy. |
| /libs/shared/ui | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-read-monolith/structure/shared-ui/plateau-offline-read-monolith--project-shared-ui.skill\|project-shared-ui]] | Reusable, app-specific UI composed from design-system primitives. |
| /libs/shared/util | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-read-monolith/structure/shared-util/plateau-offline-read-monolith--project-shared-util.skill\|project-shared-util]] | Framework-agnostic pure helpers shared across features. |
| /libs/shared/state | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-read-monolith/structure/shared-state/plateau-offline-read-monolith--project-shared-state.skill\|project-shared-state]] | Classical NgRx Store. Now hosts the `connectivity` slice (`selectIsOnline`); `notifications`/`auth` arrive later in the chain. |
| /libs/shared/http-core | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-read-monolith/structure/shared-http-core/plateau-offline-read-monolith--project-shared-http-core.skill\|project-shared-http-core]] | Base HTTP service + the shared `OfflineTransportError` every feature's Client throws on a network-level failure. |
| /libs/shared/logging | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-read-monolith/structure/shared-logging/plateau-offline-read-monolith--project-shared-logging.skill\|project-shared-logging]] | `LoggerService` with a `ConsoleLogSink` — console-only at this plateau. |
| /libs/{feature}/feature | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-read-monolith/structure/feature-feature/plateau-offline-read-monolith--project-feature-feature.skill\|project-feature-feature]] | Generic template: routed, presentational + container components (including forms and their component tests), feature-level Signal Store, own root-relative routes. |
| /libs/{feature}/data-access | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-read-monolith/structure/feature-data-access/plateau-offline-read-monolith--project-feature-data-access.skill\|project-feature-data-access]] | Generic template: this feature's Facade/Client/Mapper/Errors layering, each with its own unit-test pattern. |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/platform-shell-e2e.project.create.md|platform-shell-e2e.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/PlatformComponents/component-preview.project.create.md|PlatformComponents/component-preview.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/Repository.extend.md|Repository.extend]]

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
| `data-access` | `type:util` (`scope:shared`), `type:data-access` (`scope:shared` — the base `http-core`) |
| `ui` (scope:shared) | `type:ui`, `type:util` (`scope:shared`) |
| `util` (scope:shared) | `type:util` (leaf) |
| `store` (scope:shared) | `type:util`, `type:data-access` (`scope:shared`) |

Scope axis: `scope:shared` → only `scope:shared`; a feature scope (`scope:orders`) → its own scope + `scope:shared`; `scope:platform` adds no scope constraint (it is the composition root). The two `data-access`/`preview` rows above are gaps the V1 `solution-repository-structure` allow-list did not state — see the [example README](../plateau-offline-read-monolith.skill/example/README.md).

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
- **Offline read resilience** *(new — VP4)*: a Workbox service worker precaches the app shell + lazy chunks and applies four content-type rules — precache (shell), cache-first (images/fonts), stale-while-revalidate (API GETs), network-only (auth + every non-GET). `libs/shared/state`'s `connectivity` slice derives `isOnline` from `navigator.onLine` events **and** a periodic `HEAD /health` (either reporting offline is enough). Every feature's Client maps an `HttpErrorResponse` with `status === 0` to the shared `OfflineTransportError` before any status-code handling. `OfflineBannerComponent` is mounted once at the shell. A write attempted offline still fails immediately — no queue at this plateau.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/Repository.extend.md|Repository.extend]]
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
- No durable queue, retry, or persistence for failed mutations exists at this plateau — a write that fails offline surfaces as a failure; queueing is `plateau-offline-full-monolith` (VP5).

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
- Never build even a minimal mutation queue/retry as part of VP4 — a write that fails offline surfaces as a failure; the queue is VP5's whole job.
- Never treat a `status === 0` failure the same as a server error — the Facade (and the future sync queue) needs "we're offline, retryable" apart from "the server rejected this".
- Never let a feature build its own local offline indicator — mount the shared `OfflineBannerComponent` once at the shell.
- Never rely on `navigator.onLine` alone — it misrepresents reachability behind a captive portal or a backend outage.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/Repository.extend.md|Repository.extend]]

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

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/Repository.extend.md|Repository.extend]]
