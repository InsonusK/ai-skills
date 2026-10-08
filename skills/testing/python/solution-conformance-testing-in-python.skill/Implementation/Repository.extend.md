---
description: Add the Python test kinds (unit, mutation) as kind scripts behind the shared testing contract, the pytest plugin the unit kind loads, report-template/index.html and the README badges
element_kind: repository
change_kind: extend
tags:
  - solution/conformance-testing-in-python
  - element/repository
---

# Structure

## Project Structure
```
/src/{package}                 — or /{package} without a src/ layout
  {module}.py
  /features
    {rule}.feature
  /test
    {rule}_steps_test.py
/report-template
  index.html
Makefile                       — one line added: include tools/testing/testing.mk
README.md                      — one badge per declared badge
.gitignore                     — lines added, see below
/tools
  /testing                     — copied verbatim from solution-conformance-testing
    /kinds
      unit.sh                  — this stack's unit kind
      unit_scenarios.py        — pytest plugin unit.sh loads: each scenario's own line and status
      mutation.sh              — this stack's mutation kind
  /livingdoc                   — package.json, package-lock.json, render.mjs, copied verbatim
pyproject.toml
```

## Directory and class skills
| Directory | file | Description |
| --------- | ---- | ----------- |
| /src/{package}/features | {rule}.feature | Gherkin scenarios, beside the code they specify |
| /src/{package}/test | {rule}_steps_test.py | Their step modules; a plain `{module}_test.py` only where a scenario would be unjustifiably complex |
| /report-template | index.html | Landing page `tools/testing/test-report.sh` publishes into `$TEST_REPORT_DIR/` with its `<!-- test-reports -->` line replaced by one item per report — badge, then link; shows `run.json`. Kept outside `.github/` since this solution never owns `.github/workflows/*` |
| /tools/testing/kinds | unit.sh, unit_scenarios.py, mutation.sh | This stack's two test kinds — run `pytest` / `mutmut`, write the normalized results |

## Kind scripts
The only stack-specific code — the Makefile, the runner and the report builder are the base's `tools/testing/`. Copy verbatim, as a folder, to `tools/testing/kinds/`: [`assets/tools/testing/kinds/`](../assets/tools/testing/kinds/) — `unit.sh`, `unit_scenarios.py`, `mutation.sh`.

`report-template/index.html` — fill and copy the base's template, `{project-name}` = the package name: [`templates/report-template/index.html`](skills/testing/core/solution-conformance-testing.skill/templates/report-template/index.html)

## .gitignore
Beside the base's `tmp/` and `tools/livingdoc/node_modules/`: `.venv/`, `*.egg-info/`, `__pycache__/`, and `mutants/`, `.coverage`, `.pytest_cache/` (a `mutmut` or `pytest` started by hand — the kinds leave none of them).

## Virtual environment
The kinds run `pip install -e ".[dev]"` and call `coverage` and `mutmut` from `PATH`, so they need an environment of the project's own. Add to the `Makefile`, before the include line — `make init` once, and every `make test-kind-*` uses it; where the environment already exists (a CI job after its Python setup step) the block changes nothing:
```makefile
VENV := .venv
export PATH := $(CURDIR)/$(VENV)/bin:$(PATH)

.PHONY: init
init:
	python3 -m venv $(VENV)
	$(VENV)/bin/pip install --quiet -e ".[dev]"
```

## What the kinds write
| Kind | Below `$TEST_KIND_DIR` |
| --- | --- |
| `unit` | `badges/tests.json` and `result/unit-test.json` (counts from `report/tests/junit.xml`), `result/scenarios.json` and `report/scenarios/`, `report/tests/cucumber/pytest-bdd.json` (classic Cucumber JSON), `report/tests/livingdoc/`; in a `report` run also `badges/coverage.json`, `result/coverage-test.json` and `report/coverage/` |
| `mutation` | `badges/mutation.json` and `result/mutation-test.json` (from `mutmut export-cicd-stats`), `report/mutation/results.txt` (every mutant and its status), `report/mutation/mutmut.log` |

# Rules

## MUST
- Run every test — `pytest-bdd` scenarios and plain tests — in the one `coverage run -m pytest` of `tools/testing/kinds/unit.sh`, and exit with its exit code after the results are written.
  - Violation: a second runner invocation for the scenarios, or `mutmut`/`pytest` called by a CI workflow directly.
  - Risk: two runs give two exit codes and two coverage data files to reconcile; a caller that names a tool needs Python knowledge the contract exists to hide.
  - Fix: keep the single run of the script; every caller goes through `make test-kind-unit`.
