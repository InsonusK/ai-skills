---
description: Auth session slice inside libs/shared/state — classical NgRx actions/reducer/effects/selectors
project_name: shared-state
name: auth
element_kind: store
change_kind: create
tags:
  - solution/state-management
  - element/auth-store-ts
---

# Goals

- Own the auth session lifecycle (login, token refresh, logout, session expiry) as a single auditable NgRx slice
- Be the only place any part of the application reads "is the user logged in" / "what is the current user"

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Action group | {Slice}Actions | AuthActions | {slice}.actions.ts | auth.actions.ts |
| Reducer | {slice}Reducer | authReducer | {slice}.reducer.ts | auth.reducer.ts |
| Effects class | {Slice}Effects | AuthEffects | {slice}.effects.ts | auth.effects.ts |
| Selectors | select{Slice}* | selectCurrentUser | {slice}.selectors.ts | auth.selectors.ts |

# Implementation changes

```typescript
// auth.actions.ts
export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    'Login Requested': props<{ credentials: LoginCredentials }>(),
    'Login Succeeded': props<{ user: User }>(),
    'Login Failed': props<{ error: string }>(),
    'Session Expired': emptyProps(),
    'Logout Requested': emptyProps(),
  },
});
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
}
```

# Rule changes

## MUST
- `login$`/`refresh$`/`logout$` effects MUST call through an auth facade (in a `data-access`-style lib), never construct HTTP requests inline in the effect.
- `SessionExpired` MUST be dispatched by a single, central place (e.g. an HTTP interceptor reacting to 401s), not duplicated across features.

- Never no feature MUST maintain its own copy of "is logged in"/"current user" state — every read goes through `auth.selectors.ts`.
## SHOULD
- **A feature caching the current user in its own Signal Store instead of selecting from `shared-state`** — Consequence: two sources of truth for the same session data, which can silently diverge (e.g. after logout) — Instead: always select `selectCurrentUser` from `shared-state`; never duplicate session data into feature-level state

# Check list

- [ ] Every consumer of auth state reads it via `auth.selectors.ts`, never a locally duplicated copy
- [ ] `SessionExpired` is only ever dispatched from one central location

# Unittest TestCases

- [ ] WHEN `Login Requested` is dispatched with valid credentials THEN
  - [ ] `Login Succeeded` is dispatched with the resulting user
  - [ ] `selectCurrentUser` reflects the new user
- [ ] WHEN `Login Requested` is dispatched with invalid credentials THEN
  - [ ] `Login Failed` is dispatched with an error, and state is not mutated to a logged-in state
- [ ] WHEN `Session Expired` is dispatched THEN
  - [ ] `selectCurrentUser` becomes null and any feature relying on it reacts accordingly
