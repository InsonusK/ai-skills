---
name: class-feature-routes
description: Generic pattern for a feature's own routes — defined and exported relative to the feature's own root only, with per-route permission guards and loadComponent splitting where justified
domain: skill
type: template
plateau: platform
artifact_type: module
version: 20260711150000
tags:
  - skill/template/class
  - plateau/platform
created_by:
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
---

> Unchanged since [[skills/angular/architecture/plateau/offline-app/plateau-offline-app.skill.md|offline-app]]. Generic pattern, not tied to one concrete feature.

# Goal

- Define the feature's internal navigation entirely relative to its own root, so the feature can be mounted anywhere by any parent without modification
- Avoid pulling rarely-visited or heavy sub-pages into the feature's own lazy chunk when they can be split into their own chunk

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- The feature as a whole is already lazy via `loadChildren` at the mounting point; `loadComponent` is a second, finer-grained level of splitting within that already-lazy chunk
- `data: { preload: true }` is never set here — that decision belongs to whoever mounts this feature

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Feature routes | `{FEATURE}_ROUTES` | `ORDERS_ROUTES` | `{feature}.routes.ts` | `orders.routes.ts` |

# Implementation

```typescript
// Skill: class-feature-routes
// Plateau: platform
// Version: 20260711150000

// orders.routes.ts
export const ORDERS_ROUTES: Routes = [
  { path: '', component: OrdersListComponent },
  {
    path: ':id/print-label',
    loadComponent: () => import('./print-label/print-label.component').then(m => m.PrintLabelComponent),
  },
  {
    path: ':id/delete-confirm',
    canActivate: [requirePermission('orders.delete')],
    component: DeleteConfirmComponent,
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
- A feature's own chunk MUST declare a per-chunk bundle budget.

## SHOULD
- A sub-route SHOULD be split via `loadComponent` when it pulls in a dependency not needed by the feature's main path.
- The feature's main/landing path SHOULD stay in the feature's own chunk unless it independently exceeds its budget.

## SHOULD NOT
- Splitting SHOULD NOT be applied to every sub-route indiscriminately.

## MUST NOT
- A feature MUST NOT set `preload: true` on its own routes.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Baking the feature's expected mount segment into its own route paths**
  - Consequence: breaks the moment the feature is mounted under a different segment
  - Instead: paths are always relative to the feature's own root
- **Splitting every single sub-route into its own `loadComponent` chunk regardless of size**
  - Consequence: excessive number of tiny chunks
  - Instead: keep small, frequently-visited sub-routes bundled with the main chunk
- **A feature marking its own route `preload: true` "because it's important"**
  - Consequence: bypasses the mounting point's authority over the preloading decision
  - Instead: the feature stays silent on preloading; the parent decides

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

- [ ] WHEN the feature's chunk is built THEN
  - [ ] it stays within its declared bundle budget
- [ ] WHEN a `loadComponent`-split sub-route is navigated to THEN
  - [ ] its dependency is fetched in a separate chunk
- [ ] WHEN a feature's exported `Routes` array is inspected THEN
  - [ ] no path in it includes the feature's own mount segment, and none set `data: { preload: true }` on themselves

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]
