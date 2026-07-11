---
name: project-feature-feature
description: Generic template for any {feature}/feature lib — routed, presentational + container components for one feature, its feature-level Signal Store, and its own root-relative routes with lazy sub-splitting
domain: skill
type: template
plateau: navigable
project_kind: library
version: 20260711130000
tags:
  - skill/template/project
  - plateau/navigable
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
---

> Generic pattern, not tied to one concrete feature — every business feature added to the workspace gets its own `libs/{feature}/feature` project following this template, substituting `{Feature}`/`{feature}` with the real feature name.

# Goal

- Colocate a feature's state with its components inside `libs/{feature}/feature`, keeping feature-level state encapsulated behind the workspace's module boundaries
- Avoid classical NgRx action/reducer/effect boilerplate for state owned by exactly one feature
- Define the feature's internal navigation entirely relative to its own root, so the feature can be mounted anywhere by any parent without modification
- Avoid pulling rarely-visited or heavy sub-pages into the feature's own lazy chunk when they can be split into their own, separately-loaded chunk

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Structure

## Project Structure

```
/libs/{feature}
  /feature
    /src
      /lib
        [{feature}.store.ts](./classes/class-feature-store.skill.md)
        [{feature}.routes.ts](./classes/class-feature-routes.skill.md)
      index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| `{feature}.store.ts` | Feature-level NgRx Signal Store. Owns all state and derived data specific to this feature. | [[classes/class-feature-store.skill.md\|class-feature-store.skill]] |
| `{feature}.routes.ts` | Feature's own root-relative `Routes` array, exported from `index.ts`. Sub-routes that are heavy or rarely visited are split via `loadComponent`. | [[classes/class-feature-routes.skill.md\|class-feature-routes.skill]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]

## What Does NOT Belong Here

- HTTP client wiring / DTO mapping — belongs in this feature's own `data-access` lib
- Cross-cutting state (auth, notifications, offline-sync) — belongs in `libs/shared/state`
- A hardcoded assumption of the feature's own mount segment — that is assigned by whoever mounts it

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]

## Allowed Dependencies

- `libs/{feature}/data-access` (same scope)
- `libs/shared/ui` (tag: `type:ui`, `scope:shared`)
- `libs/shared/util` (tag: `type:util`, `scope:shared`)
- `libs/shared/state` (tag: `type:store`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST
- The feature Signal Store MUST live inside `libs/{feature}/feature`, not in a shared location.
- The feature Signal Store MUST be the only place that feature's components read/write feature-owned state.
- `{feature}.routes.ts` paths MUST be defined relative to the feature's own root only (e.g. `''`, `':id'`) — never including the feature's own name or any assumed mount prefix.
- `{FEATURE}_ROUTES` MUST be exported from `index.ts` — it is part of the feature's public API, alongside its Signal Store.
- This project MUST declare a per-chunk bundle budget for its own lazy chunk; a sub-route split via `loadComponent` gets its own separate budget.

## SHOULD
- A sub-route SHOULD be split via `loadComponent` when it pulls in a dependency not needed by the feature's main path (e.g. a charting or PDF library used only on one rarely-visited screen).
- The feature's main/landing path SHOULD stay in the feature's own chunk (not further split) unless it independently exceeds its budget.

## SHOULD NOT
- Splitting SHOULD NOT be applied to every sub-route indiscriminately — each additional `loadComponent` split is an additional network round-trip on first visit, only worth it when the split avoids meaningfully more weight than that round-trip costs.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Anti-patterns

- **Placing a feature Signal Store in `libs/shared/state` or another feature's lib**
  - Consequence: feature state leaks across module boundaries and violates the workspace's enforced module boundaries
  - Instead: keep the store colocated with the feature it belongs to
- **Baking the feature's expected mount segment into its own route paths (e.g. `path: 'orders/:id'`)**
  - Consequence: breaks the moment the feature is mounted under a different segment
  - Instead: paths are always relative to the feature's own root; the parent assigns the segment at the mounting point
- **Splitting every single sub-route into its own `loadComponent` chunk regardless of size**
  - Consequence: excessive number of tiny chunks, each paying its own network round-trip cost, with no meaningful bundle-size benefit
  - Instead: keep small, frequently-visited sub-routes bundled with the feature's main chunk; split only where genuinely heavy or rarely visited
- **Leaving a heavy, rarely-used dependency (e.g. a PDF/report generation library) in the feature's main chunk instead of splitting it off**
  - Consequence: inflates the feature's chunk for all users of that feature, even the majority who never touch the heavy sub-page
  - Instead: split that specific sub-route via `loadComponent` so its dependency is only fetched when actually needed

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Check list

- [ ] `libs/{feature}/feature` contains exactly one feature-level Signal Store for that feature
- [ ] The store is exported from the lib's `index.ts`
- [ ] No path inside `{feature}.routes.ts` includes the feature's own name or an assumed prefix
- [ ] `{FEATURE}_ROUTES` is exported from the feature's `index.ts`
- [ ] The feature's own chunk has a declared, enforced bundle budget
- [ ] Any sub-route pulling in a large, rarely-needed dependency is split via `loadComponent`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]
