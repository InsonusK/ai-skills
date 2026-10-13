import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { AuthActions } from './auth.actions';
import { selectAccessToken } from './auth.reducer';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpCtrl: HttpTestingController;
  let store: MockStore;
  let dispatch: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        provideMockStore({ selectors: [{ selector: selectAccessToken, value: 'tok-123' }] }),
      ],
    });
    http = TestBed.inject(HttpClient);
    httpCtrl = TestBed.inject(HttpTestingController);
    store = TestBed.inject(MockStore);
    dispatch = vi.fn();
    store.dispatch = dispatch;
  });
  afterEach(() => httpCtrl.verify());

  it('attaches the in-memory access token as a bearer header', () => {
    http.get('/api/orders').subscribe();
    const req = httpCtrl.expectOne('/api/orders');
    expect(req.request.headers.get('Authorization')).toBe('Bearer tok-123');
    req.flush([]);
  });

  it('does not attach a bearer to the silent-refresh request itself', () => {
    http.post('/api/auth/refresh', {}).subscribe();
    const req = httpCtrl.expectOne('/api/auth/refresh');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('dispatches Silent Refresh Requested exactly once on a 401', () => {
    http.get('/api/orders').subscribe({ error: () => undefined });
    httpCtrl.expectOne('/api/orders').flush('nope', { status: 401, statusText: 'Unauthorized' });
    expect(dispatch).toHaveBeenCalledWith(AuthActions.silentRefreshRequested());
    expect(dispatch).toHaveBeenCalledTimes(1);
  });
});
