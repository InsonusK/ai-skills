---
description: Add the Makefile and normalization scripts implementing the make unit-test/mutation-test/test-report/test-and-report contract, aggregated across whichever test projects exist
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
/scripts
  unit-test.sh
  normalize-scenarios.sh
  messages-results.jq
  mutation-test.sh
  test-report.sh
Makefile
README.md
```

Which test projects exist, and what each one references, is decided by the architecture solution that applies this one (e.g. [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]); every one of them must be part of the solution `dotnet test` runs.

## Directory and class skills
| Directory | file | Description |
| ----------------- | ---- | ----------- |
| /tests/{TestProject} | reqnroll.json | One per test project: Reqnroll's `html` formatter (native report) and `message` formatter (scenario report source), both written into that project's own `bin/` |
| /report-template | index.html | Static landing page `test-report.sh` copies into `public/`; links to `scenarios/`, `tests/`, `coverage/`, `mutation/`. Kept outside `.github/` since this solution never owns `.github/workflows/*` |
| /scripts | unit-test.sh | Runs `dotnet test` across every test project (`@todo` excluded), normalizes the aggregated result into `tmp/result/unit-test.json` and `tmp/result/scenarios.json` (+ `coverage-test.json` when `WITH_CODE_COVERAGE=true`), keeps the merged native report under `tmp/report/tests` (+ `tmp/report/coverage`) |
| /scripts | normalize-scenarios.sh | `.feature` inventory + per-scenario results → `tmp/result/scenarios.json`; identical across the .NET/Python/TypeScript variants |
| /scripts | messages-results.jq | Reqnroll's Cucumber Messages → `[{uri, line, status}]` for `normalize-scenarios.sh` |
| /scripts | mutation-test.sh | Runs `dotnet-stryker` against the whole solution (scoped to `DELTA_BASE` when `ONLY_DELTA=true`), normalizes results into `tmp/result/mutation-test.json`, keeps the native report under `tmp/report/mutation` |
| /scripts | test-report.sh | Assembles `public/` — `scenarios/` included — from `tmp/result/*.json` + `tmp/report/*`; no test/build tooling involved |
| / | Makefile | Exposes the `unit-test`/`mutation-test`/`test-report`/`test-and-report` targets required by [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] |

## Makefile
See [templates/Makefile.md](skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/templates/Makefile.md) for the full content.

## scripts/unit-test.sh
Runs `dotnet test` against the whole solution — which picks up every test project at once, `@todo` scenarios excluded — then merges their TRX counters, Reqnroll messages, and coverage files into one normalized result. See [templates/unit-test.sh.md](skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/templates/unit-test.sh.md) for the full script and the required `reqnroll.json` formatter config (one per test project).

## scripts/normalize-scenarios.sh, scripts/messages-results.jq
Build `tmp/result/scenarios.json` per [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-report|solution-conformance-testing's Scenario report]]. See [templates/normalize-scenarios.sh.md](skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/templates/normalize-scenarios.sh.md) and [templates/messages-results.jq.md](skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/templates/messages-results.jq.md).

## scripts/mutation-test.sh
Runs Stryker.NET against the whole solution — its native `--since` mode covers `ONLY_DELTA`/`DELTA_BASE` directly, so this script does not need to compute the diff itself, and Stryker's own solution-wide run already covers every test project together. See [templates/mutation-test.sh.md](skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/templates/mutation-test.sh.md) for the full script.

## scripts/test-report.sh
Pure assembly — no `dotnet`/test tooling involved, so this same script (unmodified) also works for the Python and TypeScript variants of this solution. See [templates/test-report.sh.md](skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/templates/test-report.sh.md) for the full script.

# Rules

## MUST
- `unit-test`, `mutation-test`, `test-report`, and `test-and-report` targets must exist and behave exactly as documented in [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] — this `Makefile` is the .NET implementation of that contract, not a variation of it.
  - Violation: a CI workflow or a developer runs `dotnet-stryker`/`dotnet test` directly instead of through `make mutation-test`/`make unit-test`.
  - Risk: the workflow now needs .NET-specific knowledge, and switching or reconfiguring Stryker.NET later becomes a breaking change for every CI file that calls it directly.
  - Fix: every caller (CI or a developer) goes through the `Makefile`; the project's own CI workflows call these targets stack-agnostically instead of the underlying tools directly.
- `scripts/unit-test.sh` must aggregate every test project's TRX counters and coverage files into one `tmp/result/unit-test.json`/`tmp/result/coverage-test.json` pair, not one per project.
  - Risk: without aggregation — or with a fixed trx `LogFileName` every project overwrites — `make unit-test` reports only the project that ran last, silently hiding the others.
  - Fix: log with `trx;LogFilePrefix=test-results`, sum counters across every `test-results*.trx`, and let ReportGenerator's glob pick up every project's `coverage.cobertura.xml`.
- `scripts/unit-test.sh` and `scripts/mutation-test.sh` must write their normalized JSON into `tmp/result/` and keep the native HTML report under `tmp/report/<kind>/`, per the same contract.
  - Risk: without the normalized JSON, `make test-report` and badge generation have nothing stack-independent to read.
  - Fix: write both outputs exactly as [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] specifies.
- Every test project's `reqnroll.json` must configure Reqnroll's `html` and `message` formatters to paths inside that project's own output folder — `scripts/unit-test.sh` merges them into one browsable report and one scenario report.
  - Risk: two test projects writing to the same formatter output path silently overwrite each other; a project without the `message` formatter shows all its scenarios as `missing`.
  - Fix: keep both formatters in every `reqnroll.json`, with project-relative output paths; merge them explicitly in the script.
- `scripts/unit-test.sh` must exclude `@todo` scenarios from the run, write `tmp/result/scenarios.json` through `scripts/normalize-scenarios.sh` on every run — including a red one — and only then exit with the runner's own code.
  - Risk: under `set -e` a failing runner ends the script before the scenario report is written, so the report is missing or stale exactly on the red run it should describe.
  - Fix: wrap the runner in `set +e`/`set -e`, keep its exit code, normalize, then `exit` with it.
- `scripts/normalize-scenarios.sh` must stay byte-identical across the .NET, Python, and TypeScript variants of this solution, like `scripts/test-report.sh`.
  - Risk: a stack-local tweak to the inventory scan makes the same `.feature` file produce different entries per stack, and the report stops being comparable.
  - Fix: change it in all three variants together, or not at all.
- `scripts/mutation-test.sh` must still exit with `dotnet-stryker`'s own exit code after writing `tmp/result/mutation-test.json` — normalizing the result must never swallow a real mutation-testing failure.
  - Risk: a real mutation-testing failure gets swallowed by the normalization step, and CI reports success on a run that actually found unkilled mutants.
  - Fix: propagate `dotnet-stryker`'s exit code from the script after it finishes writing the normalized result.
- `test-and-report` must run `unit-test` (with coverage), `mutation-test`, and `test-report`, in that order.
  - Risk: running them out of order, or omitting one, produces a report built from stale or missing results.
  - Fix: declare `test-and-report`'s prerequisites as `unit-test mutation-test test-report`, forwarding `WITH_CODE_COVERAGE`/`ONLY_DELTA`/`DELTA_BASE` to the targets that accept them.
- Never add stack-specific flags to the `make` targets themselves beyond `WITH_CODE_COVERAGE`/`ONLY_DELTA`/`DELTA_BASE` — a caller must not need to know this is a .NET project.
  - Risk: every caller (CI workflow, developer, script) now needs .NET-specific knowledge to invoke the targets correctly, defeating the point of the uniform contract this `Makefile` implements.
  - Fix: keep the `make` interface limited to the toggles [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] defines; anything .NET-specific stays inside the `Makefile`/scripts.

# Unittest TestCases
- [ ] WHEN `make unit-test` runs THEN `tmp/result/unit-test.json` reflects the sum of every test project's results, and `tmp/report/tests/` merges every project's native report.
- [ ] WHEN `make unit-test WITH_CODE_COVERAGE=true` runs THEN `tmp/result/coverage-test.json` and `tmp/report/coverage/` reflect coverage across every test project.
- [ ] WHEN `make unit-test` runs and a scenario fails THEN `tmp/result/scenarios.json` still lists every `.feature` entry, `@todo` ones with status `todo`, and the target exits non-zero.
- [ ] WHEN `make test-report` runs THEN `public/scenarios/index.html` shows the type × status table and every entry.
- [ ] WHEN `make mutation-test ONLY_DELTA=true DELTA_BASE=<ref>` runs THEN only mutants in code changed since `<ref>` are evaluated, across every test project.
- [ ] WHEN `make test-report` runs after both `*-test` targets THEN `public/` contains the badge JSON files and copies of the native reports.
- [ ] WHEN `make test-and-report` runs THEN it produces the same end state as running `unit-test`, `mutation-test`, and `test-report` in sequence by hand.
