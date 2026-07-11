---
name: class-auth-store
description: Auth session slice inside libs/shared/state — classical NgRx actions/reducer/effects/selectors, extended with in-memory token, granular permissions, and silent-refresh handling
domain: skill
type: template
plateau: offline-app
artifact_type: store
version: 20260711140000
tags:
  - skill/template/class
  - plateau/offline-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
---

# Goal

- Own the auth session lifecycle (login, silent refresh, logout, session expiry) as a single auditable NgRx slice
- Be the only place any part of the application reads "is the user logged in" / "what is the current user" / "what can the current user do"
- Survive a page reload from the user's point of view via a bootstrap silent refresh, without ever persisting the access token

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|GlobalStore/shared-state.project.create/auth.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Authorization is expressed as permission strings, never role names
- The access token is in-memory-only state — never written to persistent storage

__Applied solutions:__
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
// Plateau: offline-app
// Version: 20260711140000

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
  accessToken: string | null; // in-memory only — never persisted
  permissions: string[];      // flat permission strings, never role names
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
- `login$`/`silentRefreshOnInit$`/`logout$` effects MUST call through an auth facade, never construct HTTP requests inline in the effect.
- `SessionExpired` MUST be dispatched by a single, central place (the `authInterceptor` reacting to a failed refresh), not duplicated across features.
- `accessToken` MUST be held only in this slice's in-memory state — never written to any persistent storage.
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
  - Instead: always select from `shared-state`; never duplicate session data into feature-level state
- **Persisting `accessToken` to storage "to survive reloads more simply"**
  - Consequence: reintroduces exactly the XSS exposure the in-memory strategy exists to avoid
  - Instead: rely on the silent-refresh-on-bootstrap flow

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
- [ ] WHEN `Login Requested` is dispatched with invalid credentials THEN
  - [ ] `Login Failed` is dispatched with an error, and state is not mutated to a logged-in state
- [ ] WHEN the app bootstraps with a valid refresh cookie THEN
  - [ ] `Silent Refresh Succeeded` populates `accessToken` and `permissions`
- [ ] WHEN the app bootstraps with no valid refresh cookie THEN
  - [ ] `Silent Refresh Failed` is dispatched and the user is treated as logged out
- [ ] WHEN `Session Expired` is dispatched THEN
  - [ ] `accessToken`, `permissions`, and `currentUser` are cleared

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|GlobalStore/shared-state.project.create/auth.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]
