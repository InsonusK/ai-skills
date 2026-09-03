---
description: Extend libs/shared/state with the connectivity slice used by the offline-first solution to expose an accurate isOnline signal
name: shared-state
project_kind: library
element_kind: project
change_kind: extend
tags:
  - solution/offline-first
  - element/shared-state-project
---

# Goals

- Add the connectivity slice to the existing global state library so any feature can react to accurate online/offline status

# Structure

## Project Structure

```
/libs/shared/state
  /src
    /lib
      connectivity/
        connectivity.actions.ts
        connectivity.effects.ts
        connectivity.reducer.ts
        connectivity.selectors.ts
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| connectivity/ | New slice combining `navigator.onLine` events with a periodic health-check. Created per [[./connectivity.store.ts.create.md]]. |

# Implementation changes

Register the new slice in the root store configuration of `libs/shared/state`:

```typescript
// libs/shared/state/src/lib/state.config.ts
import { connectivityReducer } from './connectivity/connectivity.reducer';
import { ConnectivityEffects } from './connectivity/connectivity.effects';

export const stateConfig = {
  reducers: {
    // ... existing reducers
    connectivity: connectivityReducer,
  },
  effects: [
    // ... existing effects
    ConnectivityEffects,
  ],
};
```

# Rules

## MUST
- The `connectivity` slice is registered in the same `provideGlobalStore()` / `store.config.ts` seam as every other cross-cutting slice.
  - Risk: a slice registered through its own separate provider call is easy to miss in tests and in a second app.
  - Fix: add `provideState(connectivityFeature)` + `provideEffects(ConnectivityEffects)` to `store.config.ts` (the same seam `solution-global-store` built).
- `selectIsOnline` is the only public selector feature code consumes from this slice.
  - Risk: features reading raw slice fields couple to the slice's internal shape and can derive "online" inconsistently.
  - Fix: `index.ts` exports `selectIsOnline` only; the browser-event and health-check state stay internal.
- Feature code never reads `navigator.onLine` directly.
  - Risk: `navigator.onLine` is true behind a captive portal or during a backend outage — features would think they are online when requests fail.
  - Fix: read `selectIsOnline`, which requires the browser signal *and* the last `HEAD /health` to agree.
## SHOULD
- **Duplicating the connectivity logic inside a feature store** — Consequence: multiple sources of truth for online status — Instead: rely on `libs/shared/state` connectivity slice

# Check list

- [ ] `connectivity` reducer and effects are registered in `libs/shared/state`
- [ ] `selectIsOnline` is exported for feature consumption

# Unittest TestCases

- [ ] WHEN the browser reports offline THEN `selectIsOnline` emits `false`
- [ ] WHEN the health check succeeds after the browser reported offline THEN `selectIsOnline` emits `true`
