---
name: project-shared-state
description: Classical NgRx Store hosting global, cross-cutting state slices (auth, notifications, offline-sync) — the auth slice now owns the full session lifecycle and a global auth interceptor
domain: skill
type: template
plateau: observable
project_kind: library
version: 20260711160000
tags:
  - skill/template/project
  - plateau/observable
created_by:
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
---

# Goal

- Host global/cross-cutting state (auth session, notifications, offline-sync queue) as classical NgRx slices, each auditable via the action log and testable in isolation
- Give features one place to read/dispatch against global state without depending on each other
- Own the full auth session lifecycle: in-memory access token, granular permissions, and transparent silent refresh

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]

# Core Principles

- One slice per cross-cutting concern; a slice never contains feature-specific data
- Effects own all side effects (HTTP calls, retries, timers) — components and feature stores only dispatch actions and read selectors
- The access token lives only in memory, inside this project's auth slice; the refresh token lives only in an `HttpOnly`/`Secure`/`SameSite` cookie the client never reads

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]

# Structure

## Project Structure

```
/libs/shared/state
  /src
    /lib
      /auth
        [auth.actions.ts, auth.reducer.ts, auth.effects.ts, auth.selectors.ts](./classes/class-auth-store.skill.md)
        [auth.interceptor.ts](./classes/class-auth-interceptor.skill.md)
      /notifications
      /offline-sync
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| /auth | Auth session slice: current user, in-memory access token, granular permissions, token lifecycle events (including silent refresh), session expiry. The only place session state changes. | [[classes/class-auth-store.skill.md\|class-auth-store.skill]] |
| /auth/auth.interceptor.ts | Global HTTP interceptor: attaches the in-memory access token to outgoing requests, triggers silent refresh on 401. | [[classes/class-auth-interceptor.skill.md\|class-auth-interceptor.skill]] |
| /notifications | Global notification/toast queue slice (filled in by a future notifications-owning solution) | — |
| /offline-sync | Offline-sync queue state (filled in by the future offline-sync solution) | — |
| index.ts | Public API: exported actions and selectors per slice only; reducers/effects are registration-only | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|HttpLayer/auth.interceptor.ts.create]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @ngrx/store | matching the Angular major version in use | Actions/reducers/selectors |
| @ngrx/effects | matching the Angular major version in use | Side-effect handling (HTTP, retries, timers) |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]

## What Does NOT Belong Here

- Feature-specific state — belongs in that feature's own NgRx Signal Store inside `libs/{feature}/feature`
- HTTP client / DTO mapping logic — belongs in the relevant `data-access` lib
- Permission-based UI primitives (`*hasPermission`) — belong in `libs/shared/auth-ui`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]

## Allowed Dependencies

- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]

# Rules

## MUST
- Every slice MUST expose its actions and selectors through `index.ts`; reducers and effects are registered once in `apps/platform-shell`.
- Effects MUST be the only place a slice performs HTTP calls or other side effects.
- `accessToken` MUST be held only in the auth slice's in-memory NgRx state — never written to any persistent storage.
- `Silent Refresh Requested` MUST be dispatched once during application bootstrap, before any authenticated request is made.
- `permissions` MUST be a flat array of permission strings, never role names.
- The auth interceptor MUST be the only place an outgoing request is decorated with the `Authorization` header.

## MUST NOT
- This project MUST NOT import from any `type:feature` or `type:data-access` project.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|HttpLayer/auth.interceptor.ts.create]]

# Anti-patterns

- **Dispatching HTTP calls directly from a component against this store's actions, bypassing effects**
  - Consequence: side effects become scattered and untestable in isolation from the component tree
  - Instead: components dispatch plain actions; effects own all asynchronous work
- **Persisting `accessToken` to storage "to survive reloads more simply"**
  - Consequence: reintroduces exactly the XSS exposure the token-storage-strategy ADR chose in-memory storage to avoid
  - Instead: rely on the silent-refresh-on-bootstrap flow to repopulate the in-memory token after a reload

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]

# Check list

- [ ] Each slice's reducer, effects, and selectors are registered exactly once, in `apps/platform-shell`
- [ ] `index.ts` exports only actions and selectors, not reducers/effects directly
- [ ] No slice contains data specific to a single feature
- [ ] `accessToken` exists only as in-memory NgRx state, confirmed absent from any storage write
- [ ] Bootstrap triggers exactly one `Silent Refresh Requested` dispatch before any authenticated call

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]
