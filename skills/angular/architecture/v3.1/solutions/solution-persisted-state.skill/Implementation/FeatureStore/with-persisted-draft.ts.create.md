---
description: withPersistedDraft() — a signalStoreFeature that rehydrates an allow-listed subset of a feature Signal Store from storage in its onInit hook and writes it back on change, reusing the SENSITIVE_STATE_KEYS guard
project_name: shared-state
name: with-persisted-draft
element_kind: module
change_kind: create
tags:
  - solution/persisted-state
  - element/with-persisted-draft-ts
---

# How this generic file is used

Create once at `libs/shared/state/src/lib/persistence/with-persisted-draft.ts`. A feature Signal Store that wants an in-progress form draft to survive a reload composes this feature into its `signalStore(...)` call.

# Goals

- Give the feature tier the same explicit, allow-listed, sensitive-key-guarded persistence the global tier gets from `persistKeys()`
- Rehydrate in `withHooks({ onInit })` — before the component's template reads the store — never a post-render patch
- Persist only a genuine *draft* (in-progress input), never server data or a `loading` flag

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Signal store persistence feature | `withPersistedDraft` | `withPersistedDraft` | `with-persisted-draft.ts` | `with-persisted-draft.ts` |

# Implementation changes

File: `libs/shared/state/src/lib/persistence/with-persisted-draft.ts`

```typescript
import { effect, untracked } from '@angular/core';
import { getState, patchState, signalStoreFeature, withHooks, type WritableStateSource } from '@ngrx/signals';
import { assertPersistable, type PersistConfig } from './persisted-state';

export function withPersistedDraft<State extends object>(config: PersistConfig<State>) {
  assertPersistable(config);
  const storage = config.storage ?? localStorage;
  const pick = (s: State) =>
    config.keys.reduce((acc, k) => ((acc[k] = s[k]), acc), {} as Partial<State>);

  return signalStoreFeature(
    withHooks({
      onInit(store: WritableStateSource<State>) {
        // 1. rehydrate synchronously, before the first template read
        try {
          const raw = storage.getItem(config.key);
          if (raw) {
            const parsed = JSON.parse(raw) as Partial<State>;
            const restore = config.keys
              .filter((k) => k in parsed)
              .reduce((acc, k) => ((acc[k] = parsed[k] as State[typeof k]), acc), {} as Partial<State>);
            patchState(store, restore);
          }
        } catch {
          /* stale or malformed — start from the store's own initial state */
        }

        // 2. persist the allow-listed keys on every change (effect coalesces per tick)
        effect(() => {
          const snapshot = pick(getState(store));
          untracked(() => {
            try {
              storage.setItem(config.key, JSON.stringify(snapshot));
            } catch {
              /* storage full/unavailable — a convenience, never fatal */
            }
          });
        });
      },
    }),
  );
}
```

Usage in a feature store:

```typescript
export const OrderDraftStore = signalStore(
  { providedIn: 'root' },
  withState<OrderDraftState>({ product: '', quantity: 1, notes: '' }),
  withPersistedDraft<OrderDraftState>({ key: 'app:orders:draft', keys: ['product', 'quantity', 'notes'] }),
  withMethods(/* ... */),
);
```

# Rule changes

## MUST
- `withPersistedDraft` calls `assertPersistable()` — the same `SENSITIVE_STATE_KEYS` guard the global tier uses.
  - Risk: the feature tier having its own weaker check lets a token-bearing field slip through on this path.
  - Fix: import and call the one shared `assertPersistable`.
- Only genuine draft input is on the allow-list — never server-fetched entities, never `loading` / `error` / `submitting` flags.
  - Risk: persisting the loaded list means a reload shows stale server data as if it were fresh; persisting `submitting: true` leaves the UI wedged.
  - Fix: the `keys` array names only the form-field state the user is editing.
- Rehydration is in `withHooks({ onInit })`, synchronous.
  - Risk: hydrating in the component's `ngOnInit` or an `effect()` after render blanks the form then fills it.
  - Fix: `onInit` runs at store construction, before the route component's template — the same "before first read" guarantee as the metaReducer.
- The persist `effect()` wraps its write in `untracked()` and `try/catch`.
  - Risk: reading state inside the effect without `untracked` around the write is fine, but a throwing `setItem` inside an effect surfaces as an uncaught error every keystroke.
  - Fix: `untracked(() => { try { storage.setItem(...) } catch {} })`.

## SHOULD
- **Reaching for `withPersistedDraft` to cache a loaded list across navigation** — Consequence: it is a draft mechanism, not a data cache; persisted server data goes stale and there is no invalidation — Instead: let the store re-fetch on `load()`; persist only what the user typed

# Check list

- [ ] `withPersistedDraft` calls `assertPersistable(config)`
- [ ] The allow-list contains only editable draft fields
- [ ] Rehydration is in `withHooks({ onInit })`, not a component hook
- [ ] The write is inside `untracked()` + `try/catch`

# Unittest TestCases

- [ ] WHEN a store composing `withPersistedDraft({ keys: ['product'] })` is created and storage holds `{"product":"Widget"}` THEN
  - [ ] `store.product()` is `'Widget'` immediately after construction
- [ ] WHEN `patchState(store, { product: 'Gadget' })` runs THEN
  - [ ] `storage.getItem` reflects `"product":"Gadget"` after the effect flushes
- [ ] WHEN `withPersistedDraft({ keys: ['accessToken'] })` is constructed THEN
  - [ ] it throws
