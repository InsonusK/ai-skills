---
description: The auth session slice inside libs/shared/state — classical NgRx actions/reducer/effects/selectors for session lifecycle, in-memory access token, granular permissions, and silent refresh
project_name: shared-state
name: auth
element_kind: store
change_kind: create
tags:
  - solution/authentication
  - element/auth-store-ts
---

# Goals

- Own the auth session lifecycle (login, silent refresh, logout, session expiry) as a single auditable NgRx slice inside `libs/shared/state` (`solution-global-store`).
- Be the only place any part of the application reads "is the user logged in" / "current user" / "permissions".
- Hold the access token **in memory only** and survive a page reload via silent-refresh-on-bootstrap, never persistent storage.

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Action group | {Slice}Actions | AuthActions | {slice}.actions.ts | auth.actions.ts |
| Reducer | {slice}Reducer | authReducer | {slice}.reducer.ts | auth.reducer.ts |
| Effects class | {Slice}Effects | AuthEffects | {slice}.effects.ts | auth.effects.ts |
| Selectors | select{Slice}* | selectCurrentUser | {slice}.selectors.ts | auth.selectors.ts |

# Implementation changes

Added under `libs/shared/state/src/lib/auth/` and registered in `store.config.ts` (from `solution-global-store`).

```typescript
// auth.actions.ts
export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    'Login Requested': props<{ credentials: LoginCredentials }>(),
    'Login Succeeded': props<{ user: User }>(),
    'Login Failed': props<{ error: string }>(),
    'Silent Refresh Requested': emptyProps(),
    'Silent Refresh Succeeded': props<{ accessToken: string; permissions: string[] }>(),
    'Silent Refresh Failed': emptyProps(),
    'Session Expired': emptyProps(),
    'Logout Requested': emptyProps(),
  },
});
```

```typescript
// auth.reducer.ts — state shape
interface AuthState {
  currentUser: User | null;
  accessToken: string | null;   // in-memory only — never persisted, see token-storage-strategy ADR
  permissions: string[];        // granular permission strings, never role names, see authorization-model ADR
  refreshInProgress: boolean;
}
```

```typescript
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
        this.authFacade.silentRefresh().pipe(   // sends the HttpOnly refresh cookie automatically
          map(({ accessToken, permissions }) =>
            AuthActions.silentRefreshSucceeded({ accessToken, permissions })),
          catchError(() => of(AuthActions.silentRefreshFailed())),
        ),
      ),
    ),
  );
}
```

# Rule changes

## MUST
- `login$` / `silentRefresh$` / `logout$` effects call through an auth facade in a `data-access`-style lib — never construct an HTTP request inline.
  - Risk: HTTP transport concerns leak into the store and cannot be tested in isolation.
  - Fix: the effect calls an injected facade; the facade owns the request.
- `accessToken` is held only in this slice's in-memory NgRx state — never written to `localStorage` / `sessionStorage` / any persistent client storage, per [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/adr/token-storage-strategy.md|token-storage-strategy]].
  - Risk: the most common XSS-driven token-theft vector, doubly dangerous with federated third-party code in the same runtime.
  - Fix: rely on silent-refresh-on-bootstrap to repopulate the in-memory token after a reload.
- `Silent Refresh Requested` is dispatched exactly once during application bootstrap (an `APP_INITIALIZER`-equivalent or root route resolver), before any authenticated request.
- `permissions` is a flat array of permission strings, never role names, per [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/adr/authorization-model.md|authorization-model]].
- `Session Expired` is dispatched from a single central place (the HTTP interceptor reacting to a 401), never duplicated across features.
- Never let a feature maintain its own copy of "is logged in" / "current user" / "permissions" — every read goes through `auth.selectors.ts`.
  - Risk: two sources of truth that silently diverge (a feature still renders as logged-in after logout).
  - Fix: select `selectCurrentUser` / `selectPermissions` from `libs/shared/state`.

## SHOULD
- Avoid persisting `accessToken` "to survive reloads more simply" — the silent-refresh flow is the sanctioned mechanism.

# Check list

- [ ] `accessToken` exists only as in-memory NgRx state — confirmed absent from any storage write.
- [ ] Bootstrap triggers exactly one `Silent Refresh Requested` before any authenticated call.
- [ ] Every consumer of auth state reads it via `auth.selectors.ts`, never a duplicated copy.
- [ ] `Session Expired` is dispatched from exactly one location.

# Unittest TestCases

- [ ] WHEN `Login Requested` with valid credentials THEN `Login Succeeded` is dispatched and `selectCurrentUser` reflects the user.
- [ ] WHEN `Login Requested` with invalid credentials THEN `Login Failed` is dispatched and state is not mutated to logged-in.
- [ ] WHEN the app bootstraps with a valid refresh cookie THEN `Silent Refresh Succeeded` populates `accessToken` and `permissions`.
- [ ] WHEN the app bootstraps with no valid refresh cookie THEN `Silent Refresh Failed` is dispatched and the user is treated as logged out.
- [ ] WHEN `Session Expired` is dispatched THEN `currentUser`, `accessToken`, and `permissions` are all cleared.
