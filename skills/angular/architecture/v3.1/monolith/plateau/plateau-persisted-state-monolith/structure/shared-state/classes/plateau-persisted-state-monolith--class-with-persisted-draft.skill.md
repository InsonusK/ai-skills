---
name: plateau-persisted-state-monolith--class-with-persisted-draft
description: withPersistedDraft() in libs/shared/state/src/lib/persistence/ — a signalStoreFeature that rehydrates an allow-listed subset of a feature Signal Store from storage in its onInit hook and persists it on change, reusing the SENSITIVE_STATE_KEYS guard — persisted-state-monolith plateau
domain: skill
type: template
whenToUse: when editing with-persisted-draft.ts, or composing it into a feature's {Feature}DraftStore so an in-progress form survives a reload (VP8)
plateau: persisted-state-monolith
artifact_type: module
version: 20260903190000
tags:
  - skill/template/class
  - plateau/persisted-state-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]]"

> `libs/shared/state/src/lib/persistence/with-persisted-draft.ts`. The feature-tier counterpart of [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/shared-state/classes/plateau-persisted-state-monolith--class-persisted-state.skill.md|persistKeys()]] — same guard, same "before first read" rehydration, for `@ngrx/signals` stores.

# Goal

- Give the feature tier the same explicit, allow-listed, sensitive-key-guarded persistence the global tier gets from `persistKeys()`
- Rehydrate in `withHooks({ onInit })` — before the component's template reads the store

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/FeatureStore/with-persisted-draft.ts.create.md|FeatureStore/with-persisted-draft.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Persist only a genuine *draft* (in-progress input) — never server data, never `loading` / `error` / `submitting` flags
- Rehydrate synchronously in `onInit`; persist via an `effect()` whose write is wrapped in `untracked()` + `try/catch`
- Reuse `assertPersistable()` from `persisted-state.ts` — no second, weaker check

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/adr/rehydration-timing.md|rehydration-timing]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Signal-store persistence feature | `withPersistedDraft` | `withPersistedDraft` | `with-persisted-draft.ts` | `with-persisted-draft.ts` |

# Implementation

```typescript
// Skill: class-with-persisted-draft
// Plateau: persisted-state-monolith
// Version: 20260903190000
import { effect, untracked } from '@angular/core';
import { getState, patchState, withHooks } from '@ngrx/signals';
import { assertPersistable, PersistConfig } from './persisted-state';

export function withPersistedDraft<State extends object>(config: PersistConfig<State>) {
  assertPersistable(config);
  const storage = config.storage ?? localStorage;
  const pick = (s: State): Partial<State> =>
    config.keys.reduce((a, k) => ((a[k] = s[k]), a), {} as Partial<State>);

  return withHooks({
    onInit(store: unknown) {
      const target = store as Parameters<typeof getState>[0];
      try {
        const raw = storage.getItem(config.key);
        if (raw) {
          const parsed = JSON.parse(raw) as Partial<State>;
          const restore: Partial<State> = {};
          for (const k of config.keys) if (k in (parsed as object)) restore[k] = parsed[k] as State[typeof k];
          patchState(target as Parameters<typeof patchState>[0], restore);
        }
      } catch { /* stale — start from initial state */ }

      effect(() => {
        const snapshot = pick(getState(target) as State);
        untracked(() => {
          try { storage.setItem(config.key, JSON.stringify(snapshot)); } catch { /* full / unavailable */ }
        });
      });
    },
  });
}
```

Compose it in a feature's draft store:

```typescript
export const OrderDraftStore = signalStore(
  { providedIn: 'root' },
  withState<OrderDraftState>({ product: '', quantity: null }),
  withPersistedDraft<OrderDraftState>({ key: 'app:orders:draft', keys: ['product', 'quantity'] }),
  withMethods(/* patch / clear */),
);
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/FeatureStore/with-persisted-draft.ts.create.md|FeatureStore/with-persisted-draft.ts.create]]

# Rules

## MUST
- `withPersistedDraft` calls `assertPersistable()` — the same `SENSITIVE_STATE_KEYS` guard the global tier uses.
- Only genuine draft input is on the allow-list — never server-fetched entities, never `loading` / `error` / `submitting`.
- Rehydration is in `withHooks({ onInit })`, synchronous — never a component `ngOnInit` or a post-render `effect()`.
- The persist `effect()` wraps its write in `untracked()` + `try/catch`.
- Never apply several plateau templates per class/artifact.
- Never use `withPersistedDraft` to cache a loaded list across navigation — it is a draft mechanism with no invalidation.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/FeatureStore/with-persisted-draft.ts.create.md|FeatureStore/with-persisted-draft.ts.create]]

# Check list

- [ ] `withPersistedDraft` calls `assertPersistable(config)`
- [ ] The allow-list contains only editable draft fields
- [ ] Rehydration is in `withHooks({ onInit })`
- [ ] The write is inside `untracked()` + `try/catch`

# Unittest TestCases

- [ ] WHEN a store composing `withPersistedDraft({ keys: ['product'] })` is created and storage holds `{"product":"Widget"}` THEN `store.product()` is `'Widget'` immediately
- [ ] WHEN `patchState(store, { product: 'Gadget' })` runs and the effect flushes THEN `getItem` reflects `"product":"Gadget"`
- [ ] WHEN `withPersistedDraft({ keys: ['accessToken'] })` is constructed THEN it throws

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/FeatureStore/with-persisted-draft.ts.create.md|FeatureStore/with-persisted-draft.ts.create]]
