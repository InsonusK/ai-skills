---
name: repo-foundation
description: Base Nx workspace layout for the Angular application — apps/libs split, tag taxonomy, module boundaries, and the three-tier state-management placement rule
domain: skill
type: template
plateau: foundation
version: 20260711120000
tags:
  - skill/template/repo
  - plateau/foundation
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
---

# Structure

## Workspace Structure

```
/apps
  /[platform-shell](./platform-shell/project-platform-shell.skill.md)

/libs
  /shared
    /[ui](./shared-ui/project-shared-ui.skill.md)
    /[util](./shared-util/project-shared-util.skill.md)
    /[state](./shared-state/project-shared-state.skill.md)      <- new (solution-state-management)
  /{feature}
    /[feature](./feature-feature/project-feature-feature.skill.md)
    /data-access      <- filled in by a future data-access-owning solution
```

- `{feature}` is a placeholder for each business feature (e.g. `orders`, `catalog`, `billing`); every feature gets at least a `feature` + `data-access` pair, never a single flat lib.
- `libs/shared/ui` hosts app-specific UI wrappers composed from design-system primitives (the design system itself is a separate npm package, see the [[skills/angular/architecture/plateau/design-system/plateau-design-system.skill.md|design-system]] plateau) — not the design system itself.
- `libs/shared/util` hosts framework-agnostic helpers (pure functions, RxJS operators, mapping utilities) — no Angular DI, no HTTP, no state.
- `libs/shared/state` hosts classical NgRx slices for global/cross-cutting state (auth, notifications, offline-sync) — see the three-tier state rule below.

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[platform-shell/project-platform-shell.skill.md\|project-platform-shell.skill]] | The only deployable unit at this plateau. Composition root: bootstraps the app, owns top-level routing, registers root providers. Contains no business logic of its own. |
| /libs/shared/ui | [[shared-ui/project-shared-ui.skill.md\|project-shared-ui.skill]] | Reusable, app-specific UI composed from design-system primitives. No feature-specific business logic. |
| /libs/shared/util | [[shared-util/project-shared-util.skill.md\|project-shared-util.skill]] | Framework-agnostic pure helpers shared across features. No Angular DI, no HTTP calls, no state. |
| /libs/shared/state | [[shared-state/project-shared-state.skill.md\|project-shared-state.skill]] | Classical NgRx Store for global, cross-cutting state (auth, notifications, offline-sync). |
| /libs/{feature}/feature | [[feature-feature/project-feature-feature.skill.md\|project-feature-feature.skill]] | Generic template: routed, presentational + container components for one feature, plus its feature-level Signal Store. Public API via `index.ts` only. |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]

## Nx Tag Taxonomy

Every Nx project (app or lib) declares tags along two independent axes:

| Axis | Values | Meaning |
| ----- | ------- | ------- |
| `type` | `app`, `feature`, `data-access`, `ui`, `util`, `store` | What role the project plays |
| `scope` | `platform`, `shared`, `{feature-name}` (e.g. `orders`) | Which business area the project belongs to |

`@nx/enforce-module-boundaries` allow-list:

| type | may depend on |
| ----- | -------------- |
| `app` | any `type:feature` with matching or `scope:platform` |
| `feature` | `type:data-access` with the same `scope`, `type:ui` with `scope:shared`, `type:util` with `scope:shared`, `type:store` with `scope:shared` |
| `data-access` | `type:util` with `scope:shared` |
| `ui` (scope:shared) | `type:util` with `scope:shared` |
| `util` (scope:shared) | nothing (leaf) |
| `store` (scope:shared) | `type:util` with `scope:shared` |

Everything not explicitly listed here is denied by the lint rule. `type:store` (scope:shared) must not depend on any `type:feature` or `type:data-access` project — global state is a foundation other layers read from, not the reverse.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]

## Three-tier state placement (cross-cutting convention)

This rule applies inside every project in the workspace and has no single project of its own to live in:

