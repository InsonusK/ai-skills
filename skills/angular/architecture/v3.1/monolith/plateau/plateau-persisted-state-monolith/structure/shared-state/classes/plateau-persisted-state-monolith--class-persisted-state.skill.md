---
name: plateau-persisted-state-monolith--class-persisted-state
description: The persistKeys() metaReducer factory in libs/shared/state/src/lib/persistence/ — rehydrates and persists an allow-listed subset of a classical-NgRx slice, with the SENSITIVE_STATE_KEYS registry and the assertPersistable() guard — persisted-state-monolith plateau
domain: skill
type: template
whenToUse: when editing persisted-state.ts, adding a persisted slice, or reviewing the SENSITIVE_STATE_KEYS guard / rehydration timing (VP8)
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

> `libs/shared/state/src/lib/persistence/persisted-state.ts`. Mechanism only — the `key` / `keys` config for any slice lives at the `store.config.ts` call site, never here.

# Goal

- Give a slice a one-line, explicit opt-in to session persistence that rehydrates synchronously (no flash) and writes back debounced
- Make it structurally impossible to persist a key on `SENSITIVE_STATE_KEYS` — `assertPersistable()` throws at construction

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/persisted-state.ts.create.md|GlobalStore/persisted-state.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Rehydrate synchronously on the store-init action, only when the slice state is still `undefined`; every other action reduces then writes back (debounced to a microtask)
- Both the storage read (JSON parse) and the write are wrapped in `try/catch` and never throw — a persisted convenience degrades silently
- `SENSITIVE_STATE_KEYS` is the single registry of never-persist keys; a solution that adds a sensitive field adds its key here

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/adr/rehydration-timing.md|rehydration-timing]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Persistence metaReducer factory | `persistKeys` | `persistKeys` | `persisted-state.ts` | `persisted-state.ts` |
| Sensitive-key registry | `SENSITIVE_STATE_KEYS` | `SENSITIVE_STATE_KEYS` | `persisted-state.ts` | `persisted-state.ts` |
| Guard | `assertPersistable` | `assertPersistable` | `persisted-state.ts` | `persisted-state.ts` |

# Implementation

```typescript
// Skill: class-persisted-state
// Plateau: persisted-state-monolith
// Version: 20260903190000
import { Action, INIT, MetaReducer, UPDATE } from '@ngrx/store';

export const SENSITIVE_STATE_KEYS: readonly string[] = ['accessToken', 'refreshToken'];

export interface PersistConfig<T> {
  readonly key: string;
  readonly keys: readonly (keyof T & string)[];
  readonly storage?: Storage; // defaults to localStorage
}

export function assertPersistable<T>(config: PersistConfig<T>): void {
  const bad = config.keys.filter((k) => SENSITIVE_STATE_KEYS.includes(k));
  if (bad.length) throw new Error(`persistKeys("${config.key}"): refusing to persist: ${bad.join(', ')}`);
}

export function persistKeys<T>(config: PersistConfig<T>): MetaReducer<T> {
  assertPersistable(config);
  const storage = config.storage ?? localStorage;
  return (reducer) => {
    let scheduled = false;
    return (state: T | undefined, action: Action): T => {
      if ((action.type === INIT || action.type === UPDATE) && state === undefined) {
        const base = reducer(state, action);
        try {
          const parsed = JSON.parse(storage.getItem(config.key) ?? '{}') as Partial<T>;
          const present = config.keys.filter((k) => k in (parsed as object));
          return { ...base, ...present.reduce((a, k) => ((a[k] = (parsed as T)[k]), a), {} as Partial<T>) };
        } catch {
          return base;
        }
      }
      const next = reducer(state, action);
      if (next !== state && !scheduled) {
        scheduled = true;
        queueMicrotask(() => {
          scheduled = false;
          try {
            storage.setItem(config.key, JSON.stringify(config.keys.reduce((a, k) => ((a[k] = next[k]), a), {} as Partial<T>)));
          } catch { /* full / unavailable */ }
        });
      }
      return next;
    };
  };
}
```

Wire it (three-arg `provideState` — the only overload that applies `metaReducers`):

```typescript
provideState(preferencesFeature.name, preferencesFeature.reducer, {
  metaReducers: [persistKeys<PreferencesState>({ key: 'app:preferences', keys: ['theme', 'density', 'lastFeatureTab'] })],
})
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/persisted-state.ts.create.md|GlobalStore/persisted-state.ts.create]]

# Rules

## MUST
- `persistKeys()` calls `assertPersistable()` before returning — a sensitive key on the allow-list throws at construction, failing the app on boot and the suite in CI.
- Rehydration fires only on `INIT` / `UPDATE` with `state === undefined`; every other action just reduces + writes back.
- Both the `getItem` parse and the `setItem` write are inside `try/catch` and never throw.
- The read path intersects the parsed object with the current `keys` allow-list before merging — it tolerates a stale / partial / malformed shape.
- Writes are coalesced to one `setItem` per microtask.
- Never apply several plateau templates per class/artifact.
- Never put a slice's `key` / `keys` values inside this file — they belong at the `store.config.ts` call site.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/persisted-state.ts.create.md|GlobalStore/persisted-state.ts.create]]

# Check list

- [ ] `persistKeys()` throws when `keys` contains any `SENSITIVE_STATE_KEYS` entry
- [ ] Rehydration only on `INIT` / `UPDATE` with `state === undefined`
- [ ] Both parse and write are `try/catch`; writes debounced to a microtask
- [ ] The file exports mechanism only, no slice config

# Unittest TestCases

- [ ] WHEN `persistKeys({ keys: ['accessToken'] })` is constructed THEN it throws naming the key
- [ ] WHEN storage holds `{"theme":"dark"}` and the init action fires THEN the returned initial state has `theme: 'dark'` and other keys at defaults
- [ ] WHEN storage holds a key not on the allow-list THEN it is ignored
- [ ] WHEN storage is malformed JSON THEN the reducer returns its own initial state
- [ ] WHEN a non-init action changes state THEN after a microtask `getItem(key)` is the JSON of exactly the allow-listed keys
- [ ] WHEN `setItem` throws THEN the dispatch still completes and state updates

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/persisted-state.ts.create.md|GlobalStore/persisted-state.ts.create]]
