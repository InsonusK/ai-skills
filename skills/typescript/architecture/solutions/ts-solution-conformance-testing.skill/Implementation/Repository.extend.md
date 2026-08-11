---
description: Add the Makefile and normalization scripts implementing the make cucumber-test/mutation-test/result-page contract
element_kind: repository
change_kind: extend
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
/.github
  /pages
    index.html
/scripts
  cucumber-test.sh
  mutation-test.sh
  result-page.sh
Makefile
package.json
cucumber.mjs
stryker.conf.json
README.md
```

## Directory and class skills
| Directory | file | Description |
| ----------------- | ----------- |
| /.github/pages | index.html | Static landing page `result-page.sh` copies into `public/`; links to `tests/`, `coverage/`, `mutation/` |
| /scripts | cucumber-test.sh | Runs `cucumber-js` (wrapped in `c8` when `WITH_CODE_COVERAGE=true`), normalizes results into `tmp/result/cucumber-test.json` (+ `coverage-test.json`), keeps the native report under `tmp/report/tests` (+ `tmp/report/coverage`) |
| /scripts | mutation-test.sh | Runs `stryker run` against a `stryker.conf.json`-derived config (scoped to `DELTA_BASE` when `ONLY_DELTA=true`), normalizes results into `tmp/result/mutation-test.json`, keeps the native report under `tmp/report/mutation` |
| /scripts | result-page.sh | Assembles `public/` from `tmp/result/*.json` + `tmp/report/*` — no test/build tooling involved |
| / | stryker.conf.json | Base Stryker config; `mutation-test.sh` patches its `reporters`/`thresholds`/reporter file paths per run, never edits it in place |
| / | Makefile | Exposes the `cucumber-test`/`mutation-test`/`result-page` targets required by [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#make-command-contract) |

## Makefile
See [templates/Makefile.md](../templates/Makefile.md) for the full content.

## scripts/cucumber-test.sh
Runs `cucumber-js` (and, when `WITH_CODE_COVERAGE=true`, wraps it with `c8` for coverage), then normalizes the result. See [templates/cucumber-test.sh.md](../templates/cucumber-test.sh.md) for the full script.

## scripts/mutation-test.sh
StrykerJS has no native `--since`/delta flag the way Stryker.NET does, so this script emulates `ONLY_DELTA` itself by limiting `--mutate` to the files `git diff` reports as changed. See [templates/mutation-test.sh.md](../templates/mutation-test.sh.md) for the full script.

## scripts/result-page.sh
Pure assembly — no `npm`/test tooling involved, so this same script (unmodified) also works for the .NET and Python variants of this solution. See [templates/result-page.sh.md](../templates/result-page.sh.md) for the full script.

# Rules

## MUST
- `cucumber-test`, `mutation-test`, and `result-page` targets must exist and behave exactly as documented in [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#make-command-contract) — this `Makefile` is the TypeScript implementation of that contract, not a variation of it.
  - Violation: a CI workflow or a developer runs `stryker run`/`vitest`/`cucumber-js` directly instead of through `make mutation-test`/`make cucumber-test`.
  - Risk: the workflow now needs TypeScript-specific knowledge, and switching or reconfiguring Stryker later becomes a breaking change for every CI file that calls it directly.
  - Fix: every caller (CI or a developer) goes through the `Makefile`; the CI workflow itself is defined once, stack-agnostically, in [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md).
- `scripts/cucumber-test.sh` and `scripts/mutation-test.sh` must write their normalized JSON into `tmp/result/` and keep the native HTML report under `tmp/report/<kind>/`, per the same contract.
  - Risk: without the normalized JSON, `make result-page` and badge generation have nothing stack-independent to read.
  - Fix: write both outputs exactly as [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#make-command-contract) specifies.
- `stryker.conf.json` must exist at the path `scripts/mutation-test.sh` reads (repository root for a single-package repository) — the script patches a copy of it per run, it never creates one from scratch.
  - Risk: without the base config file present, the script has nothing to patch and `make mutation-test` fails outright.
  - Fix: commit a base `stryker.conf.json` at the path the script expects.
- `scripts/mutation-test.sh` must still exit with `stryker run`'s own exit code after writing `tmp/result/mutation-test.json` — normalizing the result must never swallow a real mutation-testing failure.
  - Risk: a real mutation-testing failure gets swallowed by the normalization step, and CI reports success on a run that actually found unkilled mutants.
  - Fix: propagate `stryker run`'s exit code from the script after it finishes writing the normalized result.
- Never add stack-specific flags to the `make` targets themselves beyond `WITH_CODE_COVERAGE`/`ONLY_DELTA`/`DELTA_BASE` — a caller must not need to know this is a TypeScript project.
  - Risk: every caller (CI workflow, developer, script) now needs TypeScript-specific knowledge to invoke the targets correctly, defeating the point of the uniform contract this `Makefile` implements.
  - Fix: keep the `make` interface limited to the toggles [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#make-command-contract) defines; anything TypeScript-specific stays inside the `Makefile`/scripts.

# Unittest TestCases
- [ ] WHEN `make cucumber-test` runs THEN `tmp/result/cucumber-test.json` and `tmp/report/tests/` exist.
- [ ] WHEN `make cucumber-test WITH_CODE_COVERAGE=true` runs THEN `tmp/result/coverage-test.json` and `tmp/report/coverage/` also exist.
- [ ] WHEN `make mutation-test ONLY_DELTA=true DELTA_BASE=<ref>` runs THEN only mutants in code changed since `<ref>` are evaluated.
- [ ] WHEN `make result-page` runs after both `*-test` targets THEN `public/` contains the badge JSON files and copies of the native reports.
