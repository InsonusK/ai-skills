---
name: solution-conformance-testing-in-angular
description: Extends TypeScript conformance testing with Angular TestBed component tests and Playwright browser UI tests through the same discovered make kinds and report contract.
whenToUse: Add component behavior and browser UI coverage to an Angular application that already uses the TypeScript Cucumber, coverage and mutation solution.
domain: skill
type: architecture
version: 20261008220000
updated: 20261008
tags:
  - skill/architecture/solution
  - solution/conformance-testing-in-angular
  - stack/typescript
  - framework/angular
  - concern/testing
creates:
  - tools/testing/kinds/components.sh
  - tools/testing/kinds/ui.sh
  - tools/testing/angular-results.mjs
  - vitest.components.config.mts
  - playwright.ui.config.ts
  - "{SourceRoot}/{Component}/test/{Component}.component.spec.ts"
  - "{SourceRoot}/{Component}/test/{Flow}.ui.spec.ts"
extends:
  - package.json
  - angular.json
  - tsconfig.spec.json
  - tsconfig.app.json
  - README.md
  - stryker.conf.json
  - tools/testing/kinds/mutation.sh
depends_on:
  - "[solution-conformance-testing-in-typescript](skills/testing/typescript/solution-conformance-testing-in-typescript.skill/solution-conformance-testing-in-typescript.skill.md)"
adr:
  - "[Angular test kinds](adr/angular-test-kinds.md)"
  - "[Complete Makefile example](adr/complete-makefile-example.md)"
---

# Goal
- Add runnable Angular component and browser UI suites to the TypeScript conformance-testing setup.
- **Kind delivery** — Deliver `components` and `ui` kinds, their native evidence, failure reports and README badges through the existing report assembler.
- Deliver separate source scopes for native UI tests and inherited Cucumber coverage/mutation.

# Capabilities
| Kind | Runner | Evidence |
| --- | --- | --- |
| `unit`, `mutation` | Inherited TypeScript tools | Gherkin conformance, domain coverage, domain mutation |
| `components` | Angular CLI → [Vitest](glossary/vitest.md) → [TestBed](glossary/testbed.md), [jsdom](glossary/jsdom.md) | Rendered DOM, bindings, inputs, outputs, async states; native component coverage |
| `ui` | [Playwright](glossary/playwright.md), Chromium | Served application, navigation, browser interactions, optional reviewed screenshot comparisons |

# Core Principles
- **Test boundary** — A scenario stays in Cucumber when it exercises a framework-independent business contract; a native component or UI spec exercises the Angular rendering or browser boundary.
- A TestBed test uses the real component template; a browser test visits the real served Angular application.
- **Native inventory** — These native suites extend the parent suite; they do not contribute Gherkin tags, steps or scenario counts to its living documentation.

# Boundaries
- The application already builds and has an Angular CLI `serve` target; application deployment and backend provisioning remain project responsibilities.
- The delivered native builder setup targets Angular CLI 22; older Angular/Karma or Nx executors need an explicit adapter decision rather than copying these commands unchanged.
- The delivered adapter assumes one Angular application with one test target; multi-application workspaces require an explicit project-selection/aggregation adapter.

# Adr
- [Angular test kinds](adr/angular-test-kinds.md): native Angular builder for compilation/TestBed and Playwright for browser evidence, two discovered kinds.

- [Complete Makefile example](adr/complete-makefile-example.md): one runnable Angular app executes all four kinds and generates its report through Make.

# Requirements
SOLUTION:
- [TypeScript conformance testing](skills/testing/typescript/solution-conformance-testing-in-typescript.skill/solution-conformance-testing-in-typescript.skill.md): apply its repository and package mutations first, including the unchanged shared core tools.

NPM:
- Matching Angular CLI/build/compiler packages and their supported Node/TypeScript versions.
- `vitest`, `@vitest/coverage-v8` of the same compatible version, and `jsdom` for the Angular unit-test builder.
- `@playwright/test` plus its matching Chromium installation for the UI runner.

# Template Skill Mutations
REPOSITORY:
- [Repository](Implementation/Repository.extend.md): copy the kind assets and configure the UI template.

PACKAGE:
- [Angular package](Implementation/{Package}.package.extend.md): extend Angular targets, dependencies and test source boundaries.
- [Component tests](Implementation/{Component}.component.spec.ts.create.md): author TestBed behavior assertions.
- [UI tests](Implementation/{Flow}.ui.spec.ts.create.md): author browser flows and reviewed visual comparisons.

# Workflow
## Apply and run
1. Apply the TypeScript dependency and the linked mutations above.
2. Confirm `make test-kinds` lists `unit`, `mutation`, `components`, `ui`.
3. Run `make test-kind-components` and `make test-kind-ui`; both run their complete suite in `check` and `report` mode, regardless of `DELTA_BASE`.
4. Run `make test-and-report` to assemble all kinds; open the component summary/coverage and native Playwright report from the common report index.

## Compilation, browser or assertion failure
1. The kind records the runner log and keeps its exit code.
2. The adapter writes its result, a red badge and a readable index even when the native JSON is missing or no tests ran.
3. The kind exits nonzero; `make test-report` can still assemble the available failure evidence.

## Async component state and visual change
1. Await TestBed stabilization or Playwright locator assertions, then assert the visible state; use controlled backend responses when the state depends on a request.
2. When a screenshot differs, inspect expected/actual/diff evidence in the UI report.
3. Follow the baseline review procedure in [UI tests](Implementation/{Flow}.ui.spec.ts.create.md#MUST); the normal kind never updates committed expectations.

# Ground truth
[The complete Angular example](example/README.md) builds and serves a real Link checker form over the inherited domain package. Its Makefile runs all four kinds and assembles the common report; [verification](verification.md) records actual executions and failure paths.

# Rules
## MUST
- [Repository rules](Implementation/Repository.extend.md#MUST)
- [Package rules](Implementation/{Package}.package.extend.md#MUST)
- [Component test rules](Implementation/{Component}.component.spec.ts.create.md#MUST)
- [UI test rules](Implementation/{Flow}.ui.spec.ts.create.md#MUST)

# Check list
- [ ] Shared core tools remain byte-identical; `make test-kinds` discovers all four kinds.
- [ ] Component and UI specs execute in distinct runners and stay outside production output and the inherited domain mutation scope.
- [ ] A deliberate DOM assertion failure produces a red component badge and native evidence.
- [ ] A deliberate browser assertion failure produces a red UI badge and native evidence.
- [ ] A missing result, missing browser or empty suite fails visibly.
- [ ] Caller-selected check/report directories contain all transient test evidence; published relative report links resolve.
- [ ] The UI baseline is reviewed, committed and read-only during normal runs.
