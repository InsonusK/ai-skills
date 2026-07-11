---
name: repo-offline-app
description: Nx workspace layout for the offline-app plateau — routed, form-capable, HTTP-backed, authenticated, observable application that keeps working (reads and writes) while the network is unreliable
domain: skill
type: template
plateau: offline-app
version: 20260711140000
tags:
  - skill/template/repo
  - plateau/offline-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
  - "[[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]]"
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
  - "[[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
  - "[[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]]"
---

# Structure

## Workspace Structure

```
/apps
  /[platform-shell](./platform-shell/project-platform-shell.skill.md)
  /[platform-shell-e2e](./platform-shell-e2e/project-platform-shell-e2e.skill.md)

/libs
  /shared
    /[ui](./shared-ui/project-shared-ui.skill.md)
    /[util](./shared-util/project-shared-util.skill.md)
    /[state](./shared-state/project-shared-state.skill.md)
    /[http-core](./shared-http-core/project-shared-http-core.skill.md)
    /[auth-ui](./shared-auth-ui/project-shared-auth-ui.skill.md)
    /[logging](./shared-logging/project-shared-logging.skill.md)
    /[offline-sync](./shared-offline-sync/project-shared-offline-sync.skill.md)      <- new (solution-offline-sync)
  /{feature}
    /[feature](./feature-feature/project-feature-feature.skill.md)
    /[data-access](./feature-data-access/project-feature-data-access.skill.md)
```

- `{feature}` is a placeholder for each business feature (e.g. `orders`, `catalog`, `billing`); every feature has both a `feature` and a `data-access` project.
- `libs/shared/offline-sync` is the only new top-level project at this plateau — everything else already existed by the previous ("tested") milestone and is extended here.
- The design system itself remains a separate, independently versioned npm package — see the [[skills/angular/architecture/plateau/design-system/plateau-design-system.skill.md|design-system]] plateau; `libs/shared/ui` only composes it.

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[platform-shell/project-platform-shell.skill.md\|project-platform-shell.skill]] | Composition root: top-level routing, selective preloading, root providers, service-worker registration, global error handling. No business logic. |
| /apps/platform-shell-e2e | [[platform-shell-e2e/project-platform-shell-e2e.skill.md\|project-platform-shell-e2e.skill]] | Playwright end-to-end project exercising real user scenarios against the built application. |
| /libs/shared/ui | [[shared-ui/project-shared-ui.skill.md\|project-shared-ui.skill]] | Reusable, app-specific UI composed from design-system primitives, including the offline banner and pending-sync indicator. |
| /libs/shared/util | [[shared-util/project-shared-util.skill.md\|project-shared-util.skill]] | Framework-agnostic pure helpers shared across features. |
| /libs/shared/state | [[shared-state/project-shared-state.skill.md\|project-shared-state.skill]] | Classical NgRx Store for global state: auth session (incl. token/permissions), connectivity, notifications. |
| /libs/shared/http-core | [[shared-http-core/project-shared-http-core.skill.md\|project-shared-http-core.skill]] | Base HTTP service every feature's Client builds on; hosts the shared `OfflineTransportError` type. |
| /libs/shared/auth-ui | [[shared-auth-ui/project-shared-auth-ui.skill.md\|project-shared-auth-ui.skill]] | Permission-checking directive and route-guard factory shared by every feature. |
| /libs/shared/logging | [[shared-logging/project-shared-logging.skill.md\|project-shared-logging.skill]] | `LoggerService`, console + backend sinks, persisted retry queue. |
| /libs/shared/offline-sync | [[shared-offline-sync/project-shared-offline-sync.skill.md\|project-shared-offline-sync.skill]] | Durable, per-feature-partitioned mutation queue and replay orchestrator. |
| /libs/{feature}/feature | [[feature-feature/project-feature-feature.skill.md\|project-feature-feature.skill]] | Generic template: routed components, feature-level Signal Store, feature-owned routes and guards. |
| /libs/{feature}/data-access | [[feature-data-access/project-feature-data-access.skill.md\|project-feature-data-access.skill]] | Generic template: Facade/Client/Mapper/Errors for one feature's HTTP access, offline-aware. |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]

## Nx Tag Taxonomy

| Axis | Values | Meaning |
| ----- | ------- | ------- |
| `type` | `app`, `e2e`, `feature`, `data-access`, `ui`, `util`, `store` | What role the project plays |
| `scope` | `platform`, `shared`, `{feature-name}` (e.g. `orders`) | Which business area the project belongs to |

