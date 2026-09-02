# Feature Model — platform-app

The platform web application: an Nx-monorepo Angular app deployed as a single shell (`apps/platform-shell`) that mounts business features and, optionally, independently deployed remote apps. Derived from the V1 main-chain plateaus (`online-monolith` → … → `multiuser-app`) plus aspirational candidates the owner asked to consider.

The **root product is `PlatformApp`** — you build one per repository. It is never a variability question and never a row in the Features table. Most features are answered once per app; two (`OfflineWriteQueue`, and `PerformanceTunedRouting`'s sub-route splitting) are answered at a finer granularity, noted inline.

## The common baseline this model assumes (concretely)

An `online-monolith`-shaped app with every optional feature removed:

```
nx.json  package.json  eslint.config.js       (Nx workspace, Angular preset, @nx/enforce-module-boundaries)
apps/
  platform-shell/
    src/app/app.routes.ts                      (mounts first-level feature root segments only)
  platform-shell-e2e/                          (Playwright e2e project, type:e2e)
libs/
  shared/
    ui/                                        (shared presentational components — type:ui)
    util/                                      (shared pure helpers — type:util)
    state/                                     (global NgRx store: slice skeleton, no slices yet — type:store)
    http-core/                                 (base HTTP service: base URL, timeout, retry — type:data-access/shared)
    logging/                                   (LoggerService + ConsoleLogSink + LOG_SINKS token — type:util)
  {feature}/
    feature/                                   (feature components + {feature}.routes.ts + {feature} Signal Store)
    data-access/                               (facade/ + client/ + mapper/ + errors — Client never exported)
```

No service worker, no `libs/shared/offline-sync`, no `libs/shared/auth-ui`, no federation config on `platform-shell`, no `BackendLogSink`, no `apps/component-preview` unless `ComponentTesting`'s visual layer is taken. `apps/platform-shell` never contains an HTTP call, business state, or a feature-specific component at any configuration.

## Feature diagram

@import "./diagrams/feature-diagram.mmd" {as="mermaid"}

Two `Requires` edges, both single-source: `BackendLogDelivery` requires `ConsoleLogging` (it registers a second `LogSink` and adds `LoggerService.report()` — real `depends_on` on `solution-logging-base`), and `Authentication` requires `FederationHost` **as V1 declares it** — flagged below as probably only partial. `OfflineWriteQueue` and `DesignSystemConsumption` are drawn as children of `OfflineReadResilience` / `FederationHost`, so their dependency is the parent-child edge, not a separate `Requires`.

## Features

| Name | Description | IsCommon |
| --- | --- | --- |
| NxWorkspaceStructure | Nx workspace as the single source of truth for inter-project dependencies: `apps/` (deployable) vs `libs/` (reusable), every project tagged `type:*` + `scope:*`, boundaries enforced by `@nx/enforce-module-boundaries`, CI on `nx affected`. Every feature split into a `feature` lib and a `data-access` lib, each exposing only its `index.ts`. | true |
| TieredStateManagement | State lives at the smallest tier that satisfies its real consumers: component `signal()` → feature NgRx Signal Store (colocated in `libs/{feature}/feature`) → global classical NgRx slice in `libs/shared/state`. Promoted upward only when a second unrelated consumer needs it; a feature never caches global state locally. | true |
| HierarchicalRouting | Routes owned hierarchically: `apps/platform-shell` knows only first-level root segments; a feature knows only paths relative to its own root and never bakes in its mount segment. A feature's `Routes` are part of its `index.ts` public API, mounted via `loadChildren` (lazy by default). | true |
| SignalForms | New forms built with Signal Forms (`form()`/`FieldTree`) by default; submission always through `submitForm()`, whose callback calls the owning feature's data-access Facade — never `HttpClient` directly. Non-trivial field schemas extracted to `{form-name}.form.ts`. Existing Reactive Forms not force-migrated. | true |
| FacadeClientDataAccess | Each feature's `data-access` lib is layered Facade (public API, business validation) → Client (internal transport + DTO mapping, never exported) → shared `libs/shared/http-core`. A raw `HttpErrorResponse` never escapes a Client — every caller gets a typed domain error. DTO↔model mapping is a hand-written function. For feature-level operations the Signal Store calls the Facade directly (no Action/Reducer/Effect). | true |
| ConsoleLogging | All logging through `LoggerService` (no direct `console.*`), every entry structured (message + context object), forwarded to a pluggable `LOG_SINKS` list — this feature registers only `ConsoleLogSink`. Production build filters `debug`/`info`. Sensitive data never logged, at any level. | true |
| BusinessLayerTesting | Vitest as the unit runner, Playwright for e2e (`apps/platform-shell-e2e`). Non-DOM units (Client, Facade, Signal Store) tested via `TestBed`, faking only the layer directly beneath. `HttpTestingController` only inside a Client spec; MSW only for deliberate cross-layer integration specs; e2e reserved for a few critical journeys. Coverage threshold enforced in CI as an error. | true |
| ComponentTesting | Every UI component tested on its own `input()`/`output()`/`model()` surface at three layers that do not substitute for each other: behavioral (Testing Library), visual regression (Playwright screenshots, light + dark, against `apps/component-preview`), accessibility (`@axe-core/playwright`), plus a paired computed-style snapshot. No faked Facade/Client, no Storybook, no Chromatic. | **flagged** — see [Open questions](#open-questions-on-v1) |
| PerformanceTunedRouting | On top of the baseline `loadChildren` lazy routing: a custom `SelectivePreloadingStrategy` (preload is opt-in via `data: { preload: true }` set only at the mount point), `loadComponent` sub-splitting for heavy/rare sub-routes (a per-route decision), and enforced (`error`-level) bundle budgets on the initial bundle and every lazy chunk. | false |
| OfflineReadResilience | A Workbox service worker with content-type-specific caching (precache shell, cache-first assets, stale-while-revalidate API GETs, network-only auth + every mutation) and an accurate `isOnline` signal (`navigator.onLine` + periodic health-check) in `libs/shared/state`. Every Client distinguishes `OfflineTransportError` from a server error. Reads stay available offline; mutations attempted offline still fail immediately. | false |
| OfflineWriteQueue | A Dexie-backed, per-feature-partitioned mutation queue: a Facade explicitly opts a specific operation into being queued on `OfflineTransportError` (a per-feature, per-operation decision) instead of failing. Idempotent replay on connectivity restore, FIFO within a partition, concurrent across partitions, server-wins conflict resolution with field-scoped diffing, a per-feature pending-sync indicator. | false |
| FederationHost | `apps/platform-shell` becomes a Native Federation dynamic host: remote apps discovered from a runtime manifest (`RemoteRegistryService`), mounted via `loadRemoteModule` like a directly-owned feature, Angular + `@platform/contracts` shared `singleton: true`. A failed remote load degrades to a fallback slot, never a shell-wide crash. | false |
| DesignSystemConsumption | The `design-system` npm package consumed as a version-negotiated federation singleton (`singleton: true`, `strictVersion: false`): shared when consumers' declared ranges align, isolated copy when they don't. `apps/platform-shell` is the only consumer that applies the theme in production; mounted remotes inherit it via the shared document. | false |
| BackendLogDelivery | A second `LogSink` (`BackendLogSink`) that batches `warn`/`error`/`report()` entries to the backend (timer/size flush + `sendBeacon` on unload), backed by a bounded IndexedDB retry queue (count/age/size limits, oldest-first eviction). Plus a single application-root `GlobalErrorHandler` routing every uncaught exception through `LoggerService.error`. No call-site changes. | false |
| Authentication | In-memory-only access token (refresh token in an `HttpOnly` cookie the client never reads), silent-refresh-on-bootstrap and on 401, permission-string authorization (never role names) checked identically in a route guard (attached at the feature's own route via a `requirePermission` factory) and a `*hasPermission` structural directive, and `SessionContract` published through `@platform/contracts` for embeddable apps to read. | false |

### Deliberately not rows

- **`apps/platform-shell` / the feature/data-access split** are structure the baseline *is*, not a choice — they are described in [the baseline](#the-common-baseline-this-model-assumes-concretely), not as features.
- **The `LogSink` extension point** is not a feature — it is the seam `ConsoleLogging` builds so `BackendLogDelivery` can attach without touching call sites.

## Aspirational candidates (owner asked to consider — confirm each before it becomes a row)

None of these map to a V1 solution. Proposed as flagged, no `Realized by`:

| Candidate | Rationale | Shape |
| --- | --- | --- |
| ServerSideRendering | Angular SSR + hydration for first-paint / SEO; interacts with `OfflineReadResilience` (service worker vs SSR cache) and `FederationHost` (remotes and SSR) | optional, app-level |
| Internationalization | Runtime locale + message catalog; interacts with `SignalForms` (validation messages) and `design-system` (RTL) | optional, app-level |
| Telemetry | Product analytics / RUM distinct from `BackendLogDelivery` (which is diagnostic logging); likely a Plateau Component, not a feature — needs the [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md\|plateau-component-create]] test | optional, composition-root |
| FeatureFlags | Runtime flag evaluation gating routes/UI; overlaps `Authentication`'s permission checks in shape | optional, app-level |
| RuntimeConfig | Environment config loaded at bootstrap (API base URLs, manifest URLs) rather than build-time `environment.ts` — arguably already implied by `FederationHost`'s runtime manifest and `OfflineReadResilience`'s health-check URL | optional, app-level |

## Open questions on V1

Recorded, not yet resolved (owner reviews as a batch). Working hypothesis is what this model currently assumes.

1. **`ComponentTesting` common or variable?** V1 puts `solution-ui-testing` in `online-monolith` (the first plateau), which argues common. First principles: visual-regression + a11y *automation* is a quality gate, not a functional necessity for the simplest legitimate platform app — which argues variable (like `PerformanceTunedRouting`). dotnet's equivalent call was to make all build/test gates common by owner preference. **Working hypothesis: common** (matches V1). Owner call.
2. **`solution-ui-testing`'s `depends_on` is family-crossed.** It declares `depends_on` on `design-system-structure` + `design-system-components` — solutions that do not exist in the platform-app family. The platform-side of ui-testing needs only `repository-structure` + `forms` + `app-testing`. **Working hypothesis: `solution-ui-testing` splits** into a platform-side and a design-system-side realization during delta-conflict-detection (a `TD-`/degenerate split). Until then the platform model treats `ComponentTesting`'s platform-side deps as `repository-structure` + `app-testing` (+ `forms` for form components).
3. **Does `Authentication` really require `FederationHost`?** V1 `solution-authentication` `depends_on solution-platform-embeddability`, and V1 only introduces auth at `multiuser-app` (after `platform-monolith`). But the only part that needs federation is publishing `SessionContract` through `@platform/contracts`. A non-federated platform app should be able to have full auth (in-memory token, silent refresh, permission guards, `*hasPermission`) with **no** `@platform/contracts`. **Working hypothesis: `Authentication` does *not* require `FederationHost`; the `SessionContract` publication is a conditional add-on** ("when `FederationHost` is also present"), mirroring how `solution-external-created-entity` handles the inbound-API dependency in dotnet v3.1. This changes the diagram (drop the flagged `Requires` edge). Owner call.
4. **Does `FederationHost` really require `OfflineReadResilience`?** V1 `solution-platform-embeddability` `depends_on solution-offline-first`, but its own prose says the fifth service-worker caching rule applies only "if the Offline-first solution is also present". **Working hypothesis: no hard requirement**; the SW fifth rule is a conditional cross-feature interaction, not a constraint. The V1 `depends_on` is over-strong.
5. **`PerformanceTunedRouting` — is "lazy loading" the right name?** The baseline `HierarchicalRouting` already lazy-loads every feature via `loadChildren`. What `solution-lazy-loading-routing` adds is *selective preloading* + *`loadComponent` code-split discipline* + *enforced bundle budgets*. **Working hypothesis: renamed `PerformanceTunedRouting`** to reflect what actually varies.
6. **Is `libs/shared/state` truly baseline if it has no slices?** `solution-state-management` `creates` it with an `auth.store.ts` *worked example* that is not the real auth slice. **Working hypothesis: the global-store skeleton + the tiering rule are baseline; every concrete slice (`auth`, `connectivity`, `notifications`, `offline-sync`) is added by the feature that needs it.** The V1 `auth.store.ts` example should move into `Authentication`.
7. **`solution-offline-sync` needs a `notifications` slice** for conflict messages, but no V1 solution creates one (it is a "worked example" stub in `state-management`). **Working hypothesis: `OfflineWriteQueue` creates the `notifications` slice**, or a small standalone solution does. Flagged for delta-conflict / solution authoring.

## Out of scope

- **Aspirational features have no `Realized by`.** Their variants/constraints will come from the feature diagram, not working solutions, until authored.
- **The design-system and embeddable-app families are modeled separately** — see [[skills/angular/architecture/v3.1/feature/feature-model.md|the umbrella model]]. This model references them only where the platform app consumes them (`DesignSystemConsumption`, `FederationHost`).
- **Constraint evidence is uneven**: `OfflineWriteQueue`→`OfflineReadResilience`, `DesignSystemConsumption`→`FederationHost`, `BackendLogDelivery`→`ConsoleLogging` are backed by real V1 `depends_on` edges. The `Authentication`→`FederationHost` and `FederationHost`→`OfflineReadResilience` edges are V1 `depends_on` this model believes are too strong (open questions 3, 4).
- **`IsCommon` is a judgment call.** Eight features are marked common by testing them against the written baseline; `ComponentTesting` is explicitly left flagged rather than forced either way.
