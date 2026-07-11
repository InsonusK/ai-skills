---
name: solution-authentication
description: Token storage strategy, silent refresh, granular permission-based authorization, route guards, and session sharing with embeddable apps
domain: skill
type: architecture
version: 1
tags:
  - skill/architecture/solution
  - angular
  - auth
  - security
triggers:
  - Adding a login/session flow
  - Restricting a route or UI element to specific permissions
  - Reviewing how an embeddable app should access the current user's session
creates:
  - libs/shared/auth-ui
extends:
  - libs/shared/state (auth slice, created by the State management solution)
  - "@platform/contracts (from the Встраиваемость платформы solution)"
depends_on:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|Структура репозитория (база)]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|State management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|App routing (база)]]"
  - "[[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|Встраиваемость платформы]]"
adr:
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/adr/token-storage-strategy|Token Storage Strategy ADR]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/adr/authorization-model|Authorization Model ADR]]"
---

# Goal

- Store auth tokens in a way that minimizes XSS exposure, given that federated third-party code shares the same JS runtime (per the "Встраиваемость платформы" solution)
- Give the UI a single, granular permission model shared by route guards, UI visibility checks, and embeddable apps
- Formally define the auth guards that the "App routing (база)" solution deliberately deferred to this solution
- Let embeddable apps read the platform's session without implementing their own authentication

# Capabilities

- No token value ever persists in `localStorage`/`sessionStorage`, closing off the most common XSS-driven token theft vector
- A single permission model reused by route guards, a structural UI directive, and embeddable apps — no parallel authorization logic to keep in sync
- Silent, transparent session recovery after a page reload or a transient 401, without forcing the user to re-authenticate
- Embeddable apps built by separate teams get a working session for free, with no login screen of their own to build or maintain

# Core Principles

- The access token lives only in memory, inside `shared-state`'s auth slice; the refresh token lives only in an `HttpOnly`/`Secure`/`SameSite` cookie the client never reads
- Every authorization check — guards, directives, embeddable apps — is expressed as a permission string, never a role name
- Route guards live inside the feature whose route they protect, consistent with the hierarchical route ownership from "App routing (база)" — never centralized in the shell
- Hiding UI with a permission check is a convenience, not a security boundary; the backend remains the actual authorization boundary
- Embeddable apps are session consumers only — they read `SessionContract` from `@platform/contracts` and never implement their own login flow

# Adr

- [[skills/angular/architecture/solutions/solution-authentication.skill/adr/token-storage-strategy|In-memory access token + HttpOnly refresh cookie instead of localStorage or fully cookie-based auth]]
  - Selected variant: in-memory access token + HttpOnly refresh cookie — chosen to minimize XSS exposure given federated third-party code shares the same JS runtime
- [[skills/angular/architecture/solutions/solution-authentication.skill/adr/authorization-model|Granular permissions instead of coarse roles]]
  - Selected variant: granular permissions — chosen for scalability and to decouple embeddable apps from the platform's own role taxonomy

# Requirements

SOLUTION:
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|State management]]
  - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|libs/shared/state auth slice]] - extended by this solution with the in-memory access token, permission list, and silent-refresh handling
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|App routing (база)]]
  - Formally implements the auth guards that solution deliberately deferred (see [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create]])
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|Встраиваемость платформы]]
  - Extends the shared [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/adr/embedding-mechanism|@platform/contracts]] package with `SessionContract`, so embeddable apps can read the platform's session

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository]] - extend - add `libs/shared/auth-ui`, and the conventions for guard/interceptor/directive placement

PROJECT:
- No new Nx project beyond `libs/shared/auth-ui`; all other changes extend existing projects (shared-state, individual feature projects)

Artifact-level:
- [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|auth.store.ts (extend)]] - extend - in-memory access token, permissions, silent-refresh handling
- [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|auth.interceptor.ts]] - create - attaches access token to outgoing requests, triggers silent refresh on 401
- [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|{feature}.guard.ts (generic pattern)]] - create - functional guard restricting navigation into one of a feature's own routes
- [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|has-permission.directive.ts]] - create - structural directive controlling UI visibility by permission
- [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|@platform/contracts (extend)]] - extend - adds `SessionContract`; the token-attaching and silent-refresh-triggering behavior defined in `auth.interceptor.ts` does not change — the future "API/HTTP-слой" solution must respect this interceptor as a fixed contract point.

