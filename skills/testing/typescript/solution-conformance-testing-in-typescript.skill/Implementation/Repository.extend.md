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
Makefile                       — one line added: include tools/testing/testing.mk
README.md                      — one badge per declared badge
/tools
  /testing                     — copied verbatim from solution-conformance-testing
    /kinds
      unit.sh                  — this stack's unit kind
      mutation.sh              — this stack's mutation kind
  /livingdoc                   — package.json, package-lock.json, render.mjs, copied verbatim
package.json
cucumber.mjs
stryker.conf.json
README.md
```

## Directory and class skills
| Directory | file | Description |
| ----------------- | ----------- |
| /report-template | index.html | Landing page `tools/testing/test-report.sh` publishes into `$TEST_REPORT_DIR/` with its `<!-- test-reports -->` line replaced by one item per report — badge, then link; shows `run.json`. Kept outside `.github/` since this solution never owns `.github/workflows/*` |
| /tools/livingdoc | package.json, package-lock.json, render.mjs | Copied verbatim from `solution-conformance-testing`; `tools/testing/kinds/unit.sh` renders `$TEST_KIND_DIR/report/tests/cucumber/messages.ndjson` → `$TEST_KIND_DIR/report/tests/livingdoc/` |
| / | stryker.conf.json | Base Stryker config; `tools/testing/kinds/mutation.sh` runs a patched copy — `reporters`, the report file paths, `tempDirName` and `ignorePatterns`, and in a `report` run `thresholds.break = 0` — and never edits the file in place |

## Kind scripts
The only stack-specific code: each runs this stack's tool and writes its `result/` data, its `report/{name}/` and its `badges/{name}.json` — the Makefile, the runner and the report builder are the base's `tools/testing/`.
- Copy verbatim to `tools/testing/kinds/unit.sh`: [`assets/tools/testing/kinds/unit.sh`](../assets/tools/testing/kinds/unit.sh)
- Copy verbatim to `tools/testing/kinds/mutation.sh`: [`assets/tools/testing/kinds/mutation.sh`](../assets/tools/testing/kinds/mutation.sh)

## .gitignore
Beside the base's `tmp/` and `tools/livingdoc/node_modules/`: `node_modules/`, and `.stryker-tmp/` and `reports/` (a `stryker run` started by hand).

# Rules

## MUST
- `test-kind-unit`, `test-kind-mutation`, `test-report`, and `test-and-report` targets must exist and behave exactly as documented in [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] — this `Makefile` is the TypeScript implementation of that contract, not a variation of it.
  - Violation: a CI workflow or a developer runs `stryker run`/`cucumber-js` directly instead of through `make test-kind-mutation`/`make test-kind-unit`.
  - Risk: the workflow now needs TypeScript-specific knowledge, and switching or reconfiguring Stryker later becomes a breaking change for every CI file that calls it directly.
  - Fix: every caller (CI or a developer) goes through the `Makefile`; the project's own CI workflows call these targets stack-agnostically instead of the underlying tools directly.
- `tools/testing/kinds/unit.sh` and `tools/testing/kinds/mutation.sh` must write each report into `$TEST_KIND_DIR/report/{name}/` and its badge through `kind_badge_count` / `kind_badge_percent`, per the same contract.
  - Risk: a badge printed by hand drifts from the other stacks' in schema and colors; a report outside `report/{name}/` is not published.
  - Fix: keep the `kind_badge_*` and `kind_scenarios_report` calls the scripts carry.
- `stryker.conf.json` must exist at the path `tools/testing/kinds/mutation.sh` reads (repository root for a single-package repository) — the script patches a copy of it per run, it never creates one from scratch.
  - Risk: without the base config file present, the script has nothing to patch and `make test-kind-mutation` fails outright.
  - Fix: commit a base `stryker.conf.json` at the path the script expects.
- `tools/testing/kinds/mutation.sh` must still exit with `stryker run`'s own exit code after writing `$TEST_KIND_DIR/result/mutation-test.json` — normalizing the result must never swallow a real mutation-testing failure.
  - Risk: a real mutation-testing failure gets swallowed by the normalization step, and CI reports success on a run that actually found unkilled mutants.
  - Fix: propagate `stryker run`'s exit code from the script after it finishes writing the normalized result.
- `tools/testing/kinds/unit.sh` must exclude `@status/todo` scenarios from the run, write `$TEST_KIND_DIR/result/scenarios.json` through `tools/testing/normalize-scenarios.sh` on every run — including a red one — and only then exit with the runner's own code.
  - Risk: under `set -e` a failing runner ends the script before the scenario report is written, so the report is missing or stale exactly on the red run it should describe.
  - Fix: wrap the runner in `set +e`/`set -e`, keep its exit code, normalize, then `exit` with it.
- Keep Stryker's sandbox (`tempDirName`) and `c8`'s temp directory below `$TEST_KIND_DIR`, and the work directory in Stryker's `ignorePatterns`, as the kind scripts set them.
  - Violation: Stryker's default `.stryker-tmp/` in the repository root.
  - Risk: a mutation run that stops early leaves a sandbox holding copies of the `.feature` files; the unit kind then lists every scenario twice, the copy as `not-run`. Raw V8 coverage files under `report/coverage/tmp/` get published with the report.
  - Fix: copy both scripts unchanged.
- Scope a `check` run of `mutation.sh` with `--mutate` over `git diff --relative --name-only {commit} -- ':(glob)src/**/*.ts'`, and skip the kind when the list is empty.
  - Violation: the pathspec `'src/**/*.ts'` without `:(glob)`.
  - Risk: without the magic word git's `**` needs a directory level, so a changed `src/{file}.ts` is not listed and the kind skips itself over a real change.
  - Fix: keep the pathspec and the `git rev-parse` of `DELTA_BASE` as `mutation.sh` has them.
