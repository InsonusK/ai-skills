---
name: plateau-online-monolith--project-feature-feature
description: Generic template for any {feature}/feature lib — routed, presentational + container components (including Signal Forms) for one feature, its feature-level Signal Store, its own root-relative routes, unit/component tested via Vitest + Testing Library. No lazy sub-splitting, no permission guards yet. — online-monolith plateau
domain: skill
type: template
plateau: online-monolith
project_kind: library
version: 20260711180000
tags:
  - skill/template/project
  - plateau/online-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]]"
  - "[[skills/angular/architecture/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]]"
  - "[[skills/angular/architecture/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]]"
---

> Generic pattern, not tied to one concrete feature — every business feature added to the workspace gets its own `libs/{feature}/feature` project following this template, substituting `{Feature}`/`{feature}` with the real feature name.

# Goal

- Colocate a feature's state with its components inside `libs/{feature}/feature`, keeping feature-level state encapsulated behind the workspace's module boundaries
- Avoid classical NgRx action/reducer/effect boilerplate for state owned by exactly one feature
- Define the feature's internal navigation entirely relative to its own root, so the feature can be mounted anywhere by any parent without modification
- Build any form inside this feature as a Signal Form, giving components synchronous, fine-grained access to field-level validity/touched/error state
- Give every component and Signal Store a fast, DOM-accurate test, without ever hitting real HTTP

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/solutions/solution-app-testing.skill/Implementation/Repository.extend|Repository.extend]]

# Structure

## Project Structure

```
/libs/{feature}
  /feature
    /src
      /lib
        [{feature}.store.ts](./classes/plateau-online-monolith--class-feature-store.skill.md)
        {feature}.store.spec.ts
        [{feature}.routes.ts](./classes/plateau-online-monolith--class-feature-routes.skill.md)
        /{form-name}
          [{form-name}.component.ts](./classes/plateau-online-monolith--class-form-component.skill.md)
          {form-name}.component.spec.ts
          {form-name}.form.ts        <- optional, only for non-trivial field schemas
      index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| `{feature}.store.ts` | Feature-level NgRx Signal Store. Owns all state and derived data specific to this feature. | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/feature-feature/classes/plateau-online-monolith--class-feature-store.skill\|class-feature-store]] |
| `{feature}.store.spec.ts` | Vitest unit test faking the feature's Facade. | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/feature-feature/classes/plateau-online-monolith--class-feature-store.skill\|class-feature-store]] |
| `{feature}.routes.ts` | Feature's own root-relative `Routes` array, exported from `index.ts`. | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/feature-feature/classes/plateau-online-monolith--class-feature-routes.skill\|class-feature-routes]] |
| `{form-name}.component.ts` (+ optional `{form-name}.form.ts`) | Any component in this feature that renders a form. | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/feature-feature/classes/plateau-online-monolith--class-form-component.skill\|class-form-component]] |
| `{form-name}.component.spec.ts` | Testing Library component test, faking the component's Signal Store. | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/feature-feature/classes/plateau-online-monolith--class-form-component.skill\|class-form-component]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @testing-library/angular | latest compatible | Component tests interacting through the rendered DOM |
| @testing-library/user-event | latest compatible | Simulated user interaction in component tests |
| vitest | matching workspace configuration | Unit/component test runner |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]

## What Does NOT Belong Here

- HTTP client wiring / DTO mapping — belongs in this feature's own `data-access` lib
- Cross-cutting state (auth skeleton, notifications) — belongs in `libs/shared/state`
- A hardcoded assumption of the feature's own mount segment — that is assigned by whoever mounts it
- A raw `HttpClient` call inside a form's submit handler — the form calls the store, which calls the Facade
- `HttpTestingController` usage in any spec in this project — that belongs only in the sibling `data-access` lib's Client spec

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/solutions/solution-app-testing.skill/Implementation/Repository.extend|Repository.extend]]

## Allowed Dependencies

- `libs/{feature}/data-access` (same scope)
- `libs/shared/ui` (tag: `type:ui`, `scope:shared`)
- `libs/shared/util` (tag: `type:util`, `scope:shared`)
- `libs/shared/state` (tag: `type:store`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST
- The feature Signal Store MUST live inside `libs/{feature}/feature`, not in a shared location.
- The feature Signal Store MUST be the only place that feature's components read/write feature-owned state.
- `{feature}.routes.ts` paths MUST be defined relative to the feature's own root only.
- `{FEATURE}_ROUTES` MUST be exported from `index.ts`.
- A form's submission MUST go through `submitForm()`.
- Any HTTP call triggered by form submission MUST go through the owning feature's `data-access` Facade.
- Every project in this lib MUST run its unit/component tests via Vitest.
- A component test MUST query and interact with the rendered DOM via Testing Library, not `fixture.componentInstance`/`debugElement`.
- A component test MUST fake the component's Signal Store (or Facade) — it MUST NOT use `HttpTestingController` or let a real HTTP call occur.
- A Signal Store test MUST fake its Facade directly — it MUST NOT reach further down to fake the Client or mock HTTP.

## SHOULD
- Field schema/validators SHOULD stay inline in the component for simple forms.
- Component test queries SHOULD prefer accessible roles/labels (`getByRole`, `getByLabelText`) over test-id attributes.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]

# Anti-patterns

- **Placing a feature Signal Store in `libs/shared/state` or another feature's lib**
  - Consequence: feature state leaks across module boundaries
  - Instead: keep the store colocated with the feature it belongs to
- **Baking the feature's expected mount segment into its own route paths**
  - Consequence: breaks the moment the feature is mounted under a different segment
  - Instead: paths are always relative to the feature's own root
- **Wiring a raw `HttpClient` call directly inside a form component's submit handler**
  - Consequence: bypasses the feature's data-access Facade
  - Instead: call through the feature's Facade from inside `submitForm()`'s callback
- **A Signal Store test faking the Client instead of the Facade**
  - Consequence: skips exercising the Facade's own business validation, couples the store test to an implementation detail two layers below what it's testing
  - Instead: always fake the layer directly beneath the unit under test

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]

# Check list

- [ ] `libs/{feature}/feature` contains exactly one feature-level Signal Store for that feature
- [ ] The store is exported from the lib's `index.ts`
- [ ] No path inside `{feature}.routes.ts` includes the feature's own name or an assumed prefix
- [ ] Every form is built with `form()`/`FieldTree`, not `FormGroup`/`FormControl`
- [ ] Every component and Signal Store in this project has a Vitest spec, none of which use `HttpTestingController`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/solutions/solution-app-testing.skill/Implementation/Repository.extend|Repository.extend]]
