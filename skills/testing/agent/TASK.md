# Task: run the examples and prove the testing contract works

For the agent that picks this branch up in a container with Go, .NET, Node and Python (`.devcontainer/devcontainer.json` on this branch adds them; the container must be rebuilt first).

## Why this task exists

Waves W2 and W3 (see `STATUS.md`) rewrote the testing contract — `make` targets, scripts, Go tools, CI workflows — in a container with **no** Go, .NET, Node, Python or Docker. Everything was checked as far as `bash`, `make`, `jq` and `perl` allow, and nothing that needs a toolchain was run. Your job is to run it, fix what breaks, and leave the examples green.

Read first: `INVARIANTS.md` §5 (the contract), then `skills/testing/core/solution-conformance-testing.skill/Implementation/Repository.create.md`.

## What was verified without a toolchain

- `tools/testing/testing.mk` + `testing.sh`: exercised end to end against a fake project — `report` and `check` runs, a skipped kind, a custom work/report directory, a missing and a stale README badge, a bad `TEST_RUN_PURPOSE`.
- `scripts/test-report.sh` (dotnet/python/typescript): run against fake kind results — badges, report copies, scenario page.
- Every example `Makefile` parses; `make test-kinds` and `make test-readme-check` pass in all nine examples.
- `bash -n` on every script.

## What was not run — check each

| # | Where | Risk |
| --- | --- | --- |
| 1 | Go: `tools/{normalize_unittest,normalize_scenarios,normalize_mutation,test_report}/main.go` | Edited without a compiler: `kindDir()`, `workDir()`, `reportDir()`, `readResult()`, the report-copy loop in `test_report`. Must build and vet. |
| 2 | Go `Makefile`: `test-kind-unit` | `CUCUMBER_JSON_DIR` and `-coverprofile` now point below `$(abspath $(TEST_KIND_DIR))`; in a `check` run `report/coverage/` is removed after the run. |
| 3 | Go `Makefile`: `test-kind-mutation` | `gremlins`' exit code when mutants survive. In a `report` run the kind should not fail on the score (dotnet uses `--break-at 0`); if `gremlins` exits non-zero there, make the Go kind consistent. |
| 4 | dotnet `scripts/unit-test.sh` | `TestResults` moved below the kind directory; new living-doc block copies every `reqnroll_messages.ndjson` to `report/tests/cucumber/{Project}.ndjson` and calls `tools/livingdoc/render.mjs` (Cucumber Messages path — never run for dotnet before). |
| 5 | dotnet `scripts/mutation-test.sh` | Stryker's `-O` output now below the kind directory; `report-template/index.html` links `reports/mutation/reports/mutation-report.html`. |
| 6 | Python and TypeScript templates | No runnable example exists. Python: `--format behave_cucumber_formatter:PrettyCucumberJSONFormatter` (from the package's PyPI page) and the pre-existing `VERIFY` placeholders for `mutmut`. Build a minimal project from each skill if you can; otherwise say they stay unverified. |
| 7 | `skills/devops/workflows/*/templates/*.example.md` | The YAML was never executed: dynamic matrix from `make -s test-kinds`, `include-hidden-files`, restoring `test-kind-*` artifacts under `$TEST_WORK_DIR/kinds/`. Review by reading; run in a scratch repository if one is available. |
| 8 | `skills/go/architecture/plateau/gw009-001` example | Carries the pre-release TaskBox copy; its plateau skill says mutation results are not evidence there. Its unit kind may need PostgreSQL — read the plateau skill before running. |

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

- A shared file has one source: the code block in the skill's Implementation/template file. Change it there **and** in every example copy in the same commit — `check.sh` §6, §8, §10 fail otherwise.
- `tools/testing/testing.mk` and `testing.sh` are verbatim everywhere; a fix goes into `skills/testing/core/solution-conformance-testing.skill/Implementation/tools/testing/`.
- Do not add a caller-facing variable or target; if the contract itself is wrong, record it in `DECISIONS.md` with ⚠️ and stop for the owner.
- One commit per stack; update `STATUS.md` with what ran and its result. Report failures with their output — do not mark an example verified that was not run.
