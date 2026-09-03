---
name: rehydration-timing
description: When persisted state is read back and merged — synchronously in a per-feature metaReducer at store init for slices, in an onInit hook for feature Signal Stores — never as a post-render patch
problem: Persisted state must be back in the store before the first render that reads it, or the user sees default state flash to the persisted value. NgRx offers several injection points (metaReducer, APP_INITIALIZER, an effect on ROOT_EFFECTS_INIT, a component effect) with different timing guarantees.
decision: A persisted classical-NgRx slice is rehydrated synchronously inside a per-feature metaReducer, on the @ngrx/store init action, before any component subscribes. A persisted feature Signal Store is rehydrated in its withHooks onInit, before the first template read. Neither uses an effect or a post-render patch. Writes are debounced to a microtask.
tags:
  - solution/persisted-state
  - stack/typescript
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem

If persisted state is merged back *after* the first render, the user sees a visible transition — the theme toggles from light to dark a frame in, the density switch jumps, a half-filled form appears empty and then fills. The fix is to have the persisted value in the store before anything reads it. NgRx and Angular offer several places to do the merge, and they do not all run early enough:

- a **metaReducer** wrapping the feature reducer — runs on the `@ngrx/store` init action, before any selector emits;
- **`provideAppInitializer` / `APP_INITIALIZER`** — runs before bootstrap completes but is designed for async work and returns a `Promise`;
- an **effect on `ROOT_EFFECTS_INIT`** — runs after the store is already live and components may have rendered;
- a **component `effect()` / `ngOnInit`** — runs after that component is on screen.

# Selected variant

**Selected variant:** [[#Synchronous metaReducer for slices, onInit hook for feature stores (selected)]]

# Searched variants

## Synchronous metaReducer for slices, onInit hook for feature stores (selected)

### Description

`persistKeys({ key, keys, storage })` returns a `MetaReducer`. On the `@ngrx/store` init action it reads `storage.getItem(key)`, parses it, and returns `{ ...state, ...pickAllowed(parsed) }` as the initial state. On every subsequent action it reduces normally and then, in a debounced microtask, writes `pick(nextState, keys)` back. The metaReducer is attached per-feature: `provideState(preferencesFeature, { metaReducers: [persistKeys(...)] })`.

For a feature Signal Store, `withPersistedDraft({ key, keys, storage })` is a `signalStoreFeature` whose `withHooks({ onInit })` does the same read-and-`patchState` before the component's template runs, and registers an `effect()` that writes the allow-listed keys back on change.

### Benefits

- The `localStorage` read is synchronous, so on store init the slice's *initial* state already contains the persisted value — the first `select()` a component makes returns the right thing. No flash, no `APP_INITIALIZER` promise to await, no bootstrap delay.
- `withHooks({ onInit })` runs when the store is constructed, which for a `providedIn: 'root'` store or one provided on a route is before that route's component template is evaluated — same "before first read" guarantee at the feature tier.
- Writes debounced to a microtask coalesce a burst of `patchState` calls (a form's keystrokes dispatch many actions) into one `setItem` per tick.
- The metaReducer is per-feature, so persistence is visibly opt-in at the `provideState` call site — grep-able, and impossible to enable "for the whole store".

### Costs

- The metaReducer must tolerate a malformed / stale / partial JSON string in storage (a previous app version wrote a different shape). It wraps the parse in `try/catch` and intersects parsed keys with the current allow-list, falling back to `initialState` on any mismatch. This is real code that must be tested, not free.
- IndexedDB-backed drafts cannot use this synchronous path — they hydrate asynchronously in `onInit` and accept a one-frame default state for a large draft that is usually off-screen at boot anyway. Accepted, and the reason the large-draft path is a deliberate exception (see [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/adr/storage-backend-choice.md|storage-backend-choice]]).

## An effect on ROOT_EFFECTS_INIT that dispatches a Hydrate action

### Description

An effect listens for `ROOT_EFFECTS_INIT`, reads storage, and dispatches `PreferencesHydrated({ ...values })`; the reducer applies it.

### Benefits

- All the persistence logic sits in the effects file, consistent with how `connectivity` and `auth` do their side effects.
- The hydrate action shows up in the DevTools log.

### Costs

- `ROOT_EFFECTS_INIT` fires *after* the store is constructed and after `provideStore`'s init action — a component that reads the selector synchronously in its constructor (or a resolver that runs first) sees default state, then the hydrate action lands and it flips. The flash this solution exists to prevent.
- Adds an effects class and an action to a slice that otherwise needs neither.

## APP_INITIALIZER / provideAppInitializer

### Description

A `provideAppInitializer` reads every persisted key from storage and dispatches the hydrate actions before bootstrap resolves.

### Benefits

- Guaranteed to complete before the first component renders.
- One central place listing everything that gets rehydrated.

### Costs

- `provideAppInitializer` is built for asynchronous startup work and blocks bootstrap on its returned promise; using it for a synchronous `localStorage.getItem` couples every persisted slice to app startup and centralizes a list that should live at each slice's own `provideState` call.
- The central initializer has to know about every persisted slice — the opposite of the per-slice opt-in this solution wants.

## A component effect() in the consuming component

### Description

Each component that cares reads storage in an `effect()` / `ngOnInit` and patches the store.

### Costs

- Runs after the component is on screen — maximal flash. Also scatters the persistence contract across every consumer and makes "which state is persisted" un-answerable without reading every component. Rejected outright.
