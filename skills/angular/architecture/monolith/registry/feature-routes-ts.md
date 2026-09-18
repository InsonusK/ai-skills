---
name: registry-feature-routes-ts
description: Conflict Detection result for the `feature-routes-ts` element
tags:
  - concern/architecture
  - stack/typescript
  - element/feature-routes-ts
---

# Element
`{feature}.routes.ts` — a feature's own root-relative `Routes` array in `libs/{feature}/feature/src/lib/`, exported from `index.ts`.

# Involved solutions
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] (`.create` — `FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create` — the feature's root-relative routes; the feature as a whole is lazy via `loadChildren` at its mount point)
- [[skills/angular/architecture/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] (VP1, `.extend` — switches a heavy / rarely-visited sub-route from `component:` to `loadComponent:` and declares the per-chunk budget)
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] (VP5, `.extend` — `Repository.extend` — wraps the feature's routes in a parent route whose `providers: [provide{Feature}OfflineSync()]` registers the feature's replay handler in a route-level env injector)
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] (VP7, `.extend` — `Routing/{feature}.guard.ts.create` — attaches `canActivate: [requirePermission('...')]` / `canMatch` on a route the feature restricts; the `requirePermission` factory itself lives in `libs/shared/auth-ui`)

# Classification
`FMN` / `TMN` — single-direction refine. Category `M` (code changes to the routes array). Kind `N` (independent):
- `performance-tuned-routing` only changes the *loading mechanism* of one sub-route (`component` → `loadComponent`).
- `offline-sync` only adds a `providers` array on a wrapping parent route — it reorders nothing and touches no child route's definition.
- `authentication` only adds a `canActivate` / `canMatch` property to one route object.

No two write the same route object property. Each of `performance-tuned-routing`, `offline-sync` and `authentication` records a `depends_on` (directly or transitively via VP3/VP2) that fixes the create-before-extend order.

# Ordering
`source: ordering-only` — `HierarchicalRouting` is a common baseline feature, not a VP, so there is no VP↔VP Feature-Model constraint here. The create-then-extend order lives solely in each extending solution's `depends_on`.

# Resolution
**Canonical — no resolver.** The `plateau-multiuser-monolith` example's `orders.routes.ts` demonstrates all four: a parent route with `providers: [provideOrdersOfflineSync()]` (VP5) and `children` holding the main path (`component:` from `solution-app-routing`), a `report` sub-route split via `loadComponent` (VP1), and an `archive` sub-route with `canActivate: [requirePermission('orders.archive')]` (VP7). `orders.routes.spec.ts` guards that no route self-sets `data.preload`, that the split sub-route is `loadComponent`, that the offline-sync provider is registered, and that the guarded route redirects to `/forbidden` without the permission.

# Architectural signal
N = 4. **Benign.** A feature's routes array is the natural attachment point for per-route concerns (lazy split, route-scoped providers, guards) — each solution adds a distinct, non-overlapping property. Not a mis-drawn VP.

# Growth history
| Plateau | N | What changed | Verified |
| --- | --- | --- | --- |
| `plateau-async-monolith` | 2 | First real: `solution-app-routing` + `solution-performance-tuned-routing` (VP1) | `orders.routes.ts` keeps the base path and adds a `loadComponent`-split `report` sub-route; production build confirms the sub-route is its own lazy chunk |
| `plateau-multiuser-monolith` | 4 | `solution-offline-sync` (VP5) and `solution-authentication` (VP7) join | `orders.routes.ts` adds the offline-sync provider wrapper and a guarded `archive` sub-route; `orders.routes.spec.ts` covers all four concerns |
