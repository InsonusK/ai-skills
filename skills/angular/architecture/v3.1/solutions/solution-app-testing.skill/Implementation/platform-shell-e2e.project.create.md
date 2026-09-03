---
description: Playwright end-to-end test project for the platform shell, containing scenario-level specs that exercise real user flows
name: platform-shell-e2e
project_kind: application
element_kind: project
change_kind: create
tags:
  - solution/app-testing
  - element/platform-shell-e2e-project
---

# Goals

- Provide a dedicated project for cross-cutting, browser-based end-to-end tests of the platform shell and its integrated features

# Structure

## Project Structure

```
/apps/platform-shell-e2e
  /src
    /spec
      {scenario-name}.e2e.spec.ts
  playwright.config.ts
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| `src/spec/{scenario-name}.e2e.spec.ts` | One spec per real user scenario, per [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{scenario-name}.e2e.spec.ts.create]]. |
| `playwright.config.ts` | Playwright configuration: base URL, browsers, CI reporter. |

# NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @playwright/test | latest stable | E2E test runner and browser automation |

# Rules

## MUST
- The project contains only scenario-level tests that cross at least one routing or feature boundary.
  - Risk: a component-level assertion in an e2e project pays the full browser cost for something a Testing Library spec covers faster.
  - Fix: e2e specs test a user journey end to end; single-component behaviour stays in the feature lib's `spec/`.
- Each spec runs against the real built application, not a mocked backend or isolated components.
  - Risk: an e2e test against mocks verifies the mocks, not the deployed app.
  - Fix: Playwright's `webServer` serves the production build; the backend is a real (or contract-stable) environment.
- Never contain unit or integration tests here.
  - Risk: they slow every `nx affected` run and live far from the code they cover.
  - Fix: unit/integration specs live in `libs/{feature}/*/src/lib/spec/`.
## SHOULD
- **Testing a single component or service in the e2e project** — Consequence: slow, flaky tests that duplicate faster test layers — Instead: keep e2e specs focused on complete user scenarios

# Check list

- [ ] `apps/platform-shell-e2e` is configured with Playwright
- [ ] Every e2e spec represents a complete user scenario
- [ ] CI runs the e2e suite against the production-like build

# Unittest TestCases

- [ ] WHEN the e2e suite runs in CI THEN
  - [ ] every spec exercises a real deployed/standalone build
  - [ ] failures produce screenshots/traces for diagnosis
