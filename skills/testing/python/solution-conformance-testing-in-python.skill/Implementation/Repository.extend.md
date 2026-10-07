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
/scripts
  unit-test.sh
  normalize-scenarios.sh
  mutation-test.sh
  test-report.sh
Makefile                       — extended; created when missing
README.md                      — one badge per declared badge
/tools
  /testing                     — testing.mk, testing.sh, copied verbatim from solution-conformance-testing
  /livingdoc                   — package.json, package-lock.json, render.mjs, copied verbatim
pyproject.toml
README.md
```

## Directory and class skills
| Directory | file | Description |
| ----------------- | ----------- |
| /features | {rule}.feature, steps/{rule}_steps.py | Gherkin scenarios and their bindings |
| /report-template | index.html | Static landing page `test-report.sh` copies into `$TEST_REPORT_DIR/`; links to `reports/scenarios/`, `reports/tests/`, `reports/tests/livingdoc/`, `reports/coverage/`, `reports/mutation/`, and shows `run.json`. Kept outside `.github/` since this solution never owns `.github/workflows/*` |
| /scripts | unit-test.sh | Runs `behave`/`pytest` under `coverage`, normalizes results into `$TEST_KIND_DIR/result/unit-test.json` (+ `coverage-test.json` in a `report` run), keeps the native report under `$TEST_KIND_DIR/report/tests` (+ `$TEST_KIND_DIR/report/coverage`) |
| /scripts | normalize-scenarios.sh | `.feature` inventory + per-scenario results → `$TEST_KIND_DIR/result/scenarios.json`; identical across the .NET/Python/TypeScript variants |
| /scripts | mutation-test.sh | Runs `mutmut run` (in a `check` run scoped to files changed since `DELTA_BASE`), normalizes results into `$TEST_KIND_DIR/result/mutation-test.json`, keeps the native report under `$TEST_KIND_DIR/report/mutation` |
| /scripts | test-report.sh | Assembles `$TEST_REPORT_DIR/` — `scenarios/` included — from `$TEST_KIND_DIR/result/*.json` + `$TEST_KIND_DIR/report/*`; no test/build tooling involved |
| / | Makefile | Declares the `unit` and `mutation` kinds, includes `tools/testing/testing.mk`, and defines `test-kind-unit`/`test-kind-mutation`/`test-report-build` as required by [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] |

## Makefile
See [templates/Makefile.md](../templates/Makefile.md) for the full content.

## scripts/unit-test.sh
Runs `behave` and the plain `test/` suite under `coverage`, then normalizes the result. The JSON parsing (behave's own `json.pretty` formatter, modeled after Cucumber's JSON schema) and the `coverage`/`jq` calls are solid; the HTML-formatter line is a choice you still have to pin. See [templates/unit-test.sh.md](../templates/unit-test.sh.md) for the full script.

## scripts/normalize-scenarios.sh
Builds `$TEST_KIND_DIR/result/scenarios.json` per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-report|solution-conformance-testing's Scenario report]]; The runner's JSON `location` already gives `uri:line`. See [templates/normalize-scenarios.sh.md](../templates/normalize-scenarios.sh.md).

## scripts/mutation-test.sh
`mutmut`'s CLI for CI-friendly result export and for scoping a run to specific changed files has moved between major versions more than Stryker.NET/StrykerJS have — every `mutmut` line in the template is a sketch to verify against the version this project pins, not a copy-paste command. See [templates/mutation-test.sh.md](../templates/mutation-test.sh.md) for the full script and its `VERIFY`/`TODO` markers.

## scripts/test-report.sh
Pure assembly — no `python`/test tooling involved, so this same script (unmodified) also works for the .NET and TypeScript variants of this solution. See [templates/test-report.sh.md](../templates/test-report.sh.md) for the full script.

# Rules

## MUST
- `test-kind-unit`, `test-kind-mutation`, `test-report`, and `test-and-report` targets must exist and behave exactly as documented in [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] — this `Makefile` is the Python implementation of that contract, not a variation of it.
  - Violation: a CI workflow or a developer runs `mutmut`/`behave` directly instead of through `make test-kind-mutation`/`make test-kind-unit`.
  - Risk: the workflow now needs Python-specific knowledge, and switching or reconfiguring `mutmut` later becomes a breaking change for every CI file that calls it directly.
  - Fix: every caller (CI or a developer) goes through the `Makefile`; the project's own CI workflows call these targets stack-agnostically instead of the underlying tools directly.
- `scripts/unit-test.sh` and `scripts/mutation-test.sh` must write their normalized JSON into `$TEST_KIND_DIR/result/` and keep the native HTML report under `$TEST_KIND_DIR/report/<kind>/`, per the same contract.
  - Risk: without the normalized JSON, `make test-report` and badge generation have nothing stack-independent to read.
  - Fix: write both outputs exactly as [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] specifies.
- Before relying on `scripts/mutation-test.sh`, replace its placeholder `KILLED`/`SURVIVED`/`TIMEDOUT`/`NO_COVERAGE` parsing with a real export from the `mutmut` version the project pins, and verify the delta-scoping flag/config key used in `ONLY_DELTA=true` mode — see the `VERIFY` comments inline.
  - Risk: `mutmut`'s CLI has moved between major versions, so unverified placeholder parsing can silently report wrong `KILLED`/`SURVIVED` counts, or crash, once a real run happens.
  - Fix: replace the placeholder parsing with a real export from the pinned `mutmut` version, and confirm the delta-scoping flag/config key before relying on a `check` run with `DELTA_BASE`.
- `scripts/mutation-test.sh` must still exit with `mutmut`'s own exit code after writing `$TEST_KIND_DIR/result/mutation-test.json` — normalizing the result must never swallow a real mutation-testing failure.
  - Risk: a real mutation-testing failure gets swallowed by the normalization step, and CI reports success on a run that actually found unkilled mutants.
  - Fix: propagate `mutmut`'s exit code from the script after it finishes writing the normalized result.
- Pin `behave-cucumber-formatter` and keep `scripts/unit-test.sh` writing classic Cucumber JSON through `behave_cucumber_formatter:PrettyCucumberJSONFormatter` into `report/tests/cucumber/`, rendered by `tools/livingdoc/`.
  - Risk: behave's own `json` formatter is not classic Cucumber JSON (tags are plain strings), so the living-doc renderer shows empty or broken tag columns, or there is no readable test report at all.
  - Fix: add the package to the `dev` extra in `pyproject.toml` and keep both `--format` lines of the script — `json.pretty` for the counts, the Cucumber formatter for the living doc.
- `scripts/unit-test.sh` must exclude `@todo` scenarios from the run, write `$TEST_KIND_DIR/result/scenarios.json` through `scripts/normalize-scenarios.sh` on every run — including a red one — and only then exit with the runner's own code.
  - Risk: under `set -e` a failing runner ends the script before the scenario report is written, so the report is missing or stale exactly on the red run it should describe.
  - Fix: wrap the runner in `set +e`/`set -e`, keep its exit code, normalize, then `exit` with it.
- `scripts/normalize-scenarios.sh` must stay byte-identical across the .NET, Python, and TypeScript variants of this solution, like `scripts/test-report.sh`.
  - Risk: a stack-local tweak to the inventory scan makes the same `.feature` file produce different entries per stack, and the report stops being comparable.
  - Fix: change it in all three variants together, or not at all.
- Declare `TEST_KINDS := unit mutation` and their badges before `include tools/testing/testing.mk`, and place that block after the first ordinary target so `make` without arguments keeps its default goal.
  - Risk: a declaration after the include is ignored; an include before every other target makes `test-kinds` the default goal.
  - Fix: keep the order in the Makefile template; `test-and-report` itself comes from `tools/testing/testing.mk`.
- Never add a caller-facing variable beyond `TEST_RUN_PURPOSE`/`DELTA_BASE`/`TEST_WORK_DIR`/`TEST_REPORT_DIR` — a caller must not need to know this is a Python project.
  - Risk: every caller (CI workflow, developer, script) now needs Python-specific knowledge to invoke the targets correctly, defeating the point of the uniform contract this `Makefile` implements.
  - Fix: keep the `make` interface limited to the toggles [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] defines; anything Python-specific stays inside the `Makefile`/scripts.
- Never let `report-template/index.html` live under `.github/`.
  - Risk: nesting a project-owned static asset inside `.github/` implies this solution owns a workflow or publishing configuration it does not — the actual publishing step is a separate, layered CI concern this solution never owns.
  - Fix: keep it at `report-template/index.html`, copied by `test-report.sh` — never generated, never placed under `.github/`.

# Unittest TestCases
- [ ] WHEN `make test-kind-unit` runs THEN `$TEST_KIND_DIR/result/unit-test.json` and `$TEST_KIND_DIR/report/tests/` exist.
- [ ] WHEN `make test-kind-unit TEST_RUN_PURPOSE=report` runs THEN `$TEST_KIND_DIR/result/coverage-test.json` and `$TEST_KIND_DIR/report/coverage/` also exist.
- [ ] WHEN `make test-kind-unit` runs and a scenario fails THEN `$TEST_KIND_DIR/result/scenarios.json` still lists every `.feature` entry, `@todo` ones with status `todo`, and the target exits non-zero.
- [ ] WHEN `make test-report` runs THEN `$TEST_REPORT_DIR/reports/scenarios/index.html` shows the type × status table and every entry.
- [ ] WHEN `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=<ref>` runs THEN only mutants in code changed since `<ref>` are evaluated.
- [ ] WHEN `make test-report` runs after both `*-test` targets THEN `$TEST_REPORT_DIR/` contains the badge JSON files and copies of the native reports.
- [ ] WHEN `make test-and-report` runs THEN it produces the same end state as running `test-kind-unit`, `test-kind-mutation`, and `test-report` in sequence by hand.
