---
name: ts-solution-conformance-testing
description: Sets up the TypeScript side of the Cucumber/coverage/mutation quality gate — @cucumber/cucumber for Gherkin scenarios, Vitest coverage for coverage, Stryker for mutation testing, and the PR-gate/trunk-gate CI split with README badges
domain: skill
type: architecture
version: 1
tags:
  - skill/architecture/solution
  - typescript
  - testing
  - bdd
  - cucumber
  - mutation-testing
triggers:
  - Set up or review the test suite of a framework-agnostic TypeScript package that must prove conformance to a Cucumber/Gherkin spec
  - Add Gherkin scenarios and step definitions to an existing TypeScript package
  - Wire coverage and mutation testing into a TypeScript package's CI pipeline
creates:
  - "{Package}/features/{rule}.feature"
  - "{Package}/features/step-definitions/{rule}.steps.ts"
extends:
  - "{Package}/package.json"
  - .github/workflows/ci.yml
  - README.md
depends_on:
  - "[[skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md|bdd-coverage-mutation-testing]]"
adr:
  - "[[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/adr/testing-tool-choice|Testing tool choice]]"
---

# Goal
- Give a framework-agnostic TypeScript package the concrete tooling to run the three-layer gate defined by [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md): Gherkin scenarios, code coverage, mutation testing.
- Make the PR-gate/trunk-gate CI split (delta-scoped mutation testing on PR, full mutation testing on merge to `master`) concrete for a TypeScript/npm/GitHub Actions setup.
- This solution targets plain, framework-agnostic TypeScript packages (a validation library consumed by any frontend). A UI framework's own component/e2e testing (e.g. Angular's Vitest/Playwright setup) is a separate concern — see that framework's own testing solution instead.

# Capabilities
- Gherkin `.feature` files execute against the package's real exported functions/classes via `@cucumber/cucumber` step definitions.
- CI fails a PR when a changed line's mutant survives, without paying for a full-package mutation run on every PR.
- `master` always has an up-to-date coverage and mutation-score report, and the README badges reflect it.

# Core Principles
- Step definitions import from the package's `src/index.ts` public API, never from an internal module path directly.
- Step definitions call the package's real exported function/class; they never re-implement the rule under test.
- Coverage and mutation testing both run against the combined suite (Vitest unit tests plus Cucumber scenarios), not against either alone.

# Adr
- [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/adr/testing-tool-choice|Testing tool choice]]
  - Selected variant: `@cucumber/cucumber` (Gherkin runner) + Vitest coverage (coverage) + Stryker (mutation testing)

# Requirements
SOLUTION:
- [[skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md|bdd-coverage-mutation-testing]]
  - Defines the PR-gate/trunk-gate split and the badge requirement this solution implements concretely for TypeScript.

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
- [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/Implementation/Repository.extend|Repository]] - extend - add the PR-gate/trunk-gate CI jobs and README badges

PACKAGE:
- [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/Implementation/{Package}.package.extend|{Package}]] - extend - add Cucumber/Vitest/Stryker scripts and config
  - [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/Implementation/{Package}.package.extend/{rule}.steps.ts.create|{rule}.steps.ts]] - create - step definitions binding a `.feature` file to the package's real API

# Workflow
## Add conformance coverage for a new validation rule (happy path)
1. A `.feature` file describing the rule (e.g. `features/{rule}.feature`) is added or extended with `Given/When/Then` scenarios.
2. `features/step-definitions/{rule}.steps.ts` is created with `Given`/`When`/`Then` bindings that import from `src/index.ts` and call the real exported function/class.
3. `vitest run --coverage` runs the unit tests; `cucumber-js` runs the scenarios; both feed the same coverage provider's output.
4. `stryker run` executes against the changed files (PR gate, via Stryker's incremental/since mode) or the whole package (trunk gate) and reports the mutation score.

## Surviving mutant found (failure path)
1. `stryker run` reports a mutant that survived in changed code.
2. The PR gate fails the check.
3. Reviewer either strengthens the assertion in the corresponding scenario/step definition, or the PR description explicitly justifies the survivor per [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#must).

# Rules

## MUST
- [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/Implementation/Repository.extend#MUST|Repository]]
- [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/Implementation/{Package}.package.extend#MUST|{Package}]]
  - [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/Implementation/{Package}.package.extend/{rule}.steps.ts.create#MUST|{rule}.steps.ts]]

## MUST NOT
- [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/Implementation/{Package}.package.extend/{rule}.steps.ts.create#MUST NOT|{rule}.steps.ts]]

# Anti-patterns
- [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/Implementation/{Package}.package.extend/{rule}.steps.ts.create|See {rule}.steps.ts.create.md]] — a step definition re-implementing the rule instead of calling the package's real exported function.
- [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/Implementation/Repository.extend|See Repository.extend.md]] — running full-package `stryker run` on every PR instead of scoping it to the diff.

# Check list
- [ ] `package.json` declares `test`, `coverage`, and `mutation` scripts backed by Vitest, Vitest coverage, and Stryker.
- [ ] Every `.feature` scenario has a matching step definition that imports from `src/index.ts` and calls production code.
- [ ] PR-gate CI job runs Stryker scoped to the diff; trunk-gate CI job runs it for the whole package.
- [ ] README shows a coverage badge and a mutation-score badge sourced from the latest trunk-gate run.
