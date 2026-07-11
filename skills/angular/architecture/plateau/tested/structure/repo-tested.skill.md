---
name: repo-tested
description: Nx workspace layout for the tested Angular application — adds a Playwright apps/platform-shell-e2e project, Vitest as the enforced unit/component test runner, coverage thresholds in CI, and per-layer test-mocking conventions, on top of the observable plateau's logging, auth, routing, forms, and data-access conventions. This is the "online-monolith" milestone.
domain: skill
type: template
plateau: tested
version: 20260711170000
tags:
  - skill/template/repo
  - plateau/tested
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
  - "[[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]]"
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
  - "[[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]]"
---

> **Deferred scope:** carried over from [[skills/angular/architecture/plateau/navigable/plateau-navigable.skill.md|navigable]] and [[skills/angular/architecture/plateau/authenticated/plateau-authenticated.skill.md|authenticated]] — `solution-app-routing`'s embeddable-module routing slice and `solution-authentication`'s `@platform/contracts` session-sharing slice both remain excluded, deferred to the future "platform" plateau. `solution-testing` introduces no Module-Federation-specific content of its own, so it adds no new deferred scope.
>
> This plateau is also the **"online-monolith" milestone**: a single deployable Nx application doing everything the online architecture needs — optimistic-update data flow, auth, routing, forms, structured logging with backend reporting, and enforced test coverage — with no offline support and no Module Federation host/embeddable-app split yet. The [[skills/angular/architecture/plateau/design-system/plateau-design-system.skill.md|design-system]] npm package is already a real dependency of `apps/platform-shell` by this point (see `platform-shell`'s own project skill's NPM Packages table) — plain consumption only; the federation-aware, version-negotiated consumption is added later by the "platform" plateau via `solution-design-system-application`, which (together with `solution-platform-embeddability`) is deliberately excluded from this plateau in full, since both solutions are 100% Module-Federation-scoped.

# Structure

## Workspace Structure

```
/apps
  /[platform-shell](./platform-shell/project-platform-shell.skill.md)
  /[platform-shell-e2e](./platform-shell-e2e/project-platform-shell-e2e.skill.md)      <- new (solution-testing)

/libs
  /shared
    /[ui](./shared-ui/project-shared-ui.skill.md)
    /[util](./shared-util/project-shared-util.skill.md)
    /[state](./shared-state/project-shared-state.skill.md)
    /[http-core](./shared-http-core/project-shared-http-core.skill.md)
    /[auth-ui](./shared-auth-ui/project-shared-auth-ui.skill.md)
    /[logging](./shared-logging/project-shared-logging.skill.md)
  /{feature}
    /[feature](./feature-feature/project-feature-feature.skill.md)
    /[data-access](./feature-data-access/project-feature-data-access.skill.md)
```

- `apps/platform-shell-e2e` hosts Playwright scenario-level specs. Tagged `type:e2e`, `scope:platform`.
- Every project's unit/component tests run via Vitest — Karma and Jest are no longer permitted as a project's test runner.

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[platform-shell/project-platform-shell.skill.md\|project-platform-shell.skill]] | The only deployable unit at this plateau. |
| /apps/platform-shell-e2e | [[platform-shell-e2e/project-platform-shell-e2e.skill.md\|project-platform-shell-e2e.skill]] | Playwright end-to-end scenario specs against the real built application. |
| /libs/shared/ui | [[shared-ui/project-shared-ui.skill.md\|project-shared-ui.skill]] | Reusable, app-specific UI composed from design-system primitives. |
| /libs/shared/util | [[shared-util/project-shared-util.skill.md\|project-shared-util.skill]] | Framework-agnostic pure helpers shared across features. |
| /libs/shared/state | [[shared-state/project-shared-state.skill.md\|project-shared-state.skill]] | Classical NgRx Store for global, cross-cutting state, including the full auth session lifecycle. |
| /libs/shared/http-core | [[shared-http-core/project-shared-http-core.skill.md\|project-shared-http-core.skill]] | Base HTTP service shared by every feature's Client. |
| /libs/shared/auth-ui | [[shared-auth-ui/project-shared-auth-ui.skill.md\|project-shared-auth-ui.skill]] | Permission-based UI primitives. |
| /libs/shared/logging | [[shared-logging/project-shared-logging.skill.md\|project-shared-logging.skill]] | `LoggerService`, pluggable `LogSink`s, bounded retry queue. |
| /libs/{feature}/feature | [[feature-feature/project-feature-feature.skill.md\|project-feature-feature.skill]] | Generic template: routed, presentational + container components (including forms and their component tests), feature-level Signal Store, own root-relative routes. |
| /libs/{feature}/data-access | [[feature-data-access/project-feature-data-access.skill.md\|project-feature-data-access.skill]] | Generic template: this feature's Facade/Client/Mapper/Errors layering, each with its own unit-test pattern. |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/platform-shell-e2e.project.create|platform-shell-e2e.project.create]]

