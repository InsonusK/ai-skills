---
name: class-feature-routes
description: Generic pattern for a feature's own root-relative Routes array, with lazy sub-splitting for heavy or rarely-visited sub-pages — applies to any {Feature}/feature lib
domain: skill
type: template
plateau: navigable
artifact_type: module
version: 20260711130000
tags:
  - skill/template/class
  - plateau/navigable
created_by:
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
---

> Generic pattern, not tied to one concrete feature — any feature's own `{feature}.routes.ts` follows this, substituting `{Feature}`/`{feature}` with the real feature name.

# Goal

- Define the feature's internal navigation entirely relative to its own root, so the feature can be mounted anywhere by any parent (the shell today, potentially an embeddable module in the future) without modification
- Avoid pulling rarely-visited or heavy sub-pages into the feature's own lazy chunk when they can be split into their own, separately-loaded chunk

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- The feature as a whole is already lazy (via `loadChildren` at the mounting point) — `loadComponent` is a second, finer-grained level of splitting *within* that already-lazy chunk, not a replacement for it
- A sub-route is only split into its own `loadComponent` chunk when it is either rarely visited relative to the feature's main path, or heavy enough to justify a separate network request

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Feature routes | `{FEATURE}_ROUTES` | `ORDERS_ROUTES` | `{feature}.routes.ts` | `orders.routes.ts` |

# Implementation

```typescript
// Skill: class-feature-routes
// Plateau: navigable
// Version: 20260711130000

// orders.routes.ts
export const ORDERS_ROUTES: Routes = [
  { path: '', component: OrdersListComponent }, // main path: bundled with the feature's own chunk
  {
    path: ':id', component: OrderDetailComponent,
  },
  {
    path: ':id/print-label', // rarely visited, pulls in a heavy PDF-rendering dependency
    loadComponent: () => import('./print-label/print-label.component').then(m => m.PrintLabelComponent),
  },
];

// index.ts
export { ORDERS_ROUTES } from './lib/orders.routes';
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Rules

## MUST
- `{feature}.routes.ts` paths MUST be defined relative to the feature's own root only (e.g. `''`, `':id'`) — never including the feature's own name or any assumed mount prefix.
- `{FEATURE}_ROUTES` MUST be exported from `index.ts` — it is part of the feature's public API, alongside its Signal Store.
- This chunk MUST declare a per-chunk bundle budget; a sub-route split via `loadComponent` gets its own separate budget.

## SHOULD
- A sub-route SHOULD be split via `loadComponent` when it pulls in a dependency not needed by the feature's main path (e.g. a charting or PDF library used only on one rarely-visited screen).
- The feature's main/landing path SHOULD stay in the feature's own chunk (not further split) unless it independently exceeds its budget.

## SHOULD NOT
- Splitting SHOULD NOT be applied to every sub-route indiscriminately — each additional `loadComponent` split is an additional network round-trip on first visit, only worth it when the split avoids meaningfully more weight than that round-trip costs.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Baking the feature's expected mount segment into its own route paths (e.g. `path: 'orders/:id'`)**
  - Consequence: breaks the moment the feature is mounted under a different segment or nested inside a future embeddable module instead of directly under the shell
  - Instead: paths are always relative to the feature's own root; the parent assigns the segment at the mounting point
- **Splitting every single sub-route into its own `loadComponent` chunk regardless of size**
  - Consequence: excessive number of tiny chunks, each paying its own network round-trip cost, with no meaningful bundle-size benefit
  - Instead: keep small, frequently-visited sub-routes bundled with the feature's main chunk; split only where a sub-route is genuinely heavy or rarely visited
- **Leaving a heavy, rarely-used dependency (e.g. a PDF/report generation library) in the feature's main chunk instead of splitting it off**
  - Consequence: inflates the feature's chunk for all users of that feature, even the majority who never touch the heavy sub-page
  - Instead: split that specific sub-route via `loadComponent` so its dependency is only fetched when actually needed

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Check list

- [ ] No path inside `{feature}.routes.ts` includes the feature's own name or an assumed prefix
- [ ] `{FEATURE}_ROUTES` is exported from the feature's `index.ts`
- [ ] The feature's own chunk has a declared, enforced bundle budget
- [ ] Any sub-route pulling in a large, rarely-needed dependency is split via `loadComponent`
- [ ] No sub-route is split via `loadComponent` purely by default, without a size or usage-frequency justification

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Unittest TestCases

- [ ] WHEN a feature's exported `Routes` array is inspected THEN
  - [ ] no path in it includes the feature's own mount segment
- [ ] WHEN the feature's chunk is built THEN
  - [ ] it stays within its declared bundle budget
- [ ] WHEN a `loadComponent`-split sub-route is navigated to THEN
  - [ ] its dependency is fetched in a separate chunk, not present in the feature's main chunk

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]
