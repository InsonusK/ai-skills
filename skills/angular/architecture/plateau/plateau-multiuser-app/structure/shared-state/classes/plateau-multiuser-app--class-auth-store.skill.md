---
name: plateau-multiuser-app--class-auth-store
description: Auth session slice inside libs/shared/state — classical NgRx actions/reducer/effects/selectors, now also the platform-host-side source of @platform/contracts' read-only SessionContract for embeddable apps — multiuser-app plateau
domain: skill
type: template
plateau: multiuser-app
artifact_type: store
version: 20260711230000
tags:
  - skill/template/class
  - plateau/multiuser-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
---

> No further deferrals: `solution-authentication`'s `@platform/contracts` session-sharing slice — deferred by every earlier plateau — is re-included below.

# Goal

- Own the auth session lifecycle (login, silent refresh, logout, session expiry) as a single auditable NgRx slice
- Be the only place any part of the application reads "is the user logged in" / "what is the current user" / "what can the current user do"
- Survive a page reload from the user's point of view via a bootstrap silent refresh, without ever persisting the access token
- Be the platform-host-side source of `@platform/contracts`' `SessionContract`, so every loaded embeddable app can read the current session without implementing its own authentication

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|GlobalStore/shared-state.project.create/auth.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Authorization is expressed as permission strings, never role names
- The access token is in-memory-only state — never written to persistent storage, and never itself exposed through `SessionContract`
- `SessionContract` exposes only `currentUser`, `permissions`, `isAuthenticated` — a read-only, signal-shaped projection of this slice, never the access token itself

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]

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
// Plateau: multiuser-app
// Version: 20260711230000

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
  accessToken: string | null; // in-memory only — never persisted, never exposed via SessionContract
  permissions: string[];      // flat permission strings, never role names
  refreshInProgress: boolean;
}

// SessionContract adapter, published as part of @platform/contracts' own repository —
// reads this slice's selectors and re-exposes a read-only signal-shaped subset:
export const sessionContractAdapter = (store: Store) => ({
  currentUser: store.selectSignal(selectCurrentUser),
  permissions: store.selectSignal(selectPermissions),
  isAuthenticated: computed(() => store.selectSignal(selectCurrentUser)() !== null),
  // accessToken deliberately NOT exposed here
});
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|GlobalStore/shared-state.project.create/auth.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]

# Rules

## MUST
- `login$`/`silentRefreshOnInit$`/`logout$` effects MUST call through an auth facade, never construct HTTP requests inline in the effect.
- `SessionExpired` MUST be dispatched by a single, central place (the `authInterceptor` reacting to a failed refresh), not duplicated across features.
- `accessToken` MUST be held only in this slice's in-memory state — never written to any persistent storage, never exposed through `SessionContract`.
- `Silent Refresh Requested` MUST be dispatched once during application bootstrap, before any authenticated request is made.
- `permissions` MUST be a flat array of permission strings, never role names.
- `SessionContract` MUST be read-only from an embeddable app's point of view — an embeddable app MUST NOT be able to mutate the session through the contract.
- An embeddable app MUST read session/permission state exclusively through `SessionContract` — it MUST NOT implement its own login flow.
- If an embeddable app is loaded without an authenticated session, it MUST render its own "not authenticated" state rather than attempting its own authentication.

## MUST NOT
- No feature MUST maintain its own copy of "is logged in"/"current user"/permissions state — every read goes through `auth.selectors.ts`.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|GlobalStore/shared-state.project.create/auth.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **A feature caching the current user in its own Signal Store instead of selecting from `shared-state`**
  - Consequence: two sources of truth for the same session data, which can silently diverge
  - Instead: always select from `shared-state`
- **Persisting `accessToken` to storage "to survive reloads more simply"**
  - Consequence: reintroduces exactly the XSS exposure the in-memory strategy exists to avoid
  - Instead: rely on the silent-refresh-on-bootstrap flow
- **An embeddable app implementing its own login screen "just in case" the platform session is missing**
  - Consequence: duplicates authentication logic across teams, creates two different ways a user could end up authenticated
  - Instead: the embeddable app only ever reads `SessionContract`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|GlobalStore/shared-state.project.create/auth.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]

# Check list

- [ ] Every consumer of auth state reads it via `auth.selectors.ts`, never a locally duplicated copy
- [ ] `SessionExpired` is only ever dispatched from one central location
- [ ] `accessToken` exists only as in-memory NgRx state, confirmed absent from any storage write and from `SessionContract`
- [ ] Bootstrap triggers exactly one `Silent Refresh Requested` dispatch before any authenticated call
- [ ] `SessionContract.permissions` reflects the same permission set the platform's own `*hasPermission` checks use

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|GlobalStore/shared-state.project.create/auth.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]

# Unittest TestCases

- [ ] WHEN `Login Requested` is dispatched with valid credentials THEN
  - [ ] `Login Succeeded` is dispatched with the resulting user
- [ ] WHEN the app bootstraps with a valid refresh cookie THEN
  - [ ] `Silent Refresh Succeeded` populates `accessToken` and `permissions`
- [ ] WHEN `Session Expired` is dispatched THEN
  - [ ] `accessToken`, `permissions`, and `currentUser` are cleared
- [ ] WHEN an embeddable app reads `SessionContract.permissions` THEN
  - [ ] it reflects the same permission set the platform's own UI uses
- [ ] WHEN the platform's session expires THEN
  - [ ] `SessionContract.isAuthenticated` becomes `false` for every embeddable app reading it, with no action needed on the embeddable app's part

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|GlobalStore/shared-state.project.create/auth.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]
