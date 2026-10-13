import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { createEffect } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { catchError, exhaustMap, fromEvent, map, merge, of, switchMap, timer } from 'rxjs';
import { ConnectivityActions } from './connectivity.actions';
import { selectIsOnline } from './connectivity.reducer';

/** health-check cadence while the app believes it is online */
export const HEALTH_CHECK_INTERVAL_MS = 30_000;
/** backed-off cadence while the app already knows it is offline */
export const OFFLINE_HEALTH_CHECK_INTERVAL_MS = 120_000;
/** lightweight, unauthenticated endpoint — must not go through authInterceptor */
export const HEALTH_CHECK_URL = '/health';

@Injectable()
export class ConnectivityEffects {
  private readonly http = inject(HttpClient);
  private readonly store = inject(Store);

  /** Translate the browser's own online/offline events into actions. */
  readonly browserEvents$ = createEffect(() =>
    merge(
      fromEvent(window, 'online').pipe(map(() => ConnectivityActions.browserReportedOnline())),
      fromEvent(window, 'offline').pipe(map(() => ConnectivityActions.browserReportedOffline())),
    ),
  );

  /**
   * Poll a lightweight HEAD /health. The interval backs off once the app is
   * already known offline, so an outage does not flood the network.
   */
  readonly healthCheck$ = createEffect(() =>
    this.store.select(selectIsOnline).pipe(
      switchMap((online) =>
        timer(0, online ? HEALTH_CHECK_INTERVAL_MS : OFFLINE_HEALTH_CHECK_INTERVAL_MS),
      ),
      exhaustMap(() =>
        this.http.head(HEALTH_CHECK_URL).pipe(
          map(() => ConnectivityActions.healthCheckSucceeded()),
          catchError(() => of(ConnectivityActions.healthCheckFailed())),
        ),
      ),
    ),
  );
}
