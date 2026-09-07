import { effect, untracked } from '@angular/core';
import { getState, patchState, withHooks } from '@ngrx/signals';
import { assertPersistable, PersistConfig } from './persisted-state';

/**
 * Signal-store feature: rehydrate an allow-listed subset of the store from browser
 * storage in the onInit hook (before the first template read), then persist those
 * keys on every change. Reuses the SENSITIVE_STATE_KEYS guard.
 *
 * Use only for a genuine draft (in-progress input) — never server data or flags.
 */
export function withPersistedDraft<State extends object>(config: PersistConfig<State>) {
  assertPersistable(config);
  const storage = config.storage ?? localStorage;

  const pickKeys = (s: State): Partial<State> => {
    const out: Partial<State> = {};
    for (const k of config.keys) out[k] = s[k];
    return out;
  };

  return withHooks({
    onInit(store: unknown) {
      const target = store as Parameters<typeof getState>[0];

      // 1. rehydrate synchronously, before the first template read
      try {
        const raw = storage.getItem(config.key);
        if (raw) {
          const parsed = JSON.parse(raw) as Partial<State>;
          const restore: Partial<State> = {};
          for (const k of config.keys) {
            if (k in (parsed as object)) restore[k] = parsed[k] as State[typeof k];
          }
          patchState(target as Parameters<typeof patchState>[0], restore);
        }
      } catch {
        /* stale or malformed — start from the store's own initial state */
      }

      // 2. persist the allow-listed keys on every change
      effect(() => {
        const snapshot = pickKeys(getState(target) as State);
        untracked(() => {
          try {
            storage.setItem(config.key, JSON.stringify(snapshot));
          } catch {
            /* storage full/unavailable — a convenience, never fatal */
          }
        });
      });
    },
  });
}
