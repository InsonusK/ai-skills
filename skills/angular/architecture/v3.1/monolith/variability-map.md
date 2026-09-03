---
tags:
  - concern/architecture
  - stack/typescript
---

# monolith Variability Map

Built per [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]], from the non-common features of [[skills/angular/architecture/v3.1/monolith/feature/feature-model.md|monolith/feature/feature-model.md]]. This map is the input to [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]]. Sibling catalogs: [[skills/angular/architecture/v3.1/platform-host/variability-map.md|platform-host]], [[skills/angular/architecture/v3.1/embeddable-app/variability-map.md|embeddable-app]], [[skills/angular/architecture/v3.1/design-system/variability-map.md|design-system]].

**Status of this catalog.** `v3.1/solutions/` does not exist yet — **Realized by** cells name the V1 source solution(s) in `skills/angular/architecture/v3.1/solutions/` plus the planned v3.1 solution (migrated / renamed / split / new). Stage 3 creates `v3.1/solutions/` and repoints every link. `v3.1/monolith/plateau/` is built by `plateau-create-by-solutions` after the catalog and the [[skills/angular/architecture/v3.1/delta-conflict-analysis.md|delta-conflict pass]].

## Variation Points

Common baseline features (`NxWorkspaceStructure`, `HierarchicalRouting`, `StateTieringPolicy`, `SignalForms`, `ConsoleLogging`, `BusinessLayerTesting`, `ComponentTesting`) are **not** rows here — every path through the family includes them. See [Features that are not VPs](#features-that-are-not-vps).

| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| VP1 | **PerformanceTunedRouting** — beyond baseline `loadChildren` lazy routing, does the app add a `SelectivePreloadingStrategy`, `loadComponent` sub-splitting discipline, and enforced bundle budgets? | Yes / No | — | Yes → [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md\|solution-performance-tuned-routing]] → v3.1 `solution-performance-tuned-routing` (renamed) | Cross-feature: extends `HierarchicalRouting`'s per-feature routes (`loadComponent` decisions) and the shell's mount points (`data.preload`) | **Yes** — V1 `online-monolith → async-monolith` |
| VP2 | **GlobalStore** — does the app establish `libs/shared/state` with a classical NgRx Store for cross-cutting state? | Yes / No | Gates VP4, VP5, VP7 | Yes → the GlobalStore part of [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md\|solution-state-tiering]] → v3.1 `solution-global-store` (carved out; the tiering rule + feature Signal Store stay in `solution-state-tiering`) | — | No — V1 materialized it from the first plateau; a real app could add it later |
| VP3 | **BackendDataAccess** — do the app's features talk to a backend through `libs/shared/http-core` + per-feature Facade/Client/Mapper? | Yes / No | Gates VP4, VP6, VP7 | Yes → [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md\|solution-api-http-layer]] → v3.1 migrated; **also modifies** [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md\|solution-repository-structure]] (the `data-access` lib becomes conditional — [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/adr/feature-lib-split-conditional-on-backend-data-access.md|ADR]]) | Cross-feature: the feature Signal Store (`StateTieringPolicy`) calls the Facade directly; `SignalForms`' `submitForm()` callback calls a Facade | No — V1 had it from the first plateau |
| VP4 | **OfflineReadResilience** — a Workbox service worker (content-type-specific caching) + an accurate `isOnline` signal + `OfflineTransportError` in every Client? | Yes / No | **requires VP3=Yes AND VP2=Yes** | Yes → [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md\|solution-offline-first]] → v3.1 migrated | Cross-feature: adds a `connectivity` slice to `GlobalStore`; extends every `data-access` Client (VP3) to throw `OfflineTransportError` | **Yes** — V1 `online-monolith → async-monolith` |
| VP5 | **OfflineWriteQueue** (per feature) — a Dexie-backed, per-feature-partitioned mutation queue with idempotent replay and server-wins conflict handling? | Yes / No | **requires VP4=Yes** | Yes → [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md\|solution-offline-sync]] → v3.1 migrated | Mandatory sub-feature: creates a `notifications` slice in `GlobalStore` ([open question 5](#vp5s-notifications-slice)). Cross-feature: a per-feature Facade opt-in on `OfflineTransportError` | **Yes** — V1 `async-monolith → offline-monolith` |
| VP6 | **BackendLogDelivery** — a `BackendLogSink` (batched, retry-queued to IndexedDB) + a global `ErrorHandler`? | Yes / No | **requires VP3=Yes** | Yes → [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md\|solution-logging-global]] → v3.1 migrated | Cross-feature: registers a second sink on `ConsoleLogging`'s `LOG_SINKS` token and adds `LoggerService.report()`; sends batches via `http-core` (VP3) | **Yes** — V1 `platform-monolith → monitored-app` |
| VP7 | **Authentication** — in-memory access token, silent refresh, permission-string route guards + `*hasPermission` directive? | Yes / No | **requires VP3=Yes AND VP2=Yes** | Yes → [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md\|solution-authentication]] → v3.1 migrated **minus** the `SessionContract` publication, which becomes `platform-host`'s `solution-session-sharing` ([platform-host VP2](skills/angular/architecture/v3.1/platform-host/variability-map.md)) | Cross-feature: adds an `auth` slice to `GlobalStore`; adds an HTTP interceptor (VP3) and route guards attached at each feature's own route (`HierarchicalRouting`) | **Yes** — V1 `monitored-app → multiuser-app` |
| VP8 | **PersistedState** *(aspirational — owner-confirmed row-to-be, no solution yet)* — selected NgRx state synced to `localStorage`/IndexedDB across browser sessions (never the access token)? | Yes / No | **requires VP2=Yes** (or the feature Signal Store tier) | *(none yet)* — a new `solution-persisted-state` authored at Stage 3 | Cross-feature: wraps a `GlobalStore` slice or feature Signal Store with a storage-sync effect; must exclude any field `solution-authentication` marks sensitive | No |

