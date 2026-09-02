---
name: solution-app-testing
description: Vitest as the unit test runner, Playwright for e2e, TestBed for non-DOM business-layer units (Client, Facade, Signal Store), and a strict layer-by-layer HTTP mocking rule (HttpTestingController only at the Client, MSW only for genuine cross-layer integration tests). Deliberately does not cover UI/component/visual testing — see solution-ui-testing.
domain: skill
type: architecture
version: 20260902000000
tags:
  - skill/architecture/solution
  - stack/typescript
  - concern/testing
  - vitest
  - playwright
  - framework/angular
  - concern/architecture
  - solution/app-testing

whenToUse: when writing a test for a Client, Facade, or Signal Store, deciding which tool mocks an HTTP call in a given test, or setting up end-to-end coverage for a critical user journey
creates:
  - apps/platform-shell-e2e
extends:
  - libs/{feature}/data-access (spec/ test files)
  - libs/{feature}/feature (spec/ Signal Store test files)
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]]"
adr:
  - "[[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/adr/test-runner-choice.md|Test runner choice ADR]]"
  - "[[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/adr/e2e-framework-choice.md|E2E framework choice ADR]]"
  - "[[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/adr/testing-layers-and-mocking.md|Testing layers and mocking ADR]]"
---

> This solution deliberately does not cover UI/component/visual testing. A component only ever needs a signal/input on the way in and a rendered result/event on the way out to be tested — it never needs the business-layer mocking this solution establishes (`HttpTestingController`, faked Facade/Client). That entire concern — behavioral component tests, visual regression, and accessibility checks — is owned by [[skills/angular/architecture/v3.1/solutions/testing/solution-ui-testing.skill/solution-ui-testing.skill|solution-ui-testing]], which reuses this solution's Vitest/Playwright ADRs without re-arguing the tool choice.

# Goal

- Standardize on Vitest and Playwright, matching Angular's own current tooling direction
- Give every business-layer architectural layer (Client, Facade, Signal Store) a matching, well-scoped test pattern
- Eliminate the risk of duplicated, inconsistent HTTP mocks by giving each mocking tool exactly one layer it belongs to

# Capabilities

- Fast, ESM-native unit test runs via Vitest
- Reliable, multi-browser e2e coverage via Playwright, with strong debugging tools (trace viewer, video capture) for CI failures
- A single source of truth for what a given HTTP request/response looks like (the Client's own `HttpTestingController` tests) — no other test layer re-asserts or re-mocks the same request differently

# Core Principles

- Non-DOM units (Client, Facade, Signal Store, plain services) are tested via `TestBed`, faking the layer directly beneath the unit under test — never skipping a layer, never reaching further down than necessary
- `HttpTestingController` is used only inside `spec/{feature}.client.spec.ts` — the unit test for a feature's own `{feature}.client.ts` — nowhere else
- MSW is reserved for tests that deliberately span multiple layers (Signal Store → Facade → Client → network), and may double as shared mocks for local dev tooling
- End-to-end tests (Playwright) are reserved for a small number of critical, cross-cutting user journeys — not a substitute for unit/integration coverage
- Coverage thresholds are enforced in CI as an error, following the same enforcement pattern already established for bundle budgets in the "Lazy loading routing" solution

# Directory layout

All business-layer test files live under a `spec/` directory next to the source files they test. This keeps `{feature}.client.ts`, `{feature}.facade.ts`, and `{feature}.store.ts` free from test-related clutter.

```text
libs/{feature}/data-access/src/lib/
  service/
    {feature}.facade.ts
    spec/
      {feature}.facade.spec.ts
  api/
    {feature}.client.ts
    spec/
      {feature}.client.spec.ts

libs/{feature}/feature/src/lib/
  {feature}.store.ts
  spec/
    {feature}.store.spec.ts

apps/platform-shell-e2e/
  src/spec/
    {scenario-name}.e2e.spec.ts
  playwright.config.ts
```

The integration spec sits under the feature library because it exercises the whole `Signal Store → Facade → Client` chain. End-to-end specs live in the dedicated `apps/platform-shell-e2e` project.

# Adr

- [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/adr/test-runner-choice.md|Vitest instead of Jest or Karma/Jasmine]]
  - Selected variant: Vitest — Angular's own current default, matching the esbuild-based build pipeline already in use
- [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/adr/e2e-framework-choice.md|Playwright instead of Cypress]]
  - Selected variant: Playwright — true multi-browser coverage (including WebKit) and better fit for scenarios involving federated embeddable modules
- [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/adr/testing-layers-and-mocking.md|TestBed for non-DOM business-layer units; HttpTestingController only at the Client, MSW only for cross-layer integration tests]]
  - Selected variant: this layered strategy — chosen to give each HTTP mock exactly one source of truth

# Requirements

SOLUTION:
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]
  - Test patterns here directly test the Facade/Client structure that solution establishes
- [[skills/angular/architecture/v3.1/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]]
  - Signal Store test pattern here directly tests the store methods that solution establishes

NPM:
- vitest
  - Unit test runner
- @playwright/test
  - End-to-end testing framework
