---
version: 20261009220000
description: Add the .NET test kinds (unit, mutation), the report builder and their normalization scripts to the Makefile behind the shared testing contract, aggregated across whichever test projects exist
element_kind: repository
change_kind: extend
tags:
  - solution/conformance-testing-in-dotnet
  - element/repository-extend

---

# Structure

## Project Structure
```
/tests
  /{TestProject}               — one per test project, however the solution splits them
    — feature and binding layout: see cucumber-testing-in-dotnet
    reqnroll.json
/report-template
  index.html
stryker-config.json
Makefile                       — one line added: include tools/testing/testing.mk
/tools
  /testing                     — copied verbatim from solution-conformance-testing
    /kinds
      unit.sh                  — this stack's unit kind
      mutation.sh              — this stack's mutation kind
  /livingdoc                   — package.json, package-lock.json, render.mjs, copied verbatim
README.md
```

Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects); include every test project in the solution.

## Directory and class skills
| Directory | file | Description |
| ----------------- | ---- | ----------- |
| /tests/{TestProject} | reqnroll.json | One per test project: Reqnroll's `html` formatter (native report) and `message` formatter (scenario inventory source), both written into that project's own `bin/` |
| /report-template | index.html | Landing page `tools/testing/test-report.sh` publishes into `$TEST_REPORT_DIR/` with its `<!-- test-reports -->` line replaced by one item per report — badge, then link; shows `run.json`. Kept outside `.github/` since this solution never owns `.github/workflows/*` |
| / | stryker-config.json | `solution` + `test-case-filter: Category!=status/todo&Category!=status/broken`, so Stryker.NET's own test runs skip `@status/todo` and `@status/broken` scenarios |

## stryker-config.json
```json
{
  "stryker-config": {
    "solution": "{Solution}.slnx",
    "test-case-filter": "Category!=status/todo&Category!=status/broken"
  }
}
```

## Kind scripts
The only stack-specific code: each runs this stack's tool and writes its `result/` data, its `report/{name}/` and its `badges/{name}.json` — the Makefile, the runner and the report builder are the base's `tools/testing/`.
- Copy verbatim to every test project as `reqnroll.json`: [`assets/reqnroll.json`](../assets/reqnroll.json)
- Copy verbatim to `tools/testing/kinds/unit.sh`: [`assets/tools/testing/kinds/unit.sh`](../assets/tools/testing/kinds/unit.sh)
- Copy verbatim to `tools/testing/kinds/mutation.sh`: [`assets/tools/testing/kinds/mutation.sh`](../assets/tools/testing/kinds/mutation.sh)

## .gitignore
Beside the base's `tmp/` and `tools/livingdoc/node_modules/`: `*.feature.cs` (Reqnroll's generated code-behind), `TestResults/` (a test run started from an IDE), `StrykerOutput/` (a `dotnet stryker` started by hand).

# Rules

## MUST
- `test-kind-unit`, `test-kind-mutation`, `test-report`, and `test-and-report` targets must exist and behave exactly as documented in [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] — this `Makefile` is the .NET implementation of that contract, not a variation of it.
  - Violation: a CI workflow or a developer runs `dotnet-stryker`/`dotnet test` directly instead of through `make test-kind-mutation`/`make test-kind-unit`.
  - Risk: the workflow now needs .NET-specific knowledge, and switching or reconfiguring Stryker.NET later becomes a breaking change for every CI file that calls it directly.
  - Fix: every caller (CI or a developer) goes through the `Makefile`; the project's own CI workflows call these targets stack-agnostically instead of the underlying tools directly.
- `tools/testing/kinds/unit.sh` must aggregate every test project's TRX counters and coverage files into one `$TEST_KIND_DIR/result/unit-test.json`/`$TEST_KIND_DIR/result/coverage-test.json` pair, not one per project.
  - Risk: without aggregation — or with a fixed trx `LogFileName` every project overwrites — `make test-kind-unit` reports only the project that ran last, silently hiding the others.
  - Fix: log with `trx;LogFilePrefix=test-results`, sum counters across every `test-results*.trx`, and let ReportGenerator's glob pick up every project's `coverage.cobertura.xml`.
- `tools/testing/kinds/unit.sh` and `tools/testing/kinds/mutation.sh` must write each report into `$TEST_KIND_DIR/report/{name}/` and its badge through `kind_badge_count` / `kind_badge_percent`, per the same contract.
  - Risk: a badge printed by hand drifts from the other stacks' in schema and colors; a report outside `report/{name}/` is not published.
  - Fix: keep the `kind_badge_*`, `kind_scenarios_check` and `kind_livingdoc` calls the scripts carry.
- Every test project's `reqnroll.json` must configure Reqnroll's `html` and `message` formatters to paths inside that project's own output folder — `tools/testing/kinds/unit.sh` merges them into one browsable report and one scenario inventory.
  - Risk: two test projects writing to the same formatter output path silently overwrite each other; a project without the `message` formatter shows all its scenarios as `not-run`.
  - Fix: keep both formatters in every `reqnroll.json`, with project-relative output paths; merge them explicitly in the script.
- `tools/testing/kinds/unit.sh` must exclude `@status/todo` and `@status/broken` scenarios from the run, write `$TEST_KIND_DIR/result/scenarios.json` through `tools/testing/normalize-scenarios.sh` on every run — including a red one — and only then exit with the runner's own code.
  - Risk: under `set -e` a failing runner ends the script before the scenario inventory is written, so the report is missing or stale exactly on the red run it should describe.
  - Fix: wrap the runner in `set +e`/`set -e`, keep its exit code, normalize, then `exit` with it.
