---
name: plateau-async-monolith--repo-async-monolith
description: Nx workspace layout for the async-monolith plateau — online-monolith plus lazy-loaded feature chunks with a selective preloading strategy, and read-resilience against an unreliable network via a service worker and a connectivity slice
domain: skill
type: template
plateau: async-monolith
version: 20260711190000
tags:
  - skill/template/repo
  - plateau/async-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
---

> Second plateau in the main application's chain. Parent: [[skills/angular/architecture/plateau/plateau-online-monolith/plateau-online-monolith|online-monolith]]. Next: [[skills/angular/architecture/plateau/plateau-offline-monolith/plateau-offline-monolith|offline-monolith]]. This is the **"async-monolith"** milestone: feature chunks are lazy-loaded and selectively preloaded, and the app keeps working against cached GET data — with a visible offline indicator — when the network is unreliable. Auth/mutation endpoints stay strictly `network-only`; a genuinely offline mutation still fails immediately (durable queueing and replay arrive with [[skills/angular/architecture/plateau/plateau-offline-monolith/plateau-offline-monolith|offline-monolith]]). Still no authentication, no Module Federation, no backend log delivery.

# Structure

## Workspace Structure

```
/apps
  /[platform-shell](./platform-shell/plateau-async-monolith--project-platform-shell.skill.md)
  /[platform-shell-e2e](./platform-shell-e2e/plateau-async-monolith--project-platform-shell-e2e.skill.md)

/libs
  /shared
    /[ui](./shared-ui/plateau-async-monolith--project-shared-ui.skill.md)
    /[util](./shared-util/plateau-async-monolith--project-shared-util.skill.md)
    /[state](./shared-state/plateau-async-monolith--project-shared-state.skill.md)
    /[http-core](./shared-http-core/plateau-async-monolith--project-shared-http-core.skill.md)
    /[logging](./shared-logging/plateau-async-monolith--project-shared-logging.skill.md)
  /{feature}
    /[feature](./feature-feature/plateau-async-monolith--project-feature-feature.skill.md)
    /[data-access](./feature-data-access/plateau-async-monolith--project-feature-data-access.skill.md)
```

- No new top-level project at this plateau — every directory already existed at `online-monolith` and is extended here: feature chunks become lazy, `platform-shell` gains a preloading strategy and service-worker registration, `shared/state` gains a `connectivity` slice, `shared/ui` gains an offline banner.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/Repository.extend|Repository.extend]]

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[skills/angular/architecture/plateau/plateau-async-monolith/structure/platform-shell/plateau-async-monolith--project-platform-shell.skill\|project-platform-shell]] | Composition root: top-level routing, selective preloading strategy, service-worker registration. Still no business logic. |
| /apps/platform-shell-e2e | [[skills/angular/architecture/plateau/plateau-async-monolith/structure/platform-shell-e2e/plateau-async-monolith--project-platform-shell-e2e.skill\|project-platform-shell-e2e]] | Playwright end-to-end scenario specs, now including an offline-read scenario. |
| /libs/shared/ui | [[skills/angular/architecture/plateau/plateau-async-monolith/structure/shared-ui/plateau-async-monolith--project-shared-ui.skill\|project-shared-ui]] | Reusable, app-specific UI, now including the offline banner. |
| /libs/shared/util | [[skills/angular/architecture/plateau/plateau-async-monolith/structure/shared-util/plateau-async-monolith--project-shared-util.skill\|project-shared-util]] | Framework-agnostic pure helpers shared across features. |
| /libs/shared/state | [[skills/angular/architecture/plateau/plateau-async-monolith/structure/shared-state/plateau-async-monolith--project-shared-state.skill\|project-shared-state]] | Classical NgRx Store, now including the `connectivity` slice (`isOnline`). |
| /libs/shared/http-core | [[skills/angular/architecture/plateau/plateau-async-monolith/structure/shared-http-core/plateau-async-monolith--project-shared-http-core.skill\|project-shared-http-core]] | Base HTTP service; now also hosts the shared `OfflineTransportError` type. |
| /libs/shared/logging | [[skills/angular/architecture/plateau/plateau-async-monolith/structure/shared-logging/plateau-async-monolith--project-shared-logging.skill\|project-shared-logging]] | `LoggerService`, console-only — unchanged from `online-monolith`. |
| /libs/{feature}/feature | [[skills/angular/architecture/plateau/plateau-async-monolith/structure/feature-feature/plateau-async-monolith--project-feature-feature.skill\|project-feature-feature]] | Generic template: routed components split into per-route lazy chunks via `loadComponent`. |
| /libs/{feature}/data-access | [[skills/angular/architecture/plateau/plateau-async-monolith/structure/feature-data-access/plateau-async-monolith--project-feature-data-access.skill\|project-feature-data-access]] | Generic template: Facade/Client/Mapper/Errors, the Client now distinguishing an offline network failure from a server rejection. |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/Repository.extend|Repository.extend]]

