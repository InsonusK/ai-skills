---
description: Add the test kinds (unit, mutation), the report builder and their normalization scripts to the Makefile behind the shared testing contract
element_kind: repository
change_kind: extend
tags:
  - solution/conformance-testing-in-typescript
  - element/repository
---

# Structure

## Project Structure
This `Makefile`/`scripts/` pair assumes a single-package repository, where the repository root is the package root (matching `{Package}.package.extend`'s `src/`, `features/`, `package.json`). In a monorepo with several packages, adapt every path below (`src/`, `features/`, `package.json`, `stryker.conf.json`, `cucumber.mjs`) to the one package this instance of the solution targets.
```
/src
  index.ts
  {rule}-validator.ts
/features
  {rule}.feature
  /step-definitions
    {rule}.steps.ts
/report-template
  index.html
/scripts
  unit-test.sh
  normalize-scenarios.sh
  messages-results.jq
  mutation-test.sh
  test-report.sh
Makefile                       — extended; created when missing
README.md                      — one badge per declared badge
/tools
  /testing                     — testing.mk, testing.sh, copied verbatim from solution-conformance-testing
  /livingdoc                   — package.json, package-lock.json, render.mjs, copied verbatim
package.json
cucumber.mjs
stryker.conf.json
README.md
```

## Directory and class skills
| Directory | file | Description |
| ----------------- | ----------- |
| /report-template | index.html | Static landing page `test-report.sh` copies into `$TEST_REPORT_DIR/`; links to `reports/scenarios/`, `reports/tests/`, `reports/tests/livingdoc/`, `reports/coverage/`, `reports/mutation/`, and shows `run.json`. Kept outside `.github/` since this solution never owns `.github/workflows/*` |
| /scripts | unit-test.sh | Runs `cucumber-js` (wrapped in `c8` in a `report` run), normalizes results into `$TEST_KIND_DIR/result/unit-test.json` (+ `coverage-test.json`), keeps the native report under `$TEST_KIND_DIR/report/tests` (+ `$TEST_KIND_DIR/report/coverage`) |
| /tools/livingdoc | package.json, package-lock.json, render.mjs | Copied verbatim from `solution-conformance-testing`; `unit-test.sh` renders `$TEST_KIND_DIR/report/tests/cucumber/messages.ndjson` → `$TEST_KIND_DIR/report/tests/livingdoc/` |
| /scripts | normalize-scenarios.sh | `.feature` inventory + per-scenario results → `$TEST_KIND_DIR/result/scenarios.json`; identical across the .NET/Python/TypeScript variants |
| /scripts | messages-results.jq | cucumber-js's Cucumber Messages → `[{uri, line, status}]` for `normalize-scenarios.sh` |
| /scripts | mutation-test.sh | Runs `stryker run` against a `stryker.conf.json`-derived config (in a `check` run scoped to files changed since `DELTA_BASE`), normalizes results into `$TEST_KIND_DIR/result/mutation-test.json`, keeps the native report under `$TEST_KIND_DIR/report/mutation` |
| /scripts | test-report.sh | Assembles `$TEST_REPORT_DIR/` — `scenarios/` included — from `$TEST_KIND_DIR/result/*.json` + `$TEST_KIND_DIR/report/*`; no test/build tooling involved |
| / | stryker.conf.json | Base Stryker config; `mutation-test.sh` patches its `reporters`/`thresholds`/reporter file paths per run, never edits it in place |
| / | Makefile | Declares the `unit` and `mutation` kinds, includes `tools/testing/testing.mk`, and defines `test-kind-unit`/`test-kind-mutation`/`test-report-build` as required by [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] |

## Makefile
See [templates/Makefile.md](../templates/Makefile.md) for the full content.

## scripts/unit-test.sh
Runs `cucumber-js` (and, in a `report` run, wraps it with `c8` for coverage), then normalizes the result. See [templates/unit-test.sh.md](../templates/unit-test.sh.md) for the full script.

## scripts/normalize-scenarios.sh
Builds `$TEST_KIND_DIR/result/scenarios.json` per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-report|solution-conformance-testing's Scenario report]]; `scripts/messages-results.jq` reduces cucumber-js's Cucumber Messages to `[{uri, line, status}]`. See [templates/normalize-scenarios.sh.md](../templates/normalize-scenarios.sh.md) and [templates/messages-results.jq.md](../templates/messages-results.jq.md).

## scripts/mutation-test.sh
StrykerJS has no native `--since`/delta flag the way Stryker.NET does, so this script emulates a `check` run's `DELTA_BASE` itself by limiting `--mutate` to the files `git diff` reports as changed. See [templates/mutation-test.sh.md](../templates/mutation-test.sh.md) for the full script.

## scripts/test-report.sh
Pure assembly — no `npm`/test tooling involved, so this same script (unmodified) also works for the .NET and Python variants of this solution. See [templates/test-report.sh.md](../templates/test-report.sh.md) for the full script.

# Rules

## MUST
- `test-kind-unit`, `test-kind-mutation`, `test-report`, and `test-and-report` targets must exist and behave exactly as documented in [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] — this `Makefile` is the TypeScript implementation of that contract, not a variation of it.
  - Violation: a CI workflow or a developer runs `stryker run`/`vitest`/`cucumber-js` directly instead of through `make test-kind-mutation`/`make test-kind-unit`.
  - Risk: the workflow now needs TypeScript-specific knowledge, and switching or reconfiguring Stryker later becomes a breaking change for every CI file that calls it directly.
  - Fix: every caller (CI or a developer) goes through the `Makefile`; the project's own CI workflows call these targets stack-agnostically instead of the underlying tools directly.
- `scripts/unit-test.sh` and `scripts/mutation-test.sh` must write their normalized JSON into `$TEST_KIND_DIR/result/` and keep the native HTML report under `$TEST_KIND_DIR/report/<kind>/`, per the same contract.
  - Risk: without the normalized JSON, `make test-report` and badge generation have nothing stack-independent to read.
  - Fix: write both outputs exactly as [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] specifies.
- `stryker.conf.json` must exist at the path `scripts/mutation-test.sh` reads (repository root for a single-package repository) — the script patches a copy of it per run, it never creates one from scratch.
  - Risk: without the base config file present, the script has nothing to patch and `make test-kind-mutation` fails outright.
  - Fix: commit a base `stryker.conf.json` at the path the script expects.
- `scripts/mutation-test.sh` must still exit with `stryker run`'s own exit code after writing `$TEST_KIND_DIR/result/mutation-test.json` — normalizing the result must never swallow a real mutation-testing failure.
  - Risk: a real mutation-testing failure gets swallowed by the normalization step, and CI reports success on a run that actually found unkilled mutants.
  - Fix: propagate `stryker run`'s exit code from the script after it finishes writing the normalized result.
- `scripts/unit-test.sh` must exclude `@todo` scenarios from the run, write `$TEST_KIND_DIR/result/scenarios.json` through `scripts/normalize-scenarios.sh` on every run — including a red one — and only then exit with the runner's own code.
  - Risk: under `set -e` a failing runner ends the script before the scenario report is written, so the report is missing or stale exactly on the red run it should describe.
  - Fix: wrap the runner in `set +e`/`set -e`, keep its exit code, normalize, then `exit` with it.
- `scripts/normalize-scenarios.sh` must stay byte-identical across the .NET, Python, and TypeScript variants of this solution, like `scripts/test-report.sh`.
  - Risk: a stack-local tweak to the inventory scan makes the same `.feature` file produce different entries per stack, and the report stops being comparable.
  - Fix: change it in all three variants together, or not at all.
- Declare `TEST_KINDS := unit mutation` and their badges before `include tools/testing/testing.mk`, and place that block after the first ordinary target so `make` without arguments keeps its default goal.
  - Risk: a declaration after the include is ignored; an include before every other target makes `test-kinds` the default goal.
  - Fix: keep the order in the Makefile template; `test-and-report` itself comes from `tools/testing/testing.mk`.
- Never add a caller-facing variable beyond `TEST_RUN_PURPOSE`/`DELTA_BASE`/`TEST_WORK_DIR`/`TEST_REPORT_DIR` — a caller must not need to know this is a TypeScript project.
  - Risk: every caller (CI workflow, developer, script) now needs TypeScript-specific knowledge to invoke the targets correctly, defeating the point of the uniform contract this `Makefile` implements.
  - Fix: keep the `make` interface limited to the toggles [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] defines; anything TypeScript-specific stays inside the `Makefile`/scripts.
- Never let `report-template/index.html` live under `.github/`.
  - Risk: nesting a project-owned static asset inside `.github/` implies this solution owns a workflow or publishing configuration it does not — the actual publishing step is a separate, layered CI concern this solution never owns.
  - Fix: keep it at `report-template/index.html`, copied by `test-report.sh` — never generated, never placed under `.github/`.
- Keep cucumber-js's `message` formatter writing to `$TEST_KIND_DIR/report/tests/cucumber/messages.ndjson` (not a temp file), and render the living doc with the base's `tools/livingdoc/` after the run, as `scripts/unit-test.sh` does — never install the renderer into the project's own `package.json`.
  - Risk: a temp file leaves nothing for the renderer; a devDependency renderer drifts from every other stack's pinned version.
  - Fix: follow the template's `CUCUMBER_MESSAGES` path and living-doc block.

# Unittest TestCases
- [ ] WHEN `make test-kind-unit` runs THEN `$TEST_KIND_DIR/result/unit-test.json` and `$TEST_KIND_DIR/report/tests/` exist.
- [ ] WHEN `make test-kind-unit TEST_RUN_PURPOSE=report` runs THEN `$TEST_KIND_DIR/result/coverage-test.json` and `$TEST_KIND_DIR/report/coverage/` also exist.
- [ ] WHEN `make test-kind-unit` runs and a scenario fails THEN `$TEST_KIND_DIR/result/scenarios.json` still lists every `.feature` entry, `@todo` ones with status `todo`, and the target exits non-zero.
- [ ] WHEN `make test-report` runs THEN `$TEST_REPORT_DIR/reports/scenarios/index.html` shows the type × status table and every entry.
- [ ] WHEN `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=<ref>` runs THEN only mutants in code changed since `<ref>` are evaluated.
- [ ] WHEN `make test-report` runs after both `*-test` targets THEN `$TEST_REPORT_DIR/` contains the badge JSON files and copies of the native reports.
- [ ] WHEN `make test-and-report` runs THEN it produces the same end state as running `test-kind-unit`, `test-kind-mutation`, and `test-report` in sequence by hand.
