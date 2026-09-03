---
name: plateau-offline-full-monolith--class-connectivity-store
description: The connectivity slice in libs/shared/state — a classical NgRx feature deriving selectIsOnline from navigator.onLine events AND a periodic HEAD /health poll — offline-full-monolith plateau
domain: skill
type: template
whenToUse: when editing the connectivity slice (VP4) — the navigator.onLine + HEAD /health derivation of selectIsOnline
plateau: offline-full-monolith
artifact_type: store
version: 20260903120000
tags:
  - skill/template/class
  - plateau/offline-full-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"

> Lives at `libs/shared/state/src/lib/connectivity/`. Registered via `provideGlobalStore()` in `store.config.ts`. Follows the same classical-NgRx shape the future `notifications` / `auth` slices will use.

# Goal

- Give the whole application one accurate `isOnline` signal — more trustworthy than `navigator.onLine` alone — and nothing else

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/GlobalStore/connectivity.store.ts.create.md|GlobalStore/connectivity.store.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- `isOnline` is derived (`browserOnline && lastHealthCheckOk`) — either signal reporting offline is enough to be offline
- All async work (browser events, the health-check timer, the HTTP call) lives in `ConnectivityEffects`; the reducer is pure
- The health-check is a lightweight, unauthenticated `HEAD /health` that must not require a session — connectivity is checkable for a logged-out user

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/GlobalStore/connectivity.store.ts.create.md|GlobalStore/connectivity.store.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/adr/connectivity-detection.md|Connectivity Detection ADR]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Action group | `{Slice}Actions` | `ConnectivityActions` | `{slice}.actions.ts` | `connectivity.actions.ts` |
| Feature + reducer | `{slice}Feature` / `{slice}Reducer` | `connectivityFeature` | `{slice}.reducer.ts` | `connectivity.reducer.ts` |
| Effects | `{Slice}Effects` | `ConnectivityEffects` | `{slice}.effects.ts` | `connectivity.effects.ts` |
| Public selector | `select{X}` | `selectIsOnline` | `{slice}.selectors.ts` | `connectivity.selectors.ts` |

# Implementation

```typescript
// Skill: class-connectivity-store
// Plateau: offline-full-monolith
// Version: 20260903090000

// connectivity.reducer.ts
export const connectivityFeature = createFeature({
  name: 'connectivity',
  reducer: createReducer(
    { browserOnline: true, lastHealthCheckOk: true },
    on(ConnectivityActions.browserReportedOnline, (s) => ({ ...s, browserOnline: true })),
    on(ConnectivityActions.browserReportedOffline, (s) => ({ ...s, browserOnline: false })),
    on(ConnectivityActions.healthCheckSucceeded, (s) => ({ ...s, lastHealthCheckOk: true })),
    on(ConnectivityActions.healthCheckFailed, (s) => ({ ...s, lastHealthCheckOk: false })),
  ),
  extraSelectors: ({ selectBrowserOnline, selectLastHealthCheckOk }) => ({
    selectIsOnline: createSelector(selectBrowserOnline, selectLastHealthCheckOk, (b, h) => b && h),
  }),
});
```

```typescript
// connectivity.effects.ts — browser events + backing-off health poll
readonly browserEvents$ = createEffect(() =>
  merge(
    fromEvent(window, 'online').pipe(map(() => ConnectivityActions.browserReportedOnline())),
    fromEvent(window, 'offline').pipe(map(() => ConnectivityActions.browserReportedOffline())),
  ),
);

readonly healthCheck$ = createEffect(() =>
  this.store.select(selectIsOnline).pipe(
    switchMap((online) => timer(0, online ? HEALTH_CHECK_INTERVAL_MS : OFFLINE_HEALTH_CHECK_INTERVAL_MS)),
    exhaustMap(() =>
      this.http.head('/health').pipe(
        map(() => ConnectivityActions.healthCheckSucceeded()),
        catchError(() => of(ConnectivityActions.healthCheckFailed())),
      ),
    ),
  ),
);
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/GlobalStore/connectivity.store.ts.create.md|GlobalStore/connectivity.store.ts.create]]

# Rules

## MUST
- `selectIsOnline` must be `false` whenever `browserOnline` is false OR `lastHealthCheckOk` is false — it is never `true` on the strength of one signal alone.
- The health-check poll interval must back off (poll less often) while `selectIsOnline` is `false`.
- The `HEAD /health` request must be unauthenticated and must not pass through `authInterceptor`.
- Only `selectIsOnline` is exported for feature consumption — the raw `browserOnline`/`lastHealthCheckOk` selectors stay internal.
- Never apply several plateau templates per class/artifact.
- Never let feature code read `navigator.onLine` directly, or duplicate this logic in a feature store.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/GlobalStore/connectivity.store.ts.create.md|GlobalStore/connectivity.store.ts.create]]

# Check list

- [ ] `isOnline` is false whenever either the browser reports offline or the last health check failed
- [ ] The health-check interval backs off while offline
- [ ] The health-check request bypasses `authInterceptor`
- [ ] `index.ts` exports `selectIsOnline` (and the actions), not the reducer/effects directly

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/GlobalStore/connectivity.store.ts.create.md|GlobalStore/connectivity.store.ts.create]]

# Unittest TestCases

- [ ] WHEN the browser fires an `offline` event THEN
  - [ ] `selectIsOnline` becomes `false` immediately, without waiting for the next health check
- [ ] WHEN the browser reports online but the health check fails THEN
  - [ ] `selectIsOnline` stays `false`
- [ ] WHEN both the browser reports online and the health check succeeds THEN
  - [ ] `selectIsOnline` becomes `true`
- [ ] WHEN a `HEAD /health` fails at the network level THEN
  - [ ] `healthCheck$` emits `healthCheckFailed()`, not an error

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/GlobalStore/connectivity.store.ts.create.md|GlobalStore/connectivity.store.ts.create]]
