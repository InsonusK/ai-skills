---
name: project-shared-state
description: Classical NgRx Store hosting global, cross-cutting state slices — auth-slice skeleton (still no real session lifecycle) plus the new connectivity slice for accurate online/offline detection
domain: skill
type: template
plateau: monitored-app
project_kind: library
version: 20260711220000
tags:
  - skill/template/project
  - plateau/monitored-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
---

> No authentication yet — the auth slice below is still `solution-state-management`'s own worked example, not a real session lifecycle. That arrives with [[skills/angular/architecture/plateau/plateau-multiuser-app.skill/plateau-multiuser-app.skill|multiuser-app]].

# Goal

- Host global/cross-cutting state as classical NgRx slices, each auditable via the action log and testable in isolation
- Give features one place to read/dispatch against global state without depending on each other
- Give the whole application a single, accurate `isOnline` signal, more trustworthy than `navigator.onLine` alone

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/shared-state.project.extend|GlobalStore/shared-state.project.extend]]

# Core Principles

- One slice per cross-cutting concern; a slice never contains feature-specific data
- Effects own all side effects (HTTP calls, retries, timers) — components and feature stores only dispatch actions and read selectors
- `isOnline` requires both the browser's own online signal and the most recent health check to agree

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/shared-state.project.extend|GlobalStore/shared-state.project.extend]]

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
      /offline-sync
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| /auth | Skeleton auth slice — worked example of the global-state tier. Real session lifecycle arrives with `multiuser-app`. | [[skills/angular/architecture/plateau/plateau-monitored-app.skill/structure/shared-state/classes/class-auth-store.skill\|class-auth-store]] |
| /connectivity | `isOnline` slice combining `navigator.onLine` events with a periodic health check. | [[skills/angular/architecture/plateau/plateau-monitored-app.skill/structure/shared-state/classes/class-connectivity-store.skill\|class-connectivity-store]] |
| /notifications | Global notification/toast queue slice (filled in by a future notifications-owning solution) | — |
| index.ts | Public API: exported actions and selectors per slice only; reducers/effects are registration-only | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/shared-state.project.extend|GlobalStore/shared-state.project.extend]]

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

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]

## Allowed Dependencies

- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]

# Rules

## MUST
- Every slice MUST expose its actions and selectors through `index.ts`; reducers and effects are registered once in `apps/platform-shell`.
- Effects MUST be the only place a slice performs HTTP calls or other side effects.
- `selectIsOnline` MUST be the only public selector consumed by feature code.

## MUST NOT
- This project MUST NOT import from any `type:feature` or `type:data-access` project.
- Feature code MUST NOT read `navigator.onLine` directly — it must use `selectIsOnline`.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/shared-state.project.extend|GlobalStore/shared-state.project.extend]]

# Anti-patterns

- **Dispatching HTTP calls directly from a component against this store's actions, bypassing effects**
  - Consequence: side effects become scattered and untestable in isolation from the component tree
  - Instead: components dispatch plain actions; effects own all asynchronous work
- **Duplicating the connectivity logic inside a feature store**
  - Consequence: multiple sources of truth for online status
  - Instead: rely on `libs/shared/state`'s `connectivity` slice

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/shared-state.project.extend|GlobalStore/shared-state.project.extend]]

# Check list

- [ ] Each slice's reducer, effects, and selectors are registered exactly once, in `apps/platform-shell`
- [ ] `index.ts` exports only actions and selectors, not reducers/effects directly
- [ ] No slice contains data specific to a single feature
- [ ] `isOnline` is false whenever either the browser reports offline or the last health check failed

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/GlobalStore/shared-state.project.extend|GlobalStore/shared-state.project.extend]]
