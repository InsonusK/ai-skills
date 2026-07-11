---
name: repo-data-capable
description: Nx workspace layout for the data-capable Angular application — adds Signal Forms (Angular >= 22), libs/shared/http-core, and the Facade/Client/Mapper/Errors structure inside every feature's data-access lib, on top of the navigable plateau's routing and state-management conventions
domain: skill
type: template
plateau: data-capable
version: 20260711140000
tags:
  - skill/template/repo
  - plateau/data-capable
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]]"
---

> **Deferred scope:** carried over unchanged from [[skills/angular/architecture/plateau/navigable/plateau-navigable.skill.md|navigable]] — `solution-app-routing`'s embeddable-module routing slice (`Implementation/EmbeddableModule/routes.ts.extend.md`) remains excluded, deferred to a future "platform" plateau. Neither `solution-forms` nor `solution-api-http-layer` touch Module Federation at all, so they introduce no new deferred scope of their own.

# Structure

## Workspace Structure

```
/apps
  /[platform-shell](./platform-shell/project-platform-shell.skill.md)

/libs
  /shared
    /[ui](./shared-ui/project-shared-ui.skill.md)
    /[util](./shared-util/project-shared-util.skill.md)
    /[state](./shared-state/project-shared-state.skill.md)
    /[http-core](./shared-http-core/project-shared-http-core.skill.md)      <- new (solution-api-http-layer)
  /{feature}
    /[feature](./feature-feature/project-feature-feature.skill.md)
    /[data-access](./feature-data-access/project-feature-data-access.skill.md)      <- new (solution-api-http-layer)
```

- `libs/shared/http-core` is a base HTTP service (base URL, timeout, retry) that every feature's Client builds on instead of raw `HttpClient`. Tagged `type:util`, `scope:shared`.
- `libs/{feature}/data-access` is now filled in: `{feature}.facade.ts` (public API), `{feature}.client.ts` (internal transport), `{feature}.mapper.ts` + `{feature}.errors.ts` (internal DTO mapping and typed domain errors).
- The workspace now requires Angular >= 22 to unlock stable Signal Forms.

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[platform-shell/project-platform-shell.skill.md\|project-platform-shell.skill]] | The only deployable unit at this plateau. Composition root: bootstraps the app, owns top-level routing and the selective preloading strategy, registers root providers. Contains no business logic of its own. |
| /libs/shared/ui | [[shared-ui/project-shared-ui.skill.md\|project-shared-ui.skill]] | Reusable, app-specific UI composed from design-system primitives. No feature-specific business logic. |
| /libs/shared/util | [[shared-util/project-shared-util.skill.md\|project-shared-util.skill]] | Framework-agnostic pure helpers shared across features. No Angular DI, no HTTP calls, no state. |
| /libs/shared/state | [[shared-state/project-shared-state.skill.md\|project-shared-state.skill]] | Classical NgRx Store for global, cross-cutting state (auth, notifications, offline-sync). |
| /libs/shared/http-core | [[shared-http-core/project-shared-http-core.skill.md\|project-shared-http-core.skill]] | Base HTTP service shared by every feature's Client — base URL, timeout, retry. Feature-agnostic. |
| /libs/{feature}/feature | [[feature-feature/project-feature-feature.skill.md\|project-feature-feature.skill]] | Generic template: routed, presentational + container components (including forms) for one feature, its feature-level Signal Store, and its own root-relative routes with lazy sub-splitting. Public API via `index.ts` only. |
| /libs/{feature}/data-access | [[feature-data-access/project-feature-data-access.skill.md\|project-feature-data-access.skill]] | Generic template: this feature's Facade/Client/Mapper/Errors layering for HTTP data operations. Only the Facade and domain error types are public. |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

## Nx Tag Taxonomy

Every Nx project (app or lib) declares tags along two independent axes:

| Axis | Values | Meaning |
| ----- | ------- | ------- |
| `type` | `app`, `feature`, `data-access`, `ui`, `util`, `store` | What role the project plays |
| `scope` | `platform`, `shared`, `{feature-name}` (e.g. `orders`) | Which business area the project belongs to |

No new tag values are introduced by this plateau; `libs/shared/http-core` uses the existing `type:util`/`scope:shared` combination, already permitted as a dependency of `type:data-access`.

