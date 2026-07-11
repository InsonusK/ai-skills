---
name: project-feature-feature
description: Generic template for any {feature}/feature lib — routed, presentational + container components (including Signal Forms) for one feature, its feature-level Signal Store, and its own root-relative routes with lazy sub-splitting
domain: skill
type: template
plateau: data-capable
project_kind: library
version: 20260711140000
tags:
  - skill/template/project
  - plateau/data-capable
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]]"
---

> Generic pattern, not tied to one concrete feature — every business feature added to the workspace gets its own `libs/{feature}/feature` project following this template, substituting `{Feature}`/`{feature}` with the real feature name.

# Goal

- Colocate a feature's state with its components inside `libs/{feature}/feature`, keeping feature-level state encapsulated behind the workspace's module boundaries
- Avoid classical NgRx action/reducer/effect boilerplate for state owned by exactly one feature
- Define the feature's internal navigation entirely relative to its own root, so the feature can be mounted anywhere by any parent without modification
- Avoid pulling rarely-visited or heavy sub-pages into the feature's own lazy chunk when they can be split into their own, separately-loaded chunk
- Build any form inside this feature as a Signal Form, giving components synchronous, fine-grained access to field-level validity/touched/error state

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]

# Structure

## Project Structure

```
/libs/{feature}
  /feature
    /src
      /lib
        [{feature}.store.ts](./classes/class-feature-store.skill.md)
        [{feature}.routes.ts](./classes/class-feature-routes.skill.md)
        /{form-name}
          [{form-name}.component.ts](./classes/class-form-component.skill.md)
          {form-name}.form.ts        <- optional, only for non-trivial field schemas
      index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| `{feature}.store.ts` | Feature-level NgRx Signal Store. Owns all state and derived data specific to this feature. | [[classes/class-feature-store.skill.md\|class-feature-store.skill]] |
| `{feature}.routes.ts` | Feature's own root-relative `Routes` array, exported from `index.ts`. Sub-routes that are heavy or rarely visited are split via `loadComponent`. | [[classes/class-feature-routes.skill.md\|class-feature-routes.skill]] |
| `{form-name}.component.ts` (+ optional `{form-name}.form.ts`) | Any component in this feature that renders a form — built with Signal Forms (`form()`/`FieldTree`), submits via `submitForm()` into this feature's data-access Facade. | [[classes/class-form-component.skill.md\|class-form-component.skill]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]

## What Does NOT Belong Here

- HTTP client wiring / DTO mapping — belongs in this feature's own `data-access` lib
- Cross-cutting state (auth, notifications, offline-sync) — belongs in `libs/shared/state`
- A hardcoded assumption of the feature's own mount segment — that is assigned by whoever mounts it
- A raw `HttpClient` call inside a form's submit handler — the form calls the store, which calls the Facade

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]

## Allowed Dependencies

- `libs/{feature}/data-access` (same scope)
- `libs/shared/ui` (tag: `type:ui`, `scope:shared`)
- `libs/shared/util` (tag: `type:util`, `scope:shared`)
- `libs/shared/state` (tag: `type:store`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST
- The feature Signal Store MUST live inside `libs/{feature}/feature`, not in a shared location.
- The feature Signal Store MUST be the only place that feature's components read/write feature-owned state.
- `{feature}.routes.ts` paths MUST be defined relative to the feature's own root only.
- `{FEATURE}_ROUTES` MUST be exported from `index.ts`.
- This project MUST declare a per-chunk bundle budget for its own lazy chunk.
- A form's submission MUST go through `submitForm()`, not a manually wired handler that bypasses the form's own validation/state.
- Any HTTP call triggered by form submission MUST go through the owning feature's `data-access` Facade, never call `HttpClient` directly from the form component.
- A custom, design-system-provided form control used inside a Signal Form MUST implement `ControlValueAccessor`.

## SHOULD
- A sub-route SHOULD be split via `loadComponent` when it pulls in a dependency not needed by the feature's main path.
- Field schema/validators SHOULD stay inline in the component for simple forms, and SHOULD be extracted into a `{form-name}.form.ts` file once cross-field validation (`when`) or the number of validators makes the component harder to read.
- Async validation SHOULD use `validateHttp()` with its built-in debounce, rather than a hand-rolled debounced subscription.

## SHOULD NOT
- Splitting SHOULD NOT be applied to every sub-route indiscriminately.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]

# Anti-patterns

- **Placing a feature Signal Store in `libs/shared/state` or another feature's lib**
  - Consequence: feature state leaks across module boundaries
  - Instead: keep the store colocated with the feature it belongs to
- **Baking the feature's expected mount segment into its own route paths**
  - Consequence: breaks the moment the feature is mounted under a different segment
  - Instead: paths are always relative to the feature's own root
- **Wiring a raw `HttpClient` call directly inside a form component's submit handler**
  - Consequence: bypasses the feature's data-access Facade, duplicating error handling and mapping logic
  - Instead: call through the feature's Facade from inside `submitForm()`'s callback (typically via the feature's store)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]

# Check list

- [ ] `libs/{feature}/feature` contains exactly one feature-level Signal Store for that feature
- [ ] The store is exported from the lib's `index.ts`
- [ ] No path inside `{feature}.routes.ts` includes the feature's own name or an assumed prefix
- [ ] Every form is built with `form()`/`FieldTree`, not `FormGroup`/`FormControl`
- [ ] Every form's submission goes through `submitForm()`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
