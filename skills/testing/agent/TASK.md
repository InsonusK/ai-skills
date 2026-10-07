# Task: run the examples and prove the testing contract works

For the agent that picks this branch up in a container with Go, .NET, Node and Python (`.devcontainer/devcontainer.json` on this branch adds them; the container must be rebuilt first).

## Why this task exists

Waves W2 and W3 (see `STATUS.md`) rewrote the testing contract — `make` targets, scripts, Go tools, CI workflows — in a container with **no** Go, .NET, Node, Python or Docker. Everything was checked as far as `bash`, `make`, `jq` and `perl` allow, and nothing that needs a toolchain was run. Your job is to run it, fix what breaks, and leave the examples green.

Read first: `INVARIANTS.md` §5 (the contract), then `skills/testing/core/solution-conformance-testing.skill/Implementation/Repository.create.md`.

## What was verified without a toolchain

- `tools/testing/` (`testing.mk`, `testing.sh`, `kind.sh`, `test-report.sh`): exercised end to end with fake kind scripts — `report` and `check` runs, a skipped kind, a failing kind, custom work/report directories, a missing and a stale README badge, an unknown kind, a bad `TEST_RUN_PURPOSE`.
- In the real examples: `make test-kinds`, `make test-readme-check`, and `make test-kind-mutation TEST_RUN_PURPOSE=check` (the skip path) run and pass.
- `bash -n` on every script.

## What was not run — check each

| # | Where | Risk |
| --- | --- | --- |
| 1 | Go: `tools/{normalize_unittest,normalize_scenarios,normalize_mutation}/main.go` | Edited without a compiler: each got `kindDir()` and writes below `TEST_KIND_DIR`. Must build and vet. The Go report builder was removed — Go now uses the shared `tools/testing/test-report.sh` (needs `jq`), never run on Go results. |
| 2 | Go `tools/testing/kinds/unit.sh` | The former Makefile recipe as a script: `CUCUMBER_JSON_DIR` and `-coverprofile` point below `$REPORT_DIR` (absolute); in a `check` run `report/coverage/` is removed after the run. |
| 3 | Go `tools/testing/kinds/mutation.sh` | The contract now says no kind exits non-zero over a score in a `report` run, and the report workflow no longer has `continue-on-error`. Find out what `gremlins` returns when mutants survive and make the Go kind conform (dotnet: `--break-at 0`; typescript: `thresholds.break = 0`). Same question for `mutmut`. |
| 4 | dotnet `tools/testing/kinds/unit.sh` | Now restores and builds itself and finds the solution file (`*.slnx` / `*.sln`) instead of a hard-coded name; `TestResults` below the kind directory; copies every `reqnroll_messages.ndjson` to `report/tests/cucumber/{Project}.ndjson` and calls `kind_livingdoc` (Cucumber Messages path — never run for dotnet before). |
| 5 | dotnet `tools/testing/kinds/mutation.sh` | Stryker's `-O` output now below the kind directory; `report-template/index.html` links `reports/mutation/reports/mutation-report.html`. |
| 6 | Python and TypeScript templates | No runnable example exists. Python: `--format behave_cucumber_formatter:PrettyCucumberJSONFormatter` (from the package's PyPI page) and the pre-existing `VERIFY` placeholders for `mutmut`. Build a minimal project from each skill if you can; otherwise say they stay unverified. |
| 7 | `skills/devops/workflows/*/templates/*.example.md` | The YAML was never executed: dynamic matrix from `make -s test-kinds`, `include-hidden-files`, restoring `test-kind-*` artifacts under `$TEST_WORK_DIR/kinds/`. Review by reading; run in a scratch repository if one is available. |
| 8 | `skills/go/architecture/plateau/gw009-001` example | Carries the pre-release TaskBox copy; its plateau skill says mutation results are not evidence there. Its unit kind may need PostgreSQL — read the plateau skill before running. |
| 9 | Python: switch `behave` → `pytest-bdd` | Owner's decision (2026-10-07), not implemented: `behave` + `behave-cucumber-formatter` is in the skill as an interim. `pytest-bdd` writes classic Cucumber JSON itself (`--cucumberjson`), runs inside the `pytest` run the script already makes for `test/`, and gives one exit code and one coverage run. Rewrite `assets/tools/testing/kinds/unit.sh` (one `coverage run -m pytest … -m "not todo" --cucumberjson=… --junitxml=…`; counts from the JUnit XML so plain tests are counted too; scenario results from the Cucumber JSON — check what `line` it reports for a Scenario Outline row), the step-definition Implementation file, `pyproject.toml.extend`, the requirements, and the tool-choice ADR. Build a minimal package to prove it. |

## Steps

1. `bash skills/testing/agent/check.sh` — must pass before and after your changes.
2. In each example (`skills/go/architecture/plateau/*/*.skill/example`, `skills/dotnet/architecture/plateau/*/*.skill/example`):
   - `make test-and-report` → exit 0; `tmp/testing/report/` holds `index.html`, `run.json`, `badges/{tests,coverage,mutation}.json`, `reports/{tests,coverage,mutation,scenarios}/`, and `reports/tests/livingdoc/index.html`.
   - `make test-and-report TEST_RUN_PURPOSE=check TEST_WORK_DIR=out/work TEST_REPORT_DIR=out/site/testing` → nothing written to `tmp/` or `public/`; `run.json` shows `mutation` as `skipped`; only `badges/tests.json`; no `reports/coverage/`.
   - `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=HEAD~1` → runs scoped to changed files, or reports nothing to mutate.
   - Break one scenario on purpose → `make test-kind-unit` exits non-zero and `result/scenarios.json` still exists; `make test-report` still builds. Revert.
3. Open one built report in a browser: every link on `index.html` resolves, "This run" shows the purpose and each kind.
4. Run `validation_queue.py` so `.validation/*-log.yaml` picks up the moved skills (see `skills/design/skill-validation.skill/`).

## Rules while fixing

- Code has one source: a real file under the skill's `assets/` (copied verbatim) or `templates/` (placeholders filled). Change it there **and** in every example copy in the same commit — `check.sh` §6 and §10 fail otherwise.
- Shared by every stack, in `skills/testing/core/solution-conformance-testing.skill/assets/`: `tools/testing/` (the Makefile side, the runner, the report builder, `kind.sh`) and `tools/livingdoc/`. Stack-specific: only `tools/testing/kinds/{kind}.sh` in the stack skill's `assets/` — and for Go the three normalizers those scripts call.
- A project's `Makefile` carries `include tools/testing/testing.mk` and no testing recipe (`check.sh` §11).
- Do not add a caller-facing variable or target; if the contract itself is wrong, record it in `DECISIONS.md` with ⚠️ and stop for the owner.
- One commit per stack; update `STATUS.md` with what ran and its result. Report failures with their output — do not mark an example verified that was not run.
