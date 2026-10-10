---
name: solution-conformance-testing-in-typescript
description: Sets up the TypeScript side of the Cucumber/coverage/mutation quality gate — @cucumber/cucumber for Gherkin scenarios, c8 for coverage, Stryker for mutation testing, and the make test-kind-unit/test-kind-mutation/test-report/test-and-report contract that downstream CI consumes
whenToUse: Set up or review the test suite of a framework-agnostic TypeScript package that must prove conformance to a Cucumber/Gherkin spec, add Gherkin scenarios and step definitions to an existing TypeScript package, or wire coverage and mutation testing into a TypeScript package's `make`/CI pipeline.
domain: skill
type: architecture
version: 20261009210000
tags:
  - solution/conformance-testing-in-typescript
  - skill/architecture/solution
  - typescript
  - concern/testing
  - concern/testing/bdd
  - cucumber
  - concern/testing/mutation
  - stack/typescript

creates:
  - "{Package}/features/{rule}.feature"
  - "{Package}/test/{rule}.steps.ts"

  - tools/testing/normalize-scenarios.sh
  - tools/testing/messages-results.jq
extends:
  - "{Package}/package.json"
  - README.md
depends_on:
  - "[[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
adr:
  - "[[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/adr/testing-tool-choice|Testing tool choice]]"
---

# Goal
- Give a framework-agnostic TypeScript package the concrete tooling to run the three-layer gate defined by [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md): Gherkin scenarios, code coverage, mutation testing.
- Expose that tooling behind the `make test-kind-unit`/`make test-kind-mutation`/`make test-report`/`make test-and-report` contract so any CI workflow can wire it in without knowing anything TypeScript-specific.
- This solution targets plain, framework-agnostic TypeScript packages (a validation library consumed by any frontend). A UI framework's own component/e2e testing (e.g. Angular's Vitest/Playwright setup) is a separate concern — see that framework's own testing solution instead.

# Capabilities
- Gherkin `.feature` files execute against the package's real exported functions/classes via `@cucumber/cucumber` step definitions.
- `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=<ref>` fails fast on a changed line's surviving mutant, without paying for a full-package mutation run on every call.
- `make test-kind-unit TEST_RUN_PURPOSE=report` and `make test-report` give `master` an up-to-date coverage/mutation-score report and the data the README badges are generated from.
- `make test-kind-unit` also writes `$TEST_KIND_DIR/result/scenarios.json` — every `.feature` entry with its type and category, its status, and `@status/todo` reason — the unit kind validates its tags and the living doc includes excluded scenarios from it, per [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-inventory).

# Core Principles
- Features and tests sit beside their production module, per [cucumber-testing-in-typescript](skills/testing/typescript/cucumber-testing-in-typescript.skill/cucumber-testing-in-typescript.skill.md); steps call adjacent real modules, and contract scenarios call the public entry point.
- Step definitions call the package's real exported function/class; they never re-implement the rule under test.
- Every test of the package is a Cucumber scenario: coverage and mutation testing run against that one suite. Tests that need a UI framework — components, pixels, a browser — are other test kinds, defined by that framework's testing solution.

# Adr
- [[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/adr/testing-tool-choice|Testing tool choice]]
  - Selected variant: `@cucumber/cucumber` (Gherkin runner) + `c8` (coverage of that run) + Stryker (mutation testing)

# Requirements
SOLUTION:
- [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]
  - Defines the `make` command contract and normalized report format this solution implements concretely for TypeScript.

NPM:
- @cucumber/cucumber
  - Runs `.feature` files against `src/{package}/test/*.steps.ts`.
- c8
  - Measures the coverage of the `cucumber-js` run (V8 coverage, HTML and summary reports).
- @stryker-mutator/core
  - Runs mutation testing against the package and reports a mutation score.
- tsx
  - Lets `@cucumber/cucumber` load TypeScript step definitions directly.

# Template Skill Mutations
REPOSITORY:
- [[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/Implementation/Repository.extend|Repository]] - extend - add the `Makefile` and normalization scripts implementing the `make test-kind-unit`/`test-kind-mutation`/`test-report`/`test-and-report` contract

PACKAGE:
- [[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/Implementation/{Package}.package.extend|{Package}]] - extend - add the Cucumber, `c8` and Stryker dependencies and config
  - [[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/Implementation/{Package}.package.extend/{rule}.feature.create|{rule}.feature]] - create - tagged scenarios beside the production module
  - [[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/Implementation/{Package}.package.extend/{rule}.steps.ts.create|{rule}.steps.ts]] - create - step definitions binding a `.feature` file to the package's real API

