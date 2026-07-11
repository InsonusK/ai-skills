---
name: project-shared-state
description: Classical NgRx Store hosting global, cross-cutting state slices — auth session (now also the source of the platform-host side of @platform/contracts' SessionContract), connectivity, notifications
domain: skill
type: template
plateau: platform
project_kind: library
version: 20260711150000
tags:
  - skill/template/project
  - plateau/platform
created_by:
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
---

> No further deferrals: `solution-authentication`'s `@platform/contracts` session-sharing slice — deferred by every earlier plateau — is re-included here. `SessionContract` (published from the separate `@platform/contracts` repository per `solution-platform-embeddability`) is a read-only signal-shaped view of this project's own `auth` slice; the contract's implementation lives in its own repository, but the data it exposes originates here.

# Goal

- Host global/cross-cutting state (auth session, connectivity, notifications) as classical NgRx slices, each auditable via the action log and testable in isolation
- Give features one place to read/dispatch against global state without depending on each other
- Be the platform-host-side source of truth that `@platform/contracts`' `SessionContract` exposes to every loaded embeddable app

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]

# Core Principles

- One slice per cross-cutting concern; a slice never contains feature-specific data
- Effects own all side effects (HTTP calls, retries, timers) — components and feature stores only dispatch actions and read selectors
- The access token lives only in this project's in-memory state — never persisted, and never itself exposed through `SessionContract` (only `currentUser`/`permissions`/`isAuthenticated` are)
- `SessionContract` is read-only from an embeddable app's point of view — only this project's own auth slice can mutate the session

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]

# Structure

## Project Structure

```
/libs/shared/state
  /src
    /lib
      /auth
        [auth.actions.ts, auth.reducer.ts, auth.effects.ts, auth.selectors.ts](./classes/class-auth-store.skill.md)
      /connectivity
        [connectivity.actions.ts, connectivity.reducer.ts, connectivity.effects.ts, connectivity.selectors.ts](./classes/class-connectivity-store.skill.md)
      /notifications
      [auth.interceptor.ts](./classes/class-auth-interceptor.skill.md)
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| /auth | Auth session slice: current user, in-memory access token, permissions, silent-refresh lifecycle. Source of `SessionContract`'s data. | [[classes/class-auth-store.skill.md\|class-auth-store.skill]] |
| /connectivity | `isOnline` slice combining `navigator.onLine` events with a periodic health check | [[classes/class-connectivity-store.skill.md\|class-connectivity-store.skill]] |
| /notifications | Global notification/toast queue slice (filled in by a future notifications-owning solution) | — |
| auth.interceptor.ts | HTTP interceptor attaching the in-memory access token to outgoing requests and triggering silent refresh on 401 | [[classes/class-auth-interceptor.skill.md\|class-auth-interceptor.skill]] |
| index.ts | Public API: exported actions and selectors per slice only | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|HttpLayer/auth.interceptor.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/shared-state.project.extend|GlobalStore/shared-state.project.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @ngrx/store | matching the Angular major version in use | Actions/reducers/selectors |
| @ngrx/effects | matching the Angular major version in use | Side-effect handling (HTTP, retries, timers) |
| @platform/contracts | semver range, singleton | Consumed (not depended on for build) — `SessionContract`'s shape mirrors this project's own `auth` slice selectors |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]

## What Does NOT Belong Here

- Feature-specific state — belongs in that feature's own NgRx Signal Store inside `libs/{feature}/feature`
- HTTP client / DTO mapping logic — belongs in the relevant `data-access` lib
- Any durable, persisted mutation queue — belongs in `libs/shared/offline-sync`
- `SessionContract`'s own implementation — that lives in the separate `@platform/contracts` repository; this project only owns the data it's derived from

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]

## Allowed Dependencies

- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]

# Rules

## MUST
- [[../repo-platform.skill.md#MUST|repo-platform]]
- Every slice MUST expose its actions and selectors through `index.ts`; reducers and effects are registered once in `apps/platform-shell`.
- `accessToken` MUST be held only in the `auth` slice's in-memory state — never written to any persistent storage, and never exposed through `SessionContract`.
- `selectIsOnline` MUST require both the browser's own online signal and the most recent health-check result to agree.
- `SessionContract` MUST be read-only from an embeddable app's point of view.

## MUST NOT
- [[../repo-platform.skill.md#MUST NOT|repo-platform]]
- This project MUST NOT import from any `type:feature` or `type:data-access` project.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]

# Anti-patterns

- **Dispatching HTTP calls directly from a component against this store's actions, bypassing effects**
  - Consequence: side effects become scattered and untestable in isolation
  - Instead: components dispatch plain actions; effects own all asynchronous work
- **Persisting `accessToken` to storage "to survive reloads more simply"**
  - Consequence: reintroduces the XSS exposure the in-memory-only strategy exists to avoid
  - Instead: rely on the silent-refresh-on-bootstrap flow
- **An embeddable app implementing its own login screen "just in case" the platform session is missing**
  - Consequence: duplicates authentication logic across teams, creates two different ways a user could end up authenticated
  - Instead: the embeddable app only ever reads `SessionContract`; this project alone establishes a session

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]

# Check list

- [ ] Each slice's reducer, effects, and selectors are registered exactly once, in `apps/platform-shell`
- [ ] `accessToken` exists only as in-memory NgRx state
- [ ] `isOnline` is false whenever either the browser reports offline or the last health check failed
- [ ] `SessionContract`'s exposed fields (`currentUser`, `permissions`, `isAuthenticated`) match what `auth.selectors.ts` exposes internally

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]
