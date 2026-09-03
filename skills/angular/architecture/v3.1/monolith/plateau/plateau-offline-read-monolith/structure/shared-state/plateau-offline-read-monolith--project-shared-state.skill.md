---
name: plateau-offline-read-monolith--project-shared-state
description: Classical NgRx Store hosting global, cross-cutting state slices — the store.config.ts registration seam plus the `connectivity` slice (`selectIsOnline`). `notifications`/`auth` arrive later in the chain. — offline-read-monolith plateau
domain: skill
type: template
plateau: offline-read-monolith
project_kind: library
version: 20260903090000
tags:
  - skill/template/project
  - plateau/offline-read-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"

> This plateau ships `libs/shared/state` with its first concrete slice — `connectivity` (VP4 / `solution-offline-first`), exposing `selectIsOnline`. The root store itself is still empty; `notifications` (VP5) and `auth` (VP7) arrive further down the chain.

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
      store.config.ts               <- provideStore({}) + provideState(connectivityFeature) + provideEffects(ConnectivityEffects)
      /notifications                <- future (VP5)
      /auth                          <- future (VP7)
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| /connectivity | The `connectivity` slice — `isOnline` derived from `navigator.onLine` events AND a periodic `HEAD /health` (either offline → offline). | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-read-monolith/structure/shared-state/classes/plateau-offline-read-monolith--class-connectivity-store.skill.md\|class-connectivity-store]] |
| store.config.ts | `provideGlobalStore()` — registers the empty root store plus the `connectivity` feature reducer + effects. | — |
| /notifications | Global notification/toast queue slice — filled in by `solution-offline-sync` (VP5). | — |
| index.ts | Public API: `provideGlobalStore`, `selectIsOnline`, the `connectivity` actions/feature. Reducers/effects stay registration-only. | — |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/GlobalStore/shared-state.project.extend.md|GlobalStore/shared-state.project.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @ngrx/store | matching the Angular major version in use | Actions/reducers/selectors |
| @ngrx/effects | matching the Angular major version in use | Side-effect handling (HTTP, retries, timers) |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

## What Does NOT Belong Here

- Feature-specific state — belongs in that feature's own NgRx Signal Store inside `libs/{feature}/feature`
- HTTP client / DTO mapping logic — belongs in the relevant `data-access` lib

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

## Allowed Dependencies

- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

# Rules

## MUST
- Every slice must expose its actions and selectors through `index.ts`; reducers and effects are registered once — here, via `provideGlobalStore()` in `store.config.ts` (which `apps/platform-shell` calls).
- Effects must be the only place a slice performs HTTP calls or other side effects.
- The `connectivity` slice's public selector surface is `selectIsOnline` only — feature code must never read `navigator.onLine` directly.
- `selectIsOnline` must be `false` whenever the browser reports offline OR the last `HEAD /health` failed; the health-check interval must back off while offline and its request must not require a session.

- This project must never import from any `type:feature` or `type:data-access` project.

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
