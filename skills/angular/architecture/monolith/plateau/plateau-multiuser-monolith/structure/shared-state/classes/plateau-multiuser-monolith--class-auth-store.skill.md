---
name: plateau-multiuser-monolith--class-auth-store
description: The auth session slice in libs/shared/state — classical NgRx actions/reducer/effects/selectors for login, silent refresh, logout and session expiry, with an in-memory-only access token and granular permission strings — multiuser-monolith plateau
domain: skill
type: template
whenToUse: when editing the auth slice (VP7) — session lifecycle, the in-memory access token, permission strings, silent refresh
plateau: multiuser-monolith
artifact_type: store
version: 20260903150000
tags:
  - skill/template/class
  - plateau/multiuser-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"

> `libs/shared/state/src/lib/auth/`. Registered via `provideGlobalStore()` alongside `connectivity` / `notifications`. The `AuthFacade` (HTTP) lives beside it; the `authInterceptor` too.
>
> **Catalog correction (fed back from the example):** `solution-authentication`'s `auth.store.ts.create` sketch gives `Login Succeeded` the props `{ user }` only — but a login response must also deliver the initial `accessToken` and `permissions`, exactly as `Silent Refresh Succeeded` does. This plateau uses `Login Succeeded` = `{ user, accessToken, permissions }`.

# Goal

- Own the auth session lifecycle as one auditable NgRx slice, and be the ONLY place any part of the app reads "is logged in" / "current user" / "permissions"
- Hold the access token in memory only; recover after a reload via silent-refresh-on-bootstrap, never persistent storage

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.create.md|GlobalStore/auth.store.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- `login$` / `silentRefresh$` / `logout$` effects call `AuthFacade` — never build an HTTP request inline
- `accessToken` exists only as in-memory NgRx state — never `localStorage` / `sessionStorage` / any persistent storage
- `permissions` is a flat array of permission strings, never role names
- `Session Expired` is dispatched from exactly one place (the interceptor's 401 path)

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.create.md|GlobalStore/auth.store.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/adr/token-storage-strategy.md|token-storage-strategy ADR]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/adr/authorization-model.md|authorization-model ADR]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Action group | `{Slice}Actions` | `AuthActions` | `{slice}.actions.ts` | `auth.actions.ts` |
| Feature + reducer | `{slice}Feature` | `authFeature` | `{slice}.reducer.ts` | `auth.reducer.ts` |
| Effects | `{Slice}Effects` | `AuthEffects` | `{slice}.effects.ts` | `auth.effects.ts` |
| HTTP facade | `{Slice}Facade` | `AuthFacade` | `{slice}.facade.ts` | `auth.facade.ts` |
| Public selectors | `select{X}` | `selectCurrentUser` … | `{slice}.selectors.ts` | `auth.selectors.ts` |

# Implementation

```typescript
// Skill: class-auth-store
// Plateau: multiuser-monolith
// Version: 20260903150000

// auth.reducer.ts
export interface AuthState {
  currentUser: User | null;
  accessToken: string | null;          // in-memory only
  permissions: readonly string[];      // permission strings, never roles
  refreshInProgress: boolean;
}
export const authFeature = createFeature({
  name: 'auth',
  reducer: createReducer(initial,
    on(AuthActions.loginSucceeded, (s, { user, accessToken, permissions }) => ({ ...s, currentUser: user, accessToken, permissions })),
    on(AuthActions.silentRefreshRequested, (s) => ({ ...s, refreshInProgress: true })),
    on(AuthActions.silentRefreshSucceeded, (s, { accessToken, permissions }) => ({ ...s, accessToken, permissions, refreshInProgress: false })),
    on(AuthActions.silentRefreshFailed, AuthActions.sessionExpired, AuthActions.logoutRequested, loggedOut),
  ),
  extraSelectors: ({ selectCurrentUser, selectAccessToken }) => ({
    selectIsLoggedIn: createSelector(selectCurrentUser, selectAccessToken, (u, t) => !!u && !!t),
  }),
});

// auth.effects.ts — effects call the facade, never inline HTTP
readonly login$ = createEffect(() => this.actions$.pipe(
  ofType(AuthActions.loginRequested),
  exhaustMap(({ credentials }) => this.auth.login(credentials).pipe(
    map((r) => AuthActions.loginSucceeded(r)),
    catchError((e) => of(AuthActions.loginFailed({ error: (e as Error).message }))),
  )),
));
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.create.md|GlobalStore/auth.store.ts.create]]

# Rules

## MUST
- `accessToken` must exist only as in-memory NgRx state — grep the codebase and find no token write to `localStorage` / `sessionStorage`.
- Bootstrap must dispatch exactly one `Silent Refresh Requested` before any authenticated request.
- `permissions` must be permission strings only, never role names.
- `Session Expired` must be dispatched from exactly one location.
- The `login$` / `silentRefresh$` / `logout$` effects must call `AuthFacade`, never construct an HTTP request inline.
- Never apply several plateau templates per class/artifact.
- Never let a feature keep its own copy of "is logged in" / "current user" / "permissions" — read the exported selectors.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.create.md|GlobalStore/auth.store.ts.create]]

# Check list

- [ ] `accessToken` is in-memory NgRx state only — no storage write
- [ ] Bootstrap fires exactly one `Silent Refresh Requested`
- [ ] Every auth consumer reads via `auth.selectors.ts`
- [ ] `Session Expired` dispatched from exactly one place

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.create.md|GlobalStore/auth.store.ts.create]]

# Unittest TestCases

- [ ] WHEN `Login Requested` with valid credentials THEN `Login Succeeded` is dispatched and `selectCurrentUser` / `selectPermissions` reflect it
- [ ] WHEN the app bootstraps with a valid refresh cookie THEN `Silent Refresh Succeeded` populates `accessToken` and `permissions`
- [ ] WHEN the app bootstraps with no valid cookie THEN `Silent Refresh Failed` and the user is treated as logged out
- [ ] WHEN `Session Expired` / `Logout Requested` is dispatched THEN `currentUser`, `accessToken` and `permissions` are all cleared

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.create.md|GlobalStore/auth.store.ts.create]]
