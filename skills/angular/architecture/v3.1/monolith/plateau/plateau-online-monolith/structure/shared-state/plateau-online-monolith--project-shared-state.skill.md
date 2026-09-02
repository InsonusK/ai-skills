---
name: plateau-online-monolith--project-shared-state
description: Classical NgRx Store hosting global, cross-cutting state slices — the project + store.config.ts registration seam; concrete slices are added by the features that need them (connectivity, notifications, auth) — online-monolith plateau
domain: skill
type: template
plateau: online-monolith
project_kind: library
version: 20260902000000
tags:
  - skill/template/project
  - plateau/online-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]]"

> This plateau ships `libs/shared/state` empty-but-wired — the classical NgRx root store with no concrete slice yet. Slices arrive with their features: `connectivity` (offline-read), `notifications` (offline-full), `auth` (multiuser).

# Goal

- Host global/cross-cutting state as classical NgRx slices, each auditable via the action log and testable in isolation
- Give features one place to read/dispatch against global state without depending on each other

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

# Core Principles

- One slice per cross-cutting concern; a slice never contains feature-specific data
- Effects own all side effects (HTTP calls, retries, timers) — components and feature stores only dispatch actions and read selectors

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

# Structure

## Project Structure

```
/libs/shared/state
  /src
    /lib
      /notifications
      /offline-sync
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| /notifications | Global notification/toast queue slice (filled in by a future notifications-owning solution) | — |
| /offline-sync | Offline-sync queue state (filled in by a future offline-sync-owning solution) | — |
| index.ts | Public API: exported actions and selectors per slice only; reducers/effects are registration-only | — |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @ngrx/store | matching the Angular major version in use | Actions/reducers/selectors |
| @ngrx/effects | matching the Angular major version in use | Side-effect handling (HTTP, retries, timers) |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

## What Does NOT Belong Here

- Feature-specific state — belongs in that feature's own NgRx Signal Store inside `libs/{feature}/feature`
- HTTP client / DTO mapping logic — belongs in the relevant `data-access` lib

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

## Allowed Dependencies

- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

# Rules

## MUST
- Every slice must expose its actions and selectors through `index.ts`; reducers and effects are registered once in `apps/platform-shell`.
- Effects must be the only place a slice performs HTTP calls or other side effects.

- This project must never import from any `type:feature` or `type:data-access` project.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]


- **Dispatching HTTP calls directly from a component against this store's actions, bypassing effects**
  - Consequence: side effects become scattered and untestable in isolation from the component tree
  - Instead: components dispatch plain actions; effects own all asynchronous work

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]

# Check list

- [ ] Each slice's reducer, effects, and selectors are registered exactly once, in `apps/platform-shell`
- [ ] `index.ts` exports only actions and selectors, not reducers/effects directly
- [ ] No slice contains data specific to a single feature

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/GlobalStore/shared-state.project.create.md|GlobalStore/shared-state.project.create]]
