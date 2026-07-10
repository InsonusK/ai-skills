---
description: Generic pattern for lazy-loading decisions inside a feature's own routes — when to split off a sub-route with loadComponent, and the feature's own chunk budget
project_name: "{Feature}"
name: "{feature}"
element_kind: module
change_kind: extend
---

# How this generic file is used
This extends the routes pattern from [[[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]] It applies inside any `libs/{feature}/feature` project, once that feature itself is already lazily loaded as a whole via `loadChildren` at its mounting point.

# Goals

- Avoid pulling rarely-visited or heavy sub-pages into the feature's own lazy chunk when they can be split into their own, separately-loaded chunk

# Core Principles

- The feature as a whole is already lazy (via `loadChildren` at the mounting point) — `loadComponent` is a second, finer-grained level of splitting *within* that already-lazy chunk, not a replacement for it
- A sub-route is only split into its own `loadComponent` chunk when it is either rarely visited relative to the feature's main path, or heavy enough (large third-party dependency, big template/asset) to justify a separate network request

# Implementation changes

```typescript
// orders.routes.ts
export const ORDERS_ROUTES: Routes = [
  { path: '', component: OrdersListComponent }, // main path: bundled with the feature's own chunk
  {
    path: ':id/print-label', // rarely visited, pulls in a heavy PDF-rendering dependency
    loadComponent: () => import('./print-label/print-label.component').then(m => m.PrintLabelComponent),
  },
];
```

# Rule changes

## MUST
- A feature's own chunk MUST declare a per-chunk bundle budget (see [[.[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend#MUST|Repository]]a sub-route split via `loadComponent` gets its own separate budget.

## SHOULD
- A sub-route SHOULD be split via `loadComponent` when it pulls in a dependency not needed by the feature's main path (e.g. a charting or PDF library used only on one rarely-visited screen).
- The feature's main/landing path SHOULD stay in the feature's own chunk (not further split) unless it independently exceeds its budget.

## SHOULD NOT
- Splitting SHOULD NOT be applied to every sub-route indiscriminately — each additional `loadComponent` split is an additional network round-trip on first visit to that sub-route, which is only worth it when the split avoids meaningfully more weight than that round-trip costs.

# Anti-patterns

- **Splitting every single sub-route into its own `loadComponent` chunk regardless of size**
  - Consequence: excessive number of tiny chunks, each paying its own network round-trip cost, with no meaningful bundle-size benefit
  - Instead: keep small, frequently-visited sub-routes bundled with the feature's main chunk; split only where a sub-route is genuinely heavy or rarely visited

- **Leaving a heavy, rarely-used dependency (e.g. a PDF/report generation library) in the feature's main chunk instead of splitting it off**
  - Consequence: inflates the feature's chunk for all users of that feature, even the majority who never touch the heavy sub-page
  - Instead: split that specific sub-route via `loadComponent` so its dependency is only fetched when actually needed

# Check list

- [ ] The feature's own chunk has a declared, enforced bundle budget
- [ ] Any sub-route pulling in a large, rarely-needed dependency is split via `loadComponent`
- [ ] No sub-route is split via `loadComponent` purely by default, without a size or usage-frequency justification

# Unittest TestCases

- [ ] WHEN the feature's chunk is built THEN
  - [ ] it stays within its declared bundle budget
- [ ] WHEN a `loadComponent`-split sub-route is navigated to THEN
  - [ ] its dependency is fetched in a separate chunk, not present in the feature's main chunk
