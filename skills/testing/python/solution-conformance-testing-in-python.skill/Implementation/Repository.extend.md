---
description: Add the test kinds (unit, mutation), the report builder and their normalization scripts to the Makefile behind the shared testing contract
element_kind: repository
change_kind: extend
tags:
  - solution/conformance-testing-in-python
  - element/repository
---

# Structure

## Project Structure
```
/{package}
/test
/features
  {rule}.feature
  /steps
    {rule}_steps.py
/report-template
  index.html
Makefile                       — one line added: include tools/testing/testing.mk
README.md                      — one badge per declared badge
/tools
  /testing                     — copied verbatim from solution-conformance-testing
    /kinds
      unit.sh                  — this stack's unit kind
      mutation.sh              — this stack's mutation kind
  /livingdoc                   — package.json, package-lock.json, render.mjs, copied verbatim
pyproject.toml
README.md
```

## Directory and class skills
| Directory | file | Description |
| ----------------- | ----------- |
| /features | {rule}.feature, steps/{rule}_steps.py | Gherkin scenarios and their bindings |
| /report-template | index.html | Static landing page `tools/testing/test-report.sh` copies into `$TEST_REPORT_DIR/`; links to `reports/scenarios/`, `reports/tests/`, `reports/tests/livingdoc/`, `reports/coverage/`, `reports/mutation/`, and shows `run.json`. Kept outside `.github/` since this solution never owns `.github/workflows/*` |

## Kind scripts
The only stack-specific code: each runs this stack's tool and writes the normalized `result/*.json` and native `report/{name}/` — the Makefile, the runner and the report builder are the base's `tools/testing/`.
- Copy verbatim to `tools/testing/kinds/unit.sh`: [`assets/tools/testing/kinds/unit.sh`](../assets/tools/testing/kinds/unit.sh)
- Fill and copy to `tools/testing/kinds/mutation.sh` — `{package}` = the package's source directory: [`templates/tools/testing/kinds/mutation.sh`](../templates/tools/testing/kinds/mutation.sh)

# Rules

## MUST
- `test-kind-unit`, `test-kind-mutation`, `test-report`, and `test-and-report` targets must exist and behave exactly as documented in [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] — this `Makefile` is the Python implementation of that contract, not a variation of it.
  - Violation: a CI workflow or a developer runs `mutmut`/`behave` directly instead of through `make test-kind-mutation`/`make test-kind-unit`.
  - Risk: the workflow now needs Python-specific knowledge, and switching or reconfiguring `mutmut` later becomes a breaking change for every CI file that calls it directly.
  - Fix: every caller (CI or a developer) goes through the `Makefile`; the project's own CI workflows call these targets stack-agnostically instead of the underlying tools directly.
- `tools/testing/kinds/unit.sh` and `tools/testing/kinds/mutation.sh` must write their normalized JSON into `$TEST_KIND_DIR/result/` and keep the native HTML report under `$TEST_KIND_DIR/report/<kind>/`, per the same contract.
  - Risk: without the normalized JSON, `make test-report` and badge generation have nothing stack-independent to read.
  - Fix: write both outputs exactly as [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] specifies.
- Before relying on `tools/testing/kinds/mutation.sh`, replace its placeholder `KILLED`/`SURVIVED`/`TIMEDOUT`/`NO_COVERAGE` parsing with a real export from the `mutmut` version the project pins, and verify the delta-scoping flag/config key used in a `check` run with `DELTA_BASE` — see the `VERIFY` comments inline.
  - Risk: `mutmut`'s CLI has moved between major versions, so unverified placeholder parsing can silently report wrong `KILLED`/`SURVIVED` counts, or crash, once a real run happens.
  - Fix: replace the placeholder parsing with a real export from the pinned `mutmut` version, and confirm the delta-scoping flag/config key before relying on a `check` run with `DELTA_BASE`.
