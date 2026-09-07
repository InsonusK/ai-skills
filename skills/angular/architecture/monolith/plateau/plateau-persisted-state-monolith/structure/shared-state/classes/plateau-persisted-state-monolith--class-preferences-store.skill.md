---
name: plateau-persisted-state-monolith--class-preferences-store
description: The preferences slice in libs/shared/state — a classical NgRx createFeature holding user UI preferences (theme, list density, last-opened feature tab), persisted to localStorage via persistKeys() — the reference persisted slice — persisted-state-monolith plateau
domain: skill
type: template
whenToUse: when editing the preferences slice (VP8) — setTheme / setDensity / rememberFeatureTab, or adding a field (which means editing the persistKeys allow-list too)
plateau: persisted-state-monolith
artifact_type: store
version: 20260903190000
tags:
  - skill/template/class
  - plateau/persisted-state-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]]"

> `libs/shared/state/src/lib/preferences/`. Registered via `provideGlobalStore()` with a `persistKeys` metaReducer whose allow-list equals every field. The reference persisted slice — flat, all-scalar, no effects.

# Goal

- Hold cross-cutting UI preferences one place so any feature reads the same theme / density
- Be the worked example of the persisted-slice pattern

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/preferences.store.ts.create.md|GlobalStore/preferences.store.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Every field is a scalar (or `null`) — safe to persist and to `JSON.stringify` losslessly
- No effects — a component that needs the resolved theme derives it from `selectTheme()` + its own `matchMedia` signal
- The `persistKeys` allow-list is exactly `['theme', 'density', 'lastFeatureTab']` — every field, listed literally

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/preferences.store.ts.create.md|GlobalStore/preferences.store.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Action group | `{Slice}Actions` | `PreferencesActions` | `{slice}.actions.ts` | `preferences.actions.ts` |
| Feature + reducer | `{slice}Feature` / `{slice}Reducer` | `preferencesFeature` | `{slice}.reducer.ts` | `preferences.reducer.ts` |
| Public selectors | `select{X}` | `selectTheme` / `selectDensity` / `selectLastFeatureTab` | `{slice}.selectors.ts` | `preferences.selectors.ts` |

# Implementation

```typescript
// Skill: class-preferences-store
// Plateau: persisted-state-monolith
// Version: 20260903190000
import { createActionGroup, props } from '@ngrx/store';
import { createFeature, createReducer, on } from '@ngrx/store';

export type ThemeChoice = 'system' | 'light' | 'dark';
export type Density = 'comfortable' | 'compact';

export interface PreferencesState {
  readonly theme: ThemeChoice;
  readonly density: Density;
  readonly lastFeatureTab: string | null;
}

export const PreferencesActions = createActionGroup({
  source: 'Preferences',
  events: {
    'Set Theme': props<{ theme: ThemeChoice }>(),
    'Set Density': props<{ density: Density }>(),
    'Remember Feature Tab': props<{ tab: string }>(),
  },
});

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

export const { reducer: preferencesReducer, selectTheme, selectDensity, selectLastFeatureTab } = preferencesFeature;
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/preferences.store.ts.create.md|GlobalStore/preferences.store.ts.create]]

# Rules

## MUST
- Every field of `PreferencesState` is a scalar or `null` — a `Date` / `Map` / nested object does not round-trip through JSON.
- The slice has no effects.
- The `persistKeys` allow-list lists every field literally — adding a field is a deliberate one-line edit to the allow-list.
- Actions are named for the user intent (`setTheme`), not a generic `patch`.
- Never apply several plateau templates per class/artifact.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/preferences.store.ts.create.md|GlobalStore/preferences.store.ts.create]]

# Check list

- [ ] `PreferencesState` is flat and all-scalar
- [ ] No `preferences.effects.ts`
- [ ] Registered with `persistKeys` and an allow-list equal to every field
- [ ] `index.ts` exports the selectors + `PreferencesActions`, not the reducer

# Unittest TestCases

- [ ] WHEN `setTheme({ theme: 'dark' })` is dispatched THEN `selectTheme` emits `'dark'`
- [ ] WHEN the slice is registered with `persistKeys` and storage holds `{"theme":"dark","density":"compact"}` THEN the initial selectors reflect those before any component subscribes
- [ ] WHEN `setDensity` is dispatched THEN after a microtask `localStorage.getItem('app:preferences')` reflects the new density

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/preferences.store.ts.create.md|GlobalStore/preferences.store.ts.create]]