- Add only `include tools/testing/testing.mk` to the `Makefile`, after its first target; never a testing recipe.
  - Risk: a recipe in the project's `Makefile` duplicates a kind script and drifts from it; an include placed first makes `test-kinds` the default goal.
  - Fix: append the include line; everything a kind does lives in `tools/testing/kinds/{kind}.sh`.
- Never add a caller-facing variable beyond `TEST_RUN_PURPOSE`/`DELTA_BASE`/`TEST_WORK_DIR`/`TEST_REPORT_DIR` — a caller must not need to know this is a TypeScript project.
  - Risk: every caller (CI workflow, developer, script) now needs TypeScript-specific knowledge to invoke the targets correctly, defeating the point of the uniform contract this `Makefile` implements.
  - Fix: keep the `make` interface limited to the toggles [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] defines; anything TypeScript-specific stays inside the `Makefile`/scripts.
- Never let `report-template/index.html` live under `.github/`.
  - Risk: nesting a project-owned static asset inside `.github/` implies this solution owns a workflow or publishing configuration it does not — the actual publishing step is a separate, layered CI concern this solution never owns.
  - Fix: keep it at `report-template/index.html`, copied by `tools/testing/test-report.sh` — never generated, never placed under `.github/`.
- Keep cucumber-js's `message` formatter writing to `$TEST_KIND_DIR/report/tests/cucumber/messages.ndjson` (not a temp file), and render the living doc with the base's `tools/livingdoc/` after the run, as `tools/testing/kinds/unit.sh` does — never install the renderer into the project's own `package.json`.
  - Risk: a temp file leaves nothing for the renderer; a devDependency renderer drifts from every other stack's pinned version.
  - Fix: follow the template's `CUCUMBER_MESSAGES` path and living-doc block.

# Unittest TestCases
- [ ] WHEN `make test-kind-unit` runs THEN `$TEST_KIND_DIR/result/unit-test.json` and `$TEST_KIND_DIR/report/tests/` exist.
- [ ] WHEN `make test-kind-unit TEST_RUN_PURPOSE=report` runs THEN `$TEST_KIND_DIR/result/coverage-test.json` and `$TEST_KIND_DIR/report/coverage/` also exist.
- [ ] WHEN `make test-kind-unit` runs and a scenario fails THEN `$TEST_KIND_DIR/result/scenarios.json` still lists every `.feature` entry, `@status/todo` ones with status `todo`, and the target exits non-zero.
- [ ] WHEN `make test-report` runs THEN `$TEST_REPORT_DIR/reports/scenarios/index.html` shows the category × status table and every entry.
- [ ] WHEN `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=<ref>` runs THEN only mutants in the `src/**/*.ts` files changed since `<ref>` are evaluated — a file directly in `src/` included — with the project's `thresholds.break` in force; with none changed the kind skips itself.
- [ ] WHEN a kind ends THEN the repository root holds no `.stryker-tmp/` and `$TEST_KIND_DIR/report/coverage/` no `tmp/`.
- [ ] WHEN `make test-report` runs after both `*-test` targets THEN `$TEST_REPORT_DIR/` contains the badge JSON files and copies of the native reports.
- [ ] WHEN `make test-and-report` runs THEN it produces the same end state as running `test-kind-unit`, `test-kind-mutation`, and `test-report` in sequence by hand.
