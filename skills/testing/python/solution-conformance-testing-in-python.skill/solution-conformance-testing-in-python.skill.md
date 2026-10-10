---
name: solution-conformance-testing-in-python
description: Sets up the Python side of the Cucumber/coverage/mutation quality gate — pytest-bdd for Gherkin scenarios inside the one pytest run, coverage.py for coverage, mutmut for mutation testing, and the make test-kind-unit/test-kind-mutation/test-report/test-and-report contract that downstream CI consumes
whenToUse: Set up or review the test suite of a Python package that must prove conformance to a Cucumber/Gherkin spec, add Gherkin scenarios and step definitions to an existing Python project, or wire coverage and mutation testing into a Python project's `make`/CI pipeline.
domain: python
type: architecture
version: 20261009200000
tags:
  - solution/conformance-testing-in-python
  - skill/architecture/solution
  - stack/python
  - concern/testing
  - concern/testing/bdd
  - cucumber
  - concern/testing/mutation

creates:
  - "{Package}/features/{rule}.feature"
  - "{Package}/test/{rule}_steps_test.py"

  - tools/testing/normalize-scenarios.sh
extends:
  - pyproject.toml
  - README.md
depends_on:
  - "[[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
adr:
  - "[[skills/testing/python/solution-conformance-testing-in-python.skill/adr/testing-tool-choice|Testing tool choice]]"
---

# Goal
- Give a Python package the concrete tooling to run the three-layer gate defined by [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md): Gherkin scenarios, code coverage, mutation testing.
- Place features and tests beside the code they specify, per [cucumber-testing-in-python](skills/testing/python/cucumber-testing-in-python.skill/cucumber-testing-in-python.skill.md), and keep them out of the installed package.
- Expose that tooling behind the `make test-kind-unit`/`make test-kind-mutation`/`make test-report`/`make test-and-report` contract so any CI workflow can wire it in without knowing anything Python-specific.

# Capabilities
- Gherkin `.feature` files execute against the package's real public functions/classes via `pytest-bdd` step definitions, in the same `pytest` run as the plain tests — one exit code, one coverage result.
- `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=<ref>` mutates only the source files changed since `<ref>`, without paying for a full-package mutation run on every call.
- `make test-kind-unit TEST_RUN_PURPOSE=report` and `make test-report` give `master` an up-to-date coverage/mutation-score report and the data the README badges are generated from.
- `make test-kind-unit` also writes `$TEST_KIND_DIR/result/scenarios.json` — every `.feature` entry with its type and category, its status, and `@status/todo` reason. The unit kind fails on an entry without its type or category tag, and the living doc takes from it the scenarios no runner executed. There is no separate scenarios page: the living doc is the one place a reader sees every scenario with its tags.

# Core Principles
- Features live in `{package}/features/`, step modules and the rare plain test in `{package}/test/`, per [cucumber-testing-in-python](skills/testing/python/cucumber-testing-in-python.skill/cucumber-testing-in-python.skill.md); `pytest` collects them from the source root.
- Step definitions import and call the package's real public functions/classes; they never re-implement the rule under test.
- Coverage and mutation testing both run against the combined suite (plain `pytest` tests plus `pytest-bdd` scenarios), not against either alone.

# Adr
- [[skills/testing/python/solution-conformance-testing-in-python.skill/adr/testing-tool-choice|Testing tool choice]]
  - Selected variant: `pytest-bdd` (Gherkin scenarios inside `pytest`) + `coverage.py` (coverage) + `mutmut` (mutation testing)

# Requirements
SOLUTION:
- [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]
  - Defines the `make` command contract and normalized report format this solution implements concretely for Python.

PYPI:
- pytest
  - The one runner: collects the scenarios and the plain tests of every `test/` folder, writes the JUnit report the counts come from.
- pytest-bdd
  - Turns every scenario of a `.feature` file into a `pytest` test bound to its `test/*_steps_test.py` module; writes classic Cucumber JSON (`--cucumberjson`) — the standard report the living-doc renderer reads.
- coverage
  - Measures line/branch coverage of that one run.
- mutmut
  - Runs mutation testing against the package and exports the counts the mutation score is computed from.

# Template Skill Mutations
PROJECT:
- [[skills/testing/python/solution-conformance-testing-in-python.skill/Implementation/pyproject.toml.extend|pyproject.toml]] - extend - declare `pytest`, `pytest-bdd`, `coverage`, `mutmut` as dev dependencies and configure `pytest` collection, the tag markers, `coverage` and `mutmut`
- [[skills/testing/python/solution-conformance-testing-in-python.skill/Implementation/{Package}.features.{rule}.feature.create|{Package}/features/{rule}.feature]] - create - Gherkin scenarios for one business rule
- [[skills/testing/python/solution-conformance-testing-in-python.skill/Implementation/{Package}.test.{rule}_steps_test.py.create|{Package}/test/{rule}_steps_test.py]] - create - step definitions calling the package's real API