- Load the `unit_scenarios` plugin in that run (`-p unit_scenarios`, `PYTHONPATH=tools/testing/kinds`) and feed its output to `tools/testing/normalize-scenarios.sh`.
  - Violation: building the scenario results from `--cucumberjson`.
  - Risk: `pytest-bdd`'s Cucumber JSON gives every row of a `Scenario Outline` the outline's line, so the scenario report cannot tell one `Examples:` block from another and marks them all `missing`.
  - Fix: keep the plugin — it reports the row's own line; `--cucumberjson` stays as the standard report for the living doc only.
- Pass `--fail-under=0` to the `coverage html` / `coverage json` calls of a `report` run.
  - Risk: with `fail_under` in `pyproject.toml` these commands exit `2` below the threshold, and a `report` run goes red over a score.
  - Fix: keep the flag in `unit.sh`; the project enforces its threshold with its own `coverage report` where it gates.
- Keep `COVERAGE_FILE`, `--junitxml`, `--cucumberjson` and `-p no:cacheprovider` as `unit.sh` sets them, and let `mutation.sh` remove `./mutants` when it is done.
  - Risk: `.coverage`, `.pytest_cache` or `mutants/` left in the repository root is output outside `$TEST_KIND_DIR` — two kinds running in parallel overwrite each other, and a CI job cannot hand the result over as one directory.
  - Fix: copy both scripts unchanged; `mutmut` accepts no other working directory than `./mutants`, so the script deletes it after reading the result.
- Scope a `check` run of `mutation.sh` by mutant-name patterns built from the source files changed since `DELTA_BASE` (`{module path}.x*`), and skip the kind when no source file changed.
  - Risk: `mutmut` 3 has no path option on its command line; an unknown flag or a config rewrite silently mutates the whole package or nothing.
  - Fix: keep the loop in `mutation.sh`; it resolves `DELTA_BASE` with `git rev-parse`, leaves test code out, and treats "nothing matches" — changed files without mutable code — as a run with no mutants.
- Exit from `mutation.sh` with `mutmut`'s own exit code after `result/mutation-test.json` is written.
  - Risk: `mutmut` has no score threshold — it exits `0` however many mutants survive and non-zero only when it could not run (a red test fails its clean run); swallowing that code hides a broken run.
  - Fix: keep `exit "$code"` as the script's last line.
- Add only `include tools/testing/testing.mk` to the `Makefile`, after its first target; never a testing recipe.
  - Risk: a recipe in the project's `Makefile` duplicates a kind script and drifts from it; an include placed first makes `test-kinds` the default goal.
  - Fix: append the include line; everything a kind does lives in `tools/testing/kinds/`.
- Never add a caller-facing variable beyond `TEST_RUN_PURPOSE`/`DELTA_BASE`/`TEST_WORK_DIR`/`TEST_REPORT_DIR`, per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]].
  - Risk: a caller needs Python knowledge to invoke the targets, defeating the uniform contract.
  - Fix: anything Python-specific stays inside the kind scripts and `pyproject.toml`.
- Never let `report-template/index.html` live under `.github/`.
  - Risk: a project-owned static asset inside `.github/` implies this solution owns a workflow it does not.
  - Fix: keep it at `report-template/index.html`, copied by `tools/testing/test-report.sh`.

# Unittest TestCases
- [ ] WHEN `make test-kind-unit` runs THEN `$TEST_KIND_DIR/result/unit-test.json` counts scenarios and plain tests together, and `report/tests/junit.xml`, `report/tests/cucumber/pytest-bdd.json`, `report/tests/livingdoc/index.html` exist.
- [ ] WHEN `make test-kind-unit` runs as a `report` run with coverage below `fail_under` THEN it exits `0` and writes `result/coverage-test.json` and `report/coverage/`.
- [ ] WHEN a scenario fails THEN `make test-kind-unit` exits non-zero, and `result/scenarios.json` still lists every `.feature` entry — the failed `Examples:` block as `failed`, its sibling blocks as `passed`, `@todo` entries as `todo`.
- [ ] WHEN `make test-kind-unit` or `make test-kind-mutation` ends THEN the repository root holds no `.coverage`, `.pytest_cache` or `mutants/`.
- [ ] WHEN `make test-kind-mutation` runs as a `report` run and mutants survive THEN it exits `0`; WHEN a test is red THEN it exits non-zero and writes no result.
- [ ] WHEN `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=<ref>` runs THEN only mutants of the source files changed since `<ref>` are evaluated; without `DELTA_BASE`, or with no source file changed, the kind skips itself.
- [ ] WHEN `make init` ran in a fresh checkout THEN `make test-and-report` passes with no Python package installed outside `.venv/`.
- [ ] WHEN `make test-and-report` runs THEN `$TEST_REPORT_DIR` holds `index.html`, `run.json`, `badges/{tests,coverage,mutation}.json` and `reports/{tests,coverage,mutation,scenarios}/`.
