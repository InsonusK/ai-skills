import { Action, INIT, MetaReducer, UPDATE } from '@ngrx/store';

/**
 * State keys that must never be written to browser storage, regardless of which
 * slice they belong to. `solution-authentication` owns the token entries.
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

const pick = <T>(state: T, keys: readonly (keyof T)[]): Partial<T> => {
  const out: Partial<T> = {};
  for (const k of keys) out[k] = state[k];
  return out;
};

export function persistKeys<T>(config: PersistConfig<T>): MetaReducer<T> {
  assertPersistable(config);
  const storage = config.storage ?? localStorage;

  return (reducer) => {
    let writeScheduled = false;

    return (state: T | undefined, action: Action): T => {
      // 1. rehydrate synchronously on store init — merge before any selector emits
      if ((action.type === INIT || action.type === UPDATE) && state === undefined) {
        const base = reducer(state, action);
        let persisted: Partial<T> = {};
        try {
          const raw = storage.getItem(config.key);
          const parsed = raw ? (JSON.parse(raw) as Partial<T>) : {};
          const present = config.keys.filter((k) => k in (parsed as object));
          persisted = pick(parsed as T, present);
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
            storage.setItem(config.key, JSON.stringify(pick(next, config.keys)));
          } catch {
            /* storage full or unavailable — a persisted convenience, never fatal */
          }
        });
      }
      return next;
    };
  };
}