# Workflow

## App bootstrap (happy path)

1. On application start, `Silent Refresh Requested` is dispatched before any authenticated request is made.
2. The browser sends the `HttpOnly` refresh cookie automatically to the refresh endpoint.
3. On success, `Silent Refresh Succeeded` populates the in-memory `accessToken` and `permissions`.
4. The app proceeds as an authenticated session, with `authInterceptor` attaching the access token to subsequent requests.

## Transparent recovery from an expired access token (happy path)

1. A request fails with a 401 (access token expired, but the refresh cookie is still valid).
2. `authInterceptor` dispatches `Silent Refresh Requested` instead of surfacing the error immediately.
3. On success, the app has a fresh `accessToken`; the originating request can be retried by the caller (full retry orchestration is finalized in the future "API/HTTP-слой" solution).
4. The user experiences no visible interruption.

![Transparent recovery from an expired access token (happy path)](./diagrams/transparent-recovery-from-an-expired-access-token-happy-path.mmd)

## Guarding a feature's own route (happy path)

1. A feature attaches `requirePermission('orders.delete')` to one of its own routes, at the point that route is defined inside the feature — not in the shell.
2. A user without that permission is redirected to a forbidden route on navigation attempt; a user with it proceeds.

## Embeddable app reads the platform session (happy path)

1. An embeddable app (per the platform-embeddability solution) is loaded into the shell.
2. It reads `SessionContract.currentUser`/`permissions`/`isAuthenticated` from `@platform/contracts` — the same singleton instance the platform itself reads from.
3. It never presents its own login screen; if `isAuthenticated` is false, it renders a "not authenticated" state and defers to the platform.

## Session expiry (cross-cutting failure path)

1. The refresh cookie itself expires or is revoked; a silent refresh fails.
2. `Session Expired` (from the base auth slice) is dispatched; `accessToken` and `permissions` are cleared.
3. Every consumer — the platform's own UI and every embeddable app reading `SessionContract` — reflects the logged-out state simultaneously, without needing to poll or be told individually.

# Rules

## MUST
- [[./Implementation/Repository.extend.md#MUST|Repository.extend]]
- [[./Implementation/GlobalStore/auth.store.ts.extend.md#MUST|auth.store.ts.extend]]
- [[./Implementation/HttpLayer/auth.interceptor.ts.create.md#MUST|auth.interceptor.ts.create]]
- [[./Implementation/Routing/{feature}.guard.ts.create.md#MUST|{feature}.guard.ts.create]]
- [[./Implementation/UI/has-permission.directive.ts.create.md#MUST|has-permission.directive.ts.create]]
- [[./Implementation/EmbeddableApp/platform-contracts.extend.md#MUST|platform-contracts.extend]]

# Anti-patterns

- [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|See Repository.extend.md]] — checking a role name (`currentUser.role === 'admin'`) instead of a permission string.
- [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|See auth.store.ts.extend.md]] — persisting `accessToken` to storage "to survive reloads more simply".
- [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|See auth.interceptor.ts.create.md]] — retrying the original request indefinitely on repeated 401s.
- [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|See {feature}.guard.ts.create.md]] — centralizing all permission guards in the shell's root routes "for visibility".
- [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|See has-permission.directive.ts.create.md]] — relying on `*hasPermission` alone to protect a destructive action, with no server-side check.
- [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|See platform-contracts.extend.md]] — an embeddable app implementing its own login screen "just in case" the platform session is missing.

# Check list

- [ ] The access token is never written to `localStorage`/`sessionStorage`
- [ ] Every authorization check (guard, directive, embeddable app) is expressed as a permission string, never a role name
- [ ] Every permission guard lives inside the feature it protects, not centralized in the shell
- [ ] `authInterceptor` is excluded from the silent-refresh request itself
- [ ] Every embeddable app reads session state only through `SessionContract`, never implementing its own login flow
- [ ] Application bootstrap triggers exactly one silent-refresh attempt before treating the user as logged out