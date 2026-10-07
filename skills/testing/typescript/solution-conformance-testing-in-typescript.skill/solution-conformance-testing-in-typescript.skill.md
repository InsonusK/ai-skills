---
name: solution-conformance-testing-in-typescript
description: Sets up the TypeScript side of the Cucumber/coverage/mutation quality gate — @cucumber/cucumber for Gherkin scenarios, Vitest coverage for coverage, Stryker for mutation testing, and the make test-kind-unit/test-kind-mutation/test-report/test-and-report contract that downstream CI consumes
whenToUse: Set up or review the test suite of a framework-agnostic TypeScript package that must prove conformance to a Cucumber/Gherkin spec, add Gherkin scenarios and step definitions to an existing TypeScript package, or wire coverage and mutation testing into a TypeScript package's `make`/CI pipeline.
domain: skill
type: architecture
version: 20261007000000
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
  - "{Package}/features/step-definitions/{rule}.steps.ts"

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
- `make test-kind-unit` also writes `$TEST_KIND_DIR/result/scenarios.json` — every `.feature` entry with its type tag, status, and `@todo` reason — and `make test-report` renders it as `$TEST_REPORT_DIR/reports/scenarios/`, per [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-report).

# Core Principles
- Step definitions import from the package's `src/index.ts` public API, never from an internal module path directly.
- Step definitions call the package's real exported function/class; they never re-implement the rule under test.
- Coverage and mutation testing both run against the combined suite (Vitest unit tests plus Cucumber scenarios), not against either alone.

# Adr
- [[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/adr/testing-tool-choice|Testing tool choice]]
  - Selected variant: `@cucumber/cucumber` (Gherkin runner) + Vitest coverage (coverage) + Stryker (mutation testing)

# Requirements
SOLUTION:
- [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]
  - Defines the `make` command contract and normalized report format this solution implements concretely for TypeScript.

NPM:
- @cucumber/cucumber
  - Runs `.feature` files against `features/step-definitions/*.steps.ts`.
- vitest
  - Runs unit tests and produces the coverage report (`--coverage`, v8 provider).
- @stryker-mutator/core
  - Runs mutation testing against the package and reports a mutation score.
- tsx
  - Lets `@cucumber/cucumber` load TypeScript step definitions directly.

# Template Skill Mutations
REPOSITORY:
- [[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/Implementation/Repository.extend|Repository]] - extend - add the `Makefile` and normalization scripts implementing the `make test-kind-unit`/`test-kind-mutation`/`test-report`/`test-and-report` contract

PACKAGE:
- [[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/Implementation/{Package}.package.extend|{Package}]] - extend - add Cucumber/Vitest/Stryker scripts and config
  - [[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/Implementation/{Package}.package.extend/{rule}.steps.ts.create|{rule}.steps.ts]] - create - step definitions binding a `.feature` file to the package's real API

# Workflow
## Add conformance coverage for a new validation rule (happy path)
1. A `.feature` file describing the rule (e.g. `features/{rule}.feature`) is added or extended with `Given/When/Then` scenarios.
2. `features/step-definitions/{rule}.steps.ts` is created with `Given`/`When`/`Then` bindings that import from `src/index.ts` and call the real exported function/class.
3. `make test-kind-unit` runs `vitest run --coverage` and `cucumber-js` (both feeding the same coverage provider's output in a `report` run), and normalizes the result into `$TEST_KIND_DIR/result/unit-test.json` and `$TEST_KIND_DIR/result/scenarios.json` (plus `$TEST_KIND_DIR/result/coverage-test.json`).
4. `make test-kind-mutation` runs `stryker run` — across the whole package in a `report` run; in a `check` run scoped to files changed since `DELTA_BASE`, and skipped without one — and normalizes the result into `$TEST_KIND_DIR/result/mutation-test.json`.
5. `make test-report` assembles `$TEST_REPORT_DIR/` — `scenarios/` included — from `$TEST_KIND_DIR/result/*.json` and `$TEST_KIND_DIR/report/*`, ready to publish. `make test-and-report` runs all three targets in sequence.
6. Which of these `make` targets run on which trigger, and how `$TEST_REPORT_DIR/` gets published, is decided by the project's own CI configuration — not by this solution.

## Surviving mutant found (report path)
1. `make test-kind-mutation` reports a mutant that survived, as part of a post-merge, report-only CI run — mutation testing is not expected to block a pull request.
2. The mutation report published to GitHub Pages shows the survivor; it does not fail the workflow or block anything.
3. Whoever notices the survivor (via the report or the README's mutation-score badge) either strengthens the assertion in the corresponding scenario/step definition in a follow-up PR, or explicitly accepts it per [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#must).

# Rules
Each linked `#MUST` section below carries its own `Violation`/`Risk`/`Fix` at the target — this index only points to where the actual rule lives.

## MUST
- [[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/Implementation/Repository.extend#MUST|Repository]]
- [[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/Implementation/{Package}.package.extend#MUST|{Package}]]
  - [[skills/testing/typescript/solution-conformance-testing-in-typescript.skill/Implementation/{Package}.package.extend/{rule}.steps.ts.create#MUST|{rule}.steps.ts]]

# Check list
- [ ] `package.json` declares `test`, `coverage`, and `mutation` scripts backed by Vitest, Vitest coverage, and Stryker.
- [ ] Every `.feature` scenario has a matching step definition that imports from `src/index.ts` and calls production code.
- [ ] `make test-kind-unit`, `make test-kind-mutation`, `make test-report`, and `make test-and-report` exist at the repository root and support the toggles defined by [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract).
- [ ] `$TEST_KIND_DIR/result/*.json` — `scenarios.json` included, written on a red run too — and `$TEST_KIND_DIR/report/<kind>/` follow that same contract's schema.
- [ ] `@todo` scenarios are excluded from the run and listed as `todo` in `$TEST_REPORT_DIR/reports/scenarios/`.
- [ ] `make test-kind-unit` keeps cucumber-js's Cucumber Messages in `$TEST_KIND_DIR/report/tests/cucumber/` and renders `$TEST_KIND_DIR/report/tests/livingdoc/` per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#living-doc-report|the parent solution's living-doc report]], from `tools/livingdoc/`, never the project's `package.json`.
