---
description: New connectivity slice in libs/shared/state — combines navigator.onLine events with a periodic health-check for an accurate isOnline signal
project_name: shared-state
name: connectivity
element_kind: store
change_kind: create
tags:
  - solution/offline-first
  - element/connectivity-store-ts
---

# Goals

- Give the whole application a single, accurate `isOnline` signal, more trustworthy than `navigator.onLine` alone

# Naming convention

Follows the same convention as the existing `auth` slice: `ConnectivityActions`, `connectivityReducer`, `ConnectivityEffects`, `selectIsOnline`.

# Implementation changes

```typescript
// connectivity.actions.ts
export const ConnectivityActions = createActionGroup({
  source: 'Connectivity',
  events: {
    'Browser Reported Online': emptyProps(),
    'Browser Reported Offline': emptyProps(),
    'Health Check Succeeded': emptyProps(),
    'Health Check Failed': emptyProps(),
  },
});
```

```typescript
// connectivity.effects.ts
export class ConnectivityEffects {
  browserEvents$ = createEffect(() =>
    merge(
      fromEvent(window, 'online').pipe(map(() => ConnectivityActions.browserReportedOnline())),
      fromEvent(window, 'offline').pipe(map(() => ConnectivityActions.browserReportedOffline())),
    ),
  );

  healthCheck$ = createEffect(() =>
    timer(0, HEALTH_CHECK_INTERVAL_MS).pipe(
      switchMap(() =>
        this.http.head('/health').pipe(
          map(() => ConnectivityActions.healthCheckSucceeded()),
          catchError(() => of(ConnectivityActions.healthCheckFailed())),
        ),
      ),
    ),
  );
}
```

```typescript
// connectivity.reducer.ts — isOnline is true only when both signals agree
interface ConnectivityState {
  browserOnline: boolean;
  lastHealthCheckOk: boolean;
}
// selectIsOnline derives: browserOnline && lastHealthCheckOk
```

# Rule changes

## MUST
- `selectIsOnline` requires both the browser online signal and the most recent health-check to agree — either one reporting offline marks the app offline.
  - Risk: `navigator.onLine` alone misreports a captive portal or a backend outage as "online".
  - Fix: `extraSelectors` computes `browserOnline && lastHealthCheckOk`.
- The health-check interval backs off while already known to be offline.
  - Risk: a fixed short interval floods the network with failing requests for the whole duration of an outage.
  - Fix: a longer interval (or exponential-ish backoff) once `isOnline` is false; back to normal cadence once a check succeeds.
- The health-check is a lightweight unauthenticated `HEAD` request — never through `authInterceptor`, never requiring a session.
  - Risk: routing it through the interceptor makes connectivity un-checkable for a logged-out user and can trigger a spurious silent-refresh on its 401.
  - Fix: a bare `fetch('/health', { method: 'HEAD' })` (or an `HttpClient` call on a context that skips the interceptor).

## SHOULD
- **Relying on `browserOnline` alone without factoring in the health-check result** — Consequence: the application can report itself as online while the backend is actually unreachable (captive portal, backend outage), per [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/adr/connectivity-detection.md|connectivity-detection]] — Instead: `isOnline` always requires both signals to agree

# Check list

- [ ] `isOnline` is false whenever either the browser reports offline or the last health check failed
- [ ] The health-check interval backs off while offline
- [ ] The health-check request bypasses `authInterceptor`

# Unittest TestCases

- [ ] WHEN the browser fires an `offline` event THEN
  - [ ] `selectIsOnline` becomes `false` immediately, without waiting for the next health check
- [ ] WHEN the browser reports online but the health check fails THEN
  - [ ] `selectIsOnline` remains `false`
- [ ] WHEN both the browser reports online and the health check succeeds THEN
  - [ ] `selectIsOnline` becomes `true`
