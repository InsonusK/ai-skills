---
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
    {Rule}.feature, StepDefinitions/{Rule}Steps.cs
    reqnroll.json
/report-template
  index.html
stryker-config.json
/scripts
  unit-test.sh
  normalize-scenarios.sh
  messages-results.jq
  mutation-test.sh
  test-report.sh
Makefile                       — extended; created when missing
/tools
  /testing                     — testing.mk, testing.sh, copied verbatim from solution-conformance-testing
  /livingdoc                   — package.json, package-lock.json, render.mjs, copied verbatim
README.md
```

Which test projects exist, and what each one references, is decided by the architecture solution that applies this one (e.g. `solution-dotnet-conformance-testing`); every one of them must be part of the solution `dotnet test` runs.

## Directory and class skills
| Directory | file | Description |
| ----------------- | ---- | ----------- |
| /tests/{TestProject} | reqnroll.json | One per test project: Reqnroll's `html` formatter (native report) and `message` formatter (scenario report source), both written into that project's own `bin/` |
| /report-template | index.html | Static landing page `test-report.sh` copies into `$TEST_REPORT_DIR/`; links to `reports/scenarios/`, `reports/tests/`, `reports/tests/livingdoc/`, `reports/coverage/`, `reports/mutation/`, and shows `run.json`. Kept outside `.github/` since this solution never owns `.github/workflows/*` |
| /scripts | unit-test.sh | Runs `dotnet test` across every test project (`@todo` excluded), normalizes the aggregated result into `$TEST_KIND_DIR/result/unit-test.json` and `$TEST_KIND_DIR/result/scenarios.json` (+ `coverage-test.json` in a `report` run), copies every project's Cucumber Messages file to `report/tests/cucumber/` and renders `report/tests/livingdoc/` with `tools/livingdoc/`, keeps the merged native report under `$TEST_KIND_DIR/report/tests` (+ `$TEST_KIND_DIR/report/coverage`) |
| /scripts | normalize-scenarios.sh | `.feature` inventory + per-scenario results → `$TEST_KIND_DIR/result/scenarios.json`; identical across the .NET/Python/TypeScript variants |
| /scripts | messages-results.jq | Reqnroll's Cucumber Messages → `[{uri, line, status}]` for `normalize-scenarios.sh` |
| / | stryker-config.json | `solution` + `test-case-filter: Category!=todo`, so Stryker.NET's own test runs skip `@todo` scenarios |
| /scripts | mutation-test.sh | Runs `dotnet-stryker` against the whole solution (in a `check` run scoped to code changed since `DELTA_BASE`), normalizes results into `$TEST_KIND_DIR/result/mutation-test.json`, keeps the native report under `$TEST_KIND_DIR/report/mutation` |
| /scripts | test-report.sh | Builds `$TEST_REPORT_DIR/` — `index.html`, `reports/`, `badges/` — from every kind's `result/*.json` + `report/*/`; no test/build tooling involved; byte-identical across the .NET/Python/TypeScript variants |
| / | Makefile | Declares the `unit` and `mutation` kinds, includes `tools/testing/testing.mk`, and defines `test-kind-unit`/`test-kind-mutation`/`test-report-build` as required by [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] |

## Makefile
See [templates/Makefile.md](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/templates/Makefile.md) for the full content.

## scripts/unit-test.sh
Runs `dotnet test` against the whole solution — which picks up every test project at once, `@todo` scenarios excluded — then merges their TRX counters, Reqnroll messages, and coverage files into one normalized result. See [templates/unit-test.sh.md](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/templates/unit-test.sh.md) for the full script and the required `reqnroll.json` formatter config (one per test project).

## scripts/normalize-scenarios.sh, scripts/messages-results.jq
Build `$TEST_KIND_DIR/result/scenarios.json` per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-report|solution-conformance-testing's Scenario report]]. See [templates/normalize-scenarios.sh.md](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/templates/normalize-scenarios.sh.md) and [templates/messages-results.jq.md](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/templates/messages-results.jq.md).

## scripts/mutation-test.sh
Runs Stryker.NET against the whole solution — its native `--since` mode covers a `check` run's `DELTA_BASE` directly, so this script does not need to compute the diff itself, and Stryker's own solution-wide run already covers every test project together. See [templates/mutation-test.sh.md](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/templates/mutation-test.sh.md) for the full script.

## stryker-config.json
```json
{
  "stryker-config": {
    "solution": "{Solution}.slnx",
    "test-case-filter": "Category!=todo"
  }
}
```

## scripts/test-report.sh
Pure assembly — no `dotnet`/test tooling involved, so this same script (unmodified) also works for the Python and TypeScript variants of this solution. See [templates/test-report.sh.md](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/templates/test-report.sh.md) for the full script.

# Rules

## MUST
- `test-kind-unit`, `test-kind-mutation`, `test-report`, and `test-and-report` targets must exist and behave exactly as documented in [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] — this `Makefile` is the .NET implementation of that contract, not a variation of it.
  - Violation: a CI workflow or a developer runs `dotnet-stryker`/`dotnet test` directly instead of through `make test-kind-mutation`/`make test-kind-unit`.
  - Risk: the workflow now needs .NET-specific knowledge, and switching or reconfiguring Stryker.NET later becomes a breaking change for every CI file that calls it directly.
  - Fix: every caller (CI or a developer) goes through the `Makefile`; the project's own CI workflows call these targets stack-agnostically instead of the underlying tools directly.
- `scripts/unit-test.sh` must aggregate every test project's TRX counters and coverage files into one `$TEST_KIND_DIR/result/unit-test.json`/`$TEST_KIND_DIR/result/coverage-test.json` pair, not one per project.
  - Risk: without aggregation — or with a fixed trx `LogFileName` every project overwrites — `make test-kind-unit` reports only the project that ran last, silently hiding the others.
  - Fix: log with `trx;LogFilePrefix=test-results`, sum counters across every `test-results*.trx`, and let ReportGenerator's glob pick up every project's `coverage.cobertura.xml`.
- `scripts/unit-test.sh` and `scripts/mutation-test.sh` must write their normalized JSON into `$TEST_KIND_DIR/result/` and keep the native HTML report under `$TEST_KIND_DIR/report/<kind>/`, per the same contract.
  - Risk: without the normalized JSON, `make test-report` and badge generation have nothing stack-independent to read.
  - Fix: write both outputs exactly as [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] specifies.
- Every test project's `reqnroll.json` must configure Reqnroll's `html` and `message` formatters to paths inside that project's own output folder — `scripts/unit-test.sh` merges them into one browsable report and one scenario report.
  - Risk: two test projects writing to the same formatter output path silently overwrite each other; a project without the `message` formatter shows all its scenarios as `missing`.
  - Fix: keep both formatters in every `reqnroll.json`, with project-relative output paths; merge them explicitly in the script.
- `scripts/unit-test.sh` must exclude `@todo` scenarios from the run, write `$TEST_KIND_DIR/result/scenarios.json` through `scripts/normalize-scenarios.sh` on every run — including a red one — and only then exit with the runner's own code.
  - Risk: under `set -e` a failing runner ends the script before the scenario report is written, so the report is missing or stale exactly on the red run it should describe.
  - Fix: wrap the runner in `set +e`/`set -e`, keep its exit code, normalize, then `exit` with it.
- `scripts/normalize-scenarios.sh` must stay byte-identical across the .NET, Python, and TypeScript variants of this solution, like `scripts/test-report.sh`.
  - Risk: a stack-local tweak to the inventory scan makes the same `.feature` file produce different entries per stack, and the report stops being comparable.
  - Fix: change it in all three variants together, or not at all.
- `stryker-config.json` must set `test-case-filter` to `Category!=todo`.
  - Risk: Stryker.NET runs its own test pass, ignoring `dotnet test`'s filter; a `@todo` scenario with an undefined step fails the initial run and aborts every mutation run.
  - Fix: keep the filter in `stryker-config.json`, which Stryker.NET reads from the repository root.
- Test projects must reference xUnit v2 (`xunit`, `xunit.runner.visualstudio` 3.x, `Reqnroll.xUnit`) on the VSTest runner — never `xunit.v3`/`Reqnroll.xunit.v3` or a Microsoft.Testing.Platform opt-in — until `recheck/stryker-xunit-v3.sh` reports `SUPPORTED`.
  - Risk: Stryker.NET 4.16 reports every mutant as survived on xunit.v3/MTP — a silent, false 0% mutation score.
  - Fix: see [[skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/adr/xunit-v2-until-stryker-supports-xunit-v3|the ADR]]; re-run the recheck script on each Stryker.NET release.
- `scripts/mutation-test.sh` must still exit with `dotnet-stryker`'s own exit code after writing `$TEST_KIND_DIR/result/mutation-test.json` — normalizing the result must never swallow a real mutation-testing failure.
  - Risk: a real mutation-testing failure gets swallowed by the normalization step, and CI reports success on a run that actually found unkilled mutants.
  - Fix: propagate `dotnet-stryker`'s exit code from the script after it finishes writing the normalized result.
- Declare `TEST_KINDS := unit mutation` and their badges before `include tools/testing/testing.mk`, and place that block after the first ordinary target so `make` without arguments keeps its default goal.
  - Risk: a declaration after the include is ignored; an include before every other target makes `test-kinds` the default goal.
  - Fix: keep the order in [templates/Makefile.md](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/templates/Makefile.md); `test-and-report` itself comes from `tools/testing/testing.mk`.
- Never add a caller-facing variable beyond `TEST_RUN_PURPOSE`/`DELTA_BASE`/`TEST_WORK_DIR`/`TEST_REPORT_DIR` — a caller must not need to know this is a .NET project.
  - Risk: every caller (CI workflow, developer, script) now needs .NET-specific knowledge to invoke the targets correctly, defeating the point of the uniform contract this `Makefile` implements.
  - Fix: keep the `make` interface limited to the variables [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] defines; anything .NET-specific stays inside the `Makefile`/scripts.

# Unittest TestCases
- [ ] WHEN `make test-kind-unit` runs THEN `$TEST_KIND_DIR/result/unit-test.json` reflects the sum of every test project's results, and `$TEST_KIND_DIR/report/tests/` merges every project's native report.
- [ ] WHEN `make test-kind-unit` runs in a `report` run THEN `$TEST_KIND_DIR/result/coverage-test.json` and `$TEST_KIND_DIR/report/coverage/` reflect coverage across every test project.
- [ ] WHEN `make test-kind-unit` runs and a scenario fails THEN `$TEST_KIND_DIR/result/scenarios.json` still lists every `.feature` entry, `@todo` ones with status `todo`, and the target exits non-zero.
- [ ] WHEN `make test-report` runs THEN `$TEST_REPORT_DIR/reports/scenarios/index.html` shows the type × status table and every entry.
- [ ] WHEN `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=<ref>` runs THEN only mutants in code changed since `<ref>` are evaluated, across every test project.
- [ ] WHEN `make test-report` runs after both kinds THEN `$TEST_REPORT_DIR/` contains `index.html`, `badges/{tests,coverage,mutation}.json`, `reports/{tests,coverage,mutation,scenarios}/`, and `run.json`.
- [ ] WHEN `make test-kind-unit` runs with `npm` available THEN `report/tests/cucumber/*.ndjson` and `report/tests/livingdoc/index.html` exist; without `npm` the kind's exit code is unchanged.
- [ ] WHEN `make test-kind-mutation TEST_RUN_PURPOSE=check` runs without `DELTA_BASE` THEN the kind skips itself, leaving `skipped` and no badge.
- [ ] WHEN `make test-readme-check` runs THEN it passes with one README badge per declared badge.
- [ ] WHEN `make test-and-report` runs THEN it produces the same end state as running every `test-kind-{kind}` and then `test-report` by hand.
