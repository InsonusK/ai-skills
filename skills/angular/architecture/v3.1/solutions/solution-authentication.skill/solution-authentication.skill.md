---
name: solution-authentication
description: In-memory access token with silent refresh, granular permission-based authorization, functional route guards attached at each feature's own route, and a *hasPermission structural directive — the monolith's authentication feature
domain: skill
type: architecture
version: 20260902000000
tags:
  - skill/architecture/solution
  - stack/typescript
  - auth
  - security
  - framework/angular
  - concern/architecture
  - solution/authentication

whenToUse: when adding a login/session flow, restricting a route or UI element to specific permissions, or reviewing how the in-memory token and silent refresh work
creates:
  - libs/shared/auth-ui
  - libs/shared/state/src/lib/auth (the auth slice)
extends:
  - libs/shared/state (register the auth slice in provideGlobalStore())
  - apps/platform-shell (HTTP interceptor registration, bootstrap silent refresh)
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]"
adr:
  - "[[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/adr/token-storage-strategy.md|token-storage-strategy]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/adr/authorization-model.md|authorization-model]]"
---

# Goal
- Store the access token so XSS exposure is minimized — in memory only, with the refresh token in an `HttpOnly` cookie the client never reads.
- Give the UI one granular permission model shared by route guards and a structural visibility directive.
- Formally implement the auth guards `solution-app-routing` deliberately deferred.

# Capabilities
- No token value ever persists in `localStorage`/`sessionStorage` — the most common token-theft vector is closed off.
- One permission model reused by a route guard and a `*hasPermission` directive — no parallel authorization logic.
- Silent, transparent session recovery after a page reload or a transient 401.

# Core Principle
- The access token lives only in memory, in `libs/shared/state`'s `auth` slice; the refresh token lives only in an `HttpOnly`/`Secure`/`SameSite` cookie the client never reads.
- Every authorization check — guard or directive — is a permission **string**, never a role name.
- Route guards live inside the feature whose route they protect (a `requirePermission` factory attached at that route), consistent with `solution-app-routing`'s hierarchical ownership — never centralized in the shell.
- Hiding UI with a permission check is a convenience, not a security boundary — the backend remains the authorization boundary.
- The HTTP interceptor (attach token, trigger silent refresh on 401) is a fixed contract point every later HTTP concern respects.

# Boundaries
- Assumes a `monolith` baseline with `solution-global-store` (for the `auth` slice), `solution-app-routing` (for guards), and `solution-api-http-layer` (the interceptor, silent-refresh call, and login all go through `libs/shared/http-core`). It is monolith VP7 and **requires** VP2 (GlobalStore) + VP3 (BackendDataAccess).
- **Does not publish `SessionContract` to embeddable apps** — that is [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]] in the `platform-host` catalog, which `depends_on` this solution. A non-federated authenticated monolith needs neither `@platform/contracts` nor federation.

# Adr
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/adr/token-storage-strategy.md|token-storage-strategy]] — in-memory access token + `HttpOnly` refresh cookie, over `localStorage` or fully cookie-based auth. Rejected: `localStorage` (XSS), full cookie auth (CSRF surface, no fine control).
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/adr/authorization-model.md|authorization-model]] — granular permission strings, over coarse roles. Rejected: role names (do not scale; couple consumers to one taxonomy).

# Requirements

SOLUTION:
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]]
  - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.create.md|libs/shared/state auth slice]] - this solution adds the `auth` slice to the store
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]
  - implements the auth guards that solution deferred (see [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create.md|{feature}.guard.ts.create]])
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]
  - the interceptor, silent-refresh call, and login round trips go through `libs/shared/http-core`

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Repository.extend.md|Repository]] - extend - add `libs/shared/auth-ui`, and guard/interceptor/directive placement conventions

PROJECT:
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create.md|libs/shared/auth-ui]] - create - login form + forbidden page, the only auth UI shared across features
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/shared-state.project.extend.md|libs/shared/state]] - extend - register the `auth` slice + `AuthEffects` in `provideGlobalStore()` ([delta-conflict Finding 4](skills/angular/architecture/v3.1/delta-conflict-analysis.md#findings))

Artifact-level:
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.create.md|auth.store.ts (create)]] - create - the `auth` slice: session lifecycle, in-memory token, permissions, silent refresh
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create.md|auth.interceptor.ts]] - create - attaches the access token, triggers silent refresh on 401
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create.md|{feature}.guard.ts (generic pattern)]] - create - functional guard restricting navigation into one of a feature's own routes
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create.md|has-permission.directive.ts]] - create - structural directive controlling UI visibility by permission

# Workflow

## App bootstrap (happy path)

1. On start, `Silent Refresh Requested` is dispatched before any authenticated request.
2. The browser sends the `HttpOnly` refresh cookie automatically.
3. `Silent Refresh Succeeded` populates the in-memory `accessToken` and `permissions`.
4. `authInterceptor` attaches the access token to subsequent requests.

## Transparent recovery from an expired access token (happy path)

1. A request fails with 401 (token expired, refresh cookie still valid).
2. `authInterceptor` dispatches `Silent Refresh Requested` instead of surfacing the error.
3. On success the caller retries the originating request; the user sees no interruption.

![Transparent recovery from an expired access token (happy path)](./diagrams/transparent-recovery-from-an-expired-access-token-happy-path.mmd)

## Guarding a feature's own route (happy path)

1. A feature attaches `requirePermission('orders.delete')` at the route's own definition inside the feature — not in the shell.
2. A user without the permission is redirected to the forbidden route; a user with it proceeds.

## Session expiry (failure path)

1. The refresh cookie expires or is revoked; silent refresh fails.
2. `Session Expired` is dispatched; `accessToken`, `currentUser`, `permissions` are cleared.
3. Every consumer reading the `auth` selectors reflects the logged-out state at once.

# Rules

## MUST
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Repository.extend.md#MUST|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/shared-state.project.extend.md#MUST|GlobalStore/shared-state.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.create.md#MUST|auth.store.ts]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create.md#MUST|auth.interceptor.ts]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create.md#MUST|{feature}.guard.ts]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create.md#MUST|has-permission.directive.ts]]
- `authInterceptor` is excluded from the silent-refresh request itself.
  - Risk: an infinite loop — the refresh request 401s, triggers a refresh, which 401s…
  - Fix: skip the interceptor for the refresh endpoint.
- Never check a role name (`currentUser.role === 'admin'`) instead of a permission string.
  - Risk: parallel authorization models that drift; consumers coupled to the role taxonomy.
  - Fix: every check is `hasPermission('...')`.
- Never centralize permission guards in the shell's root routes.
  - Risk: the shell must know every feature's authorization rules — the coupling `solution-app-routing` exists to prevent.
  - Fix: attach `requirePermission(...)` at the feature's own route.

## SHOULD
- Avoid retrying the original request indefinitely on repeated 401s — cap at one silent-refresh attempt, then treat as logged out.
- Avoid relying on `*hasPermission` alone to protect a destructive action with no server-side check.

# Check list
- [ ] The access token is never written to `localStorage`/`sessionStorage`.
- [ ] Every authorization check is a permission string, never a role name.
- [ ] Every permission guard lives inside the feature it protects.
- [ ] `authInterceptor` is excluded from the silent-refresh request.
- [ ] Bootstrap triggers exactly one silent-refresh attempt before treating the user as logged out.
