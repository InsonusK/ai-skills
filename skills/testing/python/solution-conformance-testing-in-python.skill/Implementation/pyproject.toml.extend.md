---
description: Declare pytest, pytest-bdd, coverage and mutmut as dev dependencies and configure pytest collection, the tag markers, coverage and mutmut
project_name: "{Package}"
name: "pyproject.toml"
element_kind: project
change_kind: extend
tags:
  - solution/conformance-testing-in-python
  - element/pyproject-toml
---

# Goals
- Make `pytest`, `pytest-bdd`, `coverage`, and `mutmut` available to CI and local development.
- Make one `pytest` run collect the plain `test/` suite and the `features/` scenarios, with one coverage result for both.

# Core Principles
- Dev-only tooling stays under `[project.optional-dependencies].dev` (or the project's existing dev-dependency group); it is never a runtime dependency of the package.

# Implementation changes
`{package}` = the import name of the package; `{source-root}` = the directory `mutmut` mutates — `src` in a `src/` layout, else the package directory.
```toml
[project.optional-dependencies]
dev = [
    "pytest>=8",
    "pytest-bdd>=8.1,<10",
    "coverage>=7",
    "mutmut>=3.8,<4",
]

[tool.pytest.ini_options]
testpaths = ["test", "features"]
python_files = ["*_test.py", "*_steps.py"]
markers = [
    "todo: scenario planned but not runnable yet - excluded from the run",
    "happy", "boundary", "negative", "error", "concurrency", "security", "regression",
]

[tool.coverage.run]
source = ["{package}"]
branch = true

[tool.coverage.report]
fail_under = 80

[tool.mutmut]
source_paths = ["{source-root}"]
also_copy = ["features"]
pytest_add_cli_args = ["-m", "not todo"]
```

# Rule changes

## MUST
- Add `pytest`, `pytest-bdd`, `coverage`, `mutmut` to the dev dependency group with the version ranges above, not to the package's runtime dependencies.
  - Violation: unpinned `pytest-bdd` / `mutmut`, or either as a runtime dependency.
  - Risk: `tools/testing/kinds/unit_scenarios.py` uses `pytest-bdd`'s hooks and `mutation.sh` uses `mutmut` 3's commands (`export-cicd-stats`, mutant-name patterns) — another major version breaks them silently; end users installing the package pull in test-only tooling.
  - Fix: keep the ranges; move to a new major only after `make test-and-report` passes on it.
- Set `python_files` to `*_test.py` and `*_steps.py`, and `testpaths` to `test` and `features`.
  - Risk: `pytest` does not collect a `*_steps.py` module by default, so every scenario is silently not run and the scenario report shows `missing`.
  - Fix: keep both patterns; a step module binds its feature with `scenarios(...)`.
- Register `todo` and the seven scenario type tags as `markers`.
  - Risk: `pytest-bdd` turns every Gherkin tag into a marker — an unregistered one warns on every run, and fails it under `--strict-markers`.
  - Fix: keep the `markers` list; add a project's own tags to it.
- Point `[tool.coverage.run] source` at the package, and `[tool.mutmut] source_paths` at the directory holding it.
  - Risk: a wrong `source` silently leaves code out of the coverage number; without `source_paths` `mutmut` guesses the directory and stops when it cannot.
  - Fix: `source = ["{package}"]`, `source_paths = ["{source-root}"]`.
- Keep `also_copy = ["features"]` and `pytest_add_cli_args = ["-m", "not todo"]` under `[tool.mutmut]`.
  - Risk: `mutmut` runs the tests in a copy under `./mutants`; without the `.feature` files there the step modules fail to import, and without the marker filter `@todo` scenarios run and fail its clean run.
  - Fix: keep both keys.

# Check list
- [ ] `pytest`, `pytest-bdd`, `coverage`, `mutmut` are listed under the dev dependency group with version ranges.
- [ ] `pytest --collect-only -q` lists the scenarios of every `features/steps/*_steps.py` beside the `test/` tests, with no unknown-marker warning.
- [ ] `[tool.coverage.run] source` covers the package under test; `[tool.mutmut]` carries `source_paths`, `also_copy`, `pytest_add_cli_args`.