- **Component-local state** (dialog visibility, selected tab, form draft, component-scoped loading flags) is a plain Angular `signal()` on the component — no store of any kind.
- **Feature-level state** (data/UI state owned by one feature) is an NgRx Signal Store colocated in that feature's `libs/{feature}/feature` project — see [[feature-feature/classes/class-feature-store.skill.md|class-feature-store.skill]].
- **Global/cross-cutting state** (read or dispatched by more than one unrelated feature) is a classical NgRx Store slice inside `libs/shared/state` — see [[shared-state/project-shared-state.skill.md|project-shared-state.skill]].
- State is promoted from a lower tier to a higher one only when a second, unrelated consumer genuinely needs it — never preemptively.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/LocalState/{component-name}.component.ts.extend|LocalState/{component-name}.component.ts.extend]]

# Rules

## MUST
- Every Nx project MUST declare exactly one `type:*` tag and exactly one `scope:*` tag.
- Every lib MUST expose its public API through a single `index.ts` barrel; nothing outside that barrel may be imported by other projects.
- A `type:feature` project MUST NOT import another `type:feature` project directly, regardless of scope.
- A `type:data-access` project MUST only be imported by the `type:feature` project that shares its `scope`.
- Business logic (HTTP calls, state, domain rules) MUST NOT live in `apps/platform-shell` — the shell only composes and routes.
- Every slice inside `libs/shared/state` MUST correspond to genuinely global/cross-cutting state — feature-scoped state MUST NOT be added there.
- `libs/shared/state` MUST NOT depend on any `type:feature` or `type:data-access` project.
- State that is read/written only within one component (and optionally its direct children) MUST be a plain Signal on that component, not a feature Signal Store or global NgRx state.

## SHOULD
- New business features SHOULD be scaffolded as a `{feature}/feature` + `{feature}/data-access` pair from the start, even if `data-access` is thin initially.
- Cross-feature communication SHOULD go through routing (navigation) or a `scope:platform` orchestrating layer, not direct imports between features.

## MUST NOT
- MUST NOT place a routed business feature directly under `/apps` — every feature lives under `/libs/{feature}` and is only routed to from an app.
- MUST NOT add a `type:util` project with any `scope:*` other than `shared`.
- A `type:feature` project MUST NOT reach into another feature's Signal Store directly to read cross-cutting data.
- A component MUST NOT introduce a feature Signal Store or a `libs/shared/state` slice purely to hold state no other component or feature ever needs to read.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/LocalState/{component-name}.component.ts.extend|LocalState/{component-name}.component.ts.extend]]

# Anti-patterns

- **Two features importing each other's internal components directly**
  - Consequence: hidden coupling, defeats `@nx/enforce-module-boundaries`, `nx affected` starts marking unrelated features as impacted
  - Instead: extract the shared piece into `libs/shared/ui`/`libs/shared/util`, or communicate through routing
- **Growing `apps/platform-shell` with feature-specific logic "just for now"**
  - Consequence: the shell stops being a thin composition root; affected-based builds treat it as touched by almost every change
  - Instead: scaffold a `libs/{feature}` pair even for small features and route to it from the shell
- **Single flat lib per feature instead of `feature` + `data-access` split**
  - Consequence: UI and HTTP/data concerns become entangled, harder to test in isolation
  - Instead: always split into at least `feature` and `data-access` from the start
- **Adding a feature-specific slice to `libs/shared/state` "because it might be needed elsewhere later"**
  - Consequence: erodes the state-tiering rule, turns the shared state lib into a dumping ground
  - Instead: keep the slice in the owning feature's Signal Store until a second, unrelated feature genuinely needs it
- **Creating a feature-level Signal Store for a single dialog's open/closed flag**
  - Consequence: unnecessary indirection for state nothing outside the component ever reads
  - Instead: a plain `signal(false)` field on the component

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/LocalState/{component-name}.component.ts.extend|LocalState/{component-name}.component.ts.extend]]

# Unittest TestCases

- [ ] WHEN `nx run-many -t lint` is executed THEN
  - [ ] `@nx/enforce-module-boundaries` reports no violations
- [ ] WHEN a commit only touches `/libs/orders/feature` THEN
  - [ ] `nx affected -t test` runs tests only for `orders-feature` and its dependents
- [ ] WHEN a project attempts to import another `type:feature` project directly THEN
  - [ ] lint fails with an `enforce-module-boundaries` error
- [ ] WHEN a feature attempts to import another feature's Signal Store directly THEN
  - [ ] `@nx/enforce-module-boundaries` fails the lint step
- [ ] WHEN `libs/shared/state` is built THEN
  - [ ] it has no dependency edge to any `type:feature` or `type:data-access` project in `nx graph`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]
