---
name: solution-conformance-testing-in-typescript
description: Sets up the TypeScript side of the Cucumber/coverage/mutation quality gate — @cucumber/cucumber for Gherkin scenarios, Vitest coverage for coverage, Stryker for mutation testing, and the make unit-test/mutation-test/test-report/test-and-report contract that downstream CI consumes
whenToUse: Set up or review the test suite of a framework-agnostic TypeScript package that must prove conformance to a Cucumber/Gherkin spec, add Gherkin scenarios and step definitions to an existing TypeScript package, or wire coverage and mutation testing into a TypeScript package's `make`/CI pipeline.
domain: skill
type: architecture
version: 2
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
  - Makefile
  - scripts/normalize-scenarios.sh
  - scripts/messages-results.jq
extends:
  - "{Package}/package.json"
  - README.md
depends_on:
  - "[[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
adr:
  - "[[skills/typescript/testing/solution-conformance-testing-in-typescript.skill/adr/testing-tool-choice|Testing tool choice]]"
---

# Goal
- Give a framework-agnostic TypeScript package the concrete tooling to run the three-layer gate defined by [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md): Gherkin scenarios, code coverage, mutation testing.
- Expose that tooling behind the `make unit-test`/`make mutation-test`/`make test-report`/`make test-and-report` contract so any CI workflow can wire it in without knowing anything TypeScript-specific.
- This solution targets plain, framework-agnostic TypeScript packages (a validation library consumed by any frontend). A UI framework's own component/e2e testing (e.g. Angular's Vitest/Playwright setup) is a separate concern — see that framework's own testing solution instead.

# Capabilities
- Gherkin `.feature` files execute against the package's real exported functions/classes via `@cucumber/cucumber` step definitions.
- `make mutation-test ONLY_DELTA=true DELTA_BASE=<ref>` fails fast on a changed line's surviving mutant, without paying for a full-package mutation run on every call.
- `make unit-test WITH_CODE_COVERAGE=true` and `make test-report` give `master` an up-to-date coverage/mutation-score report and the data the README badges are generated from.
- `make unit-test` also writes `tmp/result/scenarios.json` — every `.feature` entry with its type tag, status, and `@todo` reason — and `make test-report` renders it as `public/scenarios/`, per [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-report).

# Core Principles
- Step definitions import from the package's `src/index.ts` public API, never from an internal module path directly.
- Step definitions call the package's real exported function/class; they never re-implement the rule under test.
- Coverage and mutation testing both run against the combined suite (Vitest unit tests plus Cucumber scenarios), not against either alone.

# Adr
- [[skills/typescript/testing/solution-conformance-testing-in-typescript.skill/adr/testing-tool-choice|Testing tool choice]]
  - Selected variant: `@cucumber/cucumber` (Gherkin runner) + Vitest coverage (coverage) + Stryker (mutation testing)

# Requirements
SOLUTION:
- [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]
  - Defines the `make` command contract and normalized report format this solution implements concretely for TypeScript.

NPM:
- @cucumber/cucumber
  - Runs `.feature` files against `features/step-definitions/*.steps.ts`.
- vitest
  - Runs unit tests and produces the coverage report (`--coverage`, v8 provider).
- @stryker-mutator/core
  - Runs mutation testing against the package and reports a mutation score.
- ts-node (or tsx)
  - Lets `@cucumber/cucumber` load TypeScript step definitions directly.

# Template Skill Mutations
REPOSITORY:
- [[skills/typescript/testing/solution-conformance-testing-in-typescript.skill/Implementation/Repository.extend|Repository]] - extend - add the `Makefile` and normalization scripts implementing the `make unit-test`/`mutation-test`/`test-report`/`test-and-report` contract

PACKAGE:
- [[skills/typescript/testing/solution-conformance-testing-in-typescript.skill/Implementation/{Package}.package.extend|{Package}]] - extend - add Cucumber/Vitest/Stryker scripts and config
  - [[skills/typescript/testing/solution-conformance-testing-in-typescript.skill/Implementation/{Package}.package.extend/{rule}.steps.ts.create|{rule}.steps.ts]] - create - step definitions binding a `.feature` file to the package's real API

# Workflow
## Add conformance coverage for a new validation rule (happy path)
1. A `.feature` file describing the rule (e.g. `features/{rule}.feature`) is added or extended with `Given/When/Then` scenarios.
2. `features/step-definitions/{rule}.steps.ts` is created with `Given`/`When`/`Then` bindings that import from `src/index.ts` and call the real exported function/class.
3. `make unit-test` runs `vitest run --coverage` and `cucumber-js` (both feeding the same coverage provider's output when `WITH_CODE_COVERAGE=true`), and normalizes the result into `tmp/result/unit-test.json` and `tmp/result/scenarios.json` (plus `tmp/result/coverage-test.json`).
4. `make mutation-test` runs `stryker run` — scoped to changed files via Stryker's incremental/since mode when called with `ONLY_DELTA=true DELTA_BASE=<ref>`, or across the whole package otherwise — and normalizes the result into `tmp/result/mutation-test.json`.
5. `make test-report` assembles `public/` — `scenarios/` included — from `tmp/result/*.json` and `tmp/report/*`, ready to publish. `make test-and-report` runs all three targets in sequence.
6. Which of these `make` targets run on which trigger, and how `public/` gets published, is decided by the project's own CI configuration — not by this solution.

## Surviving mutant found (report path)
1. `make mutation-test` reports a mutant that survived, as part of a post-merge, report-only CI run — mutation testing is not expected to block a pull request.
2. The mutation report published to GitHub Pages shows the survivor; it does not fail the workflow or block anything.
3. Whoever notices the survivor (via the report or the README's mutation-score badge) either strengthens the assertion in the corresponding scenario/step definition in a follow-up PR, or explicitly accepts it per [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#must).

# Rules
Each linked `#MUST` section below carries its own `Violation`/`Risk`/`Fix` at the target — this index only points to where the actual rule lives.

## MUST
- [[skills/typescript/testing/solution-conformance-testing-in-typescript.skill/Implementation/Repository.extend#MUST|Repository]]
- [[skills/typescript/testing/solution-conformance-testing-in-typescript.skill/Implementation/{Package}.package.extend#MUST|{Package}]]
  - [[skills/typescript/testing/solution-conformance-testing-in-typescript.skill/Implementation/{Package}.package.extend/{rule}.steps.ts.create#MUST|{rule}.steps.ts]]

# Check list
- [ ] `package.json` declares `test`, `coverage`, and `mutation` scripts backed by Vitest, Vitest coverage, and Stryker.
- [ ] Every `.feature` scenario has a matching step definition that imports from `src/index.ts` and calls production code.
- [ ] `make unit-test`, `make mutation-test`, `make test-report`, and `make test-and-report` exist at the repository root and support the toggles defined by [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract).
- [ ] `tmp/result/*.json` — `scenarios.json` included, written on a red run too — and `tmp/report/<kind>/` follow that same contract's schema.
- [ ] `@todo` scenarios are excluded from the run and listed as `todo` in `public/scenarios/`.
