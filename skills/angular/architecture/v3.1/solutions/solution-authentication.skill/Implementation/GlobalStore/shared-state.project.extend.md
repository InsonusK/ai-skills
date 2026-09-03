---
description: Register the auth slice (and its effects) in libs/shared/state's provideGlobalStore() seam
name: shared-state
project_kind: library
element_kind: project
change_kind: extend
tags:
  - solution/authentication
  - element/shared-state-project
---

# Goals

- Wire the `auth` slice created by this solution into the same `provideGlobalStore()` seam the `connectivity` (VP4) and `notifications` (VP5) slices already use — one auditable global store

# Structure

## Project Structure

```
/libs/shared/state
  /src
    /lib
      auth/
        auth.model.ts
        auth.actions.ts
        auth.reducer.ts        <- createFeature, in-memory accessToken, permission strings
        auth.effects.ts        <- login$ / silentRefresh$ / logout$ — call AuthFacade, never inline HTTP
        auth.facade.ts         <- the auth HTTP round trips (login / refresh / logout) via http-core
        auth.interceptor.ts    <- attaches the bearer, dispatches one silent refresh on 401
        auth.selectors.ts
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| auth/ | The `auth` slice + its `AuthFacade` (HTTP) + `authInterceptor`. Same classical-NgRx shape as `connectivity`. Created per [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.create.md]]. |

# Implementation changes

Register the slice + effects in `provideGlobalStore()` (from `solution-global-store`):

```typescript
// libs/shared/state/src/lib/store.config.ts
import { authFeature } from './auth/auth.reducer';
import { AuthEffects } from './auth/auth.effects';

export function provideGlobalStore(): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideStore({}),
    provideState(connectivityFeature),   // VP4
    provideState(notificationsFeature),  // VP5
    provideState(authFeature),           // this solution
    provideEffects(ConnectivityEffects, AuthEffects),
  ]);
}
```

# Rules

## MUST
- The `auth` slice must be registered in the same `provideGlobalStore()` seam as `connectivity` / `notifications` — never a separate provider call.
- `AuthEffects` must be registered alongside `ConnectivityEffects` in the same `provideEffects(...)` call.
- `selectCurrentUser` / `selectAccessToken` / `selectPermissions` / `selectIsLoggedIn` are the only public selectors — feature code reads auth state only through these, never a duplicated copy.

- feature code must never maintain its own "is logged in" / "current user" / "permissions" — every read goes through the exported selectors.

## SHOULD
- **Splitting the auth slice into its own Nx library** — Consequence: an extra project to reason about for state that belongs with the other cross-cutting slices — Instead: keep it under `libs/shared/state/src/lib/auth/` like `connectivity` and `notifications`.

# Check list

- [ ] `authFeature` reducer + `AuthEffects` are registered in `provideGlobalStore()`
- [ ] The four auth selectors are exported for feature consumption
- [ ] No feature holds a duplicated copy of auth state

# Unittest TestCases

- [ ] WHEN the root store is inspected after `provideGlobalStore()` THEN it carries a `connectivity`, a `notifications` AND an `auth` key
- [ ] WHEN `Login Succeeded` is dispatched THEN `selectIsLoggedIn` becomes `true` and `selectPermissions` reflects the granted permissions