- msw
  - Network-level mocking for genuine cross-layer integration tests only, per [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/adr/testing-layers-and-mocking.md|Testing Layers And Mocking ADR]]

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Repository.extend|Repository]] - extend - enforce Vitest/Playwright workspace-wide, enforce coverage thresholds in CI, add the `type:e2e` tag

PROJECT:
- [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/platform-shell-e2e.project.create|apps/platform-shell-e2e]] - create - Playwright end-to-end test project for the platform shell

Artifact-level (generic patterns):
- [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Testing/{feature}.client.spec.ts.create|spec/{feature}.client.spec.ts]] - create - Client unit test via `TestBed` + `HttpTestingController`
- [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create|spec/{feature}.facade.spec.ts / spec/{feature}.store.spec.ts]] - create - Facade/Signal Store unit tests, each faking the layer directly beneath it
- [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Testing/{feature}.integration.spec.ts.create|spec/{feature}.integration.spec.ts]] - create - cross-layer integration test via MSW, reserved for genuine multi-layer scenarios
- [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Testing/{scenario-name}.e2e.spec.ts.create|apps/platform-shell-e2e/src/spec/{scenario-name}.e2e.spec.ts]] - create - Playwright end-to-end scenario in `apps/platform-shell-e2e`

# Workflow

## Testing a new feature end-to-end (happy path)

1. `spec/{feature}.client.spec.ts` gets a `TestBed` + `HttpTestingController` spec asserting exact request/response shape and DTO mapping — the one and only place this endpoint's HTTP contract is verified in detail.
2. `spec/{feature}.facade.spec.ts` gets a `TestBed` spec faking the Client, verifying business validation and orchestration.
3. `spec/{feature}.store.spec.ts` (Signal Store) gets a `TestBed` spec faking the Facade, verifying state transitions (`loading`, data, `error`).
4. The feature's components get their own UI-level test coverage — see [[skills/angular/architecture/v3.1/solutions/testing/solution-ui-testing.skill/solution-ui-testing.skill|solution-ui-testing]], not this solution.
5. If a genuinely cross-layer scenario needs verifying as a whole, one MSW-based integration spec (`spec/{feature}.integration.spec.ts`) covers it — not a substitute for the layer-specific tests above.
6. A small number of critical user journeys involving this feature get Playwright e2e coverage in `apps/platform-shell-e2e/src/spec`.

![Testing a new feature end-to-end (happy path)](skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/diagrams/testing-a-new-feature-end-to-end-happy-path.mmd)

## Misusing a mocking tool at the wrong layer (anti-pattern, caught in review)

1. An engineer, under time pressure, uses `HttpTestingController` inside a Signal Store test instead of faking the Facade.
2. This is flagged in review against [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Repository.extend#MUST]] — the store test now knows about the Client's HTTP contract, a layer it shouldn't need to know about, and duplicates what the Client's own test already verifies.
3. Fix: replace the `HttpTestingController` usage with a faked Facade, per [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create]].

# Rules

## MUST
- [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Repository.extend#MUST|Repository]]
- [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Testing/{feature}.client.spec.ts.create#MUST|spec/{feature}.client.spec.ts]]
- [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create#MUST|spec/{feature}.facade-and-store.spec.ts]]
- [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Testing/{feature}.integration.spec.ts.create#MUST|spec/{feature}.integration.spec.ts]]
- [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Testing/{scenario-name}.e2e.spec.ts.create#MUST|src/spec/{scenario-name}.e2e.spec.ts]]

## SHOULD
- Avoid — [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Repository.extend|See Repository.extend.md]] — using `HttpTestingController` outside a Client spec; lowering a coverage threshold to silence a CI failure.
- Avoid — [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Testing/{feature}.facade-and-store.spec.ts.create|See spec/{feature}.facade-and-store.spec.ts.create.md]] — a Signal Store test faking the Client instead of the Facade, skipping a layer.
- Avoid — [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Testing/{feature}.integration.spec.ts.create|See spec/{feature}.integration.spec.ts.create.md]] — reaching for the MSW integration pattern as the default way to test a Facade or Store.
- Avoid — [[skills/angular/architecture/v3.1/solutions/testing/solution-app-testing.skill/Implementation/Testing/{scenario-name}.e2e.spec.ts.create|See src/spec/{scenario-name}.e2e.spec.ts.create.md]] — writing a large number of fine-grained e2e tests instead of pushing detailed coverage down to cheaper layers.
- Avoid — **Reaching for `HttpTestingController` or a faked Facade/Client to test a component** — a component's own behavior never needs business-layer mocking; see [[skills/angular/architecture/v3.1/solutions/testing/solution-ui-testing.skill/solution-ui-testing.skill|solution-ui-testing]] instead.

# Check list

- [ ] Every project runs its tests via Vitest, none via Karma or Jest
- [ ] `apps/platform-shell-e2e` runs Playwright specs for a small set of critical user journeys
- [ ] `HttpTestingController` appears only in `spec/{feature}.client.spec.ts` files
- [ ] Every Facade/Signal Store test fakes only the layer directly beneath it
- [ ] CI enforces a coverage threshold as an error, not a warning
