---
name: project-feature-feature
description: Generic template for any {feature}/feature lib — routed presentational + container components, Signal Forms, feature-level Signal Store, and the feature's own routes/guards
domain: skill
type: template
plateau: offline-app
project_kind: library
version: 20260711140000
tags:
  - skill/template/project
  - plateau/offline-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
  - "[[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]]"
---

> Generic pattern, not tied to one concrete feature — every business feature added to the workspace gets its own `libs/{feature}/feature` project following this template.

# Goal

- Colocate a feature's state with its components inside `libs/{feature}/feature`, keeping feature-level state encapsulated
- Define the feature's internal navigation entirely relative to its own root, so it can be mounted anywhere by any parent
- Build forms as Signal Forms by default, and restrict specific routes by permission where required

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]

# Core Principles

- The feature Signal Store is the only place that feature's components read/write feature-owned state; it calls only the feature's own Facade
- Route paths are always relative to the feature's own root; the mounting project assigns the segment
- A sub-route is split into its own `loadComponent` chunk only when it is rarely visited or heavy enough to justify a separate request
- New forms use Signal Forms (`@angular/forms/signals`) by default
- Permission guards are attached at the feature's own route, using the shared `requirePermission` factory

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|FeatureStore/{Feature}.project.extend/{feature}.store.ts.create]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Structure

## Workspace place

```
/libs
  /{feature}
    /feature
```

## Project Structure

```
/libs/{feature}/feature
  /src
    /lib
      [{feature}.store.ts](./classes/class-feature-store.skill.md)
      [{feature}.routes.ts](./classes/class-feature-routes.skill.md)
      /{component-name}
        [{component-name}.component.ts](./classes/class-component-name-component.skill.md)
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| `{feature}.store.ts` | Feature-level NgRx Signal Store. Owns all state and derived data specific to this feature. | [[classes/class-feature-store.skill.md\|class-feature-store.skill]] |
| `{feature}.routes.ts` | The feature's own root-relative `Routes`, optionally guarded by `requirePermission(...)` | [[classes/class-feature-routes.skill.md\|class-feature-routes.skill]] |
| `{component-name}.component.ts` | Any presentational/container/form component owned by this feature | [[classes/class-component-name-component.skill.md\|class-component-name-component.skill]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]

## What Does NOT Belong Here

- HTTP client wiring / DTO mapping — belongs in this feature's own `data-access` lib
- Cross-cutting state (auth, connectivity, notifications) — belongs in `libs/shared/state`
- Any durable, persisted mutation queue — belongs in `libs/shared/offline-sync`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

## Allowed Dependencies

- `libs/{feature}/data-access` (same scope)
- `libs/shared/ui`, `libs/shared/util`, `libs/shared/state`, `libs/shared/auth-ui` (tag: `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]

# Rules

## MUST
- [[../repo-offline-app.skill.md#MUST|repo-offline-app]]
- The feature Signal Store MUST live inside `libs/{feature}/feature`, and MUST be the only place that feature's components read/write feature-owned state.
- `{feature}.routes.ts` paths MUST be defined relative to the feature's own root only.
- `{FEATURE}_ROUTES` MUST be exported from `index.ts`.
- A feature's own chunk MUST declare a per-chunk bundle budget; a sub-route split via `loadComponent` gets its own separate budget.
- A permission guard MUST be attached inside this feature's own routes, never centralized in the shell.

## SHOULD
- A sub-route SHOULD be split via `loadComponent` when it pulls in a dependency not needed by the feature's main path.
- Field schema/validators SHOULD stay inline for simple forms, and SHOULD be extracted into a `{form-name}.form.ts` file once cross-field validation grows.

## SHOULD NOT
- Splitting SHOULD NOT be applied to every sub-route indiscriminately.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Anti-patterns

- **Placing a feature Signal Store in `libs/shared/state` or another feature's lib**
  - Consequence: feature state leaks across module boundaries
  - Instead: keep the store colocated with the feature it belongs to
- **Baking the feature's expected mount segment into its own route paths**
  - Consequence: breaks the moment the feature is mounted under a different segment
  - Instead: paths are always relative to the feature's own root
- **Centralizing all permission guards in the shell's root routes "for visibility"**
  - Consequence: reintroduces the coupling hierarchical route ownership was designed to prevent
  - Instead: each feature attaches its own guards to its own routes

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Check list

- [ ] `libs/{feature}/feature` contains exactly one feature-level Signal Store for that feature
- [ ] The store and the feature's `Routes` are both exported from the lib's `index.ts`
- [ ] No path inside `{feature}.routes.ts` includes the feature's own name or an assumed prefix

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
