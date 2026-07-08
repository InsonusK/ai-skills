---
description: Extend the base workspace with a shared/state lib for global, cross-cutting NgRx state, and extend the module-boundary allow-list accordingly
element_kind: repository
change_kind: extend
---

# Structure

## Workspace Structure

```
/libs
  /shared
    /ui
    /util
    /state        <- new
  /{feature}
    /feature
    /data-access
```

## Directory and project skills

| Directory | Description |
| ---------- | ----------- |
| /libs/shared/state | Classical NgRx Store (actions/reducers/effects/selectors) for global, cross-cutting state: auth session, notifications, offline-sync queue. Tagged `type:store`, `scope:shared`. One slice per concern (e.g. `auth`, `notifications`, `offline-sync`), each with its own actions/reducer/effects/selectors, all registered under one root `StoreModule.forRoot`/feature registration in `apps/platform-shell`. |

# Nx tag taxonomy — extension

| Axis | New value | Meaning |
| ----- | ---------- | ------- |
| `type` | `store` | Classical NgRx Store slice for global/cross-cutting state |

`@nx/enforce-module-boundaries` allow-list — extension:

| type | may additionally depend on |
| ----- | ---------------------------- |
| `feature` | `type:store` with `scope:shared` (read/dispatch against global state) |
| `store` (scope:shared) | `type:util` with `scope:shared` |

`type:store` (scope:shared) must not depend on any `type:feature` or `type:data-access` project — global state is a foundation other layers read from, not the reverse.

# Rules

## MUST
- Every slice inside `libs/shared/state` MUST correspond to genuinely global/cross-cutting state (read or dispatched by more than one unrelated feature) — feature-scoped state MUST NOT be added here (see solution's Core Principles for the tiering rule).
- `libs/shared/state` MUST NOT depend on any `type:feature` or `type:data-access` project.

## MUST NOT
- A `type:feature` project MUST NOT reach into another feature's Signal Store directly to read cross-cutting data — if the data is genuinely needed by multiple features, it belongs in `libs/shared/state`, not in one feature's own store.

# Anti-patterns

- **Adding a feature-specific slice to `libs/shared/state` "because it might be needed elsewhere later"**
  - Consequence: erodes the tiering rule this solution exists to enforce, and turns the shared state lib into a dumping ground
  - Instead: keep the slice in the owning feature's Signal Store until a second, unrelated feature genuinely needs to read it

# Unittest TestCases

- [ ] WHEN a feature attempts to import another feature's Signal Store directly THEN
  - [ ] `@nx/enforce-module-boundaries` fails the lint step
- [ ] WHEN `libs/shared/state` is built THEN
  - [ ] it has no dependency edge to any `type:feature` or `type:data-access` project in `nx graph`
