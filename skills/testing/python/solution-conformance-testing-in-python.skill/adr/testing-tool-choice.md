---
name: testing tool choice
description: Which Gherkin runner, coverage tool, and mutation-testing tool the Python conformance-testing solution uses
problem: Pick one Gherkin/BDD runner, one coverage tool, and one mutation-testing tool for Python projects that must satisfy the solution-conformance-testing gate
decision: pytest-bdd + coverage.py + mutmut
tags:
  - solution/conformance-testing-in-python
  - concern/documentation
  - concern/documentation/adr
  - stack/python
---

# Problem
[solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md) requires a Gherkin runner, a coverage tool, and a mutation-testing tool, but leaves the concrete choice to each stack. Python needs one specific, documented choice so every package applying this solution uses the same tools.

The first choice was `behave`. It was replaced on 2026-10-07 (owner's decision), when the testing contract required the runner's standard Cucumber report and one exit code per test kind: `behave` needs a third-party formatter for classic Cucumber JSON and a second runner invocation for the plain tests, and brings nothing in return here.

# Selected variant
**Selected variant:** [[#pytest-bdd coverage.py mutmut]]

# Searched variants

## pytest-bdd coverage.py mutmut

**Selected.**

### Description
Use `pytest-bdd` for Gherkin scenarios — each scenario becomes a `pytest` test of the step module that calls `scenarios(...)` — `coverage.py` for coverage, and `mutmut` 3 for mutation testing. One `coverage run -m pytest` executes the plain `test/` suite and the scenarios.

### Benefits
- One run gives one exit code, one coverage result and one JUnit report counting plain tests and scenarios together.
- Classic Cucumber JSON comes from the runner itself (`--cucumberjson`), with no formatter plugin.
- `@todo` is excluded with a marker filter (`-m "not todo"`), the same switch `mutmut` passes to its own runs.
- `mutmut` 3 runs the tests through `pytest`, so it exercises the same combined suite with no second toolchain; it exits `0` however many mutants survive, which a `report` run requires.

### Costs
- The Gherkin layer is tied to `pytest`: a project on plain `unittest` adds `pytest` as its runner (it runs `unittest` test cases unchanged).
- `pytest-bdd`'s Cucumber JSON reports the outline's line for every `Examples:` row, so the scenario report needs a small `pytest` plugin (`tools/testing/kinds/unit_scenarios.py`) that takes the row's line from the feature file.
- `mutmut` 3 accepts no working directory but `./mutants` and no path option on the command line: the kind script deletes the directory afterwards and scopes a delta run by mutant-name patterns.

## behave coverage.py mutmut

### Description
Use `behave` for Gherkin scenarios, run beside `pytest` for the plain tests, both under `coverage.py`.

### Benefits
- `behave` has the largest community among Python Gherkin runners.
- Works with either `unittest` or `pytest` for the plain tests.

### Costs
- Two runner invocations per unit kind: two exit codes to combine and `coverage run -a` to merge the data.
- Its `json` formatter is not classic Cucumber JSON (tags are plain strings); the living-doc report needs the `behave-cucumber-formatter` plugin.
- `mutmut` 3 runs the tests through `pytest`, so `behave` scenarios would not kill a single mutant.

## cosmic-ray instead of mutmut

### Description
Use `cosmic-ray` for mutation testing instead of `mutmut`.

### Benefits
- More configurable mutation operator set.

### Costs
- Steeper setup (separate database-backed workflow) for a benefit this solution does not need.
