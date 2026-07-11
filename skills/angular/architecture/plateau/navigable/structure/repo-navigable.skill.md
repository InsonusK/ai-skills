---
name: repo-navigable
description: Nx workspace layout for the navigable Angular application — apps/libs split, tag taxonomy, module boundaries, the three-tier state-management placement rule, hierarchical root-relative route ownership, and enforced bundle budgets with selective preloading
domain: skill
type: template
plateau: navigable
version: 20260711130000
tags:
  - skill/template/repo
  - plateau/navigable
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
---

> **Deferred scope:** `solution-app-routing` also defines how an *embeddable module* (a federated remote mounted into the shell via Module Federation) mounts its own features' routes one level down (`Implementation/EmbeddableModule/routes.ts.extend.md`). Module Federation / platform-embeddability is not part of this plateau or any plateau up to and including [[skills/angular/architecture/plateau/tested/plateau-tested.skill.md|tested]] — it is introduced by a future "platform" plateau. That implementation slice is therefore deliberately excluded here: at this plateau `apps/platform-shell` only mounts directly-owned features, never a federated remote. Everything else `solution-app-routing` defines (root-relative feature routes, the shell's mounting convention, the generic `{Feature}/feature` routes pattern) works unchanged without federation, since a plain feature can be mounted into the shell with no embeddable/federated module involved.

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
  /{feature}
    /[feature](./feature-feature/project-feature-feature.skill.md)
    /data-access      <- filled in by a future data-access-owning solution
