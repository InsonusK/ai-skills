---
name: plateau-offline-monolith--project-feature-feature
description: Generic template for any {feature}/feature lib — routed presentational + container components (Signal Forms), feature-level Signal Store, own root-relative routes, now with optional per-sub-route lazy splitting. No permission guards yet. — offline-monolith plateau
domain: skill
type: template
plateau: offline-monolith
project_kind: library
version: 20260711200000
tags:
  - skill/template/project
  - plateau/offline-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill.md|solution-lazy-loading-routing]]"
---

> Generic pattern, not tied to one concrete feature — every business feature added to the workspace gets its own `libs/{feature}/feature` project following this template. No permission guards yet — attaching one to `{feature}.routes.ts` arrives with [[skills/angular/architecture/plateau/plateau-multiuser-app/plateau-multiuser-app.skill.md|multiuser-app]].

# Goal

- Colocate a feature's state with its components inside `libs/{feature}/feature`, keeping feature-level state encapsulated
- Define the feature's internal navigation entirely relative to its own root, so it can be mounted anywhere by any parent
- Split a sub-route into its own lazy chunk when it is rarely visited or heavy enough to justify a separate request

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill.md|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Core Principles

- The feature Signal Store is the only place that feature's components read/write feature-owned state; it calls only the feature's own Facade
- Route paths are always relative to the feature's own root; the mounting project assigns the segment
- A sub-route is split into its own `loadComponent` chunk only when it is rarely visited or heavy enough to justify a separate request — never every sub-route indiscriminately

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill.md|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Structure

## Project Structure

```
/libs/{feature}/feature
  /src
    /lib
      [{feature}.store.ts](./classes/plateau-offline-monolith--class-feature-store.skill.md)
      {feature}.store.spec.ts
      [{feature}.routes.ts](./classes/plateau-offline-monolith--class-feature-routes.skill.md)
      /{form-name}
        [{form-name}.component.ts](./classes/plateau-offline-monolith--class-form-component.skill.md)
        {form-name}.component.spec.ts
        {form-name}.form.ts        <- optional, only for non-trivial field schemas
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| `{feature}.store.ts` | Feature-level NgRx Signal Store. Owns all state and derived data specific to this feature. | [[skills/angular/architecture/plateau/plateau-offline-monolith/structure/feature-feature/classes/plateau-offline-monolith--class-feature-store.skill\|class-feature-store]] |
| `{feature}.routes.ts` | The feature's own root-relative `Routes`, optionally splitting a heavy sub-route into its own `loadComponent` chunk | [[skills/angular/architecture/plateau/plateau-offline-monolith/structure/feature-feature/classes/plateau-offline-monolith--class-feature-routes.skill\|class-feature-routes]] |
| `{form-name}.component.ts` (+ optional `{form-name}.form.ts`) | Any component in this feature that renders a form. | [[skills/angular/architecture/plateau/plateau-offline-monolith/structure/feature-feature/classes/plateau-offline-monolith--class-form-component.skill\|class-form-component]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill.md|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

## What Does NOT Belong Here

- HTTP client wiring / DTO mapping — belongs in this feature's own `data-access` lib
- Cross-cutting state (connectivity, notifications) — belongs in `libs/shared/state`
- Any durable, persisted queue implementation — belongs in `libs/shared/offline-sync`
- A permission guard — that arrives with [[skills/angular/architecture/plateau/plateau-multiuser-app/plateau-multiuser-app.skill.md|multiuser-app]]

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill.md|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

## Allowed Dependencies

- `libs/{feature}/data-access` (same scope)
- `libs/shared/ui`, `libs/shared/util`, `libs/shared/state` (tag: `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill.md|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Rules

## MUST
- [[skills/angular/architecture/plateau/plateau-offline-monolith/structure/plateau-offline-monolith--repo-offline-monolith.skill#MUST|repo-offline-monolith]]
- The feature Signal Store MUST live inside `libs/{feature}/feature`, and MUST be the only place that feature's components read/write feature-owned state.
- `{feature}.routes.ts` paths MUST be defined relative to the feature's own root only.
- `{FEATURE}_ROUTES` MUST be exported from `index.ts`.
- A feature's own chunk MUST declare a per-chunk bundle budget; a sub-route split via `loadComponent` gets its own separate budget.

## SHOULD
- A sub-route SHOULD be split via `loadComponent` when it pulls in a dependency not needed by the feature's main path.
- Field schema/validators SHOULD stay inline for simple forms, and SHOULD be extracted into a `{form-name}.form.ts` file once cross-field validation grows.

## SHOULD NOT
- Splitting SHOULD NOT be applied to every sub-route indiscriminately.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill.md|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Anti-patterns

- **Placing a feature Signal Store in `libs/shared/state` or another feature's lib**
  - Consequence: feature state leaks across module boundaries
  - Instead: keep the store colocated with the feature it belongs to
- **Baking the feature's expected mount segment into its own route paths**
  - Consequence: breaks the moment the feature is mounted under a different segment
  - Instead: paths are always relative to the feature's own root
- **Splitting every sub-route into its own `loadComponent` chunk "for consistency"**
  - Consequence: excessive request fan-out for routes too small to benefit
  - Instead: split only rarely-visited or genuinely heavy sub-routes

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill.md|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]

# Check list

- [ ] `libs/{feature}/feature` contains exactly one feature-level Signal Store for that feature
- [ ] The store and the feature's `Routes` are both exported from the lib's `index.ts`
- [ ] No path inside `{feature}.routes.ts` includes the feature's own name or an assumed prefix
- [ ] Every `loadComponent`-split sub-route declares its own bundle budget

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill.md|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]
