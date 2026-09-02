# Feature Model — monolith

A single-deployable Angular web application in this architecture: an Nx workspace whose one deployable unit (`apps/platform-shell`) mounts business features. Derived from the V1 main-chain plateaus (`online-monolith` → `async-monolith` → `offline-monolith` → `monitored-app` → `multiuser-app`) plus owner-reviewed aspirational candidates.

The **root product is `App`** — you build one per repository. It is never a variability question and never a row in the Features table. Most features are answered once per app; `OfflineWriteQueue` is answered per feature, and `PerformanceTunedRouting`'s `loadComponent` splitting is decided per sub-route — noted inline.

Built per [[skills/dotnet/architecture/v3.1/design/feature-map-create.skill/feature-map-create.skill.md|feature-map-create]]. The `platform-host/` catalog composes this one; `embeddable-app/` and `design-system/` are independent — see [[skills/angular/architecture/v3.1/README.md|the catalog overview]].

## The common baseline this model assumes (concretely)

Derived from first principles — *what must exist for the simplest legitimate app in this architecture to function* — **not** from "what every V1 plateau happens to include". The simplest legitimate app: an Nx workspace with a shell, feature libs holding component-local signal state, hierarchical routing, and a test setup. It talks to no backend and has no global store.

```
nx.json  package.json  eslint.config.js       (Nx workspace, Angular preset, @nx/enforce-module-boundaries)
apps/
  platform-shell/
    src/app/app.routes.ts                      (mounts first-level feature root segments only)
  platform-shell-e2e/                          (Playwright e2e project — type:e2e)
libs/
  shared/
    ui/                                        (shared presentational components — type:ui)
    util/                                      (shared pure helpers — type:util)
    logging/                                   (LoggerService + ConsoleLogSink + LOG_SINKS token — type:util)
  {feature}/
    feature/                                   ({feature} components + {feature}.routes.ts + component-local signal state)
```

**Not in the baseline** (each introduced by a variable feature):
- `libs/shared/http-core` and every `libs/{feature}/data-access` lib — `BackendDataAccess`.
- `libs/shared/state` (the classical NgRx global store) — `GlobalStore`. `Authentication` / `OfflineReadResilience` / `OfflineWriteQueue` all `require GlobalStore` (they need a place for the `auth` / `connectivity` / `notifications` slices), so it is present whenever any of them is; but it is a real independent choice on its own (a complex app with cross-feature filters/selections can have `GlobalStore=Yes` with none of them). The V1 `auth.store.ts` worked example in `solution-state-management` moves into `Authentication`.
- Service worker, `libs/shared/offline-sync`, `libs/shared/auth-ui`, `BackendLogSink`, `GlobalErrorHandler`, `apps/component-preview`.

`apps/platform-shell` never contains an HTTP call, business state, or a feature-specific component at any configuration.

## Feature diagram

@import "./diagrams/feature-diagram.mmd" {as="mermaid"}

`OfflineReadResilience`, `BackendLogDelivery`, and `Authentication` are drawn as children of `BackendDataAccess` — each needs the HTTP layer (a Client to throw `OfflineTransportError` and API GETs to cache; `http-core` to send log batches; the interceptor / silent-refresh / login round trips), so that dependency is the parent-child edge. Cross-tree `Requires`: `OfflineReadResilience` → `GlobalStore` (the `connectivity` slice) and `Authentication` → `GlobalStore` (the `auth` slice) — both need the classical NgRx store; `BackendLogDelivery` → `ConsoleLogging` (it registers a second `LogSink` and adds `LoggerService.report()`). `OfflineWriteQueue` (child of `OfflineReadResilience`) transitively requires `GlobalStore` and also directly uses a `notifications` slice.

## Features

