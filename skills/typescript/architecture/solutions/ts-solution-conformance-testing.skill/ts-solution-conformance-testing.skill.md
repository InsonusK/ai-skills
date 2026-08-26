---
name: ts-solution-conformance-testing
description: Sets up the TypeScript side of the Cucumber/coverage/mutation quality gate — @cucumber/cucumber for Gherkin scenarios, Vitest coverage for coverage, Stryker for mutation testing, and the make cucumber-test/mutation-test/result-page contract that devops-github-wf-bdd-report-publish's workflows consume
whenToUse: Set up or review the test suite of a framework-agnostic TypeScript package that must prove conformance to a Cucumber/Gherkin spec, add Gherkin scenarios and step definitions to an existing TypeScript package, or wire coverage and mutation testing into a TypeScript package's `make`/CI pipeline.
domain: skill
type: architecture
version: 1
tags:
  - skill/architecture/solution
  - typescript
  - concern/testing
  - concern/testing/bdd
  - cucumber
  - concern/testing/mutation
  - stack/typescript
  - concern/architecture

creates:
  - "{Package}/features/{rule}.feature"
  - "{Package}/features/step-definitions/{rule}.steps.ts"
  - Makefile
extends:
  - "{Package}/package.json"
  - README.md
depends_on:
  - "[[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
  - "[[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]]"
adr:
  - "[[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/adr/testing-tool-choice|Testing tool choice]]"
---

# Goal
- Give a framework-agnostic TypeScript package the concrete tooling to run the three-layer gate defined by [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md): Gherkin scenarios, code coverage, mutation testing.
- Expose that tooling behind the `make cucumber-test`/`make mutation-test`/`make result-page` contract so [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md) can wire CI without knowing anything TypeScript-specific.
- This solution targets plain, framework-agnostic TypeScript packages (a validation library consumed by any frontend). A UI framework's own component/e2e testing (e.g. Angular's Vitest/Playwright setup) is a separate concern — see that framework's own testing solution instead.

# Capabilities
- Gherkin `.feature` files execute against the package's real exported functions/classes via `@cucumber/cucumber` step definitions.
- `make mutation-test ONLY_DELTA=true DELTA_BASE=<ref>` fails fast on a changed line's surviving mutant, without paying for a full-package mutation run on every call.
- `make cucumber-test WITH_CODE_COVERAGE=true` and `make result-page` give `master` an up-to-date coverage/mutation-score report and the data the README badges are generated from.

# Core Principles
- Step definitions import from the package's `src/index.ts` public API, never from an internal module path directly.
- Step definitions call the package's real exported function/class; they never re-implement the rule under test.
- Coverage and mutation testing both run against the combined suite (Vitest unit tests plus Cucumber scenarios), not against either alone.

# Adr
- [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/adr/testing-tool-choice|Testing tool choice]]
  - Selected variant: `@cucumber/cucumber` (Gherkin runner) + Vitest coverage (coverage) + Stryker (mutation testing)

# Requirements
SOLUTION:
- [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]
  - Defines the `make` command contract and normalized report format this solution implements concretely for TypeScript.
- [[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]]
  - Owns the actual CI workflows (PR-gate and master-push) that call this solution's `Makefile`; this solution does not define any `.github/workflows/*.yml` file itself.

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
- [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/Implementation/Repository.extend|Repository]] - extend - add the `Makefile` and normalization scripts implementing the `make cucumber-test`/`mutation-test`/`result-page` contract

PACKAGE:
- [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/Implementation/{Package}.package.extend|{Package}]] - extend - add Cucumber/Vitest/Stryker scripts and config
  - [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/Implementation/{Package}.package.extend/{rule}.steps.ts.create|{rule}.steps.ts]] - create - step definitions binding a `.feature` file to the package's real API

# Workflow
## Add conformance coverage for a new validation rule (happy path)
1. A `.feature` file describing the rule (e.g. `features/{rule}.feature`) is added or extended with `Given/When/Then` scenarios.
2. `features/step-definitions/{rule}.steps.ts` is created with `Given`/`When`/`Then` bindings that import from `src/index.ts` and call the real exported function/class.
3. `make cucumber-test` runs `vitest run --coverage` and `cucumber-js` (both feeding the same coverage provider's output when `WITH_CODE_COVERAGE=true`), and normalizes the result into `tmp/result/cucumber-test.json` (plus `tmp/result/coverage-test.json`).
4. `make mutation-test` runs `stryker run` — scoped to changed files via Stryker's incremental/since mode when called with `ONLY_DELTA=true DELTA_BASE=<ref>`, or across the whole package otherwise — and normalizes the result into `tmp/result/mutation-test.json`.
5. `make result-page` assembles `public/` from `tmp/result/*.json` and `tmp/report/*`, ready to publish.
6. Which of these `make` targets run on which trigger, and how `public/` gets published to GitHub Pages, is owned by [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md) — not by this solution.

## Surviving mutant found (failure path)
1. `make mutation-test` reports a mutant that survived in changed code.
2. The CI job calling it (per [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md)) fails.
3. Reviewer either strengthens the assertion in the corresponding scenario/step definition, or the PR description explicitly justifies the survivor per [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#must).

# Rules
Each linked `#MUST` section below carries its own `Violation`/`Risk`/`Fix` at the target — this index only points to where the actual rule lives.

## MUST
- [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/Implementation/Repository.extend#MUST|Repository]]
- [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/Implementation/{Package}.package.extend#MUST|{Package}]]
  - [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/Implementation/{Package}.package.extend/{rule}.steps.ts.create#MUST|{rule}.steps.ts]]

# Check list
- [ ] `package.json` declares `test`, `coverage`, and `mutation` scripts backed by Vitest, Vitest coverage, and Stryker.
- [ ] Every `.feature` scenario has a matching step definition that imports from `src/index.ts` and calls production code.
- [ ] `make cucumber-test`, `make mutation-test`, and `make result-page` exist at the repository root and support the toggles defined by [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract).
- [ ] `tmp/result/*.json` and `tmp/report/<kind>/` follow that same contract's schema.
