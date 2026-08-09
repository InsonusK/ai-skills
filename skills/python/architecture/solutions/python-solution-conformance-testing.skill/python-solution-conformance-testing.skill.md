---
name: python-solution-conformance-testing
description: Sets up the Python side of the Cucumber/coverage/mutation quality gate — behave for Gherkin scenarios, coverage.py for coverage, mutmut for mutation testing, and the PR-gate/trunk-gate CI split with README badges
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
triggers:
  - Set up or review the test suite of a Python package that must prove conformance to a Cucumber/Gherkin spec
  - Add Gherkin scenarios and step definitions to an existing Python project
  - Wire coverage and mutation testing into a Python project's CI pipeline
creates:
  - features/{rule}.feature
  - features/steps/{rule}_steps.py
extends:
  - pyproject.toml
  - .github/workflows/ci.yml
  - README.md
depends_on:
  - "[[skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md|bdd-coverage-mutation-testing]]"
  - "[[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]]"
adr:
  - "[[skills/python/architecture/solutions/python-solution-conformance-testing.skill/adr/testing-tool-choice|Testing tool choice]]"
---

# Goal
- Give a Python package the concrete tooling to run the three-layer gate defined by [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md): Gherkin scenarios, code coverage, mutation testing.
- Add this on top of the plain unit-test structure defined by [solution-test](skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md), without changing that structure.

# Capabilities
- Gherkin `.feature` files execute against the package's real public functions/classes via `behave` step definitions.
- CI fails a PR when a changed line's mutant survives, without paying for a full-repository mutation run on every PR.
- `master` always has an up-to-date coverage and mutation-score report, and the README badges reflect it.

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
  - Defines the PR-gate/trunk-gate split and the badge requirement this solution implements concretely for Python.
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]]
  - Defines the `test/` structure for this package's plain unit tests; this solution adds `features/` alongside it, unchanged.

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
- [[skills/python/architecture/solutions/python-solution-conformance-testing.skill/Implementation/Repository.extend|Repository]] - extend - add the PR-gate/trunk-gate CI jobs and README badges

# Workflow
## Add conformance coverage for a new validation rule (happy path)
1. A `.feature` file describing the rule (e.g. `features/{rule}.feature`) is added or extended with `Given/When/Then` scenarios.
2. `features/steps/{rule}_steps.py` is created with `@given`/`@when`/`@then` bindings that call the package's real function/class.
3. `coverage run -m behave` and `coverage run -a -m pytest` (or `unittest`) both feed the same `.coverage` data file.
4. `mutmut run` executes against the changed files (PR gate) or the whole package (trunk gate) and reports the mutation score.

## Surviving mutant found (failure path)
1. `mutmut run` reports a mutant that survived in changed code.
2. The PR gate fails the check.
3. Reviewer either strengthens the assertion in the corresponding scenario/step definition, or the PR description explicitly justifies the survivor per [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#must).

# Rules

## MUST
- [[skills/python/architecture/solutions/python-solution-conformance-testing.skill/Implementation/pyproject.toml.extend#MUST|pyproject.toml]]
- [[skills/python/architecture/solutions/python-solution-conformance-testing.skill/Implementation/features.steps.{rule}_steps.py.create#MUST|features/steps/{rule}_steps.py]]

## MUST NOT
- [[skills/python/architecture/solutions/python-solution-conformance-testing.skill/Implementation/features.steps.{rule}_steps.py.create#MUST NOT|features/steps/{rule}_steps.py]]

# Anti-patterns
- [[skills/python/architecture/solutions/python-solution-conformance-testing.skill/Implementation/features.steps.{rule}_steps.py.create|See features/steps/{rule}_steps.py.create.md]] — a step definition re-implementing the rule instead of calling the package's real function/class.
- [[skills/python/architecture/solutions/python-solution-conformance-testing.skill/Implementation/Repository.extend|See Repository.extend.md]] — running full-repository `mutmut` on every PR instead of scoping it to the diff.

# Check list
- [ ] `pyproject.toml` lists `behave`, `coverage`, `mutmut` as dev dependencies.
- [ ] Every `.feature` scenario has a matching step definition that calls the package's real API.
- [ ] `coverage` combines results from both `test/` and `features/` runs before reporting.
- [ ] PR-gate CI job runs `mutmut` scoped to the diff; trunk-gate CI job runs it for the whole package.
- [ ] README shows a coverage badge and a mutation-score badge sourced from the latest trunk-gate run.
