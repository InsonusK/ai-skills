---
name: registry-feature-routes-ts
description: Conflict Detection result for the `feature-routes-ts` element in the plateau-async-monolith plateau
tags:
  - concern/architecture
  - stack/typescript
  - element/feature-routes-ts
---

# Element
`{feature}.routes.ts` — a feature's own root-relative `Routes` array in `libs/{feature}/feature/src/lib/`, exported from `index.ts`.

# Involved solutions
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] (`.create` — `FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create` — defines the feature's root-relative routes; the feature as a whole is lazy via `loadChildren` at its mount point)
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] (VP1, `.extend` — `FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend` — switches a genuinely heavy or rarely-visited sub-route from `component:` to `loadComponent:`, and declares the feature's per-chunk budget)

# Classification
`FMN` / `TMN` — single-direction refine. Category `M` (code change to the routes array). Kind `N` (independent): `solution-performance-tuned-routing` only *changes the loading mechanism* of an individual sub-route (`component` → `loadComponent`); it never removes or reorders a route `solution-app-routing` defined, and it adds nothing `solution-app-routing` would object to. The two never write the same array element. `solution-performance-tuned-routing` declares `depends_on solution-app-routing`, so the create-before-extend order is already recorded.

# Ordering
`source: ordering-only` — there is no VP↔VP Feature-Model constraint (`HierarchicalRouting` is a common baseline feature, not a VP). The create-then-extend order lives solely in `solution-performance-tuned-routing`'s `depends_on solution-app-routing`; nothing else records or needs it.

# Resolution
**Canonical — no resolver.** The `plateau-async-monolith` example demonstrates it: `orders.routes.ts` keeps `{ path: '', component: OrderFormComponent }` (from `solution-app-routing`) and adds `{ path: 'report', loadComponent: () => import(...) }` (from `solution-performance-tuned-routing`). The production build confirms `order-report-component` is emitted as its own lazy chunk, absent from the feature's main chunk. `orders.routes.spec.ts` guards that no route self-sets `data.preload` and that the main path is not further split.
