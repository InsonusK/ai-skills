# plateau-persisted-state-monolith — reference example

The `plateau-multiuser-monolith` workspace, evolved one plateau further down the chain — **the
sixth and last plateau of the monolith chain**. Everything from the parent chain still applies (see
those READMEs). This file records only the VP8 delta: `solution-persisted-state`.

**No new Nx project.** VP8 adds one folder and one slice to `libs/shared/state`, plus an opt-in
draft store to the `orders` feature.

## VP8 — PersistedState (`solution-persisted-state`)

| VP8 piece | Where |
| --- | --- |
| `persistKeys()` — a per-feature `MetaReducer` that rehydrates an allow-listed subset **synchronously** on the store-init action, then writes those keys back debounced to a microtask; wraps every storage read/write in `try/catch` | libs/shared/state/src/lib/persistence/persisted-state.ts (+ persisted-state.spec.ts) |
| `SENSITIVE_STATE_KEYS` (`accessToken`, `refreshToken`) + `assertPersistable()` — called inside `persistKeys()` / `withPersistedDraft()`, **throws at construction** if the allow-list names a sensitive key | libs/shared/state/src/lib/persistence/persisted-state.ts |
| `withPersistedDraft()` — a `signalStoreFeature` (`withHooks({ onInit })`) that rehydrates a feature Signal Store before its first template read and persists on change via an `effect()` | libs/shared/state/src/lib/persistence/with-persisted-draft.ts |
| `preferences` slice — classical NgRx `createFeature`: `theme` / `density` / `lastFeatureTab`, all scalar; the reference persisted slice, allow-list = every field | libs/shared/state/src/lib/preferences/preferences.{actions,reducer,selectors}.ts (+ preferences.spec.ts) |
| Registration — `provideState(preferencesFeature.name, preferencesFeature.reducer, { metaReducers: [persistKeys(...)] })` (the three-arg overload — the two-arg `provideState(feature, config)` form silently ignores `metaReducers`); `auth` still `provideState(authFeature)` with **no** metaReducer | libs/shared/state/src/lib/store.config.ts (+ store.config.spec.ts) |
| `OrderDraftStore` — a dedicated `signalStore` + `withPersistedDraft` holding only `product` / `quantity`; the order form binds `[ngModel]`/`(ngModelChange)` to it and calls `clear()` on submit success | libs/orders/feature/src/lib/orders-draft.store.ts (+ spec), .../order-form.component.ts |

## Running

    npm install
    npm test           # Vitest, jsdom — 31 files / 115 tests green (fake-indexeddb shims IndexedDB for the Dexie queues; localStorage is jsdom's own)
    npm run lint       # nx run-many -t lint — 12 projects green
    npm run build:prod # nx build platform-shell --configuration=production — initial 458 kB
    npm run build:sw   # nx build-sw platform-shell — sw.js precaches 9 files
    npx playwright install chromium
    npm run e2e        # apps/platform-shell-e2e (includes persisted-draft.e2e.spec.ts)

## Verified state

`npm test` (11 projects), `npm run lint` (12 projects), `nx build platform-shell --configuration=production`
(initial 458 kB), and `nx build-sw platform-shell` are all GREEN. `apps/platform-shell-e2e` typechecks
(`tsc -p apps/platform-shell-e2e/tsconfig.json`). Playwright suites are written and configured but were
not executed where this example was built.

## Fed back into the catalog

- **`provideState(feature, { metaReducers })` silently ignores the config.** `createFeature` returns
  a `FeatureSlice` object, which matches the one-arg `provideState(slice)` overload — the second
  argument is dropped and the persistence metaReducer never runs. The three-arg
  `provideState(feature.name, feature.reducer, { metaReducers })` is the only form that applies it.
  `solution-persisted-state`'s Implementation and the plateau skills were corrected to use it, and
  a `Risk:` bullet on `shared-state.project.extend` now records the trap.
- **The `withPersistedDraft` persist `effect()` needs an explicit flush in tests.** In a running app
  the effect flushes on change detection; a pure `TestBed.inject(Store)` test must call
  `TestBed.tick()` (Angular 20+) to see the write. The `orders-draft.store.spec.ts` does this via a
  `settle()` helper. Rehydration (in `onInit`, synchronous) needs no flush.
- **`auth` is structurally excluded from persistence, not just by an allow-list.** The rule is that
  the slice holding the in-memory token is never given a `metaReducers` entry at all —
  `store.config.spec.ts` asserts no `auth`-related key is ever written to `localStorage`. This is
  stricter than "the allow-list omits `accessToken`" and cannot drift.
- Budgets unchanged from VP5/VP7 (`initial` 500 kB warn / 600 kB error). The `persistence/` +
  `preferences/` additions are ~1 kB of app code; measured initial went 454 kB → 458 kB.
