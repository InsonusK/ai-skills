---
name: plateau-async-monolith--class-feature-routes
description: Generic pattern for a feature's own root-relative Routes array — applies to any {Feature}/feature lib. VP1 adds loadComponent sub-splitting for heavy/rare sub-routes plus a per-chunk bundle budget. — async-monolith plateau
domain: skill
type: template
whenToUse: when writing or reviewing a feature's {feature}.routes.ts — root-relative paths, loadComponent splits, route-level providers, permission guards
plateau: async-monolith
artifact_type: module
version: 20260902160000
tags:
  - skill/template/class
  - plateau/async-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]]"

> Generic pattern, not tied to one concrete feature. `solution-forms` and `solution-api-http-layer` don't touch a feature's routes; `solution-performance-tuned-routing` (VP1) adds the `loadComponent` sub-splitting rule.

# Goal

- Define the feature's internal navigation entirely relative to its own root, so the feature can be mounted anywhere by any parent without modification
- Keep the feature's main chunk lean by splitting a genuinely heavy or rarely-visited sub-route into its own `loadComponent` chunk

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- The feature as a whole is lazy via `loadChildren` at the mounting point; `loadComponent` is a second, finer-grained split *inside* that already-lazy chunk — it does not replace the feature-level split
- A sub-route is split via `loadComponent` only when it is rarely visited relative to the feature's main path, or heavy enough (large third-party dependency, big template/asset) to justify its own network request
- The feature stays silent on preloading — it never sets `data: { preload: true }` on its own routes; that decision belongs to whoever mounts the feature

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Feature routes | `{FEATURE}_ROUTES` | `ORDERS_ROUTES` | `{feature}.routes.ts` | `orders.routes.ts` |

# Implementation

```typescript
// Skill: class-feature-routes
// Plateau: async-monolith
// Version: 20260902160000

// orders.routes.ts
export const ORDERS_ROUTES: Routes = [
  // main path — bundled with the feature's own chunk
  { path: '', component: OrdersListComponent },
  { path: ':id', component: OrderDetailComponent },
  // rarely visited, pulls in a heavy PDF/report dependency — its own chunk
  {
    path: ':id/print-label',
    loadComponent: () =>
      import('./print-label/print-label.component').then((m) => m.PrintLabelComponent),
  },
];

// index.ts
export { ORDERS_ROUTES } from './lib/orders.routes';
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Rules

## MUST
- `{feature}.routes.ts` paths must be defined relative to the feature's own root only.
- `{FEATURE}_ROUTES` must be exported from `index.ts`.
- A sub-route pulling in a large, rarely-needed dependency (charting, PDF, report generation) must be split via `loadComponent` so that weight is not in the feature's main chunk.
- Never set `data: { preload: true }` on a route inside `{feature}.routes.ts` — a feature must not opt itself into preloading.

## SHOULD
- Keep the feature's main/landing path in the feature's own chunk (not split further) unless it independently exceeds its budget.
- Never split every sub-route via `loadComponent` by default — each split is an extra network round-trip on first visit; split only where it avoids meaningfully more weight than that round-trip costs.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]


- **Baking the feature's expected mount segment into its own route paths**
  - Consequence: breaks the moment the feature is mounted under a different segment
  - Instead: paths are always relative to the feature's own root
- **Leaving a heavy, rarely-used dependency (e.g. a PDF/report library) in the feature's main chunk**
  - Consequence: inflates the chunk for every user of the feature, including the majority who never open that sub-page
  - Instead: split that specific sub-route via `loadComponent`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Check list

- [ ] No path inside `{feature}.routes.ts` includes the feature's own name or an assumed prefix
- [ ] `{FEATURE}_ROUTES` is exported from the feature's `index.ts`
- [ ] Any sub-route pulling a large, rarely-needed dependency is split via `loadComponent`; small/common ones are not split
- [ ] No route in `{feature}.routes.ts` sets `data: { preload: true }`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Unittest TestCases

- [ ] WHEN a feature's exported `Routes` array is inspected THEN
  - [ ] no path in it includes the feature's own mount segment
  - [ ] no route sets `data: { preload: true }`
- [ ] WHEN a `loadComponent`-split sub-route is navigated to THEN
  - [ ] its dependency is fetched in a separate chunk, not present in the feature's main chunk

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]
