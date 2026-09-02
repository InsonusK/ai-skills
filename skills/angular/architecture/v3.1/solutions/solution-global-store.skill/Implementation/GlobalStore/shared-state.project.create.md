---
description: Structure of the shared/state Nx project hosting classical NgRx slices for global, cross-cutting state
name: shared-state
project_kind: library
element_kind: project
change_kind: create
tags:
  - solution/global-store
  - element/shared-state-project
---

# Goals

- Host cross-cutting state (state read or dispatched by more than one unrelated feature) as classical NgRx slices, each auditable via the action log and testable in isolation.
- Give features one place to read/dispatch against cross-cutting state without depending on each other.

# Core Principles

- One slice per cross-cutting concern; a slice never contains feature-specific data.
- Effects own all side effects (HTTP calls, retries, timers) — components and feature stores only dispatch actions and read selectors.
- This solution creates the **project and the registration seam** only. Concrete slices are added by the feature that needs them: `auth` by `solution-authentication`, `connectivity` by `solution-offline-first`, `notifications` by `solution-offline-sync`.

# Structure

## Project Structure

```
/libs/shared/state
  /src
    /lib
      store.config.ts          (root StoreModule.forRoot / provideStore wiring, extended per slice)
      (each cross-cutting feature adds its own /{slice} folder here)
    index.ts                   (re-exports every slice's public actions + selectors)
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| store.config.ts | The single place the root store is provided; each slice-adding solution registers its reducer + effects here. |
| index.ts | Public API: exported actions and selectors per slice only; reducers/effects are registration-only and not imported directly by consumers. |
| /{slice} | Added by the owning solution — `auth` (`solution-authentication`), `connectivity` (`solution-offline-first`), `notifications` (`solution-offline-sync`). Each: `{slice}.actions.ts` / `.reducer.ts` / `.effects.ts` / `.selectors.ts`. |

# NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @ngrx/store | matching the Angular major version in use | Actions/reducers/selectors |
| @ngrx/effects | matching the Angular major version in use | Side-effect handling (HTTP, retries, timers) |

# What Does NOT Belong Here

- Feature-specific state (e.g. the list of orders being edited) — belongs in that feature's own NgRx Signal Store inside `libs/{feature}/feature`
- HTTP client / DTO mapping logic — belongs in the relevant `data-access` lib; effects here call into a `data-access` facade, they do not construct HTTP requests themselves

# Allowed Dependencies

- `libs/shared/util` (tag: `type:util`, `scope:shared`)

# Rules

## MUST
- Every slice MUST expose its actions and selectors through `index.ts`; reducers and effects are registered once in `apps/platform-shell` and are not imported by feature code directly.
- Effects MUST be the only place a slice performs HTTP calls or other side effects.

- Never import from any `type:feature` or `type:data-access` project.
## SHOULD
- **Dispatching HTTP calls directly from a component against this store's actions, bypassing effects** — Consequence: side effects become scattered and untestable in isolation from the component tree — Instead: components dispatch plain actions; effects own all asynchronous work

# Check list

- [ ] Each slice's reducer, effects, and selectors are registered exactly once, in `apps/platform-shell`
- [ ] `index.ts` exports only actions and selectors, not reducers/effects directly
- [ ] No slice contains data specific to a single feature

# Unittest TestCases

- [ ] WHEN an auth token expires THEN
  - [ ] the auth effect dispatches a session-expired action
  - [ ] the auth reducer clears the session from state
  - [ ] any feature subscribed to the auth selector reacts accordingly
