---
description: Extend libs/shared/state with the persistence/ mechanism folder and the first persisted slice (preferences), registered in store.config.ts with the persistKeys() metaReducer
name: shared-state
project_kind: library
element_kind: project
change_kind: extend
tags:
  - solution/persisted-state
  - element/shared-state-project
---

# Goals

- Add the `persistence/` mechanism (the `persistKeys()` factory + the `withPersistedDraft()` signalStoreFeature + `SENSITIVE_STATE_KEYS`) to the global state library
- Add the `preferences` slice and register it, in the same `store.config.ts` seam, wrapped with its persistence metaReducer
- Leave every existing slice (`connectivity`, `notifications`, `auth`) untouched — `auth` in particular is never given a metaReducer

# Structure

## Project Structure

```
/libs/shared/state
  /src
    /lib
      persistence/                  <- new (VP8 / solution-persisted-state)
        persisted-state.ts          <- persistKeys() metaReducer + SENSITIVE_STATE_KEYS + assertPersistable()
        with-persisted-draft.ts     <- signalStoreFeature for the feature tier
        persisted-state.spec.ts
      preferences/                  <- new (VP8) — the first persisted slice
        preferences.actions.ts
        preferences.reducer.ts
        preferences.selectors.ts
        preferences.spec.ts
      store.config.ts               <- preferences registered with persistKeys(...) metaReducer
    index.ts                        <- + persistKeys, withPersistedDraft, SENSITIVE_STATE_KEYS, preferences selectors/actions
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| persistence/ | The persistence mechanism. `persisted-state.ts` per [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/persisted-state.ts.create.md|persisted-state.ts.create]]; `with-persisted-draft.ts` per [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/FeatureStore/with-persisted-draft.ts.create.md|with-persisted-draft.ts.create]]. |
| preferences/ | The `preferences` slice (`theme` / `density` / `lastFeatureTab`). Per [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/preferences.store.ts.create.md|preferences.store.ts.create]]. |

# Implementation changes

Register `preferences` in the existing `provideGlobalStore()` seam, wrapped with the persistence metaReducer:

```typescript
// libs/shared/state/src/lib/store.config.ts
import { provideState } from '@ngrx/store';
import { preferencesFeature, PreferencesState } from './preferences/preferences.reducer';
import { persistKeys } from './persistence/persisted-state';

export function provideGlobalStore(): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideStore({}),
    provideState(connectivityFeature),
    provideState(notificationsFeature),
    provideState(authFeature),                        // <- never a metaReducer
    // three-arg provideState(name, reducer, config) — the only overload that takes metaReducers
    provideState(preferencesFeature.name, preferencesFeature.reducer, {   // <- new (VP8)
      metaReducers: [
        persistKeys<PreferencesState>({
          key: 'app:preferences',
          keys: ['theme', 'density', 'lastFeatureTab'],
        }),
      ],
    }),
    provideEffects(ConnectivityEffects, AuthEffects),
  ]);
}
```

`index.ts` gains: `persistKeys`, `withPersistedDraft`, `SENSITIVE_STATE_KEYS`, `assertPersistable`, `type PersistConfig`, `selectTheme` / `selectDensity` / `selectLastFeatureTab`, `PreferencesActions`, `preferencesFeature` / `type PreferencesState` / `type ThemeChoice` / `type Density`.

# Rules

## MUST
- `preferences` is registered in the same `provideGlobalStore()` / `store.config.ts` seam as every other slice — not through a separate provider call.
  - Risk: a slice registered on its own is easy to miss in tests and in a second consumer app.
  - Fix: one more `provideState(...)` line inside `provideGlobalStore()`.
- The `persistKeys` metaReducer is attached at the feature's own `provideState(name, reducer, { metaReducers: [...] })` — never as a global metaReducer on `provideStore`.
  - Risk: a global metaReducer applies to every slice, so `auth` would be persisted too. Also: `provideState(feature, { metaReducers })` (two args, a `FeatureSlice` object) silently ignores the config — only the three-arg `provideState(name, reducer, config)` overload applies it.
  - Fix: `provideState(preferencesFeature.name, preferencesFeature.reducer, { metaReducers: [persistKeys(...)] })`; the opt-in is visible at each slice's registration.
- `provideState(authFeature)` carries no `metaReducers` — ever.
  - Risk: the slice holding the in-memory access token must not be a persistence target, even with an allow-list that excludes the token today.
  - Fix: leave `authFeature`'s registration exactly as `solution-authentication` left it.
- The `persistence/` folder holds mechanism only — no slice-specific config.
  - Risk: putting `preferences`'s key list inside `persisted-state.ts` couples the generic factory to one consumer.
  - Fix: `persisted-state.ts` exports `persistKeys`; the `key` / `keys` config lives at the `store.config.ts` call site.

## SHOULD
- **Registering a second global metaReducer array to "keep persistence config together"** — Consequence: reintroduces the whole-store persistence risk and hides which slices are actually persisted — Instead: one `metaReducers` entry per persisted `provideState` call

# Check list

- [ ] `persistence/` and `preferences/` folders exist under `libs/shared/state/src/lib`
- [ ] `preferences` is registered via `provideState(preferencesFeature, { metaReducers: [persistKeys(...)] })` inside `provideGlobalStore()`
- [ ] `authFeature`'s registration has no `metaReducers`
- [ ] `index.ts` re-exports `persistKeys` / `withPersistedDraft` / `SENSITIVE_STATE_KEYS` and the `preferences` selectors + actions

# Unittest TestCases

- [ ] WHEN `provideGlobalStore()` is wired in a TestBed and `localStorage` holds `app:preferences` = `{"theme":"dark"}` THEN
  - [ ] the store's initial `preferences.theme` is `'dark'`
- [ ] WHEN the store snapshot keys are inspected THEN
  - [ ] they are `['auth', 'connectivity', 'notifications', 'preferences']`
- [ ] WHEN `PreferencesActions.setDensity({ density: 'compact' })` is dispatched THEN
  - [ ] after a microtask `localStorage.getItem('app:preferences')` contains `"density":"compact"`