### VP3 modifies `solution-repository-structure`

V1 `solution-repository-structure` mandates "every business feature is split into at least a `feature` lib and a `data-access` lib **from the start**". Once `BackendDataAccess` is a VP, a no-backend feature has no `data-access` lib. That solution is modified (a feature is a `feature` lib; `BackendDataAccess` adds the `data-access` lib for features with server data) and the decision recorded as [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/adr/feature-lib-split-conditional-on-backend-data-access.md|a v3.1 ADR]] per [[skills/common-workflow/architecture/design/solution-update.skill/solution-update.skill.md|solution-update]].

### VP2 gates VP4/VP5/VP7 — the "requires GlobalStore" constraint

`OfflineReadResilience` (`connectivity` slice), `OfflineWriteQueue` (`notifications` slice), and `Authentication` (`auth` slice) each write a classical NgRx slice, which only exists once `GlobalStore=Yes`. This is the same shape as dotnet v3.1's "VP5/VP6/VP7 require VP2 (Persistence)". Not yet backed by a `depends_on` edge in any V1 solution (V1's `solution-state-management` always created the store); Stage 3 adds `depends_on solution-global-store` to `solution-offline-first`, `solution-offline-sync`, `solution-authentication`, recorded as a v3.1 ADR.

### VP3 gates VP4/VP6/VP7 — the "requires BackendDataAccess" constraint

`OfflineReadResilience` needs a Client to throw `OfflineTransportError` and API GET routes to cache; `BackendLogDelivery` sends batches through `http-core`; `Authentication`'s interceptor, silent-refresh and login are all HTTP round trips. V1 backs this partly: `solution-offline-first` and `solution-offline-sync` and `solution-logging-global` already `depends_on solution-api-http-layer`; `solution-authentication` does **not** (V1 gap — Stage 3 adds it).

### VP5's `notifications` slice

`solution-offline-sync` surfaces conflict messages through a `notifications` slice that V1 leaves as a worked-example stub in `solution-state-management` and never actually creates. **Working hypothesis (owner):** `solution-offline-sync` (v3.1) creates it. Confirmed at Stage 3 / delta-conflict.

## Features that are not VPs