`@nx/enforce-module-boundaries` allow-list (cumulative):

| type | may depend on |
| ----- | -------------- |
| `app` | any `type:feature` with matching or `scope:platform` |
| `e2e` (scope:platform) | nothing (drives the built app through the browser only) |
| `feature` | `type:data-access` with the same `scope`, `type:ui`/`type:util`/`type:store` with `scope:shared` |
| `data-access` | `type:util` with `scope:shared` |
| `ui` (scope:shared) | `type:util` with `scope:shared` |
| `util` (scope:shared) | nothing (leaf) — except `libs/shared/offline-sync`, which is tagged `type:util` but may additionally depend on `libs/shared/state` (to read the `connectivity` slice) |
| `store` (scope:shared) | `type:util` with `scope:shared` |

Everything not explicitly listed here is denied by the lint rule. `type:store` (scope:shared) must not depend on any `type:feature` or `type:data-access` project.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]

## Cross-cutting conventions

These rules apply inside every project in the workspace and have no single project of their own to live in:

- **Three-tier state placement**: component-local state (dialog visibility, selected tab, form draft, loading flags) is a plain `signal()` on the component; feature-level state is an NgRx Signal Store colocated in `libs/{feature}/feature` (see [[feature-feature/classes/class-feature-store.skill.md|class-feature-store.skill]]); global/cross-cutting state is a classical NgRx slice inside `libs/shared/state` (see [[shared-state/project-shared-state.skill.md|project-shared-state.skill]]). State is promoted upward only when a second, unrelated consumer genuinely needs it.
- **Hierarchical route ownership**: every routable `type:feature` project exports its own root-relative `Routes` from `index.ts`; the mounting project (the shell, or an embeddable module in a future plateau) assigns the segment name at the mounting point. A route path never bakes in an assumed mount prefix.
- **Selective preloading**: `data: { preload: true }` is set only at the mounting point, never inside a feature's own routes.
- **Facade/Client/Mapper layering**: a feature's Signal Store calls only its Facade; the Facade calls only the Client; the Client is the only place that talks to `libs/shared/http-core` and the only place a raw `HttpErrorResponse` is caught and turned into a typed domain error (including `OfflineTransportError` for a genuine network-level failure).
- **Single logging seam**: every part of the application logs through `LoggerService`; no direct `console.*` call is permitted outside `libs/shared/logging`.
- **Offline-aware mutations**: a Facade explicitly opts a mutation into the offline queue by catching `OfflineTransportError` and calling `MutationQueueService.enqueue(...)`; queueing is never automatic for every method.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/LocalState/{component-name}.component.ts.extend|LocalState/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]

## Deferred to the platform plateau

Two implementation slices are Module-Federation-specific and require `RemoteRegistryService`/`@platform/contracts`, which do not exist until `solution-platform-embeddability` is applied. They are deliberately excluded from this plateau and re-included in full by the [[skills/angular/architecture/plateau/platform/plateau-platform.skill.md|platform]] plateau:

- `solution-app-routing`'s embeddable-module route mounting (`Implementation/EmbeddableModule/routes.ts.extend.md`) — there is no embeddable module to mount yet.
- `solution-authentication`'s `@platform/contracts` session-sharing extension (`Implementation/EmbeddableApp/platform-contracts.extend.md`) — there is no embeddable app to share the session with yet.
- Within `solution-offline-first`'s service worker, the fifth caching rule (stale-while-revalidate for federated remote chunks) is deferred for the same reason — see [[platform-shell/classes/class-service-worker.skill.md|class-service-worker.skill]] for the documented gap. The other four caching strategies are applied in full at this plateau.
- `solution-design-system-application` and `solution-platform-embeddability` themselves are 100% federation-scoped and are not applied at all until the `platform` plateau.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/EmbeddableModule/routes.ts.extend|EmbeddableModule/routes.ts.extend (deferred)]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend (deferred)]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create (rule 5 deferred)]]

# Rules

