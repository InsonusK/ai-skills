---
name: class-feature-routes
description: Generic pattern for a feature's own root-relative Routes array, with lazy sub-splitting for heavy or rarely-visited sub-pages — applies to any {Feature}/feature lib
domain: skill
type: template
plateau: observable
artifact_type: module
version: 20260711160000
tags:
  - skill/template/class
  - plateau/observable
created_by:
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
---

> Unchanged since [[skills/angular/architecture/plateau/navigable/plateau-navigable.skill.md|navigable]] — `solution-forms` and `solution-api-http-layer` don't touch a feature's routes, and `solution-authentication` defines route guards as their own artifact (see [[../classes/class-feature-guard.skill.md|class-feature-guard.skill]]) that a route entry references via `canActivate`, rather than as a `{feature}.routes.ts.extend.md` mutation of this file.

# Goal

- Define the feature's internal navigation entirely relative to its own root, so the feature can be mounted anywhere by any parent without modification
- Avoid pulling rarely-visited or heavy sub-pages into the feature's own lazy chunk when they can be split into their own, separately-loaded chunk

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- The feature as a whole is already lazy (via `loadChildren` at the mounting point) — `loadComponent` is a second, finer-grained level of splitting *within* that already-lazy chunk
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
// Plateau: data-capable
// Version: 20260711160000

// orders.routes.ts
export const ORDERS_ROUTES: Routes = [
  { path: '', component: OrdersListComponent },
  { path: ':id', component: OrderDetailComponent },
  {
    path: ':id/print-label',
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
- `{feature}.routes.ts` paths MUST be defined relative to the feature's own root only.
- `{FEATURE}_ROUTES` MUST be exported from `index.ts`.
- This chunk MUST declare a per-chunk bundle budget; a sub-route split via `loadComponent` gets its own separate budget.

## SHOULD
- A sub-route SHOULD be split via `loadComponent` when it pulls in a dependency not needed by the feature's main path.
- The feature's main/landing path SHOULD stay in the feature's own chunk unless it independently exceeds its budget.

## SHOULD NOT
- Splitting SHOULD NOT be applied to every sub-route indiscriminately.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Baking the feature's expected mount segment into its own route paths**
  - Consequence: breaks the moment the feature is mounted under a different segment
  - Instead: paths are always relative to the feature's own root
- **Splitting every single sub-route into its own `loadComponent` chunk regardless of size**
  - Consequence: excessive number of tiny chunks with no meaningful bundle-size benefit
  - Instead: split only where genuinely heavy or rarely visited

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Check list

- [ ] No path inside `{feature}.routes.ts` includes the feature's own name or an assumed prefix
- [ ] `{FEATURE}_ROUTES` is exported from the feature's `index.ts`
- [ ] The feature's own chunk has a declared, enforced bundle budget
- [ ] Any sub-route pulling in a large, rarely-needed dependency is split via `loadComponent`

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
