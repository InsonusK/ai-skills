---
name: solution-conformance-testing-in-angular
description: Extends TypeScript conformance testing with Angular TestBed component tests and Playwright browser UI tests through the same discovered make kinds and report contract.
whenToUse: Add or maintain conformance testing in a non-Nx repository whose product is one Angular application, including applications consuming npm libraries from other repositories.
domain: skill
type: architecture
version: 20261010110000
updated: 20261009
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
  - "{SourceRoot}/{Component}/spec/{Component}.component.spec.ts"
  - "{SourceRoot}/{Component}/spec/{Flow}.ui.spec.ts"
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
  - "[Angular test kinds](./adr/angular-test-kinds.md)"
  - "[Complete Makefile example](./adr/complete-makefile-example.md)"
  - "[Chromium system dependencies](./adr/chromium-system-dependencies.md)"
  - "[Repository shape selection](./adr/repository-shape-selection.md)"
  - "[Browser spec discovery](./adr/browser-spec-discovery.md)"
---

# Goal
- Add runnable Angular component and browser UI suites to the TypeScript conformance-testing setup.
- **Kind delivery** — Deliver `components` and `ui` kinds, their native evidence, failure reports and README badges through the existing report assembler.
- Deliver separate source scopes for native UI tests and inherited Cucumber coverage/mutation.

# Capabilities
| Kind | Runner | Evidence |
| --- | --- | --- |
| `unit`, `mutation` | Inherited TypeScript tools | Gherkin conformance, domain coverage, domain mutation |
| `components` | Angular CLI → [Vitest](./glossary/vitest.md) → [TestBed](./glossary/testbed.md), [jsdom](./glossary/jsdom.md) | Rendered DOM, bindings, inputs, outputs, async states; native component coverage |
| `ui` | [Playwright](./glossary/playwright.md), Chromium | Served application, navigation, browser interactions, optional reviewed screenshot comparisons |

# Core Principles
- **Test boundary** — A scenario stays in Cucumber when it exercises a framework-independent business contract; a native component or UI spec exercises the Angular rendering or browser boundary.
- A TestBed test uses the real component template; a browser test visits the real served Angular application.
- **Native inventory** — These native suites extend the parent suite; they do not contribute Gherkin tags, steps or scenario counts to its living documentation.
- **Two folders, two kinds of test** — Angular-native specs (`*.component.spec.ts`, `*.ui.spec.ts`) live in the component's `spec/` folder, the layout of the Angular catalog's `solution-ui-testing`; Cucumber step files stay in `test/` beside `features/`, the layout of the TypeScript parent.

# Boundaries
- The application already builds and has an Angular CLI `serve` target; application deployment and backend provisioning remain project responsibilities.
- The delivered native builder setup targets Angular CLI 22; older Angular/Karma builders need a verified version adapter rather than copying these commands unchanged.
- The direct delivery assumes one Angular application with one test target. Library repositories use `solution-conformance-testing-in-angular-library`; Nx repositories use `solution-conformance-testing-in-angular-nx`, inheriting these spec and evidence rules.

# Adr
- [Angular test kinds](./adr/angular-test-kinds.md): native Angular builder for compilation/TestBed and Playwright for browser evidence, two discovered kinds.

- [Complete Makefile example](./adr/complete-makefile-example.md): one runnable Angular app executes all four kinds and generates its report through Make.

- [Chromium system dependencies](./adr/chromium-system-dependencies.md): initialization installs the browser runtime libraries through the pinned installer.

- [Repository shape selection](./adr/repository-shape-selection.md): choose one entry skill by what the repository holds.
- [Browser spec discovery](./adr/browser-spec-discovery.md): run UI, visual, computed-style and accessibility browser suffixes together.

# Requirements
SOLUTION:
- [TypeScript conformance testing](skills/testing/typescript/solution-conformance-testing-in-typescript.skill/solution-conformance-testing-in-typescript.skill.md): apply its repository and package mutations first, including the unchanged shared core tools.

NPM:
- Matching Angular CLI/build/compiler packages and their supported Node/TypeScript versions.
- `vitest`, `@vitest/coverage-v8` of the same compatible version, and `jsdom` for the Angular unit-test builder.
- `@playwright/test` plus its matching Chromium installation for the UI runner.

# Template Skill Mutations
REPOSITORY:
- [Repository](./Implementation/Repository.extend.md): copy the kind assets and configure the UI template.

PACKAGE:
- [Angular package](./Implementation/{Package}.package.extend.md): extend Angular targets, dependencies and test source boundaries.
- [Component tests](./Implementation/{Component}.component.spec.ts.create.md): author TestBed behavior assertions.
- [UI tests](./Implementation/{Flow}.ui.spec.ts.create.md): author browser flows and reviewed visual comparisons.

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
3. Follow the baseline review procedure in [UI tests](./Implementation/{Flow}.ui.spec.ts.create.md#MUST); the normal kind never updates committed expectations.

# Ground truth
[The complete Angular example](./examples/README.md) builds and serves a real Link checker form over the inherited domain package; its Makefile runs all four kinds and assembles the common report. Verified on 2026-10-09 with Node 24, Angular 22, Vitest 5, Playwright 1.64 and Chromium:
- `make init && make test-and-report` — exit `0`; 25/25 Cucumber scenarios, domain coverage 98.1%, domain mutation score 90.2%, 4/4 component tests, 4/4 browser tests with one screenshot comparison.
- `make test-and-report TEST_RUN_PURPOSE=check` with caller-chosen directories — every kind but mutation runs, mutation is skipped without `DELTA_BASE`, the domain coverage report is not published.
- A deliberately wrong component expectation — the run exits non-zero, the `components` badge is red (3/4), the other kinds still run and the report is assembled.
- Two UI runs one right after the other on one machine — both pass: each run serves the application on a free port of its own.
- `run-example.sh` and the base source-contract check pass with the shared catalog browser-suffix matcher; 25/25 scenarios, components/UI 4/4, living-doc tags and report links verified again.
- The `components` and `ui` report pages have the same sections in the application, library and Nx examples: result, evidence links, tests; `components` adds the coverage figure, `ui` the screenshot comparisons — the reference image always, the run's image and the difference after a failed comparison (checked by changing a style in the Nx example).
- Not verified: the `.devcontainer` image build, a workspace with several applications through this direct delivery, an Angular version before 22.

# Rules
## MUST
- [Repository rules](./Implementation/Repository.extend.md#MUST)
- [Package rules](./Implementation/{Package}.package.extend.md#MUST)
- [Component test rules](./Implementation/{Component}.component.spec.ts.create.md#MUST)
- [UI test rules](./Implementation/{Flow}.ui.spec.ts.create.md#MUST)

# Check list
- [ ] Shared core tools remain byte-identical; `make test-kinds` discovers all four kinds.
- [ ] Component and UI specs execute in distinct runners and stay outside production output and the inherited domain mutation scope.
- [ ] A deliberate DOM assertion failure produces a red component badge and native evidence.
- [ ] A deliberate browser assertion failure produces a red UI badge and native evidence.
- [ ] A missing result, missing browser or empty suite fails visibly.
- [ ] Caller-selected check/report directories contain all transient test evidence; published relative report links resolve.
- [ ] The UI baseline is reviewed, committed and read-only during normal runs.
