---
description: Declare pytest, pytest-bdd, coverage and mutmut as dev dependencies, keep the co-located tests out of the installed package, and configure pytest collection, the tag markers, coverage and mutmut
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
- Make one `pytest` run collect every `{package}/test/` module, with one coverage result.
- Keep the tests that sit beside the code out of the installed package, out of the coverage number and out of mutation.

# Core Principles
- Dev-only tooling stays under `[project.optional-dependencies].dev` (or the project's existing dev-dependency group); it is never a runtime dependency of the package.

# Implementation changes
`{package}` = the import name of the package; `{source-root}` = the directory the packages are found in — `src` in a `src/` layout, else `.`. A project that already has a `[tool.setuptools.packages.find]` table adds the two `exclude` patterns to it.
```toml
[project.optional-dependencies]
dev = [
    "pytest>=8",
    "pytest-bdd>=8.1,<10",
    "coverage>=7",
    "mutmut>=3.8,<4",
]

[tool.setuptools.packages.find]
where = ["{source-root}"]
exclude = ["*.test", "*.test.*"]

[tool.pytest.ini_options]
testpaths = ["{source-root}"]
python_files = ["*_test.py"]
addopts = "--import-mode=importlib"
markers = [
    "todo: scenario planned but not runnable yet - excluded from the run",
    "happy", "boundary", "negative", "error", "concurrency", "security", "regression",
]

[tool.coverage.run]
source = ["{package}"]
branch = true
omit = ["*/test/*"]

[tool.coverage.report]
fail_under = 80

[tool.mutmut]
source_paths = ["{source-root}"]
do_not_mutate = ["*/test/*"]
pytest_add_cli_args = ["-m", "not todo"]
```

# Rule changes

## MUST
- Add `pytest`, `pytest-bdd`, `coverage`, `mutmut` to the dev dependency group with the version ranges above, not to the package's runtime dependencies.
  - Violation: unpinned `pytest-bdd` / `mutmut`, or either as a runtime dependency.
  - Risk: `tools/testing/kinds/unit_scenarios.py` uses `pytest-bdd`'s hooks and `mutation.sh` uses `mutmut` 3's commands (`export-cicd-stats`, mutant-name patterns) — another major version breaks them silently; end users installing the package pull in test-only tooling.
  - Fix: keep the ranges; move to a new major only after `make test-and-report` passes on it.
- Exclude `*.test` and `*.test.*` in `[tool.setuptools.packages.find]`.
  - Violation: `exclude = ["test*"]` (matches a top-level package only) or `exclude = ["*test*"]`.
  - Risk: measured on the example — with `"test*"` both test modules were in the wheel, and nothing fails when that happens; `"*test*"` removes every production package with `test` in its name (`latest`) just as silently.
  - Fix: the two patterns above; list the wheel once (`pip wheel . -w dist && unzip -l dist/*.whl`) — no `test/` path in it.
- Set `testpaths` to the source root, `python_files` to `*_test.py`, and `addopts` to `--import-mode=importlib`.
  - Risk: with the default import mode two packages that each have a `test/{name}_test.py` stop collection with "import file mismatch".
  - Fix: keep the three lines; a step module binds its feature with `scenarios(...)`.
- Keep `omit = ["*/test/*"]` under `[tool.coverage.run]` and `do_not_mutate = ["*/test/*"]` under `[tool.mutmut]`.
  - Risk: measured on the example — `mutmut` mutated the test module too, 6 mutants no test can kill, and the score fell from 93.5% to 78.4%; coverage counts test code as product code once a `test/` folder gets an `__init__.py`.
  - Fix: keep both lines.
- Register `todo` and the seven scenario type tags as `markers`.
  - Risk: `pytest-bdd` turns every Gherkin tag into a marker — an unregistered one warns on every run, and fails it under `--strict-markers`.
  - Fix: keep the `markers` list; add a project's own tags to it.
- Point `[tool.coverage.run] source` at the package, and `[tool.mutmut] source_paths` at the source root.
  - Risk: a wrong `source` silently leaves code out of the coverage number; without `source_paths` `mutmut` guesses the directory and stops when it cannot.
  - Fix: `source = ["{package}"]`, `source_paths = ["{source-root}"]`.
- Keep `pytest_add_cli_args = ["-m", "not todo"]` under `[tool.mutmut]`.
  - Risk: `mutmut` runs the tests in a copy under `./mutants`; without the marker filter `@todo` scenarios run and fail its clean run.
  - Fix: keep the key.

# Check list
- [ ] `pytest`, `pytest-bdd`, `coverage`, `mutmut` are listed under the dev dependency group with version ranges.
- [ ] `pytest --collect-only -q` lists the scenarios of every `test/*_steps_test.py` beside the plain tests, with no unknown-marker warning.
- [ ] The built wheel holds no `test/` path.
- [ ] `[tool.coverage.run] source` covers the package under test; `[tool.mutmut]` carries `source_paths`, `do_not_mutate`, `pytest_add_cli_args`.
