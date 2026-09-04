# monolith plateaus

Six plateaus, built by `plateau-create-by-solutions` from the catalogue in `../../solutions/`.
**Flat lineage** — a single chain, each `standalone: true`, each `parent_plateaus` entry the single
previous plateau. Capabilities are **cumulative**: everything the parent has, plus its own delta.

## Plateau × VP matrix

Rows = plateaus, columns = the 8 monolith Variation Points (full definitions:
[`../variability-map.md`](../variability-map.md)). `Yes` = the VP is realized at that plateau,
`no` = it is not. Answers are **cumulative** down the chain — a plateau has every VP its parent has,
plus its own. Scan a **column** for the shallowest plateau that includes a VP; read a **row** for a
plateau's complete VP set.

| Plateau | VP1 | VP2 | VP3 | VP4 | VP5 | VP6 | VP7 | VP8 |
|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| plateau-online-monolith        | no  | Yes | Yes | no  | no  | no  | no  | no  |
| plateau-async-monolith         | Yes | Yes | Yes | no  | no  | no  | no  | no  |
| plateau-offline-read-monolith  | Yes | Yes | Yes | Yes | no  | no  | no  | no  |
| plateau-offline-full-monolith  | Yes | Yes | Yes | Yes | Yes | no  | no  | no  |
| plateau-multiuser-monolith     | Yes | Yes | Yes | Yes | Yes | Yes | Yes | no  |
| plateau-persisted-state-monolith | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |

| VP | Name | First realized at | Solution |
|---|---|---|---|
| **VP1** | PerformanceTunedRouting — selective preload + `loadComponent` split + enforced bundle budgets | plateau-async-monolith | `solution-performance-tuned-routing` |
| **VP2** | GlobalStore — `libs/shared/state`, classical NgRx for cross-cutting state | plateau-online-monolith | `solution-global-store` |
| **VP3** | BackendDataAccess — `libs/shared/http-core` + per-feature Facade/Client/Mapper | plateau-online-monolith | `solution-api-http-layer` |
| **VP4** | OfflineReadResilience — Workbox SW, `connectivity` slice, `OfflineTransportError` | plateau-offline-read-monolith | `solution-offline-first` |
| **VP5** | OfflineWriteQueue — Dexie queue, idempotent replay, per-entity `syncStatus` | plateau-offline-full-monolith | `solution-offline-sync` |
| **VP6** | BackendLogDelivery — `BackendLogSink` + bounded IndexedDB retry queue + `GlobalErrorHandler` | plateau-multiuser-monolith | `solution-logging-global` |
| **VP7** | Authentication — in-memory access token, silent refresh, permission-string guards | plateau-multiuser-monolith | `solution-authentication` |
| **VP8** | PersistedState — `persistKeys()` metaReducer + `SENSITIVE_STATE_KEYS` guard + `withPersistedDraft()` + a persisted `preferences` slice | plateau-persisted-state-monolith | `solution-persisted-state` |

Constraints between VPs (from the variability map): VP4 requires VP2 **and** VP3; VP5 requires VP4;
VP6 requires VP3; VP7 requires VP2 **and** VP3; VP8 requires VP2. Every row above satisfies them.

## Lineage & new solutions

| # | Plateau | Parent | New solutions in its `created_by` (on top of the parent chain) |
|---|---------|--------|-----------------------------------|
| 1 | **plateau-online-monolith** | — (from scratch) | `solution-repository-structure`, `solution-app-routing`, `solution-state-tiering`, `solution-global-store`, `solution-forms`, `solution-api-http-layer`, `solution-logging-base`, `solution-app-testing`, `solution-ui-testing` |
| 2 | **plateau-async-monolith** | online-monolith | `solution-performance-tuned-routing` |
| 3 | **plateau-offline-read-monolith** | async-monolith | `solution-offline-first` |
| 4 | **plateau-offline-full-monolith** | offline-read-monolith | `solution-offline-sync` — *owner's current app* |
| 5 | **plateau-multiuser-monolith** | offline-full-monolith | `solution-logging-global`, `solution-authentication` — *`plateau-platform-host`'s parent* |
| 6 | **plateau-persisted-state-monolith** | multiuser-monolith | `solution-persisted-state` — no new Nx project |

Build scaffolding (anchor contract, mechanical check, decisions log) is in [`../../agent/`](../../agent/) —
run `bash skills/angular/architecture/v3.1/agent/check.sh` after any change.

## What each plateau folder holds

```
plateau-{name}/
  plateau-{name}.skill/
    plateau-{name}.skill.md      the plateau summary an agent reads before writing code
    example/                     a runnable Nx workspace — vitest + nx lint + prod build (+ build-sw) green
  structure/                     one skill per project + per class (prefix `plateau-{name}--`)
  registry/                      delta-conflict-detection ordering records (per plateau)
```

| Plateau | structure skills | registry | example gates |
|---|---|---|---|
| plateau-online-monolith | 27 | 3 | vitest, `nx lint` (10 projects), prod build |
| plateau-async-monolith | 28 | 1 | + `order-report` emitted as its own lazy chunk |
| plateau-offline-read-monolith | 31 | 2 | + `nx build-sw` (`dist/.../sw.js`) |
| plateau-offline-full-monolith | 36 | 1 | vitest 20 files / 70 tests, `nx lint` (11), prod build 446 kB, build-sw |
| plateau-multiuser-monolith | 44 | 4 | vitest 28 files / 98 tests, `nx lint` (12), prod build 454 kB, build-sw |
| plateau-persisted-state-monolith | 48 | 1 | vitest 31 files / 115 tests, `nx lint` (12), prod build 458 kB, build-sw; e2e typechecks |

The example evolves **one Nx workspace** down the chain — each plateau's `example/` is a snapshot of
that workspace grown by that plateau's solutions (living workspace at `/tmp/ng-ex/online-monolith`
where this was built). Playwright specs are written and configured throughout but were not executed
in the build sandbox.

## `registry/` — DOP step 6

When two or more solutions modify the same code element and the interaction is only about **ordering**
or is a **benign N≥3 bucket** (not a real semantic conflict), `delta-conflict-detection` records a
per-element file in the `registry/` folder of the shallowest plateau where all the intersecting
solutions coexist. Every Angular v3.1 group is **canonical — zero resolver solutions**. Highlights:

- **`monolith-repository`** / **`platform-shell-project`** — repo- and composition-root buckets; every
  feature adds one distinct tag / allow-list row / bootstrap wiring. `source: ordering-only`, N≥3 benign.
- **`shared-state-project`** — the `store.config.ts` slice seam: `global-store` `.create` + `offline-first`
  / `offline-sync` / `authentication` / `persisted-state` `.extend` (one distinct slice each; VP8 also
  adds a feature-local persistence metaReducer on `preferences`). `TMN`, `source: constraint`
  (every slice-adding VP requires VP2). N = 4 at `plateau-multiuser-monolith`, **N = 5** at
  `plateau-persisted-state-monolith`. Closes delta-conflict **Finding 4**.
- **`feature-facade-ts`** / **`feature-routes-ts`** — a feature's Facade / routes array as the natural
  attachment point for `api-http-layer` + `offline-sync` (queueing branch) / `performance-tuned-routing`
  + `offline-sync` (route providers) + `authentication` (guard). `TMN` / `FMN`, member-disjoint.

Full classification: [`../../delta-conflict-analysis.md`](../../delta-conflict-analysis.md).
