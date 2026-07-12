---
name: plateau-multiuser-app--project-shared-state
description: Classical NgRx Store hosting global, cross-cutting state slices — auth now a real session lifecycle (login, silent refresh, logout), plus the connectivity slice for accurate online/offline detection — multiuser-app plateau
domain: skill
type: template
plateau: multiuser-app
project_kind: library
version: 20260711230000
tags:
  - skill/template/project
  - plateau/multiuser-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"
---

# Goal

- Host global/cross-cutting state as classical NgRx slices, each auditable via the action log and testable in isolation
- Give features one place to read/dispatch against global state without depending on each other
- Own the auth session lifecycle as a single auditable NgRx slice, and be the platform-host-side source of `@platform/contracts`' `SessionContract`
- Give the whole application a single, accurate `isOnline` signal, more trustworthy than `navigator.onLine` alone

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]

# Core Principles

- One slice per cross-cutting concern; a slice never contains feature-specific data
- Effects own all side effects (HTTP calls, retries, timers) — components and feature stores only dispatch actions and read selectors
- `isOnline` requires both the browser's own online signal and the most recent health check to agree
- The access token is in-memory-only state — never persisted, never exposed via `SessionContract`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]

# Structure

## Project Structure

```
/libs/shared/state
  /src
    /lib
      /auth
        [auth.actions.ts, auth.reducer.ts, auth.effects.ts, auth.selectors.ts](./classes/plateau-multiuser-app--class-auth-store.skill.md)
        [auth.interceptor.ts](./classes/plateau-multiuser-app--class-auth-interceptor.skill.md)
      /connectivity
        [connectivity.actions.ts, connectivity.reducer.ts, connectivity.effects.ts, connectivity.selectors.ts](./classes/plateau-multiuser-app--class-connectivity-store.skill.md)
      /notifications
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| /auth | Real session lifecycle: login, silent refresh, logout, session expiry. Platform-host-side source of `SessionContract`. | [[skills/angular/architecture/plateau/plateau-multiuser-app/structure/shared-state/classes/plateau-multiuser-app--class-auth-store.skill\|class-auth-store]] |
| /auth/auth.interceptor.ts | Attaches the in-memory access token to outgoing requests; triggers silent refresh on 401. | [[skills/angular/architecture/plateau/plateau-multiuser-app/structure/shared-state/classes/plateau-multiuser-app--class-auth-interceptor.skill\|class-auth-interceptor]] |
| /connectivity | `isOnline` slice combining `navigator.onLine` events with a periodic health check. | [[skills/angular/architecture/plateau/plateau-multiuser-app/structure/shared-state/classes/plateau-multiuser-app--class-connectivity-store.skill\|class-connectivity-store]] |
| /notifications | Global notification/toast queue slice (filled in by a future notifications-owning solution) | — |
| index.ts | Public API: exported actions and selectors per slice only; reducers/effects are registration-only | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|HttpLayer/auth.interceptor.ts.create]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @ngrx/store | matching the Angular major version in use | Actions/reducers/selectors |
| @ngrx/effects | matching the Angular major version in use | Side-effect handling (HTTP, retries, timers) |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]

## What Does NOT Belong Here

- Feature-specific state — belongs in that feature's own NgRx Signal Store inside `libs/{feature}/feature`
- HTTP client / DTO mapping logic — belongs in the relevant `data-access` lib

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]

## Allowed Dependencies

- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]

# Rules

## MUST
- Every slice MUST expose its actions and selectors through `index.ts`; reducers and effects are registered once in `apps/platform-shell`.
- Effects MUST be the only place a slice performs HTTP calls or other side effects.
- `selectIsOnline` MUST be the only public selector consumed by feature code.
- `accessToken` MUST be held only in the `auth` slice's in-memory state — never written to any persistent storage, never exposed through `SessionContract`.
- `authInterceptor` MUST be the only place an outgoing request is decorated with the `Authorization` header.

## MUST NOT
- This project MUST NOT import from any `type:feature` or `type:data-access` project.
- Feature code MUST NOT read `navigator.onLine` directly — it must use `selectIsOnline`.
- No feature MUST maintain its own copy of "is logged in"/"current user"/permissions state.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]

# Anti-patterns

- **Dispatching HTTP calls directly from a component against this store's actions, bypassing effects**
  - Consequence: side effects become scattered and untestable in isolation from the component tree
  - Instead: components dispatch plain actions; effects own all asynchronous work
- **Duplicating the connectivity logic inside a feature store**
  - Consequence: multiple sources of truth for online status
  - Instead: rely on `libs/shared/state`'s `connectivity` slice
- **Persisting `accessToken` to storage "to survive reloads more simply"**
  - Consequence: reintroduces exactly the XSS exposure the in-memory strategy exists to avoid
  - Instead: rely on the silent-refresh-on-bootstrap flow

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]

# Check list

- [ ] Each slice's reducer, effects, and selectors are registered exactly once, in `apps/platform-shell`
- [ ] `index.ts` exports only actions and selectors, not reducers/effects directly
- [ ] No slice contains data specific to a single feature
- [ ] `isOnline` is false whenever either the browser reports offline or the last health check failed
- [ ] `accessToken` exists only as in-memory NgRx state, absent from any storage write and from `SessionContract`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]