REPOSITORY:
- [[skills/testing/python/solution-conformance-testing-in-python.skill/Implementation/Repository.extend|Repository]] - extend - add the kind scripts, the `pytest` plugin the unit kind loads, and the `Makefile` include implementing the `make test-kind-unit`/`test-kind-mutation`/`test-report`/`test-and-report` contract

# Workflow
## Add conformance coverage for a new validation rule (happy path)
1. A `.feature` file describing the rule (e.g. `src/{package}/features/{rule}.feature`) is added or extended with `Given/When/Then` scenarios.
2. `src/{package}/test/{rule}_steps_test.py` is created with `scenarios("../features/{rule}.feature")` and `@given`/`@when`/`@then` bindings that call the package's real function/class.
3. `make test-kind-unit` runs one `coverage run -m pytest` over the source root, and normalizes the result into `$TEST_KIND_DIR/result/unit-test.json` and `$TEST_KIND_DIR/result/scenarios.json` (plus `$TEST_KIND_DIR/result/coverage-test.json` in a `report` run).
4. `make test-kind-mutation` runs `mutmut run` — across the whole package in a `report` run; in a `check` run scoped to the source files changed since `DELTA_BASE`, and skipped without one or when none changed — and normalizes the result into `$TEST_KIND_DIR/result/mutation-test.json`.
5. `make test-report` gathers every kind's `report/` and `badges/` into `$TEST_REPORT_DIR/`, ready to publish. `make test-and-report` runs all three targets in sequence.
6. Which of these `make` targets run on which trigger, and how `$TEST_REPORT_DIR/` gets published, is decided by the project's own CI configuration — not by this solution.

## Surviving mutant found (report path)
1. `make test-kind-mutation` reports a mutant that survived, as part of a post-merge, report-only CI run — mutation testing is not expected to block a pull request.
2. The mutation report published to GitHub Pages shows the survivor; it does not fail the workflow or block anything.
3. Whoever notices the survivor (via the report or the README's mutation-score badge) either strengthens the assertion in the corresponding scenario/step definition in a follow-up PR, or explicitly accepts it per [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#must).

# Ground truth
[`examples/`](./examples/) is a small package carrying this solution as it is delivered (`src/` layout), and the place to see every tag at work: eight features, one for each `@type/…` (`domain` twice), with scenarios of every `@category/…`, two `@status/todo`, one `@status/broken` and one `@status/validated` — put there to show the tag; in a project only a person sets it. `src/linkcheck/` holds seven of the features in its `features/`, their step modules and a shared `conftest.py` in its `test/`; the sub-package `src/linkcheck/batch/` has its own `features/` and `test/`.
- `make init`, then `make test-and-report`, with no Python package installed outside `.venv/` — exit `0`; 26/26 tests, coverage 99.2%, mutation score 87.4%; `result/scenarios.json` lists both `Examples:` blocks and the `@status/todo` entry with its reason; the living doc shows every scenario — the `todo` and `broken` ones with their reason — with all its tags, and the status legend below.
- `pip wheel .` — the wheel holds the package's modules only: no `test/` path, no `.feature` file.
- `make test-and-report TEST_RUN_PURPOSE=check` — mutation skipped, no coverage report, only the `tests` badge.
- A broken `Examples:` row — `make test-kind-unit` exits non-zero, that block is `failed` and its sibling `passed`.
- `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=HEAD~1` in a copy with its own git history — only the changed module's mutants run.

# Rules
Each linked `#MUST` section below carries its own `Violation`/`Risk`/`Fix` at the target — this index only points to where the actual rule lives.

## MUST
- [[skills/testing/python/solution-conformance-testing-in-python.skill/Implementation/pyproject.toml.extend#MUST|pyproject.toml]]
- [[skills/testing/python/solution-conformance-testing-in-python.skill/Implementation/{Package}.test.{rule}_steps_test.py.create#MUST|{Package}/test/{rule}_steps_test.py]]
- [[skills/testing/python/solution-conformance-testing-in-python.skill/Implementation/Repository.extend#MUST|Repository]]

# Check list
- [ ] `pyproject.toml` lists `pytest`, `pytest-bdd`, `coverage`, `mutmut` as dev dependencies with version ranges.
- [ ] Every `.feature` scenario has a matching step definition that calls the package's real API.
- [ ] One `pytest` run executes every `{package}/test/` module; `coverage` reports that run.
- [ ] The built wheel holds no `test/` module and no `.feature` file.
- [ ] `make test-kind-unit`, `make test-kind-mutation`, `make test-report`, and `make test-and-report` exist at the repository root and support the toggles defined by [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract).
- [ ] `$TEST_KIND_DIR/result/*.json` — `scenarios.json` included, written on a red run too — and `$TEST_KIND_DIR/report/<kind>/` follow that same contract's schema.
- [ ] `@status/todo` scenarios are excluded from the run and listed with their reason in the living doc, `$TEST_REPORT_DIR/reports/tests/livingdoc/`.
