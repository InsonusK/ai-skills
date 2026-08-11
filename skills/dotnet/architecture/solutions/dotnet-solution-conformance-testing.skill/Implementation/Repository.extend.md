---
description: Add the Makefile and normalization scripts implementing the make cucumber-test/mutation-test/result-page contract
element_kind: repository
change_kind: extend
---

# Structure

## Project Structure
```
/{Module}
  /{Module}.Tests
    Rules/{Rule}.feature
    StepDefinitions/{Rule}Steps.cs
    reqnroll.json
/.github
  /pages
    index.html
/scripts
  cucumber-test.sh
  mutation-test.sh
  result-page.sh
Makefile
README.md
```

## Directory and class skills
| Directory | file | Description |
| ----------------- | ----------- |
| /{Module}/{Module}.Tests | reqnroll.json | Configures Reqnroll's html formatter output path, which `cucumber-test.sh` copies from |
| /.github/pages | index.html | Static landing page `result-page.sh` copies into `public/`; links to `tests/`, `coverage/`, `mutation/` |
| /scripts | cucumber-test.sh | Runs `dotnet test`, normalizes results into `tmp/result/cucumber-test.json` (+ `coverage-test.json` when `WITH_CODE_COVERAGE=true`), keeps the native report under `tmp/report/tests` (+ `tmp/report/coverage`) |
| /scripts | mutation-test.sh | Runs `dotnet-stryker` (scoped to `DELTA_BASE` when `ONLY_DELTA=true`), normalizes results into `tmp/result/mutation-test.json`, keeps the native report under `tmp/report/mutation` |
| /scripts | result-page.sh | Assembles `public/` from `tmp/result/*.json` + `tmp/report/*` — no test/build tooling involved |
| / | Makefile | Exposes the `cucumber-test`/`mutation-test`/`result-page` targets required by [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#make-command-contract) |

## Makefile
See [templates/Makefile.md](../templates/Makefile.md) for the full content.

## scripts/cucumber-test.sh
Runs the Reqnroll scenarios (and, when `WITH_CODE_COVERAGE=true`, line coverage), then normalizes the result. See [templates/cucumber-test.sh.md](../templates/cucumber-test.sh.md) for the full script and the required `reqnroll.json` formatter config.

## scripts/mutation-test.sh
Runs Stryker.NET — its native `--since` mode covers `ONLY_DELTA`/`DELTA_BASE` directly, so this script does not need to compute the diff itself. See [templates/mutation-test.sh.md](../templates/mutation-test.sh.md) for the full script.

## scripts/result-page.sh
Pure assembly — no `dotnet`/test tooling involved, so this same script (unmodified) also works for the Python and TypeScript variants of this solution. See [templates/result-page.sh.md](../templates/result-page.sh.md) for the full script.

# Rules

## MUST
- `cucumber-test`, `mutation-test`, and `result-page` targets must exist and behave exactly as documented in [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#make-command-contract) — this `Makefile` is the .NET implementation of that contract, not a variation of it.
  - Violation: a CI workflow or a developer runs `dotnet-stryker`/`dotnet test` directly instead of through `make mutation-test`/`make cucumber-test`.
  - Risk: the workflow now needs .NET-specific knowledge, and switching or reconfiguring Stryker.NET later becomes a breaking change for every CI file that calls it directly.
  - Fix: every caller (CI or a developer) goes through the `Makefile`; the CI workflow itself is defined once, stack-agnostically, in [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md).
- `scripts/cucumber-test.sh` and `scripts/mutation-test.sh` must write their normalized JSON into `tmp/result/` and keep the native HTML report under `tmp/report/<kind>/`, per the same contract.
  - Risk: without the normalized JSON, `make result-page` and badge generation have nothing stack-independent to read.
  - Fix: write both outputs exactly as [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#make-command-contract) specifies.
- `{Module}.Tests/reqnroll.json` must configure Reqnroll's html formatter to the exact path `scripts/cucumber-test.sh` copies from — without it, the script's `cp` step fails.
  - Risk: `scripts/cucumber-test.sh`'s `cp` step fails because the formatter never wrote to the path it expects, breaking `make cucumber-test` for every caller.
  - Fix: point `reqnroll.json`'s html formatter output at exactly the path the script copies from.
- `scripts/mutation-test.sh` must still exit with `dotnet-stryker`'s own exit code after writing `tmp/result/mutation-test.json` — normalizing the result must never swallow a real mutation-testing failure.
  - Risk: a real mutation-testing failure gets swallowed by the normalization step, and CI reports success on a run that actually found unkilled mutants.
  - Fix: propagate `dotnet-stryker`'s exit code from the script after it finishes writing the normalized result.
- Never add stack-specific flags to the `make` targets themselves beyond `WITH_CODE_COVERAGE`/`ONLY_DELTA`/`DELTA_BASE` — a caller must not need to know this is a .NET project.
  - Risk: every caller (CI workflow, developer, script) now needs .NET-specific knowledge to invoke the targets correctly, defeating the point of the uniform contract this `Makefile` implements.
  - Fix: keep the `make` interface limited to the toggles [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#make-command-contract) defines; anything .NET-specific stays inside the `Makefile`/scripts.

# Unittest TestCases
- [ ] WHEN `make cucumber-test` runs THEN `tmp/result/cucumber-test.json` and `tmp/report/tests/` exist.
- [ ] WHEN `make cucumber-test WITH_CODE_COVERAGE=true` runs THEN `tmp/result/coverage-test.json` and `tmp/report/coverage/` also exist.
- [ ] WHEN `make mutation-test ONLY_DELTA=true DELTA_BASE=<ref>` runs THEN only mutants in code changed since `<ref>` are evaluated.
- [ ] WHEN `make result-page` runs after both `*-test` targets THEN `public/` contains the badge JSON files and copies of the native reports.
