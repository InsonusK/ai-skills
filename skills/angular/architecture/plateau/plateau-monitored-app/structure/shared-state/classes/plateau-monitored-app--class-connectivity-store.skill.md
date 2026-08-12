---
name: plateau-monitored-app--class-connectivity-store
description: Connectivity slice inside libs/shared/state — combines navigator.onLine events with a periodic health-check for an accurate isOnline signal — monitored-app plateau
domain: skill
type: template
plateau: monitored-app
artifact_type: store
version: 20260711220000
tags:
  - skill/template/class
  - plateau/monitored-app
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"
---

# Goal

- Give the whole application a single, accurate `isOnline` signal, more trustworthy than `navigator.onLine` alone

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/connectivity.store.ts.create|GlobalStore/connectivity.store.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- `isOnline` requires both the browser's own signal and the most recent health check to agree

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/connectivity.store.ts.create|GlobalStore/connectivity.store.ts.create]]

# Naming convention

Follows the same convention as the `auth` slice:

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------------- | -------------------- | --------- |
| Action group | `{Slice}Actions` | `ConnectivityActions` | `{slice}.actions.ts` | `connectivity.actions.ts` |
| Reducer | `{slice}Reducer` | `connectivityReducer` | `{slice}.reducer.ts` | `connectivity.reducer.ts` |
| Effects class | `{Slice}Effects` | `ConnectivityEffects` | `{slice}.effects.ts` | `connectivity.effects.ts` |
| Selectors | `select{Slice}*` | `selectIsOnline` | `{slice}.selectors.ts` | `connectivity.selectors.ts` |

# Implementation

```typescript
// Skill: class-connectivity-store
// Plateau: monitored-app
// Version: 20260711220000

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

// connectivity.effects.ts
export class ConnectivityEffects {
  browserEvents$ = createEffect(() =>
    merge(
      fromEvent(window, 'online').pipe(map(() => ConnectivityActions.browserReportedOnline())),
      fromEvent(window, 'offline').pipe(map(() => ConnectivityActions.browserReportedOffline())),
    ),
  );

  healthCheck$ = createEffect(() =>
    timer(0, HEALTH_CHECK_INTERVAL_MS).pipe( // interval backs off while offline
      switchMap(() =>
        this.http.head('/health').pipe( // unauthenticated, bypasses authInterceptor
          map(() => ConnectivityActions.healthCheckSucceeded()),
          catchError(() => of(ConnectivityActions.healthCheckFailed())),
        ),
      ),
    ),
  );
}

// connectivity.reducer.ts — isOnline is true only when both signals agree
interface ConnectivityState {
  browserOnline: boolean;
  lastHealthCheckOk: boolean;
}
// selectIsOnline derives: browserOnline && lastHealthCheckOk
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/connectivity.store.ts.create|GlobalStore/connectivity.store.ts.create]]

# Rules

## MUST
- `selectIsOnline` MUST require both the browser's own online signal and the most recent health-check result to agree.
- The health-check interval MUST back off while already known to be offline.
- The health-check endpoint MUST be a lightweight, unauthenticated `HEAD` request — it MUST NOT go through `authInterceptor`.
- `selectIsOnline` MUST be the only public selector consumed by feature code; `navigator.onLine` MUST NOT be read directly by feature code.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/connectivity.store.ts.create|GlobalStore/connectivity.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/shared-state.project.extend|GlobalStore/shared-state.project.extend]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Relying on `browserOnline` alone without factoring in the health-check result**
  - Consequence: the application can report itself as online while the backend is actually unreachable (captive portal, backend outage)
  - Instead: `isOnline` always requires both signals to agree
- **Duplicating the connectivity logic inside a feature store**
  - Consequence: multiple sources of truth for online status
  - Instead: rely on `libs/shared/state`'s `connectivity` slice

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/connectivity.store.ts.create|GlobalStore/connectivity.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/shared-state.project.extend|GlobalStore/shared-state.project.extend]]

# Check list

- [ ] `isOnline` is false whenever either the browser reports offline or the last health check failed
- [ ] The health-check interval backs off while offline
- [ ] The health-check request bypasses `authInterceptor`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/connectivity.store.ts.create|GlobalStore/connectivity.store.ts.create]]

# Unittest TestCases

- [ ] WHEN the browser fires an `offline` event THEN
  - [ ] `selectIsOnline` becomes `false` immediately, without waiting for the next health check
- [ ] WHEN the browser reports online but the health check fails THEN
  - [ ] `selectIsOnline` remains `false`
- [ ] WHEN both the browser reports online and the health check succeeds THEN
  - [ ] `selectIsOnline` becomes `true`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/connectivity.store.ts.create|GlobalStore/connectivity.store.ts.create]]
