---
name: project-platform-shell-e2e
description: Playwright end-to-end test project for the platform shell — scenario-level specs exercising real user flows against the built application
domain: skill
type: template
plateau: offline-monolith
project_kind: application
version: 20260711200000
tags:
  - skill/template/project
  - plateau/offline-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]]"
---

# Goal

- Provide a dedicated project for cross-cutting, browser-based end-to-end tests of the platform shell and its integrated features

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/platform-shell-e2e.project.create|platform-shell-e2e.project.create]]

# Core Principles

- Every spec runs against the real built application, never a mocked backend or isolated component
- The suite stays focused on a small number of critical, cross-cutting user journeys — detailed logic coverage is pushed down to unit/component/integration tests

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/platform-shell-e2e.project.create|platform-shell-e2e.project.create]]

# Structure

## Project Structure

```
/apps/platform-shell-e2e
  /src
    /e2e
      [{scenario-name}.e2e.spec.ts](./classes/class-scenario-name-e2e-spec.skill.md)
  playwright.config.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| src/e2e/{scenario-name}.e2e.spec.ts | One spec per real user scenario | [[skills/angular/architecture/plateau/plateau-offline-monolith.skill/structure/platform-shell-e2e/classes/class-scenario-name-e2e-spec.skill\|class-scenario-name-e2e-spec]] |
| playwright.config.ts | Base URL, browsers, CI reporter | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/platform-shell-e2e.project.create|platform-shell-e2e.project.create]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @playwright/test | latest stable | E2E test runner and browser automation |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/platform-shell-e2e.project.create|platform-shell-e2e.project.create]]

## What Does NOT Belong Here

- Unit or integration tests — those belong in `libs/{feature}/data-access` or `libs/{feature}/feature`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/platform-shell-e2e.project.create|platform-shell-e2e.project.create]]

## Allowed Dependencies

- None — drives the built application through the browser only, no source-level Nx project dependency

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/platform-shell-e2e.project.create|platform-shell-e2e.project.create]]

# Rules

## MUST
- The project MUST contain only scenario-level tests that cross at least one routing or feature boundary.
- Each spec MUST run against the real built application.

## MUST NOT
- The project MUST NOT contain unit or integration tests.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/platform-shell-e2e.project.create|platform-shell-e2e.project.create]]

# Anti-patterns

- **Testing a single component or service in the e2e project**
  - Consequence: slow, flaky tests that duplicate faster test layers
  - Instead: keep e2e specs focused on complete user scenarios

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/platform-shell-e2e.project.create|platform-shell-e2e.project.create]]

# Check list

- [ ] `apps/platform-shell-e2e` is configured with Playwright
- [ ] Every e2e spec represents a complete user scenario
- [ ] CI runs the e2e suite against the production-like build

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/platform-shell-e2e.project.create|platform-shell-e2e.project.create]]
