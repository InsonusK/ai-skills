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
- The connectivity slice MUST be registered in the same global store configuration as the existing `auth` slice.
- `selectIsOnline` MUST be the only public selector consumed by feature code.

- Never feature code MUST NOT read `navigator.onLine` directly — it must use `selectIsOnline`.
## SHOULD
- **Duplicating the connectivity logic inside a feature store** — Consequence: multiple sources of truth for online status — Instead: rely on `libs/shared/state` connectivity slice

# Check list

- [ ] `connectivity` reducer and effects are registered in `libs/shared/state`
- [ ] `selectIsOnline` is exported for feature consumption

# Unittest TestCases

- [ ] WHEN the browser reports offline THEN `selectIsOnline` emits `false`
- [ ] WHEN the health check succeeds after the browser reported offline THEN `selectIsOnline` emits `true`
