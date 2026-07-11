---
name: project-feature-feature
description: Generic template for any {feature}/feature lib — routed, presentational + container components (including Signal Forms) for one feature, its feature-level Signal Store, its own root-relative routes (with lazy sub-splitting and permission guards), all protected consistently with the platform's auth model, and unit/component tested via Vitest + Testing Library
domain: skill
type: template
plateau: tested
project_kind: library
version: 20260711170000
tags:
  - skill/template/project
  - plateau/tested
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
  - "[[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]]"
---

> Generic pattern, not tied to one concrete feature — every business feature added to the workspace gets its own `libs/{feature}/feature` project following this template, substituting `{Feature}`/`{feature}` with the real feature name.

# Goal

- Colocate a feature's state with its components inside `libs/{feature}/feature`, keeping feature-level state encapsulated behind the workspace's module boundaries
- Avoid classical NgRx action/reducer/effect boilerplate for state owned by exactly one feature
- Define the feature's internal navigation entirely relative to its own root, so the feature can be mounted anywhere by any parent without modification
- Avoid pulling rarely-visited or heavy sub-pages into the feature's own lazy chunk when they can be split into their own, separately-loaded chunk
- Build any form inside this feature as a Signal Form, giving components synchronous, fine-grained access to field-level validity/touched/error state
- Restrict navigation into any of this feature's own routes based on a required permission, reusing the platform's shared permission model
- Give every component and Signal Store a fast, DOM-accurate test, without ever hitting real HTTP

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]

# Structure

## Project Structure

```
/libs/{feature}
  /feature
    /src
      /lib
        [{feature}.store.ts](./classes/class-feature-store.skill.md)
        {feature}.store.spec.ts
        [{feature}.routes.ts](./classes/class-feature-routes.skill.md)
        [permission.guard.ts (as needed)](./classes/class-feature-guard.skill.md)
        /{form-name}
          [{form-name}.component.ts](./classes/class-form-component.skill.md)
          {form-name}.component.spec.ts
          {form-name}.form.ts        <- optional, only for non-trivial field schemas
      index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| `{feature}.store.ts` | Feature-level NgRx Signal Store. Owns all state and derived data specific to this feature. | [[classes/class-feature-store.skill.md\|class-feature-store.skill]] |
| `{feature}.store.spec.ts` | Vitest unit test faking the feature's Facade. | [[classes/class-feature-store.skill.md\|class-feature-store.skill]] |
| `{feature}.routes.ts` | Feature's own root-relative `Routes` array, exported from `index.ts`. | [[classes/class-feature-routes.skill.md\|class-feature-routes.skill]] |
| `permission.guard.ts` usage | Restricts navigation into a specific route of this feature based on a required permission. | [[classes/class-feature-guard.skill.md\|class-feature-guard.skill]] |
| `{form-name}.component.ts` (+ optional `{form-name}.form.ts`) | Any component in this feature that renders a form. | [[classes/class-form-component.skill.md\|class-form-component.skill]] |
| `{form-name}.component.spec.ts` | Testing Library component test, faking the component's Signal Store. | [[classes/class-form-component.skill.md\|class-form-component.skill]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @testing-library/angular | latest compatible | Component tests interacting through the rendered DOM |
| @testing-library/user-event | latest compatible | Simulated user interaction in component tests |
| vitest | matching workspace configuration | Unit/component test runner |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]

## What Does NOT Belong Here

- HTTP client wiring / DTO mapping — belongs in this feature's own `data-access` lib
- Cross-cutting state (auth, notifications, offline-sync) — belongs in `libs/shared/state`
- A hardcoded assumption of the feature's own mount segment — that is assigned by whoever mounts it
- A raw `HttpClient` call inside a form's submit handler — the form calls the store, which calls the Facade
- Role-name-based checks (`role === 'admin'`) — every authorization check is a permission string
- `HttpTestingController` usage in any spec in this project — that belongs only in the sibling `data-access` lib's Client spec

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]

## Allowed Dependencies

- `libs/{feature}/data-access` (same scope)
- `libs/shared/ui` (tag: `type:ui`, `scope:shared`)
- `libs/shared/util` (tag: `type:util`, `scope:shared`)
- `libs/shared/state` (tag: `type:store`, `scope:shared`)
- `libs/shared/auth-ui` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]

# Rules

## MUST
- The feature Signal Store MUST live inside `libs/{feature}/feature`, not in a shared location.
- The feature Signal Store MUST be the only place that feature's components read/write feature-owned state.
- `{feature}.routes.ts` paths MUST be defined relative to the feature's own root only.
- `{FEATURE}_ROUTES` MUST be exported from `index.ts`.
- This project MUST declare a per-chunk bundle budget for its own lazy chunk.
- A form's submission MUST go through `submitForm()`.
- Any HTTP call triggered by form submission MUST go through the owning feature's `data-access` Facade.
- A permission guard MUST be attached inside this feature's own routes, at the specific path it protects.
- A failed permission check MUST redirect to a forbidden/not-authorized route.
- Every project in this lib MUST run its unit/component tests via Vitest.
- A component test MUST query and interact with the rendered DOM via Testing Library, not `fixture.componentInstance`/`debugElement`.
- A component test MUST fake the component's Signal Store (or Facade) — it MUST NOT use `HttpTestingController` or let a real HTTP call occur.
- A Signal Store test MUST fake its Facade directly — it MUST NOT reach further down to fake the Client or mock HTTP.

## SHOULD
- A sub-route SHOULD be split via `loadComponent` when it pulls in a dependency not needed by the feature's main path.
- Field schema/validators SHOULD stay inline in the component for simple forms.
- Component test queries SHOULD prefer accessible roles/labels (`getByRole`, `getByLabelText`) over test-id attributes.

## SHOULD NOT
- Splitting SHOULD NOT be applied to every sub-route indiscriminately.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create|Testing/{feature}.facade-and-store.spec.ts.create]]

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
- **Centralizing all permission guards in the shell's root routes "for visibility"**
  - Consequence: reintroduces the coupling hierarchical route ownership was designed to prevent
  - Instead: each feature attaches its own guards to its own routes
- **A Signal Store test faking the Client instead of the Facade**
  - Consequence: skips exercising the Facade's own business validation, couples the store test to an implementation detail two layers below what it's testing
  - Instead: always fake the layer directly beneath the unit under test
- **Asserting against `fixture.componentInstance.someSignal()` instead of the rendered DOM**
  - Consequence: couples the test to internal implementation — a safe refactor breaks the test for no real reason
  - Instead: assert what `screen` shows, exactly as a user would perceive it

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create|Testing/{feature}.facade-and-store.spec.ts.create]]

# Check list

- [ ] `libs/{feature}/feature` contains exactly one feature-level Signal Store for that feature
- [ ] The store is exported from the lib's `index.ts`
- [ ] No path inside `{feature}.routes.ts` includes the feature's own name or an assumed prefix
- [ ] Every form is built with `form()`/`FieldTree`, not `FormGroup`/`FormControl`
- [ ] Every route requiring a specific permission uses `requirePermission(...)` at that feature's own route definition
- [ ] Every component and Signal Store in this project has a Vitest spec, none of which use `HttpTestingController`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]
