---
name: repo-platform
description: Nx workspace layout for the platform plateau — the final, head plateau where all 17 solutions are fully applied. apps/platform-shell becomes a Native Federation dynamic host consuming independently deployed embeddable apps and a version-negotiated design-system singleton, on top of the full offline-capable, authenticated, observable application.
domain: skill
type: template
plateau: platform
version: 20260711150000
tags:
  - skill/template/repo
  - plateau/platform
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
  - "[[skills/angular/architecture/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill|solution-design-system-structure]]"
  - "[[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill|solution-design-system-tokens]]"
  - "[[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill|solution-design-system-components]]"
  - "[[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]]"
  - "[[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]]"
---

# Structure

## Workspace Structure

```
/apps
  /[platform-shell](./platform-shell/project-platform-shell.skill.md)      <- now a Native Federation dynamic host (type:host)
  /[platform-shell-e2e](./platform-shell-e2e/project-platform-shell-e2e.skill.md)

/libs
  /shared
    /[ui](./shared-ui/project-shared-ui.skill.md)
    /[util](./shared-util/project-shared-util.skill.md)
    /[state](./shared-state/project-shared-state.skill.md)
    /[http-core](./shared-http-core/project-shared-http-core.skill.md)
    /[auth-ui](./shared-auth-ui/project-shared-auth-ui.skill.md)
    /[logging](./shared-logging/project-shared-logging.skill.md)
    /[offline-sync](./shared-offline-sync/project-shared-offline-sync.skill.md)
  /{feature}
    /[feature](./feature-feature/project-feature-feature.skill.md)
    /[data-access](./feature-data-access/project-feature-data-access.skill.md)
```

No new top-level project is added to this monorepo by the five federation/design-system solutions applied at this plateau — they extend `apps/platform-shell` (federation host, design-system singleton) and `libs/shared/state` (session contract exposure). Two things this plateau depends on live outside this repository, by design, and are **not** modeled as projects here:

- The **design system** itself (`solution-design-system-structure`/`-tokens`/`-components`) is an independently versioned npm package built in its own repository — see the [[skills/angular/architecture/plateau/design-system/plateau-design-system.skill.md|design-system]] plateau. `solution-design-system-application` (applied here) only adds federation-aware, version-negotiated *consumption* of that package by `apps/platform-shell`.
- Each **embeddable app** (`solution-platform-embeddability`) is an independently deployed repository, owned and released by its own team — see the sibling [[skills/angular/architecture/plateau/embeddable-app/plateau-embeddable-app.skill.md|embeddable-app]] plateau for its own baseline structure. This plateau only models the platform-**host** side: `apps/platform-shell` becoming a Native Federation dynamic host and the `remote-registry.service.ts` that discovers embeddable apps at runtime.
- `@platform/contracts` (the shared EventBus/session contract package referenced by both sides) is likewise published from its own separate repository; this plateau documents the platform-host side of the contract (the `SessionContract` shape derived from `libs/shared/state`'s `auth` slice) inside [[shared-state/project-shared-state.skill.md|project-shared-state.skill]], not as a project of its own.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/Repository.extend|PlatformHost/Repository.extend]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/EmbeddableApp/Repository.create|EmbeddableApp/Repository.create (sibling embeddable-app plateau)]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/PlatformHost/platform-shell.federation.extend|PlatformHost/platform-shell.federation.extend]]

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[platform-shell/project-platform-shell.skill.md\|project-platform-shell.skill]] | Composition root and, as of this plateau, a Native Federation dynamic host: routing, preloading, service worker (incl. federated-remote caching), global error handling, remote registry, design-system singleton. No business logic. |
| /apps/platform-shell-e2e | [[platform-shell-e2e/project-platform-shell-e2e.skill.md\|project-platform-shell-e2e.skill]] | Playwright end-to-end project. |
| /libs/shared/ui | [[shared-ui/project-shared-ui.skill.md\|project-shared-ui.skill]] | Reusable app-specific UI: offline banner, pending-sync indicator. |
| /libs/shared/util | [[shared-util/project-shared-util.skill.md\|project-shared-util.skill]] | Framework-agnostic pure helpers. |
| /libs/shared/state | [[shared-state/project-shared-state.skill.md\|project-shared-state.skill]] | Classical NgRx Store: auth (incl. `SessionContract` source), connectivity, notifications. |
| /libs/shared/http-core | [[shared-http-core/project-shared-http-core.skill.md\|project-shared-http-core.skill]] | Base HTTP service; shared `OfflineTransportError`. |
| /libs/shared/auth-ui | [[shared-auth-ui/project-shared-auth-ui.skill.md\|project-shared-auth-ui.skill]] | Permission directive and route-guard factory. |
| /libs/shared/logging | [[shared-logging/project-shared-logging.skill.md\|project-shared-logging.skill]] | `LoggerService`, console + backend sinks, retry queue. |
| /libs/shared/offline-sync | [[shared-offline-sync/project-shared-offline-sync.skill.md\|project-shared-offline-sync.skill]] | Durable, per-feature-partitioned mutation queue and replay orchestrator. |
| /libs/{feature}/feature | [[feature-feature/project-feature-feature.skill.md\|project-feature-feature.skill]] | Generic template: routed components, feature-level Signal Store, feature-owned routes/guards. |
| /libs/{feature}/data-access | [[feature-data-access/project-feature-data-access.skill.md\|project-feature-data-access.skill]] | Generic template: Facade/Client/Mapper/Errors, offline-aware. |

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
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/Repository.extend|PlatformHost/Repository.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/PlatformHost/platform-shell.federation.extend|PlatformHost/platform-shell.federation.extend]]

