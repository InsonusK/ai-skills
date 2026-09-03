---
name: plateau-multiuser-monolith--project-shared-state
description: Classical NgRx Store hosting global, cross-cutting state slices — the store.config.ts registration seam plus the `connectivity` slice (`selectIsOnline`). `notifications`/`auth` arrive later in the chain. — multiuser-monolith plateau
domain: skill
type: template
whenToUse: when adding or editing a global NgRx slice in libs/shared/state, or wiring store.config.ts / provideGlobalStore()
plateau: multiuser-monolith
project_kind: library
version: 20260903150000
tags:
  - skill/template/project
  - plateau/multiuser-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"

> This plateau ships `libs/shared/state` with three concrete slices — `connectivity` (VP4, `selectIsOnline`), `notifications` (VP5, `selectNotifications`), and `auth` (VP7, `selectCurrentUser` / `selectAccessToken` / `selectPermissions` / `selectIsLoggedIn`). The `auth/` folder also holds `AuthFacade` (the login/refresh/logout HTTP) and `authInterceptor`. The root store itself is still empty. VP8 (PersistedState) is aspirational.

# Goal

- Host global/cross-cutting state as classical NgRx slices, each auditable via the action log and testable in isolation
- Give features one place to read/dispatch against global state without depending on each other

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

# Core Principles

- One slice per cross-cutting concern; a slice never contains feature-specific data
- Effects own all side effects (HTTP calls, retries, timers) — components and feature stores only dispatch actions and read selectors

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

# Structure

## Project Structure

```
/libs/shared/state
  /src
    /lib
      /connectivity                 <- new (VP4 / solution-offline-first)
        connectivity.actions.ts
        connectivity.reducer.ts     <- createFeature, extraSelector selectIsOnline
        connectivity.effects.ts     <- browser online/offline events + HEAD /health poll (backs off while offline)
        connectivity.selectors.ts
        connectivity.spec.ts
      /notifications                <- new (VP5 / solution-offline-sync)
        notifications.actions.ts    <- show / dismiss / clearAll
        notifications.reducer.ts    <- createFeature, selectNotifications; id generated in the reducer
        notifications.selectors.ts
        notifications.spec.ts
      /auth                          <- new (VP7 / solution-authentication)
        auth.model.ts               <- User, LoginCredentials, RefreshResult
        auth.actions.ts             <- login / silent refresh / logout / session expired
        auth.reducer.ts             <- createFeature; in-memory accessToken, permission strings
        auth.effects.ts             <- login$ / silentRefresh$ / logout$ → call AuthFacade
        auth.facade.ts              <- the auth HTTP round trips via http-core
        auth.interceptor.ts         <- attaches the bearer; 401 → one Silent Refresh Requested
        auth.selectors.ts
        auth.spec.ts
      store.config.ts               <- provideStore({}) + provideState({connectivity,notifications,auth}Feature) + provideEffects(ConnectivityEffects, AuthEffects)
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| /connectivity | The `connectivity` slice — `isOnline` derived from `navigator.onLine` events AND a periodic `HEAD /health` (either offline → offline). | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/shared-state/classes/plateau-multiuser-monolith--class-connectivity-store.skill.md\|class-connectivity-store]] |
| /notifications | The `notifications` slice — a list of `{ id, message, detail? }` with `show` / `dismiss` / `clearAll`; `ReplayOrchestrator` dispatches `show(...)` on a server-wins conflict. No effects. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/shared-state/classes/plateau-multiuser-monolith--class-notifications-store.skill.md\|class-notifications-store]] |
| /auth | The `auth` slice + `AuthFacade` (HTTP) + `authInterceptor`. In-memory access token, silent refresh, permission strings. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/shared-state/classes/plateau-multiuser-monolith--class-auth-store.skill.md\|class-auth-store]] · [[skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/shared-state/classes/plateau-multiuser-monolith--class-auth-interceptor.skill.md\|class-auth-interceptor]] |
| store.config.ts | `provideGlobalStore()` — the empty root store plus the `connectivity`, `notifications` and `auth` reducers (+ `ConnectivityEffects`, `AuthEffects`). | — |
| index.ts | Public API: `provideGlobalStore`, the `*IsOnline` / `*Notifications` / `*CurrentUser` / `*AccessToken` / `*Permissions` / `*IsLoggedIn` selectors, `AuthActions` / `NotificationsActions` / `ConnectivityActions`, `AuthFacade`, `authInterceptor`. | — |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/GlobalStore/shared-state.project.extend.md|GlobalStore/shared-state.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/GlobalStore/shared-state.project.extend.md|GlobalStore/shared-state.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/shared-state.project.extend.md|GlobalStore/shared-state.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.create.md|GlobalStore/auth.store.ts.create]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @ngrx/store | matching the Angular major version in use | Actions/reducers/selectors |
| @ngrx/effects | matching the Angular major version in use | Side-effect handling (HTTP, retries, timers) |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

## What Does NOT Belong Here

- Feature-specific state — belongs in that feature's own NgRx Signal Store inside `libs/{feature}/feature`
- Feature-specific DTO mapping — belongs in a feature's `data-access` lib. (The `auth/` folder's `AuthFacade` is the one sanctioned exception: it owns the login/refresh/logout round trips, which are cross-cutting, not feature-specific — see VP7.)

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

## Allowed Dependencies

- `libs/shared/util` (tag: `type:util`, `scope:shared`)
- `libs/shared/http-core` (tag: `type:data-access`, `scope:shared`) — VP7: `AuthFacade` sends login/refresh/logout through the base HTTP service

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

# Rules

## MUST
- Every slice must expose its actions and selectors through `index.ts`; reducers and effects are registered once — here, via `provideGlobalStore()` in `store.config.ts` (which `apps/platform-shell` calls).
- Effects must be the only place a slice performs HTTP calls or other side effects.
- The `connectivity` slice's public selector surface is `selectIsOnline` only — feature code must never read `navigator.onLine` directly.
- `selectIsOnline` must be `false` whenever the browser reports offline OR the last `HEAD /health` failed; the health-check interval must back off while offline and its request must not require a session.
- The `auth` slice's `accessToken` must exist only as in-memory NgRx state — never `localStorage` / `sessionStorage`. `authInterceptor` is the only place a request gets an `Authorization` header, and it must skip the silent-refresh request itself. Every auth check exposed for consumption is a permission string, never a role.

- This project must never import from any `type:feature` or `type:data-access` project (except the sanctioned `auth/` → `libs/shared/http-core`).

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]


- **Dispatching HTTP calls directly from a component against this store's actions, bypassing effects**
  - Consequence: side effects become scattered and untestable in isolation from the component tree
  - Instead: components dispatch plain actions; effects own all asynchronous work

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

# Check list

- [ ] Each slice's reducer, effects, and selectors are registered exactly once, in `apps/platform-shell`
- [ ] `index.ts` exports only actions and selectors, not reducers/effects directly
- [ ] No slice contains data specific to a single feature

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]