```

- `{feature}` is a placeholder for each business feature (e.g. `orders`, `catalog`, `billing`); every feature gets at least a `feature` + `data-access` pair, never a single flat lib.
- `libs/shared/ui` hosts app-specific UI wrappers composed from design-system primitives (the design system itself is a separate npm package, see the [[skills/angular/architecture/plateau/design-system/plateau-design-system.skill.md|design-system]] plateau) — not the design system itself.
- `libs/shared/util` hosts framework-agnostic helpers (pure functions, RxJS operators, mapping utilities) — no Angular DI, no HTTP, no state.
- `libs/shared/state` hosts classical NgRx slices for global/cross-cutting state (auth, notifications, offline-sync) — see the three-tier state rule below.
- Every routable feature now also exports its `Routes` from `index.ts`, in addition to its Signal Store.

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[platform-shell/project-platform-shell.skill.md\|project-platform-shell.skill]] | The only deployable unit at this plateau. Composition root: bootstraps the app, owns top-level routing (root segments only) and the selective preloading strategy, registers root providers. Contains no business logic of its own. |
| /libs/shared/ui | [[shared-ui/project-shared-ui.skill.md\|project-shared-ui.skill]] | Reusable, app-specific UI composed from design-system primitives. No feature-specific business logic. |
| /libs/shared/util | [[shared-util/project-shared-util.skill.md\|project-shared-util.skill]] | Framework-agnostic pure helpers shared across features. No Angular DI, no HTTP calls, no state. |
| /libs/shared/state | [[shared-state/project-shared-state.skill.md\|project-shared-state.skill]] | Classical NgRx Store for global, cross-cutting state (auth, notifications, offline-sync). |
| /libs/{feature}/feature | [[feature-feature/project-feature-feature.skill.md\|project-feature-feature.skill]] | Generic template: routed, presentational + container components for one feature, its feature-level Signal Store, and its own root-relative routes with lazy sub-splitting. Public API via `index.ts` only. |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]

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

Everything not explicitly listed here is denied by the lint rule. `type:store` (scope:shared) must not depend on any `type:feature` or `type:data-access` project.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]

## Three-tier state placement (cross-cutting convention)

- **Component-local state** is a plain Angular `signal()` on the component — no store of any kind.
- **Feature-level state** is an NgRx Signal Store colocated in that feature's `libs/{feature}/feature` project — see [[feature-feature/classes/class-feature-store.skill.md|class-feature-store.skill]].
- **Global/cross-cutting state** is a classical NgRx Store slice inside `libs/shared/state` — see [[shared-state/project-shared-state.skill.md|project-shared-state.skill]].
- State is promoted from a lower tier to a higher one only when a second, unrelated consumer genuinely needs it — never preemptively.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/LocalState/{component-name}.component.ts.extend|LocalState/{component-name}.component.ts.extend]]

## Hierarchical route ownership (cross-cutting convention)

- Routes are owned hierarchically: the shell only knows first-level root segments; a feature only knows paths relative to its own root.
- A project at any level MUST NOT define a path that reaches outside the segment it owns, and MUST NOT assume the segment name under which its parent will mount it.
- The parent (the shell) assigns the mount segment at the point of mounting — the child never bakes its own name into its own routes.
- A feature's routes are part of its public API, exported from `index.ts` alongside its Signal Store — see [[feature-feature/classes/class-feature-routes.skill.md|class-feature-routes.skill]].

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository.extend]]

## Selective preloading & bundle budgets (cross-cutting convention)

- Every `type:app` project declares an initial-bundle budget at `error` threshold; every routable `type:feature` project declares a per-chunk budget for its own lazy chunk.
- `data: { preload: true }` is set only at the mounting point (`apps/platform-shell`'s `app.routes.ts`) — never inside a feature's own exported routes.
- The router is configured with a custom `SelectivePreloadingStrategy` (`withPreloading`), never `PreloadAllModules` and never left at the default `NoPreloading`.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]

# Rules

## MUST
- Every Nx project MUST declare exactly one `type:*` tag and exactly one `scope:*` tag.
- Every lib MUST expose its public API through a single `index.ts` barrel; nothing outside that barrel may be imported by other projects.
- A `type:feature` project MUST NOT import another `type:feature` project directly, regardless of scope.
- A `type:data-access` project MUST only be imported by the `type:feature` project that shares its `scope`.
- Business logic (HTTP calls, state, domain rules) MUST NOT live in `apps/platform-shell` — the shell only composes and routes.
- Every slice inside `libs/shared/state` MUST correspond to genuinely global/cross-cutting state — feature-scoped state MUST NOT be added there.
- `libs/shared/state` MUST NOT depend on any `type:feature` or `type:data-access` project.
- State that is read/written only within one component MUST be a plain Signal on that component, not a feature Signal Store or global NgRx state.
- Every routable `type:feature` project MUST export its `Routes` array from `index.ts`, using paths defined only relative to its own root.
- A project at any level MUST NOT define a route path that reaches outside the root segment it owns.
- The project that mounts a child (the shell mounting a feature) MUST assign the root segment at the mounting point — the child never declares its own mount prefix.
- `apps/platform-shell` MUST NOT import any component from inside a feature directly into `app.routes.ts` — only the feature's exported `Routes`.
- Every `type:app` project MUST declare an initial-bundle budget (`error` threshold) in its build configuration.
- Every routable `type:feature` project MUST declare a per-chunk budget for its own lazy chunk.
- `data: { preload: true }` MUST be set only at the mounting point, never inside the feature's own exported routes.

## SHOULD
- New business features SHOULD be scaffolded as a `{feature}/feature` + `{feature}/data-access` pair from the start, even if `data-access` is thin initially.
- Cross-feature communication SHOULD go through routing (navigation) or a `scope:platform` orchestrating layer, not direct imports between features.
- Bundle budget thresholds SHOULD be reviewed and adjusted deliberately when a feature's legitimate size grows, rather than silenced by raising the threshold reflexively.

## MUST NOT
- MUST NOT place a routed business feature directly under `/apps` — every feature lives under `/libs/{feature}` and is only routed to from an app.
- MUST NOT add a `type:util` project with any `scope:*` other than `shared`.
- A `type:feature` project MUST NOT reach into another feature's Signal Store directly to read cross-cutting data.
- A component MUST NOT introduce a feature Signal Store or a `libs/shared/state` slice purely to hold state no other component or feature ever needs to read.
- A `type:app` project (the shell) MUST NOT reference a path that exists two or more levels below its own mount point.
- A feature MUST NOT set `preload: true` on its own routes to opt itself into preloading — that decision belongs to whoever mounts it.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/LocalState/{component-name}.component.ts.extend|LocalState/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]

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
- **A feature exporting routes with its own name baked into the path (e.g. `orders/list` instead of `list`)**
  - Consequence: the feature silently assumes it will always be mounted at a specific segment, breaking the moment it is remounted elsewhere
  - Instead: the feature only ever defines paths relative to its own root; the mounting project decides the segment name
- **The shell adding a route that targets a specific page inside a feature (e.g. `path: 'feature1/page'`)**
  - Consequence: shell now depends on the feature's internal route structure, breaking hierarchical ownership and making `nx affected` treat the shell as touched by internal feature navigation changes
  - Instead: mount only `feature1` as a segment; the feature's own routes define `page` beneath it
- **Marking every top-level segment `preload: true` "to be safe"**
  - Consequence: degenerates into the equivalent of `PreloadAllModules`
  - Instead: mark only the small number of genuinely high-traffic segments; leave the rest on-demand
- **Raising a bundle budget threshold to make a CI failure go away without investigating the cause**
  - Consequence: defeats the purpose of the budget — a genuine regression goes unnoticed
  - Instead: investigate why the bundle grew; only raise the threshold if the growth is a deliberate, reviewed trade-off

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]

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
- [ ] WHEN a feature's exported `Routes` array is inspected THEN
  - [ ] no path in it includes the feature's own mount segment
- [ ] WHEN the shell's route config is inspected THEN
  - [ ] it only references first-level mount segments, never a path nested two or more levels deep
- [ ] WHEN a non-lazy import accidentally pulls feature code into the initial bundle THEN
  - [ ] the `type:app` project's initial-bundle budget fails the build with an error, not a warning
- [ ] WHEN a feature's own exported `Routes` are inspected THEN
  - [ ] none of them set `data: { preload: true }` on themselves

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]
