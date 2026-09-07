import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Store } from '@ngrx/store';
import { provideGlobalStore } from './store.config';
import { selectIsOnline } from './connectivity/connectivity.reducer';

describe('provideGlobalStore', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideGlobalStore()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.match(() => true).forEach((r) => r.flush(null)));

  it('wires the root store with the connectivity + notifications slices registered', () => {
    const store = TestBed.inject(Store);
    let snapshot: Record<string, unknown> = {};
    store.subscribe((s) => (snapshot = s as Record<string, unknown>)).unsubscribe();
    expect(Object.keys(snapshot).sort()).toEqual(['connectivity', 'notifications']);
    expect(snapshot['connectivity']).toEqual({ browserOnline: true, lastHealthCheckOk: true });
    expect(snapshot['notifications']).toEqual({ items: [] });
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
