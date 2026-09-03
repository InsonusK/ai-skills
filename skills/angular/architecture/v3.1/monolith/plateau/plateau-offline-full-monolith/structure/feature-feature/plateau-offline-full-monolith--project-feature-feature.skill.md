---
name: plateau-offline-full-monolith--project-feature-feature
description: Generic template for any {feature}/feature lib — routed, presentational + container components (including Signal Forms) for one feature, its feature-level Signal Store, its own root-relative routes with loadComponent sub-splitting for heavy/rare sub-routes, unit/component tested via Vitest + Testing Library. No permission guards yet. — offline-full-monolith plateau
domain: skill
type: template
plateau: offline-full-monolith
project_kind: library
version: 20260903120000
tags:
  - skill/template/project
  - plateau/offline-full-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]]"

> Generic pattern, not tied to one concrete feature — every business feature added to the workspace gets its own `libs/{feature}/feature` project following this template, substituting `{Feature}`/`{feature}` with the real feature name. VP5 adds `{feature}.offline-sync.ts` (the replay handler, registered in the feature's route `providers`, wiring `onReplayStart` / `onReplayResult`), a per-row `syncStatus` (`queued → sending → synced | failed | conflict`) + `setSyncStatus` + `hydratePending` on the feature Signal Store, and a derived `pendingSyncCount`.

# Goal

- Colocate a feature's state with its components inside `libs/{feature}/feature`, keeping feature-level state encapsulated behind the workspace's module boundaries
- Avoid classical NgRx action/reducer/effect boilerplate for state owned by exactly one feature
- Define the feature's internal navigation entirely relative to its own root, so the feature can be mounted anywhere by any parent without modification
- Build any form inside this feature as a Signal Form, giving components synchronous, fine-grained access to field-level validity/touched/error state
- Give every component and Signal Store a fast, DOM-accurate test, without ever hitting real HTTP

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/FeatureStore/{Feature}.project.extend.md|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]] - [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend.md|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]

# Structure

## Project Structure

