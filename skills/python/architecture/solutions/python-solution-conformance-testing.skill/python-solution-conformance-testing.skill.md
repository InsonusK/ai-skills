---
name: python-solution-conformance-testing
description: Sets up the Python side of the Cucumber/coverage/mutation quality gate — behave for Gherkin scenarios, coverage.py for coverage, mutmut for mutation testing, and the make cucumber-test/mutation-test/result-page contract that devops-github-wf-bdd-report-publish's workflows consume
whenToUse: Set up or review the test suite of a Python package that must prove conformance to a Cucumber/Gherkin spec, add Gherkin scenarios and step definitions to an existing Python project, or wire coverage and mutation testing into a Python project's `make`/CI pipeline.
domain: python
type: architecture
version: 1
tags:
  - skill/architecture/solution
  - python
  - testing
  - bdd
  - cucumber
  - mutation-testing
creates:
  - features/{rule}.feature
  - features/steps/{rule}_steps.py
  - Makefile
extends:
  - pyproject.toml
  - README.md
depends_on:
  - "[[skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md|bdd-coverage-mutation-testing]]"
  - "[[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]]"
  - "[[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]]"
adr:
  - "[[skills/python/architecture/solutions/python-solution-conformance-testing.skill/adr/testing-tool-choice|Testing tool choice]]"
---

# Goal
- Give a Python package the concrete tooling to run the three-layer gate defined by [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md): Gherkin scenarios, code coverage, mutation testing.
- Add this on top of the plain unit-test structure defined by [solution-test](skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md), without changing that structure.
- Expose that tooling behind the `make cucumber-test`/`make mutation-test`/`make result-page` contract so [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md) can wire CI without knowing anything Python-specific.

# Capabilities
- Gherkin `.feature` files execute against the package's real public functions/classes via `behave` step definitions.
- `make mutation-test ONLY_DELTA=true DELTA_BASE=<ref>` fails fast on a changed line's surviving mutant, without paying for a full-package mutation run on every call.
- `make cucumber-test WITH_CODE_COVERAGE=true` and `make result-page` give `master` an up-to-date coverage/mutation-score report and the data the README badges are generated from.

# Core Principles
- `behave`'s own convention (`features/` at the repository root, with `features/steps/` for step definitions) is used as-is; it is a separate root from `test/`, not folded into the mirrored structure [solution-test](skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md) defines for plain unit tests.
- Step definitions import and call the package's real public functions/classes; they never re-implement the rule under test.
- Coverage and mutation testing both run against the combined suite (`unittest`/`pytest` tests plus `behave` scenarios), not against either alone.

# Adr
- [[skills/python/architecture/solutions/python-solution-conformance-testing.skill/adr/testing-tool-choice|Testing tool choice]]
  - Selected variant: `behave` (Gherkin runner) + `coverage.py` (coverage) + `mutmut` (mutation testing)

# Requirements
SOLUTION:
- [[skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md|bdd-coverage-mutation-testing]]
  - Defines the `make` command contract and normalized report format this solution implements concretely for Python.
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]]
  - Defines the `test/` structure for this package's plain unit tests; this solution adds `features/` alongside it, unchanged.
- [[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]]
  - Owns the actual CI workflows (PR-gate and master-push) that call this solution's `Makefile`; this solution does not define any `.github/workflows/*.yml` file itself.

PYPI:
- behave
  - Runs `.feature` files against `features/steps/*.py` step definitions.
- coverage
  - Measures line/branch coverage across both the `test/` suite and `features/` scenarios.
- mutmut
  - Runs mutation testing against the package and reports a mutation score.

# Template Skill Mutations
PROJECT:
- [[skills/python/architecture/solutions/python-solution-conformance-testing.skill/Implementation/pyproject.toml.extend|pyproject.toml]] - extend - declare `behave`, `coverage`, `mutmut` as dev dependencies and configure `coverage` to include `features/`
- [[skills/python/architecture/solutions/python-solution-conformance-testing.skill/Implementation/features.{rule}.feature.create|features/{rule}.feature]] - create - Gherkin scenarios for one business rule
- [[skills/python/architecture/solutions/python-solution-conformance-testing.skill/Implementation/features.steps.{rule}_steps.py.create|features/steps/{rule}_steps.py]] - create - step definitions calling the package's real API

REPOSITORY:
- [[skills/python/architecture/solutions/python-solution-conformance-testing.skill/Implementation/Repository.extend|Repository]] - extend - add the `Makefile` and normalization scripts implementing the `make cucumber-test`/`mutation-test`/`result-page` contract

# Workflow
## Add conformance coverage for a new validation rule (happy path)
1. A `.feature` file describing the rule (e.g. `features/{rule}.feature`) is added or extended with `Given/When/Then` scenarios.
2. `features/steps/{rule}_steps.py` is created with `@given`/`@when`/`@then` bindings that call the package's real function/class.
3. `make cucumber-test` runs `coverage run -m behave` and `coverage run -a -m pytest` (or `unittest`) into the same `.coverage` data file, and normalizes the result into `tmp/result/cucumber-test.json` (plus `tmp/result/coverage-test.json` when `WITH_CODE_COVERAGE=true`).
4. `make mutation-test` runs `mutmut run` — scoped to changed files when called with `ONLY_DELTA=true DELTA_BASE=<ref>`, or across the whole package otherwise — and normalizes the result into `tmp/result/mutation-test.json`.
5. `make result-page` assembles `public/` from `tmp/result/*.json` and `tmp/report/*`, ready to publish.
6. Which of these `make` targets run on which trigger, and how `public/` gets published to GitHub Pages, is owned by [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md) — not by this solution.

## Surviving mutant found (failure path)
1. `make mutation-test` reports a mutant that survived in changed code.
2. The CI job calling it (per [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md)) fails.
3. Reviewer either strengthens the assertion in the corresponding scenario/step definition, or the PR description explicitly justifies the survivor per [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#must).

# Rules
Each linked `#MUST` section below carries its own `Violation`/`Risk`/`Fix` at the target — this index only points to where the actual rule lives.

## MUST
- [[skills/python/architecture/solutions/python-solution-conformance-testing.skill/Implementation/pyproject.toml.extend#MUST|pyproject.toml]]
- [[skills/python/architecture/solutions/python-solution-conformance-testing.skill/Implementation/features.steps.{rule}_steps.py.create#MUST|features/steps/{rule}_steps.py]]

# Check list
- [ ] `pyproject.toml` lists `behave`, `coverage`, `mutmut` as dev dependencies.
- [ ] Every `.feature` scenario has a matching step definition that calls the package's real API.
- [ ] `coverage` combines results from both `test/` and `features/` runs before reporting.
- [ ] `make cucumber-test`, `make mutation-test`, and `make result-page` exist at the repository root and support the toggles defined by [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#make-command-contract).
- [ ] `tmp/result/*.json` and `tmp/report/<kind>/` follow that same contract's schema.
