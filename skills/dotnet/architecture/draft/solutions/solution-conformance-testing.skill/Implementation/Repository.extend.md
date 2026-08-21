---
description: Add the Makefile and normalization scripts implementing the make unit-test/mutation-test/test-report/test-and-report contract, aggregated across all five test projects
element_kind: repository
change_kind: extend
---

# Structure

## Project Structure
```
/{Module}
  /{Module}.Domain.Tests
    Rules/{Rule}.feature
    StepDefinitions/{Rule}Steps.cs
    reqnroll.json
  /{Module}.Application.Tests
    Rules/{Rule}.feature
    StepDefinitions/{Rule}Steps.cs
    reqnroll.json
  /{Module}.Interfaces.Tests
    Rules/{Rule}.feature
    StepDefinitions/{Rule}Steps.cs
    reqnroll.json
/Shared.Tests
  Rules/{Rule}.feature
  StepDefinitions/{Rule}Steps.cs
  reqnroll.json
/BuildingBlocks.Tests
  Rules/{Rule}.feature
  StepDefinitions/{Rule}Steps.cs
  reqnroll.json
/report-template
  index.html
/scripts
  unit-test.sh
  mutation-test.sh
  test-report.sh
Makefile
README.md
```

## Directory and class skills
| Directory | file | Description |
| ----------------- | ----------- |
| /{Module}.Domain.Tests, /{Module}.Application.Tests, /{Module}.Interfaces.Tests, /Shared.Tests, /BuildingBlocks.Tests | reqnroll.json | One per test project, configuring Reqnroll's html formatter output path; `unit-test.sh` merges all five into one report |
| /report-template | index.html | Static landing page `test-report.sh` copies into `public/`; links to `tests/`, `coverage/`, `mutation/`. Kept outside `.github/` since this solution never owns `.github/workflows/*` |
| /scripts | unit-test.sh | Runs `dotnet test` across every test project in the solution, normalizes the aggregated result into `tmp/result/unit-test.json` (+ `coverage-test.json` when `WITH_CODE_COVERAGE=true`), keeps the merged native report under `tmp/report/tests` (+ `tmp/report/coverage`) |
| /scripts | mutation-test.sh | Runs `dotnet-stryker` against the whole solution (scoped to `DELTA_BASE` when `ONLY_DELTA=true`), normalizes results into `tmp/result/mutation-test.json`, keeps the native report under `tmp/report/mutation` |
| /scripts | test-report.sh | Assembles `public/` from `tmp/result/*.json` + `tmp/report/*` — no test/build tooling involved |
| / | Makefile | Exposes the `unit-test`/`mutation-test`/`test-report`/`test-and-report` targets required by [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] |

## Makefile
See [templates/Makefile.md](../templates/Makefile.md) for the full content.

## scripts/unit-test.sh
Runs `dotnet test` against the whole solution — which picks up all five test projects at once — then merges their TRX counters and coverage files into one normalized result. See [templates/unit-test.sh.md](../templates/unit-test.sh.md) for the full script and the required `reqnroll.json` formatter config (one per test project).

## scripts/mutation-test.sh
Runs Stryker.NET against the whole solution — its native `--since` mode covers `ONLY_DELTA`/`DELTA_BASE` directly, so this script does not need to compute the diff itself, and Stryker's own solution-wide run already covers all five test projects together. See [templates/mutation-test.sh.md](../templates/mutation-test.sh.md) for the full script.

## scripts/test-report.sh
Pure assembly — no `dotnet`/test tooling involved, so this same script (unmodified) also works for the Python and TypeScript variants of this solution. See [templates/test-report.sh.md](../templates/test-report.sh.md) for the full script.

# Rules

## MUST
- `unit-test`, `mutation-test`, `test-report`, and `test-and-report` targets must exist and behave exactly as documented in [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] — this `Makefile` is the .NET implementation of that contract, not a variation of it.
  - Violation: a CI workflow or a developer runs `dotnet-stryker`/`dotnet test` directly instead of through `make mutation-test`/`make unit-test`.
  - Risk: the workflow now needs .NET-specific knowledge, and switching or reconfiguring Stryker.NET later becomes a breaking change for every CI file that calls it directly.
  - Fix: every caller (CI or a developer) goes through the `Makefile`; the CI workflow itself is defined once, stack-agnostically, in [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md).
- `scripts/unit-test.sh` must aggregate all five test projects' TRX counters and coverage files into one `tmp/result/unit-test.json`/`tmp/result/coverage-test.json` pair, not one per project.
  - Risk: without aggregation, `make unit-test` reports only one project's numbers (whichever ran last), silently hiding the other four.
  - Fix: sum counters across every `*.trx` file `dotnet test` produced, and let ReportGenerator's glob pick up every project's `coverage.cobertura.xml`.
- `scripts/unit-test.sh` and `scripts/mutation-test.sh` must write their normalized JSON into `tmp/result/` and keep the native HTML report under `tmp/report/<kind>/`, per the same contract.
  - Risk: without the normalized JSON, `make test-report` and badge generation have nothing stack-independent to read.
  - Fix: write both outputs exactly as [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] specifies.
- Every test project's `reqnroll.json` must configure Reqnroll's html formatter to a distinct path under that project's own output folder — `scripts/unit-test.sh` merges all five into one browsable report.
  - Risk: two test projects writing to the same formatter output path silently overwrite each other, losing one project's scenario report.
  - Fix: give each `reqnroll.json` a project-specific output path; merge them explicitly in the script.
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
- [ ] WHEN `make unit-test` runs THEN `tmp/result/unit-test.json` reflects the sum of all five test projects' results, and `tmp/report/tests/` merges all five native reports.
- [ ] WHEN `make unit-test WITH_CODE_COVERAGE=true` runs THEN `tmp/result/coverage-test.json` and `tmp/report/coverage/` reflect coverage across all five projects.
- [ ] WHEN `make mutation-test ONLY_DELTA=true DELTA_BASE=<ref>` runs THEN only mutants in code changed since `<ref>` are evaluated, across every test project.
- [ ] WHEN `make test-report` runs after both `*-test` targets THEN `public/` contains the badge JSON files and copies of the native reports.
- [ ] WHEN `make test-and-report` runs THEN it produces the same end state as running `unit-test`, `mutation-test`, and `test-report` in sequence by hand.
