---
description: The persistKeys() metaReducer factory that rehydrates and persists an allow-listed subset of a classical-NgRx slice, plus the SENSITIVE_STATE_KEYS registry and the assertPersistable() guard
project_name: shared-state
name: persisted-state
element_kind: module
change_kind: create
tags:
  - solution/persisted-state
  - element/persisted-state-ts
---

# How this generic file is used

Create once at `libs/shared/state/src/lib/persistence/persisted-state.ts`. Every persisted classical-NgRx slice imports `persistKeys` from here; no slice writes its own storage code.

# Goals

- Give a slice a one-line, explicit opt-in to session persistence that rehydrates synchronously (no flash) and writes back debounced
- Make "which keys are persisted" a finite array at the `provideState` call site, never `*`
- Make it structurally impossible to persist a key on the `SENSITIVE_STATE_KEYS` registry — the guard throws at construction

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Persistence metaReducer factory | `persistKeys` | `persistKeys` | `persisted-state.ts` | `persisted-state.ts` |
| Sensitive-key registry | `SENSITIVE_STATE_KEYS` | `SENSITIVE_STATE_KEYS` | `persisted-state.ts` | `persisted-state.ts` |

# Implementation changes

File: `libs/shared/state/src/lib/persistence/persisted-state.ts`

```typescript
import { MetaReducer, Action } from '@ngrx/store';
import { INIT, UPDATE } from '@ngrx/store';

/**
 * State keys that must never be written to browser storage, regardless of which
 * slice they belong to. `solution-authentication` owns the token entries; add to
 * this list from the solution that introduces the sensitive field.
 */
export const SENSITIVE_STATE_KEYS: readonly string[] = ['accessToken', 'refreshToken'];

export interface PersistConfig<T> {
  /** storage key — namespaced by the app, e.g. `app:preferences` */
  readonly key: string;
  /** the explicit allow-list — the only state keys read back and written */
  readonly keys: readonly (keyof T & string)[];
  /** defaults to localStorage; pass sessionStorage for per-tab state */
  readonly storage?: Storage;
}

/** Throws if the allow-list names a key the app must never persist. */
export function assertPersistable<T>(config: PersistConfig<T>): void {
  const offending = config.keys.filter((k) => SENSITIVE_STATE_KEYS.includes(k));
  if (offending.length) {
    throw new Error(
      `persistKeys("${config.key}"): refusing to persist sensitive key(s): ${offending.join(', ')}`,
    );
  }
}

const pick = <T>(state: T, keys: readonly (keyof T)[]): Partial<T> =>
  keys.reduce((acc, k) => ((acc[k] = state[k]), acc), {} as Partial<T>);

export function persistKeys<T>(config: PersistConfig<T>): MetaReducer<T> {
  assertPersistable(config);
  const storage = config.storage ?? localStorage;

  return (reducer) => {
    let writeScheduled = false;

    return (state, action: Action) => {
      // 1. rehydrate synchronously on store init — merge before any selector emits
      if ((action.type === INIT || action.type === UPDATE) && state === undefined) {
        const base = reducer(state, action);
        let persisted: Partial<T> = {};
        try {
          const raw = storage.getItem(config.key);
          const parsed = raw ? (JSON.parse(raw) as Partial<T>) : {};
          // intersect with the current allow-list — tolerate a stale/partial shape
          persisted = pick(parsed as T, config.keys.filter((k) => k in parsed));
        } catch {
          persisted = {};
        }
        return { ...base, ...persisted };
      }

      // 2. reduce, then persist the allow-listed slice (debounced to a microtask)
      const next = reducer(state, action);
      if (next !== state && !writeScheduled) {
        writeScheduled = true;
        queueMicrotask(() => {
          writeScheduled = false;
          try {
            storage.setItem(config.key, JSON.stringify(pick(next as T, config.keys)));
          } catch {
            /* storage full or unavailable — a persisted convenience, never fatal */
          }
        });
      }
      return next;
    };
  };
}
```

Wire it at the slice's `provideState` call in `store.config.ts` (see [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/shared-state.project.extend.md|shared-state.project.extend]]):

```typescript
provideState(preferencesFeature, {
  metaReducers: [persistKeys<PreferencesState>({ key: 'app:preferences', keys: ['theme', 'density', 'lastFeatureTab'] })],
}),
```

# Rule changes

## MUST
- `persistKeys()` calls `assertPersistable()` before returning, so a sensitive key on the allow-list throws at construction.
  - Risk: a deferred check (a lint rule, a runtime warning) can be silenced or missed; a token key then reaches storage.
  - Fix: the guard runs synchronously in the factory; a bad allow-list crashes the app on boot and the test suite in CI.
- Rehydration happens only on the store-init action and only when the slice's state is still `undefined`.
  - Risk: merging storage into state on a later action would clobber a runtime change the user just made with a stale persisted value.
  - Fix: gate the merge on `action.type === INIT || UPDATE` **and** `state === undefined`; every other action just reduces + writes back.
- The write path is wrapped in `try/catch` and never throws.
  - Risk: `localStorage.setItem` throws when storage is full or disabled (private mode); an unhandled throw in a reducer breaks every dispatch.
  - Fix: swallow the write error — a persisted convenience degrades silently, it does not take the app down.
- The read path tolerates a malformed, stale, or partial JSON string.
  - Risk: a previous app version wrote a different shape; a hard parse or a blind spread corrupts the initial state.
  - Fix: `try/catch` the parse, then intersect the parsed object with the current `keys` allow-list before merging.
- `SENSITIVE_STATE_KEYS` is the single registry of never-persist keys; a solution that adds a sensitive field adds its key here.
  - Risk: scattered "don't persist this" comments are not enforceable.
  - Fix: one exported `readonly string[]`; `assertPersistable()` is the only consumer.

# Check list

- [ ] `persistKeys()` throws when `keys` contains any `SENSITIVE_STATE_KEYS` entry
- [ ] Rehydration only fires on `INIT`/`UPDATE` with `state === undefined`
- [ ] Both `getItem` parse and `setItem` write are inside `try/catch`
- [ ] Writes are coalesced to one `setItem` per microtask

# Unittest TestCases

- [ ] WHEN `persistKeys({ keys: ['accessToken'] })` is constructed THEN
  - [ ] it throws with a message naming the offending key
- [ ] WHEN a reducer wrapped by `persistKeys` receives the init action and storage holds `{"theme":"dark"}` THEN
  - [ ] the returned initial state has `theme: 'dark'` and the other keys at their defaults
- [ ] WHEN storage holds a stale key not on the allow-list THEN
  - [ ] that key is ignored and does not appear in the rehydrated state
- [ ] WHEN a non-init action changes the state THEN
  - [ ] after a microtask, `storage.getItem(key)` is the JSON of exactly the allow-listed keys
- [ ] WHEN `storage.setItem` throws THEN
  - [ ] the dispatch still completes and state is updated
