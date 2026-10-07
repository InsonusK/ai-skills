---
name: solution-conformance-testing-in-python
description: Sets up the Python side of the Cucumber/coverage/mutation quality gate — pytest-bdd for Gherkin scenarios inside the one pytest run, coverage.py for coverage, mutmut for mutation testing, and the make test-kind-unit/test-kind-mutation/test-report/test-and-report contract that downstream CI consumes
whenToUse: Set up or review the test suite of a Python package that must prove conformance to a Cucumber/Gherkin spec, add Gherkin scenarios and step definitions to an existing Python project, or wire coverage and mutation testing into a Python project's `make`/CI pipeline.
domain: python
type: architecture
version: 20261007180000
tags:
  - solution/conformance-testing-in-python
  - skill/architecture/solution
  - stack/python
  - concern/testing
  - concern/testing/bdd
  - cucumber
  - concern/testing/mutation

creates:
  - features/{rule}.feature
  - features/steps/{rule}_steps.py

  - tools/testing/normalize-scenarios.sh
extends:
  - pyproject.toml
  - README.md
depends_on:
  - "[[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
  - "[[skills/testing/python/solution-test-layout-in-python.skill/solution-test-layout-in-python.skill.md|solution-test-layout-in-python]]"
adr:
  - "[[skills/testing/python/solution-conformance-testing-in-python.skill/adr/testing-tool-choice|Testing tool choice]]"
---

# Goal
- Give a Python package the concrete tooling to run the three-layer gate defined by [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md): Gherkin scenarios, code coverage, mutation testing.
- Add this on top of the plain unit-test structure defined by [solution-test-layout-in-python](skills/testing/python/solution-test-layout-in-python.skill/solution-test-layout-in-python.skill.md), without changing that structure.
- Expose that tooling behind the `make test-kind-unit`/`make test-kind-mutation`/`make test-report`/`make test-and-report` contract so any CI workflow can wire it in without knowing anything Python-specific.

# Capabilities
- Gherkin `.feature` files execute against the package's real public functions/classes via `pytest-bdd` step definitions, in the same `pytest` run as the plain `test/` suite — one exit code, one coverage result.
- `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=<ref>` mutates only the source files changed since `<ref>`, without paying for a full-package mutation run on every call.
- `make test-kind-unit TEST_RUN_PURPOSE=report` and `make test-report` give `master` an up-to-date coverage/mutation-score report and the data the README badges are generated from.
- `make test-kind-unit` also writes `$TEST_KIND_DIR/result/scenarios.json` — every `.feature` entry with its type tag, status, and `@todo` reason — and `make test-report` renders it as `$TEST_REPORT_DIR/reports/scenarios/`, per [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-report).

# Core Principles
- Scenarios live in `features/` at the repository root, step modules in `features/steps/` — a separate root from `test/`, not folded into the mirrored structure [solution-test-layout-in-python](skills/testing/python/solution-test-layout-in-python.skill/solution-test-layout-in-python.skill.md) defines for plain unit tests. `pytest` collects both roots.
- Step definitions import and call the package's real public functions/classes; they never re-implement the rule under test.
- Coverage and mutation testing both run against the combined suite (plain `pytest` tests plus `pytest-bdd` scenarios), not against either alone.

# Adr
- [[skills/testing/python/solution-conformance-testing-in-python.skill/adr/testing-tool-choice|Testing tool choice]]
  - Selected variant: `pytest-bdd` (Gherkin scenarios inside `pytest`) + `coverage.py` (coverage) + `mutmut` (mutation testing)

# Requirements
SOLUTION:
- [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]
  - Defines the `make` command contract and normalized report format this solution implements concretely for Python.
- [[skills/testing/python/solution-test-layout-in-python.skill/solution-test-layout-in-python.skill.md|solution-test-layout-in-python]]
  - Defines the `test/` structure for this package's plain unit tests; this solution adds `features/` alongside it, unchanged.

PYPI:
- pytest
  - The one runner: collects the plain `test/` suite and the scenarios, writes the JUnit report the counts come from.
- pytest-bdd
  - Turns every scenario of a `.feature` file into a `pytest` test bound to `features/steps/*_steps.py`; writes classic Cucumber JSON (`--cucumberjson`) — the standard report the living-doc renderer reads.
- coverage
  - Measures line/branch coverage of that one run.
- mutmut
  - Runs mutation testing against the package and exports the counts the mutation score is computed from.

# Template Skill Mutations
PROJECT:
- [[skills/testing/python/solution-conformance-testing-in-python.skill/Implementation/pyproject.toml.extend|pyproject.toml]] - extend - declare `pytest`, `pytest-bdd`, `coverage`, `mutmut` as dev dependencies and configure `pytest` collection, the tag markers, `coverage` and `mutmut`
- [[skills/testing/python/solution-conformance-testing-in-python.skill/Implementation/features.{rule}.feature.create|features/{rule}.feature]] - create - Gherkin scenarios for one business rule
- [[skills/testing/python/solution-conformance-testing-in-python.skill/Implementation/features.steps.{rule}_steps.py.create|features/steps/{rule}_steps.py]] - create - step definitions calling the package's real API

