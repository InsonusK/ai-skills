import { inject, Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of } from 'rxjs';
import { AuthActions } from './auth.actions';
import { AuthFacade } from './auth.facade';

@Injectable()
export class AuthEffects {
  private readonly actions$ = inject(Actions);
  private readonly auth = inject(AuthFacade);

  readonly login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.loginRequested),
      exhaustMap(({ credentials }) =>
        this.auth.login(credentials).pipe(
          map(({ user, accessToken, permissions }) =>
            AuthActions.loginSucceeded({ user, accessToken, permissions }),
          ),
          catchError((e: unknown) => of(AuthActions.loginFailed({ error: (e as Error).message }))),
        ),
      ),
    ),
  );

  readonly silentRefresh$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.silentRefreshRequested),
      exhaustMap(() =>
        this.auth.silentRefresh().pipe(
          map(({ accessToken, permissions }) =>
            AuthActions.silentRefreshSucceeded({ accessToken, permissions }),
          ),
          catchError(() => of(AuthActions.silentRefreshFailed())),
        ),
      ),
    ),
  );

  readonly logout$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.logoutRequested),
      exhaustMap(() => this.auth.logout().pipe(catchError(() => of(void 0)))),
    ),
    { dispatch: false },
  );
}
