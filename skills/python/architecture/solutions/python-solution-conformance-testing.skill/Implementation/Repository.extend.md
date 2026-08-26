---
description: Add the Makefile and normalization scripts implementing the make unit-test/mutation-test/test-report/test-and-report contract
element_kind: repository
change_kind: extend
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
  mutation-test.sh
  test-report.sh
Makefile
pyproject.toml
README.md
```

## Directory and class skills
| Directory | file | Description |
| ----------------- | ----------- |
| /features | {rule}.feature, steps/{rule}_steps.py | Gherkin scenarios and their bindings |
| /report-template | index.html | Static landing page `test-report.sh` copies into `public/`; links to `tests/`, `coverage/`, `mutation/`. Kept outside `.github/` since this solution never owns `.github/workflows/*` |
| /scripts | unit-test.sh | Runs `behave`/`pytest` under `coverage`, normalizes results into `tmp/result/unit-test.json` (+ `coverage-test.json` when `WITH_CODE_COVERAGE=true`), keeps the native report under `tmp/report/tests` (+ `tmp/report/coverage`) |
| /scripts | mutation-test.sh | Runs `mutmut run` (scoped to `DELTA_BASE` when `ONLY_DELTA=true`), normalizes results into `tmp/result/mutation-test.json`, keeps the native report under `tmp/report/mutation` |
| /scripts | test-report.sh | Assembles `public/` from `tmp/result/*.json` + `tmp/report/*` — no test/build tooling involved |
| / | Makefile | Exposes the `unit-test`/`mutation-test`/`test-report`/`test-and-report` targets required by [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] |

## Makefile
See [templates/Makefile.md](../templates/Makefile.md) for the full content.

## scripts/unit-test.sh
Runs `behave` and the plain `test/` suite under `coverage`, then normalizes the result. The JSON parsing (behave's own `json.pretty` formatter, modeled after Cucumber's JSON schema) and the `coverage`/`jq` calls are solid; the HTML-formatter line is a choice you still have to pin. See [templates/unit-test.sh.md](../templates/unit-test.sh.md) for the full script.

## scripts/mutation-test.sh
`mutmut`'s CLI for CI-friendly result export and for scoping a run to specific changed files has moved between major versions more than Stryker.NET/StrykerJS have — every `mutmut` line in the template is a sketch to verify against the version this project pins, not a copy-paste command. See [templates/mutation-test.sh.md](../templates/mutation-test.sh.md) for the full script and its `VERIFY`/`TODO` markers.

## scripts/test-report.sh
Pure assembly — no `python`/test tooling involved, so this same script (unmodified) also works for the .NET and TypeScript variants of this solution. See [templates/test-report.sh.md](../templates/test-report.sh.md) for the full script.

# Rules

## MUST
- `unit-test`, `mutation-test`, `test-report`, and `test-and-report` targets must exist and behave exactly as documented in [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] — this `Makefile` is the Python implementation of that contract, not a variation of it.
  - Violation: a CI workflow or a developer runs `mutmut`/`behave` directly instead of through `make mutation-test`/`make unit-test`.
  - Risk: the workflow now needs Python-specific knowledge, and switching or reconfiguring `mutmut` later becomes a breaking change for every CI file that calls it directly.
  - Fix: every caller (CI or a developer) goes through the `Makefile`; the CI workflow itself is defined once, stack-agnostically, in [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md).
- `scripts/unit-test.sh` and `scripts/mutation-test.sh` must write their normalized JSON into `tmp/result/` and keep the native HTML report under `tmp/report/<kind>/`, per the same contract.
  - Risk: without the normalized JSON, `make test-report` and badge generation have nothing stack-independent to read.
  - Fix: write both outputs exactly as [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] specifies.
- Before relying on `scripts/mutation-test.sh`, replace its placeholder `KILLED`/`SURVIVED`/`TIMEDOUT`/`NO_COVERAGE` parsing with a real export from the `mutmut` version the project pins, and verify the delta-scoping flag/config key used in `ONLY_DELTA=true` mode — see the `VERIFY` comments inline.
  - Risk: `mutmut`'s CLI has moved between major versions, so unverified placeholder parsing can silently report wrong `KILLED`/`SURVIVED` counts, or crash, once a real run happens.
  - Fix: replace the placeholder parsing with a real export from the pinned `mutmut` version, and confirm the delta-scoping flag/config key before relying on `ONLY_DELTA=true`.
- `scripts/mutation-test.sh` must still exit with `mutmut`'s own exit code after writing `tmp/result/mutation-test.json` — normalizing the result must never swallow a real mutation-testing failure.
  - Risk: a real mutation-testing failure gets swallowed by the normalization step, and CI reports success on a run that actually found unkilled mutants.
  - Fix: propagate `mutmut`'s exit code from the script after it finishes writing the normalized result.
- Pick and pin one behave HTML formatter plugin (or an equivalent conversion of the JSON output) so `tmp/report/tests/index.html` exists — `scripts/unit-test.sh`'s `TODO` must be resolved before this solution is considered applied.
  - Risk: without a resolved HTML formatter, `tmp/report/tests/index.html` never gets created, so `make unit-test`'s native report is missing even though the normalized JSON exists.
  - Fix: pick and pin a behave HTML formatter (or convert the JSON output) and resolve the script's `TODO` before treating the solution as applied.
- `test-and-report` must run `unit-test` (with coverage), `mutation-test`, and `test-report`, in that order.
  - Risk: running them out of order, or omitting one, produces a report built from stale or missing results.
  - Fix: declare `test-and-report`'s prerequisites as `unit-test mutation-test test-report`, forwarding `WITH_CODE_COVERAGE`/`ONLY_DELTA`/`DELTA_BASE` to the targets that accept them.
- Never add stack-specific flags to the `make` targets themselves beyond `WITH_CODE_COVERAGE`/`ONLY_DELTA`/`DELTA_BASE` — a caller must not need to know this is a Python project.
  - Risk: every caller (CI workflow, developer, script) now needs Python-specific knowledge to invoke the targets correctly, defeating the point of the uniform contract this `Makefile` implements.
  - Fix: keep the `make` interface limited to the toggles [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] defines; anything Python-specific stays inside the `Makefile`/scripts.
- Never let `report-template/index.html` live under `.github/`.
  - Risk: nesting a project-owned static asset inside `.github/` implies this solution owns a workflow or Pages configuration it does not — the actual publishing step is a separate, layered CI concern owned by [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md).
  - Fix: keep it at `report-template/index.html`, copied by `test-report.sh` — never generated, never placed under `.github/`.

# Unittest TestCases
- [ ] WHEN `make unit-test` runs THEN `tmp/result/unit-test.json` and `tmp/report/tests/` exist.
- [ ] WHEN `make unit-test WITH_CODE_COVERAGE=true` runs THEN `tmp/result/coverage-test.json` and `tmp/report/coverage/` also exist.
- [ ] WHEN `make mutation-test ONLY_DELTA=true DELTA_BASE=<ref>` runs THEN only mutants in code changed since `<ref>` are evaluated.
- [ ] WHEN `make test-report` runs after both `*-test` targets THEN `public/` contains the badge JSON files and copies of the native reports.
- [ ] WHEN `make test-and-report` runs THEN it produces the same end state as running `unit-test`, `mutation-test`, and `test-report` in sequence by hand.
