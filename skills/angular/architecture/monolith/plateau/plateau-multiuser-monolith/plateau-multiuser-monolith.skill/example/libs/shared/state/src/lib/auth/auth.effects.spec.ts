import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { firstValueFrom, of, Subject, throwError } from 'rxjs';
import { AuthActions } from './auth.actions';
import { AuthEffects } from './auth.effects';
import { AuthFacade } from './auth.facade';

describe('AuthEffects', () => {
  const facade = { login: vi.fn(), silentRefresh: vi.fn(), logout: vi.fn() };
  let actions$: Subject<unknown>;

  beforeEach(() => {
    actions$ = new Subject();
    Object.values(facade).forEach((f) => f.mockReset());
    TestBed.configureTestingModule({
      providers: [AuthEffects, provideMockActions(() => actions$), { provide: AuthFacade, useValue: facade }],
    });
  });

  it('loginRequested → loginSucceeded on a successful facade call', async () => {
    facade.login.mockReturnValue(of({ user: { id: 'u1', name: 'Ada' }, accessToken: 't', permissions: ['x'] }));
    const effects = TestBed.inject(AuthEffects);
    const result = firstValueFrom(effects.login$);
    actions$.next(AuthActions.loginRequested({ credentials: { email: 'a@b.c', password: 'p' } }));
    expect(await result).toEqual(AuthActions.loginSucceeded({ user: { id: 'u1', name: 'Ada' }, accessToken: 't', permissions: ['x'] }));
  });

  it('loginRequested → loginFailed on a rejected facade call', async () => {
    facade.login.mockReturnValue(throwError(() => new Error('bad credentials')));
    const effects = TestBed.inject(AuthEffects);
    const result = firstValueFrom(effects.login$);
    actions$.next(AuthActions.loginRequested({ credentials: { email: 'a@b.c', password: 'p' } }));
    expect(await result).toEqual(AuthActions.loginFailed({ error: 'bad credentials' }));
  });

  it('silentRefreshRequested → silentRefreshSucceeded when the refresh cookie is valid', async () => {
    facade.silentRefresh.mockReturnValue(of({ accessToken: 'fresh', permissions: ['orders.view'] }));
    const effects = TestBed.inject(AuthEffects);
    const result = firstValueFrom(effects.silentRefresh$);
    actions$.next(AuthActions.silentRefreshRequested());
    expect(await result).toEqual(AuthActions.silentRefreshSucceeded({ accessToken: 'fresh', permissions: ['orders.view'] }));
  });

  it('silentRefreshRequested → silentRefreshFailed when there is no valid cookie', async () => {
    facade.silentRefresh.mockReturnValue(throwError(() => new Error('401')));
    const effects = TestBed.inject(AuthEffects);
    const result = firstValueFrom(effects.silentRefresh$);
    actions$.next(AuthActions.silentRefreshRequested());
    expect(await result).toEqual(AuthActions.silentRefreshFailed());
  });
});