`@nx/enforce-module-boundaries` allow-list (unchanged from [[skills/angular/architecture/plateau/navigable/plateau-navigable.skill.md|navigable]]):

| type | may depend on |
| ----- | -------------- |
| `app` | any `type:feature` with matching or `scope:platform` |
| `feature` | `type:data-access` with the same `scope`, `type:ui` with `scope:shared`, `type:util` with `scope:shared`, `type:store` with `scope:shared` |
| `data-access` | `type:util` with `scope:shared` |
| `ui` (scope:shared) | `type:util` with `scope:shared` |
| `util` (scope:shared) | nothing (leaf) |
| `store` (scope:shared) | `type:util` with `scope:shared` |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

## Three-tier state placement (cross-cutting convention)

Unchanged from [[skills/angular/architecture/plateau/navigable/plateau-navigable.skill.md|navigable]] — component Signal → feature Signal Store → global NgRx Store, with feature-scoped operations now calling straight into that feature's Facade (no Action/Reducer/Effect); global/cross-cutting state keeps its existing classical NgRx chain (Effect → Facade → Client) unchanged.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/LocalState/{component-name}.component.ts.extend|LocalState/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

## Hierarchical route ownership & selective preloading (cross-cutting convention)

Unchanged from [[skills/angular/architecture/plateau/navigable/plateau-navigable.skill.md|navigable]].

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]

## Signal Forms (cross-cutting convention)

- New forms are built with Signal Forms (`@angular/forms/signals`: `form()`, `FieldTree`) by default. Existing Reactive Forms are never force-migrated purely for consistency — only touched opportunistically when a form is already being substantially reworked.
- Field schema/validators stay inline in the component for simple forms, and are extracted into a `{form-name}.form.ts` file once cross-field (`when`) logic or validator count makes the component harder to read.
- Form components live inside their owning feature's `libs/{feature}/feature` project — see [[feature-feature/classes/class-form-component.skill.md|class-form-component.skill]].

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/Repository.extend|Repository.extend]]

# Rules

## MUST
- Every Nx project MUST declare exactly one `type:*` tag and exactly one `scope:*` tag.
- Every lib MUST expose its public API through a single `index.ts` barrel; nothing outside that barrel may be imported by other projects.
- A `type:feature` project MUST NOT import another `type:feature` project directly, regardless of scope.
- A `type:data-access` project MUST only be imported by the `type:feature` project that shares its `scope`.
- Business logic (HTTP calls, state, domain rules) MUST NOT live in `apps/platform-shell` — the shell only composes and routes.
- Every slice inside `libs/shared/state` MUST correspond to genuinely global/cross-cutting state.
- `libs/shared/state` MUST NOT depend on any `type:feature` or `type:data-access` project.
- State that is read/written only within one component MUST be a plain Signal on that component.
- Every routable `type:feature` project MUST export its `Routes` array from `index.ts`, relative to its own root only.
- A project at any level MUST NOT define a route path that reaches outside the root segment it owns.
- The mounting project MUST assign the root segment — the child never declares its own mount prefix.
- Every `type:app` and routable `type:feature` project MUST declare an enforced (`error`-level) bundle budget.
- `data: { preload: true }` MUST be set only at the mounting point, never inside a feature's own exported routes.
- The workspace MUST run Angular >= 22 before Signal Forms rules apply.
- New forms MUST use Signal Forms by default.
- A form's submission MUST go through `submitForm()`, never a manually wired handler that bypasses the form's own validation/state.
- Any HTTP call triggered by form submission MUST go through the owning feature's `data-access` Facade, never `HttpClient` directly.
- A custom, design-system-provided form control used in a Signal Form MUST implement `ControlValueAccessor`.
- A feature's `{feature}.client.ts` MUST NOT be exported from that feature's `index.ts` — only the Facade (and domain error types) is public.
- Every Client MUST build its HTTP calls on top of `libs/shared/http-core`'s base service, never call `HttpClient` directly.
- A Client MUST catch every `HttpErrorResponse` it can produce and rethrow a typed domain error from `{feature}.errors.ts` — a raw `HttpErrorResponse` MUST NOT escape the Client.
- For feature-scoped operations, the calling Signal Store method MUST call the Facade directly — no Action/Reducer/Effect for feature-level data operations.

