---
description: Base Nx workspace layout — apps/libs split, tag taxonomy and module boundaries
element_kind: repository
change_kind: create
tags:
  - solution/repository-structure
  - element/monolith-repository
---

# Structure

## Workspace Structure

```
/apps
  /platform-shell

/libs
  /shared
    /ui
    /util
  /{feature}
    /feature
    /data-access        <- only when the feature talks to a backend (BackendDataAccess = Yes)
```

- `{feature}` is a placeholder for each business feature (e.g. `orders`, `catalog`, `billing`).
- The minimum for a feature is a single `feature` lib (components, routing, store). A feature that talks to a backend is **additionally** split into a `data-access` lib (API calls, DTO mapping) so UI and data-access evolve and are reused independently — that split is added by `solution-api-http-layer` (`BackendDataAccess` = Yes), per [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/adr/feature-lib-split-conditional-on-backend-data-access.md|feature-lib-split-conditional-on-backend-data-access]]. A feature never gets a single *flat* lib mixing UI and data concerns.
- `libs/shared/ui` hosts app-specific UI wrappers that are not part of the design system itself (the design system is a separate npm package, see the `solution-design-system-*` solutions) — for example composed layouts built out of design-system primitives.
- `libs/shared/util` hosts framework-agnostic helpers (pure functions, RxJS operators, mapping utilities) with no Angular DI, no HTTP, no state.

## Directory and project skills

| Directory | Description |
| ---------- | ----------- |
| /apps/platform-shell | The only deployable unit at this stage (see `solution-federation-host` for how this splits into a host + embeddable apps). Composition root: bootstraps the app, owns top-level routing, registers root providers. Contains no business logic of its own. |
| /libs/shared/ui | Reusable, app-specific UI composed from design-system primitives. No feature-specific business logic. |
| /libs/shared/util | Framework-agnostic pure helpers shared across features. No Angular DI, no HTTP calls, no state. |
| /libs/{feature}/feature | Routed, presentational + container components for one feature, its feature-level Signal Store, and feature-local routing. Public API exposed only via `index.ts`. Every feature has this lib. |
| /libs/{feature}/data-access | HTTP calls, DTO-to-domain-model mapping, and facade for one feature. **Present only when the feature has server data** (`solution-api-http-layer`, `BackendDataAccess` = Yes). Consumed only by that feature's `feature` lib (see `solution-api-http-layer` for the internal shape of this lib). |

# Nx tag taxonomy

Every Nx project (app or lib) must declare tags along two independent axes:

| Axis | Values | Meaning |
| ----- | ------- | ------- |
| `type` | `app`, `feature`, `data-access`, `ui`, `util` | What role the project plays |
| `scope` | `platform`, `shared`, `{feature-name}` (e.g. `orders`) | Which business area the project belongs to |

`@nx/enforce-module-boundaries` is configured with the following allow-list:

| type | may depend on |
| ----- | -------------- |
| `app` | any `type:feature` with matching or `scope:platform` |
| `feature` | `type:data-access` with the same `scope`, `type:ui` with `scope:shared`, `type:util` with `scope:shared` |
| `data-access` | `type:util` with `scope:shared` |
| `ui` (scope:shared) | `type:util` with `scope:shared` |
| `util` (scope:shared) | nothing (leaf) |

Everything not explicitly listed here is denied by the lint rule.

# Rules

## MUST
- Every Nx project must declare exactly one `type:*` tag and exactly one `scope:*` tag.
- Every lib must expose its public API through a single `index.ts` barrel; nothing outside that barrel may be imported by other projects.
- A `type:feature` project must never import another `type:feature` project directly, regardless of scope.
- A `type:data-access` project must only be imported by the `type:feature` project that shares its `scope`.
- Business logic (HTTP calls, state, domain rules) must never live in `apps/platform-shell` — the shell only composes and routes.

- Never place a routed business feature directly under `/apps` — every feature lives under `/libs/{feature}` and is only routed to from an app.
- Never add a `type:util` project with any `scope:*` other than `shared` — utils are by definition scope-agnostic; a feature-specific helper belongs inside that feature's own lib, not in a scoped util.
## SHOULD
- A business feature with server data should be scaffolded as a `{feature}/feature` + `{feature}/data-access` pair from the start, even if `data-access` is thin initially — splitting later is more expensive than starting split. A feature with no backend has just the `feature` lib.
- Cross-feature communication should go through routing (navigation) or through a `scope:platform` orchestrating layer, not through direct imports between features.

- **Two features importing each other's internal components directly** — Consequence: hidden coupling, `@nx/enforce-module-boundaries` becomes ineffective if features are allowed to bypass it, and `nx affected` starts marking unrelated features as impacted — Instead: extract the shared piece into `libs/shared/ui` or `libs/shared/util`, or communicate through routing
- **Growing `apps/platform-shell` with feature-specific logic "just for now"** — Consequence: the shell stops being a thin composition root, affected-based builds treat the shell as touched by almost every change, and the future platform/embedded-app split (see `solution-federation-host`) becomes harder to carve out — Instead: scaffold a `libs/{feature}` pair even for small features and route to it from the shell
- **Single flat lib mixing UI and HTTP/data concerns for a feature that has both** — Consequence: UI and data concerns become entangled, harder to test in isolation, and `solution-api-http-layer` has no clean seam to attach to — Instead: a feature with server data gets a `feature` lib and a `data-access` lib; a feature with no backend gets just the `feature` lib
# Check list

- [ ] Every project under `/apps` and `/libs` has both a `type:*` and a `scope:*` tag
- [ ] `nx run-many -t lint` passes with zero `@nx/enforce-module-boundaries` violations
- [ ] Every lib's public API is limited to what is re-exported from its `index.ts`
- [ ] `apps/platform-shell` contains no HTTP calls, no business state, no feature-specific components
- [ ] Every business feature has a `feature` project under `/libs/{feature}`; every feature with server data also has a `data-access` project

# Unittest TestCases

- [ ] WHEN `nx run-many -t lint` is executed THEN
  - [ ] `@nx/enforce-module-boundaries` reports no violations
- [ ] WHEN a commit only touches `/libs/orders/feature` THEN
  - [ ] `nx affected -t test` runs tests only for `orders-feature` and its dependents, not for unrelated features
- [ ] WHEN a project attempts to import another `type:feature` project directly THEN
  - [ ] lint fails with an `enforce-module-boundaries` error
