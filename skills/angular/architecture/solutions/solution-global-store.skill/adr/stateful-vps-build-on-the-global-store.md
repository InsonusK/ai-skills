---
name: stateful-vps-build-on-the-global-store
description: Why OfflineReadResilience, OfflineWriteQueue, Authentication and PersistedState each require GlobalStore and add a slice, rather than carrying their own store
problem: Four monolith Variation Points each need cross-cutting state (connectivity, notifications, auth, preferences). Each could ship its own root store, or all could depend on GlobalStore (VP2) and register a slice in the one libs/shared/state. V1's solution-state-management always materialised the store, so the question was never forced.
decision: Each of the four VPs requires VP2 (GlobalStore) and registers a slice through the store.config.ts seam. The constraint is encoded as a real depends_on solution-global-store edge on solution-offline-first, solution-offline-sync, solution-authentication, and solution-persisted-state, and recorded as the "requires GlobalStore" Constraint in the monolith Variability Map.
tags:
  - solution/global-store
  - stack/typescript
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem

`solution-state-tiering` promotes state to the third tier (`libs/shared/state`, classical NgRx) only when a second unrelated feature genuinely needs it. Four monolith VPs produce exactly that kind of state:

- `OfflineReadResilience` — a `connectivity` slice (`isOnline`, last health-check), read by every feature that shows an offline affordance.
- `OfflineWriteQueue` — a `notifications` slice for conflict messages, read by the shell and any feature with queued mutations.
- `Authentication` — an `auth` slice (current user, permission strings, token lifecycle), read by every guarded route and `*hasPermission` check.
- `PersistedState` — a `preferences` slice (theme, density, last-viewed) plus the persistence mechanism, read across the app.

V1's `solution-state-management` always created the store from the first plateau, so "these features need a store" was never a decision — it was a given. Once `GlobalStore` is its own VP2 that a plateau can answer `No`, the question is forced: does each of the four VPs carry its own store, or do they all depend on VP2?

# Selected variant

**Selected variant:** [[#Each VP requires GlobalStore and registers a slice (selected)]]

Every one of the four solutions declares `depends_on solution-global-store` and adds its slice via the `store.config.ts` registration seam. The monolith Variability Map records this as the "requires VP2" Constraint on VP4/VP5/VP7/VP8. A plateau that answers `GlobalStore = No` cannot compose any of the four.

# Searched variants

## Each VP requires GlobalStore and registers a slice (selected)

### Description

`solution-global-store` (VP2) ships `libs/shared/state` and the `store.config.ts` seam. `solution-offline-first`, `solution-offline-sync`, `solution-authentication`, and `solution-persisted-state` each `depend_on` it and contribute one slice (reducer + effects, or a metaReducer for VP8). The `depends_on` edge is the machine-checkable form of the Feature-Model `Requires` edge.

### Benefits

- One auditable store for the flows most likely to produce a hard-to-reproduce bug — a login → silent-refresh → offline-replay → conflict sequence is one action log, not four.
- The constraint is explicit and checkable: `plateau-map-create` cross-checks every plateau's VP set against it, so "offline-first without a store to put `connectivity` in" is caught, not discovered at runtime.
- No duplicated store bootstrap, DevTools wiring, or effects registration across four solutions.
- Matches `solution-state-tiering`'s own rule — these slices are read by multiple unrelated features, which is the definition of tier 3.

### Costs

- The four VPs cannot be composed on a `GlobalStore = No` plateau. Accepted: a local-only app with offline sync or auth is not a real combination — each of those features *is* cross-cutting state.
- `libs/shared/state` accumulates slices from several solutions (N = 5 at `plateau-persisted-state-monolith`). Tracked in the intersection registry as canonical — each slice is member-disjoint and the seam was built for exactly this.

## Each VP ships its own store

### Description

`solution-offline-first` provides a connectivity store, `solution-authentication` an auth store, etc. — each a `@ngrx/signals` or standalone NgRx store provided at the root, no shared `libs/shared/state`.

### Benefits

- Each VP is self-contained; composing one drags in nothing else.
- No "requires VP2" constraint to maintain.

### Costs

- Four parallel root stores, each with its own bootstrap and DevTools registration.
- No unified action ordering across a cross-VP sequence — the exact interleaving of an auth refresh and an offline replay is unrecoverable, which is where these flows fail (see [[skills/angular/architecture/solutions/solution-global-store.skill/adr/classical-ngrx-for-the-global-tier.md|classical-ngrx-for-the-global-tier]]).
- `connectivity` / `auth` are read by many features; giving each its own store recreates tier 3 four times under four different shapes — the sprawl `solution-state-tiering` exists to prevent.

## Make GlobalStore implied rather than a constraint

### Description

Keep the slice-in-shared-store design, but do not encode a `depends_on` / Constraint — assume any plateau composing offline-first/auth/etc. also composes global-store.

### Benefits

- Slightly shorter `depends_on` lists.

### Costs

- A plateau could compose `solution-offline-first` without `solution-global-store` and the `connectivity` slice would have nowhere to register — a silent build/runtime break with nothing checking for it.
- `variability-map-create` requires every Constraint to trace to real evidence; an unencoded assumption is exactly what that rule forbids.
