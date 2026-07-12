---
name: plateau-online-monolith--repo-online-monolith
description: Nx workspace layout for the online-monolith Angular application — apps/libs split with enforced module boundaries, three-tier state management, hierarchical routing, Signal Forms, a Facade/Client HTTP data-access layer, console-only logging, and Vitest/Playwright test coverage. No offline support, no Module Federation, no authentication yet.
domain: skill
type: template
plateau: online-monolith
version: 20260711180000
tags:
  - skill/template/repo
  - plateau/online-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]"
  - "[[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]]"
  - "[[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill.md|solution-testing]]"
---

> First plateau in the main application's chain (no parent) — the **"online-monolith" milestone**: a single deployable Nx application with structure, state, routing, forms, an HTTP data layer, console logging, and enforced test coverage. No lazy-loading yet (that's [[skills/angular/architecture/plateau/plateau-async-monolith/plateau-async-monolith.skill.md|async-monolith]]), no offline support, no Module Federation, no backend log delivery, and no authentication (that's [[skills/angular/architecture/plateau/plateau-multiuser-app/plateau-multiuser-app.skill.md|multiuser-app]], the last plateau in the chain) — every user is implicitly trusted at this stage. The [[skills/angular/architecture/plateau/plateau-design-system/plateau-design-system.skill.md|design-system]] npm package is already a real, plain dependency of `apps/platform-shell` (see `platform-shell`'s own project skill's NPM Packages table).

# Structure

## Workspace Structure

```
/apps
  /[platform-shell](./platform-shell/plateau-online-monolith--project-platform-shell.skill.md)
  /[platform-shell-e2e](./platform-shell-e2e/plateau-online-monolith--project-platform-shell-e2e.skill.md)

/libs
  /shared
    /[ui](./shared-ui/plateau-online-monolith--project-shared-ui.skill.md)
    /[util](./shared-util/plateau-online-monolith--project-shared-util.skill.md)
    /[state](./shared-state/plateau-online-monolith--project-shared-state.skill.md)
    /[http-core](./shared-http-core/plateau-online-monolith--project-shared-http-core.skill.md)
    /[logging](./shared-logging/plateau-online-monolith--project-shared-logging.skill.md)
  /{feature}
    /[feature](./feature-feature/plateau-online-monolith--project-feature-feature.skill.md)
    /[data-access](./feature-data-access/plateau-online-monolith--project-feature-data-access.skill.md)
```

- `apps/platform-shell-e2e` hosts Playwright scenario-level specs. Tagged `type:e2e`, `scope:platform`.
- Every project's unit/component tests run via Vitest — Karma and Jest are not permitted as a project's test runner.

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/platform-shell/plateau-online-monolith--project-platform-shell.skill\|project-platform-shell]] | The only deployable unit at this plateau. Composition root: bootstraps the app, owns top-level routing, registers root providers. |
| /apps/platform-shell-e2e | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/platform-shell-e2e/plateau-online-monolith--project-platform-shell-e2e.skill\|project-platform-shell-e2e]] | Playwright end-to-end scenario specs against the real built application. |
| /libs/shared/ui | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/shared-ui/plateau-online-monolith--project-shared-ui.skill\|project-shared-ui]] | Reusable, app-specific UI composed from design-system primitives. |
| /libs/shared/util | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/shared-util/plateau-online-monolith--project-shared-util.skill\|project-shared-util]] | Framework-agnostic pure helpers shared across features. |
| /libs/shared/state | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/shared-state/plateau-online-monolith--project-shared-state.skill\|project-shared-state]] | Classical NgRx Store for global, cross-cutting state (e.g. auth session skeleton, notifications). |
| /libs/shared/http-core | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/shared-http-core/plateau-online-monolith--project-shared-http-core.skill\|project-shared-http-core]] | Base HTTP service shared by every feature's Client. |
| /libs/shared/logging | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/shared-logging/plateau-online-monolith--project-shared-logging.skill\|project-shared-logging]] | `LoggerService` with a `ConsoleLogSink` — console-only at this plateau. |
| /libs/{feature}/feature | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/feature-feature/plateau-online-monolith--project-feature-feature.skill\|project-feature-feature]] | Generic template: routed, presentational + container components (including forms and their component tests), feature-level Signal Store, own root-relative routes. |
| /libs/{feature}/data-access | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/feature-data-access/plateau-online-monolith--project-feature-data-access.skill\|project-feature-data-access]] | Generic template: this feature's Facade/Client/Mapper/Errors layering, each with its own unit-test pattern. |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill.md|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill.md|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/platform-shell-e2e.project.create|platform-shell-e2e.project.create]]

## Nx Tag Taxonomy

| Axis | Values | Meaning |
| ----- | ------- | ------- |
| `type` | `app`, `e2e`, `feature`, `data-access`, `ui`, `util`, `store` | What role the project plays |
| `scope` | `platform`, `shared`, `{feature-name}` (e.g. `orders`) | Which business area the project belongs to |

