---
name: plateau-navigable
description: The foundation workspace with real navigation — hierarchical, root-relative route ownership at every level and a selective preloading/bundle-budget strategy on top of it. Features can be reached and moved around without touching the shell.
domain: skill
type: template
version: 20260711130000
tags:
  - skill/template/plateau
  - plateau/navigable
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill.md|solution-lazy-loading-routing]]"
---

> Second plateau in the main application's chain. Previous: [[skills/angular/architecture/plateau/foundation/plateau-foundation.skill.md|foundation]]. Next: [[skills/angular/architecture/plateau/data-capable/plateau-data-capable.skill.md|data-capable]].

# Core Principles

- `apps/` are deployable units, `libs/` are reusable code; state lives at the smallest tier that satisfies its real consumers (component Signal → feature Signal Store → global NgRx Store) — carried over unchanged from [[skills/angular/architecture/plateau/foundation/plateau-foundation.skill.md|foundation]]
- Routes are owned hierarchically: the shell only ever knows first-level root segments; a feature only ever knows paths relative to its own root, and never bakes its own mount segment into its own route definitions
- The parent (the shell, at this plateau) assigns the mount segment at the point of mounting, via one `loadChildren` entry per root segment — the child never assumes where it will be mounted
- Every routable feature's `Routes` are part of its public API, exported from `index.ts` alongside its Signal Store
- Preloading is opt-in, never the default: `loadChildren` makes every feature lazy by default, and only a deliberately reviewed subset of top-level segments is marked for background preloading, exclusively at the mounting point
- `loadComponent` is a second, finer-grained split *inside* an already-lazy feature chunk, applied only where a sub-route is heavy or rarely visited — not a default

# Capabilities

- structure
  - `nx affected` runs CI tasks only for projects impacted by a change
  - A dependency graph (`nx graph`) that reflects the real architecture, enforced by lint
- state management
  - No NgRx boilerplate for purely local UI state; feature state stays encapsulated; a single auditable global store (`libs/shared/state`)
- routing
  - Any feature can be mounted, remounted, or moved without changing its own code — it never knows its own URL prefix
  - One consistent mounting mechanism (`loadChildren` against an exported `Routes` array) for every directly-owned feature
  - A single root `app.routes.ts` in `apps/platform-shell` stays a flat list of one-root-segment-per-feature entries, never a nested route tree
- performance
  - A custom `SelectivePreloadingStrategy` warms up only the small set of features explicitly marked worth it, leaving everything else purely on-demand
  - Enforced (`error`-level) bundle budgets on the initial bundle and every feature's own lazy chunk catch regressions in CI
  - Heavy or rarely-visited sub-pages inside a feature are split into their own `loadComponent` chunk without affecting the feature's main chunk

# Usecases

## Add a new routed feature

```mermaid
sequenceDiagram
    autonumber
    actor Dev
    participant Nx as Nx CLI
    participant Feature as {feature}.routes.ts
    participant Shell as app.routes.ts

    Dev->>Nx: scaffold libs/{feature}/feature (+ data-access)
    Dev->>Feature: define routes relative to feature's own root only (e.g. '', ':id')
    Dev->>Feature: export {FEATURE}_ROUTES from index.ts
    Dev->>Shell: add one loadChildren entry mounting the feature at its own root segment
    Note over Dev,Shell: the feature never knows or assumes the segment name the shell mounts it at
```

## Mark a high-traffic feature for background preloading

```mermaid
sequenceDiagram
    autonumber
    actor Dev
    participant Shell as app.routes.ts
    participant Strategy as SelectivePreloadingStrategy
    actor User

    Dev->>Shell: add data: { preload: true } to a feature's mounting entry
    User->>Shell: navigates to the initial route
    Shell->>Strategy: initial navigation settles
    Strategy->>Strategy: check route.data.preload for every mounted segment
    Strategy-->>Shell: fetch only flagged features' chunks, in the background
    Note over User,Strategy: navigating into the flagged feature later is instant;<br/>everything else stays purely on-demand
```

## Split a heavy sub-page out of a feature's chunk

```mermaid
sequenceDiagram
    autonumber
    actor Dev
    participant Routes as {feature}.routes.ts
    actor User

    Dev->>Routes: identify a rarely-visited or heavy sub-route (e.g. PDF export)
    Dev->>Routes: switch that sub-route from component: to loadComponent:
    Note over Dev,Routes: the feature's main chunk shrinks by that dependency's weight
    User->>Routes: navigates to the feature's main path
    Routes-->>User: main chunk only, heavy sub-page not yet fetched
    User->>Routes: navigates into the split sub-route
    Routes-->>User: sub-route's own chunk fetched on demand
```