| Name | Description | IsCommon |
| --- | --- | --- |
| NxWorkspaceStructure | Nx workspace as the single source of truth for inter-project dependencies: `apps/` (deployable) vs `libs/` (reusable), every project tagged `type:*` + `scope:*`, boundaries enforced by `@nx/enforce-module-boundaries`, CI on `nx affected`. A feature is a `feature` lib (components + routes + local state); a `data-access` lib is added **only** when the feature needs server data (see [Open questions](#open-questions-on-v1) #1). Every lib exposes only its `index.ts` barrel. | true |
| HierarchicalRouting | Routes owned hierarchically: `apps/platform-shell` knows only first-level root segments; a feature knows only paths relative to its own root and never bakes in its mount segment. A feature's `Routes` are part of its `index.ts` public API, mounted via `loadChildren` (lazy by default). | true |
| StateTieringPolicy | The *rule* for where state lives, plus the two lower tiers: component `signal()` for component-local state, and a feature NgRx Signal Store (colocated in `libs/{feature}/feature`) for feature-scoped state. State is promoted to a higher tier only when a second unrelated consumer needs it; a feature never caches higher-tier state locally. The *rule* and these two tiers are common; the third tier (a classical NgRx global store) is `GlobalStore`, a VP. | true |
| SignalForms | Convention: new forms built with Signal Forms (`form()`/`FieldTree`), submission always through `submitForm()`, whose callback calls the owning feature's data-access Facade (never `HttpClient`). Non-trivial field schemas extracted to `{form-name}.form.ts`. Existing Reactive Forms not force-migrated. Costs nothing for an app with no forms. | true |
| ConsoleLogging | Convention: all logging through `LoggerService` (no direct `console.*`), every entry structured (message + context object), forwarded to a pluggable `LOG_SINKS` list — this feature registers only `ConsoleLogSink`. Production build filters `debug`/`info`. Sensitive data never logged, at any level. The `LOG_SINKS` seam is what `BackendLogDelivery` later attaches to. | true |
| BusinessLayerTesting | Vitest as the unit runner, Playwright for e2e (`apps/platform-shell-e2e`). Non-DOM units (Client, Facade, Signal Store) tested via `TestBed`, faking only the layer directly beneath. `HttpTestingController` only inside a Client spec; MSW only for deliberate cross-layer integration specs; e2e for a few critical journeys. Coverage threshold enforced in CI as an error. | true |
| ComponentTesting | Every UI component tested on its own `input()`/`output()`/`model()` surface at three non-substitutable layers: behavioral (Testing Library), visual regression (Playwright screenshots, light + dark, against `apps/component-preview`), accessibility (`@axe-core/playwright`), plus a paired computed-style snapshot. No faked Facade/Client, no Storybook, no Chromatic. | **flagged** — see [Open questions](#open-questions-on-v1) #2 |
| PerformanceTunedRouting | On top of the baseline `loadChildren` lazy routing: a custom `SelectivePreloadingStrategy` (preload opt-in via `data: { preload: true }` set only at the mount point), `loadComponent` sub-splitting for heavy/rare sub-routes (a per-route decision), and enforced (`error`-level) bundle budgets on the initial bundle and every lazy chunk. | false |
| GlobalStore | `libs/shared/state` with a classical NgRx Store (actions / reducers / effects / selectors) for cross-cutting state read or dispatched by more than one unrelated feature — the third tier of `StateTieringPolicy`. The app's own cross-feature state (shared filters, selections, notifications) lives here; `Authentication`, `OfflineReadResilience`, and `OfflineWriteQueue` each add their own slice to it (`auth`, `connectivity`, `notifications`). | false |
| BackendDataAccess | The feature ↔ backend layer: `libs/shared/http-core` (base URL, timeout, retry) plus, in each feature that needs it, a `data-access` lib layered Facade (public API, business validation) → Client (internal transport + DTO mapping, never exported) → `http-core`. A raw `HttpErrorResponse` never escapes a Client — every caller gets a typed domain error. DTO↔model mapping is a hand-written function. For feature-level operations the Signal Store calls the Facade directly (no Action/Reducer/Effect). | false |
| OfflineReadResilience | A Workbox service worker with content-type-specific caching (precache shell, cache-first assets, stale-while-revalidate API GETs, network-only auth + every mutation) and an accurate `isOnline` signal (`navigator.onLine` + periodic health-check) in a `connectivity` global slice. Every Client distinguishes `OfflineTransportError` from a server error. Reads stay available offline; mutations attempted offline still fail immediately. Requires `BackendDataAccess` + `GlobalStore`. | false |
| OfflineWriteQueue | A Dexie-backed, per-feature-partitioned mutation queue: a Facade explicitly opts a specific operation into being queued on `OfflineTransportError` (a per-feature, per-operation decision) instead of failing. Idempotent replay on connectivity restore, FIFO within a partition, concurrent across partitions, server-wins conflict resolution with field-scoped diffing, a per-feature pending-sync indicator. Requires `OfflineReadResilience`; adds a `notifications` global slice (see [Open questions](#open-questions-on-v1) #5). | false |
| BackendLogDelivery | A second `LogSink` (`BackendLogSink`) batching `warn`/`error`/`report()` entries to the backend (timer/size flush + `sendBeacon` on unload), backed by a bounded IndexedDB retry queue (count/age/size limits, oldest-first eviction). Plus a single application-root `GlobalErrorHandler` routing every uncaught exception through `LoggerService.error`. No call-site changes. Requires `BackendDataAccess` + `ConsoleLogging`. | false |
| Authentication | In-memory-only access token (refresh token in an `HttpOnly` cookie the client never reads), silent-refresh-on-bootstrap and on 401, permission-string authorization (never role names) checked identically in a route guard (attached at the feature's own route via a `requirePermission` factory) and a `*hasPermission` structural directive. Adds the `auth` slice to `libs/shared/state`. Requires `BackendDataAccess` + `GlobalStore`. **Publishing `SessionContract` to embeddable apps is not part of this — that belongs to `platform-host/`.** | false |

### Deliberately not rows

- **`apps/platform-shell`, the feature-lib pattern, the `index.ts` barrel** are the baseline structure the family *is* — described [above](#the-common-baseline-this-model-assumes-concretely), not features.
- **The `auth` / `connectivity` / `notifications` slices** are not features — each is a part of `Authentication` / `OfflineReadResilience` / `OfflineWriteQueue` respectively, added to `GlobalStore`.
- **The `LOG_SINKS` extension point** is the seam `ConsoleLogging` builds for `BackendLogDelivery`, not a feature.

## Aspirational candidates (owner asked to consider — confirm each before it becomes a row)

None map to a V1 solution. Proposed as flagged, no `Realized by`:

| Candidate | Rationale | Shape |
| --- | --- | --- |
| ServerSideRendering | Angular SSR + hydration for first paint / SEO; interacts with `OfflineReadResilience` (SW vs SSR cache) | optional, app-level |
| Internationalization | Runtime locale + message catalog; interacts with `SignalForms` (validation messages) and `design-system` (RTL) | optional, app-level |
| Telemetry | Product analytics / RUM, distinct from `BackendLogDelivery`'s diagnostic logging; likely a Plateau Component — needs the [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md\|plateau-component-create]] test | optional, composition-root |
| FeatureFlags | Runtime flag evaluation gating routes/UI; overlaps `Authentication`'s permission checks in shape | optional, app-level |
| RuntimeConfig | API base URLs / manifest URLs / health-check URL loaded at bootstrap rather than build-time `environment.ts` — already implied by `OfflineReadResilience`'s health-check URL | optional, app-level |
| PersistedState | Selected NgRx state (a feature Signal Store, or a `GlobalStore` slice) synced to `localStorage` / IndexedDB so it survives a browser session — user preferences, draft forms, last-viewed items, filter state. **Never** for the access token (`solution-authentication` explicitly forbids persisting it). No V1 solution realizes this (`offline-sync`'s Dexie queue and `logging-global`'s IndexedDB queue are feature-specific, not a general state-persistence mechanism); a new solution would. Requires `GlobalStore` (or the feature Signal Store tier). | optional, app-level — **owner confirmed as a row-to-be, pending its solution** |

## Open questions on V1

Recorded, not resolved (owner reviews as a batch). Working hypothesis is what this model currently assumes.

1. **`solution-repository-structure`'s "split every feature into `feature` + `data-access` from the start" is wrong once `BackendDataAccess` is a VP.** A no-backend feature has no `data-access` lib. **Working hypothesis: modify `solution-repository-structure`** — a feature is a `feature` lib; a `data-access` lib is added by `BackendDataAccess` for features with server data.
2. **`ComponentTesting` common or variable?** V1 puts `solution-ui-testing` in the first plateau (→ common). First principles: visual-regression + a11y *automation* is a quality gate, not a functional necessity (→ variable, like `PerformanceTunedRouting`). dotnet's equivalent call made all build/test gates common by owner preference. **Working hypothesis: common** (matches V1).
3. **`solution-ui-testing`'s `depends_on` is family-crossed** — it declares `design-system-structure` + `design-system-components`, which do not exist in `monolith/`. **Working hypothesis: `solution-ui-testing` splits during delta-conflict-detection** into a monolith-side (`ComponentTesting`) and a design-system-side realization. The monolith side's real deps are `repository-structure` + `app-testing` (+ `forms` for form components).
4. **`libs/shared/state` ownership — RESOLVED (owner, this session):** `GlobalStore` is a **VP**, realized by the GlobalStore part of `solution-state-management` (which splits: the tiering rule + feature Signal Store pattern stay common as `StateTieringPolicy`; `libs/shared/state` + classical NgRx become the `GlobalStore` VP). `Authentication` / `OfflineReadResilience` / `OfflineWriteQueue` `require GlobalStore` and add their slices to it — the dotnet v3.1 "VP5/6/7 require VP2 (Persistence)" pattern. The V1 `auth.store.ts` worked example moves into `Authentication`.
5. **The `notifications` slice** that `OfflineWriteQueue` needs for conflict messages is a "worked example" stub in `solution-state-management` and unowned. **Working hypothesis: `OfflineWriteQueue` creates it** (or a tiny standalone solution does).
6. **`solution-lazy-loading-routing` → rename `PerformanceTunedRouting`** — the baseline `HierarchicalRouting` already lazy-loads via `loadChildren`; what varies is selective preloading + `loadComponent` discipline + enforced budgets. **Working hypothesis: renamed.**
7. **`apps/platform-shell` name** — carried from V1, but "platform-shell" is a platform-ism in a plain monolith. **Working hypothesis: keep the V1 name** (rename is ~50 files of churn for little gain); revisit only if it confuses.

## Out of scope

- **Aspirational features have no `Realized by`** — variants/constraints come from the diagram, not working solutions, until authored. `PersistedState` is owner-confirmed as a row-to-be but has no solution yet.
- **The federation world is two separate catalogs** — `platform-host/` composes this model (the host **is** a monolith + federation); `embeddable-app/` is the remote role. `SessionContract` publication, remotes, `@platform/contracts` live there, not here.
- **`design-system` is a separate catalog** — this model consumes it as a plain npm dependency (like Angular itself), not modeled as a feature here.
- **Constraint evidence**: `OfflineReadResilience` / `BackendLogDelivery` / `Authentication` require `BackendDataAccess` is new to v3.1 (V1 predates `BackendDataAccess` being a VP), grounded in each solution's actual HTTP usage. `OfflineReadResilience`/`Authentication` require `GlobalStore` is new to v3.1 for the same reason. `OfflineWriteQueue` → `OfflineReadResilience` and `BackendLogDelivery` → `ConsoleLogging` are backed by real V1 `depends_on` edges.
- **`IsCommon` is a judgment call** — six features tested against the written baseline; `ComponentTesting` left flagged.