## MUST
- Every Nx project MUST declare exactly one `type:*` tag and exactly one `scope:*` tag.
- Every lib MUST expose its public API through a single `index.ts` barrel; a feature's Client and Mapper are never exported.
- A `type:feature` project MUST NOT import another `type:feature` project directly, regardless of scope.
- Business logic (HTTP calls, state, domain rules) MUST NOT live in `apps/platform-shell` — the shell only composes, routes, and registers cross-cutting infrastructure (service worker, global error handler, preloading strategy).
- Every routable `type:feature` project MUST export its `Routes` relative to its own root only; the mounting project assigns the segment.
- The workspace MUST run Angular >= 22 (Signal Forms baseline) and every new form MUST be built with Signal Forms.
- A feature's Client MUST build its HTTP calls on `libs/shared/http-core`, never call `HttpClient` directly, and MUST translate every transport failure into a typed domain error, including the shared `OfflineTransportError` for a network-level failure (`status === 0`).
- The access token MUST only ever live in `shared-state`'s in-memory auth slice — never in `localStorage`/`sessionStorage`.
- Every part of the application MUST log through `LoggerService` — no direct `console.*` call outside `libs/shared/logging`.
- The service worker MUST be generated via Workbox's programmatic build API, integrated into the Nx build pipeline. Auth endpoints and every non-GET request MUST be `network-only`, never cached.
- Every queued mutation MUST carry a client-generated idempotency key, reused unchanged across every replay attempt, and MUST be partitioned by the feature that created it.
- Every Nx project MUST run its unit/component tests via Vitest; end-to-end tests MUST be Playwright specs in `apps/platform-shell-e2e`.

## SHOULD
- New business features SHOULD be scaffolded as a `{feature}/feature` + `{feature}/data-access` pair from the start.
- Field schema/validators SHOULD stay inline for simple forms, and SHOULD be extracted into a `{form-name}.form.ts` file once cross-field validation makes the component harder to read.
- Genuinely global reads (e.g. current user, connectivity) SHOULD come from `libs/shared/state` selectors rather than being duplicated locally.

## SHOULD NOT
- Existing Reactive Forms code SHOULD NOT be migrated to Signal Forms purely for consistency — only when already being substantially reworked.
- A feature or embeddable module SHOULD NOT set `preload: true` on its own routes.

## MUST NOT
- MUST NOT place a routed business feature directly under `/apps`.
- MUST NOT add a `type:util` project with any `scope:*` other than `shared`.
- A component or Signal Store method MUST NOT import a feature's Client directly, bypassing the Facade.
- This plateau MUST NOT introduce any caching or offline behavior for auth/mutation endpoints — they remain `network-only` unconditionally.
- A Facade MUST NOT enqueue an operation whose business validation already failed before the Client was ever called.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

- **Single flat lib per feature instead of `feature` + `data-access`**
  - Consequence: UI and HTTP/data concerns become entangled, harder to test in isolation
  - Instead: always split from the start
- **A Signal Store method calling the feature's Client directly, skipping the Facade**
  - Consequence: bypasses business-rule validation and, once offline-sync exists, the queueing decision the Facade owns
  - Instead: the store always goes through the Facade
- **Configuring auth or mutation endpoints with any service-worker caching strategy other than network-only**
  - Consequence: a cached auth/mutation response is a correctness and security bug, not just staleness
  - Instead: auth and all non-GET requests are always `network-only`
- **Enqueueing every `OfflineTransportError` unconditionally**
  - Consequence: some operations (one-time, time-sensitive) queued and replayed later can produce a confusing or wrong result
  - Instead: each Facade explicitly decides which of its operations are queueable
- **Persisting the access token to storage "to survive reloads more simply"**
  - Consequence: reintroduces the XSS exposure the in-memory token strategy exists to avoid
  - Instead: rely on the silent-refresh-on-bootstrap flow

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]

# Unittest TestCases

- [ ] WHEN `nx run-many -t lint` is executed THEN
  - [ ] `@nx/enforce-module-boundaries` reports no violations
- [ ] WHEN a feature's Client makes a request while the network is genuinely unreachable THEN
  - [ ] it throws the shared `OfflineTransportError`, not that feature's generic domain error
- [ ] WHEN the service worker's routing configuration is inspected THEN
  - [ ] every auth/mutation endpoint resolves to `network-only`
- [ ] WHEN two features both have pending mutations and one feature's replay fails repeatedly THEN
  - [ ] the other feature's partition still completes its replay successfully
- [ ] WHEN the codebase is searched for direct `console.*` calls outside `libs/shared/logging` THEN
  - [ ] none are found
- [ ] WHEN `nx run-many -t test` is executed THEN
  - [ ] every project runs via Vitest, and `apps/platform-shell-e2e` runs via Playwright separately

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]
