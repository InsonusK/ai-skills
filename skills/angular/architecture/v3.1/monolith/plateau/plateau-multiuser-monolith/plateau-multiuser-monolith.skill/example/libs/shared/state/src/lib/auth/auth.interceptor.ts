import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { Store } from '@ngrx/store';
import { catchError, throwError } from 'rxjs';
import { AuthActions } from './auth.actions';
import { selectAccessToken } from './auth.reducer';

/**
 * Attaches the in-memory access token to every outgoing request, and on a 401
 * dispatches a single silent-refresh instead of surfacing the error. The
 * silent-refresh request itself is never intercepted (no bearer, no 401 loop).
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const store = inject(Store);

  if (req.url.includes('/auth/')) {
    return next(req); // login / refresh / logout — cookie-only, never a bearer
  }

  const accessToken = store.selectSignal(selectAccessToken)();
  const authorized = accessToken
    ? req.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } })
    : req;

  return next(authorized).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        store.dispatch(AuthActions.silentRefreshRequested());
      }
      return throwError(() => error);
    }),
  );
};
