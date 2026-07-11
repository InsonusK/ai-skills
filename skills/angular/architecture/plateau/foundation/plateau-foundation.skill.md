---
name: plateau-foundation
description: The base Nx workspace — apps/libs split with enforced module boundaries, and the three-tier state-management placement rule (component Signals / feature Signal Store / global NgRx). No routing, no HTTP, no UI yet.
domain: skill
type: template
version: 20260711120000
tags:
  - skill/template/plateau
  - plateau/foundation
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
---

> First plateau in the main application's chain. Next: [[skills/angular/architecture/plateau/navigable/plateau-navigable.skill.md|navigable]].

# Core Principles

- `apps/` are deployable units, `libs/` are reusable code — nothing else lives at top level
- Every business feature is split into at least a `feature` lib and a `data-access` lib from the start
- Every lib exposes a narrow public API through a single `index.ts` barrel; internals are never imported directly
- Boundaries between features/layers are enforced by Nx tags (`type:*`, `scope:*`) via `@nx/enforce-module-boundaries`, not by convention
- State lives at the smallest tier that satisfies its real consumers: component Signal → feature Signal Store → global NgRx Store — promoted upward only when a second, unrelated consumer genuinely needs it

# Capabilities

- structure
  - `nx affected` runs CI tasks only for projects impacted by a change
  - A dependency graph (`nx graph`) that reflects the real architecture, enforced by lint, not just folder convention
- state management
  - No NgRx boilerplate for purely local UI state
  - Feature-level state stays encapsulated inside its owning feature, with no cross-feature state leakage
  - A single, auditable source of truth (`libs/shared/state`) ready for the future auth, notifications, and offline-sync solutions to build on

# Usecases

## Bootstrap the workspace

```mermaid
sequenceDiagram
    autonumber
    actor Dev
    participant Nx as Nx CLI
    participant Lint as ESLint

    Dev->>Nx: create workspace with Angular preset
    Nx-->>Dev: apps/platform-shell, libs/shared/ui, libs/shared/util, libs/shared/state
    Dev->>Lint: configure @nx/enforce-module-boundaries allow-list
    Dev->>Nx: configure CI to use nx affected
```

## Add a new business feature

```mermaid
sequenceDiagram
    autonumber
    actor Dev
    participant Nx as Nx CLI
    participant Store as {feature}.store.ts

    Dev->>Nx: scaffold libs/{feature}/feature + libs/{feature}/data-access
    Dev->>Store: add feature-level Signal Store (state owned by this feature only)
    Note over Dev,Store: a piece of state is promoted to libs/shared/state only once<br/>a second, unrelated feature genuinely needs to read it
```
