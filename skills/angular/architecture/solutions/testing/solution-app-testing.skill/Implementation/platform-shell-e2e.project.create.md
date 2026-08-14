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
| `src/spec/{scenario-name}.e2e.spec.ts` | One spec per real user scenario, per [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/Implementation/Testing/{scenario-name}.e2e.spec.ts.create]]. |
| `playwright.config.ts` | Playwright configuration: base URL, browsers, CI reporter. |

# NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @playwright/test | latest stable | E2E test runner and browser automation |

# Rules

## MUST
- The project MUST contain only scenario-level tests that cross at least one routing or feature boundary.
- Each spec MUST run against the real built application, not against mocked backend or isolated components.

## MUST NOT
- The project MUST NOT contain unit or integration tests — those belong in `libs/{feature}/data-access/src/lib/spec` or `libs/{feature}/feature/src/lib/spec` per this solution's other implementation files.

# Anti-patterns

- **Testing a single component or service in the e2e project**
  - Consequence: slow, flaky tests that duplicate faster test layers
  - Instead: keep e2e specs focused on complete user scenarios

# Check list

- [ ] `apps/platform-shell-e2e` is configured with Playwright
- [ ] Every e2e spec represents a complete user scenario
- [ ] CI runs the e2e suite against the production-like build

# Unittest TestCases

- [ ] WHEN the e2e suite runs in CI THEN
  - [ ] every spec exercises a real deployed/standalone build
  - [ ] failures produce screenshots/traces for diagnosis