- **The seven common baseline features** — every member of this family includes them; they are shared core, not variability. `SignalForms` and `ConsoleLogging` are common *conventions* (zero cost for an app with no forms / that never logs), a deliberate owner call, mirroring dotnet v3.1's `SoftValueObjects` / `AppLogging`.
- **`ComponentTesting`** — **flagged** (feature-model open question 2): V1 puts it in the first plateau (→ common), first principles say visual/a11y *automation* is a quality gate (→ variable). **Working hypothesis: common**, so no VP row. If the owner rules it variable it becomes VP9 (Yes/No, realized by `solution-ui-testing`).
- **The `auth` / `connectivity` / `notifications` slices** — each is a part of VP7 / VP4 / VP5, added to `GlobalStore`, not an independent choice.
- **`solution-ui-testing`'s family-crossed `depends_on`** — it declares `design-system-structure` + `design-system-components`, which are in the `design-system/` catalog. Stage 3 / delta-conflict splits it into a monolith-side realization (deps: `repository-structure` + `app-testing` + `forms`) and a design-system-side one.

## Plateau Map derivation

**No plateaus exist in `v3.1/monolith/` yet.** This section will list one row per `v3.1/monolith/plateau/*` once `plateau-create-by-solutions` runs.

### Reference: how the V1 plateaus map onto these VPs

The V1 main chain realizes a staged subset of this space. Expressed in v3.1 monolith VP IDs (V1 always materialized `GlobalStore` and `BackendDataAccess` from its first plateau, so `online-monolith` already has VP2=Yes, VP3=Yes):

| V1 plateau | v3.1 monolith VP answers | Notes |
| --- | --- | --- |
| `plateau-online-monolith` | VP2=Yes, VP3=Yes; VP1/VP4–VP8 = No | v3.1 also allows a *thinner* plateau below this — VP2=No, VP3=No (a local-only app) — which V1 has no equivalent for |
| `plateau-async-monolith` | + VP1=Yes, VP4=Yes | preloading/budgets + read resilience |
| `plateau-offline-monolith` | + VP5=Yes (per feature) | durable write queue |
| `plateau-monitored-app` | + VP6=Yes | *(V1 also adds the whole `platform-host` layer here — that part moves to [[skills/angular/architecture/v3.1/platform-host/variability-map.md|platform-host]])* |
| `plateau-multiuser-app` | + VP7=Yes | *(V1's final chain plateau; the federation/session parts are `platform-host` + `embeddable-app`)* |

Every reference row is consistent with every stated Constraint: no row sets VP4/VP5/VP7 without VP2=Yes and VP3=Yes, no row sets VP5 without VP4, no row sets VP6 without VP3.

### Combinations v3.1 allows that V1 has no plateau for

- **VP2=No, VP3=No** — a purely local app (no backend, no global store): tools, calculators, offline-only editors. V1's chain starts at `online-monolith` which already has both.
- **VP3=Yes, VP2=No** — a backend-connected app with only component + feature-level state, no cross-cutting store.
- **VP7=Yes without the federation layer** — an authenticated monolith that is not a platform host. V1 only introduces auth on the platform chain (`multiuser-app` descends from `platform-monolith`).

## Out of scope

- **Realized-by links are provisional** — they point at V1 solutions + name the planned v3.1 solution; Stage 3 repoints them into `v3.1/solutions/`.
- **`Migration = Yes`** is asserted for VP1/VP4/VP5/VP6/VP7 on the strength of the V1 plateau chain, which documents exactly those transitions as named milestones. dotnet v3.1 marked everything `No` for lack of an observed transition; the Angular V1 chain **is** that evidence.
- **Constraint evidence is uneven**: VP4→VP3 and VP5→VP4 and VP6→VP3 are backed by real V1 `depends_on` edges. VP2's gating of VP4/VP5/VP7, VP3's gating of VP7, and VP7→VP3 are v3.1 decisions to be encoded in `depends_on` at Stage 3 (v3.1 ADR).
- **`platform-host/`, `embeddable-app/`, `design-system/` are separate catalogs** — federation, session sharing, remotes, and the component library are modeled there.
