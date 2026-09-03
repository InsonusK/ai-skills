---
description: Add Vitest as the workspace test runner, a Playwright e2e project, enforced coverage thresholds in CI, and the file-naming/placement conventions for each test layer
element_kind: repository
change_kind: extend
tags:
  - solution/app-testing
  - element/monolith-repository
---

# Structure

## Workspace Structure

```
/apps
  /platform-shell
  /platform-shell-e2e     <- new, Playwright e2e project
/libs
  /shared
    /...
  /{feature}
    /feature
      /src/lib/
        {feature}.store.ts
        spec/{feature}.store.spec.ts        <- Signal Store unit tests (TestBed)
        spec/{feature}.integration.spec.ts    <- cross-layer integration tests (MSW)
    /data-access
      /src/lib/
        {feature}.client.ts
        {feature}.facade.ts
        spec/{feature}.client.spec.ts       <- Client unit tests (HttpTestingController)
        spec/{feature}.facade.spec.ts       <- Facade unit tests (TestBed)
```

## Directory and project skills

| Directory | Description |
| ---------- | ----------- |
| /apps/platform-shell-e2e | New Nx project, tagged `type:e2e`, `scope:platform`. Playwright specs exercising the running application end-to-end. |
| /libs/{feature}/data-access/src/lib/spec/*.spec.ts | `TestBed`-based unit tests for `{feature}.client.ts` (using `HttpTestingController`) and `{feature}.facade.ts` (faking the Client). |
| /libs/{feature}/feature/src/lib/spec/*.store.spec.ts | Signal Store unit tests (`TestBed`, faking the Facade). Component-level test files in this same directory belong to [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]], not this solution. |
| /libs/{feature}/feature/src/lib/spec/*.integration.spec.ts | Cross-layer integration tests using MSW, reserved for genuine multi-layer scenarios. |

# Nx tag taxonomy — extension

| Axis | New value | Meaning |
| ----- | ---------- | ------- |
| `type` | `e2e` | Playwright end-to-end test project |

# Rules

## MUST
- Every Nx project runs its unit tests via Vitest — no Karma, no Jest.
  - Risk: a mixed runner set means two config models, two assertion dialects, and CI steps that behave differently per project.
  - Fix: the `@analogjs/vitest-angular` (or Angular 22 `@angular/build:unit-test`) runner for every project; per [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/adr/test-runner-choice.md|test-runner-choice]].
- End-to-end tests are Playwright, in a dedicated `type:e2e` project.
  - Risk: e2e specs scattered into feature libs slow every `nx affected` run and blur the unit/e2e boundary.
  - Fix: one `apps/platform-shell-e2e` (`type:e2e`, `scope:platform`); per [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/adr/e2e-framework-choice.md|e2e-framework-choice]].
- `HttpTestingController` is used only inside `spec/{feature}.client.spec.ts`.
  - Risk: HTTP expectations in a Facade or store test couple that test to transport detail two layers below what it verifies.
  - Fix: only the Client spec asserts the wire; everything above fakes the layer directly beneath it; per [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/adr/testing-layers-and-mocking.md|testing-layers-and-mocking]].
- MSW is used only for tests that deliberately span more than one layer.
  - Risk: MSW as a default mock hides which layer a test actually exercises and duplicates the Client spec's job.
  - Fix: MSW for `{feature}.integration.spec.ts` only; unit tests fake the immediate collaborator.
- CI enforces a per-project minimum coverage threshold as an `error`, not a `warning`.
  - Risk: a `warning` threshold silently erodes — a real regression in coverage ships.
  - Fix: fail the build below the configured percentage (the number is deployment-specific), same pattern as the bundle budgets.

# Unittest TestCases

- [ ] WHEN a project's test configuration is inspected THEN
  - [ ] it runs via Vitest, not Karma or Jest
- [ ] WHEN the codebase is searched for `HttpTestingController` usage THEN
  - [ ] every occurrence is inside a `spec/{feature}.client.spec.ts` file

## SHOULD
- **Using `HttpTestingController` inside a Facade or Signal Store test "to save time faking the Client"** — Consequence: reintroduces exactly the duplicated-mock risk this solution's ADR exists to prevent — the same HTTP call ends up asserted in two different, potentially inconsistent ways — Instead: fake the Client directly in a Facade test; fake the Facade directly in a Signal Store test
- **Lowering a coverage threshold to make a CI failure go away without investigating the cause** — Consequence: defeats the purpose of enforcing coverage in the first place — a genuine drop in tested code goes unnoticed — Instead: investigate why coverage dropped; only lower the threshold as a deliberate, reviewed decision
