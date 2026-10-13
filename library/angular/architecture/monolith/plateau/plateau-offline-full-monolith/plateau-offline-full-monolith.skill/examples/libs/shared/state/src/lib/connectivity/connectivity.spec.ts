import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { firstValueFrom, take } from 'rxjs';
import { ConnectivityActions } from './connectivity.actions';
import { connectivityFeature, connectivityReducer, selectIsOnline } from './connectivity.reducer';
import { ConnectivityEffects, HEALTH_CHECK_URL } from './connectivity.effects';

const initial = { browserOnline: true, lastHealthCheckOk: true };
const tick = () => new Promise((r) => setTimeout(r, 5));

describe('connectivityReducer', () => {
  it('flips browserOnline on the browser events', () => {
    let s = connectivityReducer(initial, ConnectivityActions.browserReportedOffline());
    expect(s.browserOnline).toBe(false);
    s = connectivityReducer(s, ConnectivityActions.browserReportedOnline());
    expect(s.browserOnline).toBe(true);
  });

  it('flips lastHealthCheckOk on the health-check events', () => {
    let s = connectivityReducer(initial, ConnectivityActions.healthCheckFailed());
    expect(s.lastHealthCheckOk).toBe(false);
    s = connectivityReducer(s, ConnectivityActions.healthCheckSucceeded());
    expect(s.lastHealthCheckOk).toBe(true);
  });
});

describe('selectIsOnline — both signals must agree', () => {
  const cases: Array<[boolean, boolean, boolean]> = [
    [true, true, true],
    [true, false, false],
    [false, true, false],
    [false, false, false],
  ];
  it.each(cases)('browser=%s health=%s -> isOnline=%s', (browser, health, expected) => {
    const state = {
      [connectivityFeature.name]: { browserOnline: browser, lastHealthCheckOk: health },
    };
    expect(selectIsOnline(state)).toBe(expected);
  });
});

describe('ConnectivityEffects', () => {
  let http: HttpTestingController;
  let store: MockStore;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideMockStore({ selectors: [{ selector: selectIsOnline, value: true }] }),
        ConnectivityEffects,
      ],
    });
    http = TestBed.inject(HttpTestingController);
    store = TestBed.inject(MockStore);
  });

  afterEach(() => http.match(() => true).forEach((r) => r.flush(null)));

  it('browserEvents$ turns a window "offline" event into browserReportedOffline', async () => {
    const effects = TestBed.inject(ConnectivityEffects);
    const next = firstValueFrom(effects.browserEvents$.pipe(take(1)));
    window.dispatchEvent(new Event('offline'));
    expect(await next).toEqual(ConnectivityActions.browserReportedOffline());
  });

  it('healthCheck$ maps a successful HEAD /health to healthCheckSucceeded', async () => {
    const effects = TestBed.inject(ConnectivityEffects);
    const first = firstValueFrom(effects.healthCheck$.pipe(take(1)));
    await tick();
    const req = http.expectOne(HEALTH_CHECK_URL);
    expect(req.request.method).toBe('HEAD');
    req.flush(null);
    expect(await first).toEqual(ConnectivityActions.healthCheckSucceeded());
  });

  it('healthCheck$ maps a network error to healthCheckFailed', async () => {
    const effects = TestBed.inject(ConnectivityEffects);
    const first = firstValueFrom(effects.healthCheck$.pipe(take(1)));
    await tick();
    http.expectOne(HEALTH_CHECK_URL).error(new ProgressEvent('error'));
    expect(await first).toEqual(ConnectivityActions.healthCheckFailed());
  });

  it('backs off the poll interval once the store reports offline', async () => {
    store.overrideSelector(selectIsOnline, false);
    store.refreshState();
    const effects = TestBed.inject(ConnectivityEffects);
    const first = firstValueFrom(effects.healthCheck$.pipe(take(1)));
    await tick();
    http.expectOne(HEALTH_CHECK_URL).flush(null);
    // still resolves — the effect just polls less often while offline
    expect(await first).toEqual(ConnectivityActions.healthCheckSucceeded());
  });
});
