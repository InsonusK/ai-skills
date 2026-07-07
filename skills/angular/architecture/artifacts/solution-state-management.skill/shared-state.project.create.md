---
description: Structure of the shared/state Nx project hosting classical NgRx slices for global, cross-cutting state
name: shared-state
project_kind: library
element_kind: project
change_kind: create
---

# Goals

- Host global/cross-cutting state (auth session, notifications, offline-sync queue) as classical NgRx slices, each auditable via the action log and testable in isolation
- Give features one place to read/dispatch against global state without depending on each other

# Core Principles

- One slice per cross-cutting concern; a slice never contains feature-specific data
- Effects own all side effects (HTTP calls, retries, timers) — components and feature stores only dispatch actions and read selectors

# Structure

## Project Structure

```
/libs/shared/state
  /src
    /lib
      /auth
        auth.actions.ts
        auth.reducer.ts
        auth.effects.ts
        auth.selectors.ts
      /notifications
        notifications.actions.ts
        notifications.reducer.ts
        notifications.effects.ts
        notifications.selectors.ts
      /offline-sync
        offline-sync.actions.ts
        offline-sync.reducer.ts
        offline-sync.effects.ts
        offline-sync.selectors.ts
    index.ts
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| /auth | Auth session slice: current user, token lifecycle events, session expiry. Actions/effects here are the only place session state changes. |
| /notifications | Global notification/toast queue, consumed by any feature that needs to surface a message |
| /offline-sync | Offline-sync queue state (see the future "Синхронизация offline-данных" solution) — pending mutations, retry/conflict status |
| index.ts | Public API: exported actions and selectors per slice only; reducers/effects are registration-only and not meant to be imported directly by consumers |

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

## MUST NOT
- This project MUST NOT import from any `type:feature` or `type:data-access` project.

# Anti-patterns

- **Dispatching HTTP calls directly from a component against this store's actions, bypassing effects**
  - Consequence: side effects become scattered and untestable in isolation from the component tree
  - Instead: components dispatch plain actions; effects own all asynchronous work

# Check list

- [ ] Each slice's reducer, effects, and selectors are registered exactly once, in `apps/platform-shell`
- [ ] `index.ts` exports only actions and selectors, not reducers/effects directly
- [ ] No slice contains data specific to a single feature

# Unittest TestCases

- [ ] WHEN an auth token expires THEN
  - [ ] the auth effect dispatches a session-expired action
  - [ ] the auth reducer clears the session from state
  - [ ] any feature subscribed to the auth selector reacts accordingly
