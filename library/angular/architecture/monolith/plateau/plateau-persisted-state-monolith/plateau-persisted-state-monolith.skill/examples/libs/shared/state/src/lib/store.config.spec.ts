import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Store } from '@ngrx/store';
import { provideGlobalStore } from './store.config';
import { selectIsOnline } from './connectivity/connectivity.reducer';

describe('provideGlobalStore', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideGlobalStore()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.match(() => true).forEach((r) => r.flush(null));
    localStorage.clear();
  });

  it('wires the root store with the connectivity + notifications + auth + preferences slices registered', () => {
    const store = TestBed.inject(Store);
    let snapshot: Record<string, unknown> = {};
    store.subscribe((s) => (snapshot = s as Record<string, unknown>)).unsubscribe();
    expect(Object.keys(snapshot).sort()).toEqual([
      'auth',
      'connectivity',
      'notifications',
      'preferences',
    ]);
    expect(snapshot['connectivity']).toEqual({ browserOnline: true, lastHealthCheckOk: true });
    expect(snapshot['notifications']).toEqual({ items: [] });
    expect(snapshot['auth']).toMatchObject({ currentUser: null, accessToken: null, permissions: [] });
    expect(snapshot['preferences']).toEqual({
      theme: 'system',
      density: 'comfortable',
      lastFeatureTab: null,
    });
  });

  it('exposes selectIsOnline = true from the initial state', () => {
    const store = TestBed.inject(Store);
    let online: boolean | undefined;
    store
      .select(selectIsOnline)
      .subscribe((v) => (online = v))
      .unsubscribe();
    expect(online).toBe(true);
  });

  it('starts the health-check effect (a HEAD /health is issued)', async () => {
    TestBed.inject(Store);
    await new Promise((r) => setTimeout(r, 5)); // let timer(0, …) fire
    const req = http.expectOne('/health');
    expect(req.request.method).toBe('HEAD');
    req.flush(null);
  });
});

describe('provideGlobalStore — preferences persistence (VP8)', () => {
  beforeEach(() => localStorage.clear());
  afterEach(() => localStorage.clear());

  it('rehydrates the preferences slice from localStorage on store init', () => {
    localStorage.setItem('app:preferences', JSON.stringify({ theme: 'dark', density: 'compact' }));
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideGlobalStore()],
    });
    const store = TestBed.inject(Store);
    let pref: { theme: string; density: string } | undefined;
    store
      .select((s: Record<string, unknown>) => s['preferences'] as { theme: string; density: string })
      .subscribe((p) => (pref = p))
      .unsubscribe();
    expect(pref?.theme).toBe('dark');
    expect(pref?.density).toBe('compact');
  });

  it('never registers a metaReducer on the auth slice — no token is written to storage', () => {
    localStorage.setItem('app:preferences', JSON.stringify({ theme: 'dark' }));
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideGlobalStore()],
    });
    const store = TestBed.inject(Store);
    store.dispatch({ type: '[dbg] noop' });
    // no `app:auth` key is ever created
    const keys = Object.keys(localStorage);
    expect(keys.some((k) => k.includes('auth'))).toBe(false);
  });
});
