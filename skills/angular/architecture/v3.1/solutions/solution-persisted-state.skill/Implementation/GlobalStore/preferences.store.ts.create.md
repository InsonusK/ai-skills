---
description: The preferences slice — a classical-NgRx createFeature holding user UI preferences (theme, list density, last-opened feature tab) that survive a browser session via the persistKeys() metaReducer
project_name: shared-state
name: preferences
element_kind: store
change_kind: create
tags:
  - solution/persisted-state
  - element/preferences-store
---

# How this generic file is used

Create at `libs/shared/state/src/lib/preferences/`. This is the catalog's first persisted slice and the worked example of the pattern — a small, flat, all-scalar slice whose every field is safe to survive a session.

# Goals

- Hold cross-cutting UI preferences one place so any feature reads the same theme / density
- Be the reference persisted slice: flat, all-scalar, an explicit allow-list equal to every field, no effects

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Slice feature | `{name}Feature` | `preferencesFeature` | `{name}.reducer.ts` | `preferences.reducer.ts` |
| Actions group | `{Name}Actions` | `PreferencesActions` | `{name}.actions.ts` | `preferences.actions.ts` |

# Implementation changes

```
/libs/shared/state/src/lib/preferences
  preferences.actions.ts
  preferences.reducer.ts     <- createFeature; selectTheme / selectDensity / selectLastFeatureTab
  preferences.selectors.ts
  preferences.spec.ts
```

```typescript
// preferences.reducer.ts
import { createFeature, createReducer, on } from '@ngrx/store';
import { PreferencesActions } from './preferences.actions';

export type ThemeChoice = 'system' | 'light' | 'dark';
export type Density = 'comfortable' | 'compact';

export interface PreferencesState {
  readonly theme: ThemeChoice;
  readonly density: Density;
  readonly lastFeatureTab: string | null;
}

const initialState: PreferencesState = { theme: 'system', density: 'comfortable', lastFeatureTab: null };

export const preferencesFeature = createFeature({
  name: 'preferences',
  reducer: createReducer(
    initialState,
    on(PreferencesActions.setTheme, (s, { theme }) => ({ ...s, theme })),
    on(PreferencesActions.setDensity, (s, { density }) => ({ ...s, density })),
    on(PreferencesActions.rememberFeatureTab, (s, { tab }) => ({ ...s, lastFeatureTab: tab })),
  ),
});

export const {
  name: preferencesFeatureKey,
  reducer: preferencesReducer,
  selectTheme,
  selectDensity,
  selectLastFeatureTab,
} = preferencesFeature;
```

# Rule changes

## MUST
- Every field of `PreferencesState` is a scalar (or `null`), safe to persist and to `JSON.stringify` losslessly.
  - Risk: a `Date`, a `Map`, or a nested object in a persisted slice round-trips through JSON as a string / `{}` and comes back the wrong type.
  - Fix: keep the slice flat and primitive; a structured draft belongs in a feature store with `withPersistedDraft`, not here.
- The slice has no effects.
  - Risk: an effect that, say, calls `matchMedia` to resolve `'system'` turns a pure preference store into one with a side-effect lifecycle to test and register.
  - Fix: `provideState(preferencesFeature)` only — a component that needs the resolved theme derives it from `selectTheme()` + its own `matchMedia` signal.
- The `persistKeys` allow-list for this slice is exactly `['theme', 'density', 'lastFeatureTab']` — every field, listed explicitly.
  - Risk: `keys: Object.keys(initialState)` or `'*'` silently persists any field added later, including one that should not survive a session.
  - Fix: a literal array; adding a field is a deliberate one-line edit to the allow-list.
- Actions are named for the user intent (`setTheme`, `rememberFeatureTab`), not the field.
  - Risk: a generic `patch({ ...Partial<PreferencesState> })` action makes the DevTools log unreadable and lets a caller set any field.
  - Fix: one narrowly-typed action per intent.

# Check list

- [ ] `PreferencesState` is flat and all-scalar
- [ ] No `preferences.effects.ts` exists
- [ ] The slice is registered with `persistKeys` and the allow-list lists every field literally
- [ ] `index.ts` exports the selectors + `PreferencesActions`, not the reducer

# Unittest TestCases

- [ ] WHEN `PreferencesActions.setTheme({ theme: 'dark' })` is dispatched THEN
  - [ ] `selectTheme` emits `'dark'`
- [ ] WHEN the slice is registered with `persistKeys` and storage holds `{"theme":"dark","density":"compact"}` THEN
  - [ ] the initial `selectTheme` / `selectDensity` are `'dark'` / `'compact'` before any component subscribes
- [ ] WHEN `setDensity` is dispatched THEN
  - [ ] after a microtask, `localStorage.getItem('app:preferences')` reflects the new density
