import { INIT } from '@ngrx/store';
import { assertPersistable, persistKeys, SENSITIVE_STATE_KEYS } from './persisted-state';

interface DemoState {
  theme: string;
  density: string;
  loading: boolean;
}
const initial: DemoState = { theme: 'system', density: 'comfortable', loading: false };
const baseReducer = (s: DemoState = initial, a: { type: string; theme?: string }): DemoState =>
  a.type === '[Demo] set theme' ? { ...s, theme: a.theme! } : s;

const flushMicrotasks = () => Promise.resolve();

describe('persistKeys', () => {
  beforeEach(() => localStorage.clear());

  it('throws at construction when the allow-list names a sensitive key', () => {
    expect(() => persistKeys<{ accessToken: string }>({ key: 'x', keys: ['accessToken'] })).toThrow(
      /sensitive key/,
    );
  });

  it('assertPersistable covers every SENSITIVE_STATE_KEYS entry', () => {
    for (const k of SENSITIVE_STATE_KEYS) {
      expect(() => assertPersistable({ key: 'x', keys: [k] })).toThrow();
    }
  });

  it('rehydrates allow-listed keys from storage on the init action', () => {
    localStorage.setItem('app:demo', JSON.stringify({ theme: 'dark', density: 'compact' }));
    const reducer = persistKeys<DemoState>({ key: 'app:demo', keys: ['theme', 'density'] })(baseReducer);
    const state = reducer(undefined, { type: INIT });
    expect(state).toEqual({ theme: 'dark', density: 'compact', loading: false });
  });

  it('ignores a stale storage key that is not on the allow-list', () => {
    localStorage.setItem('app:demo', JSON.stringify({ theme: 'dark', gone: 'stale' }));
    const reducer = persistKeys<DemoState>({ key: 'app:demo', keys: ['theme'] })(baseReducer);
    const state = reducer(undefined, { type: INIT }) as Record<string, unknown>;
    expect(state['gone']).toBeUndefined();
    expect(state['theme']).toBe('dark');
  });

  it('falls back to initial state on malformed JSON', () => {
    localStorage.setItem('app:demo', '{not json');
    const reducer = persistKeys<DemoState>({ key: 'app:demo', keys: ['theme'] })(baseReducer);
    expect(reducer(undefined, { type: INIT })).toEqual(initial);
  });

  it('writes only the allow-listed keys back after a change, debounced', async () => {
    const reducer = persistKeys<DemoState>({ key: 'app:demo', keys: ['theme'] })(baseReducer);
    let state = reducer(undefined, { type: INIT });
    state = reducer(state, { type: '[Demo] set theme', theme: 'light' });
    await flushMicrotasks();
    expect(JSON.parse(localStorage.getItem('app:demo')!)).toEqual({ theme: 'light' });
  });

  it('does not throw when storage.setItem fails', async () => {
    const failing: Storage = {
      getItem: () => null,
      setItem: () => {
        throw new Error('quota');
      },
      removeItem: () => undefined,
      clear: () => undefined,
      key: () => null,
      length: 0,
    };
    const reducer = persistKeys<DemoState>({ key: 'k', keys: ['theme'], storage: failing })(baseReducer);
    let state = reducer(undefined, { type: INIT });
    expect(() => {
      state = reducer(state, { type: '[Demo] set theme', theme: 'x' });
    }).not.toThrow();
    await flushMicrotasks();
    expect(state.theme).toBe('x');
  });
});
