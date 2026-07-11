---
name: class-auth-store
description: Auth session slice inside libs/shared/state — classical NgRx actions/reducer/effects/selectors, now owning the full session lifecycle (in-memory access token, granular permissions, silent refresh)
domain: skill
type: template
plateau: authenticated
artifact_type: store
version: 20260711150000
tags:
  - skill/template/class
  - plateau/authenticated
created_by:
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
---

> This is the auth slice first established in [[skills/angular/architecture/plateau/foundation/plateau-foundation.skill.md|foundation]] as the worked example of the global-state tier. `solution-authentication` extends it (not recreates it) with the actual session-lifecycle logic — in-memory access token, granular permissions, silent refresh — that foundation deliberately left out.

# Goal

- Own the auth session lifecycle (login, token refresh, logout, session expiry) as a single auditable NgRx slice
- Be the only place any part of the application reads "is the user logged in" / "what is the current user"
- Add token lifecycle state (in-memory access token, refresh-in-progress flag) and a granular permission set to the slice
- Trigger a silent refresh on app bootstrap so an in-memory-only access token survives a page reload from the user's point of view

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|GlobalStore/shared-state.project.create/auth.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- The access token lives only in memory, inside this slice; the refresh token lives only in an `HttpOnly`/`Secure`/`SameSite` cookie the client never reads
- Every authorization check derived from this slice is expressed as a permission string, never a role name

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|GlobalStore/shared-state.project.create/auth.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Action group | `{Slice}Actions` | `AuthActions` | `{slice}.actions.ts` | `auth.actions.ts` |
| Reducer | `{slice}Reducer` | `authReducer` | `{slice}.reducer.ts` | `auth.reducer.ts` |
| Effects class | `{Slice}Effects` | `AuthEffects` | `{slice}.effects.ts` | `auth.effects.ts` |
| Selectors | `select{Slice}*` | `selectCurrentUser`, `selectAccessToken`, `selectPermissions` | `{slice}.selectors.ts` | `auth.selectors.ts` |

# Implementation

```typescript
// Skill: class-auth-store
// Plateau: authenticated
// Version: 20260711150000

// auth.actions.ts
export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    'Login Requested': props<{ credentials: LoginCredentials }>(),
    'Login Succeeded': props<{ user: User }>(),
    'Login Failed': props<{ error: string }>(),
    'Session Expired': emptyProps(),
    'Logout Requested': emptyProps(),
    'Silent Refresh Requested': emptyProps(),
    'Silent Refresh Succeeded': props<{ accessToken: string; permissions: string[] }>(),
    'Silent Refresh Failed': emptyProps(),
  },
});

// auth.reducer.ts — state shape
interface AuthState {
  currentUser: User | null;
  accessToken: string | null; // in-memory only — never persisted, see token-storage-strategy ADR
  permissions: string[];      // granular permission strings, see authorization-model ADR
  refreshInProgress: boolean;
}

// auth.effects.ts
export class AuthEffects {
  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.loginRequested),
      exhaustMap(({ credentials }) =>
        this.authFacade.login(credentials).pipe(
          map(user => AuthActions.loginSucceeded({ user })),
          catchError(error => of(AuthActions.loginFailed({ error }))),
        ),
      ),
    ),
  );

  silentRefreshOnInit$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.silentRefreshRequested),
      exhaustMap(() =>
        this.authFacade.silentRefresh().pipe( // sends the HttpOnly refresh cookie automatically
          map(({ accessToken, permissions }) =>
            AuthActions.silentRefreshSucceeded({ accessToken, permissions })),
          catchError(() => of(AuthActions.silentRefreshFailed())),
        ),
      ),
    ),
  );
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|GlobalStore/shared-state.project.create/auth.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]

# Rules

## MUST
- `login$`/`refresh$`/`logout$` effects MUST call through an auth facade (in a `data-access`-style lib), never construct HTTP requests inline in the effect.
- `SessionExpired` MUST be dispatched by a single, central place (the auth interceptor reacting to 401s after a failed silent refresh), not duplicated across features.
- `accessToken` MUST be held only in this slice's in-memory NgRx state — never written to any persistent storage.
- `Silent Refresh Requested` MUST be dispatched once during application bootstrap, before any authenticated request is made.
- `permissions` MUST be a flat array of permission strings, never role names.

## MUST NOT
- No feature MUST maintain its own copy of "is logged in"/"current user"/permissions state — every read goes through `auth.selectors.ts`.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|GlobalStore/shared-state.project.create/auth.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **A feature caching the current user in its own Signal Store instead of selecting from `shared-state`**
  - Consequence: two sources of truth for the same session data, which can silently diverge (e.g. after logout)
  - Instead: always select `selectCurrentUser` from `shared-state`; never duplicate session data into feature-level state
- **Persisting `accessToken` to storage "to survive reloads more simply"**
  - Consequence: reintroduces exactly the XSS exposure the token-storage-strategy ADR chose in-memory storage to avoid
  - Instead: rely on the silent-refresh-on-bootstrap flow to repopulate the in-memory token after a reload

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|GlobalStore/shared-state.project.create/auth.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]

# Check list

- [ ] Every consumer of auth state reads it via `auth.selectors.ts`, never a locally duplicated copy
- [ ] `SessionExpired` is only ever dispatched from one central location
- [ ] `accessToken` exists only as in-memory NgRx state, confirmed absent from any storage write
- [ ] Bootstrap triggers exactly one `Silent Refresh Requested` dispatch before any authenticated call

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|GlobalStore/shared-state.project.create/auth.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]

# Unittest TestCases

- [ ] WHEN `Login Requested` is dispatched with valid credentials THEN
  - [ ] `Login Succeeded` is dispatched with the resulting user
  - [ ] `selectCurrentUser` reflects the new user
- [ ] WHEN `Login Requested` is dispatched with invalid credentials THEN
  - [ ] `Login Failed` is dispatched with an error, and state is not mutated to a logged-in state
- [ ] WHEN `Session Expired` is dispatched THEN
  - [ ] `selectCurrentUser`, `accessToken`, and `permissions` are cleared
- [ ] WHEN the app bootstraps with a valid refresh cookie THEN
  - [ ] `Silent Refresh Succeeded` populates `accessToken` and `permissions`
- [ ] WHEN the app bootstraps with no valid refresh cookie THEN
  - [ ] `Silent Refresh Failed` is dispatched and the user is treated as logged out

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|GlobalStore/shared-state.project.create/auth.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]