## SHOULD
- New business features SHOULD be scaffolded as a `{feature}/feature` + `{feature}/data-access` pair from the start.
- Cross-feature communication SHOULD go through routing or a `scope:platform` orchestrating layer, not direct imports between features.
- Bundle budget thresholds SHOULD be reviewed and adjusted deliberately when a feature's legitimate size grows.

## SHOULD NOT
- Existing Reactive Forms code SHOULD NOT be migrated to Signal Forms purely for the sake of consistency — only migrate a form when it is already being substantially rewritten for other reasons.

## MUST NOT
- MUST NOT place a routed business feature directly under `/apps`.
- MUST NOT add a `type:util` project with any `scope:*` other than `shared`.
- A `type:feature` project MUST NOT reach into another feature's Signal Store directly to read cross-cutting data.
- A component MUST NOT introduce a feature Signal Store or a `libs/shared/state` slice purely to hold state no other component or feature ever needs to read.
- A `type:app` project (the shell) MUST NOT reference a path that exists two or more levels below its own mount point.
- A feature MUST NOT set `preload: true` on its own routes to opt itself into preloading.
- A component or Signal Store method MUST NOT import a feature's Client directly, bypassing the Facade.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

- **Two features importing each other's internal components directly**
  - Consequence: hidden coupling, defeats `@nx/enforce-module-boundaries`
  - Instead: extract the shared piece into `libs/shared/ui`/`libs/shared/util`, or communicate through routing
- **Single flat lib per feature instead of `feature` + `data-access` split**
  - Consequence: UI and HTTP/data concerns become entangled, harder to test in isolation
  - Instead: always split into at least `feature` and `data-access` from the start
- **A feature exporting routes with its own name baked into the path**
  - Consequence: breaks the moment the feature is remounted elsewhere
  - Instead: paths relative to the feature's own root only; the mounting project decides the segment name
- **Marking every top-level segment `preload: true` "to be safe"**
  - Consequence: degenerates into the equivalent of `PreloadAllModules`
  - Instead: mark only the small number of genuinely high-traffic segments
- **Starting a brand-new form with Reactive Forms "because that's what the rest of the codebase still has"**
  - Consequence: perpetuates the older pattern indefinitely, never captures the Signal Forms benefits this solution exists to adopt
  - Instead: every new form starts with Signal Forms; only an existing, untouched Reactive Forms form is left as-is
- **Mass-migrating all existing Reactive Forms to Signal Forms in one pass**
  - Consequence: large, high-risk changeset with no functional benefit to users
  - Instead: migrate opportunistically, only when a form is already being substantially reworked
- **A Signal Store method calling the feature's Client directly, skipping the Facade**
  - Consequence: bypasses business-rule validation the Facade exists to enforce
  - Instead: the store always goes through the Facade; only the Facade calls the Client
- **A Client method letting a raw `HttpErrorResponse` propagate uncaught**
  - Consequence: feature/business code ends up branching on HTTP status codes instead of a meaningful domain error
  - Instead: catch every possible transport error inside the Client and rethrow the feature's typed domain error
- **Adding a feature-specific special case directly into `base-http.service.ts`**
  - Consequence: turns a feature-agnostic shared service into a growing pile of one-off conditions
  - Instead: keep this service generic; feature-specific behavior belongs in that feature's own Client

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|HttpCore/shared-http-core.project.create]]

# Unittest TestCases

- [ ] WHEN `nx run-many -t lint` is executed THEN
  - [ ] `@nx/enforce-module-boundaries` reports no violations
- [ ] WHEN a new form component is added to the codebase THEN
  - [ ] it is built with `form()`/`FieldTree` from `@angular/forms/signals`, not `FormGroup`/`FormControl`
- [ ] WHEN a feature's `index.ts` is inspected THEN
  - [ ] it exports the Facade and domain error types only, never the Client or Mapper
- [ ] WHEN a Client method's HTTP call fails THEN
  - [ ] the Client rethrows a typed domain error, never the original `HttpErrorResponse`
- [ ] WHEN a feature-scoped operation is inspected THEN
  - [ ] no Action, Reducer, or Effect exists for it — only a Signal Store method calling the Facade
- [ ] WHEN a non-lazy import accidentally pulls feature code into the initial bundle THEN
  - [ ] the `type:app` project's initial-bundle budget fails the build with an error, not a warning

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]