## Nx Tag Taxonomy

Unchanged from `online-monolith`: `type` ∈ {`app`, `e2e`, `feature`, `data-access`, `ui`, `util`, `store`}, `scope` ∈ {`platform`, `shared`, `{feature-name}`}. The `@nx/enforce-module-boundaries` allow-list is unchanged.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]

## Cross-cutting conventions

These rules apply inside every project in the workspace and have no single project of their own to live in:

- **Three-tier state placement**, **hierarchical route ownership**, **Facade/Client/Mapper layering**, **single logging seam** — unchanged from `online-monolith`.
- **Selective preloading**: any route that should be background-preloaded carries `data: { preload: true }`, set only by the project that mounts it — never inside the feature's own exported routes.
- **Bundle budgets**: every `type:app` and routable `type:feature` project declares a budget block (`error` threshold) in its build configuration.
- **Offline-aware reads**: `isOnline`, from `shared/state`'s `connectivity` slice, is the only source of truth feature code may use to know whether the network is reachable — never `navigator.onLine` directly.
- **Offline-aware transport errors**: a feature's Client throws the shared `OfflineTransportError` when an HTTP call never reaches the server (`status === 0`), distinct from a typed domain error for an actual server rejection.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/Repository.extend|Repository.extend]]

# Rules

## MUST
- Every `type:app` project MUST declare an initial-bundle budget (`error` threshold) in its build configuration.
- Every routable `type:feature` project MUST declare a per-chunk budget for its own lazy chunk.
- `data: { preload: true }` MUST be set only at the mounting point, never inside a feature's own exported routes.
- A feature's Client MUST check for a network-level failure (`HttpErrorResponse` with `status === 0`) before any status-code-specific handling, and throw the shared `OfflineTransportError` in that case.
- `selectIsOnline` MUST be the only public selector feature code uses to read connectivity; `navigator.onLine` MUST NOT be read directly outside the `connectivity` slice's own effect.
- The service worker MUST be generated via Workbox's programmatic build API; auth endpoints and every non-GET request MUST remain `network-only`, never cached.
- All other rules from [[skills/angular/architecture/plateau/plateau-online-monolith/plateau-online-monolith|online-monolith]] continue to apply unchanged.

## SHOULD
- Bundle budget thresholds SHOULD be reviewed and adjusted deliberately when a feature's legitimate size grows, rather than silenced by raising the threshold reflexively.

## MUST NOT
- A feature or embeddable module MUST NOT set `preload: true` on its own routes.
- This plateau MUST NOT introduce any caching or offline behavior for auth/mutation endpoints — they remain `network-only` unconditionally.
- A genuinely offline mutation MUST NOT be silently queued at this plateau — durable queueing does not exist until `offline-monolith`; the Facade lets the `OfflineTransportError` surface as an immediate failure.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

- **Raising a bundle budget threshold to make a CI failure go away without investigating the cause**
  - Consequence: a genuine regression goes unnoticed
  - Instead: investigate why the bundle grew; only raise the threshold as a deliberate, reviewed trade-off
- **A feature marking its own route `preload: true` "because it's important"**
  - Consequence: bypasses the mounting point's authority over the preloading decision
  - Instead: the feature stays silent on preloading; the shell decides and sets the flag when mounting it
- **Duplicating the connectivity logic inside a feature store instead of reading `selectIsOnline`**
  - Consequence: multiple, potentially disagreeing sources of truth for online status
  - Instead: rely on `libs/shared/state`'s `connectivity` slice everywhere
- **Configuring auth or mutation endpoints with any service-worker caching strategy other than network-only**
  - Consequence: a cached auth/mutation response is a correctness and security bug, not just staleness
  - Instead: auth and all non-GET requests are always `network-only`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/Repository.extend|Repository.extend]]

# Unittest TestCases

- [ ] WHEN a non-lazy import accidentally pulls feature code into the initial bundle THEN
  - [ ] the `type:app` project's initial-bundle budget fails the build with an error
- [ ] WHEN a feature's own exported `Routes` are inspected THEN
  - [ ] none of them set `data: { preload: true }` on themselves
- [ ] WHEN a feature's Client makes a request while the network is genuinely unreachable THEN
  - [ ] it throws the shared `OfflineTransportError`, not that feature's generic domain error
- [ ] WHEN the service worker's routing configuration is inspected THEN
  - [ ] every auth/mutation endpoint resolves to `network-only`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/Repository.extend|Repository.extend]]
