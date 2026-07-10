---
description: New connectivity slice in libs/shared/state — combines navigator.onLine events with a periodic health-check for an accurate isOnline signal
project_name: shared-state
name: connectivity
element_kind: store
change_kind: create
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
- `selectIsOnline` MUST require both the browser's own online signal and the most recent health-check result to agree — either one reporting offline is enough to mark the app as offline.
- The health-check interval MUST back off (check less frequently) while already known to be offline, to avoid flooding the network with pointless requests during an outage.
- The health-check endpoint MUST be a lightweight, unauthenticated `HEAD` request — it MUST NOT go through `authInterceptor` or require a valid session, since connectivity should be checkable even for a logged-out user.

# Anti-patterns

- **Relying on `browserOnline` alone without factoring in the health-check result**
  - Consequence: the application can report itself as online while the backend is actually unreachable (captive portal, backend outage), per [[../..[[skills/angular/architecture/solutions/solution-offline-first.skill/adr/connectivity-detection]]nstead: `isOnline` always requires both signals to agree

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
