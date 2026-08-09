---
name: testing tool choice
description: Which Gherkin runner, coverage tool, and mutation-testing tool the Python conformance-testing solution uses
problem: Pick one Gherkin/BDD runner, one coverage tool, and one mutation-testing tool for Python projects that must satisfy the bdd-coverage-mutation-testing gate
decision: behave + coverage.py + mutmut
---

# Problem
[bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md) requires a Gherkin runner, a coverage tool, and a mutation-testing tool, but leaves the concrete choice to each stack. Python needs one specific, documented choice so every package applying this solution uses the same tools.

# Selected variant
**Selected variant:** [[#behave coverage.py mutmut]]

# Searched variants

## behave coverage.py mutmut

### Description
Use `behave` for Gherkin scenarios (the most widely used Python Cucumber implementation), `coverage.py` for coverage (the standard tool, already assumed by [code-coverage](skills/common-workflow/test/code-coverage.skill.md)'s threshold guidance), and `mutmut` for mutation testing.

### Benefits
- `behave` has the largest community and plugin ecosystem among Python Gherkin runners.
- `coverage.py` is already the de-facto standard and works with either `unittest` or `pytest`, matching [solution-test](skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md)'s runner-agnostic stance.
- `mutmut` supports running an arbitrary test command, so it can exercise the combined `test/` + `features/` suite without a second, parallel toolchain.

### Costs
- `behave`'s step-matching (regex/parse-based) is less type-safe than a typed alternative, so a step definition can silently fail to match a new scenario phrasing.
- `mutmut` mutation runs are slow on large packages, which is why the PR gate must stay delta-scoped per [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#ci-enforcement).

## pytest-bdd instead of behave

### Description
Use `pytest-bdd`, which expresses Gherkin scenarios as `pytest` test functions instead of a separate runner.

### Benefits
- Scenarios run inside the existing `pytest` process, one less CLI invocation in CI.
- Reuses `pytest` fixtures directly.

### Costs
- Ties the Gherkin layer to `pytest` specifically, while [solution-test](skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md) deliberately keeps the plain unit-test runner optional (`unittest` or `pytest`).

## cosmic-ray instead of mutmut

### Description
Use `cosmic-ray` for mutation testing instead of `mutmut`.

### Benefits
- More configurable mutation operator set.

### Costs
- Steeper setup (separate database-backed workflow) for a benefit this solution does not need; `mutmut`'s simpler CLI-driven flow fits the PR-gate/trunk-gate split more directly.