- `tools/testing/kinds/mutation.sh` must still exit with `mutmut`'s own exit code after writing `$TEST_KIND_DIR/result/mutation-test.json` — normalizing the result must never swallow a real mutation-testing failure.
  - Risk: a real mutation-testing failure gets swallowed by the normalization step, and CI reports success on a run that actually found unkilled mutants.
  - Fix: propagate `mutmut`'s exit code from the script after it finishes writing the normalized result.
- Pin `behave-cucumber-formatter` and keep `tools/testing/kinds/unit.sh` writing classic Cucumber JSON through `behave_cucumber_formatter:PrettyCucumberJSONFormatter` into `report/tests/cucumber/`, rendered by `tools/livingdoc/`.
  - Risk: behave's own `json` formatter is not classic Cucumber JSON (tags are plain strings), so the living-doc renderer shows empty or broken tag columns, or there is no readable test report at all.
  - Fix: add the package to the `dev` extra in `pyproject.toml` and keep both `--format` lines of the script — `json.pretty` for the counts, the Cucumber formatter for the living doc.
- `tools/testing/kinds/unit.sh` must exclude `@todo` scenarios from the run, write `$TEST_KIND_DIR/result/scenarios.json` through `tools/testing/normalize-scenarios.sh` on every run — including a red one — and only then exit with the runner's own code.
  - Risk: under `set -e` a failing runner ends the script before the scenario report is written, so the report is missing or stale exactly on the red run it should describe.
  - Fix: wrap the runner in `set +e`/`set -e`, keep its exit code, normalize, then `exit` with it.
- Add only `include tools/testing/testing.mk` to the `Makefile`, after its first target; never a testing recipe.
  - Risk: a recipe in the project's `Makefile` duplicates a kind script and drifts from it; an include placed first makes `test-kinds` the default goal.
  - Fix: append the include line; everything a kind does lives in `tools/testing/kinds/{kind}.sh`.
- Never add a caller-facing variable beyond `TEST_RUN_PURPOSE`/`DELTA_BASE`/`TEST_WORK_DIR`/`TEST_REPORT_DIR` — a caller must not need to know this is a Python project.
  - Risk: every caller (CI workflow, developer, script) now needs Python-specific knowledge to invoke the targets correctly, defeating the point of the uniform contract this `Makefile` implements.
  - Fix: keep the `make` interface limited to the toggles [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] defines; anything Python-specific stays inside the `Makefile`/scripts.
- Never let `report-template/index.html` live under `.github/`.
  - Risk: nesting a project-owned static asset inside `.github/` implies this solution owns a workflow or publishing configuration it does not — the actual publishing step is a separate, layered CI concern this solution never owns.
  - Fix: keep it at `report-template/index.html`, copied by `tools/testing/test-report.sh` — never generated, never placed under `.github/`.

# Unittest TestCases
- [ ] WHEN `make test-kind-unit` runs THEN `$TEST_KIND_DIR/result/unit-test.json` and `$TEST_KIND_DIR/report/tests/` exist.
- [ ] WHEN `make test-kind-unit TEST_RUN_PURPOSE=report` runs THEN `$TEST_KIND_DIR/result/coverage-test.json` and `$TEST_KIND_DIR/report/coverage/` also exist.
- [ ] WHEN `make test-kind-unit` runs and a scenario fails THEN `$TEST_KIND_DIR/result/scenarios.json` still lists every `.feature` entry, `@todo` ones with status `todo`, and the target exits non-zero.
- [ ] WHEN `make test-report` runs THEN `$TEST_REPORT_DIR/reports/scenarios/index.html` shows the type × status table and every entry.
- [ ] WHEN `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=<ref>` runs THEN only mutants in code changed since `<ref>` are evaluated.
- [ ] WHEN `make test-report` runs after both `*-test` targets THEN `$TEST_REPORT_DIR/` contains the badge JSON files and copies of the native reports.
- [ ] WHEN `make test-and-report` runs THEN it produces the same end state as running `test-kind-unit`, `test-kind-mutation`, and `test-report` in sequence by hand.