- `stryker-config.json` must set `test-case-filter` to `Category!=status/todo&Category!=status/broken`.
  - Risk: Stryker.NET runs its own test pass, ignoring `dotnet test`'s filter; a `@status/todo` scenario with an undefined step fails the initial run and aborts every mutation run.
  - Fix: keep the filter in `stryker-config.json`, which Stryker.NET reads from the repository root.
- Test projects must reference xUnit v2 (`xunit`, `xunit.runner.visualstudio` 3.x, `Reqnroll.xUnit`) on the VSTest runner — never `xunit.v3`/`Reqnroll.xunit.v3` or a Microsoft.Testing.Platform opt-in — until `recheck/stryker-xunit-v3.sh` reports `SUPPORTED`.
  - Risk: Stryker.NET 4.16 reports every mutant as survived on xunit.v3/MTP — a silent, false 0% mutation score.
  - Fix: see [[skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/adr/xunit-v2-until-stryker-supports-xunit-v3|the ADR]]; re-run the recheck script on each Stryker.NET release.
- `tools/testing/kinds/mutation.sh` must still exit with `dotnet-stryker`'s own exit code after writing `$TEST_KIND_DIR/result/mutation-test.json` — normalizing the result must never swallow a real mutation-testing failure.
  - Risk: a real mutation-testing failure gets swallowed by the normalization step, and CI reports success on a run that actually found unkilled mutants.
  - Fix: propagate `dotnet-stryker`'s exit code from the script after it finishes writing the normalized result.
- Add only `include tools/testing/testing.mk` to the `Makefile`, after its first target; never a testing recipe.
  - Risk: a recipe in the project's `Makefile` duplicates a kind script and drifts from it; an include placed first makes `test-kinds` the default goal.
  - Fix: append the include line; everything a kind does lives in `tools/testing/kinds/{kind}.sh`.
- `tools/testing/kinds/mutation.sh` must scope a `check` run with one `--mutate` pattern per changed production file (`git diff --relative --name-only {commit} -- '*.cs'`, each path made relative to its project, files of `*Tests.csproj` projects left out) — never with Stryker's `--since`.
  - Violation: `dotnet-stryker --since:HEAD~1`.
  - Risk: `--since` accepts a branch, a tag or a full commit id only, and with Reqnroll-generated tests Stryker.NET 4.16.0 logs every changed file as "Changed test file" and ignores its mutants ("Removed by since filter") — the run is green and tests nothing.
  - Fix: keep the loop in `assets/tools/testing/kinds/mutation.sh`; it resolves `DELTA_BASE` with `git rev-parse` and skips the kind when no production file changed.
- Never add a caller-facing variable beyond `TEST_RUN_PURPOSE`/`DELTA_BASE`/`TEST_WORK_DIR`/`TEST_REPORT_DIR` — a caller must not need to know this is a .NET project.
  - Risk: every caller (CI workflow, developer, script) now needs .NET-specific knowledge to invoke the targets correctly, defeating the point of the uniform contract this `Makefile` implements.
  - Fix: keep the `make` interface limited to the variables [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] defines; anything .NET-specific stays inside the `Makefile`/scripts.

# Unittest TestCases
- [ ] WHEN `make test-kind-unit` runs THEN `$TEST_KIND_DIR/result/unit-test.json` reflects the sum of every test project's results, and `$TEST_KIND_DIR/report/tests/` merges every project's native report.
- [ ] WHEN `make test-kind-unit` runs in a `report` run THEN `$TEST_KIND_DIR/result/coverage-test.json` and `$TEST_KIND_DIR/report/coverage/` reflect coverage across every test project.
- [ ] WHEN `make test-kind-unit` runs and a scenario fails THEN `$TEST_KIND_DIR/result/scenarios.json` still lists every `.feature` entry, `@status/todo` ones with status `todo`, and the target exits non-zero.
- [ ] WHEN `make test-report` runs THEN the `tests` link opens the living doc, which shows every scenario, all its tags, exclusion reasons and the status legend; no separate scenarios page exists.
- [ ] WHEN `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=<ref>` runs THEN only mutants in production `.cs` files changed since `<ref>` (any commit expression — `HEAD~1`, `origin/main`) are evaluated, and the kind skips itself when none changed.
- [ ] WHEN `make test-report` runs after both kinds THEN `$TEST_REPORT_DIR/` contains `index.html`, `badges/{tests,coverage,mutation}.json`, `reports/{tests,coverage,mutation}/`, and `run.json`.
- [ ] WHEN `make test-kind-unit` runs with `npm` available THEN `report/tests/cucumber/*.ndjson` and `report/tests/livingdoc/index.html` exist; without `npm` the kind's exit code is unchanged.
- [ ] WHEN `make test-kind-mutation TEST_RUN_PURPOSE=check` runs without `DELTA_BASE` THEN the kind skips itself, leaving `skipped` and no badge.
- [ ] WHEN `make test-readme-check` runs THEN it passes with one README badge per declared badge.
- [ ] WHEN `make test-and-report` runs THEN it produces the same end state as running every `test-kind-{kind}` and then `test-report` by hand.
