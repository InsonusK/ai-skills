---
name: plateau-multiuser-monolith--class-feature-routes
description: Generic pattern for a feature's own root-relative Routes array — applies to any {Feature}/feature lib. VP1 adds loadComponent sub-splitting for heavy/rare sub-routes plus a per-chunk bundle budget; VP5 a parent route with `providers: [provide{Feature}OfflineSync()]`; VP7 `canActivate: [requirePermission('...')]` on a protected sub-route. — multiuser-monolith plateau
domain: skill
type: template
whenToUse: when writing or reviewing a feature's {feature}.routes.ts — root-relative paths, loadComponent splits, route-level providers, permission guards
plateau: multiuser-monolith
artifact_type: module
version: 20260903150000
tags:
  - skill/template/class
  - plateau/multiuser-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"

> Generic pattern, not tied to one concrete feature. `solution-forms` and `solution-api-http-layer` don't touch a feature's routes; `solution-performance-tuned-routing` (VP1) adds the `loadComponent` sub-splitting rule; `solution-offline-sync` (VP5) wraps the feature's routes in a parent route whose `providers` carry `provide{Feature}OfflineSync()`; `solution-authentication` (VP7) attaches `requirePermission('...')` as a `canActivate` / `canMatch` guard on any route the feature wants to protect — the guard factory itself lives in `libs/shared/auth-ui`, only its attachment is here.

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
// Plateau: multiuser-monolith
// Version: 20260903150000

// orders.routes.ts
export const ORDERS_ROUTES: Routes = [
  {
    path: '',
    // VP5: the feature's replay handler, registered in a route-level env injector —
    // never in the shell's app.config.ts (that would static-import the lazy feature).
    providers: [provideOrdersOfflineSync()],
    children: [
      // main path — bundled with the feature's own chunk
      { path: '', component: OrdersListComponent },
      { path: ':id', component: OrderDetailComponent },
      // rarely visited, pulls in a heavy PDF/report dependency — its own chunk
      {
        path: ':id/print-label',
        loadComponent: () =>
          import('./print-label/print-label.component').then((m) => m.PrintLabelComponent),
      },
      // VP7: a route this feature restricts by permission. requirePermission(...) is
      // imported from @org/shared-auth-ui; only the attachment lives here. A failed
      // check redirects to /forbidden (the factory returns a UrlTree).
      {
        path: 'archive',
        canActivate: [requirePermission('orders.archive')],
        loadComponent: () =>
          import('./order-archive/order-archive.component').then((m) => m.OrderArchiveComponent),
      },
    ],
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
- **VP5** — `provide{Feature}OfflineSync()` must be placed on a parent route's `providers` inside `{feature}.routes.ts` (a route-level env injector), never in the shell's `app.config.ts` — a static import of the lazy feature there would pull it into the initial bundle.
- **VP7** — a route this feature restricts by permission must carry `canActivate: [requirePermission('<permission-string>')]` (and `canMatch` when the route should be invisible, not just blocked). The permission argument is a permission string, never a role name. The `requirePermission` factory is imported from `@org/shared-auth-ui` — never redefined per feature — and only its attachment lives here; a failed check redirects to `/forbidden`, it never silently fails navigation.
- **VP7** — a permission guard must never be centralized in `apps/platform-shell`'s `app.routes.ts`; it is attached at the exact feature-owned path it protects, consistent with hierarchical route ownership.

## SHOULD
- Keep the feature's main/landing path in the feature's own chunk (not split further) unless it independently exceeds its budget.
- Never split every sub-route via `loadComponent` by default — each split is an extra network round-trip on first visit; split only where it avoids meaningfully more weight than that round-trip costs.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create.md|Routing/{feature}.guard.ts.create]]


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
- [ ] `provide{Feature}OfflineSync()` (VP5), if the feature queues mutations, sits on a route's `providers` here — not in `app.config.ts`
- [ ] A permission-restricted route (VP7) carries `canActivate: [requirePermission('<string>')]` with `requirePermission` imported from `@org/shared-auth-ui`; no permission guard is defined in `apps/platform-shell`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Unittest TestCases

- [ ] WHEN a feature's exported `Routes` array is inspected THEN
  - [ ] no path in it includes the feature's own mount segment
  - [ ] no route sets `data: { preload: true }`
- [ ] WHEN a `loadComponent`-split sub-route is navigated to THEN
  - [ ] its dependency is fetched in a separate chunk, not present in the feature's main chunk
- [ ] WHEN a user without `orders.archive` navigates to the guarded route THEN
  - [ ] navigation is redirected to `/forbidden` (the guard returns a `UrlTree`), never a silent no-op
- [ ] WHEN a user with `orders.archive` navigates to the guarded route THEN
  - [ ] navigation proceeds

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]