## Nx Tag Taxonomy

New value on the existing `type` axis:

| Axis | New value | Meaning |
| ----- | ---------- | ------- |
| `type` | `e2e` | Playwright end-to-end test project |

`@nx/enforce-module-boundaries` allow-list is otherwise unchanged from [[skills/angular/architecture/plateau/observable/plateau-observable.skill.md|observable]].

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]

## Testing conventions (cross-cutting convention)

- Every Nx project runs its unit/component tests via Vitest — no project may configure Karma or Jest.
- End-to-end tests are written with Playwright, only inside `apps/platform-shell-e2e`.
- `HttpTestingController` is used only inside a feature's own `{feature}.client.ts` spec — every other layer fakes the layer directly beneath it (Facade fakes Client, Signal Store fakes Facade, component fakes Store/Facade).
- MSW is reserved for tests that deliberately span more than one architectural layer (a feature-level integration test) — never a substitute for faking the layer directly below the unit under test.
- CI enforces a minimum code-coverage threshold per project as a hard `error`, not a warning; the exact percentage is a configurable, deployment-specific parameter.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]

## Session, authorization, routing, forms, data-access, logging conventions

Unchanged from [[skills/angular/architecture/plateau/observable/plateau-observable.skill.md|observable]].

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]] - [[skills/angular/architecture/solutions/solution-logging-global.skill/Implementation/Repository.extend|Repository.extend]]

# Rules

## MUST
- Every Nx project MUST declare exactly one `type:*` tag and exactly one `scope:*` tag.
- Every lib MUST expose its public API through a single `index.ts` barrel.
- Business logic MUST NOT live in `apps/platform-shell`.
- Every routable `type:feature` project MUST export its `Routes` array from `index.ts`.
- New forms MUST use Signal Forms by default.
- A Client MUST catch every `HttpErrorResponse` and rethrow a typed domain error.
- The access token MUST only ever be held in the `shared-state` auth slice's in-memory field.
- A permission guard MUST be attached inside the feature's own routes.
- Every part of the application MUST log through `LoggerService` — no direct `console.*` call outside `libs/shared/logging`'s own `ConsoleLogSink`.
- The global `ErrorHandler` MUST be registered in `apps/platform-shell` via `{ provide: ErrorHandler, useClass: GlobalErrorHandler }`.
- Every Nx project MUST run its unit/component tests via Vitest — no project may configure Karma or Jest as its test runner.
- End-to-end tests MUST be written with Playwright, in the dedicated `type:e2e` project.
- `HttpTestingController` MUST be used only inside a feature's own `{feature}.client.ts` unit tests.
- MSW MUST be used only for tests that deliberately span more than one architectural layer.
- CI MUST enforce a minimum code-coverage threshold per project as a hard `error`.

## SHOULD
- New business features SHOULD be scaffolded as a `{feature}/feature` + `{feature}/data-access` pair from the start.
- The retry queue's three limit values SHOULD be configurable per deployment, not hardcoded.

## SHOULD NOT
- Existing Reactive Forms code SHOULD NOT be migrated to Signal Forms purely for consistency.

## MUST NOT
- MUST NOT place a routed business feature directly under `/apps`.
- A component or Signal Store method MUST NOT import a feature's Client directly, bypassing the Facade.
- A feature MUST NOT set `preload: true` on its own routes.
- `accessToken` MUST NOT be written to any persistent client storage.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

- **Single flat lib per feature instead of `feature` + `data-access` split**
  - Consequence: UI and HTTP/data concerns become entangled
  - Instead: always split into at least `feature` and `data-access` from the start
- **Calling `console.log`/`console.warn`/`console.error` directly from feature code**
  - Consequence: bypasses `LoggerService`'s single seam for level filtering and the PII safeguard
  - Instead: always call `LoggerService`
- **Using `HttpTestingController` inside a Facade or Signal Store test "to save time faking the Client"**
  - Consequence: reintroduces the duplicated-mock risk this solution's ADR exists to prevent — the same HTTP call ends up asserted in two different, potentially inconsistent ways
  - Instead: fake the Client directly in a Facade test; fake the Facade directly in a Signal Store test
- **Lowering a coverage threshold to make a CI failure go away without investigating the cause**
  - Consequence: defeats the purpose of enforcing coverage in the first place — a genuine drop in tested code goes unnoticed
  - Instead: investigate why coverage dropped; only lower the threshold as a deliberate, reviewed decision

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]] - [[skills/angular/architecture/solutions/solution-logging-base.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]

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
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]