`@nx/enforce-module-boundaries` allow-list:

| type | may depend on |
| ----- | -------------- |
| `app` | any `type:feature` with matching or `scope:platform` |
| `e2e` (scope:platform) | nothing |
| `feature` | `type:data-access` with the same `scope`, `type:ui`/`type:util`/`type:store` with `scope:shared` |
| `data-access` | `type:util` with `scope:shared` |
| `ui` (scope:shared) | `type:util` with `scope:shared` |
| `util` (scope:shared) | nothing (leaf) |
| `store` (scope:shared) | `type:util` with `scope:shared` |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill.md|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]

## Cross-cutting conventions

- **Three-tier state placement**: component Signal → feature Signal Store → global NgRx Store, promoted upward only when a second, unrelated consumer genuinely needs it.
- **Hierarchical route ownership**: the shell only knows first-level root segments; a feature only knows paths relative to its own root; the parent assigns the mount segment.
- **Facade/Client/Mapper layering**: every feature's `data-access` lib is Facade (public API, business validation) → Client (internal transport + DTO mapping) → shared `libs/shared/http-core`.
- **Single logging seam**: everything logs through `LoggerService`, currently forwarding only to `ConsoleLogSink` — no direct `console.*` call anywhere else.
- **Testing conventions**: every Nx project runs unit/component tests via Vitest; end-to-end tests are Playwright specs in `apps/platform-shell-e2e`; `HttpTestingController` is used only inside a feature's own `{feature}.client.ts` spec — every other layer fakes the layer directly beneath it; CI enforces a minimum coverage threshold as a hard error.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill.md|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]

# Rules

## MUST
- Every Nx project MUST declare exactly one `type:*` tag and exactly one `scope:*` tag.
- Every lib MUST expose its public API through a single `index.ts` barrel.
- Business logic MUST NOT live in `apps/platform-shell`.
- Every routable `type:feature` project MUST export its `Routes` array from `index.ts`.
- New forms MUST use Signal Forms by default.
- A Client MUST catch every `HttpErrorResponse` and rethrow a typed domain error.
- Every part of the application MUST log through `LoggerService` — no direct `console.*` call outside `libs/shared/logging`'s own `ConsoleLogSink`.
- Every Nx project MUST run its unit/component tests via Vitest — no project may configure Karma or Jest as its test runner.
- End-to-end tests MUST be written with Playwright, in the dedicated `type:e2e` project.
- `HttpTestingController` MUST be used only inside a feature's own `{feature}.client.ts` unit tests.
- MSW MUST be used only for tests that deliberately span more than one architectural layer.
- CI MUST enforce a minimum code-coverage threshold per project as a hard `error`.

## SHOULD
- New business features SHOULD be scaffolded as a `{feature}/feature` + `{feature}/data-access` pair from the start.

## SHOULD NOT
- Existing Reactive Forms code SHOULD NOT be migrated to Signal Forms purely for consistency.

## MUST NOT
- MUST NOT place a routed business feature directly under `/apps`.
- A component or Signal Store method MUST NOT import a feature's Client directly, bypassing the Facade.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill.md|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

- **Single flat lib per feature instead of `feature` + `data-access` split**
  - Consequence: UI and HTTP/data concerns become entangled
  - Instead: always split into at least `feature` and `data-access` from the start
- **Calling `console.log`/`console.warn`/`console.error` directly from feature code**
  - Consequence: bypasses `LoggerService`'s single seam for level filtering, and the future backend-sink extension won't see call sites that bypass it
  - Instead: always call `LoggerService`
- **Using `HttpTestingController` inside a Facade or Signal Store test "to save time faking the Client"**
  - Consequence: the same HTTP call ends up asserted in two different, potentially inconsistent ways
  - Instead: fake the Client directly in a Facade test; fake the Facade directly in a Signal Store test
- **Lowering a coverage threshold to make a CI failure go away without investigating the cause**
  - Consequence: a genuine drop in tested code goes unnoticed
  - Instead: investigate why coverage dropped; only lower the threshold as a deliberate, reviewed decision

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill.md|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]

# Unittest TestCases

- [ ] WHEN `nx run-many -t lint` is executed THEN
  - [ ] `@nx/enforce-module-boundaries` reports no violations
- [ ] WHEN the codebase is searched for direct `console.*` calls outside `libs/shared/logging` THEN
  - [ ] none are found
- [ ] WHEN a project's test configuration is inspected THEN
  - [ ] it runs via Vitest, not Karma or Jest
- [ ] WHEN the codebase is searched for `HttpTestingController` usage THEN
  - [ ] every occurrence is inside a `{feature}.client.ts` spec file
- [ ] WHEN CI runs on a PR that drops coverage below the configured threshold THEN
  - [ ] the build fails, not just warns

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill.md|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]
