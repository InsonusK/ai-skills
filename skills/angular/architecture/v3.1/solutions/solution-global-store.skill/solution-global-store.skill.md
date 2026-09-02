---
name: solution-global-store
description: The classical NgRx global store (libs/shared/state) — the third state tier, for cross-cutting state read or dispatched by more than one unrelated feature; creates the project and the registration seam, concrete slices are added by the features that need them
domain: skill
type: architecture
version: 20260902000000
tags:
  - skill/architecture/solution
  - stack/typescript
  - ngrx
  - state-management
  - framework/angular
  - concern/architecture
  - solution/global-store

whenToUse: when the app needs cross-cutting state shared by two or more unrelated features (auth session, connectivity, notifications, the offline-sync queue), or when reviewing whether a slice belongs in libs/shared/state
creates:
  - libs/shared/state
extends:
  - apps/platform-shell (root store registration)
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]]"
adr:
  - "[[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/adr/classical-ngrx-for-the-global-tier.md|classical-ngrx-for-the-global-tier]]"
---

# Goal
- Give cross-cutting state one auditable home — a classical NgRx Store in `libs/shared/state` — with an action log and effect-based side-effect handling.
- Let any feature read or dispatch against that state without depending on another feature.
- Create only the project and the registration seam; each concrete slice is owned by the feature that introduces it.

# Capabilities
- A single source of truth for auth / connectivity / notifications / offline-sync state, each a slice with its own actions, reducer, effects, selectors.
- Effect-based retry and conflict handling for the flows that need it (`solution-offline-sync` builds directly on this).
- `@nx/enforce-module-boundaries` proves no `type:feature` / `type:data-access` project is a dependency of the store.

# Core Principle
- `libs/shared/state` is the **third tier** of [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] — reached only when a second, unrelated feature genuinely needs a piece of state.
- One slice per cross-cutting concern; a slice never holds feature-specific data.
- Effects own every side effect (HTTP, retries, timers). Components and feature Signal Stores dispatch plain actions and read selectors — nothing else.
- The store project depends only on `libs/shared/util`; it never depends on a `type:feature` or `type:data-access` project — global state is a foundation other layers read from, not the reverse.
- This solution ships the project skeleton + `store.config.ts` registration seam. The `auth` slice is added by `solution-authentication`, `connectivity` by `solution-offline-first`, `notifications` by `solution-offline-sync`.

# Boundaries
- Assumes the `solution-repository-structure` baseline + `solution-state-tiering`'s rule. It adds one project (`libs/shared/state`, tagged `type:store` / `scope:shared`) and one boundary-allow-list extension.
- A monolith with `GlobalStore` and no auth/offline is valid — the app's own cross-feature state (shared filters, selections) lives here.
- Does not itself author any slice. A plateau that composes this solution without any slice-adding solution gets an empty-but-wired store.

# Adr
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/adr/classical-ngrx-for-the-global-tier.md|classical-ngrx-for-the-global-tier]] — the global tier is classical NgRx (actions/reducers/effects), not another Signal Store, for the action log and effect-based retry/conflict handling. Rejected: `@ngrx/signals` all the way up; a hand-rolled service.

# Requirements

SOLUTION:
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]]
  - provides the tiering rule this store is the third tier of
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]
  - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|libs/shared]] - hosts the new `libs/shared/state` project

NPM:
- `@ngrx/store`, `@ngrx/effects` — matching the Angular major version in use.

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/Repository.extend.md|Repository]] - extend - add `libs/shared/state`, the `type:store` tag, and the module-boundary rules for cross-cutting state

PROJECT:
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|libs/shared/state]] - create - the store project + `store.config.ts` registration seam (no concrete slices)

# Workflow

## Adding a cross-cutting slice (happy path — done by another solution)

1. A slice-owning solution (`solution-authentication`, `solution-offline-first`, …) adds `libs/shared/state/src/lib/{slice}/` with its actions/reducer/effects/selectors.
2. It registers the reducer + effects in `store.config.ts`.
3. It re-exports the slice's public actions + selectors from `libs/shared/state/src/index.ts`.
4. Any feature reads `select{X}` or dispatches `{X}Action` — no feature-to-feature dependency.

# Rules

## MUST
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/Repository.extend.md#MUST|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md#MUST|shared-state.project.create]]
- Never add a feature-scoped slice to `libs/shared/state`.
  - Risk: the store becomes a dumping ground and the tiering rule this catalog enforces erodes.
  - Fix: keep the slice in the owning feature's Signal Store until a second unrelated feature needs it.
- Never let `libs/shared/state` depend on a `type:feature` or `type:data-access` project.
  - Risk: a dependency cycle through the store; `nx affected` marks the store touched by every feature change.
  - Fix: effects call a `data-access` Facade that is injected, not imported as a project dependency where the boundary forbids it — or the data belongs in a feature store, not here.

## SHOULD
- Avoid dispatching an HTTP call directly from a component against a store action — components dispatch plain actions; effects own all asynchronous work.

# Check list
- [ ] `libs/shared/state` exists, tagged `type:store` / `scope:shared`.
- [ ] `store.config.ts` is the single registration point; `index.ts` exports only actions + selectors.
- [ ] `nx graph` shows no edge from `libs/shared/state` to any `type:feature` / `type:data-access` project.
- [ ] No slice contains data specific to a single feature.
