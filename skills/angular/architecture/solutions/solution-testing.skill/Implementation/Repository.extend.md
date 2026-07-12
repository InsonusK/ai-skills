---
description: Add Vitest as the workspace test runner, a Playwright e2e project, enforced coverage thresholds in CI, and the file-naming/placement conventions for each test layer
element_kind: repository
change_kind: extend
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
      /src/lib/**/*.spec.ts        <- component tests (Testing Library)
    /data-access
      /src/lib/**/*.spec.ts        <- Client/Facade unit tests (TestBed)
```

## Directory and project skills

| Directory | Description |
| ---------- | ----------- |
| /apps/platform-shell-e2e | New Nx project, tagged `type:e2e`, `scope:platform`. Playwright specs exercising the running application end-to-end. |
| /libs/{feature}/data-access/**/*.spec.ts | `TestBed`-based unit tests for `{feature}.client.ts` (using `HttpTestingController`) and `{feature}.facade.ts` (faking the Client). |
| /libs/{feature}/feature/**/*.spec.ts | Signal Store unit tests (`TestBed`, faking the Facade) and component tests (Testing Library, faking the Signal Store). |

# Nx tag taxonomy — extension

| Axis | New value | Meaning |
| ----- | ---------- | ------- |
| `type` | `e2e` | Playwright end-to-end test project |

# Rules

## MUST
- Every Nx project MUST run its unit/component tests via Vitest — no project may configure Karma or Jest as its test runner, per [[skills/angular/architecture/solutions/solution-testing.skill/adr/test-runner-choice]].
- End-to-end tests MUST be written with Playwright, in a dedicated `type:e2e` project, per [[skills/angular/architecture/solutions/solution-testing.skill/adr/e2e-framework-choice]].
- `HttpTestingController` MUST be used only inside a feature's own `{feature}.client.ts` unit tests — no other test may use it, per [[skills/angular/architecture/solutions/solution-testing.skill/adr/testing-layers-and-mocking]].
- MSW MUST be used only for tests that deliberately span more than one architectural layer (e.g. a feature-level integration test), never as a substitute for faking the layer directly below the unit under test.
- CI MUST enforce a minimum code coverage threshold per project (`error`, not `warning`, consistent with the bundle-budget enforcement pattern from the "Lazy loading routing" solution); the exact percentage is a configurable, deployment-specific parameter.

# Anti-patterns

- **Using `HttpTestingController` inside a Facade or Signal Store test "to save time faking the Client"**
  - Consequence: reintroduces exactly the duplicated-mock risk this solution's ADR exists to prevent — the same HTTP call ends up asserted in two different, potentially inconsistent ways
  - Instead: fake the Client directly in a Facade test; fake the Facade directly in a Signal Store test

- **Lowering a coverage threshold to make a CI failure go away without investigating the cause**
  - Consequence: defeats the purpose of enforcing coverage in the first place — a genuine drop in tested code goes unnoticed
  - Instead: investigate why coverage dropped; only lower the threshold as a deliberate, reviewed decision

# Unittest TestCases

- [ ] WHEN a project's test configuration is inspected THEN
  - [ ] it runs via Vitest, not Karma or Jest
- [ ] WHEN the codebase is searched for `HttpTestingController` usage THEN
  - [ ] every occurrence is inside a `{feature}.client.ts` spec file