```
/libs/{feature}
  /feature
    /src
      /lib
        [{feature}.store.ts](./classes/plateau-offline-full-monolith--class-feature-store.skill.md)
        {feature}.store.spec.ts
        [{feature}.routes.ts](./classes/plateau-offline-full-monolith--class-feature-routes.skill.md)   <- loadComponent split (VP1); parent route `providers: [provide{Feature}OfflineSync()]` (VP5)
        {feature}.routes.spec.ts   <- asserts no self-set preload flag; split sub-route is loadComponent; route registers the offline-sync provider
        {feature}.offline-sync.ts   <- new (VP5): the FeatureReplay handler — replay() + onReplayStart/onReplayResult driving the store's per-row syncStatus
        /{form-name}
          [{form-name}.component.ts](./classes/plateau-offline-full-monolith--class-form-component.skill.md)
          {form-name}.component.spec.ts
          {form-name}.form.ts        <- optional, only for non-trivial field schemas
        /{heavy-subpage}
          {heavy-subpage}.component.ts   <- own loadComponent chunk; not exported from index.ts
      index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| `{feature}.store.ts` | Feature-level NgRx Signal Store. Owns all state and derived data specific to this feature. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-feature/classes/plateau-offline-full-monolith--class-feature-store.skill\|class-feature-store]] |
| `{feature}.store.spec.ts` | Vitest unit test faking the feature's Facade. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-feature/classes/plateau-offline-full-monolith--class-feature-store.skill\|class-feature-store]] |
| `{feature}.routes.ts` | Feature's own root-relative `Routes` array, exported from `index.ts`. Heavy/rare sub-routes are split via `loadComponent`; no route sets `data: { preload: true }`. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-feature/classes/plateau-offline-full-monolith--class-feature-routes.skill\|class-feature-routes]] |
| `{feature}.routes.spec.ts` | Vitest guard: no route self-sets `preload`; the heavy sub-route uses `loadComponent`, the main path does not. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-feature/classes/plateau-offline-full-monolith--class-feature-routes.skill\|class-feature-routes]] |
| `{form-name}.component.ts` (+ optional `{form-name}.form.ts`) | Any component in this feature that renders a form. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-feature/classes/plateau-offline-full-monolith--class-form-component.skill\|class-form-component]] |
| `{form-name}.component.spec.ts` | Testing Library component test, faking the component's Signal Store. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-feature/classes/plateau-offline-full-monolith--class-form-component.skill\|class-form-component]] |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/FeatureStore/{Feature}.project.extend.md|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]] - [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend.md|FormComponent/{component-name}.component.ts.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @testing-library/angular | latest compatible | Component tests interacting through the rendered DOM |
| @testing-library/user-event | latest compatible | Simulated user interaction in component tests |
| vitest | matching workspace configuration | Unit/component test runner |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create.md|Testing/{component-name}.component.spec.ts.create]]

## What Does NOT Belong Here

- HTTP client wiring / DTO mapping — belongs in this feature's own `data-access` lib
- Cross-cutting state (auth skeleton, notifications) — belongs in `libs/shared/state`
- A hardcoded assumption of the feature's own mount segment — that is assigned by whoever mounts it
- A raw `HttpClient` call inside a form's submit handler — the form calls the store, which calls the Facade
- `HttpTestingController` usage in any spec in this project — that belongs only in the sibling `data-access` lib's Client spec

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]] - [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend.md|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]

## Allowed Dependencies

- `libs/{feature}/data-access` (same scope)
- `libs/shared/ui` (tag: `type:ui`, `scope:shared`)
- `libs/shared/util` (tag: `type:util`, `scope:shared`)
- `libs/shared/state` (tag: `type:store`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]

# Rules

## MUST
- The feature Signal Store must live inside `libs/{feature}/feature`, not in a shared location.
- The feature Signal Store must be the only place that feature's components read/write feature-owned state.
- `{feature}.routes.ts` paths must be defined relative to the feature's own root only.
- `{FEATURE}_ROUTES` must be exported from `index.ts`.
- A sub-route that pulls a large, rarely-needed dependency must be split via `loadComponent`; the feature must never set `data: { preload: true }` on its own routes.
- If the feature has a queueable mutation, `{feature}.offline-sync.ts` must register a `FeatureReplay` via `provideFeatureReplay(...)` placed in the feature's own **route `providers`** (a route-level env injector) — never in the shell's `app.config.ts` (a static import of the lazy feature would pull it into the initial bundle).
- The feature Signal Store must carry a **per-row `syncStatus`** (`queued`/`sending`/`failed`/`conflict`), set to `'queued'` when the Facade returns `{ queued: true }` and driven from `onReplayStart`/`onReplayResult` in `{feature}.offline-sync.ts`. `<ui-pending-sync-indicator>`'s count is a computed **derived from those rows** — never a separately tracked number. On a cold start the store's `hydratePending()` rebuilds the optimistic rows from `MutationQueueService.pendingForFeatureOnce(...)` — a queued action must never be an invisible count.
- A form's submission must go through `submitForm()`.
- Any HTTP call triggered by form submission must go through the owning feature's `data-access` Facade.
- Every project in this lib must run its unit/component tests via Vitest.
- A component test must query and interact with the rendered DOM via Testing Library, not `fixture.componentInstance`/`debugElement`.
- A component test must fake the component's Signal Store (or Facade) — it must never use `HttpTestingController` or let a real HTTP call occur.
- A Signal Store test must fake its Facade directly — it must never reach further down to fake the Client or mock HTTP.

## SHOULD
- Field schema/validators should stay inline in the component for simple forms.
- Component test queries should prefer accessible roles/labels (`getByRole`, `getByLabelText`) over test-id attributes.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/FeatureStore/{Feature}.project.extend.md|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create.md|FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]] - [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend.md|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create.md|Testing/{component-name}.component.spec.ts.create]]


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
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/FeatureStore/{Feature}.project.extend.md|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]] - [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend.md|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create.md|Testing/{component-name}.component.spec.ts.create]]

# Check list

- [ ] `libs/{feature}/feature` contains exactly one feature-level Signal Store for that feature
- [ ] The store is exported from the lib's `index.ts`
- [ ] No path inside `{feature}.routes.ts` includes the feature's own name or an assumed prefix
- [ ] Every form is built with `form()`/`FieldTree`, not `FormGroup`/`FormControl`
- [ ] Every component and Signal Store in this project has a Vitest spec, none of which use `HttpTestingController`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] - [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/Implementation/FeatureStore/{Feature}.project.extend.md|FeatureStore/{Feature}.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]] - [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend.md|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