REPOSITORY:
- [[skills/testing/python/solution-conformance-testing-in-python.skill/Implementation/Repository.extend|Repository]] - extend - add the kind scripts, the `pytest` plugin the unit kind loads, and the `Makefile` include implementing the `make test-kind-unit`/`test-kind-mutation`/`test-report`/`test-and-report` contract

# Workflow
## Add conformance coverage for a new validation rule (happy path)
1. A `.feature` file describing the rule (e.g. `features/{rule}.feature`) is added or extended with `Given/When/Then` scenarios.
2. `features/steps/{rule}_steps.py` is created with `scenarios("../{rule}.feature")` and `@given`/`@when`/`@then` bindings that call the package's real function/class.
3. `make test-kind-unit` runs one `coverage run -m pytest` over `test/` and `features/`, and normalizes the result into `$TEST_KIND_DIR/result/unit-test.json` and `$TEST_KIND_DIR/result/scenarios.json` (plus `$TEST_KIND_DIR/result/coverage-test.json` in a `report` run).
4. `make test-kind-mutation` runs `mutmut run` — across the whole package in a `report` run; in a `check` run scoped to the source files changed since `DELTA_BASE`, and skipped without one or when none changed — and normalizes the result into `$TEST_KIND_DIR/result/mutation-test.json`.
5. `make test-report` assembles `$TEST_REPORT_DIR/` — `scenarios/` included — from `$TEST_KIND_DIR/result/*.json` and `$TEST_KIND_DIR/report/*`, ready to publish. `make test-and-report` runs all three targets in sequence.
6. Which of these `make` targets run on which trigger, and how `$TEST_REPORT_DIR/` gets published, is decided by the project's own CI configuration — not by this solution.

## Surviving mutant found (report path)
1. `make test-kind-mutation` reports a mutant that survived, as part of a post-merge, report-only CI run — mutation testing is not expected to block a pull request.
2. The mutation report published to GitHub Pages shows the survivor; it does not fail the workflow or block anything.
3. Whoever notices the survivor (via the report or the README's mutation-score badge) either strengthens the assertion in the corresponding scenario/step definition in a follow-up PR, or explicitly accepts it per [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#must).

# Ground truth
[`example/`](./example/) is a minimal package (`src/` layout, one feature with a `Scenario Outline` of two `Examples:` blocks and a `@todo` scenario, one plain test) carrying this solution as it is delivered. Verified on 2026-10-07 with Python 3.13, `pytest` 9.1, `pytest-bdd` 9.0 (and 8.1), `coverage` 7.16, `mutmut` 3.8:
- `make test-and-report` — exit `0`; 5/5 tests, coverage 100%, mutation score 93.5% (29 killed, 2 survived); the scenario report lists both `Examples:` blocks and the `@todo` entry with its reason.
- `make test-and-report TEST_RUN_PURPOSE=check` — mutation skipped, no coverage report, only the `tests` badge.
- A broken `Examples:` row — `make test-kind-unit` exits non-zero, that block is `failed` and its sibling `passed`.
- `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=HEAD~1` in a copy with its own git history — only the changed module's mutants run.

# Rules
Each linked `#MUST` section below carries its own `Violation`/`Risk`/`Fix` at the target — this index only points to where the actual rule lives.

## MUST
- [[skills/testing/python/solution-conformance-testing-in-python.skill/Implementation/pyproject.toml.extend#MUST|pyproject.toml]]
- [[skills/testing/python/solution-conformance-testing-in-python.skill/Implementation/features.steps.{rule}_steps.py.create#MUST|features/steps/{rule}_steps.py]]
- [[skills/testing/python/solution-conformance-testing-in-python.skill/Implementation/Repository.extend#MUST|Repository]]

# Check list
- [ ] `pyproject.toml` lists `pytest`, `pytest-bdd`, `coverage`, `mutmut` as dev dependencies with version ranges.
- [ ] Every `.feature` scenario has a matching step definition that calls the package's real API.
- [ ] One `pytest` run executes `test/` and `features/` together; `coverage` reports that run.
- [ ] `make test-kind-unit`, `make test-kind-mutation`, `make test-report`, and `make test-and-report` exist at the repository root and support the toggles defined by [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract).
- [ ] `$TEST_KIND_DIR/result/*.json` — `scenarios.json` included, written on a red run too — and `$TEST_KIND_DIR/report/<kind>/` follow that same contract's schema.
- [ ] `@todo` scenarios are excluded from the run and listed as `todo` in `$TEST_REPORT_DIR/reports/scenarios/`.