## Nx Tag Taxonomy

| Axis | Values | Meaning |
| ----- | ------- | ------- |
| `type` | `app`, `host`, `e2e`, `feature`, `data-access`, `ui`, `util`, `store` | What role the project plays |
| `scope` | `platform`, `shared`, `{feature-name}` (e.g. `orders`) | Which business area the project belongs to |

`@nx/enforce-module-boundaries` allow-list (cumulative — new row vs. the `tested`/`offline-app` plateaus is `host`):

| type | may depend on |
| ----- | -------------- |
| `app`, `host` (`apps/platform-shell` carries both) | any `type:feature` with matching or `scope:platform` |
| `e2e` (scope:platform) | nothing |
| `feature` | `type:data-access` with the same `scope`, `type:ui`/`type:util`/`type:store` with `scope:shared` |
| `data-access` | `type:util` with `scope:shared` |
| `ui` (scope:shared) | `type:util` with `scope:shared` |
| `util` (scope:shared) | nothing (leaf), except `libs/shared/offline-sync` may additionally depend on `libs/shared/state` |
| `store` (scope:shared) | `type:util` with `scope:shared` |

`type:host` is additive to `type:app` on `apps/platform-shell`, not a replacement — the project keeps its existing composition-root responsibilities and gains dynamic remote-loading responsibilities. Everything not explicitly listed here is denied by the lint rule.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/Repository.extend|PlatformHost/Repository.extend]]

## Cross-cutting conventions