# Workflow
## Add conformance coverage for a new validation rule (happy path)
1. A `.feature` file describing the rule (e.g. `src/{package}/features/{rule}.feature`) is added or extended with `Given/When/Then` scenarios.
2. `src/{package}/test/{rule}.steps.ts` is created with `Given`/`When`/`Then` bindings that import the adjacent real module and call the real exported function/class.
3. `make test-kind-unit` runs `cucumber-js` — under `c8` in a `report` run — and normalizes the result into `$TEST_KIND_DIR/result/unit-test.json` and `$TEST_KIND_DIR/result/scenarios.json` (plus `$TEST_KIND_DIR/result/coverage-test.json`).
4. `make test-kind-mutation` runs `stryker run` — across the whole package in a `report` run; in a `check` run scoped to the `src/**/*.ts` files changed since `DELTA_BASE`, and skipped without one or when none changed — and normalizes the result into `$TEST_KIND_DIR/result/mutation-test.json`.
5. `make test-report` gathers every kind's `report/` and `badges/` into `$TEST_REPORT_DIR/`, ready to publish. `make test-and-report` runs all three targets in sequence.
6. Which of these `make` targets run on which trigger, and how `$TEST_REPORT_DIR/` gets published, is decided by the project's own CI configuration — not by this solution.

## Surviving mutant found (report path)
1. `make test-kind-mutation` reports a mutant that survived, as part of a post-merge, report-only CI run — mutation testing is not expected to block a pull request.
2. The mutation report published to GitHub Pages shows the survivor; it does not fail the workflow or block anything.
3. Whoever notices the survivor (via the report or the README's mutation-score badge) either strengthens the assertion in the corresponding scenario/step definition in a follow-up PR, or explicitly accepts it per [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#must).

# Ground truth
[`examples/`](./examples/) ports the Python reference's eight features beside real TypeScript production modules: URL checks, extraction, batch summary, CLI, file store, record mapping, result contract and package metadata. All seven type tags, seven categories, two todos with reasons, one broken scenario and one validated scenario are present. The unchanged validated scenario is `store.feature` / `A stored check is read back`, for owner confirmation.

Verified on 2026-10-08 with Node 24, cucumber-js 10, c8 10, StrykerJS 8 and tsx 4:
- `make init && make test-and-report`: exit 0; 25/25 Cucumber scenarios, coverage 99.31%, mutation score 90.2% (101 killed, 11 survived). Python also has one plain whitespace test, so its test count is 26.
- `npm run build && npm pack --dry-run`: only production `dist/` output, README and package metadata are shipped; no feature or step file is compiled into the distribution.
- `run-example.sh`: report and caller-selected check directories, living-doc completeness, zero broken links, and delta mutation pass.
- On an isolated copy, a broken scenario fails the unit kind while `make test-report` succeeds and reports it as failed; removing a category names the offending scenario. Delta mutation evaluates only the changed `extractor.ts` (93.3%).
- The store uses a per-instance promise queue; concurrent async writers complete before assertions, with no sleeps or network dependencies.

# Rules
Each linked `#MUST` section below carries its own `Violation`/`Risk`/`Fix` at the target — this index only points to where the actual rule lives.

## MUST
- [[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/Implementation/Repository.extend#MUST|Repository]]
- [[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/Implementation/{Package}.package.extend#MUST|{Package}]]
  - [[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/Implementation/{Package}.package.extend/{rule}.steps.ts.create#MUST|{rule}.steps.ts]]

# Check list
- [ ] `package.json` lists `@cucumber/cucumber`, `tsx`, `c8`, `@stryker-mutator/core` as dev dependencies; `cucumber.mjs` and `stryker.conf.json` are in the package root.
- [ ] Every `.feature` scenario has a matching step definition that imports the adjacent real module and calls production code.
- [ ] `make test-kind-unit`, `make test-kind-mutation`, `make test-report`, and `make test-and-report` exist at the repository root and support the toggles defined by [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract).
- [ ] `$TEST_KIND_DIR/result/*.json` — `scenarios.json` included, written on a red run too — and `$TEST_KIND_DIR/report/<kind>/` follow that same contract's schema.
- [ ] `@status/todo` and `@status/broken` scenarios are excluded from the run and shown with their tags and reasons in the living doc.
- [ ] `make test-kind-unit` keeps cucumber-js's Cucumber Messages in `$TEST_KIND_DIR/report/tests/cucumber/` and renders `$TEST_KIND_DIR/report/tests/livingdoc/` per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#living-doc-report|the parent solution's living-doc report]], from `tools/livingdoc/`, never the project's `package.json`.
