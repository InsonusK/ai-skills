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
- The `auth` slice is registered in the same `provideGlobalStore()` seam as `connectivity` / `notifications` — never a separate provider call.
  - Risk: a slice on its own provider path is easy to omit in a test or a second app, and drifts from the others.
  - Fix: `provideState(authFeature)` in `store.config.ts`.
- `AuthEffects` is registered alongside `ConnectivityEffects` in the same `provideEffects(...)` call.
  - Risk: a separate `provideEffects` call for auth can be missed, so login/refresh effects never run.
  - Fix: `provideEffects(ConnectivityEffects, AuthEffects)` in the one `provideGlobalStore()`.
- `selectCurrentUser` / `selectAccessToken` / `selectPermissions` / `selectIsLoggedIn` are the only public selectors.
  - Risk: feature code reading raw slice fields couples to the slice shape and can derive "is logged in" inconsistently.
  - Fix: `index.ts` exports those four selectors + `AuthActions` + `AuthFacade` + `authInterceptor`; nothing else.
- Feature code never keeps its own copy of "is logged in" / "current user" / "permissions".
  - Risk: two sources of truth that silently diverge — a feature still renders as logged-in after logout.
  - Fix: every read goes through the exported selectors.

## SHOULD
- **Splitting the auth slice into its own Nx library** — Consequence: an extra project to reason about for state that belongs with the other cross-cutting slices — Instead: keep it under `libs/shared/state/src/lib/auth/` like `connectivity` and `notifications`.

# Check list

- [ ] `authFeature` reducer + `AuthEffects` are registered in `provideGlobalStore()`
- [ ] The four auth selectors are exported for feature consumption
- [ ] No feature holds a duplicated copy of auth state

# Unittest TestCases

- [ ] WHEN the root store is inspected after `provideGlobalStore()` THEN it carries a `connectivity`, a `notifications` AND an `auth` key
- [ ] WHEN `Login Succeeded` is dispatched THEN `selectIsLoggedIn` becomes `true` and `selectPermissions` reflects the granted permissions