- **Three-tier state placement**, **hierarchical route ownership**, **selective preloading**, **Facade/Client/Mapper layering**, **single logging seam**, and **offline-aware mutations** — unchanged from the `offline-app` plateau; see that plateau's repo skill for the full statement of each.
- **Federation host boundary**: `apps/platform-shell` never knows about a specific embeddable app at build time — only the shape of `@platform/contracts` and the federation `remoteEntry` contract. The list of available remotes is resolved at runtime (Dynamic Federation), never hardcoded.
- **Embeddable-module route mounting (re-included)**: when the shell mounts an embeddable module via `RemoteRegistryService`, that module owns its own root-relative routes the same way a directly-owned feature does — it mounts its own features' root segments without knowing what the shell will call it, and without hardcoding its own expected mount prefix. This is the same hierarchical mounting pattern as `{feature}.routes.ts`, one level down; the module's own internal routing lives in its own embeddable-app repository — see the sibling [[skills/angular/architecture/plateau/embeddable-app/plateau-embeddable-app.skill.md|embeddable-app]] plateau for that repository's own routes file.
- **Session sharing via `@platform/contracts` (re-included)**: an embeddable app reads the platform's session (current user, permissions, `isAuthenticated`) exclusively through `@platform/contracts`' read-only `SessionContract` — never by implementing its own login flow or maintaining its own copy of session state. `SessionContract` is a read-only signal-shaped view of `libs/shared/state`'s `auth` slice — see [[shared-state/project-shared-state.skill.md|project-shared-state.skill]].
- **Design-system singleton sharing**: the design system is declared a version-negotiated (`singleton: true`, `strictVersion: false`) federation shared dependency; the theme is applied exactly once, by `apps/platform-shell`, in production.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/Repository.extend|PlatformHost/Repository.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/EmbeddableModule/routes.ts.extend|EmbeddableModule/routes.ts.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/PlatformHost/platform-shell.federation.extend|PlatformHost/platform-shell.federation.extend]]

## No further deferrals

This is the final, head plateau in the main application's chain: all 17 solutions under `skills/angular/architecture/solutions/` are applied here, in full. Nothing that reached this plateau remains deferred:

- `solution-app-routing`'s embeddable-module mounting and `solution-authentication`'s `@platform/contracts` session sharing — deferred by `offline-app` (and every earlier plateau) — are re-included above.
- `solution-offline-first`'s fifth caching rule (federated remote chunks) is re-included in full — see [[platform-shell/classes/class-service-worker.skill.md|class-service-worker.skill]] — now that `RemoteRegistryService` exists to source `KNOWN_REMOTE_ORIGINS` from.
- `solution-design-system-application` and `solution-platform-embeddability` — 100% federation-scoped, excluded from every earlier plateau — are applied in full.
- `solution-design-system-structure`/`-tokens`/`-components` were never deferred: they were fully applied from the start, in their own separate repository — see the [[skills/angular/architecture/plateau/design-system/plateau-design-system.skill.md|design-system]] plateau. This plateau adds only the federation-aware *consumption* side.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/EmbeddableModule/routes.ts.extend|EmbeddableModule/routes.ts.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/PlatformHost/platform-shell.federation.extend|PlatformHost/platform-shell.federation.extend]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/Repository.extend|PlatformHost/Repository.extend]]

# Rules

## MUST
- Every Nx project MUST declare exactly one `type:*` tag (plus `host` additively for `apps/platform-shell`) and exactly one `scope:*` tag.
- Every lib MUST expose its public API through a single `index.ts` barrel; a feature's Client and Mapper are never exported.
- A `type:feature` project MUST NOT import another `type:feature` project directly, regardless of scope.
- Business logic MUST NOT live in `apps/platform-shell`.
- Every routable `type:feature` project MUST export its `Routes` relative to its own root only.
- The workspace MUST run Angular >= 22 and every new form MUST be built with Signal Forms.
- A feature's Client MUST build its HTTP calls on `libs/shared/http-core` and translate every transport failure into a typed domain error, including `OfflineTransportError`.
- The access token MUST only ever live in `shared-state`'s in-memory auth slice.
- Every part of the application MUST log through `LoggerService`.
- The service worker MUST be generated via Workbox's programmatic build API; auth endpoints and every non-GET request MUST be `network-only`.
- Every queued mutation MUST carry a client-generated idempotency key and MUST be partitioned by feature.
- `apps/platform-shell` MUST declare the `type:host` tag and mark `@platform/contracts` and Angular as `singleton: true` shared dependencies; the list of available remotes MUST be resolved at runtime, never hardcoded into the host's build output.
- `apps/platform-shell`'s federation config MUST declare the design system as a shared dependency with `singleton: true` and `strictVersion: false`; its root styles MUST import the design system's theme.
- An embeddable app MUST read session/permission state exclusively through `SessionContract` — never implement its own login flow.
- Every Nx project MUST run its unit/component tests via Vitest; end-to-end tests MUST be Playwright specs in `apps/platform-shell-e2e`.

## SHOULD
- New business features SHOULD be scaffolded as a `{feature}/feature` + `{feature}/data-access` pair from the start.
- Field schema/validators SHOULD stay inline for simple forms, extracted once cross-field validation grows.
- Genuinely global reads SHOULD come from `libs/shared/state` selectors rather than being duplicated locally.
- The platform's declared design-system version range SHOULD be kept up to date as the platform itself upgrades.

## SHOULD NOT
- Existing Reactive Forms code SHOULD NOT be migrated to Signal Forms purely for consistency.
- A feature or embeddable module SHOULD NOT set `preload: true` on its own routes.

## MUST NOT
- MUST NOT place a routed business feature directly under `/apps`.
- MUST NOT add a `type:util` project with any `scope:*` other than `shared`.
- A component or Signal Store method MUST NOT import a feature's Client directly, bypassing the Facade.
- Auth/mutation endpoints MUST NOT be cached under any strategy other than network-only.
- A Facade MUST NOT enqueue an operation whose business validation already failed before the Client was ever called.
- `apps/platform-shell` MUST NOT bundle a specific embeddable app's code at build time, and MUST NOT depend on an embeddable app's internal implementation.
- The design system's federation shared-dependency declaration MUST NOT set `strictVersion: true`.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/Repository.extend|PlatformHost/Repository.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/PlatformHost/platform-shell.federation.extend|PlatformHost/platform-shell.federation.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]

# Anti-patterns

- **Single flat lib per feature instead of `feature` + `data-access`**
  - Consequence: UI and HTTP/data concerns become entangled
  - Instead: always split from the start
- **Hardcoding a remote's URL or version into the host's source**
  - Consequence: the platform must be rebuilt and redeployed every time an embeddable app ships a new version, defeating independent deployability
  - Instead: resolve remotes from a runtime configuration/manifest
- **Setting `strictVersion: true` for the design system shared dependency**
  - Consequence: reintroduces lockstep-upgrade coupling; a mismatched embeddable app simply fails to load
  - Instead: always `strictVersion: false`, letting a mismatched consumer fall back to its own isolated copy
- **An embeddable app implementing its own login screen "just in case" the platform session is missing**
  - Consequence: duplicates authentication logic and creates two different ways a user could end up authenticated
  - Instead: the embeddable app only ever reads `SessionContract`
- **Configuring auth or mutation endpoints with any service-worker caching strategy other than network-only**
  - Consequence: a cached auth/mutation response is a correctness and security bug
  - Instead: auth and all non-GET requests are always network-only

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/Repository.extend|PlatformHost/Repository.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/PlatformHost/platform-shell.federation.extend|PlatformHost/platform-shell.federation.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/Repository.extend|Repository.extend]]

# Unittest TestCases

- [ ] WHEN `nx run-many -t lint` is executed THEN
  - [ ] `@nx/enforce-module-boundaries` reports no violations
- [ ] WHEN a new embeddable app is added to the runtime remote registry without a platform rebuild THEN
  - [ ] the platform can load and mount it without redeploying
- [ ] WHEN two remotes both depend on `@platform/contracts` at compatible versions THEN
  - [ ] only one instance of the contracts package (and one Angular instance) is loaded in the browser
- [ ] WHEN an embeddable app's declared design-system version range matches the platform's currently loaded version THEN
  - [ ] it shares the single loaded instance, with no duplicate design-system bundle fetched
- [ ] WHEN the platform's session expires THEN
  - [ ] `SessionContract.isAuthenticated` becomes `false` for every embeddable app reading it, without any action needed on the embeddable app's part
- [ ] WHEN the service worker's routing configuration is inspected THEN
  - [ ] every auth/mutation endpoint resolves to `network-only`, and requests to a known federated remote origin resolve to stale-while-revalidate

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/Repository.extend|PlatformHost/Repository.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/PlatformHost/platform-shell.federation.extend|PlatformHost/platform-shell.federation.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]
